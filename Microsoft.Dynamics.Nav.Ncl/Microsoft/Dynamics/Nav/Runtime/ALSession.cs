using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class ALSession
{
	private static readonly Regex ApplicationAreasRegex = new Regex("^#([A-Z0-9]+)(?:(\\s)*,(\\s)*#([A-Z0-9]+))*$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

	public static ExecutionMode ALCurrentExecutionMode => NavCurrentThread.Session.ExecutionMode;

	public static bool ALVerboseTraceLevelEnabled => NavCurrentThread.Session.Diagnostics.VerboseTraceLevelEnabled;

	public static bool ALFullALFunctionTracingEnabled => NavCurrentThread.Session.Diagnostics.FullALFunctionTracingEnabled;

	public static NavClientType ALCurrentClientType
	{
		get
		{
			switch (NavCurrentThread.Session.ClientConnectionType)
			{
			case ConnectionType.Background:
				return NavClientType.Background;
			case ConnectionType.ManagementClient:
				return NavClientType.Management;
			case ConnectionType.Nas:
				return NavClientType.NAS;
			case ConnectionType.WebServiceClient:
				return NavCurrentThread.Session.WebConnectionType switch
				{
					WebConnectionType.OData => NavClientType.OData, 
					WebConnectionType.ODataV4 => NavClientType.ODataV4, 
					WebConnectionType.Api => NavClientType.Api, 
					_ => NavClientType.SOAP, 
				};
			case ConnectionType.RoleTailoredClient:
				return NavClientType.Windows;
			case ConnectionType.ClientService:
			case ConnectionType.WebClient:
				return NavClientType.Web;
			case ConnectionType.Tablet:
				return NavClientType.Tablet;
			case ConnectionType.Phone:
				return NavClientType.Phone;
			case ConnectionType.Desktop:
				return NavClientType.Desktop;
			default:
				return ServerUserSettings.Instance.DefaultClient.Value;
			}
		}
	}

	public static NavClientType ALDefaultClientType
	{
		get
		{
			NavClientType value = ServerUserSettings.Instance.DefaultClient.Value;
			if ((uint)(value - 2) <= 3u || (uint)(value - 9) <= 4u)
			{
				return ServerUserSettings.Instance.DefaultClient.Value;
			}
			throw new NavNCLGetUrlException(string.Format(CultureInfo.CurrentCulture, Lang.GetUrlUnknownClient, ProductNames.ProductNameServer));
		}
	}

	public static Microsoft.Dynamics.Nav.Types.ExecutionContext ALExecutionContext => NavCurrentThread.Session.ExecutionContext;

	public static bool ALStartSession(DataError errorLevel, ByRef<int> sessionId, int objectId)
	{
		return ALStartSession(errorLevel, sessionId, objectId, NavCurrentThread.Session.Company.CompanyName, null);
	}

	public static bool ALStartSession(DataError errorLevel, ByRef<int> sessionId, int objectId, string companyName)
	{
		return ALStartSession(errorLevel, sessionId, objectId, companyName, null);
	}

	public static void ALSendTraceTag(string tag, string category, ALVerbosity verbosity, string message)
	{
		ALSendTraceTag(tag, category, verbosity, message, DataClassification.CustomerContent);
	}

	public static void ALSendTraceTag(string tag, string category, ALVerbosity verbosity, string message, DataClassification dataClassification)
	{
		NavDiagnostics diagnostics = NavCurrentThread.Session.Diagnostics;
		diagnostics.SendTraceTag(tag, category, (Verbosity)verbosity, dataClassification, message);
	}

	public static void ALEnableVerboseTelemetry(bool enableFullALFunctionTracing, NavDuration duration)
	{
		NavDiagnostics diagnostics = NavCurrentThread.Session.Diagnostics;
		double totalMilliseconds = ServerUserSettings.Instance.MaximumTraceLevelOverrideDuration.Value.TotalMilliseconds;
		diagnostics.OverrideTraceLevel(Verbosity.Verbose, TimeSpan.FromMilliseconds(Math.Min(duration.Value, totalMilliseconds)));
		if (enableFullALFunctionTracing)
		{
			diagnostics.EnableFullALTracing(TimeSpan.FromMilliseconds(Math.Min(duration.Value, totalMilliseconds)));
		}
	}

	public static bool ALBindSubscription(DataError errorLevel, object codeunit)
	{
		try
		{
			if (!(codeunit is NavCodeunit navCodeunit))
			{
				throw new NavNCLEventBindingException(Lang.IllegalBindSubscriptionArgument);
			}
			navCodeunit.BindSubscription();
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		return true;
	}

	public static bool ALUnbindSubscription(DataError errorLevel, object codeunit)
	{
		try
		{
			if (!(codeunit is NavCodeunit navCodeunit))
			{
				throw new NavNCLEventBindingException(Lang.IllegalUnbindSubscriptionArgument);
			}
			navCodeunit.UnBindSubscription();
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		return true;
	}

	public static string ALApplicationArea(string applicationArea = null)
	{
		string applicationAreas = NavCurrentThread.Session.ApplicationAreas;
		if (applicationArea != null)
		{
			if (applicationArea.Length > 0 && !ApplicationAreasRegex.IsMatch(applicationArea))
			{
				throw new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidApplicationAreaValue, applicationArea));
			}
			NavCurrentThread.Session.ApplicationAreas = applicationArea;
		}
		return applicationAreas;
	}

	public static string ALApplicationIdentifier()
	{
		return NavCurrentThread.Session.AppId;
	}

	public static bool ALStopSession(DataError errorLevel, int sessionID)
	{
		return ALStopSession(errorLevel, sessionID, null);
	}

	public static bool ALStopSession(DataError errorLevel, int sessionID, string comment)
	{
		DiagnosticsResolver.GetMostSpecificInstance(NavCurrentThread.Session).SendTraceTag("00000UH", Category.AL, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "StopSession ID = {0}", sessionID);
		if (NavCurrentThread.Session.CallbackAllowed && NavCurrentThread.Session.Id == sessionID)
		{
			DiagnosticsResolver.GetMostSpecificInstance(NavCurrentThread.Session).SendTraceTag("000019L", Category.AL, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Attempt at stopping ones own session from client session with sessionId = {0}, with comment = {1}", sessionID, comment);
			return false;
		}
		if (sessionID < 0)
		{
			try
			{
				return SessionEventTableHandler.RemoveSustainedWebServiceSession(sessionID);
			}
			catch (NavBaseException exception)
			{
				DiagnosticsResolver.GetMostSpecificInstance(NavCurrentThread.Session).SendExceptionTag("00000UO", Category.AL, exception, Verbosity.Normal);
				if (errorLevel != DataError.TrapError)
				{
					throw;
				}
				return false;
			}
		}
		NavSession navSession = NavCurrentThread.Session.Tenant.Tree.Children.OfType<NavSession>().FirstOrDefault((NavSession c) => c.Id == sessionID);
		if (navSession == null)
		{
			return false;
		}
		navSession.SessionComment = comment;
		DiagnosticsResolver.GetMostSpecificInstance(NavCurrentThread.Session).SendTraceTag("00000UI", Category.AL, Verbosity.Verbose, PrivacyClassification.SystemMetadata, TraceStringHelper.CreateALFunctionStatementTrace(NavCurrentThread.Session, "StopSession (Session ID: " + sessionID + ")"));
		Task task = navSession.DisposeAsync(ShutdownScope.SingleSession, !navSession.AdminSession, !navSession.AdminSession, alwaysAsync: false, externalStopRequest: true);
		if (!task.IsCompleted)
		{
			task.LogOrFailOnException();
			return true;
		}
		if (task.Status == TaskStatus.RanToCompletion)
		{
			return true;
		}
		try
		{
			task.WaitThrowingOriginalException();
			return false;
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
	}

	public static bool ALIsSessionActive(int sessionId)
	{
		return NavCurrentThread.Session.Tenant.GetSessionById(sessionId) != null;
	}

	public static bool ALStartSession(DataError errorLevel, ByRef<int> sessionId, int objectId, string companyName, NavRecord record)
	{
		NavSession session = null;
		NavSession session2 = NavCurrentThread.Session;
		NavRecord transferRecord = null;
		DiagnosticsResolver.GetMostSpecificInstance(session2).SendTraceTag("00000UJ", Category.AL, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "StartSession invoked with ObjectId = {0}", objectId);
		if (session2.TestExecution != null && (!session2.TestExecution.CommitTestCodeunits || !session2.TestExecution.CommitTestFunctions))
		{
			throw new NavTestStartSessionNotAllowedException();
		}
		try
		{
			session = new NavSession(session2.Tenant, session2.ClientTimeZone, session2.Authenticator.Clone(), session2.AppId);
			session.ClientConnectionType = ConnectionType.Background;
			session.ClientCallbackOverride = new HeadlessClientCallback(ProductNames.ServerServiceDisplayName, session);
			session.SessionComment = string.Format(CultureInfo.CurrentCulture, Lang.StartSessionParentComment, session2.Id);
			session.AccessLock.BeginExclusiveExecution();
			session.Open();
			DiagnosticsResolver.GetMostSpecificInstance(session2).SendTraceTag("00000UK", Category.AL, Verbosity.Verbose, PrivacyClassification.SystemMetadata, TraceStringHelper.CreateALFunctionStatementTrace(session2, "StartSession (Session ID: " + session.Id + ")"));
			sessionId.ObjectValue = session.Id;
			session.RegionalSettings = session2.RegionalSettings;
			session.WorkDate = session2.WorkDate;
			session.OpenCompany(companyName);
			if (record != null)
			{
				transferRecord = record.CloneRecord(session);
			}
			session.SessionComment = string.Empty;
			session.AccessLock.EndExclusiveExecution();
			Task task = NavTaskFactory.RunTask(() =>
			{
				RunCodeunitAndDisposeSession(session, objectId, transferRecord);
			});
			task.LogOrFailOnException();
			return true;
		}
		catch (Exception ex)
		{
			if (session != null && !session.IsDisposed)
			{
				session.Dispose();
			}
			if (DataError.ThrowError == errorLevel || !(ex is NavBaseException))
			{
				throw;
			}
			return false;
		}
		finally
		{
			NavCurrentThread.Session = session2;
		}
	}

	public static Microsoft.Dynamics.Nav.Types.ExecutionContext ALGetModuleExecutionContext(Guid appId)
	{
		return NavCurrentThread.Session.GetModuleExecutionContext(appId);
	}

	public static Microsoft.Dynamics.Nav.Types.ExecutionContext ALGetModuleExecutionContext()
	{
		return NavCurrentThread.Session.GetCurrentModuleExecutionContext();
	}

	private static void RunCodeunitAndDisposeSession(NavSession session, int codeUnitId, NavRecord record)
	{
		using (NavThread.AddCurrentThreadToScheduler(NavEnvironment.Instance.ThreadScheduler))
		{
			try
			{
				session.AccessLock.BeginExclusiveExecution();
				NavCurrentThread.Session = session;
				Thread.CurrentThread.CurrentCulture = session.WindowsCulture;
				Thread.CurrentThread.CurrentUICulture = session.Culture;
				NavCodeunit.InvokeRun(codeUnitId, (record == null) ? null : new object[1] { record });
				if (session.IsTransactionActive())
				{
					session.EndTransaction(commit: true);
				}
			}
			catch (NavSessionTerminatedException exception)
			{
				LogWriterHelper.LogExceptionEvent(exception);
			}
			catch (NavBaseException ex)
			{
				session.SessionComment = ex.Message;
				LogWriterHelper.LogExceptionEvent(ex, EventLogEntryType.Error);
			}
			finally
			{
				NavCurrentThread.Session = null;
				session.Dispose();
			}
		}
	}
}
