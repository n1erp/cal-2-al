namespace Microsoft.Dynamics.Nav.Types;

public enum NavDataUpgradeMethodExecutionState
{
	Pending,
	InProgress,
	Completed,
	FailedPendingResume,
	Failed,
	CanceledPendingResume,
	Canceled,
	NotExecuted
}
