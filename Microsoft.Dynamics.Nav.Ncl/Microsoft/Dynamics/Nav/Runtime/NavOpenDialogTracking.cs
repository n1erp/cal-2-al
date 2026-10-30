using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavOpenDialogTracking : IDisposable
{
	private readonly HashSet<NavDialog> openDialogs = new HashSet<NavDialog>();

	private bool isDisposed;

	internal int OpenedDialogsCount => openDialogs.Count;

	public NavOpenDialogTracking()
	{
		NavCompany company = NavCurrentThread.Session.Company;
		company.PushOpenedDialogTracking(this);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	internal bool Contains(NavDialog dialog)
	{
		return openDialogs.Contains(dialog);
	}

	internal void RegisterOpenDialog(NavDialog dialog)
	{
		openDialogs.Add(dialog);
	}

	internal void UnregisterOpenDialog(NavDialog dialog)
	{
		openDialogs.Remove(dialog);
	}

	private void CloseAllOpenDialogs()
	{
		if (openDialogs.Count <= 0)
		{
			return;
		}
		NavDialog[] array = openDialogs.ToArray();
		NavDialog[] array2 = array;
		foreach (NavDialog navDialog in array2)
		{
			if (navDialog != null && !navDialog.IsDisposed)
			{
				navDialog.CloseDialogIfOpen();
			}
		}
		openDialogs.Clear();
	}

	protected void Dispose(bool disposing)
	{
		if (isDisposed || !disposing)
		{
			return;
		}
		try
		{
			CloseAllOpenDialogs();
		}
		finally
		{
			if (NavCurrentThread.Session != null && NavCurrentThread.Session.IsOpen && NavCurrentThread.Session.Company != null)
			{
				NavCurrentThread.Session.Company.PopUntilOpenedDialogTracking(this);
			}
		}
		isDisposed = true;
	}

	[Conditional("DEBUG")]
	private static void CheckDialogTrackingState()
	{
		_ = NavCurrentThread.Session.IsOpen;
	}
}
