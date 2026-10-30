using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavServerTracingEvents
{
	void WriteSqlExecuteScalarStartEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName);

	void WriteSqlExecuteScalarStopEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName);

	void WriteSqlExecuteNonQueryStartEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName);

	void WriteSqlExecuteNonQueryStopEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName);

	void WriteSqlExecuteReaderStartEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName);

	void WriteSqlExecuteReaderStopEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName);

	void WriteSqlReadNextResultStartEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName);

	void WriteSqlReadNextResultStopEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName);

	void WriteSqlReadNextRowStartEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName);

	void WriteSqlReadNextRowStopEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName);

	void WriteSqlBeginTransactionStartEvent(string tenantId, int sessionId, string userName, string serverInstanceName);

	void WriteSqlBeginTransactionStopEvent(string tenantId, int sessionId, string userName, string serverInstanceName);

	void WriteSqlPrepareStartEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName);

	void WriteSqlPrepareStopEvent(string tenantId, int sessionId, string userName, string sqlStatement, string serverInstanceName);

	void WriteSqlOpenConnectionStartEvent(string tenantId, int sessionId, string userName, string serverInstanceName);

	void WriteSqlOpenConnectionStopEvent(string tenantId, int sessionId, string userName, string serverInstanceName);

	void WriteSqlCommitStartEvent(string tenantId, int sessionId, string userName, string serverInstanceName);

	void WriteSqlCommitStopEvent(string tenantId, int sessionId, string userName, string serverInstanceName);

	void WriteSqlRollbackStartEvent(string tenantId, int sessionId, string userName, string serverInstanceName);

	void WriteSqlRollbackStopEvent(string tenantId, int sessionId, string userName, string serverInstanceName);

	void WriteRunQueryStartEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName);

	void WriteRunQueryStopEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName);

	void WriteRunReportStartEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName);

	void WriteRunReportStopEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName);

	void WriteGetPageStartEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName);

	void WriteGetPageStopEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName);

	void WriteRunXmlPortStartEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName);

	void WriteRunXmlPortStopEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName);

	void WriteInvokeCodeUnitStartEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName);

	void WriteInvokeCodeUnitStopEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName);

	void WriteInvokeFormApplicationMethodStartEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName);

	void WriteInvokeFormApplicationMethodStopEvent(string tenantId, int sessionId, string userName, int objectId, string serverInstanceName);

	void WriteCreateServiceSessionStartEvent(string tenantId, int sessionId, string userName, string connectionType, string serverInstanceName);

	void WriteCreateServiceSessionStopEvent(string tenantId, int sessionId, string userName, string connectionType, string serverInstanceName);

	void WriteEndServiceSessionStartEvent(string tenantId, int sessionId, string userName, string connectionType, string serverInstanceName);

	void WriteEndServiceSessionStopEvent(string tenantId, int sessionId, string userName, string connectionType, string serverInstanceName);

	void WriteALFunctionStartEvent(string tenantId, int sessionId, string objectType, int objectId, string objectExtensionAppId, string objectExtensionInfo, string functionName, string serverInstanceName, string platformVersion, string aadTenantId, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId);

	void WriteALFunctionStopEvent(string tenantId, int sessionId, string objectType, int objectId, string objectExtensionAppId, string objectExtensionInfo, string functionName, string serverInstanceName, string platformVersion, string aadTenantId, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId, long executionMilliseconds);

	void WriteALFunctionStatementEvent(string tenantId, int sessionId, string objectType, int objectId, string objectExtensionAppId, string objectExtensionInfo, string functionName, int lineNumber, string statement, string serverInstanceName, string platformVersion, string aadTenantId, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId);

	void WriteALFunctionFailedEvent(string tenantId, int sessionId, string userName, string objectType, int objectId, string objectExtensionAppId, string objectExtensionInfo, string functionName, string failureMessage, string serverInstanceName, string platformVersion, string aadTenantId, string aadUserId, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId);

	void WriteALFunctionFailedEventNoPersonalData(string tenantId, int sessionId, string objectType, int objectId, string objectExtensionAppId, string objectExtensionInfo, string functionName, string failureMessage, string serverInstanceName, string platformVersion, string aadTenantId, Guid clientSessionId, Guid clientActivityId, Guid serverSessionId, Guid serverActivityId);

	void WriteSessionOpenedEvent(string tenantId, int sessionId, string userName, string connectionType, string serverInstanceName);

	void WriteSessionClosedEvent(string tenantId, int sessionId, string userName, string connectionType, string serverInstanceName);

	void WritePermissionCheckStartEvent(string serverInstanceName, int sessionId, int objectType, int objectId, int permissions, int callingObjectType, int callingObjectId, int queryCategory);

	void WritePermissionCheckSuccessfulEvent(string serverInstanceName, int sessionId, int objectType, int objectId, int permissions, int callingObjectType, int callingObjectId, int indirectPermissionFromCaller, int queryCategory);

	void WritePermissionCheckUnsuccessfulEvent(string serverInstanceName, int sessionId, int objectType, int objectId, int permissions, int callingObjectType, int callingObjectId, int indirectPermissionFromCaller, int queryCategory);

	void WritePermissionUncheckedEvent(string serverInstanceName, int sessionId, int objectType, int objectId, int permissions, int callingObjectType, int callingObjectId);

	void WriteRecorderSessionError(string serverInstanceName, string tenantId, string recorder, int expectedSessionId, int actualSessionId);
}
