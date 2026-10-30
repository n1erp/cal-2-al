namespace Microsoft.Dynamics.Nav.Types;

public static class NavThreadInfo
{
	public const int ThrottleSliceLength = 50;

	public const int RequestToStopWaitTime = 75;

	public const int ThreadsToStopAtSliceEnd = 1;

	public const int MaxDisposedThreads = 50;
}
