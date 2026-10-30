using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Translation;

internal class LanguagesMetadata
{
	internal Dictionary<string, List<TranslationItem>> SymbolIdAndItsMultilanguageValues { get; }

	internal DiagnosticBag diagnostics { get; }

	public static LanguagesMetadata Extract(IFileSystem fileSystem, DiagnosticBag diagnostics, string moduleInfoName)
	{
		LanguagesMetadata languagesMetadata = new LanguagesMetadata(diagnostics);
		if (fileSystem != null)
		{
			HashSet<string> languagesAdded = new HashSet<string>();
			List<Tuple<string, string>> list = new List<Tuple<string, string>>();
			foreach (string item in from p in fileSystem.GetFiles("/Translations/*.lcl")
				where !IsDefaultXliffOrLclFile(p, moduleInfoName)
				select p)
			{
				LanguageFileExtracting.ExtractLcl(fileSystem, item, languagesMetadata, diagnostics, languagesAdded, list);
			}
			foreach (string item2 in from p in fileSystem.GetFiles("/Translations/*.xlf").Concat(fileSystem.GetFiles("/Translations/*.xliff"))
				where !IsDefaultXliffOrLclFile(p, moduleInfoName)
				select p)
			{
				LanguageFileExtracting.ExtractXliff12(fileSystem, item2, languagesMetadata, diagnostics, languagesAdded, list);
			}
			(from languageAndFile in list
				group languageAndFile by languageAndFile.Item1 into grouping
				where grouping.Count() > 1
				select grouping).Do((IGrouping<string, Tuple<string, string>> grouping) =>
			{
				grouping.Do((Tuple<string, string> file) =>
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateLanguageTranslation, Location.None, file);
				});
			});
		}
		return languagesMetadata;
	}

	private static bool IsDefaultXliffOrLclFile(string filePath, string extensionName)
	{
		string fileName = Path.GetFileName(filePath);
		if (!(fileName == LanguageFileUtilities.GetTranslationFileName(extensionName, ".xlf")) && !(fileName == LanguageFileUtilities.GetTranslationFileName(extensionName, ".xliff")))
		{
			return fileName == LanguageFileUtilities.GetTranslationFileName(extensionName, ".lcl");
		}
		return true;
	}

	private LanguagesMetadata(DiagnosticBag diagnostics)
	{
		SymbolIdAndItsMultilanguageValues = new Dictionary<string, List<TranslationItem>>();
		this.diagnostics = diagnostics;
	}

	internal void AddLanguageForSymbol(TranslationItem translationItem)
	{
		if (translationItem.TranslatedString == null)
		{
			return;
		}
		if (SymbolIdAndItsMultilanguageValues.ContainsKey(translationItem.Id))
		{
			List<TranslationItem> list = SymbolIdAndItsMultilanguageValues[translationItem.Id];
			TranslationItem translationItem2 = list.FirstOrDefault((TranslationItem item) => item.Culture == translationItem.Culture);
			if (translationItem2 == null)
			{
				list.Add(translationItem);
				return;
			}
			diagnostics.Add(ErrorCode.WRN_DuplicateTranslationItemForSameId, translationItem2.Location);
			diagnostics.Add(ErrorCode.WRN_DuplicateTranslationItemForSameId, translationItem.Location);
		}
		else
		{
			SymbolIdAndItsMultilanguageValues[translationItem.Id] = new List<TranslationItem> { translationItem };
		}
	}

	public string GetLanguagesForSymbolByLanguageName(Symbol property, BoundLabel label)
	{
		return PropertyValueTextHelper.GetMultilanguageString(from v in GetLanguagesForSymbol(property, label)
			select PropertyValueTextHelper.GetMultilanguageStringItem(CultureInfo.GetCultureInfo(v.Culture).ThreeLetterWindowsLanguageName, v.TranslatedString));
	}

	internal List<(int languageId, string translatedText)> GetLanguagesForSymbolByLanguageId(Symbol typeSymbol, BoundLabel label)
	{
		List<TranslationItem> languagesForSymbol = GetLanguagesForSymbol(typeSymbol, label);
		List<(int, string)> list = new List<(int, string)>(languagesForSymbol.Count);
		foreach (TranslationItem item in languagesForSymbol)
		{
			list.Add((CultureInfo.GetCultureInfo(item.Culture).LCID, item.TranslatedString));
		}
		return list;
	}

	private List<TranslationItem> GetLanguagesForSymbol(Symbol symbol, BoundLabel label = null)
	{
		bool isMissingCaption = label == null;
		List<TranslationItem> listOfTranslationItemsForSymbol = GetListOfTranslationItemsForSymbol(symbol, isMissingCaption, out var languageSymbolId);
		List<TranslationItem> list = TranslationItem.ProcessAndFilterTranslationItems(label, listOfTranslationItemsForSymbol, diagnostics);
		string defaultCulture = "en-US";
		if (!list.Contains((TranslationItem l) => l.Culture == defaultCulture))
		{
			string text = label?.Text.StringValue ?? symbol.Name;
			list.Add(TranslationItem.CreateTranslated(languageSymbolId, text, text, defaultCulture, symbol.GetLocation()));
		}
		return list;
	}

	private List<TranslationItem> GetListOfTranslationItemsForSymbol(Symbol symbol, bool isMissingCaption, out string languageSymbolId)
	{
		languageSymbolId = LanguageFileUtilities.GetLanguageSymbolId(symbol, shouldHashName: true, isMissingCaption);
		if (SymbolIdAndItsMultilanguageValues.TryGetValue(languageSymbolId, out var value))
		{
			return value;
		}
		languageSymbolId = LanguageFileUtilities.GetLanguageSymbolId(symbol, shouldHashName: false, isMissingCaption);
		if (SymbolIdAndItsMultilanguageValues.TryGetValue(languageSymbolId, out value))
		{
			return value;
		}
		return new List<TranslationItem>();
	}
}
