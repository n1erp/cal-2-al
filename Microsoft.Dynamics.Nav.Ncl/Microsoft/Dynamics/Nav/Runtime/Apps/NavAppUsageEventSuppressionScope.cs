using System;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppUsageEventSuppressionScope : IDisposable
{
	private NavSession session;

	public NavAppUsageEventSuppressionScope(NavSession session, string reason)
	{
		this.session = session;
		session?.PushNavAppUsageEventSuppression(reason);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			session?.PopNavAppUsageEventSuppression();
			session = null;
		}
	}
}
