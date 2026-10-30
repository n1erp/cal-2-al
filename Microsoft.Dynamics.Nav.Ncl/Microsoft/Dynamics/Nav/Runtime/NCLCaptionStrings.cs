using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLCaptionStrings : NCLLookupArray<int, string>
{
	private string defaultValue;

	private string lastValue;

	private int lastLcid;

	private int lastGlobalLcid;

	private int lastDefaultLcid;

	private object thisLock = new object();

	public NCLCaptionStrings(KeyValuePair<int, string>[] lcIdsAndCaptions, string defaultValue)
		: base(lcIdsAndCaptions)
	{
		if (string.IsNullOrEmpty(defaultValue))
		{
			defaultValue = string.Empty;
		}
		this.defaultValue = defaultValue;
	}

	public static NCLCaptionStrings CreateNCLCaptionStrings(string captionML, string defaultValue)
	{
		Dictionary<string, string> dictionary = MultiLanguage.SplitMultiLanguageString(captionML);
		KeyValuePair<int, string>[] array = new KeyValuePair<int, string>[dictionary.Count];
		int num = 0;
		foreach (string key in dictionary.Keys)
		{
			array[num] = new KeyValuePair<int, string>(LanguageHelper.GetLanguageIdByAbbreviatedName(key), dictionary[key]);
			num++;
		}
		return new NCLCaptionStrings(array, defaultValue);
	}

	public static NCLCaptionStrings CreateTextConstant(int[] languageIds, string[] values)
	{
		if (languageIds == null)
		{
			throw new ArgumentNullException("languageIds");
		}
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		KeyValuePair<int, string>[] array = new KeyValuePair<int, string>[languageIds.Length];
		string text = null;
		for (int i = 0; i < languageIds.Length; i++)
		{
			if (text == null && languageIds[i] != 1)
			{
				text = values[i];
			}
			array[i] = new KeyValuePair<int, string>(languageIds[i], values[i]);
		}
		return new NCLCaptionStrings(array, text);
	}

	public int GetCulture(int index)
	{
		return GetKeyValuePair(index).Key;
	}

	public IEnumerable<int> GetCultures()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return GetCulture(i);
		}
	}

	public string GetCaption(int index)
	{
		return GetKeyValuePair(index).Value;
	}

	public string GetValueOrDefault()
	{
		int lcId = ((NavCurrentThread.Session == null) ? NavEnvironment.DefaultLanguage : NavCurrentThread.Session.LocalLanguage);
		return GetValueOrDefault(lcId);
	}

	public string GetValueOrDefault(int lcId)
	{
		lock (thisLock)
		{
			if (lastLcid != lcId || (NavCurrentThread.Session != null && lastGlobalLcid != NavCurrentThread.Session.GlobalLanguage) || lastDefaultLcid != NavEnvironment.DefaultLanguage)
			{
				lastValue = GetValueOrDefaultNoCache(lcId);
				lastLcid = lcId;
				lastGlobalLcid = ((NavCurrentThread.Session != null) ? NavCurrentThread.Session.GlobalLanguage : NavEnvironment.DefaultLanguage);
				lastDefaultLcid = NavEnvironment.DefaultLanguage;
			}
			return lastValue;
		}
	}

	private string GetValueOrDefaultNoCache(int lcId)
	{
		if (TryGetValue(lcId, out var value))
		{
			return value;
		}
		int primaryLanguageId = LanguageHelper.GetPrimaryLanguageId(lcId);
		if (lcId != primaryLanguageId && TryGetValue(primaryLanguageId, out value))
		{
			return value;
		}
		if (NavCurrentThread.Session == null)
		{
			if (lcId != 1033 && TryGetValue(1033, out value))
			{
				return value;
			}
			return defaultValue;
		}
		int globalLanguage = NavCurrentThread.Session.GlobalLanguage;
		if (lcId != globalLanguage)
		{
			if (TryGetValue(globalLanguage, out value))
			{
				return value;
			}
			int primaryLanguageId2 = LanguageHelper.GetPrimaryLanguageId(globalLanguage);
			if (lcId != primaryLanguageId2 && primaryLanguageId2 != globalLanguage && TryGetValue(primaryLanguageId2, out value))
			{
				return value;
			}
		}
		if (lcId != NavEnvironment.DefaultLanguage && TryGetValue(NavEnvironment.DefaultLanguage, out value))
		{
			return value;
		}
		if (lcId != 1033 && TryGetValue(1033, out value))
		{
			return value;
		}
		return defaultValue;
	}
}
