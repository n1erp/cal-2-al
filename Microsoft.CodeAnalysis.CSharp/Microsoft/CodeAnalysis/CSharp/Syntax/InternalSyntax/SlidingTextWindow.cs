using System;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class SlidingTextWindow : IDisposable
{
	public const char InvalidCharacter = '\uffff';

	private const int DefaultWindowLength = 2048;

	private readonly SourceText _text;

	private int _basis;

	private int _offset;

	private readonly int _textEnd;

	private char[] _characterWindow;

	private int _characterWindowCount;

	private int _lexemeStart;

	private readonly StringTable _strings;

	private static readonly ObjectPool<char[]> s_windowPool = new ObjectPool<char[]>(() => new char[2048]);

	public SourceText Text => _text;

	public int Position => _basis + _offset;

	public int Offset => _offset;

	public char[] CharacterWindow => _characterWindow;

	public int LexemeRelativeStart => _lexemeStart;

	public int CharacterWindowCount => _characterWindowCount;

	public int LexemeStartPosition => _basis + _lexemeStart;

	public int Width => _offset - _lexemeStart;

	public SlidingTextWindow(SourceText text)
	{
		_text = text;
		_basis = 0;
		_offset = 0;
		_textEnd = text.Length;
		_strings = StringTable.GetInstance();
		_characterWindow = s_windowPool.Allocate();
		_lexemeStart = 0;
	}

	public void Dispose()
	{
		if (_characterWindow != null)
		{
			s_windowPool.Free(_characterWindow);
			_characterWindow = null;
			_strings.Free();
		}
	}

	public void Start()
	{
		_lexemeStart = _offset;
	}

	public void Reset(int position)
	{
		int num = position - _basis;
		if (num >= 0 && num <= _characterWindowCount)
		{
			_offset = num;
			return;
		}
		int val = Math.Min(_text.Length, position + _characterWindow.Length) - position;
		val = Math.Max(val, 0);
		if (val > 0)
		{
			_text.CopyTo(position, _characterWindow, 0, val);
		}
		_lexemeStart = 0;
		_offset = 0;
		_basis = position;
		_characterWindowCount = val;
	}

	private bool MoreChars()
	{
		if (_offset >= _characterWindowCount)
		{
			if (Position >= _textEnd)
			{
				return false;
			}
			if (_lexemeStart > _characterWindowCount / 4)
			{
				Array.Copy(_characterWindow, _lexemeStart, _characterWindow, 0, _characterWindowCount - _lexemeStart);
				_characterWindowCount -= _lexemeStart;
				_offset -= _lexemeStart;
				_basis += _lexemeStart;
				_lexemeStart = 0;
			}
			if (_characterWindowCount >= _characterWindow.Length)
			{
				char[] characterWindow = _characterWindow;
				char[] array = new char[_characterWindow.Length * 2];
				Array.Copy(characterWindow, 0, array, 0, _characterWindowCount);
				_characterWindow = array;
			}
			int num = Math.Min(_textEnd - (_basis + _characterWindowCount), _characterWindow.Length - _characterWindowCount);
			_text.CopyTo(_basis + _characterWindowCount, _characterWindow, _characterWindowCount, num);
			_characterWindowCount += num;
			return num > 0;
		}
		return true;
	}

	internal bool IsReallyAtEnd()
	{
		if (_offset >= _characterWindowCount)
		{
			return Position >= _textEnd;
		}
		return false;
	}

	public void AdvanceChar()
	{
		_offset++;
	}

	public void AdvanceChar(int n)
	{
		_offset += n;
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
		if (_offset >= _characterWindowCount && !MoreChars())
		{
			return '\uffff';
		}
		return _characterWindow[_offset];
	}

	public char PeekChar(int delta)
	{
		int position = Position;
		AdvanceChar(delta);
		char result = ((_offset < _characterWindowCount || MoreChars()) ? _characterWindow[_offset] : '\uffff');
		Reset(position);
		return result;
	}

	public bool IsUnicodeEscape()
	{
		if (PeekChar() == '\\')
		{
			char c = PeekChar(1);
			if (c == 'U' || c == 'u')
			{
				return true;
			}
		}
		return false;
	}

	public char PeekCharOrUnicodeEscape(out char surrogateCharacter)
	{
		if (IsUnicodeEscape())
		{
			return PeekUnicodeEscape(out surrogateCharacter);
		}
		surrogateCharacter = '\uffff';
		return PeekChar();
	}

	public char PeekUnicodeEscape(out char surrogateCharacter)
	{
		int position = Position;
		char result = ScanUnicodeEscape(peek: true, out surrogateCharacter, out var _);
		Reset(position);
		return result;
	}

	public char NextCharOrUnicodeEscape(out char surrogateCharacter, out SyntaxDiagnosticInfo info)
	{
		char c = PeekChar();
		if (c == '\\')
		{
			char c2 = PeekChar(1);
			if (c2 == 'U' || c2 == 'u')
			{
				return ScanUnicodeEscape(peek: false, out surrogateCharacter, out info);
			}
		}
		surrogateCharacter = '\uffff';
		info = null;
		AdvanceChar();
		return c;
	}

	public char NextUnicodeEscape(out char surrogateCharacter, out SyntaxDiagnosticInfo info)
	{
		return ScanUnicodeEscape(peek: false, out surrogateCharacter, out info);
	}

	private char ScanUnicodeEscape(bool peek, out char surrogateCharacter, out SyntaxDiagnosticInfo info)
	{
		surrogateCharacter = '\uffff';
		info = null;
		int position = Position;
		PeekChar();
		AdvanceChar();
		char c = PeekChar();
		if (c == 'U')
		{
			uint num = 0u;
			AdvanceChar();
			if (!SyntaxFacts.IsHexDigit(PeekChar()))
			{
				if (!peek)
				{
					info = CreateIllegalEscapeDiagnostic(position);
				}
			}
			else
			{
				for (int i = 0; i < 8; i++)
				{
					c = PeekChar();
					if (!SyntaxFacts.IsHexDigit(c))
					{
						if (!peek)
						{
							info = CreateIllegalEscapeDiagnostic(position);
						}
						break;
					}
					num = (uint)((num << 4) + SyntaxFacts.HexValue(c));
					AdvanceChar();
				}
				if (num > 1114111)
				{
					if (!peek)
					{
						info = CreateIllegalEscapeDiagnostic(position);
					}
				}
				else
				{
					c = GetCharsFromUtf32(num, out surrogateCharacter);
				}
			}
		}
		else
		{
			int num2 = 0;
			AdvanceChar();
			if (!SyntaxFacts.IsHexDigit(PeekChar()))
			{
				if (!peek)
				{
					info = CreateIllegalEscapeDiagnostic(position);
				}
			}
			else
			{
				for (int j = 0; j < 4; j++)
				{
					char c2 = PeekChar();
					if (!SyntaxFacts.IsHexDigit(c2))
					{
						if (c == 'u' && !peek)
						{
							info = CreateIllegalEscapeDiagnostic(position);
						}
						break;
					}
					num2 = (num2 << 4) + SyntaxFacts.HexValue(c2);
					AdvanceChar();
				}
				c = (char)num2;
			}
		}
		return c;
	}

	public bool TryScanXmlEntity(out char ch, out char surrogate)
	{
		ch = '&';
		AdvanceChar();
		surrogate = '\uffff';
		switch (PeekChar())
		{
		case 'l':
			if (AdvanceIfMatches("lt;"))
			{
				ch = '<';
				return true;
			}
			break;
		case 'g':
			if (AdvanceIfMatches("gt;"))
			{
				ch = '>';
				return true;
			}
			break;
		case 'a':
			if (AdvanceIfMatches("amp;"))
			{
				ch = '&';
				return true;
			}
			if (AdvanceIfMatches("apos;"))
			{
				ch = '\'';
				return true;
			}
			break;
		case 'q':
			if (AdvanceIfMatches("quot;"))
			{
				ch = '"';
				return true;
			}
			break;
		case '#':
		{
			AdvanceChar();
			uint num = 0u;
			if (AdvanceIfMatches("x"))
			{
				char c;
				while (SyntaxFacts.IsHexDigit(c = PeekChar()))
				{
					AdvanceChar();
					if (num <= 134217727)
					{
						num = (num << 4) + (uint)SyntaxFacts.HexValue(c);
						continue;
					}
					return false;
				}
			}
			else
			{
				char c2;
				while (SyntaxFacts.IsDecDigit(c2 = PeekChar()))
				{
					AdvanceChar();
					if (num <= 134217727)
					{
						num = (num << 3) + (num << 1) + (uint)SyntaxFacts.DecValue(c2);
						continue;
					}
					return false;
				}
			}
			if (AdvanceIfMatches(";"))
			{
				ch = GetCharsFromUtf32(num, out surrogate);
				return true;
			}
			break;
		}
		}
		return false;
	}

	private bool AdvanceIfMatches(string desired)
	{
		int length = desired.Length;
		for (int i = 0; i < length; i++)
		{
			if (PeekChar(i) != desired[i])
			{
				return false;
			}
		}
		AdvanceChar(length);
		return true;
	}

	private SyntaxDiagnosticInfo CreateIllegalEscapeDiagnostic(int start)
	{
		return new SyntaxDiagnosticInfo(start - LexemeStartPosition, Position - start, ErrorCode.ERR_IllegalEscape);
	}

	public string Intern(StringBuilder text)
	{
		return _strings.Add(text);
	}

	public string Intern(char[] array, int start, int length)
	{
		return _strings.Add(array, start, length);
	}

	public string GetInternedText()
	{
		return Intern(_characterWindow, _lexemeStart, Width);
	}

	public string GetText(bool intern)
	{
		return GetText(LexemeStartPosition, Width, intern);
	}

	public string GetText(int position, int length, bool intern)
	{
		int num = position - _basis;
		switch (length)
		{
		case 0:
			return string.Empty;
		case 1:
			if (_characterWindow[num] == ' ')
			{
				return " ";
			}
			if (_characterWindow[num] == '\n')
			{
				return "\n";
			}
			break;
		case 2:
		{
			char c = _characterWindow[num];
			if (c == '\r' && _characterWindow[num + 1] == '\n')
			{
				return "\r\n";
			}
			if (c == '/' && _characterWindow[num + 1] == '/')
			{
				return "//";
			}
			break;
		}
		case 3:
			if (_characterWindow[num] == '/' && _characterWindow[num + 1] == '/' && _characterWindow[num + 2] == ' ')
			{
				return "// ";
			}
			break;
		}
		if (intern)
		{
			return Intern(_characterWindow, num, length);
		}
		return new string(_characterWindow, num, length);
	}

	internal static char GetCharsFromUtf32(uint codepoint, out char lowSurrogate)
	{
		if (codepoint < 65536)
		{
			lowSurrogate = '\uffff';
			return (char)codepoint;
		}
		lowSurrogate = (char)((codepoint - 65536) % 1024 + 56320);
		return (char)((codepoint - 65536) / 1024 + 55296);
	}
}
