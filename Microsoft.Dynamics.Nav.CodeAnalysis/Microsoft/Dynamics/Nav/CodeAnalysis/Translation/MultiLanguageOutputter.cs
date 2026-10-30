using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Translation;

public class MultiLanguageOutputter
{
	private readonly Dictionary<string, List<TranslationItem>> translationItemsByCulture;

	private readonly Func<ILanguageFileOutputter> languageFileOutputterFactory;

	public MultiLanguageOutputter(Func<ILanguageFileOutputter> languageFileOutputterFactory)
	{
		this.languageFileOutputterFactory = languageFileOutputterFactory;
		translationItemsByCulture = new Dictionary<string, List<TranslationItem>>();
	}

	public void WriteLabel(TranslationItem translationItem)
	{
		if (translationItem.Culture != null)
		{
			List<TranslationItem> translationItemsForCulture = GetTranslationItemsForCulture(translationItem.Culture);
			lock (translationItemsForCulture)
			{
				translationItemsForCulture.Add(translationItem);
			}
		}
	}

	private List<TranslationItem> GetTranslationItemsForCulture(string cultureName)
	{
		if (!translationItemsByCulture.TryGetValue(cultureName, out var value))
		{
			lock (translationItemsByCulture)
			{
				if (!translationItemsByCulture.TryGetValue(cultureName, out value))
				{
					value = new List<TranslationItem>();
					translationItemsByCulture[cultureName] = value;
				}
			}
		}
		return value;
	}

	public void FinalizeExtraction(Action<CultureInfo, Stream> processTranslationFile)
	{
		foreach (KeyValuePair<string, List<TranslationItem>> cultureAndItems in translationItemsByCulture)
		{
			string key = cultureAndItems.Key;
			CultureInfo cultureInfo = CultureInfo.GetCultureInfo(key);
			ILanguageFileOutputter languageFileOutputter = languageFileOutputterFactory();
			using Stream stream = new MemoryStream();
			XmlWriter writer = XmlWriter.Create(stream, LanguageFileOutputter.DefaultXmlWriterSettings);
			try
			{
				languageFileOutputter.WriteDefaultNodesAndContinue(writer, "Please update this value", key, () =>
				{
					foreach (TranslationItem item in cultureAndItems.Value)
					{
						languageFileOutputter.WriteLabel(writer, item);
					}
				});
				processTranslationFile(cultureInfo, stream);
			}
			finally
			{
				if (writer != null)
				{
					((IDisposable)writer).Dispose();
				}
			}
		}
	}
}
