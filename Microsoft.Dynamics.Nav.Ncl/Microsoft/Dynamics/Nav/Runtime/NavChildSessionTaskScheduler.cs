using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavChildSessionTaskScheduler : IDisposable
{
	private bool isDisposed;

	public void ScheduleChildSessionTaskRun(NavChildSessionTaskRuntime taskRuntime)
	{
		throw new NotImplementedException();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!isDisposed)
		{
			isDisposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
