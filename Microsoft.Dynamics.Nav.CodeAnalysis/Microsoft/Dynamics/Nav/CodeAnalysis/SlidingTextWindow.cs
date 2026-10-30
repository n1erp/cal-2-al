using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class SlidingTextWindow
{
	public const char InvalidCharacter = '\uffff';

	private const int DefaultWindowLength = 2048;

	private readonly SourceText text;

	private int basis;

	private int offset;

	private readonly int textEnd;

	private char[] characterWindow;

	private int characterWindowCount;

	private int lexemeStart;

	public SourceText Text => text;

	public int Position => basis + offset;

	public int Offset => offset;

	public char[] CharacterWindow => characterWindow;

	public int LexemeRelativeStart => lexemeStart;

	public int CharacterWindowCount => characterWindowCount;

	public int LexemeStartPosition => basis + lexemeStart;

	public int Width => offset - lexemeStart;

	public SlidingTextWindow(SourceText text)
	{
		this.text = text;
		basis = 0;
		offset = 0;
		textEnd = text.Length;
		characterWindow = new char[2048];
		lexemeStart = 0;
	}

	public void Start()
	{
		lexemeStart = offset;
	}

	public void Reset(int position)
	{
		int num = position - basis;
		if (num >= 0 && num <= characterWindowCount)
		{
			offset = num;
			return;
		}
		int val = Math.Min(text.Length, position + characterWindow.Length) - position;
		val = Math.Max(val, 0);
		if (val > 0)
		{
			text.CopyTo(position, characterWindow, 0, val);
		}
		lexemeStart = 0;
		offset = 0;
		basis = position;
		characterWindowCount = val;
	}

	private bool MoreChars()
	{
		if (offset >= characterWindowCount)
		{
			if (Position >= textEnd)
			{
				return false;
			}
			if (lexemeStart > characterWindowCount / 4)
			{
				Array.Copy(characterWindow, lexemeStart, characterWindow, 0, characterWindowCount - lexemeStart);
				characterWindowCount -= lexemeStart;
				offset -= lexemeStart;
				basis += lexemeStart;
				lexemeStart = 0;
			}
			if (characterWindowCount >= characterWindow.Length)
			{
				char[] sourceArray = characterWindow;
				char[] destinationArray = new char[characterWindow.Length * 2];
				Array.Copy(sourceArray, 0, destinationArray, 0, characterWindowCount);
				characterWindow = destinationArray;
			}
			int num = Math.Min(textEnd - (basis + characterWindowCount), characterWindow.Length - characterWindowCount);
			text.CopyTo(basis + characterWindowCount, characterWindow, characterWindowCount, num);
			characterWindowCount += num;
			return num > 0;
		}
		return true;
	}

	internal bool IsReallyAtEnd()
	{
		if (offset >= characterWindowCount)
		{
			return Position >= textEnd;
		}
		return false;
	}

	public void AdvanceChar()
	{
		offset++;
	}

	public void AdvanceChar(int n)
	{
		offset += n;
	}

	public char NextChar()
	{
		char c = PeekChar();
		if (c != '\uffff')
		{
			AdvanceChar();
		}
		return c;
	}

	public char PeekChar()
	{
		if (offset >= characterWindowCount && !MoreChars())
		{
			return '\uffff';
		}
		return characterWindow[offset];
	}

	public char PeekChar(int delta)
	{
		int position = Position;
		AdvanceChar(delta);
		char result = ((offset < characterWindowCount || MoreChars()) ? characterWindow[offset] : '\uffff');
		Reset(position);
		return result;
	}

	public string GetText()
	{
		return GetText(LexemeStartPosition, Width);
	}

	public string GetText(int position, int length)
	{
		int num = position - basis;
		switch (length)
		{
		case 0:
			return string.Empty;
		case 1:
			if (characterWindow[num] == ' ')
			{
				return " ";
			}
			break;
		case 2:
		{
			char c = characterWindow[num];
			if (c == '\r' && characterWindow[num + 1] == '\n')
			{
				return "\r\n";
			}
			if (c == '/' && characterWindow[num + 1] == '/')
			{
				return "//";
			}
			break;
		}
		case 3:
			if (characterWindow[num] == '/' && characterWindow[num + 1] == '/' && characterWindow[num + 2] == ' ')
			{
				return "// ";
			}
			break;
		}
		return new string(characterWindow, num, length);
	}
}
