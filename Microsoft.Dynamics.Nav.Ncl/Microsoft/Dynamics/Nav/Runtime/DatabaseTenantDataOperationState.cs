namespace Microsoft.Dynamics.Nav.Runtime;

public enum DatabaseTenantDataOperationState
{
	None,
	CopyInProgress,
	CopyFailed,
	MoveInProgress,
	RemoveInProgress,
	RemoveFailed
}
