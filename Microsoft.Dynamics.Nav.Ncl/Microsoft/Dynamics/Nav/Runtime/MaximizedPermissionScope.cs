using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class MaximizedPermissionScope : IDisposable
{
	private NavSession session;

	private int[] tableObjects;

	public MaximizedPermissionScope(NavSession session, int[] tableObjects)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		if (tableObjects == null)
		{
			throw new ArgumentNullException("tableObjects");
		}
		this.session = session;
		this.tableObjects = tableObjects;
		this.session.MaximizePermissions(tableObjects);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing && session != null)
		{
			session.RemoveMaximizedPermissions(tableObjects);
			session = null;
			tableObjects = null;
		}
	}
}
