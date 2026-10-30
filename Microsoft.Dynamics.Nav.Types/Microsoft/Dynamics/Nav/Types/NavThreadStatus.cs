namespace Microsoft.Dynamics.Nav.Types;

public enum NavThreadStatus
{
	Unknown,
	LinkStart,
	RequestToStart,
	Running,
	RequestToStop,
	Stopped,
	WaitForExternal,
	Disposed,
	SchedulerDisposed,
	RequestToStopNoResponse
}
