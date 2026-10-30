using System;
using System.IO;
using System.Xml;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal class XmlDocumentationCommentTextReader
{
	internal sealed class Reader : TextReader
	{
		private string _text;

		private int _position;

		private const int maxReadsPastTheEnd = 100;

		private int _readsPastTheEnd;

		private static readonly string s_rootElementName = "_" + Guid.NewGuid().ToString("N");

		private static readonly string s_currentElementName = "_" + Guid.NewGuid().ToString("N");

		internal static readonly string RootStart = "<" + s_rootElementName + ">";

		internal static readonly string CurrentStart = "<" + s_currentElementName + ">";

		internal static readonly string CurrentEnd = "</" + s_currentElementName + ">";

		internal int Position => _position;

		public bool Eof => _readsPastTheEnd >= 100;

		public void Reset()
		{
			_text = null;
			_position = 0;
			_readsPastTheEnd = 0;
		}

		public void SetText(string text)
		{
			_text = text;
			_readsPastTheEnd = 0;
			if (_position > 0)
			{
				_position = RootStart.Length;
			}
		}

		public static bool ReachedEnd(XmlReader reader)
		{
			if (reader.Depth == 1 && reader.NodeType == XmlNodeType.EndElement)
			{
				return reader.Name == s_currentElementName;
			}
			return false;
		}

		public override int Read(char[] buffer, int index, int count)
		{
			if (count == 0 || Eof)
			{
				return 0;
			}
			int num = count;
			_position += EncodeAndAdvance(RootStart, _position, buffer, ref index, ref count);
			_position += EncodeAndAdvance(CurrentStart, _position - RootStart.Length, buffer, ref index, ref count);
			_position += EncodeAndAdvance(_text, _position - RootStart.Length - CurrentStart.Length, buffer, ref index, ref count);
			_position += EncodeAndAdvance(CurrentEnd, _position - RootStart.Length - CurrentStart.Length - _text.Length, buffer, ref index, ref count);
			if (num == count)
			{
				_readsPastTheEnd++;
				buffer[index] = ' ';
				count--;
			}
			return num - count;
		}

		private static int EncodeAndAdvance(string src, int srcIndex, char[] dest, ref int destIndex, ref int destCount)
		{
			if (destCount == 0 || srcIndex < 0 || srcIndex >= src.Length)
			{
				return 0;
			}
			int num = Math.Min(src.Length - srcIndex, destCount);
			src.CopyTo(srcIndex, dest, destIndex, num);
			destIndex += num;
			destCount -= num;
			return num;
		}

		public override int Read()
		{
			throw ExceptionUtilities.Unreachable;
		}

		public override int Peek()
		{
			throw ExceptionUtilities.Unreachable;
		}
	}

	private XmlReader _reader;

	private readonly Reader _textReader = new Reader();

	private static readonly ObjectPool<XmlDocumentationCommentTextReader> s_pool = new ObjectPool<XmlDocumentationCommentTextReader>(() => new XmlDocumentationCommentTextReader(), 2);

	private static readonly XmlReaderSettings s_xmlSettings = new XmlReaderSettings
	{
		DtdProcessing = DtdProcessing.Prohibit
	};

	public static XmlException ParseAndGetException(string text)
	{
		XmlDocumentationCommentTextReader xmlDocumentationCommentTextReader = s_pool.Allocate();
		XmlException result = xmlDocumentationCommentTextReader.ParseInternal(text);
		s_pool.Free(xmlDocumentationCommentTextReader);
		return result;
	}

	internal XmlException ParseInternal(string text)
	{
		_textReader.SetText(text);
		if (_reader == null)
		{
			_reader = XmlReader.Create(_textReader, s_xmlSettings);
		}
		try
		{
			do
			{
				_reader.Read();
			}
			while (!Reader.ReachedEnd(_reader));
			if (_textReader.Eof)
			{
				_reader.Dispose();
				_reader = null;
				_textReader.Reset();
			}
			return null;
		}
		catch (XmlException result)
		{
			_reader.Dispose();
			_reader = null;
			_textReader.Reset();
			return result;
		}
	}
}
