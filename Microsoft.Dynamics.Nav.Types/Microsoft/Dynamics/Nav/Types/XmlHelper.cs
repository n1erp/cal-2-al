using System.IO;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Types;

public static class XmlHelper
{
	internal static XmlReader SafeXmlReader(TextReader textReader, bool prohibitDtdProcessing)
	{
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.DtdProcessing = ((!prohibitDtdProcessing) ? DtdProcessing.Ignore : DtdProcessing.Prohibit);
		xmlReaderSettings.XmlResolver = null;
		return XmlReader.Create(textReader, xmlReaderSettings);
	}

	public static XmlReader SafeXmlReader(TextReader textReader)
	{
		return SafeXmlReader(textReader, prohibitDtdProcessing: true);
	}

	internal static XmlReader SafeXmlReader(Stream documentStream, bool prohibitDtdProcessing)
	{
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.DtdProcessing = ((!prohibitDtdProcessing) ? DtdProcessing.Ignore : DtdProcessing.Prohibit);
		xmlReaderSettings.XmlResolver = null;
		return XmlReader.Create(documentStream, xmlReaderSettings);
	}

	public static XmlReader SafeXmlReader(Stream documentStream)
	{
		return SafeXmlReader(documentStream, prohibitDtdProcessing: true);
	}

	internal static XmlReader SafeXmlReader(string documentPath)
	{
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.DtdProcessing = DtdProcessing.Prohibit;
		xmlReaderSettings.XmlResolver = null;
		return XmlReader.Create(documentPath, xmlReaderSettings);
	}

	public static XmlDocument SafeXmlDocument()
	{
		return new XmlDocument
		{
			XmlResolver = null
		};
	}

	public static XmlDocument SafeXmlLoader(XmlDocument xmlDocument, string xmlDocumentData, bool preserveWhiteSpace = false)
	{
		if (xmlDocument == null)
		{
			xmlDocument = SafeXmlDocument();
			xmlDocument.PreserveWhitespace = preserveWhiteSpace;
		}
		using StringReader textReader = new StringReader(xmlDocumentData);
		using XmlReader reader = SafeXmlReader(textReader);
		xmlDocument.Load(reader);
		return xmlDocument;
	}
}
