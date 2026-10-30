using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2013.Word;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class WordContentControlExtensions
{
	public static IEnumerable<OpenXmlElement> ContentControls(this OpenXmlPart part)
	{
		return part.RootElement.ContentControls();
	}

	public static IEnumerable<OpenXmlElement> ContentControls(this OpenXmlElement element)
	{
		return from e in element.Descendants()
			where e is SdtBlock || e is SdtRun || e is SdtCell
			select e;
	}

	public static IEnumerable<OpenXmlElement> ContentControls(this WordprocessingDocument document)
	{
		foreach (OpenXmlElement item in document.MainDocumentPart.ContentControls())
		{
			yield return item;
		}
		foreach (OpenXmlElement item2 in document.MainDocumentPart.HeaderParts.SelectMany((HeaderPart header) => header.ContentControls()))
		{
			yield return item2;
		}
		foreach (OpenXmlElement item3 in document.MainDocumentPart.FooterParts.SelectMany((FooterPart footer) => footer.ContentControls()))
		{
			yield return item3;
		}
		if (document.MainDocumentPart.FootnotesPart != null)
		{
			foreach (OpenXmlElement item4 in document.MainDocumentPart.FootnotesPart.ContentControls())
			{
				yield return item4;
			}
		}
		if (document.MainDocumentPart.EndnotesPart == null)
		{
			yield break;
		}
		foreach (OpenXmlElement item5 in document.MainDocumentPart.EndnotesPart.ContentControls())
		{
			yield return item5;
		}
	}

	public static IEnumerable<OpenXmlElement> RepeaterControls(this OpenXmlPart part)
	{
		return part.RootElement.RepeaterControls();
	}

	public static IEnumerable<OpenXmlElement> RepeaterControls(this OpenXmlElement element)
	{
		return from repeaterSectionElement in element.Descendants()
			where repeaterSectionElement is SdtRepeatedSection
			select repeaterSectionElement.Parent.Parent;
	}

	public static IEnumerable<OpenXmlElement> RepeaterControls(this WordprocessingDocument document)
	{
		foreach (OpenXmlElement item in document.MainDocumentPart.RepeaterControls())
		{
			yield return item;
		}
		foreach (OpenXmlElement item2 in document.MainDocumentPart.HeaderParts.SelectMany((HeaderPart header) => header.RepeaterControls()))
		{
			yield return item2;
		}
		foreach (OpenXmlElement item3 in document.MainDocumentPart.FooterParts.SelectMany((FooterPart footer) => footer.RepeaterControls()))
		{
			yield return item3;
		}
		if (document.MainDocumentPart.FootnotesPart != null)
		{
			foreach (OpenXmlElement item4 in document.MainDocumentPart.FootnotesPart.RepeaterControls())
			{
				yield return item4;
			}
		}
		if (document.MainDocumentPart.EndnotesPart == null)
		{
			yield break;
		}
		foreach (OpenXmlElement item5 in document.MainDocumentPart.EndnotesPart.RepeaterControls())
		{
			yield return item5;
		}
	}
}
