namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavServerEventViewerEvents
{
	void WriteScomFatalSqlErrorEvent(string serverInstanceName, string tenantId, string message);

	void WriteScomSqlConnectionEstablishedEvent(string serverInstanceName, string tenantId, string message);

	void WriteSubsystemErrorMessageEvent(string serverInstanceName, string navTenantId, string message);

	void WriteSubsystemWarningMessageEvent(string serverInstanceName, string navTenantId, string message);

	void WriteSubsystemInfoMessageEvent(string serverInstanceName, string navTenantId, string message);

	void WriteExceptionErrorMessageEvent(string serverInstanceName, string navTenantId, string message);

	void WriteExceptionWarningMessageEvent(string serverInstanceName, string navTenantId, string message);

	void WriteExceptionInfoMessageEvent(string serverInstanceName, string navTenantId, string message);

	void WriteODataConnectedEvent(string serverInstanceName, string message);

	void WriteODataConnectionClosedEvent(string serverInstanceName, string message);

	void WriteRtcConnectedEvent(string serverInstanceName, string message);

	void WriteRtcConnectionClosedEvent(string serverInstanceName, string message);

	void WriteWebServiceConnectedEvent(string serverInstanceName, string message);

	void WriteWebServiceConnectionClosedEvent(string serverInstanceName, string message);

	void WriteAdminServiceConnectedEvent(string serverInstanceName, string message);

	void WriteAdminServiceConnectionClosedEvent(string serverInstanceName, string message);

	void WriteNasStartingEvent(string serverInstanceName, string navTenantId, string message);

	void WriteNasCompletedEvent(string serverInstanceName, string navTenantId, string message);

	void WriteNasFailedEvent(string serverInstanceName, string navTenantId, string message);

	void WriteNasPermanentlyFailedEvent(string serverInstanceName, string navTenantId, string message);

	void WriteALInfoMessageEvent(string serverInstanceName, string navTenantId, string message);

	void WriteNavServerInitializingEvent(string serverInstanceName, string message);

	void WriteNavServerReadyEvent(string serverInstanceName, string message);

	void WriteNavServerShuttingDownEvent(string serverInstanceName, string message);

	void WriteApplicationMountedEvent(string serverInstanceName, string message);

	void WriteApplicationMountFailedEvent(string serverInstanceName, string message);

	void WriteTenantConfigLoadedEvent(string serverInstanceName, string navTenantId, string message);

	void WriteTenantMountedEvent(string serverInstanceName, string navTenantId, string message);

	void WriteTenantMountFailedEvent(string serverInstanceName, string navTenantId, string message);

	void WriteTenantDismountedEvent(string serverInstanceName, string navTenantId, string message);

	void WriteAuthenticationSubsystemFailedEvent(string serverInstanceName, string navTenantId, string message);

	void WriteAuthenticationSubsystemInfoMessageEvent(string serverInstanceName, string navTenantId, string message);

	void LimitedUserOptionalTablePermissionErrorInfoMessage(string serverInstanceName, string navTenantId, string message);

	void WriteCertificateMonitorPermanentlyFailedEvent(string serverInstanceName, string navTenantId, string message);

	void WriteCertificateReplacedInformationEvent(string serverInstanceName, string navTenantId, string message);

	void WriteCertificateExpiredErrorEvent(string serverInstanceName, string navTenantId, string message);

	void WriteCertificateCloseToExpirationWarningEvent(string serverInstanceName, string navTenantId, string message);

	void WriteCertificateNewInformationEvent(string serverInstanceName, string navTenantId, string message);
}
