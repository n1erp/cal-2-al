using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal sealed class NonReentrantLock
{
	[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	internal struct SemaphoreDisposer(NonReentrantLock semaphore) : IDisposable
	{
		private readonly NonReentrantLock semaphore = semaphore;

		public void Dispose()
		{
			semaphore.Release();
		}
	}

	private static readonly Action<object> cancellationTokenCanceledEventHandler = CancellationTokenCanceledEventHandler;

	private readonly object syncLock;

	private volatile int owningThreadId;

	[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
	public static readonly Func<NonReentrantLock> Factory = () => new NonReentrantLock(useThisInstanceForSynchronization: true);

	private bool IsLocked => owningThreadId != 0;

	private bool IsOwnedByMe => owningThreadId == Environment.CurrentManagedThreadId;

	public NonReentrantLock(bool useThisInstanceForSynchronization = false)
	{
		syncLock = (useThisInstanceForSynchronization ? this : new object());
	}

	public void Wait(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (IsOwnedByMe)
		{
			throw new LockRecursionException();
		}
		CancellationTokenRegistration cancellationTokenRegistration = default;
		if (cancellationToken.CanBeCanceled)
		{
			cancellationToken.ThrowIfCancellationRequested();
			lock (syncLock)
			{
				if (!IsLocked)
				{
					TakeOwnership();
					return;
				}
			}
			cancellationTokenRegistration = cancellationToken.Register(cancellationTokenCanceledEventHandler, syncLock, useSynchronizationContext: false);
		}
		using (cancellationTokenRegistration)
		{
			SpinWait spinWait = default;
			while (IsLocked && !spinWait.NextSpinWillYield)
			{
				spinWait.SpinOnce();
			}
			lock (syncLock)
			{
				while (IsLocked)
				{
					cancellationToken.ThrowIfCancellationRequested();
					Monitor.Wait(syncLock);
				}
				TakeOwnership();
			}
		}
	}

	public void Release()
	{
		AssertHasLock();
		lock (syncLock)
		{
			ReleaseOwnership();
			Monitor.Pulse(syncLock);
		}
	}

	public bool LockHeldByMe()
	{
		return IsOwnedByMe;
	}

	public void AssertHasLock()
	{
		if (!LockHeldByMe())
		{
			throw new InvalidOperationException("Lock is not held by me!");
		}
	}

	private void TakeOwnership()
	{
		owningThreadId = Environment.CurrentManagedThreadId;
	}

	private void ReleaseOwnership()
	{
		owningThreadId = 0;
	}

	private static void CancellationTokenCanceledEventHandler(object obj)
	{
		lock (obj)
		{
			Monitor.PulseAll(obj);
		}
	}

	internal SemaphoreDisposer DisposableWait(CancellationToken cancellationToken = default(CancellationToken))
	{
		Wait(cancellationToken);
		return new SemaphoreDisposer(this);
	}
}
