using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavUpgradeCodeunit : NavCodeunit
{
	public override CodeunitSubType SubType => CodeunitSubType.Upgrade;

	public virtual IEnumerable<string> TableSyncSetupMethods => GetMethodsWithCustomAttribute(typeof(NavTableSyncSetupAttribute));

	public virtual IEnumerable<string> PreconditionCheckMethods => GetMethodsWithCustomAttribute(typeof(NavCheckPreconditionAttribute));

	public virtual IEnumerable<string> UpgradePerCompanyMethods => GetMethodsWithCustomAttribute(typeof(NavUpgradeAttribute));

	public virtual IEnumerable<string> UpgradePerDatabaseMethods => GetMethodsWithCustomAttribute(typeof(NavUpgradePerDatabaseAttribute));

	protected NavUpgradeCodeunit(ITreeObject parent, int objectId)
		: this(parent, objectId, 0)
	{
	}

	protected NavUpgradeCodeunit(ITreeObject parent, int objectId, int tableId)
		: base(parent, objectId, tableId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000I3", Category.DataUpgrade, Verbosity.Normal, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "Creating upgrade codeunit for object ID: {0} table ID {1}", objectId, tableId));
		if (tableId != 0)
		{
			NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(tableId);
			throw NavNCLUpgradeCodeunitMethodArgumentException.CreateNotSupportedRecord(Session.RuntimeLanguage, ObjectId.ObjectNumber, Session.GetObjectName(ObjectId), tableId, metaTableById.TableName);
		}
	}

	protected internal override bool DoRun(DataError errorLevel, NavRecord record)
	{
		throw new NavUpgradeCodeunitOnRunCallException(string.Format(CultureInfo.CurrentCulture, Lang.CannotCallOnRunInUpgradeCodeunit, ObjectId.ObjectNumber));
	}

	protected virtual void OpenCompany(NavSession navSession, string companyName)
	{
		navSession.OpenCompany(companyName, updateUserPersonalization: false);
	}

	internal virtual object InvokeCodeunitMethod(string methodName, string companyName, object[] args)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Invoking upgrade codeunit {0} method '{1}' for tenant {2} company {3} in session {4}", ObjectId.ObjectNumber, methodName, Session.Tenant.Id, companyName, Session.Id);
		using (DiagnosticsResolver.CreateTraceScope("00000UL", DiagnosticParameter.Oii(content), (MonitorType)786435, Category.DataUpgrade))
		{
			try
			{
				using (NavMetadataAccessContext.GetAccessToRemoved(Session, Guid.NewGuid().ToString("B")))
				{
					if (!string.IsNullOrEmpty(companyName))
					{
						OpenCompany(Session, companyName);
					}
					object result = NavCodeunit.Invoke(Session, ObjectId.ObjectNumber, methodName, args);
					using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(Session.Tenant.Database))
					{
						navSqlConnectionScope.Connection.CachedCommands.CloseAllReadersOnTransactionEnd();
					}
					return result;
				}
			}
			catch (Exception ex)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000I6", Category.DataUpgrade, ex);
				string message = ((!string.IsNullOrEmpty(companyName)) ? string.Format(CultureInfo.CurrentCulture, Lang.UpgradeCodeunitExecutionError, methodName, ObjectId.ObjectNumber, companyName, ex.Message) : string.Format(CultureInfo.CurrentCulture, Lang.UpgradeCodeunitExecutionWithoutCompanySpecifiedError, methodName, ObjectId.ObjectNumber, ex.Message));
				throw new NavUpgradeCodeunitExecutionException(message, ex);
			}
		}
	}

	internal virtual void RunTableSyncSetup()
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Running table sync setup for codeunit {0}", ObjectId.ObjectNumber);
		using (DiagnosticsResolver.CreateTimingScope("00000I7", Category.DataUpgrade, DiagnosticParameter.SystemMetadata(content)))
		{
			foreach (string tableSyncSetupMethod in TableSyncSetupMethods)
			{
				using NavRecord navRecord = new NavRecord(Session, 2000000135);
				object[] args = new object[1] { navRecord };
				InvokeCodeunitMethod(tableSyncSetupMethod, null, args);
			}
		}
	}

	protected virtual IEnumerable<string> GetMethodsWithCustomAttribute(Type attributeType)
	{
		MethodInfo[] methods = GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		IEnumerable<MethodInfo> source = methods.Where((MethodInfo next) => next.GetCustomAttributes(attributeType, inherit: false).Any());
		return source.Select((MethodInfo next) => next.Name);
	}
}
