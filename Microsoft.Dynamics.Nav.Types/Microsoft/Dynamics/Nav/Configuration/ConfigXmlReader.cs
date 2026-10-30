using System.Configuration.Internal;
using System.IO;
using System.Xml;

namespace Microsoft.Dynamics.Nav.Configuration;

internal sealed class ConfigXmlReader : XmlTextReader, IConfigErrorInfo
{
	private readonly string filename;

	private readonly int lineOffset;

	int IConfigErrorInfo.LineNumber
	{
		get
		{
			if (lineOffset > 0)
			{
				return LineNumber + lineOffset;
			}
			return LineNumber;
		}
	}

	string IConfigErrorInfo.Filename => filename;

	internal ConfigXmlReader(TextReader xmlReader, string filename, int lineOffset)
		: base(xmlReader)
	{
		DtdProcessing = DtdProcessing.Prohibit;
		this.filename = filename;
		this.lineOffset = lineOffset;
	}

	internal ConfigXmlReader(Stream xmlStream, string filename, int lineOffset)
		: base(xmlStream)
	{
		DtdProcessing = DtdProcessing.Prohibit;
		this.filename = filename;
		this.lineOffset = lineOffset;
	}

	public bool MoveToNextElement()
	{
		while (Read() && NodeType != XmlNodeType.Element)
		{
		}
		return !EOF;
	}
}
