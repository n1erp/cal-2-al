using System;
using System.IO;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class NavXmlPortTextAdapter : INavXmlPortAdapter, IDisposable
{
	private const int ZeroTerminator = 1;

	private NavXmlPort xmlPortInstance;

	private INavStreamWriter navStream;

	private StreamWriter unicodeStream;

	private TextEncoding? writeEncoding;

	protected string remainder = "";

	private StreamWriter UnicodeStream
	{
		get
		{
			if (unicodeStream == null)
			{
				Encoding encoding = ((xmlPortInstance.TextEncoding == TextEncoding.UTF8) ? Encoding.UTF8 : Encoding.Unicode);
				unicodeStream = new StreamWriter(xmlPortInstance.Destination.InternalStreamCheckedForWrite, encoding);
			}
			return unicodeStream;
		}
	}

	protected string FieldSeparator => xmlPortInstance.FieldSeparator;

	protected string FieldDelimiter => xmlPortInstance.FieldDelimiter;

	protected string LineSeparator => xmlPortInstance.RecordSeparator;

	protected string TableSeparator => xmlPortInstance.TableSeparator;

	protected bool UseUnicode
	{
		get
		{
			if (xmlPortInstance.TextEncoding != TextEncoding.UTF8)
			{
				return xmlPortInstance.TextEncoding == TextEncoding.UTF16;
			}
			return true;
		}
	}

	private TextEncoding TextEncoding => xmlPortInstance.TextEncoding;

	internal NavXmlPortTextAdapter(NavXmlPort xmlPortInstance)
	{
		navStream = xmlPortInstance.Destination;
		this.xmlPortInstance = xmlPortInstance;
	}

	public abstract void WriteAttributeString(NavXmlPortNode node, string value);

	public abstract void WriteNamespaceString(string prefix, string namespaceuri);

	public abstract void WriteString(string text, NavXmlPortNode node);

	public void WriteEndElement(XmlPortNodeSourceType sourceType)
	{
	}

	public void WriteRaw(string data)
	{
		WriteStringToStream(data, string.Empty);
	}

	public void WriteStartElement(string prefix, string localName, XmlPortNodeSourceType sourceType)
	{
	}

	public void WriteStartElement(string prefix, string localName, string ns, XmlPortNodeSourceType sourceType)
	{
	}

	public void WriteNewLine()
	{
		if (remainder == FieldSeparator)
		{
			remainder = string.Empty;
		}
		WriteStringToStream(string.Empty, LineSeparator);
	}

	public void WriteTableSeparator()
	{
		if (remainder == LineSeparator)
		{
			remainder = string.Empty;
		}
		if (!string.IsNullOrEmpty(TableSeparator))
		{
			WriteStringToStream(string.Empty, TableSeparator);
		}
	}

	public void Flush()
	{
		if (!string.IsNullOrEmpty(TableSeparator) && remainder == TableSeparator)
		{
			remainder = string.Empty;
		}
		if (UseUnicode)
		{
			UnicodeStream.Flush();
		}
	}

	public void Close()
	{
	}

	protected void WriteStringToStream(string writeNowTxt, string remainderTxt)
	{
		if (!string.IsNullOrEmpty(remainder))
		{
			InternalWriteStringToStream(remainder);
		}
		InternalWriteStringToStream(writeNowTxt);
		remainder = remainderTxt;
	}

	private void InternalWriteStringToStream(string txt)
	{
		if (!writeEncoding.HasValue)
		{
			writeEncoding = TextEncoding;
		}
		else if (writeEncoding != TextEncoding)
		{
			throw new NavNCLXmlPortNodeProcessException(Lang.XmlPortExportTextEncoding);
		}
		if (UseUnicode)
		{
			UnicodeStream.Write(txt);
			return;
		}
		byte[] array = NCLManagedAdapter.TextToByte(txt, TextEncoding);
		navStream.WriteBytes(array, array.Length - 1);
	}

	public void Dispose()
	{
		if (unicodeStream != null)
		{
			unicodeStream.Dispose();
		}
	}
}
