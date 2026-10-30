using System;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

public interface ITranslationExtractor
{
	void FinalizeExtraction(Action<CultureInfo, Stream> processTranslationFile);

	void StoreLabel(IElement element, PropertyType propertyType, string sourceString, string translatedString, string culture, string comment);
}
