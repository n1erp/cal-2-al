using System;
using System.Globalization;
using System.Threading;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class NonReEntrantTimerWrapper : IDisposable
{
	internal sealed class InvalidWaitHandle : WaitHandle
	{
		private static readonly InvalidWaitHandle instance = new InvalidWaitHandle();

		public static WaitHandle Instance => instance;

		public override IntPtr Handle
		{
			get
			{
				return WaitHandle.InvalidHandle;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		private InvalidWaitHandle()
		{
		}
	}

	private TimerCallback timerDelegate;

	private Timer timer;

	private int dueTime;

	private string timerDescription;

	private readonly object threadLock = new object();

	internal NonReEntrantTimerWrapper(string timerName, TimerCallback timerDelegate, object state, int dueTime, bool startImmediately = false)
	{
		this.timerDelegate = timerDelegate;
		this.dueTime = dueTime;
		int num = ((!startImmediately) ? dueTime : 0);
		timerDescription = string.Format(CultureInfo.InvariantCulture, "{0}: {1} (due time: {2})", "NonReEntrantTimerWrapper", timerName, dueTime);
		timer = new Timer(TimerDelegateWrapper, state, -1, -1);
		timer.Change(num, -1);
	}

	private void TimerDelegateWrapper(object state)
	{
		if (!Monitor.TryEnter(threadLock))
		{
			return;
		}
		try
		{
			if (timerDelegate == null)
			{
				return;
			}
			using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00001R3", DiagnosticParameter.SystemMetadata(timerDescription), MonitorType.None, Category.Operations, createNewActivity: true, Verbosity.Verbose))
			{
				timerDelegate(state);
			}
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000004I", Category.Runtime, exception, Verbosity.Error, DiagnosticParameter.SystemMetadata(timerDescription));
		}
		finally
		{
			if (timerDelegate != null)
			{
				timer?.Change(dueTime, -1);
			}
			Monitor.Exit(threadLock);
		}
	}

	internal void Change(int newDueTime)
	{
		dueTime = newDueTime;
		timer?.Change(dueTime, -1);
	}

	internal void FireNow()
	{
		timer?.Change(0, -1);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		lock (threadLock)
		{
			if (this.timer != null)
			{
				Timer timer = this.timer;
				this.timer = null;
				timerDelegate = null;
				timer.Dispose(InvalidWaitHandle.Instance);
			}
		}
	}
}
