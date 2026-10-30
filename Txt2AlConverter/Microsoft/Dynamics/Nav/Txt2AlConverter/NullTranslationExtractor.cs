using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis.Translation;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

internal class NullTranslationExtractor : ITranslationExtractor
{
	public static ITranslationExtractor Instance { get; } = new NullTranslationExtractor();

	private List<TranslationItem> GetCultureList(Dictionary<string, List<TranslationItem>> translationItemsByCulture, string culture)
	{
		return new List<TranslationItem>();
	}

	public void FinalizeExtraction(Action<CultureInfo, Stream> processTranslationFile)
	{
	}

	public void StoreLabel(IElement element, PropertyType propertyType, string sourceString, string translatedString, string culture, string comment)
	{
	}
}
