using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

[EventSource(Name = "Microsoft-DynamicsNAV-Server")]
public sealed class NavServerEventSource : EventSource
{
	private class NavServerTracingEventsImplementation : INavServerTracingEvents
	{
		private readonly NavServerEventSource eventSource;

		public NavServerTracingEventsImplementation(NavServerEventSource eventSource)
		{
			this.eventSource = eventSource;
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlExecuteScalarStartEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 1, tenantId, sessionId, userName, sqlStatement, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlExecuteScalarStopEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 2, tenantId, sessionId, userName, sqlStatement, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlExecuteNonQueryStartEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 3, tenantId, sessionId, userName, sqlStatement, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlExecuteNonQueryStopEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 4, tenantId, sessionId, userName, sqlStatement, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlExecuteReaderStartEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 5, tenantId, sessionId, userName, sqlStatement, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlExecuteReaderStopEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 6, tenantId, sessionId, userName, sqlStatement, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlReadNextResultStartEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 7, tenantId, sessionId, userName, sqlStatement, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlReadNextResultStopEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 8, tenantId, sessionId, userName, sqlStatement, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlReadNextRowStartEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 9, tenantId, sessionId, userName, sqlStatement, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlReadNextRowStopEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 10, tenantId, sessionId, userName, sqlStatement, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlBeginTransactionStartEvent(string tenantId, int sessionId, string userName, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 11, tenantId, sessionId, userName, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlBeginTransactionStopEvent(string tenantId, int sessionId, string userName, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 12, tenantId, sessionId, userName, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlPrepareStartEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 13, tenantId, sessionId, userName, sqlStatement, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlPrepareStopEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 14, tenantId, sessionId, userName, sqlStatement, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlOpenConnectionStartEvent(string tenantId, int sessionId, string userName, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 15, tenantId, sessionId, userName, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlOpenConnectionStopEvent(string tenantId, int sessionId, string userName, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 16, tenantId, sessionId, userName, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlCommitStartEvent(string tenantId, int sessionId, string userName, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 17, tenantId, sessionId, userName, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlCommitStopEvent(string tenantId, int sessionId, string userName, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 18, tenantId, sessionId, userName, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlRollbackStartEvent(string tenantId, int sessionId, string userName, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 19, tenantId, sessionId, userName, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSqlRollbackStopEvent(string tenantId, int sessionId, string userName, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)2, 20, tenantId, sessionId, userName, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteRunQueryStartEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 300, tenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteRunQueryStopEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 301, tenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteRunReportStartEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 302, tenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteRunReportStopEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 303, tenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteGetPageStartEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 304, tenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteGetPageStopEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 305, tenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
		}

		void INavServerTracingEvents.WriteInvokeCodeUnitStartEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 306, tenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteInvokeCodeUnitStopEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 307, tenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteInvokeFormApplicationMethodStartEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 308, tenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
		}

		void INavServerTracingEvents.WriteInvokeFormApplicationMethodStopEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 309, tenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteRunXmlPortStartEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 310, tenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteRunXmlPortStopEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 311, tenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
		}

		void INavServerTracingEvents.WriteCreateServiceSessionStartEvent(string tenantId, int sessionId, string userName, string connectionType, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 312, tenantId, sessionId, userName, connectionType, serverInstanceName);
		}

		void INavServerTracingEvents.WriteCreateServiceSessionStopEvent(string tenantId, int sessionId, string userName, string connectionType, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 313, tenantId, sessionId, userName, connectionType, serverInstanceName);
		}

		void INavServerTracingEvents.WriteEndServiceSessionStartEvent(string tenantId, int sessionId, string userName, string connectionType, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 314, tenantId, sessionId, userName, connectionType, serverInstanceName);
		}

		void INavServerTracingEvents.WriteEndServiceSessionStopEvent(string tenantId, int sessionId, string userName, string connectionType, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 315, tenantId, sessionId, userName, connectionType, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteALFunctionStartEvent(string tenantId, int sessionId, string objectType, int objectId, string objectExtensionAppId, string objectExtensionInfo, string functionName, string serverInstanceName, string platformVersion, string aadTenantId, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId)
		{
			eventSource.WriteEvent((EventKeywords)8, 400, tenantId, sessionId, objectType, objectId, objectExtensionAppId ?? string.Empty, objectExtensionInfo ?? string.Empty, functionName ?? string.Empty, serverInstanceName, platformVersion, aadTenantId, clientSessionId, clientActivityId, serverSessionId, serverActivityId);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteALFunctionStopEvent(string tenantId, int sessionId, string objectType, int objectId, string objectExtensionAppId, string objectExtensionInfo, string functionName, string serverInstanceName, string platformVersion, string aadTenantId, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId, long executionMilliseconds)
		{
			eventSource.WriteEvent((EventKeywords)8, 401, tenantId, sessionId, objectType, objectId, objectExtensionAppId ?? string.Empty, objectExtensionInfo ?? string.Empty, functionName ?? string.Empty, serverInstanceName, platformVersion, aadTenantId, clientSessionId, clientActivityId, serverSessionId, serverActivityId, executionMilliseconds);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteALFunctionStatementEvent(string tenantId, int sessionId, string objectType, int objectId, string objectExtensionAppId, string objectExtensionInfo, string functionName, int lineNumber, string statement, string serverInstanceName, string platformVersion, string aadTenantId, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId)
		{
			eventSource.WriteEvent((EventKeywords)8, 403, tenantId, sessionId, objectType, objectId, objectExtensionAppId ?? string.Empty, objectExtensionInfo ?? string.Empty, functionName ?? string.Empty, lineNumber, statement, serverInstanceName, platformVersion, aadTenantId, clientSessionId, clientActivityId, serverSessionId, serverActivityId);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteALFunctionFailedEvent(string tenantId, int sessionId, string userName, string objectType, int objectId, string objectExtensionAppId, string objectExtensionInfo, string functionName, string failureMessage, string serverInstanceName, string platformVersion, string aadTenantId, string aadUserId, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId)
		{
			eventSource.WriteEvent((EventKeywords)8, 402, tenantId, sessionId, userName ?? string.Empty, objectType, objectId, objectExtensionAppId ?? string.Empty, objectExtensionInfo ?? string.Empty, functionName ?? string.Empty, failureMessage ?? string.Empty, serverInstanceName, platformVersion, aadTenantId, aadUserId, clientSessionId, clientActivityId, serverSessionId, serverActivityId);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteALFunctionFailedEventNoPersonalData(string tenantId, int sessionId, string objectType, int objectId, string objectExtensionAppId, string objectExtensionInfo, string functionName, string failureMessage, string serverInstanceName, string platformVersion, string aadTenantId, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId)
		{
			eventSource.WriteEvent((EventKeywords)8, 404, tenantId, sessionId, objectType, objectId, objectExtensionAppId ?? string.Empty, objectExtensionInfo ?? string.Empty, functionName ?? string.Empty, failureMessage ?? string.Empty, serverInstanceName, platformVersion, aadTenantId, clientSessionId, clientActivityId, serverSessionId, serverActivityId);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSessionOpenedEvent(string tenantId, int sessionId, string userName, string connectionType, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 500, tenantId, sessionId, userName ?? string.Empty, connectionType, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteSessionClosedEvent(string tenantId, int sessionId, string userName, string connectionType, string serverInstanceName)
		{
			eventSource.WriteEvent((EventKeywords)4, 501, tenantId, sessionId, userName ?? string.Empty, connectionType, serverInstanceName);
		}

		[NonEvent]
		void INavServerTracingEvents.WritePermissionCheckStartEvent(string serverInstanceName, int sessionId, int objectType, int objectId, int permissions, int callingObjectType, int callingObjectId, int validationCategory)
		{
			eventSource.WriteEvent((EventKeywords)64, 800, serverInstanceName, sessionId, objectType, objectId, permissions, callingObjectType, callingObjectId, callingObjectType, callingObjectId, validationCategory);
		}

		[NonEvent]
		void INavServerTracingEvents.WritePermissionCheckSuccessfulEvent(string serverInstanceName, int sessionId, int objectType, int objectId, int permissions, int callingObjectType, int callingObjectId, int indirectPermissionFromCaller, int validationCategory)
		{
			eventSource.WriteEvent((EventKeywords)64, 801, serverInstanceName, sessionId, objectType, objectId, permissions, callingObjectType, callingObjectId, indirectPermissionFromCaller, validationCategory);
		}

		[NonEvent]
		void INavServerTracingEvents.WritePermissionCheckUnsuccessfulEvent(string serverInstanceName, int sessionId, int objectType, int objectId, int permissions, int callingObjectType, int callingObjectId, int indirectPermissionFromCaller, int validationCategory)
		{
			eventSource.WriteEvent((EventKeywords)64, 802, serverInstanceName, sessionId, objectType, objectId, permissions, callingObjectType, callingObjectId, indirectPermissionFromCaller, validationCategory);
		}

		[NonEvent]
		void INavServerTracingEvents.WritePermissionUncheckedEvent(string serverInstanceName, int sessionId, int objectType, int objectId, int permissions, int callingObjectType, int callingObjectId)
		{
			eventSource.WriteEvent((EventKeywords)64, 803, serverInstanceName, sessionId, objectType, objectId, permissions, callingObjectType, callingObjectId);
		}

		[NonEvent]
		void INavServerTracingEvents.WriteRecorderSessionError(string serverInstanceName, string tenantId, string recorder, int expectedSessionId, int actualSessionId)
		{
			eventSource.WriteEvent((EventKeywords)128, 820, serverInstanceName, tenantId, recorder, expectedSessionId, actualSessionId);
		}
	}

	public static class Keywords
	{
		private const ulong ReservedBitMask = 17592186044415uL;

		public const EventKeywords EventViewer = (EventKeywords)1L;

		public const EventKeywords SqlTracing = (EventKeywords)2L;

		public const EventKeywords ServiceCall = (EventKeywords)4L;

		public const EventKeywords ALFunctionCallTracing = (EventKeywords)8L;

		public const EventKeywords CrmTracing = (EventKeywords)16L;

		public const EventKeywords TelemetryTracing = (EventKeywords)32L;

		public const EventKeywords LicenseTelemetry = (EventKeywords)64L;

		public const EventKeywords SessionRecorderTelemetry = (EventKeywords)128L;

		[NonEvent]
		internal static bool Compare(EventKeywords keyword1, EventKeywords keyword2)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected I8, but got Unknown
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected I8, but got Unknown
			ulong num = (ulong)(keyword1 & 0xFFFFFFFFFFFL);
			ulong num2 = (ulong)(keyword2 & 0xFFFFFFFFFFFL);
			return num == num2;
		}
	}

	public static class Tasks
	{
		public const EventTask ExecuteScalar = (EventTask)1;

		public const EventTask ExecuteNonQuery = (EventTask)2;

		public const EventTask ExecuteReader = (EventTask)3;

		public const EventTask ReadNextResult = (EventTask)4;

		public const EventTask ReadNextRow = (EventTask)5;

		public const EventTask BeginTransaction = (EventTask)6;

		public const EventTask Prepare = (EventTask)7;

		public const EventTask OpenConnection = (EventTask)8;

		public const EventTask Commit = (EventTask)9;

		public const EventTask Rollback = (EventTask)10;

		public const EventTask CloseConnection = (EventTask)11;

		public const EventTask Message = (EventTask)12;

		public const EventTask ExceptionThrown = (EventTask)13;

		public const EventTask RunQuery = (EventTask)14;

		public const EventTask RunReport = (EventTask)15;

		public const EventTask GetPage = (EventTask)16;

		public const EventTask InvokeCodeUnit = (EventTask)17;

		public const EventTask InvokeFormApplicationMethod = (EventTask)18;

		public const EventTask RunXmlPort = (EventTask)19;

		public const EventTask ExecuteALFunction = (EventTask)20;

		public const EventTask OpenSession = (EventTask)21;

		public const EventTask CloseSession = (EventTask)22;

		public const EventTask CreateServiceSession = (EventTask)23;

		public const EventTask EndServiceSession = (EventTask)24;

		public const EventTask CrmRetrieveMultiple = (EventTask)25;

		public const EventTask CrmRetrieve = (EventTask)26;

		public const EventTask CrmCreate = (EventTask)27;

		public const EventTask CrmUpdate = (EventTask)28;

		public const EventTask CrmDelete = (EventTask)29;

		public const EventTask CrmAssociate = (EventTask)30;

		public const EventTask CrmDisassociate = (EventTask)31;

		public const EventTask CrmSetState = (EventTask)32;

		public const EventTask TelemetryData = (EventTask)33;
	}

	internal static class EventId
	{
		public const int SqlExecuteScalarStart = 1;

		public const int SqlExecuteScalarStop = 2;

		public const int SqlExecuteNonQueryStart = 3;

		public const int SqlExecuteNonQueryStop = 4;

		public const int SqlExecuteReaderStart = 5;

		public const int SqlExecuteReaderStop = 6;

		public const int SqlReadNextResultStart = 7;

		public const int SqlReadNextResultStop = 8;

		public const int SqlReadNextRowStart = 9;

		public const int SqlReadNextRowStop = 10;

		public const int SqlBeginTransactionStart = 11;

		public const int SqlBeginTransactionStop = 12;

		public const int SqlPrepareStart = 13;

		public const int SqlPrepareStop = 14;

		public const int SqlOpenConnectionStart = 15;

		public const int SqlOpenConnectionStop = 16;

		public const int SqlCommitStart = 17;

		public const int SqlCommitStop = 18;

		public const int SqlRollbackStart = 19;

		public const int SqlRollbackStop = 20;

		public const int ScomFatalSqlError = 112;

		public const int ScomSqlConnectionEstablished = 113;

		public const int SubsystemErrorMessage = 200;

		public const int SubsystemInfoMessage = 201;

		public const int SubsystemWarningMessage = 202;

		public const int NavWindowsServiceStart = 203;

		public const int NavWindowsServiceStop = 204;

		public const int ODataConnected = 205;

		public const int ODataClosed = 206;

		public const int WebServiceConnected = 207;

		public const int WebServiceClosed = 208;

		public const int RtcConnected = 209;

		public const int RtcClosed = 210;

		public const int ALInfoMessage = 211;

		public const int AdminServiceConnected = 214;

		public const int AdminSeviceClosed = 215;

		public const int ExceptionErrorMessage = 216;

		public const int ExceptionInfoMessage = 217;

		public const int ExceptionWarningMessage = 218;

		public const int NasStarting = 219;

		public const int NasCompleted = 220;

		public const int NasFailed = 221;

		public const int NasPermanentlyFailed = 222;

		public const int NavServerInitializing = 223;

		public const int NavServerReady = 224;

		public const int NavServerShuttingDown = 225;

		public const int ApplicationMounted = 226;

		public const int ApplicationMountFailed = 227;

		public const int TenantMounted = 228;

		public const int TenantMountFailed = 229;

		public const int TenantDismounted = 230;

		public const int AuthenticationSubsystemFailed = 231;

		public const int AuthenticationSubsystemInfoMessage = 232;

		public const int LimitedUserOptionalTablePermissionErrorInfoMessage = 233;

		public const int TenantConfigLoaded = 234;

		public const int RunQueryStart = 300;

		public const int RunQueryStop = 301;

		public const int RunReportStart = 302;

		public const int RunReportStop = 303;

		public const int GetPageStart = 304;

		public const int GetPageStop = 305;

		public const int InvokeCodeUnitStart = 306;

		public const int InvokeCodeUnitStop = 307;

		public const int InvokeFormApplicationMethodStart = 308;

		public const int InvokeFormApplicationMethodStop = 309;

		public const int RunXmlPortStart = 310;

		public const int RunXmlPortStop = 311;

		public const int CreateServiceSessionStart = 312;

		public const int CreateServiceSessionStop = 313;

		public const int EndServiceSessionStart = 314;

		public const int EndServiceSessionStop = 315;

		public const int ALFunctionStart = 400;

		public const int ALFunctionStop = 401;

		public const int ALFunctionFailed = 402;

		public const int ALFunctionStatement = 403;

		public const int ALFunctionFailedNoPersonalData = 404;

		public const int SessionOpened = 500;

		public const int SessionClosed = 501;

		public const int PermissionCheckStartEventId = 800;

		public const int PermissionAccessSuccessEventId = 801;

		public const int PermissionAccessDeniedEventId = 802;

		public const int PermissionUncheckedEventId = 803;

		public const int SessionRecorderErrorEvent = 820;

		public const int CertificateMonitorPermanentlyFailed = 1000;

		public const int CertificateReplace = 1001;

		public const int CertificateExpired = 1002;

		public const int CertificateCloseToExpiration = 1003;

		public const int CertificateNew = 1006;
	}

	private class NavServerEventViewerEventsImplementation : INavServerEventViewerEvents
	{
		private readonly NavServerEventSource eventSource;

		public NavServerEventViewerEventsImplementation(NavServerEventSource eventSource)
		{
			this.eventSource = eventSource;
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteSubsystemErrorMessageEvent(string serverInstanceName, string navTenantId, string message)
		{
			WriteEvent(200, serverInstanceName, navTenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteSubsystemWarningMessageEvent(string serverInstanceName, string tenantId, string message)
		{
			WriteEvent(202, serverInstanceName, tenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteSubsystemInfoMessageEvent(string serverInstanceName, string tenantId, string message)
		{
			WriteEvent(201, serverInstanceName, tenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteExceptionErrorMessageEvent(string serverInstanceName, string tenantId, string message)
		{
			WriteEvent(216, serverInstanceName, tenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteExceptionWarningMessageEvent(string serverInstanceName, string tenantId, string message)
		{
			WriteEvent(218, serverInstanceName, tenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteExceptionInfoMessageEvent(string serverInstanceName, string tenantId, string message)
		{
			WriteEvent(217, serverInstanceName, tenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteODataConnectedEvent(string serverInstanceName, string message)
		{
			WriteEvent(205, serverInstanceName, null, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteODataConnectionClosedEvent(string serverInstanceName, string message)
		{
			WriteEvent(206, serverInstanceName, null, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteRtcConnectedEvent(string serverInstanceName, string message)
		{
			WriteEvent(209, serverInstanceName, null, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteRtcConnectionClosedEvent(string serverInstanceName, string message)
		{
			WriteEvent(210, serverInstanceName, null, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteWebServiceConnectedEvent(string serverInstanceName, string message)
		{
			WriteEvent(207, serverInstanceName, null, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteWebServiceConnectionClosedEvent(string serverInstanceName, string message)
		{
			WriteEvent(208, serverInstanceName, null, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteAdminServiceConnectedEvent(string serverInstanceName, string message)
		{
			WriteEvent(214, serverInstanceName, null, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteAdminServiceConnectionClosedEvent(string serverInstanceName, string message)
		{
			WriteEvent(215, serverInstanceName, null, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteNasStartingEvent(string serverInstanceName, string tenantId, string message)
		{
			WriteEvent(219, serverInstanceName, tenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteNasCompletedEvent(string serverInstanceName, string tenantId, string message)
		{
			WriteEvent(220, serverInstanceName, tenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteNasFailedEvent(string serverInstanceName, string tenantId, string message)
		{
			WriteEvent(221, serverInstanceName, tenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteNasPermanentlyFailedEvent(string serverInstanceName, string tenantId, string message)
		{
			WriteEvent(222, serverInstanceName, tenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteALInfoMessageEvent(string serverInstanceName, string tenantId, string message)
		{
			WriteEvent(211, serverInstanceName, tenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteNavServerInitializingEvent(string serverInstanceName, string message)
		{
			WriteEvent(223, serverInstanceName, null, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteNavServerReadyEvent(string serverInstanceName, string message)
		{
			WriteEvent(224, serverInstanceName, null, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteNavServerShuttingDownEvent(string serverInstanceName, string message)
		{
			WriteEvent(225, serverInstanceName, null, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteApplicationMountedEvent(string serverInstanceName, string message)
		{
			WriteEvent(226, serverInstanceName, null, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteApplicationMountFailedEvent(string serverInstanceName, string message)
		{
			WriteEvent(227, serverInstanceName, null, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteTenantConfigLoadedEvent(string serverInstanceName, string navTenantId, string message)
		{
			WriteEvent(234, serverInstanceName, navTenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteTenantMountedEvent(string serverInstanceName, string navTenantId, string message)
		{
			WriteEventRaw(228, serverInstanceName, navTenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteTenantMountFailedEvent(string serverInstanceName, string navTenantId, string message)
		{
			WriteEvent(229, serverInstanceName, navTenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteTenantDismountedEvent(string serverInstanceName, string navTenantId, string message)
		{
			WriteEvent(230, serverInstanceName, navTenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteAuthenticationSubsystemFailedEvent(string serverInstanceName, string navTenantId, string message)
		{
			WriteEvent(231, serverInstanceName, navTenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteAuthenticationSubsystemInfoMessageEvent(string serverInstanceName, string navTenantId, string message)
		{
			WriteEvent(232, serverInstanceName, navTenantId, message);
		}

		void INavServerEventViewerEvents.LimitedUserOptionalTablePermissionErrorInfoMessage(string serverInstanceName, string navTenantId, string message)
		{
			WriteEvent(233, serverInstanceName, navTenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteScomFatalSqlErrorEvent(string serverInstanceName, string navTenantId, string message)
		{
			WriteEventRaw(112, serverInstanceName, navTenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteScomSqlConnectionEstablishedEvent(string serverInstanceName, string navTenantId, string message)
		{
			WriteEventRaw(113, serverInstanceName, navTenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteCertificateMonitorPermanentlyFailedEvent(string serverInstanceName, string navTenantId, string message)
		{
			WriteEvent(1000, serverInstanceName, navTenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteCertificateReplacedInformationEvent(string serverInstanceName, string navTenantId, string message)
		{
			WriteEvent(1001, serverInstanceName, navTenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteCertificateExpiredErrorEvent(string serverInstanceName, string navTenantId, string message)
		{
			WriteEvent(1002, serverInstanceName, navTenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteCertificateCloseToExpirationWarningEvent(string serverInstanceName, string navTenantId, string message)
		{
			WriteEvent(1003, serverInstanceName, navTenantId, message);
		}

		[NonEvent]
		void INavServerEventViewerEvents.WriteCertificateNewInformationEvent(string serverInstanceName, string navTenantId, string message)
		{
			WriteEvent(1006, serverInstanceName, navTenantId, message);
		}

		[NonEvent]
		private void WriteEvent(int id, string serverInstanceName, string tenantId, string message)
		{
			if (!((EventSource)eventSource).IsEnabled())
			{
				return;
			}
			string messageToLog = CreateMessage(message);
			if (tenantId == null)
			{
				RunWriteEventMethodWithExceptionHandling(() =>
				{
					((EventSource)eventSource).WriteEvent(id, serverInstanceName, messageToLog);
				});
			}
			else
			{
				RunWriteEventMethodWithExceptionHandling(() =>
				{
					((EventSource)eventSource).WriteEvent(id, serverInstanceName, tenantId, messageToLog);
				});
			}
		}

		[NonEvent]
		private void WriteEventRaw(int id, string serverInstanceName, string tenantId, string message)
		{
			if (((EventSource)eventSource).IsEnabled())
			{
				RunWriteEventMethodWithExceptionHandling(() =>
				{
					((EventSource)eventSource).WriteEvent(id, serverInstanceName, tenantId ?? string.Empty, message);
				});
			}
		}

		private static string CreateMessage(string message)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (NavCurrentThread.Session != null)
			{
				stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Session type: {0}", NavCurrentThread.Session.ClientConnectionType));
				stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "Session ID: {0}", NavCurrentThread.Session.Id));
			}
			stringBuilder.Append(message);
			return stringBuilder.ToString();
		}
	}

	private const string RecorderSessionErrorMessage = "Server instance: {0}\nTenant ID: {1}\nRecorder: {2}\nExpected Recorded Session ID: {3}\nActual Recorded Session ID: {4}\n";

	private bool inTelemetryEvent;

	private const string LicenseTelemetryChannelMessage = "Server instance: {0}\nSession ID: {1}\nObject Type: {2}\nObject ID: {3}\nPermissions: {4}\nCalling Object Type: {5}\nCalling Object ID: {6}\nValidation Category: {7}";

	private const string AlFunctionTracingStartStopChannelMessage = "AAD tenant ID: {9}\nPlatform version: {8}\nServer instance: {7}\nTenant ID: {0}\nSession ID: {1}\nObject type: {2}\nObject ID: {3}\nObject App ID: {4}\nObject App Info: {5}\nFunction name: {6}";

	private const string ALFunctionTracingStatementChannelMessage = "AAD tenant ID: {11}\nPlatform version: {10}\nServer instance: {9}\nTenant ID: {0}\nSession ID: {1}\nObject type: {2}\nObject ID: {3}\nObject App ID: {4}\nObject App Info: {5}\nFunction name: {6}\nLine number: {7}\nStatement: {8}";

	private const string ALFunctionTracingFailedChannelMessage = "AAD tenant ID: {11}\nPlatform version: {10}\nServer instance: {9}\nTenant ID: {0}\nSession ID: {1}\nObject type: {3}\nObject ID: {4}\nObject App ID: {5}\nObject App Info: {6}\nFunction name: {7}\nFailure message: {8}";

	private const string ALFunctionTracingFailedChannelMessageNoPersonalData = "AAD tenant ID: {10}\nPlatform version: {9}\nServer instance: {8}\nTenant ID: {0}\nSession ID: {1}\nObject type: {2}\nObject ID: {3}\nObject App ID: {4}\nObject App Info: {5}\nFunction name: {6}\nFailure message: {7}";

	private const string ServiceCallTracingAlObjectStartStopChannelMessage = "Server instance: {4}\nTenant ID: {0}\nSession ID: {1}\nUser name: {2}\nObject ID: {3}";

	private const string ServiceCallTracingSessionServiceStartStopChannelMessage = "Server instance: {4}\nTenant ID: {0}\nSession ID: {1}\nUser name: {2}\nConnection type: {3}";

	private const string SqlTracingWithNoSqlStatementChannelMessage = "Server instance: {3}\nTenant ID: {0}\nSession ID: {1}\nUser name: {2}";

	private const string SqlTracingWithSqlStatementChannelMessage = "Server instance: {4}\nTenant ID: {0}\nSession ID: {1}\nUser name: {2}\nSql statement: {3}";

	private volatile INavServerTracingEvents navServerTracingEvents;

	public static readonly string[] TaskNames = new string[33]
	{
		"None", "ExecuteScalar", "ExecuteNonQuery", "ExecuteReader", "ReadNextResult", "ReadNextRow", "BeginTransaction", "Prepare", "OpenConnection", "Commit",
		"Rollback", "CloseConnection", "Message", "ExceptionThrown", "RunQuery", "RunReport", "GetPage", "InvokeCodeUnit", "InvokeFormApplicationMethod", "RunXmlPort",
		"ExecuteALFunction", "OpenSession", "CloseSession", "CreateServiceSession", "EndServiceSession", "CrmRetrieveMultiple", "CrmRetrieve", "CrmCreate", "CrmUpdate", "CrmDelete",
		"CrmAssociate", "CrmDisassociate", "CrmSetState"
	};

	internal static readonly int[] EventViewerEvents = new int[44]
	{
		112, 113, 200, 201, 202, 205, 206, 207, 208, 209,
		210, 211, 214, 215, 216, 217, 218, 219, 220, 221,
		222, 223, 225, 224, 226, 227, 234, 228, 229, 230,
		231, 232, 233, 1000, 1001, 1002, 1003, 1006, 700, 701,
		705, 707, 708, 712
	};

	public const string EventViewerChannelMessageWithServerInstance = "Server instance: {0}\n{1}";

	public const string EventViewerChannelMessageWithServerInstanceAndTenantId = "Server instance: {0}\nTenant ID: {1}\n{2}";

	public const string EventViewerChannelMessageWithTenantInMessage = "Server instance: {0}\nTenant: {1}\n{2}";

	private volatile INavServerEventViewerEvents navServerEventViewerEvents;

	internal const string IgnoreOnEventCommand = "IgnoreOnEventCommand";

	private static readonly Lazy<NavServerEventSource> log = new Lazy<NavServerEventSource>(() => new NavServerEventSource());

	private readonly object mutex = new object();

	private volatile int registrationCount;

	private volatile IExecutionListener executionListener;

	[ThreadStatic]
	private static bool writeEventWithExceptionHandlingAlreadyCalledOnThisThead;

	public INavServerTracingEvents NavServerTracingEvents
	{
		[NonEvent]
		get
		{
			if (navServerTracingEvents == null)
			{
				lock (mutex)
				{
					if (navServerTracingEvents == null)
					{
						return navServerTracingEvents = new NavServerTracingEventsImplementation(this);
					}
				}
			}
			return navServerTracingEvents;
		}
		[NonEvent]
		set
		{
			lock (mutex)
			{
				navServerTracingEvents = value;
			}
		}
	}

	public INavServerEventViewerEvents NavServerEventViewerEvents
	{
		[NonEvent]
		get
		{
			if (navServerEventViewerEvents == null)
			{
				lock (mutex)
				{
					if (navServerEventViewerEvents == null)
					{
						return navServerEventViewerEvents = new NavServerEventViewerEventsImplementation(this);
					}
				}
			}
			return navServerEventViewerEvents;
		}
		[NonEvent]
		set
		{
			lock (mutex)
			{
				navServerEventViewerEvents = value;
			}
		}
	}

	public static NavServerEventSource Log
	{
		[NonEvent]
		get
		{
			return log.Value;
		}
	}

	private bool IsDisabledFromController => registrationCount == 0;

	internal IExecutionListener AlFunctionExecutionListener
	{
		[NonEvent]
		get
		{
			if (executionListener == null)
			{
				lock (mutex)
				{
					if (executionListener == null)
					{
						return executionListener = new ALFunctionTracingExecutionListener();
					}
				}
			}
			return executionListener;
		}
		[NonEvent]
		set
		{
			lock (mutex)
			{
				executionListener = value;
			}
		}
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteRecorderSessionError(string serverInstanceName, string tenantId, string recorder, int expectedSessionId, int actualSessionId)
	{
		NavServerTracingEvents.WriteRecorderSessionError(serverInstanceName, tenantId, recorder, expectedSessionId, actualSessionId);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WritePermissionCheckStartEvent(string serverInstanceName, string navTenantId, int sessionId, int objectType, int objectId, int permissions, int callingObjectType, int callingObjectId, int validationCategory)
	{
		if (NavPermissionTelemetryRegistrar.HasListeners && !inTelemetryEvent)
		{
			inTelemetryEvent = true;
			try
			{
				NavPermissionTelemetryRegistrar.OnPermissionTelemetry(sessionId, NavPermissionTelemetryEvent.CreateEvent(800, serverInstanceName, navTenantId, sessionId, objectType, objectId, permissions, callingObjectType, callingObjectId, 0, validationCategory));
			}
			finally
			{
				inTelemetryEvent = false;
			}
		}
		NavServerTracingEvents.WritePermissionCheckStartEvent(serverInstanceName, sessionId, objectType, objectId, permissions, callingObjectType, callingObjectId, validationCategory);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WritePermissionCheckSuccessfulEvent(string serverInstanceName, string navTenantId, int sessionId, int objectType, int objectId, int permissions, int callingObjectType, int callingObjectId, int indirectPermissionFromCaller, int validationCategory)
	{
		if (NavPermissionTelemetryRegistrar.HasListeners && !inTelemetryEvent)
		{
			inTelemetryEvent = true;
			try
			{
				NavPermissionTelemetryRegistrar.OnPermissionTelemetry(sessionId, NavPermissionTelemetryEvent.CreateEvent(801, serverInstanceName, navTenantId, sessionId, objectType, objectId, permissions, callingObjectType, callingObjectId, indirectPermissionFromCaller, validationCategory));
			}
			finally
			{
				inTelemetryEvent = false;
			}
		}
		NavServerTracingEvents.WritePermissionCheckSuccessfulEvent(serverInstanceName, sessionId, objectType, objectId, permissions, callingObjectType, callingObjectId, indirectPermissionFromCaller, validationCategory);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WritePermissionCheckUnsuccessfulEvent(string serverInstanceName, string navTenantId, int sessionId, int objectType, int objectId, int permissions, int callingObjectType, int callingObjectId, int indirectPermissionFromCaller, int validationCategory)
	{
		if (NavPermissionTelemetryRegistrar.HasListeners && !inTelemetryEvent)
		{
			inTelemetryEvent = true;
			try
			{
				NavPermissionTelemetryRegistrar.OnPermissionTelemetry(sessionId, NavPermissionTelemetryEvent.CreateEvent(802, serverInstanceName, navTenantId, sessionId, objectType, objectId, permissions, callingObjectType, callingObjectId, indirectPermissionFromCaller, validationCategory));
			}
			finally
			{
				inTelemetryEvent = false;
			}
		}
		NavServerTracingEvents.WritePermissionCheckUnsuccessfulEvent(serverInstanceName, sessionId, objectType, objectId, permissions, callingObjectType, callingObjectId, indirectPermissionFromCaller, validationCategory);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WritePermissionUncheckedEvent(string serverInstanceName, string navTenantId, int sessionId, int objectType, int objectId, int permissions, int callingObjectType, int callingObjectId)
	{
		if (NavPermissionTelemetryRegistrar.HasListeners && !inTelemetryEvent)
		{
			inTelemetryEvent = true;
			try
			{
				NavPermissionTelemetryRegistrar.OnPermissionTelemetry(sessionId, NavPermissionTelemetryEvent.CreateEvent(803, serverInstanceName, navTenantId, sessionId, objectType, objectId, permissions, callingObjectType, callingObjectId));
			}
			finally
			{
				inTelemetryEvent = false;
			}
		}
		NavServerTracingEvents.WritePermissionUncheckedEvent(serverInstanceName, sessionId, objectType, objectId, permissions, callingObjectType, callingObjectId);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteALFunctionStartEvent(string navTenantId, int sessionId, string objectType, int objectId, string objectExtensionAppId, string objectExtensionInfo, string functionName, string serverInstanceName, string platformVersion, string aadTenantId, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId)
	{
		NavServerTracingEvents.WriteALFunctionStartEvent(navTenantId, sessionId, objectType, objectId, objectExtensionAppId, objectExtensionInfo, functionName, serverInstanceName, platformVersion, aadTenantId, clientSessionId, clientActivityId, serverSessionId, serverActivityId);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteALFunctionStopEvent(string navTenantId, int sessionId, string objectType, int objectId, string objectExtensionAppId, string objectExtensionInfo, string functionName, string serverInstanceName, string platformVersion, string aadTenantId, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId, long executionMilliseconds)
	{
		NavServerTracingEvents.WriteALFunctionStopEvent(navTenantId, sessionId, objectType, objectId, objectExtensionAppId, objectExtensionInfo, functionName, serverInstanceName, platformVersion, aadTenantId, clientSessionId, clientActivityId, serverSessionId, serverActivityId, executionMilliseconds);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteALFunctionStatementEvent(string navTenantId, int sessionId, string objectType, int objectId, string objectExtensionAppId, string objectExtensionInfo, string functionName, int lineNumber, string statement, string serverInstanceName, string platformVersion, string aadTenantId, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId)
	{
		NavServerTracingEvents.WriteALFunctionStatementEvent(navTenantId, sessionId, objectType, objectId, objectExtensionAppId, objectExtensionInfo, functionName, lineNumber, statement, serverInstanceName, platformVersion, aadTenantId, clientSessionId, clientActivityId, serverSessionId, serverActivityId);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteALFunctionFailedEvent(string navTenantId, int sessionId, string userName, string objectType, int objectId, string objectExtensionAppId, string objectExtensionInfo, string functionName, string failureMessage, string serverInstanceName, string platformVersion, string aadTenantId, string aadUserId, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId)
	{
		NavServerTracingEvents.WriteALFunctionFailedEvent(navTenantId, sessionId, userName, objectType, objectId, objectExtensionAppId, objectExtensionInfo, functionName, failureMessage, serverInstanceName, platformVersion, aadTenantId, aadUserId, clientSessionId, clientActivityId, serverSessionId, serverActivityId);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteALFunctionFailedEventNoPersonalData(string navTenantId, int sessionId, string objectType, int objectId, string objectExtensionAppId, string objectExtensionInfo, string functionName, string failureMessage, string serverInstanceName, string platformVersion, string aadTenantId, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId)
	{
		NavServerTracingEvents.WriteALFunctionFailedEventNoPersonalData(navTenantId, sessionId, objectType, objectId, objectExtensionAppId, objectExtensionInfo, functionName, failureMessage, serverInstanceName, platformVersion, aadTenantId, clientSessionId, clientActivityId, serverSessionId, serverActivityId);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteRunQueryStartEvent(string navTenantId, int sessionId, string userName, int objectId, string serverInstanceName)
	{
		NavServerTracingEvents.WriteRunQueryStartEvent(navTenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteRunQueryStopEvent(string navTenantId, int sessionId, string userName, int objectId, string serverInstanceName)
	{
		NavServerTracingEvents.WriteRunQueryStopEvent(navTenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteRunReportStartEvent(string navTenantId, int sessionId, string userName, int objectId, string serverInstanceName)
	{
		NavServerTracingEvents.WriteRunReportStartEvent(navTenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteRunReportStopEvent(string navTenantId, int sessionId, string userName, int objectId, string serverInstanceName)
	{
		NavServerTracingEvents.WriteRunReportStopEvent(navTenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteGetPageStartEvent(string navTenantId, int sessionId, string userName, int objectId, string serverInstanceName)
	{
		NavServerTracingEvents.WriteGetPageStartEvent(navTenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteGetPageStopEvent(string navTenantId, int sessionId, string userName, int objectId, string serverInstanceName)
	{
		NavServerTracingEvents.WriteGetPageStopEvent(navTenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteRunXmlPortStartEvent(string navTenantId, int sessionId, string userName, int objectId, string serverInstanceName)
	{
		NavServerTracingEvents.WriteRunXmlPortStartEvent(navTenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteRunXmlPortStopEvent(string navTenantId, int sessionId, string userName, int objectId, string serverInstanceName)
	{
		NavServerTracingEvents.WriteRunXmlPortStopEvent(navTenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteInvokeCodeUnitStartEvent(string navTenantId, int sessionId, string userName, int objectId, string serverInstanceName)
	{
		NavServerTracingEvents.WriteInvokeCodeUnitStartEvent(navTenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteInvokeCodeUnitStopEvent(string navTenantId, int sessionId, string userName, int objectId, string serverInstanceName)
	{
		NavServerTracingEvents.WriteInvokeCodeUnitStopEvent(navTenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteInvokeFormApplicationMethodStartEvent(string navTenantId, int sessionId, string userName, int objectId, string serverInstanceName)
	{
		NavServerTracingEvents.WriteInvokeFormApplicationMethodStartEvent(navTenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteInvokeFormApplicationMethodStopEvent(string navTenantId, int sessionId, string userName, int objectId, string serverInstanceName)
	{
		NavServerTracingEvents.WriteInvokeFormApplicationMethodStopEvent(navTenantId, sessionId, userName ?? string.Empty, objectId, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSessionOpenedEvent(string navTenantId, int sessionId, string userName, string connectionType, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSessionOpenedEvent(navTenantId, sessionId, userName, connectionType, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSessionClosedEvent(string navTenantId, int sessionId, string userName, string connectionType, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSessionClosedEvent(navTenantId, sessionId, userName, connectionType, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteCreateServiceSessionStartEvent(string navTenantId, int sessionId, string userName, string connectionType, string serverInstanceName)
	{
		NavServerTracingEvents.WriteCreateServiceSessionStartEvent(navTenantId, sessionId, userName, connectionType, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteCreateServiceSessionStopEvent(string navTenantId, int sessionId, string userName, string connectionType, string serverInstanceName)
	{
		NavServerTracingEvents.WriteCreateServiceSessionStopEvent(navTenantId, sessionId, userName, connectionType, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteEndServiceSessionStartEvent(string navTenantId, int sessionId, string userName, string connectionType, string serverInstanceName)
	{
		NavServerTracingEvents.WriteEndServiceSessionStartEvent(navTenantId, sessionId, userName, connectionType, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteEndServiceSessionStopEvent(string navTenantId, int sessionId, string userName, string connectionType, string serverInstanceName)
	{
		NavServerTracingEvents.WriteEndServiceSessionStopEvent(navTenantId, sessionId, userName, connectionType, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlExecuteScalarStartEvent(string navTenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlExecuteScalarStartEvent(navTenantId, sessionId, userName, sqlStatement, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlExecuteScalarStopEvent(string navTenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlExecuteScalarStopEvent(navTenantId, sessionId, userName, sqlStatement, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlExecuteNonQueryStartEvent(string navTenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlExecuteNonQueryStartEvent(navTenantId, sessionId, userName, sqlStatement, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlExecuteNonQueryStopEvent(string navTenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlExecuteNonQueryStopEvent(navTenantId, sessionId, userName, sqlStatement, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlExecuteReaderStartEvent(string navTenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlExecuteReaderStartEvent(navTenantId, sessionId, userName, sqlStatement, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlExecuteReaderStopEvent(string navTenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlExecuteReaderStopEvent(navTenantId, sessionId, userName, sqlStatement, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlReadNextResultStartEvent(string navTenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlReadNextResultStartEvent(navTenantId, sessionId, userName, sqlStatement, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlReadNextResultStopEvent(string navTenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlReadNextResultStopEvent(navTenantId, sessionId, userName, sqlStatement, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlReadNextRowStartEvent(string navTenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlReadNextRowStartEvent(navTenantId, sessionId, userName, sqlStatement, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlReadNextRowStopEvent(string navTenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlReadNextRowStopEvent(navTenantId, sessionId, userName, sqlStatement, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlBeginTransactionStartEvent(string navTenantId, int sessionId, string userName, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlBeginTransactionStartEvent(navTenantId, sessionId, userName, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlBeginTransactionStopEvent(string navTenantId, int sessionId, string userName, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlBeginTransactionStopEvent(navTenantId, sessionId, userName, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlPrepareStartEvent(string navTenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlPrepareStartEvent(navTenantId, sessionId, userName, sqlStatement, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlPrepareStopEvent(string navTenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlPrepareStopEvent(navTenantId, sessionId, userName, sqlStatement, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlOpenConnectionStartEvent(string navTenantId, int sessionId, string userName, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlOpenConnectionStartEvent(navTenantId, sessionId, userName, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlOpenConnectionStopEvent(string navTenantId, int sessionId, string userName, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlOpenConnectionStopEvent(navTenantId, sessionId, userName, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlCommitStartEvent(string navTenantId, int sessionId, string userName, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlCommitStartEvent(navTenantId, sessionId, userName, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlCommitStopEvent(string tenantId, int sessionId, string userName, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlCommitStopEvent(tenantId, sessionId, userName, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlRollbackStartEvent(string navTenantId, int sessionId, string userName, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlRollbackStartEvent(navTenantId, sessionId, userName, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSqlRollbackStopEvent(string navTenantId, int sessionId, string userName, string serverInstanceName)
	{
		NavServerTracingEvents.WriteSqlRollbackStopEvent(navTenantId, sessionId, userName, serverInstanceName);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSubsystemErrorMessageEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteSubsystemErrorMessageEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSubsystemWarningMessageEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteSubsystemWarningMessageEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteSubsystemInfoMessageEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteSubsystemInfoMessageEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteNavServerInitializingEvent(string serverInstanceName, string message)
	{
		NavServerEventViewerEvents.WriteNavServerInitializingEvent(serverInstanceName, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteNavServerReadyEvent(string serverInstanceName, string message)
	{
		NavServerEventViewerEvents.WriteNavServerReadyEvent(serverInstanceName, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteNavServerShuttingDownEvent(string serverInstanceName, string message)
	{
		NavServerEventViewerEvents.WriteNavServerShuttingDownEvent(serverInstanceName, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteApplicationMountedEvent(string serverInstanceName, string message)
	{
		NavServerEventViewerEvents.WriteApplicationMountedEvent(serverInstanceName, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteApplicationMountFailedEvent(string serverInstanceName, string message)
	{
		NavServerEventViewerEvents.WriteApplicationMountFailedEvent(serverInstanceName, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteTenantConfigLoadedEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteTenantConfigLoadedEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteTenantMountedEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteTenantMountedEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteTenantMountFailedEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteTenantMountFailedEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteTenantDismountedEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteTenantDismountedEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteExceptionErrorMessageEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteExceptionErrorMessageEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteExceptionWarningMessageEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteExceptionWarningMessageEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteExceptionInfoMessageEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteExceptionInfoMessageEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteODataConnectedEvent(string serverInstanceName, string message)
	{
		NavServerEventViewerEvents.WriteODataConnectedEvent(serverInstanceName, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteODataConnectionClosedEvent(string serverInstanceName, string message)
	{
		NavServerEventViewerEvents.WriteODataConnectionClosedEvent(serverInstanceName, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteRtcConnectedEvent(string serverInstanceName, string message)
	{
		NavServerEventViewerEvents.WriteRtcConnectedEvent(serverInstanceName, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteRtcConnectionClosedEvent(string serverInstanceName, string message)
	{
		NavServerEventViewerEvents.WriteRtcConnectionClosedEvent(serverInstanceName, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteWebServiceConnectedEvent(string serverInstanceName, string message)
	{
		NavServerEventViewerEvents.WriteWebServiceConnectedEvent(serverInstanceName, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteWebServiceConnectionClosedEvent(string serverInstanceName, string message)
	{
		NavServerEventViewerEvents.WriteWebServiceConnectionClosedEvent(serverInstanceName, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteAdminServiceConnectedEvent(string serverInstanceName, string message)
	{
		NavServerEventViewerEvents.WriteAdminServiceConnectedEvent(serverInstanceName, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteAdminServiceConnectionClosedEvent(string serverInstanceName, string message)
	{
		NavServerEventViewerEvents.WriteAdminServiceConnectionClosedEvent(serverInstanceName, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteNasStartingEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteNasStartingEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteNasCompletedEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteNasCompletedEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteNasFailedEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteNasFailedEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteNasPermanentlyFailedEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteNasPermanentlyFailedEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteALInfoMessageEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteALInfoMessageEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteAuthenticationSubsystemFailedEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteAuthenticationSubsystemFailedEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteAuthenticationSubsystemInfoMessage(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteAuthenticationSubsystemInfoMessageEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void LimitedUserOptionalTablePermissionErrorInfoMessage(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.LimitedUserOptionalTablePermissionErrorInfoMessage(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteScomFatalSqlErrorEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteScomFatalSqlErrorEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteScomSqlConnectionEstablishedEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteScomSqlConnectionEstablishedEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteCertificateMonitorPermanentlyFailedEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteCertificateMonitorPermanentlyFailedEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteCertificateReplacedInformationEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteCertificateReplacedInformationEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteCertificateNewInformationEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteCertificateNewInformationEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteCertificateExpiredErrorEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteCertificateExpiredErrorEvent(serverInstanceName, navTenantId, message);
	}

	[Event(/*Could not decode attribute arguments.*/)]
	public void WriteCertificateCloseToExpirationWarningEvent(string serverInstanceName, string navTenantId, string message)
	{
		NavServerEventViewerEvents.WriteCertificateCloseToExpirationWarningEvent(serverInstanceName, navTenantId, message);
	}

	internal NavServerEventSource()
		: base(true)
	{
	}

	[NonEvent]
	protected override void OnEventCommand(EventCommandEventArgs command)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		try
		{
			if (command.Arguments == null || !command.Arguments.TryGetValue("IgnoreOnEventCommand", out var value) || string.Compare(value, true.ToString(), StringComparison.OrdinalIgnoreCase) != 0)
			{
				if ((int)command.Command == -2)
				{
					RegisterExecutionListener();
				}
				else if ((int)command.Command == -3)
				{
					UnRegisterExecutionListener();
				}
			}
		}
		finally
		{
			((EventSource)this).OnEventCommand(command);
		}
	}

	[NonEvent]
	internal void RegisterExecutionListener()
	{
		lock (mutex)
		{
			registrationCount++;
			SendTraceTag("00001Y3", Verbosity.Normal, PrivacyClassification.SystemMetadata, "Register AlFunctionExecutionListener, count = " + registrationCount);
			if (!ExecutionListener.AllRegisteredListeners.Contains(AlFunctionExecutionListener))
			{
				ExecutionListener.AddListener(AlFunctionExecutionListener);
				SendTraceTag("00001Y4", Verbosity.Normal, PrivacyClassification.SystemMetadata, "AlFunctionExecutionListener added");
			}
		}
	}

	[NonEvent]
	internal void UnRegisterExecutionListener()
	{
		lock (mutex)
		{
			registrationCount--;
			SendTraceTag("00001Y5", Verbosity.Normal, PrivacyClassification.SystemMetadata, "UnRegister AlFunctionExecutionListener, count = " + registrationCount);
			if (registrationCount == 0)
			{
				ExecutionListener.RemoveListener(AlFunctionExecutionListener);
				SendTraceTag("00001Y6", Verbosity.Warning, PrivacyClassification.SystemMetadata, "AlFunctionExecutionListener removed");
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[NonEvent]
	internal bool IsEnabled(EventKeywords keyword)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (!Keywords.Compare(keyword, (EventKeywords)1))
		{
			if (!IsDisabledFromController)
			{
				return ((EventSource)this).IsEnabled((EventLevel)0, keyword);
			}
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[NonEvent]
	internal bool IsEnabledALFunctionCallTracing()
	{
		if (!IsDisabledFromController)
		{
			return ((EventSource)this).IsEnabled((EventLevel)0, (EventKeywords)8);
		}
		return false;
	}

	[NonEvent]
	private void WriteEvent(EventKeywords keyword, int eventId, params object[] args)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (IsEnabled(keyword))
		{
			RunWriteEventMethodWithExceptionHandling(() =>
			{
				((EventSource)this).WriteEvent(eventId, args);
			});
		}
	}

	private static void SendTraceTag(string tag, Verbosity verbosity, PrivacyClassification classification, string message)
	{
		if (NavEnvironment.IsInstantiated)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag(tag, Category.Telemetry, verbosity, classification, message);
		}
	}

	private static void RunWriteEventMethodWithExceptionHandling(Action action)
	{
		//IL_0009: Expected Obj, but got Unknown
		try
		{
			action();
		}
		catch (EventSourceException ex)
		{
			EventSourceException exception = ex;
			if (ServerUserSettings.Instance.EnableEventSourceExceptionLogging.Value && !writeEventWithExceptionHandlingAlreadyCalledOnThisThead)
			{
				writeEventWithExceptionHandlingAlreadyCalledOnThisThead = true;
				try
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000114", Category.Operations, (Exception)(object)exception, Verbosity.Normal);
					return;
				}
				finally
				{
					writeEventWithExceptionHandlingAlreadyCalledOnThisThead = false;
				}
			}
		}
	}
}
