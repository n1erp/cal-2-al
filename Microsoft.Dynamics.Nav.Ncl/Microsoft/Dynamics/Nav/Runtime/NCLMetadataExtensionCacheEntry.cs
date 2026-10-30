using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetadataExtensionCacheEntry
{
	private NavAppObjectExtensionEntry[] navAppExtensionObjects;

	public ApplicationObjectId ApplicationObjectId { get; private set; }

	public IEnumerable<NavAppObjectExtensionEntry> ExtensionObjects => navAppExtensionObjects;

	public NCLMetadataExtensionCacheEntry(ApplicationObjectId applicationObjectId)
	{
		ApplicationObjectId = applicationObjectId;
	}

	public void AddNavAppExtensionObject(int runtimeAppId, NCLApplicationObjectExtension navAppExtensionObject)
	{
		lock (this)
		{
			NavAppObjectExtensionEntry[] second = new NavAppObjectExtensionEntry[1]
			{
				new NavAppObjectExtensionEntry(runtimeAppId, navAppExtensionObject)
			};
			if (navAppExtensionObjects == null)
			{
				navAppExtensionObjects = second;
				return;
			}
			navAppExtensionObjects = (from entry in navAppExtensionObjects.Concat(second)
				orderby entry.RuntimeAppId
				select entry).ToArray();
		}
	}

	public NCLApplicationObjectExtension SpecificRuntimeAppObject(int runtimeAppId)
	{
		NavAppObjectExtensionEntry[] array = navAppExtensionObjects;
		int num = FindArrayIndexOfGroupObject(array, runtimeAppId);
		if (num != -1)
		{
			return array[num].ExtensionObject;
		}
		return null;
	}

	private static int FindArrayIndexOfGroupObject(NavAppObjectExtensionEntry[] sortedEntries, int runtimeAppId)
	{
		if (sortedEntries != null)
		{
			int num = 0;
			int num2 = sortedEntries.Length - 1;
			while (num <= num2)
			{
				int num3 = num + num2 >> 1;
				int runtimeAppId2 = sortedEntries[num3].RuntimeAppId;
				if (runtimeAppId2 < runtimeAppId)
				{
					num = num3 + 1;
					continue;
				}
				if (runtimeAppId2 > runtimeAppId)
				{
					num2 = num3 - 1;
					continue;
				}
				return num3;
			}
		}
		return -1;
	}
}
