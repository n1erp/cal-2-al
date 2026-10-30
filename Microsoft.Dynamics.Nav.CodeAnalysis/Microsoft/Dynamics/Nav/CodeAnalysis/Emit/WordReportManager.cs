using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class WordReportManager
{
	public bool UpdateWordDocumentLayout(Stream wordLayout, Stream outputStream, string oldXmlPart, string newXmlPart)
	{
		WordDocumentUpdate wordDocumentUpdate = new WordDocumentUpdate(string.Empty, XDocument.Parse(newXmlPart), "NavWordReportXmlPart", "urn:microsoft-dynamics-nav/reports/");
		XDocument oldXmlPart2 = (string.IsNullOrEmpty(oldXmlPart) ? null : XDocument.Parse(oldXmlPart));
		return wordDocumentUpdate.UpdateWordDocumentLayout(wordLayout, outputStream, oldXmlPart2);
	}

	public static void NewWordDocumentLayout(Stream outputStream, string xmlPart)
	{
		if (xmlPart == null)
		{
			throw new ArgumentNullException("xmlPart");
		}
		using MemoryStream memoryStream = new MemoryStream();
		StreamWriter streamWriter = new StreamWriter(memoryStream, Encoding.Unicode);
		streamWriter.Write(xmlPart);
		streamWriter.Flush();
		memoryStream.Seek(0L, SeekOrigin.Begin);
		WordDocumentNewHandler.EmptyDocument(memoryStream, outputStream);
		if (outputStream.CanSeek)
		{
			outputStream.Seek(0L, SeekOrigin.Begin);
		}
	}
}
