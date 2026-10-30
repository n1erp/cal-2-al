using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class SyncChangeAdapter_Clean
{
	public static SyncTableChanges CalculateChanges(SyncTableBundle bundle)
	{
		if (bundle.OriginalSnapshot == null)
		{
			return new SyncTableChanges(SyncTableChangeType.None, Enumerable.Empty<SyncFieldChange>(), Enumerable.Empty<SyncKeyChange>());
		}
		return new SyncTableChanges(SyncTableChangeType.Remove, Enumerable.Empty<SyncFieldChange>(), Enumerable.Empty<SyncKeyChange>());
	}
}
