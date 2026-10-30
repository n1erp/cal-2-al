using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Translation;

internal static class LanguageFileExtracting
{
	internal static void ExtractLcl(IFileSystem fileSystem, string filePath, LanguagesMetadata languagesMetadata, DiagnosticBag diagnostics, HashSet<string> languagesAdded, List<Tuple<string, string>> languagesAndFiles)
	{
		byte[] bytes = fileSystem.ReadBytes(filePath);
		string text = Encoding.UTF8.GetString(bytes);
		NewlineIterator newlineIterator = new NewlineIterator(text);
		LCX lCX = Deserialize<LCX>(fileSystem, filePath, diagnostics);
		if (lCX == null)
		{
			return;
		}
		string tgtCul = lCX.TgtCul;
		if (!TryValidateCulture(tgtCul, filePath, text, newlineIterator, diagnostics))
		{
			return;
		}
		languagesAndFiles.Add(Tuple.Create(tgtCul, filePath));
		if (languagesAdded.Contains(tgtCul))
		{
			return;
		}
		languagesAdded.Add(tgtCul);
		if (lCX.Item == null)
		{
			return;
		}
		LCXItem[] item = lCX.Item;
		foreach (LCXItem lCXItem in item)
		{
			if (string.Equals(lCXItem?.Str?.Tgt?.Stat, "Loc", StringComparison.OrdinalIgnoreCase))
			{
				Location stringLocation = GetStringLocation(filePath, text, newlineIterator, lCXItem.ItemId);
				if (lCXItem != null && lCXItem.Str?.Val != null && lCXItem.ItemId.Length > 0)
				{
					string sourceString = lCXItem?.Str?.Val?.Replace("]5D;", "]");
					string translatedString = lCXItem?.Str?.Tgt?.Val?.Replace("]5D;", "]");
					string text2 = lCXItem?.ItemId;
					text2 = ((text2[0] == ';') ? text2.Substring(1) : text2);
					TranslationItem translationItem = TranslationItem.CreateTranslated(text2, sourceString, translatedString, tgtCul, stringLocation);
					languagesMetadata.AddLanguageForSymbol(translationItem);
				}
			}
		}
	}

	internal static void ExtractXliff12(IFileSystem fileSystem, string filePath, LanguagesMetadata languagesMetadata, DiagnosticBag diagnostics, HashSet<string> languagesAdded, List<Tuple<string, string>> languagesAndFiles)
	{
		byte[] bytes = fileSystem.ReadBytes(filePath);
		string text = Encoding.UTF8.GetString(bytes);
		NewlineIterator newlineIterator = new NewlineIterator(text);
		xliff xliff2 = Deserialize<xliff>(fileSystem, filePath, diagnostics);
		if (xliff2 == null)
		{
			return;
		}
		string text2 = xliff2?.file?.targetlanguage;
		xliffFileBodyGroupTransunit[] array = xliff2?.file?.body?.group?.transunit;
		if (!TryValidateCulture(text2, filePath, text, newlineIterator, diagnostics))
		{
			return;
		}
		languagesAndFiles.Add(Tuple.Create(text2, filePath));
		if (languagesAdded.Contains(text2))
		{
			return;
		}
		languagesAdded.Add(text2);
		if (array == null || array.Length == 0)
		{
			return;
		}
		xliffFileBodyGroupTransunit[] array2 = array;
		foreach (xliffFileBodyGroupTransunit xliffFileBodyGroupTransunit2 in array2)
		{
			if (string.Equals(xliffFileBodyGroupTransunit2?.translate, "yes", StringComparison.OrdinalIgnoreCase) && xliffFileBodyGroupTransunit2 != null && xliffFileBodyGroupTransunit2.target?.Value != null)
			{
				Location stringLocation = GetStringLocation(filePath, text, newlineIterator, xliffFileBodyGroupTransunit2.id);
				TranslationItem translationItem = TranslationItem.CreateTranslated(xliffFileBodyGroupTransunit2?.id, xliffFileBodyGroupTransunit2?.source, xliffFileBodyGroupTransunit2?.target?.Value, text2, stringLocation);
				languagesMetadata.AddLanguageForSymbol(translationItem);
			}
		}
	}

	private static T Deserialize<T>(IFileSystem fileSystem, string filePath, DiagnosticBag diagnostics)
	{
		try
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
			MemoryStream input = new MemoryStream(fileSystem.ReadBytes(filePath));
			XmlReaderSettings settings = new XmlReaderSettings
			{
				XmlResolver = null
			};
			using XmlReader xmlReader = XmlReader.Create(input, settings);
			return (T)xmlSerializer.Deserialize(xmlReader);
		}
		catch (InvalidOperationException exception)
		{
			HandleXmlDeserializationException(diagnostics, exception);
		}
		return default;
	}

	private static Location GetStringLocation(string filePath, string fileAsString, NewlineIterator newlineIterator, string id)
	{
		int num = fileAsString.IndexOf(id, newlineIterator.LastSearchedPosition, StringComparison.Ordinal);
		int line = newlineIterator.CalculateNewlinesUpUntil(num + id.Length);
		int currentNewlinePosition = newlineIterator.CurrentNewlinePosition;
		TextSpan sourceSpan = new TextSpan(num, id.Length);
		LinePosition start = new LinePosition(line, num - currentNewlinePosition - 1);
		LinePosition end = new LinePosition(line, num - currentNewlinePosition + id.Length);
		LinePositionSpan lineSpan = new LinePositionSpan(start, end);
		return new ExternalFileLocation(filePath, sourceSpan, lineSpan);
	}

	private static void HandleXmlDeserializationException(DiagnosticBag diagnostics, InvalidOperationException exception)
	{
		string text = exception.Message + " " + exception?.InnerException?.Message;
		diagnostics.Add(ErrorCode.WRN_XmlValidationErrorOccurred, Location.None, text);
	}

	[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
	private static bool TryValidateCulture(string cultureText, string filePath, string fileAsString, NewlineIterator newlineIterator, DiagnosticBag diagnostics)
	{
		if (string.IsNullOrEmpty(cultureText))
		{
			diagnostics.Add(ErrorCode.WRN_TranslationFileTargetLanguageMissing, Location.None, filePath);
			return false;
		}
		try
		{
			if (MultiLanguageHelper.IsCustomCulture(CultureInfo.GetCultureInfo(cultureText)))
			{
				Location stringLocation = GetStringLocation(filePath, fileAsString, newlineIterator, cultureText);
				diagnostics.Add(ErrorCode.WRN_TranslationFileTargetLanguageInvalid, stringLocation, filePath, cultureText);
				return false;
			}
		}
		catch (CultureNotFoundException)
		{
			Location stringLocation2 = GetStringLocation(filePath, fileAsString, newlineIterator, cultureText);
			diagnostics.Add(ErrorCode.WRN_TranslationFileTargetLanguageInvalid, stringLocation2, filePath, cultureText);
			return false;
		}
		return true;
	}
}
