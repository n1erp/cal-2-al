using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class WordCustomXml
{
	public static bool ReplaceCustomXmlPart(XDocument xmlPartDocumentDocument, WordprocessingDocument wordDoc, string customXmlRootName, string customXmlNamespaceStart)
	{
		bool result = false;
		XDocument xDocument = WordDocumentUpdate.StripCustomXmlPart(xmlPartDocumentDocument);
		string namespaceName = xDocument.Root.Name.NamespaceName;
		MainDocumentPart mainDocumentPart = wordDoc.MainDocumentPart;
		List<CustomXmlPart> list = new List<CustomXmlPart>();
		CustomXmlPart customXmlPart = null;
		foreach (CustomXmlPart customXmlPart2 in mainDocumentPart.CustomXmlParts)
		{
			string navNamespaceCustomXml = GetNavNamespaceCustomXml(customXmlPart2, customXmlRootName, customXmlNamespaceStart);
			if (navNamespaceCustomXml != null)
			{
				if (customXmlPart == null && string.Equals(navNamespaceCustomXml, namespaceName, StringComparison.Ordinal))
				{
					customXmlPart = customXmlPart2;
				}
				else
				{
					list.Add(customXmlPart2);
				}
			}
		}
		foreach (CustomXmlPart item in list)
		{
			mainDocumentPart.DeletePart(item);
			result = true;
		}
		if (customXmlPart != null)
		{
			XDocument xDocument2;
			using (Stream stream = customXmlPart.GetStream(FileMode.OpenOrCreate))
			{
				xDocument2 = XDocument.Load(stream);
			}
			if (xDocument.Equals(xDocument2) || xDocument.ToString(SaveOptions.DisableFormatting) == xDocument2.ToString(SaveOptions.DisableFormatting))
			{
				return result;
			}
		}
		using (Stream stream2 = (customXmlPart ?? mainDocumentPart.AddCustomXmlPart(CustomXmlPartType.CustomXml)).GetStream(FileMode.Create))
		{
			xDocument.Save(stream2);
		}
		return true;
	}

	public static string GetNavNamespaceCustomXml(OpenXmlPart xmlPart, string customXmlRootName, string customXmlNamespaceStart)
	{
		XDocument xDocument = xmlPart.GetXDocument();
		if (xDocument == null || xDocument.Root == null)
		{
			return null;
		}
		string localName = xDocument.Root.Name.LocalName;
		string namespaceName = xDocument.Root.Name.NamespaceName;
		if (localName != customXmlRootName || !namespaceName.StartsWith(customXmlNamespaceStart, StringComparison.Ordinal))
		{
			return null;
		}
		return namespaceName;
	}

	[SuppressMessage("Microsoft.Security.Xml", "CA3053:UseXmlSecureResolver")]
	private static XDocument GetXDocument(this OpenXmlPart part)
	{
		if (part == null)
		{
			return null;
		}
		XDocument xDocument = part.Annotation<XDocument>();
		if (xDocument != null)
		{
			return xDocument;
		}
		using (StreamReader input = new StreamReader(part.GetStream(FileMode.OpenOrCreate)))
		{
			using XmlReader reader = XmlReader.Create(input, new XmlReaderSettings
			{
				DtdProcessing = DtdProcessing.Prohibit,
				XmlResolver = null
			});
			xDocument = XDocument.Load(reader);
		}
		part.AddAnnotation(xDocument);
		return xDocument;
	}
}
