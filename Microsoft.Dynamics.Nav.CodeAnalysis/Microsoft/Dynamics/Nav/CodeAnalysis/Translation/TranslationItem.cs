using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Translation;

public class TranslationItem
{
	public const bool DefaultTranslationItemLocked = false;

	public const string DefaultSourceLanguageValue = "en-US";

	public string Id { get; }

	public string NamedId { get; }

	public string ItemType { get; }

	public string LegacyId { get; }

	public string Culture { get; }

	public string SourceString { get; }

	public string TranslatedString { get; }

	public Location Location { get; }

	public bool Locked { get; }

	public int? MaxSize { get; }

	public string Comment { get; }

	private TranslationItem(string id, string namedId, string sourceString, Location location, string translatedString = null, bool? locked = null, int? maxSize = null, string comment = null, string culture = "en-US", string itemType = null, string legacyId = null)
	{
		Id = id;
		NamedId = namedId;
		Culture = culture;
		SourceString = sourceString;
		TranslatedString = translatedString;
		Location = location;
		Locked = locked ?? false;
		MaxSize = maxSize;
		Comment = comment ?? string.Empty;
		ItemType = itemType;
		LegacyId = legacyId;
	}

	public static TranslationItem CreateUntranslated(string id, string nameId, string sourceString, Location location, int? maxSize, bool? locked, string comment, string itemType)
	{
		return new TranslationItem(id, nameId, sourceString, location, null, locked, maxSize, comment, "en-US", itemType);
	}

	public static TranslationItem CreateTranslated(string id, string sourceString, string translatedString, string culture, Location location, string comment = null, string namedId = null, string legacyId = null)
	{
		return new TranslationItem(id, namedId, sourceString, location, translatedString, null, null, comment, culture, null, legacyId);
	}

	internal static List<TranslationItem> ProcessAndFilterTranslationItems(BoundLabel label, List<TranslationItem> multilanguageValues, DiagnosticBag diagnostics)
	{
		string text = label?.Text?.StringValue;
		int? num = label?.MaxLength?.Int32Value;
		if (string.IsNullOrEmpty(text))
		{
			return multilanguageValues;
		}
		List<TranslationItem> list = new List<TranslationItem>();
		foreach (TranslationItem multilanguageValue in multilanguageValues)
		{
			if (multilanguageValue.SourceString == text)
			{
				if (!num.HasValue || multilanguageValue.TranslatedString.Length <= num)
				{
					list.Add(multilanguageValue);
					continue;
				}
				TranslationItem item = multilanguageValue.CloneWithTrimmedTranslatedString(num.Value);
				list.Add(item);
				diagnostics.Add(ErrorCode.WRN_TranslationItemTranslatedStringTooLong, multilanguageValue.Location);
			}
			else
			{
				diagnostics.Add(ErrorCode.WRN_TranslationItemSourceStringMismatch, multilanguageValue.Location);
			}
		}
		return list;
	}

	internal TranslationItem CloneWithTrimmedTranslatedString(int translatedStringLength)
	{
		return new TranslationItem(Id, NamedId, SourceString, Location, TranslatedString.Substring(0, translatedStringLength), Locked, MaxSize, Comment, Culture);
	}
}
