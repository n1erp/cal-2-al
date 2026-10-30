using System;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NCLFieldEnumBaseMetadata(string option) : NCLOptionMetadata(option)
{
	private class CacheEntry
	{
		public int GroupId { get; private set; }

		public NCLOptionMetadata Metadata { get; private set; }

		public CacheEntry(int groupId, NCLOptionMetadata metadata)
		{
			GroupId = groupId;
			Metadata = metadata;
		}
	}

	private readonly object lockObj = new object();

	private CacheEntry[] optionMetadataByAppGroup = Array.Empty<CacheEntry>();

	public override bool IsEnum => true;

	internal override string[] Options => GetAppGroupAwareEnumMetadata().Options;

	public override string OptionString => GetAppGroupAwareEnumMetadata().OptionString;

	internal override int[] OrdinalValues => GetAppGroupAwareEnumMetadata().OrdinalValues;

	public override string OptionCaption => GetAppGroupAwareEnumMetadata().OptionCaption;

	public override string OptionCaptionML => GetAppGroupAwareEnumMetadata().OptionCaptionML;

	protected internal abstract NCLOptionMetadata GetEnumMetadataFromMetadataProvider();

	private NCLOptionMetadata GetAppGroupAwareEnumMetadata()
	{
		int groupId = NavCurrentThread.ResolveAppGroup().GroupId;
		CacheEntry[] array = optionMetadataByAppGroup;
		int num = FindArrayIndexOfAppObject(array, groupId);
		if (num != -1)
		{
			return array[num].Metadata;
		}
		lock (lockObj)
		{
			num = FindArrayIndexOfAppObject(array, groupId);
			if (num != -1)
			{
				return array[num].Metadata;
			}
			NCLOptionMetadata enumMetadataFromMetadataProvider = GetEnumMetadataFromMetadataProvider();
			CacheEntry cacheEntry = new CacheEntry(groupId, enumMetadataFromMetadataProvider);
			optionMetadataByAppGroup = (from entry in optionMetadataByAppGroup.Concat(new CacheEntry[1] { cacheEntry })
				orderby entry.GroupId
				select entry).ToArray();
			return cacheEntry.Metadata;
		}
	}

	private static int FindArrayIndexOfAppObject(CacheEntry[] sortedEntries, int appGroupId)
	{
		if (sortedEntries != null)
		{
			int num = 0;
			int num2 = sortedEntries.Length - 1;
			while (num <= num2)
			{
				int num3 = num + num2 >> 1;
				int groupId = sortedEntries[num3].GroupId;
				if (groupId < appGroupId)
				{
					num = num3 + 1;
					continue;
				}
				if (groupId > appGroupId)
				{
					num2 = num3 - 1;
					continue;
				}
				return num3;
			}
		}
		return -1;
	}

	public override bool IsValidOrdinal(int ordinal)
	{
		return GetAppGroupAwareEnumMetadata().IsValidOrdinal(ordinal);
	}

	public override string GetOptionFromIndex(int index)
	{
		return GetAppGroupAwareEnumMetadata().GetOptionFromIndex(index);
	}

	public override int GetIndexFromOption(string option)
	{
		return GetAppGroupAwareEnumMetadata().GetIndexFromOption(option);
	}

	public override string GetCaptionFromIndex(int index)
	{
		return GetAppGroupAwareEnumMetadata().GetCaptionFromIndex(index);
	}

	public override int GetIndexFromCaption(string caption)
	{
		return GetAppGroupAwareEnumMetadata().GetIndexFromCaption(caption);
	}
}
