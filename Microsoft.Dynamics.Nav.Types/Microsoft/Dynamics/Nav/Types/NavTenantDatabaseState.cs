namespace Microsoft.Dynamics.Nav.Types;

public enum NavTenantDatabaseState
{
	Mounted,
	Dismounted,
	Failed,
	PendingExclusiveSync,
	PendingInstantSync,
	Operational,
	SyncInProgress,
	IncompatibleVersion,
	PendingSync,
	PendingExtensionSync
}
