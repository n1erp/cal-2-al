using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class SyncBundleFilters
{
	public static IEnumerable<ISyncBundleFilter> AllFilters
	{
		get
		{
			yield return new NoNormalFieldsBundleFilter();
		}
	}
}
