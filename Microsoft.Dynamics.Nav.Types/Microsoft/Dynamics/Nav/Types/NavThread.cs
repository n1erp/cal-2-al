using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Types;

public class NavThread : IDisposable
{
	private static readonly bool ThrottlingDisabled = !ServerUserSettings.Instance.EnableThreadThrottling.Value;

	[ThreadStatic]
	private static NavThread currentNavThread;

	private NavThreadScheduler scheduler;

	private Thread thread;

	private readonly object threadWaitObject = new object();

	private int threadStatus;

	private int threadPriority;

	private NavThread next;

	public Thread Thread => thread;

	internal int RequestToStopTick { get; set; }

	public NavThreadStatus ThreadStatus => (NavThreadStatus)threadStatus;

	public NavThreadPriority ThreadPriority => (NavThreadPriority)threadPriority;

	public NavThread Next
	{
		get
		{
			return next;
		}
		internal set
		{
			next = value;
		}
	}

	private NavThread()
	{
	}

	internal static NavThread CreateLinkListRoot(NavThreadPriority threadPriority)
	{
		NavThread navThread = new NavThread
		{
			threadStatus = 1,
			threadPriority = (int)threadPriority
		};
		navThread.next = navThread;
		return navThread;
	}

	public static NavThread AddCurrentThreadToScheduler(NavThreadScheduler scheduler, NavThreadPriority threadPriority = NavThreadPriority.Background)
	{
		if (ThrottlingDisabled)
		{
			return null;
		}
		if (scheduler == null)
		{
			throw new ArgumentNullException("scheduler");
		}
		NavThread navThread = (currentNavThread = new NavThread
		{
			scheduler = scheduler,
			thread = Thread.CurrentThread,
			threadStatus = 0,
			threadPriority = (int)threadPriority
		});
		scheduler.AddNewThread(navThread);
		navThread.SetThreadStatus(NavThreadStatus.Running);
		return navThread;
	}

	private void SetThreadStatus(NavThreadStatus newStatus)
	{
		NavThreadStatus navThreadStatus = (NavThreadStatus)Interlocked.Exchange(ref threadStatus, (int)newStatus);
		scheduler.ThreadChangedStatus((NavThreadPriority)threadPriority, navThreadStatus, newStatus);
		if (navThreadStatus != NavThreadStatus.WaitForExternal)
		{
			ThreadThrottle();
		}
	}

	private bool SetThreadStatusIfEqualTo(NavThreadStatus value, NavThreadStatus comparand)
	{
		bool flag = Interlocked.CompareExchange(ref threadStatus, (int)value, (int)comparand) == (int)comparand;
		if (flag)
		{
			scheduler.ThreadChangedStatus((NavThreadPriority)threadPriority, comparand, value);
			ThreadThrottle();
		}
		return flag;
	}

	internal void SetRequestToStopIfRunning(int currentTickCount)
	{
		if (SetThreadStatusIfEqualTo(NavThreadStatus.RequestToStop, NavThreadStatus.Running))
		{
			RequestToStopTick = currentTickCount;
		}
	}

	internal void SetRequestToStopNoResponseIfRequestToStop()
	{
		SetThreadStatusIfEqualTo(NavThreadStatus.RequestToStopNoResponse, NavThreadStatus.RequestToStop);
	}

	internal void SetRequestToStartIfStopped()
	{
		if (SetThreadStatusIfEqualTo(NavThreadStatus.RequestToStart, NavThreadStatus.Stopped))
		{
			lock (threadWaitObject)
			{
				Monitor.Pulse(threadWaitObject);
			}
		}
	}

	internal void SetSchedulerDisposed()
	{
		if (ThreadStatus == NavThreadStatus.Disposed)
		{
			return;
		}
		SetThreadStatus(NavThreadStatus.SchedulerDisposed);
		lock (threadWaitObject)
		{
			Monitor.Pulse(threadWaitObject);
		}
	}

	public static void RunExternalAction(Action action)
	{
		RunExternalAction(() =>
		{
			action();
			return 0;
		});
	}

	public static T RunExternalAction<T>(Func<T> action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		NavThread navThread = currentNavThread;
		if (navThread == null)
		{
			return action();
		}
		try
		{
			navThread.SetThreadStatus(NavThreadStatus.WaitForExternal);
			return action();
		}
		finally
		{
			navThread.SetThreadStatus(NavThreadStatus.Running);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Throttle()
	{
		if (!ThrottlingDisabled)
		{
			NavThread navThread = currentNavThread;
			if (navThread != null && navThread.ThreadStatus != NavThreadStatus.Disposed)
			{
				navThread.ThreadThrottle();
			}
		}
	}

	internal void ThreadThrottle()
	{
		if ((ThreadStatus != NavThreadStatus.RequestToStop && ThreadStatus != NavThreadStatus.RequestToStopNoResponse) || currentNavThread != this)
		{
			return;
		}
		lock (threadWaitObject)
		{
			while (SetThreadStatusIfEqualTo(NavThreadStatus.Stopped, NavThreadStatus.RequestToStop) || SetThreadStatusIfEqualTo(NavThreadStatus.Stopped, NavThreadStatus.RequestToStopNoResponse))
			{
				Monitor.Wait(threadWaitObject);
			}
		}
		SetThreadStatusIfEqualTo(NavThreadStatus.Running, NavThreadStatus.RequestToStart);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			SetThreadStatus(NavThreadStatus.Disposed);
		}
		thread = null;
		currentNavThread = null;
	}
}
