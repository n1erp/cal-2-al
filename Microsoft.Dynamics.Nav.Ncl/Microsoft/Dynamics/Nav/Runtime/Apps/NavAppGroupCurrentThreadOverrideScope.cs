using System;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppGroupCurrentThreadOverrideScope : IDisposable
{
	public NavAppGroupCurrentThreadOverrideScope(NavAppGroup appGroup)
	{
		NavCurrentThread.SetOverriddenAppGroups(appGroup);
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
			NavCurrentThread.ClearOverriddenAppGroup();
		}
	}
}
