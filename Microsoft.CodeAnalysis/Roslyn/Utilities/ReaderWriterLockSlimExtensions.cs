using System;
using System.Threading;

namespace Roslyn.Utilities;

internal static class ReaderWriterLockSlimExtensions
{
	internal struct ReadLockExiter : IDisposable
	{
		private readonly ReaderWriterLockSlim _lock;

		internal ReadLockExiter(ReaderWriterLockSlim @lock)
		{
			_lock = @lock;
			@lock.EnterReadLock();
		}

		public void Dispose()
		{
			_lock.ExitReadLock();
		}
	}

	internal struct WriteLockExiter : IDisposable
	{
		private readonly ReaderWriterLockSlim _lock;

		internal WriteLockExiter(ReaderWriterLockSlim @lock)
		{
			_lock = @lock;
			@lock.EnterWriteLock();
		}

		public void Dispose()
		{
			_lock.ExitWriteLock();
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
