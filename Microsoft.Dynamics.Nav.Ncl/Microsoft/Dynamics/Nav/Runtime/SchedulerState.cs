namespace Microsoft.Dynamics.Nav.Runtime;

internal enum SchedulerState
{
	NotStarted,
	Started,
	StartedAndFirstScheduledHasCompletedSuccessfully,
	Stopping,
	Stopped
}
