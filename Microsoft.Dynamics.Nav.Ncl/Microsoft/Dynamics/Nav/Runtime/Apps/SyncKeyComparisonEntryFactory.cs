using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class SyncKeyComparisonEntryFactory
{
	public static IEnumerable<SyncKeyComparisonEntry> CreateComparisonEntries(SyncTableBundle bundle)
	{
		List<SyncKeyComparisonEntry> list = new List<SyncKeyComparisonEntry>();
		List<NCLMetaKey> list2 = bundle.TargetNCL?.Keys?.ToList() ?? new List<NCLMetaKey>();
		List<NCLMetaKey> list3 = bundle.OriginalSnapshotNCL?.Keys?.ToList() ?? new List<NCLMetaKey>();
		foreach (NCLMetaKey item in list3)
		{
			MetaKey oldMetaKey = bundle.OriginalSnapshot.Keys[item.KeyIndex];
			NCLMetaKey nCLMetaKey = FindKeyMatch(list2, item);
			if (nCLMetaKey == null)
			{
				list.Add(new SyncKeyComparisonEntry(oldMetaKey, item, null, null));
				continue;
			}
			MetaKey newMetaKey = bundle.Target.Keys[nCLMetaKey.KeyIndex];
			list.Add(new SyncKeyComparisonEntry(oldMetaKey, item, newMetaKey, nCLMetaKey));
			list2.Remove(nCLMetaKey);
		}
		foreach (NCLMetaKey item2 in list2)
		{
			MetaKey newMetaKey2 = bundle.Target.Keys[item2.KeyIndex];
			list.Add(new SyncKeyComparisonEntry(null, null, newMetaKey2, item2));
		}
		return list;
	}

	private static NCLMetaKey FindKeyMatch(IList<NCLMetaKey> candidates, NCLMetaKey target)
	{
		return candidates.FirstOrDefault((NCLMetaKey c) => c.KeyName == target.KeyName) ?? candidates.FirstOrDefault((NCLMetaKey c) => (c.KeyName == null || target.KeyName == null) && c.GetKeyName() == target.GetKeyName());
	}
}
