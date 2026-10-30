using System;
using System.IO;
using System.Text;

namespace Microsoft.Dynamics.Nav.Model.Parser;

internal class SlidingTextWindow : IDisposable
{
	public const char InvalidCharacter = '\uffff';

	private readonly StreamReader reader;

	private int offset;

	private readonly string charWindow;

	private readonly int charWindowCount;

	private int lexemeStart;

	public int Width => offset - lexemeStart;

	public int Position => offset;

	public int LexemeStartPosition => lexemeStart;

	public SlidingTextWindow(Stream stream, Encoding encoding)
	{
		reader = new StreamReader(stream, encoding);
		charWindow = reader.ReadToEnd();
		charWindowCount = charWindow.Length;
	}

	public void Dispose()
	{
		reader.Dispose();
		GC.SuppressFinalize(this);
	}

	private bool MoreChars()
	{
		if (offset >= charWindowCount)
		{
			return false;
		}
		return true;
	}

	public char PeekChar()
	{
		if (offset >= charWindowCount && !MoreChars())
		{
			return '\uffff';
		}
		return charWindow[offset];
	}

	public char PeekChar(int delta)
	{
		int position = Position;
		AdvanceChar(delta);
		char result = ((offset < charWindowCount || MoreChars()) ? charWindow[offset] : '\uffff');
		Reset(position);
		return result;
	}

	public void AdvanceChar()
	{
		offset++;
	}

	public void AdvanceChar(int n)
	{
		offset += n;
	}

	public void Start()
	{
		lexemeStart = offset;
	}

	public void Reset(int position)
	{
		offset = position;
	}

	public string GetText()
	{
		return GetText(LexemeStartPosition, Width);
	}

	public string GetText(int position, int length)
	{
		switch (length)
		{
		case 0:
			return string.Empty;
		case 1:
			if (charWindow[position] == ' ')
			{
				return " ";
			}
			break;
		case 2:
		{
			char c = charWindow[position];
			if (c == '\r' && charWindow[position + 1] == '\n')
			{
				return "\r\n";
			}
			if (c == '/' && charWindow[position + 1] == '/')
			{
				return "//";
			}
			break;
		}
		case 3:
			if (charWindow[position] == '/' && charWindow[position + 1] == '/' && charWindow[position + 2] == ' ')
			{
				return "// ";
			}
			break;
		}
		return charWindow.Substring(position, length);
	}
}
