using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLEnumMetadata : NCLOptionMetadata
{
	private readonly int[] indexes;

	private readonly NCLCaptionStrings[] optionCaptions;

	private readonly NCLLookupArray<int, string[]> lcidCaptions;

	private readonly ConcurrentDictionary<int, string> optionCaptionCache = new ConcurrentDictionary<int, string>();

	private readonly ConcurrentDictionary<int, string> optionCaptionMLCache = new ConcurrentDictionary<int, string>();

	public override bool IsEnum => true;

	internal override int[] OrdinalValues => indexes;

	public override bool HasCaption => optionCaptions != null;

	public override string OptionCaption
	{
		get
		{
			if (optionCaptions == null || optionCaptions.Length == 0)
			{
				return OptionString;
			}
			int lcid = GetCurrentLcid();
			return optionCaptionCache.GetOrAdd(lcid, (int locale) =>
			{
				StringBuilder stringBuilder = new StringBuilder(optionCaptions[0].GetValueOrDefault(locale), 256);
				for (int i = 1; i < optionCaptions.Length; i++)
				{
					stringBuilder.Append(NavOption.OptionDelimiter[0]);
					stringBuilder.Append(optionCaptions[i].GetValueOrDefault(lcid));
				}
				return stringBuilder.ToString();
			});
		}
	}

	public override string OptionCaptionML
	{
		get
		{
			if (!HasCaption)
			{
				return null;
			}
			int lcid = GetCurrentLcid();
			return optionCaptionMLCache.GetOrAdd(lcid, (int locale) => LanguageHelper.GetAbbreviatedLanguageName(lcid) + "=\"" + OptionCaption + "\"");
		}
	}

	protected NCLEnumMetadata(string[] options, int[] indexes, NCLCaptionStrings[] optionCaptions)
		: base(string.Join(",", options))
	{
		this.indexes = indexes;
		this.optionCaptions = optionCaptions;
		if (options.Length != this.indexes.Length || (optionCaptions != null && this.indexes.Length != optionCaptions.Length))
		{
			throw new ArgumentException(Lang.OptionStringHasWrongNumberOfParameters);
		}
		if (optionCaptions == null)
		{
			return;
		}
		int[] array = optionCaptions.SelectMany((NCLCaptionStrings o) => o.GetCultures()).Distinct().ToArray();
		if (array.Length == 0)
		{
			return;
		}
		KeyValuePair<int, string[]>[] valuePairs = array.Select((int c) => new KeyValuePair<int, string[]>(c, optionCaptions.Select((NCLCaptionStrings oc) => oc.GetValueOrDefault(c)).ToArray())).ToArray();
		lcidCaptions = new NCLLookupArray<int, string[]>(valuePairs);
	}

	internal static NCLEnumMetadata Create(string[] option, int[] indexes, NCLCaptionStrings[] optionCaptions)
	{
		return new NCLEnumMetadata(option, indexes, optionCaptions);
	}

	public static NCLOptionMetadata Create(int id)
	{
		return NavGlobal.MetadataProvider.GetEnumMetadata(id) ?? NCLOptionMetadata.Default;
	}

	public override bool IsValidOrdinal(int ordinal)
	{
		return indexes.Any((int idx) => idx == ordinal);
	}

	public override string GetOptionFromIndex(int index)
	{
		int num = index;
		for (int i = 0; i < indexes.Length; i++)
		{
			if (num == indexes[i])
			{
				return Options[i];
			}
		}
		return index.ToString(CultureInfo.InvariantCulture);
	}

	public override int GetIndexFromOption(string option)
	{
		int result = StringHelper.FindStringInStringArrayUsingCurrentCulture(Options, option);
		if (result != -1)
		{
			return indexes[result];
		}
		if (int.TryParse(option, out result))
		{
			return result;
		}
		return -1;
	}

	public override int GetIndexFromCaption(string caption)
	{
		if (lcidCaptions == null)
		{
			return base.GetIndexFromCaption(caption);
		}
		if (!NCLOptionMetadata.TryFindCurrentCultureIndex(lcidCaptions, out var index))
		{
			return base.GetIndexFromCaption(caption);
		}
		KeyValuePair<int, string[]> keyValuePair = lcidCaptions.GetKeyValuePair(index);
		int num = StringHelper.FindStringInStringArrayUsingCulture(keyValuePair.Key, keyValuePair.Value, caption);
		if (num >= 0)
		{
			return OrdinalValues[num];
		}
		return -1;
	}

	public override string GetCaptionFromIndex(int index)
	{
		string text = null;
		for (int i = 0; i < indexes.Length; i++)
		{
			if (index == indexes[i])
			{
				text = optionCaptions[i].GetValueOrDefault();
				break;
			}
		}
		return text ?? GetOptionFromIndex(index);
	}

	private static int GetCurrentLcid()
	{
		if (NavCurrentThread.Session != null)
		{
			return NavCurrentThread.Session.LocalLanguage;
		}
		return NavEnvironment.DefaultLanguage;
	}
}
