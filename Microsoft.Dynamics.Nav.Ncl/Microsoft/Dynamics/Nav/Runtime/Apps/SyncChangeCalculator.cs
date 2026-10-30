using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class SyncChangeCalculator
{
	public static SyncTableChanges CalculateChanges(AppSyncContext syncContext, SyncTableBundle bundle)
	{
		switch (syncContext.AppSyncMode)
		{
		case NavAppSyncMode.Add:
		case NavAppSyncMode.Development:
			return SyncChangeAdapter_Add.CalculateChanges(bundle);
		case NavAppSyncMode.Clean:
			return SyncChangeAdapter_Clean.CalculateChanges(bundle);
		case NavAppSyncMode.ForceSync:
			return SyncChangeAdapter_Force.CalculateChanges(bundle);
		default:
			throw new InvalidOperationException("Unsupported sync mode: " + syncContext.AppSyncMode);
		}
	}
}
