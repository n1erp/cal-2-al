namespace Microsoft.Dynamics.Nav.Types;

public enum SyncNavAppStatus
{
	InProgress,
	AppSyncSuccessful,
	AppAlreadyChangedWarning,
	AppStillInstalledError,
	AppNotPublished,
	AmbiguousApp,
	MustBeTenantDatabase,
	ApplicationNotMountedError,
	TenantNotSyncedError,
	UnknownAppError,
	HigherVersionSyncedError,
	SyncNotSupported,
	SyncValidationFailed
}
