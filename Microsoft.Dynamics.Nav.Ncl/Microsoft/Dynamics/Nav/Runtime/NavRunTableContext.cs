using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavRunTableContext : IDisposable
{
	private bool disposed;

	public NavSession Session { get; private set; }

	public string ActivityId { get; }

	private NavRunTableContext(NavSession session, string activityId)
	{
		ActivityId = activityId;
		Session = session;
		Session.SetRunTableContext(this);
	}

	public static NavRunTableContext GetOrCreate(NavSession session, string activityId)
	{
		return session.RunTableContext ?? new NavRunTableContext(session, activityId);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing)
			{
				Session.ClearRunTableContext();
				Session = null;
			}
			disposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
