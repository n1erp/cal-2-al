using System;
using System.Threading;
using System.Threading.Tasks;

namespace Roslyn.Utilities;

internal static class SemaphoreSlimExtensions
{
	internal struct SemaphoreDisposer(SemaphoreSlim semaphore) : IDisposable
	{
		private readonly SemaphoreSlim _semaphore = semaphore;

		public void Dispose()
		{
			_semaphore.Release();
		}
	}

	public static SemaphoreDisposer DisposableWait(this SemaphoreSlim semaphore, CancellationToken cancellationToken = default(CancellationToken))
	{
		semaphore.Wait(cancellationToken);
		return new SemaphoreDisposer(semaphore);
	}

	public static async Task<SemaphoreDisposer> DisposableWaitAsync(this SemaphoreSlim semaphore, CancellationToken cancellationToken = default(CancellationToken))
	{
		await semaphore.WaitAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return new SemaphoreDisposer(semaphore);
	}
}
