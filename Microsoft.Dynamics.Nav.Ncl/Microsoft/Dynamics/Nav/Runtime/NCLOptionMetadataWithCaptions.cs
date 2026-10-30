using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Cache;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NCLOptionMetadataWithCaptions : NCLOptionMetadata
{
	private readonly int tableId;

	private readonly int fieldId;

	private readonly MemoryCache<int, NCLLookupArray<int, string[]>> optionCaptionCache;

	private NCLLookupArray<int, string[]> lcIdCaptions;

	private int CurrentCultureIndex
	{
		get
		{
			NCLOptionMetadata.TryFindCurrentCultureIndex(lcIdCaptions, out var index);
			return index;
		}
	}

	public override string OptionCaption
	{
		get
		{
			KeyValuePair<int, string[]> optionCaption;
			if (ServerUserSettings.Instance.LegacyOptionCaptionResolution.Value)
			{
				if (lcIdCaptions == null)
				{
					return OptionString;
				}
				int currentCultureIndex = CurrentCultureIndex;
				optionCaption = lcIdCaptions.GetKeyValuePair(currentCultureIndex);
			}
			else if (!TryFindOptionCaption(out optionCaption))
			{
				return OptionString;
			}
			StringBuilder stringBuilder = new StringBuilder(optionCaption.Value[0], 256);
			for (int i = 1; i < optionCaption.Value.Length; i++)
			{
				stringBuilder.Append(NavOption.OptionDelimiter[0]);
				stringBuilder.Append(optionCaption.Value[i]);
			}
			return stringBuilder.ToString();
		}
	}

	public override bool HasCaption => lcIdCaptions != null;

	public NCLOptionMetadataWithCaptions(string option, NCLCaptionStrings captions, int tableId, int fieldId)
		: base(option)
	{
		if (captions == null)
		{
			throw new ArgumentNullException("captions");
		}
		this.tableId = tableId;
		this.fieldId = fieldId;
		if (captions.Count != 0)
		{
			KeyValuePair<int, string[]>[] array = new KeyValuePair<int, string[]>[captions.Count];
			for (int i = 0; i < captions.Count; i++)
			{
				int culture = captions.GetCulture(i);
				string[] value = ParseOptionCaption(captions.GetCaption(i));
				array[i] = new KeyValuePair<int, string[]>(culture, value);
			}
			lcIdCaptions = new NCLLookupArray<int, string[]>(array.ToArray());
		}
		int value2 = ServerUserSettings.Instance.MaxNumberOfCachedOptionCaptions.Value;
		optionCaptionCache = new MemoryCache<int, NCLLookupArray<int, string[]>>(value2, (value2 / 10 <= 0) ? 1 : (value2 / 10));
	}

	private string[] ParseOptionCaption(string caption)
	{
		string[] array = caption.Split(NavOption.OptionDelimiter, StringSplitOptions.None);
		if (array.Length != Options.Length)
		{
			string[] array2 = new string[Options.Length];
			Array.Copy(array, array2, (array2.Length < array.Length) ? array2.Length : array.Length);
			array = array2;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (string.IsNullOrEmpty(array[i]))
			{
				array[i] = Options[i];
			}
		}
		return array;
	}

	private bool AnyCaptionAvailable(int? languageAppGroupId)
	{
		if (lcIdCaptions == null)
		{
			if (languageAppGroupId.HasValue)
			{
				return DoesAppGroupContainTranslationsForTable(languageAppGroupId.Value);
			}
			return false;
		}
		return true;
	}

	private bool DoesAppGroupContainTranslationsForTable(int appGroup)
	{
		return LanguageProvider.Provider.HasDataFor(ObjectType.Table, tableId, appGroup);
	}

	private NCLLookupArray<int, string[]> AppGroupLanguageOptionCaption()
	{
		int? num = NavCurrentThread.TryResolveAppGroup()?.GroupId;
		if (!num.HasValue)
		{
			return null;
		}
		if (!DoesAppGroupContainTranslationsForTable(num.Value))
		{
			return null;
		}
		string text = MetaTable.MultiLanguageTextMerge(string.Empty, 2, ObjectType.Table, tableId, fieldId, num.Value);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		Dictionary<string, string> dictionary = MultiLanguage.SplitMultiLanguageString(text);
		int capacity = (lcIdCaptions?.Count ?? 0) + dictionary.Count;
		List<KeyValuePair<int, string[]>> list = new List<KeyValuePair<int, string[]>>(capacity);
		for (int i = 0; i < (lcIdCaptions?.Count ?? 0); i++)
		{
			KeyValuePair<int, string[]> keyValuePair = lcIdCaptions.GetKeyValuePair(i);
			list.Add(keyValuePair);
		}
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			int languageIdByAbbreviatedName = LanguageHelper.GetLanguageIdByAbbreviatedName(item.Key);
			int num2 = -1;
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].Key == languageIdByAbbreviatedName)
				{
					num2 = j;
					break;
				}
			}
			KeyValuePair<int, string[]> keyValuePair2 = new KeyValuePair<int, string[]>(languageIdByAbbreviatedName, ParseOptionCaption(item.Value));
			if (num2 != -1)
			{
				list[num2] = keyValuePair2;
			}
			else
			{
				list.Add(keyValuePair2);
			}
		}
		return new NCLLookupArray<int, string[]>(list.ToArray());
	}

	private bool TryFindOptionCaption(out KeyValuePair<int, string[]> optionCaption)
	{
		int? languageAppGroupId = NavCurrentThread.TryResolveAppGroup()?.GroupId;
		if (!AnyCaptionAvailable(languageAppGroupId))
		{
			optionCaption = default;
			return false;
		}
		if (languageAppGroupId.HasValue)
		{
			NCLLookupArray<int, string[]> nCLLookupArray = optionCaptionCache.FindValue(languageAppGroupId.Value, 0L, () => AppGroupLanguageOptionCaption());
			if (nCLLookupArray != null && NCLOptionMetadata.TryFindCurrentCultureIndex(nCLLookupArray, out var index))
			{
				optionCaption = nCLLookupArray.GetKeyValuePair(index);
				return true;
			}
		}
		if (NCLOptionMetadata.TryFindCurrentCultureIndex(lcIdCaptions, out var index2))
		{
			optionCaption = lcIdCaptions.GetKeyValuePair(index2);
			return true;
		}
		optionCaption = default;
		return false;
	}

	public override int GetIndexFromCaption(string caption)
	{
		KeyValuePair<int, string[]> optionCaption;
		if (ServerUserSettings.Instance.LegacyOptionCaptionResolution.Value)
		{
			if (lcIdCaptions == null)
			{
				return base.GetIndexFromCaption(caption);
			}
			int currentCultureIndex = CurrentCultureIndex;
			optionCaption = lcIdCaptions.GetKeyValuePair(currentCultureIndex);
		}
		else if (!TryFindOptionCaption(out optionCaption))
		{
			return base.GetIndexFromCaption(caption);
		}
		int result = StringHelper.FindStringInStringArrayUsingCulture(optionCaption.Key, optionCaption.Value, caption);
		if (result >= 0)
		{
			return result;
		}
		if (int.TryParse(caption, out result))
		{
			return result;
		}
		return -1;
	}

	public override string GetCaptionFromIndex(int index)
	{
		if (ServerUserSettings.Instance.LegacyOptionCaptionResolution.Value)
		{
			if (lcIdCaptions == null)
			{
				return base.GetOptionFromIndex(index);
			}
			if (index < 0 || index >= Options.Length)
			{
				return index.ToString(CultureInfo.InvariantCulture);
			}
			return lcIdCaptions.GetKeyValuePair(CurrentCultureIndex).Value[index];
		}
		if (index < 0 || index >= Options.Length)
		{
			return index.ToString(CultureInfo.InvariantCulture);
		}
		if (!TryFindOptionCaption(out var optionCaption))
		{
			return base.GetCaptionFromIndex(index);
		}
		return optionCaption.Value[index];
	}
}
