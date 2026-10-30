using System;
using System.Threading;
using System.Threading.Tasks;

namespace Roslyn.Utilities;

internal static class SemaphoreExtensions
{
	internal struct SemaphoreDisposer(Semaphore semaphore) : IDisposable
	{
		private readonly Semaphore _semaphore = semaphore;

		public void Dispose()
		{
			_semaphore.Release();
		}
	}

	public static SemaphoreDisposer DisposableWait(this Semaphore semaphore, CancellationToken cancellationToken)
	{
		if (cancellationToken.CanBeCanceled)
		{
			if (WaitHandle.WaitAny(new WaitHandle[2] { semaphore, cancellationToken.WaitHandle }) != 0)
			{
				cancellationToken.ThrowIfCancellationRequested();
				throw ExceptionUtilities.Unreachable;
			}
		}
		else
		{
			semaphore.WaitOne();
		}
		return new SemaphoreDisposer(semaphore);
	}

	public static Task<SemaphoreDisposer> DisposableWaitAsync(this Semaphore semaphore, CancellationToken cancellationToken)
	{
		return Task.Factory.StartNew(() => semaphore.DisposableWait(cancellationToken), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
	}
}
