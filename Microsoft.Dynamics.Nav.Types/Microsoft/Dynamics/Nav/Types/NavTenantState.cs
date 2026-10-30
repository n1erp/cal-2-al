namespace Microsoft.Dynamics.Nav.Types;

public enum NavTenantState
{
	Mounted,
	Dismounted,
	Failed,
	OperationalWithSyncPending,
	OperationalWithSyncFailure,
	OperationalSyncInProgress,
	Operational,
	OperationalDataUpgradePending,
	OperationalDataUpgradeInProgress,
	OperationalWithDataUpgradeFailure,
	OperationalWithMissingApplicationVersionFailure,
	Mounting,
	OperationalTenantDataCopyInProgress,
	OperationalTenantDataCopyFailed,
	OperationalTenantDataMoveInProgress,
	OperationalTenantRemoveInProgress,
	OperationalTenantRemoveFailed
}
