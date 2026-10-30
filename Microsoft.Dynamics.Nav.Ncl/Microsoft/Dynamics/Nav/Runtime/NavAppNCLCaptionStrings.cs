using System;
using System.Linq;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavAppNCLCaptionStrings
{
	private class AppCaptionStringsEntry
	{
		public int GroupId { get; }

		public NCLCaptionStrings CaptionStrings { get; }

		public int CaptionStringsHashCode { get; }

		public AppCaptionStringsEntry(int groupId, NCLCaptionStrings captionStrings)
		{
			GroupId = groupId;
			for (int i = 0; i < captionStrings.Count; i++)
			{
				int culture = captionStrings.GetCulture(i);
				string caption = captionStrings.GetCaption(i);
				CaptionStringsHashCode = HashCodeHelper.CombineHashCodes(CaptionStringsHashCode, culture, caption.GetHashCode());
			}
			CaptionStrings = captionStrings;
		}

		public AppCaptionStringsEntry(int groupId, AppCaptionStringsEntry existing)
		{
			GroupId = groupId;
			CaptionStringsHashCode = existing.CaptionStringsHashCode;
			CaptionStrings = existing.CaptionStrings;
		}
	}

	private readonly object lockObj = new object();

	private AppCaptionStringsEntry[] captionStringsByAppGroup = new AppCaptionStringsEntry[0];

	public NCLCaptionStrings GetCaptionStrings(int languageAppGroupId, Func<int, NCLCaptionStrings> creator)
	{
		AppCaptionStringsEntry[] array = captionStringsByAppGroup;
		int num = FindArrayIndexOfAppObject(array, languageAppGroupId);
		if (num != -1)
		{
			return array[num].CaptionStrings;
		}
		lock (lockObj)
		{
			num = FindArrayIndexOfAppObject(array, languageAppGroupId);
			if (num != -1)
			{
				return array[num].CaptionStrings;
			}
			NCLCaptionStrings nCLCaptionStrings = creator(languageAppGroupId);
			AppCaptionStringsEntry appCaptionStringsEntry = new AppCaptionStringsEntry(languageAppGroupId, nCLCaptionStrings);
			AppCaptionStringsEntry[] array2 = captionStringsByAppGroup;
			foreach (AppCaptionStringsEntry appCaptionStringsEntry2 in array2)
			{
				if (appCaptionStringsEntry2.CaptionStringsHashCode != appCaptionStringsEntry.CaptionStringsHashCode || appCaptionStringsEntry2.CaptionStrings.Count != nCLCaptionStrings.Count)
				{
					continue;
				}
				bool flag = true;
				for (int j = 0; (j < appCaptionStringsEntry2.CaptionStrings.Count) & flag; j++)
				{
					int culture = appCaptionStringsEntry2.CaptionStrings.GetCulture(j);
					string caption = appCaptionStringsEntry2.CaptionStrings.GetCaption(j);
					int culture2 = nCLCaptionStrings.GetCulture(j);
					string caption2 = nCLCaptionStrings.GetCaption(j);
					if (culture != culture2 || caption != caption2)
					{
						flag = false;
					}
				}
				if (flag)
				{
					appCaptionStringsEntry = new AppCaptionStringsEntry(languageAppGroupId, appCaptionStringsEntry2);
				}
			}
			captionStringsByAppGroup = (from entry in captionStringsByAppGroup.Concat(new AppCaptionStringsEntry[1] { appCaptionStringsEntry })
				orderby entry.GroupId
				select entry).ToArray();
			return appCaptionStringsEntry.CaptionStrings;
		}
	}

	private static int FindArrayIndexOfAppObject(AppCaptionStringsEntry[] sortedEntries, int appGroupId)
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
}
