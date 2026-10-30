using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Types;

public class NavThreadScheduler : IDisposable
{
	private readonly bool PrioritizedSchedulingEnabled;

	private readonly int maxRunningThreads;

	private const int ThrottleSliceLength = 50;

	private const int RequestToStopWaitTime = 75;

	private readonly object linkedListlockObject = new object();

	private readonly object schedulerWaitObject = new object();

	private Thread schedulerThread;

	private int rescheduleThreadsLastTickCount;

	private bool stopRequested;

	private readonly NavThread linkStart;

	private readonly NavThread uILinkStart;

	private NavThread lastStarted;

	private NavThread uILastStarted;

	private NavThread lastStopped;

	private NavThread uILastStopped;

	private readonly int[][] threadCount;

	private bool isDisposed;

	private int CombinedRunningOrUnblockedThreadCount => threadCount[1][3] + threadCount[0][3] + threadCount[1][2] + threadCount[0][2] + threadCount[1][4] + threadCount[0][4];

	private int CombinedRunningThreadCount => threadCount[1][3] + threadCount[0][3];

	private int CombinedRequestToStopThreadCount => threadCount[1][4] + threadCount[0][4];

	private int CombinedStoppedThreadCount => threadCount[1][5] + threadCount[0][5];

	private int CombinedDisposedThreadCount => threadCount[1][7] + threadCount[0][7];

	private event EventHandler OnIdle;

	public NavThreadScheduler(int maxRunningThreads = -1, bool forcePrioritizedScheduling = false)
	{
		if (ServerUserSettings.Instance.EnableThreadThrottling.Value)
		{
			schedulerThread = new Thread(RunScheduler)
			{
				Priority = ThreadPriority.Highest,
				Name = "NavThreadScheduler"
			};
			this.maxRunningThreads = ((maxRunningThreads > 0) ? maxRunningThreads : Math.Max(1, Environment.ProcessorCount - 1));
			PrioritizedSchedulingEnabled = forcePrioritizedScheduling || (ServerUserSettings.Instance.EnablePrioritizedThreadThrottling.Value && this.maxRunningThreads > 2);
			linkStart = NavThread.CreateLinkListRoot(NavThreadPriority.Background);
			lastStarted = (lastStopped = linkStart);
			uILinkStart = NavThread.CreateLinkListRoot(NavThreadPriority.UI);
			uILastStarted = (uILastStopped = uILinkStart);
			int num = Enum.GetValues(typeof(NavThreadPriority)).Cast<int>().Max() + 1;
			threadCount = new int[num][];
			for (int i = 0; i < num; i++)
			{
				threadCount[i] = new int[Enum.GetValues(typeof(NavThreadStatus)).Cast<int>().Max() + 1];
			}
			schedulerThread.Start();
		}
	}

	internal void AddNewThread(NavThread thread)
	{
		if (isDisposed)
		{
			return;
		}
		if (thread == null)
		{
			throw new ArgumentNullException("thread");
		}
		if (thread.Next != null)
		{
			throw new InvalidOperationException();
		}
		lock (linkedListlockObject)
		{
			if (PrioritizedSchedulingEnabled && thread.ThreadPriority == NavThreadPriority.Background)
			{
				thread.Next = lastStarted.Next;
				lastStarted.Next = thread;
				lastStarted = thread;
			}
			else
			{
				thread.Next = uILastStarted.Next;
				uILastStarted.Next = thread;
				uILastStarted = thread;
			}
		}
	}

	internal void ThreadChangedStatus(NavThreadPriority priority, NavThreadStatus oldStatus, NavThreadStatus newStatus)
	{
		if (oldStatus != newStatus)
		{
			AdjustThreadCount(priority, oldStatus, -1);
			AdjustThreadCount(priority, newStatus, 1);
			if (oldStatus != NavThreadStatus.WaitForExternal && (newStatus == NavThreadStatus.Running || newStatus == NavThreadStatus.Stopped || newStatus == NavThreadStatus.Disposed))
			{
				InterruptSchedulerWait();
			}
		}
	}

	internal void RunActionOnceOnIdle(Action action)
	{
		EventHandler handler = null;
		handler = (object sender, EventArgs e) =>
		{
			action();
			OnIdle -= handler;
		};
		OnIdle += handler;
	}

	private void InterruptSchedulerWait()
	{
		if ((schedulerThread.ThreadState & ThreadState.WaitSleepJoin) == 0)
		{
			return;
		}
		bool lockTaken = false;
		try
		{
			Monitor.TryEnter(schedulerWaitObject, ref lockTaken);
			if (lockTaken)
			{
				Monitor.Pulse(schedulerWaitObject);
			}
		}
		finally
		{
			if (lockTaken)
			{
				Monitor.Exit(schedulerWaitObject);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int AdjustThreadCount(NavThreadPriority priority, NavThreadStatus status, int delta)
	{
		return Interlocked.Add(ref threadCount[(int)priority][(int)status], delta);
	}

	private int RequestToStartThreadCount(NavThreadPriority priority)
	{
		return threadCount[(int)priority][2];
	}

	private int RunningOrUnblockedThreadCount(NavThreadPriority priority)
	{
		return threadCount[(int)priority][3] + threadCount[(int)priority][2] + threadCount[(int)priority][4];
	}

	private int RunningThreadCount(NavThreadPriority priority)
	{
		return threadCount[(int)priority][3];
	}

	private int RequestToStopThreadCount(NavThreadPriority priority)
	{
		return threadCount[(int)priority][4];
	}

	private int StoppedThreadCount(NavThreadPriority priority)
	{
		return threadCount[(int)priority][5];
	}

	private int DisposedThreadCount(NavThreadPriority priority)
	{
		return threadCount[(int)priority][7];
	}

	private void RunScheduler()
	{
		while (!stopRequested)
		{
			RescheduleThreads();
			lock (schedulerWaitObject)
			{
				Monitor.Wait(schedulerWaitObject, 50);
			}
			if (CombinedRunningOrUnblockedThreadCount != 0 || OnIdle == null)
			{
				continue;
			}
			lock (linkedListlockObject)
			{
				if (CombinedRunningOrUnblockedThreadCount == 0)
				{
					OnIdle?.Invoke(null, null);
				}
			}
		}
		ReleaseAllThreadsForDisposal();
	}

	private void ReleaseAllThreadsForDisposal()
	{
		lock (linkedListlockObject)
		{
			foreach (NavThread item in Threads())
			{
				if (item != linkStart && item != uILinkStart)
				{
					item.SetSchedulerDisposed();
				}
			}
		}
	}

	private void RescheduleThreads()
	{
		int tickCount = Environment.TickCount;
		int num = tickCount - rescheduleThreadsLastTickCount;
		bool flag = num >= 50 || num < 0;
		int maxAllowedThreadsToContinueUI;
		int num2 = (maxAllowedThreadsToContinueUI = maxRunningThreads);
		if (flag)
		{
			if (CombinedRequestToStopThreadCount > 0)
			{
				lock (linkedListlockObject)
				{
					foreach (NavThread item in from thread in Threads()
						where thread.ThreadStatus == NavThreadStatus.RequestToStop
						select thread)
					{
						long num3 = tickCount - item.RequestToStopTick;
						if (num3 < 0 || num3 > 75)
						{
							item.SetRequestToStopNoResponseIfRequestToStop();
						}
					}
				}
			}
			rescheduleThreadsLastTickCount = tickCount;
			num2 -= Math.Min(CombinedStoppedThreadCount, 1);
		}
		StopRunningThreadsIfNecessary(tickCount, flag, num2, maxAllowedThreadsToContinueUI);
		StartThreadsIfPermissible();
		if (flag && CombinedDisposedThreadCount > 50)
		{
			RemoveDisposed();
		}
	}

	private void StopRunningThreadsIfNecessary(int currentTickCount, bool fullSliceRound, int maxAllowedThreadsToContinue, int maxAllowedThreadsToContinueUI)
	{
		int num = CombinedRunningOrUnblockedThreadCount - maxAllowedThreadsToContinue;
		int num2 = 0;
		bool flag = false;
		while (num > 0 && CombinedRunningThreadCount > 0)
		{
			NavThread navThread = null;
			lock (linkedListlockObject)
			{
				if (!flag)
				{
					navThread = FindThreadWithStatus(NavThreadStatus.Running, lastStopped.Next);
					if (navThread != null)
					{
						lastStopped = navThread;
						num--;
					}
					else
					{
						flag = true;
						if (fullSliceRound)
						{
							maxAllowedThreadsToContinueUI -= Math.Min(StoppedThreadCount(NavThreadPriority.UI), 1);
						}
						num2 = RunningOrUnblockedThreadCount(NavThreadPriority.UI) - maxAllowedThreadsToContinueUI;
					}
				}
				if (flag && num2 > 0 && RunningThreadCount(NavThreadPriority.UI) > 0)
				{
					navThread = FindThreadWithStatus(NavThreadStatus.Running, uILastStopped.Next);
					if (navThread != null)
					{
						uILastStopped = navThread;
						num2--;
					}
				}
			}
			if (navThread != null)
			{
				navThread.SetRequestToStopIfRunning(currentTickCount);
				continue;
			}
			break;
		}
	}

	private void StartThreadsIfPermissible()
	{
		bool flag = false;
		while (CombinedRunningOrUnblockedThreadCount < maxRunningThreads && CombinedStoppedThreadCount > 0)
		{
			NavThread navThread;
			lock (linkedListlockObject)
			{
				navThread = (flag ? FindThreadWithStatus(NavThreadStatus.Stopped, lastStarted.Next) : FindThreadWithStatus(NavThreadStatus.Stopped, uILastStarted.Next));
				if (navThread == null)
				{
					flag = true;
				}
				else if (flag)
				{
					lastStarted = navThread;
				}
				else
				{
					uILastStarted = navThread;
				}
			}
			navThread?.SetRequestToStartIfStopped();
		}
	}

	private void RemoveDisposed()
	{
		int num = 0;
		int num2 = 0;
		lock (linkedListlockObject)
		{
			NavThread next = linkStart;
			while (true)
			{
				if (next.Next.ThreadStatus == NavThreadStatus.Disposed)
				{
					if (lastStarted == next.Next)
					{
						lastStarted = lastStarted.Next;
					}
					if (lastStopped == next.Next)
					{
						lastStopped = lastStopped.Next;
					}
					next.Next = next.Next.Next;
					num++;
				}
				else
				{
					next = next.Next;
					if (next == linkStart)
					{
						break;
					}
				}
			}
			next = uILinkStart;
			while (true)
			{
				if (next.Next.ThreadStatus == NavThreadStatus.Disposed)
				{
					if (uILastStarted == next.Next)
					{
						uILastStarted = uILastStarted.Next;
					}
					if (uILastStopped == next.Next)
					{
						uILastStopped = uILastStopped.Next;
					}
					next.Next = next.Next.Next;
					num2++;
				}
				else
				{
					next = next.Next;
					if (next == uILinkStart)
					{
						break;
					}
				}
			}
		}
		AdjustThreadCount(NavThreadPriority.Background, NavThreadStatus.Disposed, -num);
		AdjustThreadCount(NavThreadPriority.UI, NavThreadStatus.Disposed, -num2);
	}

	private IEnumerable<NavThread> Threads()
	{
		NavThread start = linkStart;
		NavThread result = linkStart;
		do
		{
			yield return result;
			result = result.Next;
		}
		while (result != start);
		start = uILinkStart;
		result = uILinkStart;
		do
		{
			yield return result;
			result = result.Next;
		}
		while (result != start);
	}

	private NavThread FindThreadWithStatus(NavThreadStatus status, NavThread start = null)
	{
		lock (linkedListlockObject)
		{
			if (start == null)
			{
				start = linkStart;
			}
			NavThread navThread = start;
			do
			{
				if (navThread.ThreadStatus == status)
				{
					return navThread;
				}
				navThread = navThread.Next;
			}
			while (navThread != start);
		}
		return null;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		isDisposed = true;
		if (disposing && schedulerThread != null)
		{
			stopRequested = true;
			lock (schedulerWaitObject)
			{
				Monitor.Pulse(schedulerWaitObject);
			}
			schedulerThread.Join();
			schedulerThread = null;
		}
	}
}
