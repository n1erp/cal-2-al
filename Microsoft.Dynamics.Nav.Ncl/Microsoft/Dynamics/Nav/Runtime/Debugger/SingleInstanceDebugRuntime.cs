namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class SingleInstanceDebugRuntime : DebugRuntime
{
	private bool isDisposed;

	public static SingleInstanceDebugRuntime Instance { get; set; }

	protected internal SingleInstanceDebugRuntime(DebugEnvironment environment, NavSession session)
		: base(environment, session)
	{
	}

	protected override bool CanCoexistWithOtherListener(IExecutionListener otherListener)
	{
		return otherListener.GetType() != typeof(SingleInstanceDebugRuntime);
	}

	protected override void Dispose(bool disposing)
	{
		if (!isDisposed)
		{
			if (disposing)
			{
				Instance = null;
				base.Dispose(disposing);
			}
			isDisposed = true;
		}
	}
}
