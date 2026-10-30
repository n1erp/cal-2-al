using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class ThreadPeriodicScheduler : PeriodicScheduler
{
	private readonly Thread schdulerThread;

	public ThreadPeriodicScheduler(string name, int period, ThreadPriority priority = ThreadPriority.Normal)
		: base(period)
	{
		schdulerThread = new Thread(DoScheduledAction)
		{
			IsBackground = true,
			Name = name,
			Priority = priority
		};
	}

	protected override void OnStart()
	{
		schdulerThread.Start();
	}

	protected override void OnStop(object syncObject)
	{
		while (State != SchedulerState.Stopped)
		{
			Wait(syncObject);
		}
	}

	private void DoScheduledAction()
	{
		while (RunAction())
		{
		}
	}

	protected override void Wait(object syncObject)
	{
		Monitor.Wait(syncObject);
	}

	protected override void Wait(object syncObject, int periodToWait)
	{
		Monitor.Wait(syncObject, periodToWait);
	}
}
