using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class PeriodicScheduler
{
	private readonly object schedulerLockObject = new object();

	private SchedulerState schedulerState;

	private readonly int period;

	private Func<bool> scheduledAction;

	public Func<bool> ScheduledAction
	{
		get
		{
			return scheduledAction;
		}
		set
		{
			scheduledAction = value;
		}
	}

	public SchedulerState State => schedulerState;

	protected PeriodicScheduler(int period)
	{
		this.period = period;
	}

	public void Start()
	{
		lock (schedulerLockObject)
		{
			OnStart();
			ChangeState(SchedulerState.Started);
		}
	}

	private void ChangeState(SchedulerState newState)
	{
		lock (schedulerLockObject)
		{
			schedulerState = newState;
			Monitor.PulseAll(schedulerLockObject);
		}
	}

	protected abstract void OnStart();

	public void Stop()
	{
		lock (schedulerLockObject)
		{
			switch (schedulerState)
			{
			case SchedulerState.NotStarted:
			case SchedulerState.Stopped:
				return;
			case SchedulerState.Started:
			case SchedulerState.StartedAndFirstScheduledHasCompletedSuccessfully:
				ChangeState(SchedulerState.Stopping);
				break;
			}
			OnStop(schedulerLockObject);
		}
	}

	protected abstract void OnStop(object syncObject);

	protected bool RunAction()
	{
		lock (schedulerLockObject)
		{
			switch (schedulerState)
			{
			case SchedulerState.NotStarted:
			case SchedulerState.Stopped:
				return false;
			case SchedulerState.Stopping:
				ChangeState(SchedulerState.Stopped);
				return false;
			case SchedulerState.Started:
			case SchedulerState.StartedAndFirstScheduledHasCompletedSuccessfully:
				break;
			}
		}
		try
		{
			if (scheduledAction() && schedulerState == SchedulerState.Started)
			{
				lock (schedulerLockObject)
				{
					if (schedulerState == SchedulerState.Started)
					{
						ChangeState(SchedulerState.StartedAndFirstScheduledHasCompletedSuccessfully);
					}
				}
			}
		}
		catch (Exception exception)
		{
			ChangeState(SchedulerState.Stopped);
			LogWriterHelper.LogExceptionEvent(exception, EventLogEntryType.Error);
			throw;
		}
		lock (schedulerLockObject)
		{
			Wait(schedulerLockObject, period);
		}
		return true;
	}

	public void EnsureFirstSchedule()
	{
		if (schedulerState == SchedulerState.StartedAndFirstScheduledHasCompletedSuccessfully)
		{
			return;
		}
		lock (schedulerLockObject)
		{
			while (true)
			{
				switch (schedulerState)
				{
				case SchedulerState.NotStarted:
				case SchedulerState.Started:
					Wait(schedulerLockObject);
					break;
				case SchedulerState.StartedAndFirstScheduledHasCompletedSuccessfully:
				case SchedulerState.Stopping:
				case SchedulerState.Stopped:
					return;
				}
			}
		}
	}

	protected abstract void Wait(object syncObject);

	protected abstract void Wait(object syncObject, int periodToWait);
}
