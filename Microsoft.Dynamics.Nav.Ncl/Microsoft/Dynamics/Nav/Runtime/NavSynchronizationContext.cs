using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSynchronizationContext : SynchronizationContext
{
	private static NavSynchronizationContext instance = new NavSynchronizationContext();

	public static NavSynchronizationContext Instance => instance;

	private static SendOrPostCallback ClearThreadLocalStorageDelegate(SendOrPostCallback originalDelegate)
	{
		return (object state) =>
		{
			NavThreadLocalStorage current = NavThreadLocalStorage.Current;
			try
			{
				NavThreadLocalStorage.Clear();
				originalDelegate(state);
			}
			finally
			{
				NavThreadLocalStorage.Restore(current);
			}
		};
	}

	public override void Send(SendOrPostCallback d, object state)
	{
		base.Send(ClearThreadLocalStorageDelegate(d), state);
	}

	public override void Post(SendOrPostCallback d, object state)
	{
		base.Post(ClearThreadLocalStorageDelegate(d), state);
	}
}
