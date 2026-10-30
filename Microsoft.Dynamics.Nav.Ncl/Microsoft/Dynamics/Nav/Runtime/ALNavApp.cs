using System;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALNavApp
{
	public static bool ALNavAppGetArchiveRecordRef(int tableId, ByRef<NavRecordRef> recordRef)
	{
		if (NavCurrentThread.Session.NavAppUpgradeSessionManager != null)
		{
			NavRecordRef archiveRecordRef = NavCurrentThread.Session.NavAppUpgradeSessionManager.GetArchiveRecordRef(tableId);
			if (archiveRecordRef != null)
			{
				recordRef.Value = archiveRecordRef;
				return true;
			}
		}
		return false;
	}

	public static string ALNavAppGetArchiveVersion()
	{
		if (NavCurrentThread.Session.NavAppUpgradeSessionManager == null)
		{
			return string.Empty;
		}
		return NavCurrentThread.Session.NavAppUpgradeSessionManager.GetArchiveVersion();
	}

	public static bool ALNavAppRestoreArchiveData(int tableId)
	{
		return ALNavAppRestoreArchiveData(DataError.ThrowError, tableId, runApplicationTrigger: false);
	}

	public static bool ALNavAppRestoreArchiveData(int tableId, bool runApplicationTrigger)
	{
		return ALNavAppRestoreArchiveData(DataError.ThrowError, tableId, runApplicationTrigger);
	}

	public static bool ALNavAppRestoreArchiveData(DataError errorLevel, int tableId)
	{
		return ALNavAppRestoreArchiveData(errorLevel, tableId, runApplicationTrigger: false);
	}

	public static bool ALNavAppRestoreArchiveData(DataError errorLevel, int tableId, bool runApplicationTrigger)
	{
		if (NavCurrentThread.Session.NavAppUpgradeSessionManager != null)
		{
			return NavCurrentThread.Session.NavAppUpgradeSessionManager.RestoreArchiveData(errorLevel, tableId, runApplicationTrigger);
		}
		return false;
	}

	public static void ALNavAppDeleteArchiveData(int tableId)
	{
		if (NavCurrentThread.Session.NavAppUpgradeSessionManager != null)
		{
			NavCurrentThread.Session.NavAppUpgradeSessionManager.DeleteArchiveData(tableId);
		}
	}

	public static void ALNavAppLoadPackageData(int tableId)
	{
		if (NavCurrentThread.Session.NavAppUpgradeSessionManager != null)
		{
			NavCurrentThread.Session.NavAppUpgradeSessionManager.LoadPackageData(tableId);
		}
	}

	public static bool ALNavAppIsInstalling()
	{
		return NavCurrentThread.Session.NavAppUpgradeSessionManager != null;
	}

	public static bool ALGetCurrentModuleInfo(DataError errorLevel, ByRef<NavModuleInfo> info)
	{
		Guid moduleId = ((WrappedValue<Guid>)(object)NavCurrentThread.Session?.CurrentMethodScope?.ApplicationObject?.GetOwningAppId())?.Value ?? Guid.Empty;
		return ALGetModuleInfo(errorLevel, moduleId, info);
	}

	public static bool ALGetModuleInfo(DataError errorLevel, Guid moduleId, ByRef<NavModuleInfo> info)
	{
		if (moduleId == Guid.Empty)
		{
			info.Value = new NavModuleInfo(Guid.Empty, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily, "Microsoft", new Version(NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion), new Version(NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion), new NavList<NavModuleDependencyInfo>());
			return true;
		}
		NavAppRuntimeMetadata val = NavCurrentThread.TryResolveAppGroup()?.OrderedAppMetadata.FirstOrDefault((NavAppRuntimeMetadata a) => ((WrappedValue<Guid>)(object)a.AppId).Value == moduleId);
		if (val == null)
		{
			info.Value = null;
			if (errorLevel == DataError.TrapError)
			{
				return false;
			}
			string message = string.Format(CultureInfo.CurrentCulture, Lang.NavApp_NoInstalledMatchFound, moduleId);
			throw new NavAppException(message);
		}
		NavList<NavModuleDependencyInfo> dependencies = new NavList<NavModuleDependencyInfo>(val.Dependencies.Select((NavAppRuntimeDependencyDefinition d) => new NavModuleDependencyInfo(((WrappedValue<Guid>)(object)d.AppId).Value, d.Name, d.Publisher)).ToArray());
		Version version = GetDataVersionForInstall(val) ?? GetDataVersionForUpgrade(val) ?? val.Version;
		info.Value = new NavModuleInfo(((WrappedValue<Guid>)(object)val.AppId).Value, val.Name, val.Publisher, val.Version, version, dependencies);
		return true;
	}

	private static Version GetDataVersionForInstall(NavAppRuntimeMetadata app)
	{
		Version result = null;
		NavAppInstallationContext navAppInstallationContext = NavCurrentThread.Session?.AppInstallationContext;
		if ((WrappedComparable<Guid>)(object)((navAppInstallationContext != null) ? navAppInstallationContext.AppBeingTargeted.AppId : null) == (WrappedComparable<Guid>)(object)app.AppId && navAppInstallationContext.IsInstalling)
		{
			result = (navAppInstallationContext.HasData ? app.Version : new Version(0, 0, 0, 0));
		}
		return result;
	}

	private static Version GetDataVersionForUpgrade(NavAppRuntimeMetadata app)
	{
		Version result = null;
		NavAppUpgradeContext navAppUpgradeContext = null;
		NavDataUpgradeManager navDataUpgradeManager = NavCurrentThread.Session?.Database?.UpgradeManager;
		NavSession session = NavCurrentThread.Session;
		object obj;
		if (session == null)
		{
			obj = null;
		}
		else
		{
			NavAppUpgradeContext appUpgradeContext = session.AppUpgradeContext;
			if (appUpgradeContext == null)
			{
				obj = null;
			}
			else
			{
				NavAppRuntimeMetadata appBeingTargeted = appUpgradeContext.AppBeingTargeted;
				obj = ((appBeingTargeted != null) ? appBeingTargeted.AppId : null);
			}
		}
		if ((WrappedComparable<Guid>)obj == (WrappedComparable<Guid>)(object)app.AppId)
		{
			navAppUpgradeContext = NavCurrentThread.Session?.AppUpgradeContext;
		}
		else if (navDataUpgradeManager != null && navDataUpgradeManager.GetUpgradeInformation().State == NavWorkflowState.InProgress)
		{
			navAppUpgradeContext = navDataUpgradeManager.ExtensionUpgradeContexts.FirstOrDefault((NavAppJointUpgradeContext c) =>
			{
				if (c.ContextType == JointUpgradeContextType.Upgrade)
				{
					NavAppRuntimeMetadata appBeingTargeted4 = c.AppBeingTargeted;
					return (WrappedComparable<Guid>)(object)((appBeingTargeted4 != null) ? appBeingTargeted4.AppId : null) == (WrappedComparable<Guid>)(object)app.AppId;
				}
				return false;
			})?.Context as NavAppUpgradeContext;
		}
		if (navAppUpgradeContext != null)
		{
			NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
			DiagnosticParameter firstParameter = DiagnosticParameter.SystemMetadata(app.AppId);
			DiagnosticParameter[] array = new DiagnosticParameter[3]
			{
				DiagnosticParameter.SystemMetadata(navAppUpgradeContext.TransactionalPhaseCompleted),
				null,
				null
			};
			INavAppReference oldAppBeingUpgradedFrom = navAppUpgradeContext.OldAppBeingUpgradedFrom;
			array[1] = DiagnosticParameter.SystemMetadata((oldAppBeingUpgradedFrom != null) ? oldAppBeingUpgradedFrom.Version : null);
			NavAppRuntimeMetadata appBeingTargeted2 = navAppUpgradeContext.AppBeingTargeted;
			array[2] = DiagnosticParameter.SystemMetadata((appBeingTargeted2 != null) ? appBeingTargeted2.Version : null);
			mostSpecificInstance.SendTraceTag("0000A1J", Category.Runtime, Verbosity.Verbose, "Upgrading App ID {0} where TransactionalPhaseCompeleted: {1}, OldAppBeingUpgradedFrom version: {2} and AppBeingTargeted version: {3}", firstParameter, array);
			object obj2;
			if (!navAppUpgradeContext.TransactionalPhaseCompleted)
			{
				INavAppReference oldAppBeingUpgradedFrom2 = navAppUpgradeContext.OldAppBeingUpgradedFrom;
				obj2 = ((oldAppBeingUpgradedFrom2 != null) ? oldAppBeingUpgradedFrom2.Version : null);
			}
			else
			{
				NavAppRuntimeMetadata appBeingTargeted3 = navAppUpgradeContext.AppBeingTargeted;
				obj2 = ((appBeingTargeted3 != null) ? appBeingTargeted3.Version : null);
			}
			result = (Version)obj2;
		}
		return result;
	}
}
