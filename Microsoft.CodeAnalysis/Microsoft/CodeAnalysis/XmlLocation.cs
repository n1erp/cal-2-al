using System;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis;

internal class XmlLocation : Location, IEquatable<XmlLocation>
{
	private readonly FileLinePositionSpan _positionSpan;

	public override LocationKind Kind => LocationKind.XmlFile;

	private XmlLocation(string path, int lineNumber, int columnNumber)
	{
		LinePosition start = new LinePosition(lineNumber, columnNumber);
		LinePosition end = new LinePosition(lineNumber, columnNumber + 1);
		_positionSpan = new FileLinePositionSpan(path, start, end);
	}

	public static XmlLocation Create(XmlException exception, string path)
	{
		int lineNumber = Math.Max(exception.LineNumber - 1, 0);
		int columnNumber = Math.Max(exception.LinePosition - 1, 0);
		return new XmlLocation(path, lineNumber, columnNumber);
	}

	public static XmlLocation Create(XObject obj, string path)
	{
		int lineNumber = Math.Max(((IXmlLineInfo)obj).LineNumber - 1, 0);
		int columnNumber = Math.Max(((IXmlLineInfo)obj).LinePosition - 1, 0);
		return new XmlLocation(path, lineNumber, columnNumber);
	}

	public override FileLinePositionSpan GetLineSpan()
	{
		return _positionSpan;
	}

	public bool Equals(XmlLocation other)
	{
		if ((object)this == other)
		{
			return true;
		}
		if (other != null)
		{
			return other._positionSpan.Equals(_positionSpan);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as XmlLocation);
	}

	public override int GetHashCode()
	{
		return _positionSpan.GetHashCode();
	}
}
