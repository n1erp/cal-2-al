namespace Microsoft.Dynamics.Nav.Runtime;

public enum TaskRunState
{
	Created,
	Initializing,
	Running,
	FailedRetryNotSet,
	FailedRetry,
	Failed,
	CompletedDoneNotSet,
	Completed
}
