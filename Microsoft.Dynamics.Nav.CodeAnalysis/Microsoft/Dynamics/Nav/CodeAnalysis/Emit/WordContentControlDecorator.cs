using System;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2013.Word;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class WordContentControlDecorator
{
	private const string NavDecoratorPrefix = "#Nav: ";

	public static bool DecorateContentControls(WordprocessingDocument wordProcessingDocument, string customXmlRootName, string customXmlNamespaceStart)
	{
		if (wordProcessingDocument == null)
		{
			return false;
		}
		bool seed = wordProcessingDocument.RepeaterControls().SelectMany((OpenXmlElement contentControl) => contentControl.Descendants<SdtProperties>()).Aggregate(seed: false, (bool current, SdtProperties sdtProperty) => current | DecorateContentElement(sdtProperty.GetFirstChild<DocumentFormat.OpenXml.Office2013.Word.DataBinding>(), sdtProperty, customXmlRootName, customXmlNamespaceStart));
		return wordProcessingDocument.ContentControls().SelectMany((OpenXmlElement contentControl) => contentControl.Descendants<SdtProperties>()).Aggregate(seed, (bool current, SdtProperties sdtProperty) => current | DecorateContentElement(sdtProperty.GetFirstChild<DocumentFormat.OpenXml.Wordprocessing.DataBinding>(), sdtProperty, customXmlRootName, customXmlNamespaceStart));
	}

	private static bool DecorateContentElement(OpenXmlElement dataBinding, SdtProperties sdtProperty, string customXmlRootName, string customXmlNamespaceStart)
	{
		if (dataBinding == null)
		{
			return false;
		}
		string prefixMapFromXmlElement = GetPrefixMapFromXmlElement(dataBinding);
		if (!WordDocumentXmlHelper.IsNavNameSpace(prefixMapFromXmlElement, customXmlNamespaceStart))
		{
			return false;
		}
		return (byte)(0u | (UpdateControlAlias(sdtProperty, dataBinding, customXmlRootName) ? 1u : 0u) | (UpdateContentControlTag(sdtProperty, prefixMapFromXmlElement, customXmlNamespaceStart) ? 1u : 0u)) != 0;
	}

	private static bool UpdateContentControlTag(SdtProperties sdtProperty, string prefixMap, string customXmlNamespaceStart)
	{
		bool flag = false;
		Tag tag = sdtProperty.GetFirstChild<Tag>();
		if (tag == null)
		{
			tag = new Tag();
			sdtProperty.AppendChild(tag);
			flag = true;
		}
		string text = WordDocumentXmlHelper.GetNamespaceFromPrefixMap(prefixMap);
		if (!string.IsNullOrEmpty(text))
		{
			text = text.Replace(customXmlNamespaceStart, string.Empty).TrimEnd('/');
		}
		string newValue = "#Nav: " + text;
		return flag | UpdateStringValue(tag, newValue);
	}

	private static bool UpdateControlAlias(SdtProperties sdtProperty, OpenXmlElement dataBinding, string customXmlRootName)
	{
		bool flag = false;
		SdtAlias sdtAlias = sdtProperty.GetFirstChild<SdtAlias>();
		if (sdtAlias == null)
		{
			sdtAlias = new SdtAlias();
			sdtProperty.AppendChild(sdtAlias);
			flag = true;
		}
		string newValue = "#Nav: " + WordDocumentXmlHelper.NormalizeName(GetXPathFromXmlElement(dataBinding)).Replace("/" + customXmlRootName, string.Empty);
		return flag | UpdateStringValue(sdtAlias, newValue);
	}

	private static bool UpdateStringValue(StringType stringElement, string newValue)
	{
		string text = stringElement.Val;
		if (string.IsNullOrEmpty(text) || (text.StartsWith("#Nav: ", StringComparison.CurrentCulture) && !text.Equals(newValue, StringComparison.CurrentCulture)))
		{
			stringElement.Val = newValue;
			return true;
		}
		return false;
	}

	private static string GetXPathFromXmlElement(OpenXmlElement binding)
	{
		if (binding is DocumentFormat.OpenXml.Wordprocessing.DataBinding dataBinding)
		{
			return dataBinding.XPath.Value;
		}
		if (binding is DocumentFormat.OpenXml.Office2013.Word.DataBinding dataBinding2)
		{
			return dataBinding2.XPath.Value;
		}
		return string.Empty;
	}

	private static string GetPrefixMapFromXmlElement(OpenXmlElement binding)
	{
		if (binding is DocumentFormat.OpenXml.Wordprocessing.DataBinding dataBinding)
		{
			return dataBinding.PrefixMappings.Value;
		}
		if (binding is DocumentFormat.OpenXml.Office2013.Word.DataBinding dataBinding2)
		{
			return dataBinding2.PrefixMappings.Value;
		}
		return string.Empty;
	}
}
