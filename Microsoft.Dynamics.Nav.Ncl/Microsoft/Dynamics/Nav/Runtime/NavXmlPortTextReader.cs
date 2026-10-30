using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavXmlPortTextReader : IDisposable
{
	private const string xmlHeaderString = "<?xml version=\"1.0\" encoding=\"UTF-16\" standalone=\"no\"?> ";

	private const string xmlTagBeginString = "<";

	private const string xmlTagEndString = ">";

	private const string xmlEmptyTagEndString = "/>";

	private const string xmlEndTagBeginString = "</";

	private const string xmlAttributeStart = "=\"";

	private const string xmlAttributeEnd = "\"";

	private const char EOS = '\uffff';

	private bool returnXmlHeader;

	private bool returnXmlEnd;

	private bool fixedFormat;

	private bool endOfInput;

	private bool hasFieldData;

	private string fieldSeparator;

	private string fieldDelimiter;

	private string recordSeparator;

	private string tableSeparator;

	private string multiTableSeparator;

	private bool tableSeparatorSeen;

	private bool recordSeparatorSeen;

	private bool fieldSeparatorSeen;

	private bool sameFieldAndRecordSeparator;

	private NavXmlPort xmlPort;

	private Stream inputStream;

	private StreamReader unicodeStream;

	private bool useUnicode;

	private List<char> lookAheadBuffer;

	private StringBuilder xmlBuffer;

	private byte[] rawBytesBuffer;

	private int rawBytesCnt;

	private bool singleByteEncoding;

	private byte[] readCharInBuffer;

	private char[] readCharOutBuffer;

	private TextEncoding? textEncoding;

	private List<NavXmlPortNode> tableNodes;

	private int currentTableNodeIdx;

	private TextEncoding NonUnicodeTextEncoding
	{
		get
		{
			if (!textEncoding.HasValue)
			{
				if (xmlPort.TextEncoding == TextEncoding.Windows)
				{
					textEncoding = TextEncoding.Windows;
				}
				else
				{
					textEncoding = TextEncoding.MSDos;
				}
			}
			return textEncoding.Value;
		}
	}

	internal NavXmlPortTextReader(NavXmlPort xmlPortInstance)
	{
		if (xmlPortInstance == null)
		{
			throw new ArgumentNullException("xmlPortInstance");
		}
		xmlPort = xmlPortInstance;
		fixedFormat = xmlPort.Metadata.Format == XmlPortNodeFormat.FixedText;
		useUnicode = xmlPort.TextEncoding == TextEncoding.UTF8 || xmlPort.TextEncoding == TextEncoding.UTF16;
		tableNodes = new List<NavXmlPortNode>();
		currentTableNodeIdx = 0;
		foreach (NavXmlPortNode node in xmlPort.Nodes)
		{
			if (node.MetaData.SourceType == XmlPortNodeSourceType.Table)
			{
				tableNodes.Add(node);
			}
		}
		tableSeparator = xmlPort.TableSeparator;
		recordSeparator = xmlPort.RecordSeparator;
		fieldSeparator = (fixedFormat ? string.Empty : xmlPort.FieldSeparator);
		fieldDelimiter = (fixedFormat ? string.Empty : xmlPort.FieldDelimiter);
		multiTableSeparator = ((tableNodes.Count > 1) ? xmlPort.TableSeparator : string.Empty);
		sameFieldAndRecordSeparator = fieldSeparator.Equals(recordSeparator);
		returnXmlHeader = true;
		returnXmlEnd = true;
		lookAheadBuffer = new List<char>();
		xmlBuffer = new StringBuilder(2048);
		rawBytesBuffer = new byte[2];
		singleByteEncoding = Encoding.Default.IsSingleByte;
		readCharInBuffer = new byte[1];
		readCharOutBuffer = new char[1];
	}

	internal string GetNextXmlBuffer()
	{
		NavCurrentThread.Session.ThrowSessionTerminatedExceptionIfStopping();
		if (endOfInput || TryReadChar('\uffff'))
		{
			if (returnXmlEnd)
			{
				returnXmlEnd = false;
				return "</" + xmlPort.Nodes[0].MetaData.NodeName + ">";
			}
			return null;
		}
		if (returnXmlHeader)
		{
			returnXmlHeader = false;
			return "<?xml version=\"1.0\" encoding=\"UTF-16\" standalone=\"no\"?> <" + xmlPort.Nodes[0].MetaData.NodeName + ">";
		}
		string text = ProcessRecord();
		if (text != null)
		{
			return text;
		}
		endOfInput = true;
		returnXmlEnd = false;
		return "</" + xmlPort.Nodes[0].MetaData.NodeName + ">";
	}

	private string ProcessRecord()
	{
		NavXmlPortNode navXmlPortNode = tableNodes[currentTableNodeIdx];
		recordSeparatorSeen = false;
		tableSeparatorSeen = false;
		xmlBuffer.Clear();
		xmlBuffer.Append("<" + navXmlPortNode.MetaData.NodeName);
		int num = navXmlPortNode.AttributeChildren.Count;
		int num2 = navXmlPortNode.ElementChildren.Count;
		if (endOfInput || (tableNodes.Count > 1 && TryRead(multiTableSeparator)))
		{
			num2 = 0;
			num = 0;
			tableSeparatorSeen = true;
			recordSeparatorSeen = true;
		}
		for (int i = 0; i < num; i++)
		{
			NavXmlPortNode navXmlPortNode2 = navXmlPortNode.AttributeChildren[i];
			if (navXmlPortNode2.MetaData.SourceType == XmlPortNodeSourceType.Field || navXmlPortNode2.MetaData.SourceType == XmlPortNodeSourceType.Text)
			{
				ParseAttribute(navXmlPortNode2);
				if (i == 0 && !hasFieldData)
				{
					return null;
				}
				if (i < num - 1)
				{
					ParseFieldSeparator();
				}
			}
		}
		xmlBuffer.Append((num2 == 0) ? "/>" : ">");
		int num3 = 0;
		while (num3 < num2)
		{
			NavXmlPortNode navXmlPortNode3 = navXmlPortNode.ElementChildren[num3];
			if (navXmlPortNode3.MetaData.SourceType == XmlPortNodeSourceType.Field || navXmlPortNode3.MetaData.SourceType == XmlPortNodeSourceType.Text)
			{
				ParseField(navXmlPortNode3);
				if (num3 == 0 && !hasFieldData)
				{
					return null;
				}
				if (num3 < num2 - 1 || navXmlPortNode3.MetaData.Unbound)
				{
					ParseFieldSeparator();
				}
				if (navXmlPortNode3.MetaData.Unbound && !RecordSeparatorSeen())
				{
					continue;
				}
			}
			num3++;
		}
		if (num2 > 0)
		{
			xmlBuffer.Append("</" + navXmlPortNode.MetaData.NodeName + ">");
		}
		if (!RecordSeparatorSeen() && !tableSeparatorSeen && !ReadToRecordSeparator() && !endOfInput)
		{
			return null;
		}
		if (tableNodes.Count > 1 && (tableSeparatorSeen || TryRead(tableSeparator)) && currentTableNodeIdx < tableNodes.Count - 1)
		{
			currentTableNodeIdx++;
		}
		return xmlBuffer.ToString();
	}

	private void ParseField(NavXmlPortNode node)
	{
		if (!recordSeparatorSeen)
		{
			xmlBuffer.Append("<" + node.MetaData.NodeName + ">");
			if (fixedFormat)
			{
				ReadFixedFormatField(node);
			}
			else
			{
				ReadVariableFormatField();
			}
			xmlBuffer.Append("</" + node.MetaData.NodeName + ">");
		}
	}

	private void ParseAttribute(NavXmlPortNode node)
	{
		xmlBuffer.Append(" " + node.MetaData.NodeName + "=\"");
		if (fixedFormat)
		{
			ReadFixedFormatField(node);
		}
		else
		{
			ReadVariableFormatField();
		}
		xmlBuffer.Append("\"");
	}

	private void ReadFixedFormatField(NavXmlPortNode node)
	{
		if (recordSeparatorSeen)
		{
			return;
		}
		hasFieldData = true;
		string text = recordSeparator;
		int length = text.Length;
		string text2 = multiTableSeparator;
		int length2 = text2.Length;
		int i = 0;
		for (int width = node.MetaData.Width; i < width; i++)
		{
			char c = ReadChar();
			if (c == '\uffff')
			{
				recordSeparatorSeen = true;
				hasFieldData = i > 0;
				break;
			}
			if (length > 0 && c == text[0] && (length == 1 || TryRead(text.Substring(1))))
			{
				recordSeparatorSeen = true;
				tableSeparatorSeen = TryReadDelimiterSubString(text2, text);
				break;
			}
			if (length2 > 0 && c == text2[0] && (length2 == 1 || TryRead(text2.Substring(1))))
			{
				recordSeparatorSeen = true;
				tableSeparatorSeen = true;
				break;
			}
			AddChar(c);
		}
	}

	private void ReadVariableFormatField(bool skipCharacters = false)
	{
		if (recordSeparatorSeen)
		{
			return;
		}
		bool flag = false;
		hasFieldData = true;
		fieldSeparatorSeen = false;
		if (!string.IsNullOrEmpty(fieldDelimiter))
		{
			if (TryRead(fieldDelimiter))
			{
				flag = true;
			}
			else if (endOfInput)
			{
				recordSeparatorSeen = true;
				hasFieldData = false;
				return;
			}
		}
		string text = (flag ? fieldDelimiter : fieldSeparator);
		int length = text.Length;
		string text2 = (flag ? string.Empty : recordSeparator);
		int length2 = text2.Length;
		string text3 = (flag ? string.Empty : multiTableSeparator);
		int length3 = text3.Length;
		while (true)
		{
			char c = ReadChar();
			if (c == '\uffff')
			{
				recordSeparatorSeen = true;
				break;
			}
			if (length > 0 && c == text[0] && (length == 1 || TryRead(text.Substring(1))))
			{
				fieldSeparatorSeen = !flag;
				if (skipCharacters && !flag)
				{
					recordSeparatorSeen = TryReadDelimiterSubString(text2, text);
				}
				break;
			}
			if (length2 > 0 && c == text2[0] && (length2 == 1 || TryRead(text2.Substring(1))))
			{
				recordSeparatorSeen = true;
				tableSeparatorSeen = TryReadDelimiterSubString(text3, text2);
				break;
			}
			if (length3 > 0 && c == text3[0] && (length3 == 1 || TryRead(text3.Substring(1))))
			{
				recordSeparatorSeen = true;
				tableSeparatorSeen = true;
				break;
			}
			if (!skipCharacters)
			{
				AddChar(c);
			}
		}
	}

	private bool TryReadDelimiterSubString(string delimiter2, string delimiter1)
	{
		if (delimiter2.StartsWith(delimiter1, StringComparison.Ordinal))
		{
			string value = delimiter2.Substring(delimiter1.Length);
			if (TryRead(value))
			{
				return true;
			}
		}
		return false;
	}

	private bool RecordSeparatorSeen()
	{
		if (!recordSeparatorSeen)
		{
			if (sameFieldAndRecordSeparator)
			{
				return fieldSeparatorSeen;
			}
			return false;
		}
		return true;
	}

	private void ParseFieldSeparator()
	{
		if (!fieldSeparatorSeen && !recordSeparatorSeen && !endOfInput)
		{
			if (TryRead(fieldSeparator))
			{
				fieldSeparatorSeen = true;
			}
			else if (TryRead(multiTableSeparator) || (string.IsNullOrEmpty(multiTableSeparator) && TryReadChar('\uffff')))
			{
				recordSeparatorSeen = true;
				tableSeparatorSeen = true;
			}
			else if (TryRead(recordSeparator))
			{
				recordSeparatorSeen = true;
			}
		}
	}

	private bool ReadToRecordSeparator()
	{
		if (string.IsNullOrEmpty(recordSeparator))
		{
			return true;
		}
		while (!tableSeparatorSeen && !RecordSeparatorSeen() && !endOfInput)
		{
			ReadVariableFormatField(skipCharacters: true);
		}
		if (!tableSeparatorSeen)
		{
			return RecordSeparatorSeen();
		}
		return true;
	}

	private bool TryRead(string value)
	{
		int length = value.Length;
		if (length == 0)
		{
			return false;
		}
		for (int i = 0; i < length; i++)
		{
			char c = ReadChar();
			if (c != value[i])
			{
				for (int j = 0; j < i; j++)
				{
					lookAheadBuffer.Add(value[j]);
				}
				lookAheadBuffer.Add(c);
				return false;
			}
		}
		return true;
	}

	private bool TryReadChar(char value)
	{
		char c = ReadChar();
		if (c != value)
		{
			lookAheadBuffer.Add(c);
			return false;
		}
		return true;
	}

	private char DBCS_ReadChar()
	{
		int num = inputStream.ReadByte();
		if (rawBytesCnt == 0)
		{
			if (num == -1)
			{
				endOfInput = true;
				return '\uffff';
			}
			rawBytesBuffer[rawBytesCnt++] = (byte)num;
			num = inputStream.ReadByte();
			if (num != -1)
			{
				rawBytesBuffer[rawBytesCnt++] = (byte)num;
			}
		}
		else
		{
			if (rawBytesCnt != 1)
			{
				throw new NavNCLXmlPortInvalidTextException();
			}
			if (num != -1)
			{
				rawBytesBuffer[rawBytesCnt++] = (byte)num;
			}
		}
		string text = NCLManagedAdapter.ByteToText(rawBytesBuffer, NonUnicodeTextEncoding);
		switch (text.Length)
		{
		case 1:
			rawBytesBuffer[0] = 0;
			rawBytesBuffer[1] = 0;
			rawBytesCnt = 0;
			break;
		case 2:
			rawBytesBuffer[0] = rawBytesBuffer[1];
			rawBytesBuffer[1] = 0;
			rawBytesCnt = 1;
			break;
		default:
			throw new NavNCLXmlPortInvalidTextException();
		}
		return text[0];
	}

	private char ReadChar()
	{
		if (lookAheadBuffer.Count > 0)
		{
			char result = lookAheadBuffer[0];
			lookAheadBuffer.RemoveAt(0);
			return result;
		}
		if (useUnicode)
		{
			if (unicodeStream.EndOfStream)
			{
				endOfInput = true;
				return '\uffff';
			}
			return (char)unicodeStream.Read();
		}
		if (singleByteEncoding)
		{
			if (inputStream.Read(readCharInBuffer, 0, readCharInBuffer.Length) == 0)
			{
				endOfInput = true;
				return '\uffff';
			}
			if (NonUnicodeTextEncoding == TextEncoding.Windows)
			{
				Encoding.Default.GetChars(readCharInBuffer, 0, readCharInBuffer.Length, readCharOutBuffer, 0);
				return readCharOutBuffer[0];
			}
			return NCLManagedAdapter.ByteToTextChar(readCharInBuffer[0]);
		}
		return DBCS_ReadChar();
	}

	private void AddChar(char c)
	{
		switch (c)
		{
		case '<':
			xmlBuffer.Append("&lt;");
			break;
		case '>':
			xmlBuffer.Append("&gt;");
			break;
		case '"':
			xmlBuffer.Append("&quot;");
			break;
		case '\'':
			xmlBuffer.Append("&apos;");
			break;
		case '&':
			xmlBuffer.Append("&amp;");
			break;
		default:
			xmlBuffer.Append(c);
			break;
		}
	}

	internal void ConvertTextToXml(NavScope parent)
	{
		inputStream = xmlPort.Source.InternalStreamCheckedForRead;
		if (useUnicode)
		{
			unicodeStream = new StreamReader(inputStream);
		}
		NavXmlPortTextToXmlStream stream = new NavXmlPortTextToXmlStream(this);
		xmlPort.Source = new NavInStream(parent, new NavStreamProvider(stream));
	}

	public void Dispose()
	{
		if (unicodeStream != null)
		{
			unicodeStream.Dispose();
		}
	}
}
