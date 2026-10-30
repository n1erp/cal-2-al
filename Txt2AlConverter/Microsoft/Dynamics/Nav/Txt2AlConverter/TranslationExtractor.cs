using System;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis.Translation;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter;

public class TranslationExtractor : ITranslationExtractor
{
	private readonly MultiLanguageOutputter multiLanguageOutputter;

	private readonly AlExportOptions alExportOptions;

	public TranslationExtractor(AlExportOptions alExportOptions)
	{
		this.alExportOptions = alExportOptions;
		multiLanguageOutputter = new MultiLanguageOutputter(() => this.alExportOptions.TranslationFileFormat switch
		{
			TranslationFileFormat.Lcg => (ILanguageFileOutputter)new LcgOutputter(), 
			TranslationFileFormat.Xliff => new XliffOutputter(), 
			_ => throw new ArgumentOutOfRangeException(this.alExportOptions.TranslationFileFormat.ToString()), 
		});
	}

	public void FinalizeExtraction(Action<CultureInfo, Stream> processTranslationFile)
	{
		multiLanguageOutputter.FinalizeExtraction(processTranslationFile);
	}

	public void StoreLabel(IElement element, PropertyType propertyType, string sourceString, string translatedString, string culture, string comment)
	{
		string rootObjectName = null;
		IElement root = AlUtilities.GetRoot(element);
		if (root.ElementTypeInfo.ElementType == ElementType.Modification)
		{
			rootObjectName = AlUtilities.GetExtensionObjectName(root, alExportOptions.GetReservedExtensionObjectId(root));
		}
		string translationItemId = TranslationUtilities.GetTranslationItemId(element, propertyType, shouldHashName: true, rootObjectName);
		string translationItemId2 = TranslationUtilities.GetTranslationItemId(element, propertyType, shouldHashName: false, rootObjectName);
		string legacyId = string.Empty;
		if (alExportOptions.AddLegacyTranslationInformation)
		{
			legacyId = TranslationUtilities.GetTranslationItemLegacyId(root, element, propertyType);
		}
		TranslationItem translationItem = TranslationItem.CreateTranslated(translationItemId, sourceString, translatedString, culture, null, comment, translationItemId2, legacyId);
		multiLanguageOutputter.WriteLabel(translationItem);
	}
}
