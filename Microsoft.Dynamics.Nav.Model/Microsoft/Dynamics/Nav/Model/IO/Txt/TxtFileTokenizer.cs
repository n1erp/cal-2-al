using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

public class TxtFileTokenizer
{
	private const char CharZero = '\0';

	private const char CharEndOfBuffer = '\0';

	private const int BufSize = 32768;

	private const int TabWith = 8;

	private readonly StreamReader inputStream;

	private char[] buf;

	private int tokenStartIndex;

	private int tokenLastIndex;

	private int lineTabCount;

	private int lineStartIndex;

	private char tokenStringDupChar;

	private readonly StringBuilder stringBuilder = new StringBuilder(1024);

	private TokenizerOptions tokenOptions;

	private bool endOfLine;

	private bool endOfStream;

	private static StringLookup<Token> keywords;

	public const string NavNewLine = "\r\n";

	public TokenizerOptions TokenOptions
	{
		get
		{
			return tokenOptions;
		}
		set
		{
			tokenOptions = value;
		}
	}

	public bool EndOfStream => endOfStream;

	public int LineNo { get; private set; }

	public int LinePos => tokenStartIndex - lineStartIndex + 1;

	public int LineIndentation => LinePos - 1 - lineTabCount + lineTabCount * 8;

	public string TokenAsProcessedString
	{
		get
		{
			if (TokenLength >= 2)
			{
				char c = buf[tokenStartIndex];
				if ((c == '"' || c == '\'') && buf[tokenLastIndex - 1] == c)
				{
					if (c != tokenStringDupChar)
					{
						return new string(buf, tokenStartIndex + 1, TokenLength - 2);
					}
					char[] array = new char[TokenLength];
					int length = 0;
					for (int i = tokenStartIndex + 1; i < tokenLastIndex - 1; i++)
					{
						if (buf[i] == c && buf[i + 1] == c)
						{
							i++;
						}
						array[length++] = buf[i];
					}
					return new string(array, 0, length);
				}
			}
			return TokenAsString;
		}
	}

	public string TokenAsString => new string(buf, tokenStartIndex, TokenLength);

	public int TokenLength => tokenLastIndex - tokenStartIndex;

	public char TokenFirstChar => buf[tokenStartIndex];

	public int TokenAsInt32
	{
		get
		{
			int num = 0;
			for (int i = tokenStartIndex; i < tokenLastIndex; i++)
			{
				num = (num << 3) + (num << 1) + buf[i] - 48;
			}
			return num;
		}
	}

	private static void InitKeyWords()
	{
		if (keywords == null)
		{
			keywords = new StringLookup<Token>(from pair in EnumHelper<Token>.GetKeyValuePairs()
				where pair.Key > Token.InternalGroupKeyword && pair.Value[1] != '_'
				select new KeyValuePair<string, Token>(pair.Value, pair.Key));
		}
	}

	public TxtFileTokenizer(Stream stream, Encoding encoding)
	{
		InitKeyWords();
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		inputStream = new StreamReader(stream, encoding);
		buf = new char[32770];
		buf[0] = '\0';
		tokenStartIndex = 0;
		tokenLastIndex = 0;
		tokenOptions = TokenizerOptions.SkipWhiteSpace | TokenizerOptions.SkipLineBreak | TokenizerOptions.NoBracketProcessing;
		endOfStream = false;
		LineNo = 1;
	}

	public int GetTokenLinePos()
	{
		int num = tokenStartIndex;
		while (num > 0 && buf[num - 1] != '\r' && buf[num - 1] != '\n')
		{
			num--;
		}
		return tokenStartIndex - num;
	}

	public string GetTokenLine()
	{
		int num = tokenStartIndex;
		while (num > 0 && buf[num - 1] != '\r' && buf[num - 1] != '\n')
		{
			num--;
		}
		int i;
		for (i = ((tokenLastIndex > 0) ? (tokenLastIndex - 1) : 0); buf[i] != '\r' && buf[i] != '\n' && buf[i] != '\0'; i++)
		{
		}
		return new string(buf, num, i - num + 1);
	}

	private bool FillBuffer(ref int startOfToken, ref int index)
	{
		int num = index - lineStartIndex;
		for (int i = 0; i < num; i++)
		{
			buf[i] = buf[lineStartIndex + i];
		}
		int num2 = inputStream.Read(buf, num, 32768 - num);
		buf[num + num2] = '\0';
		startOfToken -= lineStartIndex;
		index -= lineStartIndex;
		lineStartIndex = 0;
		return num2 > 0;
	}

	public Token NextToken(TokenizerOptions options)
	{
		TokenizerOptions tokenizerOptions = tokenOptions;
		try
		{
			tokenOptions = options;
			return NextToken();
		}
		finally
		{
			tokenOptions = tokenizerOptions;
		}
	}

	public Token NextToken()
	{
		tokenStringDupChar = '\0';
		int startOfToken = tokenLastIndex;
		int num = lineTabCount;
		bool flag = endOfLine;
		int i;
		Token token;
		while (true)
		{
			if (flag)
			{
				LineNo++;
				lineStartIndex = startOfToken;
				lineTabCount = 0;
				num = 0;
				flag = false;
			}
			i = startOfToken;
			char c = buf[i++];
			switch (c)
			{
			case '\0':
				i--;
				token = Token.None;
				goto IL_0401;
			case '\t':
				num++;
				goto case '\f';
			case '\f':
			case ' ':
				while (true)
				{
					switch (buf[i])
					{
					case '\t':
						num++;
						goto IL_01ba;
					case '\f':
					case ' ':
						goto IL_01ba;
					}
					break;
					IL_01ba:
					i++;
				}
				token = (((TokenOptions & TokenizerOptions.SkipLeadingWhiteSpace) == 0) ? Token.WhiteSpace : Token.None);
				goto IL_0401;
			case '\n':
			case '\r':
				token = (((TokenOptions & TokenizerOptions.SkipLeadingLineBreak) == 0) ? Token.EndOfLine : Token.None);
				if (c == '\r' && buf[i] == '\n')
				{
					flag = true;
					i++;
					if (token != Token.None)
					{
						break;
					}
				}
				else if (c == '\n')
				{
					flag = true;
					if (token != Token.None)
					{
						break;
					}
				}
				goto IL_0401;
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				for (; char.IsDigit(buf[i]); i++)
				{
				}
				token = Token.Number;
				goto IL_0401;
			case '\'':
				if ((TokenOptions & TokenizerOptions.NoPingProcessing) != 0)
				{
					token = Token.Ping;
					break;
				}
				while (true)
				{
					char c2 = buf[i];
					if ((uint)c2 <= 10u)
					{
						if (c2 == '\0')
						{
							break;
						}
						if (c2 != '\n')
						{
							goto IL_02ba;
						}
					}
					else if (c2 != '\r')
					{
						if (c2 != '\'')
						{
							goto IL_02ba;
						}
						i++;
						char c3 = buf[i];
						if (c3 == '\0')
						{
							goto IL_02ac;
						}
						if (c3 == '\'')
						{
							tokenStringDupChar = '\'';
							goto IL_02ba;
						}
						goto IL_02b3;
					}
					flag = true;
					goto IL_02ba;
					IL_02ba:
					i++;
				}
				token = Token.OpenString;
				goto IL_0401;
			case '"':
				if ((TokenOptions & TokenizerOptions.NoPingProcessing) != 0)
				{
					token = Token.Quote;
					break;
				}
				while (true)
				{
					char c2 = buf[i];
					if ((uint)c2 <= 10u)
					{
						if (c2 == '\0')
						{
							break;
						}
						if (c2 != '\n')
						{
							goto IL_031e;
						}
					}
					else if (c2 != '\r')
					{
						if (c2 != '"')
						{
							goto IL_031e;
						}
						goto IL_0310;
					}
					goto IL_0307;
					IL_031e:
					i++;
				}
				token = Token.OpenIdentifier;
				goto IL_0401;
			case '{':
				token = Token.OpenCurlyBracket;
				break;
			case '}':
				token = Token.CloseCurlyBracket;
				break;
			case '[':
				token = Token.OpenSquareBracket;
				break;
			case ']':
				token = Token.CloseSquareBracket;
				break;
			case '(':
				token = Token.OpenParenthesis;
				break;
			case ')':
				token = Token.CloseParenthesis;
				break;
			case '-':
				token = Token.Minus;
				break;
			case '+':
				token = Token.Plus;
				break;
			case ';':
				token = Token.Semicolon;
				break;
			case ':':
				token = Token.Colon;
				break;
			case '=':
				token = Token.Equal;
				break;
			case '@':
				token = Token.At;
				break;
			case '.':
				token = Token.Point;
				break;
			case ',':
				token = Token.Comma;
				break;
			case '\u001a':
				token = Token.EndOfStream;
				break;
			default:
				{
					if (ModelTextHelper.IsNavLetter(c) || c == '_')
					{
						for (; ModelTextHelper.IsNavLetter(buf[i]) || char.IsDigit(buf[i]) || buf[i] == '_'; i++)
						{
						}
						token = (((TokenOptions & TokenizerOptions.NoKeywordLookup) == 0) ? LookUpKeyword(startOfToken, i) : Token.Identifier);
						goto IL_0401;
					}
					token = Token.UnknownChar;
					break;
				}
				IL_0401:
				if (buf[i] == '\0')
				{
					if (FillBuffer(ref startOfToken, ref i))
					{
						continue;
					}
					if (token == Token.None)
					{
						token = Token.EndOfStream;
						endOfStream = true;
					}
				}
				if (token == Token.None)
				{
					startOfToken = i;
					continue;
				}
				break;
				IL_02b3:
				token = Token.String;
				break;
				IL_02ac:
				token = Token.String;
				goto IL_0401;
				IL_0310:
				i++;
				token = Token.Identifier;
				break;
				IL_0307:
				flag = true;
				token = Token.OpenIdentifier;
				break;
			}
			break;
		}
		tokenStartIndex = startOfToken;
		tokenLastIndex = i;
		lineTabCount += num;
		endOfLine = flag;
		return token;
	}

	public string ReadToEndOfLineAsString()
	{
		TokenizerOptions tokenizerOptions = TokenOptions;
		try
		{
			TokenOptions = TokenizerOptions.NoPingProcessing | TokenizerOptions.NoKeywordLookup;
			stringBuilder.Clear();
			while (true)
			{
				Token token = NextToken();
				if (token == Token.EndOfStream || token == Token.EndOfLine)
				{
					break;
				}
				stringBuilder.Append(buf, tokenStartIndex, tokenLastIndex - tokenStartIndex);
			}
			return stringBuilder.ToString();
		}
		finally
		{
			TokenOptions = tokenizerOptions;
		}
	}

	public Token ReadUntilToken(out string result, Token stopToken)
	{
		return ReadUntilPredicate(out result, (Token t, StringBuilder sb) => t == stopToken);
	}

	public Token ReadUntilToken(out string result, Token stopToken1, Token stopToken2)
	{
		return ReadUntilPredicate(out result, (Token t, StringBuilder sb) => t == stopToken1 || t == stopToken2);
	}

	public Token ReadUntilToken(out string result, Token stopToken1, Token stopToken2, Token stopToken3)
	{
		return ReadUntilPredicate(out result, (Token t, StringBuilder sb) => t == stopToken1 || t == stopToken2 || t == stopToken3);
	}

	public Token ReadUntilToken(out string result, params Token[] stopTokens)
	{
		return ReadUntilPredicate(out result, (Token t, StringBuilder sb) => stopTokens.Any((Token st) => t == st));
	}

	public Token ReadUntilPredicate(out string result, Func<Token, StringBuilder, bool> predicate)
	{
		TokenizerOptions tokenizerOptions = TokenOptions;
		try
		{
			TokenOptions = TokenizerOptions.NoPingProcessing | TokenizerOptions.NoKeywordLookup;
			stringBuilder.Clear();
			Token token;
			while (true)
			{
				token = NextToken();
				if (stringBuilder.Length != 0 || ((token != Token.WhiteSpace || (tokenizerOptions & TokenizerOptions.SkipLeadingWhiteSpace) == 0) && (token != Token.EndOfLine || (tokenizerOptions & TokenizerOptions.SkipLeadingLineBreak) == 0)))
				{
					if (token == Token.EndOfStream || predicate(token, stringBuilder))
					{
						break;
					}
					stringBuilder.Append(buf, tokenStartIndex, tokenLastIndex - tokenStartIndex);
				}
			}
			TrimSb(stringBuilder, (tokenizerOptions & TokenizerOptions.TrimTrailingWhiteSpace) != 0, (tokenizerOptions & TokenizerOptions.TrimTrailingLineBreak) != 0);
			result = stringBuilder.ToString();
			return token;
		}
		finally
		{
			TokenOptions = tokenizerOptions;
		}
	}

	internal void RunWithTokenizerOptions(TokenizerOptions options, Action action)
	{
		TokenizerOptions tokenizerOptions = tokenOptions;
		try
		{
			tokenOptions = options;
			action();
		}
		finally
		{
			TokenOptions = tokenizerOptions;
		}
	}

	internal static string FormatToken(Token token)
	{
		switch (token)
		{
		case Token.OpenCurlyBracket:
			return "{";
		case Token.CloseCurlyBracket:
			return "}";
		case Token.OpenSquareBracket:
			return "[";
		case Token.CloseSquareBracket:
			return "]";
		case Token.OpenParenthesis:
			return "(";
		case Token.CloseParenthesis:
			return ")";
		case Token.Minus:
			return "-";
		case Token.Plus:
			return "+";
		case Token.Equal:
			return "=";
		case Token.Semicolon:
			return ";";
		case Token.Colon:
			return ":";
		case Token.Ping:
			return "'";
		case Token.Quote:
			return "\"";
		case Token.At:
			return "@";
		case Token.Point:
			return ".";
		case Token.Comma:
			return ",";
		case Token.EndOfStream:
		case Token.EndOfLine:
			return "<" + token.ToString() + ">";
		default:
			return token.ToString();
		}
	}

	private Token LookUpKeyword(int startOfIdentifier, int endIndex)
	{
		if ((tokenOptions & TokenizerOptions.NoKeywordLookup) == 0 && keywords.TryGetValue(buf, startOfIdentifier, endIndex - startOfIdentifier, out var value))
		{
			return value;
		}
		return Token.Identifier;
	}

	private static void TrimSb(StringBuilder sb, bool trimWhiteSpace, bool trimLineBreak)
	{
		while (sb.Length > 0)
		{
			switch (sb[sb.Length - 1])
			{
			case '\t':
			case '\f':
			case ' ':
				if (!trimWhiteSpace)
				{
					return;
				}
				sb.Length--;
				break;
			case '\n':
			case '\r':
				if (!trimLineBreak)
				{
					return;
				}
				sb.Length--;
				break;
			default:
				return;
			}
		}
	}
}
