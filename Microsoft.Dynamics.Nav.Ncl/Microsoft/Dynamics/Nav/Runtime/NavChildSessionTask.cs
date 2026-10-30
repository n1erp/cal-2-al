using System;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavChildSessionTask : IDisposable
{
	private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

	private bool isDisposed;

	public virtual bool RunInReadOnlySession { get; } = true;

	public TimeSpan Timeout { get; set; }

	public NavChildSessionTaskStatus Status { get; set; }

	public CancellationTokenSource CancellationTokenSource => cancellationTokenSource;

	public abstract bool BeforeRunTask();

	public abstract void RunTaskInChildSession();

	public abstract void AfterRunTask();

	public abstract void AfterRunTaskError(NavChildSessionTaskError childSessionTaskError);

	protected virtual void Dispose(bool disposing)
	{
		if (!isDisposed)
		{
			if (disposing)
			{
				cancellationTokenSource.Dispose();
			}
			isDisposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
