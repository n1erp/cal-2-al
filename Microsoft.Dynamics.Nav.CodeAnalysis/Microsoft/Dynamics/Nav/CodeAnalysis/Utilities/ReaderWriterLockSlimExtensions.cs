using System;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class ReaderWriterLockSlimExtensions
{
	internal struct ReadLockExiter : IDisposable
	{
		private readonly ReaderWriterLockSlim lockSlim;

		internal ReadLockExiter(ReaderWriterLockSlim @lock)
		{
			lockSlim = @lock;
			@lock.EnterReadLock();
		}

		public void Dispose()
		{
			lockSlim.ExitReadLock();
		}
	}

	internal struct WriteLockExiter : IDisposable
	{
		private readonly ReaderWriterLockSlim lockSlim;

		internal WriteLockExiter(ReaderWriterLockSlim @lock)
		{
			lockSlim = @lock;
			@lock.EnterWriteLock();
		}

		public void Dispose()
		{
			lockSlim.ExitWriteLock();
		}
	}

	internal static ReadLockExiter DisposableRead(this ReaderWriterLockSlim @lock)
	{
		return new ReadLockExiter(@lock);
	}

	internal static WriteLockExiter DisposableWrite(this ReaderWriterLockSlim @lock)
	{
		return new WriteLockExiter(@lock);
	}

	internal static void AssertCanRead(this ReaderWriterLockSlim @lock)
	{
		if (!@lock.IsReadLockHeld && !@lock.IsUpgradeableReadLockHeld && !@lock.IsWriteLockHeld)
		{
			throw new InvalidOperationException();
		}
	}

	internal static void AssertCanWrite(this ReaderWriterLockSlim @lock)
	{
		if (!@lock.IsWriteLockHeld)
		{
			throw new InvalidOperationException();
		}
	}
}
