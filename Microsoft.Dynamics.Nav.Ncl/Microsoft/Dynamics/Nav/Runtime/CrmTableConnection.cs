using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Data;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Xrm;
using Microsoft.Dynamics.Nav.Xrm.Proxy;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CrmTableConnection : TableConnection
{
	private class CrmTestDataProvider : TempTableDataProvider
	{
		private readonly int primaryKeyIndex;

		internal CrmTestDataProvider(NavSession session, NCLMetaTable table)
			: base(session, table)
		{
			primaryKeyIndex = table.Keys[0].GetKeyFieldByIndex(0).FieldIndex;
		}

		public override InsertResult Insert(int companyToken, MutableRecordBuffer recordBuffer, bool allowBuffering, out ReadOnlyRecordBuffer outputRecordBuffer)
		{
			bool flag = recordBuffer[primaryKeyIndex].ToGuid() == Guid.Empty;
			if (flag)
			{
				recordBuffer[primaryKeyIndex] = new NavGuid(Guid.NewGuid());
			}
			InsertResult insertResult = base.Insert(companyToken, recordBuffer, allowBuffering, out outputRecordBuffer);
			if ((insertResult != InsertResult.Success) & flag)
			{
				recordBuffer[primaryKeyIndex] = NavGuid.Default;
			}
			return insertResult;
		}
	}

	private SecureString connectionString;

	private IXrmConnection xrmConnection;

	private readonly NavSession session;

	private bool needsCredentials;

	private readonly int defaultProxyVersion;

	private const string UserToken = "{USER}";

	private const string PasswordToken = "{PASSWORD}";

	private const string TestConnectionString = "@@test@@";

	private Dictionary<int, DataProvider> testDataProviders;

	internal override TableConnectionType ConnectionType => TableConnectionType.CRM;

	internal override string ConnectionTypeName => Lang.MicrosoftDynamicsCRM;

	internal override bool SupportsTransactions => false;

	internal bool IsTestConnection { get; private set; }

	internal SourceLevels XrmTraceLevel { get; set; }

	private CrmTableConnection(ITreeObject parent, NavSession session, string name, string connectionString)
		: base(parent, name)
	{
		XrmTraceLevel = ServerUserSettings.Instance.ExternalTraceLevel.Value;
		defaultProxyVersion = ExternalDataServiceManager.GetDefaultVersion("Xrm");
		this.connectionString = SecureStringExtension.CreateFromUnsecureString(connectionString);
		IsTestConnection = NavCurrentThread.Session.TestExecution.InTest && (connectionString == "@@test@@" || connectionString.IndexOf("Url=@@test@@", StringComparison.OrdinalIgnoreCase) > -1);
		needsCredentials = connectionString.Contains("{PASSWORD}");
		this.session = session;
	}

	internal static CrmTableConnection Create(ITreeObject parent, NavSession session, string name, string connectionString)
	{
		return new CrmTableConnection(parent, session, name, connectionString);
	}

	internal DataAccess CreateDataAccess(NCLMetaTable table, GlobalFilters globalFilters)
	{
		CheckDisposed();
		return new DataAccess(session, (!IsTestConnection) ? new CrmDataProvider(session, table) : CreateCrmTestDataProvider(table), globalFilters, DataAccessTableVersionTokens.CreateForTempTable(), VirtualAndTempTransactionalDataCache.Instance);
	}

	internal IXrmConnection GetXrmConnection()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected Obj, but got Unknown
		CheckDisposed();
		if (xrmConnection != null)
		{
			if (xrmConnection.IsReady)
			{
				return xrmConnection;
			}
			((IDisposable)xrmConnection).Dispose();
			xrmConnection = null;
		}
		try
		{
			CrmConnectionStringBuilder val = new CrmConnectionStringBuilder(connectionString.ConvertToUnsecureString());
			SecureString secureString = null;
			if (needsCredentials)
			{
				if (!RequestCredentials(out var credentials))
				{
					throw new NavCrmException(Lang.CrmNoCredentials);
				}
				val.Password = credentials.Password;
				val.UserName = credentials.UserName;
				if (((DbConnectionStringBuilder)(object)val).ContainsKey("CallerId"))
				{
					bool supportCallerId = val.SupportCallerId;
					val.SupportCallerId = true;
					secureString = SecureStringExtension.CreateFromUnsecureString(((object)val).ToString());
					val.SupportCallerId = supportCallerId;
				}
				else
				{
					secureString = SecureStringExtension.CreateFromUnsecureString(((object)val).ToString());
				}
			}
			session.Diagnostics.SendTraceTag("00000LH", Category.Crm, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Connecting to Dynamics CRM in session {0} with connection string: {1}", DiagnosticParameter.SystemMetadata(session.Id), DiagnosticParameter.Oii(val.DiagnosticLogString()));
			IXrmConnection val2 = null;
			int num = 0;
			while (val2 == null)
			{
				try
				{
					lock (this)
					{
						val2 = GetCrmServiceClientInstance(val);
					}
				}
				catch (ObjectDisposedException exception)
				{
					session.Diagnostics.SendExceptionTag("00000NF", Category.Crm, exception, Verbosity.Warning);
					if (num++ == 3)
					{
						throw;
					}
				}
			}
			if (val2.IsReady)
			{
				if (needsCredentials)
				{
					connectionString = secureString;
					needsCredentials = false;
				}
				session.Diagnostics.SendTraceTag("00000NG", Category.Crm, Verbosity.Normal, PrivacyClassification.SystemMetadata, "New CRM organizationService, sessionId:{0}, retries:{1}, version:{2}", session.Id, num, val2.ConnectedOrgVersion);
				return xrmConnection = val2;
			}
			XrmException lastCrmException = val2.LastCrmException;
			string lastCrmError = val2.LastCrmError;
			EmitTracesToDiagnostics(val2);
			((IDisposable)val2).Dispose();
			if (lastCrmException != null)
			{
				throw new NavCrmException(string.Format(CultureInfo.CurrentCulture, Lang.CrmConnectionFailure, ((Exception)(object)lastCrmException).Message), (Exception)(object)lastCrmException);
			}
			throw NavCrmException.Create(string.Format(CultureInfo.CurrentCulture, Lang.CrmConnectionFailure, string.IsNullOrEmpty(lastCrmError) ? "Unknown error." : lastCrmError));
		}
		catch (ConfigurationErrorsException ex)
		{
			throw new NavCrmException(string.Format(CultureInfo.CurrentCulture, Lang.CrmConnectionFailure, ex.Message));
		}
		catch (UriFormatException ex2)
		{
			throw new NavCrmException(string.Format(CultureInfo.CurrentCulture, Lang.CrmConnectionFailure, ex2.Message));
		}
	}

	private IXrmConnection GetCrmServiceClientInstance(CrmConnectionStringBuilder crmConnectionStringBuilder)
	{
		IXrmConnection val;
		try
		{
			crmConnectionStringBuilder.RequireNewInstance = true;
			val = XrmServiceProvider.GetService(GetProxyVersion(crmConnectionStringBuilder)).OpenConnection(XrmTraceLevel, ((object)crmConnectionStringBuilder).ToString());
			if (crmConnectionStringBuilder.CallerId != Guid.Empty)
			{
				val.CallerId = crmConnectionStringBuilder.CallerId;
			}
		}
		catch (ArgumentNullException ex)
		{
			throw session.Diagnostics.SendExceptionTag("00000TK", Category.Crm, NavCrmException.Create(string.Format(CultureInfo.CurrentCulture, Lang.CrmConnectionFailure, ex.Message), ex));
		}
		if (!val.IsReady)
		{
			string lastCrmError = val.LastCrmError;
			EmitTracesToDiagnostics(val);
			((IDisposable)val).Dispose();
			throw session.Diagnostics.SendExceptionTag("00000UY", Category.Crm, NavCrmException.Create(string.Format(CultureInfo.CurrentCulture, Lang.CrmConnectionFailure, lastCrmError)));
		}
		string url = crmConnectionStringBuilder.Url;
		if (!url.ToUpperInvariant().Contains(".DYNAMICS.COM"))
		{
			session.Diagnostics.SendTraceTag("00000UZ", Category.Crm, Verbosity.Verbose, "Connect OnPrem url {0}, connected Org Uri={1}, connection string={2}", DiagnosticParameter.Oii(url), DiagnosticParameter.Oii(val.CrmConnectOrgUriActual), DiagnosticParameter.Oii(crmConnectionStringBuilder.DiagnosticLogString()));
			return val;
		}
		return ValidateCrmOnlineConnection(crmConnectionStringBuilder, val);
	}

	internal void EmitTracesToDiagnostics(IXrmConnection connection)
	{
		IList<string> list = ((connection != null) ? connection.TraceList : null);
		if (list != null && list.Count > 0)
		{
			list.ForEach((string trace) =>
			{
				session.Diagnostics.SendTraceTag("00001Q6", Category.Crm, Verbosity.Normal, "Xrm Trace: {0}", DiagnosticParameter.SystemMetadata(trace));
			});
		}
	}

	private IXrmConnection ValidateCrmOnlineConnection(CrmConnectionStringBuilder crmConnectionStringBuilder, IXrmConnection connection)
	{
		IXrmClientCredentials clientCredentials = connection.ClientCredentials;
		string userName = crmConnectionStringBuilder.UserName;
		string password = crmConnectionStringBuilder.Password;
		string text = ((clientCredentials == null || string.IsNullOrEmpty(clientCredentials.UserName)) ? string.Empty : clientCredentials.UserName);
		string text2 = ((clientCredentials == null || string.IsNullOrEmpty(clientCredentials.Password)) ? string.Empty : clientCredentials.Password);
		if (clientCredentials == null || !text.Equals(userName, StringComparison.CurrentCultureIgnoreCase) || !text2.Equals(password))
		{
			session.Diagnostics.SendTraceTag("00000RI", Category.Crm, Verbosity.Normal, "Force new CRM connection instance, sessionId:{0}, orgId:{1}, connection string:{2}", DiagnosticParameter.SystemMetadata(session.Id), DiagnosticParameter.Oii(connection.ConnectedOrgUniqueName), DiagnosticParameter.Oii(crmConnectionStringBuilder.DiagnosticLogString()));
			if (crmConnectionStringBuilder.RequireNewInstance)
			{
				((IDisposable)connection).Dispose();
				throw session.Diagnostics.SendExceptionTag("00000RJ", Category.Crm, NavCrmException.Create(string.Format(CultureInfo.CurrentCulture, Lang.CrmConnectionFailure, Lang.AuthenticationFailed)));
			}
			try
			{
				((IDisposable)connection).Dispose();
				crmConnectionStringBuilder.RequireNewInstance = true;
				connection = XrmServiceProvider.GetService(GetProxyVersion(crmConnectionStringBuilder)).OpenConnection(XrmTraceLevel, ((object)crmConnectionStringBuilder).ToString());
				if (crmConnectionStringBuilder.CallerId != Guid.Empty)
				{
					connection.CallerId = crmConnectionStringBuilder.CallerId;
				}
			}
			catch (ArgumentNullException ex)
			{
				throw session.Diagnostics.SendExceptionTag("00000TL", Category.Crm, NavCrmException.Create(string.Format(CultureInfo.CurrentCulture, Lang.CrmConnectionFailure, ex.Message)));
			}
			clientCredentials = null;
			if (connection.IsReady)
			{
				clientCredentials = connection.ClientCredentials;
			}
			text = ((clientCredentials == null || string.IsNullOrEmpty(clientCredentials.UserName)) ? string.Empty : clientCredentials.UserName);
			text2 = ((clientCredentials == null || string.IsNullOrEmpty(clientCredentials.Password)) ? string.Empty : clientCredentials.Password);
			if (clientCredentials == null || !text.Equals(userName, StringComparison.CurrentCultureIgnoreCase) || !text2.Equals(password))
			{
				((IDisposable)connection).Dispose();
				throw session.Diagnostics.SendExceptionTag("00000RK", Category.Crm, NavCrmException.Create(string.Format(CultureInfo.CurrentCulture, Lang.CrmConnectionFailure, Lang.AuthenticationFailed)));
			}
		}
		return connection;
	}

	internal void FaultOrganizationService()
	{
		IXrmConnection val = xrmConnection;
		Exception ex = (Exception)(object)((val != null) ? val.LastCrmException : null);
		IXrmConnection val2 = xrmConnection;
		string text = ((val2 != null) ? val2.LastCrmError : null) ?? string.Empty;
		if (ex != null)
		{
			session.Diagnostics.SendExceptionTag("00000LV", Category.Crm, ex, Verbosity.Verbose, DiagnosticParameter.CustomerContent("Faulting CRM Organization service, error = " + text));
		}
		else
		{
			session.Diagnostics.SendTraceTag("00001W3", Category.Crm, Verbosity.Verbose, "Faulting CRM Organization service without crm exception, error = '{0}'", DiagnosticParameter.CustomerContent(text));
		}
		EmitTracesToDiagnostics(xrmConnection);
		((IDisposable)xrmConnection)?.Dispose();
		xrmConnection = null;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && !IsDisposed)
		{
			((IDisposable)xrmConnection)?.Dispose();
			xrmConnection = null;
		}
		base.Dispose(disposing);
	}

	private DataProvider CreateCrmTestDataProvider(NCLMetaTable table)
	{
		if (testDataProviders == null)
		{
			testDataProviders = new Dictionary<int, DataProvider>();
		}
		if (!testDataProviders.TryGetValue(table.TableId, out var value))
		{
			value = new CrmTestDataProvider(session, table);
			testDataProviders.Add(table.TableId, value);
		}
		return value;
	}

	private int GetProxyVersion(CrmConnectionStringBuilder crmConnectionStringBuilder)
	{
		if (crmConnectionStringBuilder.ProxyVersion == 0)
		{
			return defaultProxyVersion;
		}
		return crmConnectionStringBuilder.ProxyVersion;
	}
}
