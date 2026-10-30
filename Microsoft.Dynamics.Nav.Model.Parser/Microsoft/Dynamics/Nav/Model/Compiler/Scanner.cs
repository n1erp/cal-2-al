using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Model.Parser;

namespace Microsoft.Dynamics.Nav.Model.Compiler;

public class Scanner : IDisposable
{
	private const int DefaultBufferSize = 10000;

	private static Dictionary<string, Token> keywords;

	private static readonly SortedSet<string> editorKeywords = new SortedSet<string>();

	private static SortedSet<Token> keywordsAllowsAsIdentifiers;

	private readonly bool enablePartialScan;

	private readonly bool skipWS;

	private SlidingTextWindow text;

	private int lineStartIndex;

	private int linePosition;

	public static IDictionary<string, Token> Keywords
	{
		get
		{
			InitKeyWords();
			return keywords;
		}
	}

	public int LineNo { get; private set; }

	public int Length => text.Width;

	public int LinePos => linePosition;

	public int StartIndex => text.LexemeStartPosition;

	public string TokenVal => text.GetText();

	private Scanner(bool skipWhitespace = true, int bufSize = 10000, bool enablePartialScan = false)
	{
		InitKeyWords();
		skipWS = skipWhitespace;
		linePosition = 0;
		this.enablePartialScan = enablePartialScan;
	}

	public Scanner(Stream stream, Encoding encoding, bool skipWhitespace = true, int bufSize = 10000, bool enablePartialScan = false)
		: this(skipWhitespace, bufSize, enablePartialScan)
	{
		Initialize(stream, encoding);
	}

	public static bool IsTokenIdentifier(Token token)
	{
		if (token != Token.t_id)
		{
			return keywordsAllowsAsIdentifiers.Contains(token);
		}
		return true;
	}

	public static bool IsKeyword(Token token)
	{
		return keywords.Values.Any((Token tempToken) => tempToken == token);
	}

	public void Dispose()
	{
		text.Dispose();
		GC.SuppressFinalize(this);
	}

	private void Initialize(Stream stream, Encoding encoding)
	{
		text = new SlidingTextWindow(stream, encoding);
		LineNo = 0;
		lineStartIndex = 0;
		linePosition = 0;
	}

	public Token NextToken()
	{
		text.Start();
		Token token;
		do
		{
			char c = text.PeekChar();
			switch (c)
			{
			case '\uffff':
				linePosition = text.Position - lineStartIndex;
				lineStartIndex = Math.Max(lineStartIndex - text.Position, 0);
				return Token.t_endofinput;
			case '\t':
			case '\f':
			case ' ':
				text.AdvanceChar();
				while ((c = text.PeekChar()) == ' ' || c == '\t' || c == '\f')
				{
					text.AdvanceChar();
				}
				token = Token.t_space;
				if (skipWS)
				{
					token = Token.t_none;
					text.Start();
				}
				continue;
			case '\r':
				text.AdvanceChar();
				if (text.PeekChar() == '\n')
				{
					text.AdvanceChar();
				}
				IncreaseLineNumber();
				token = Token.t_newline;
				if (skipWS)
				{
					token = Token.t_none;
					text.Start();
				}
				continue;
			case '\n':
				text.AdvanceChar();
				if (text.PeekChar() == '\r')
				{
					text.AdvanceChar();
				}
				IncreaseLineNumber();
				token = Token.t_newline;
				if (skipWS)
				{
					token = Token.t_none;
					text.Start();
				}
				continue;
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
				text.AdvanceChar();
				token = Token.t_s32;
				while (char.IsDigit(text.PeekChar()))
				{
					text.AdvanceChar();
				}
				if (text.PeekChar() == '\uffff')
				{
					continue;
				}
				if (text.PeekChar() == '.' && char.IsDigit(text.PeekChar(1)))
				{
					token = Token.t_bcd;
					text.AdvanceChar();
					while (char.IsDigit(text.PeekChar()))
					{
						text.AdvanceChar();
					}
					if (text.PeekChar() == 'T')
					{
						text.AdvanceChar();
						token = Token.t_time;
					}
				}
				else if (text.PeekChar() == 'D')
				{
					text.AdvanceChar();
					token = Token.t_date;
					if (text.PeekChar() == 'T')
					{
						text.AdvanceChar();
						token = Token.t_datetime;
					}
				}
				else if (text.PeekChar() == 'T')
				{
					text.AdvanceChar();
					token = Token.t_time;
				}
				else if (text.PeekChar() == 'L')
				{
					text.AdvanceChar();
					token = Token.t_s64;
				}
				continue;
			case '\'':
				text.AdvanceChar();
				while (text.PeekChar() != '\uffff')
				{
					if ((c = text.PeekChar()) == '\r' || c == '\n')
					{
						IncreaseLineNumber();
						if (enablePartialScan)
						{
							break;
						}
					}
					text.AdvanceChar();
					if (c == '\'')
					{
						if (text.PeekChar() != '\'')
						{
							break;
						}
						text.AdvanceChar();
					}
				}
				token = Token.t_string;
				continue;
			case '+':
				text.AdvanceChar();
				token = Token.t_plus;
				if (text.PeekChar() == '=')
				{
					text.AdvanceChar();
					token = Token.t_assignplus;
				}
				continue;
			case '-':
				text.AdvanceChar();
				token = Token.t_minus;
				if (text.PeekChar() == '=')
				{
					text.AdvanceChar();
					token = Token.t_assignminus;
				}
				continue;
			case '*':
				text.AdvanceChar();
				token = Token.t_mult;
				if (text.PeekChar() == '=')
				{
					text.AdvanceChar();
					token = Token.t_assignmult;
				}
				continue;
			case '/':
				text.AdvanceChar();
				token = Token.t_rdiv;
				if ((c = text.PeekChar()) == '=')
				{
					text.AdvanceChar();
					token = Token.t_assignrdiv;
				}
				else
				{
					if (c != '/')
					{
						continue;
					}
					text.AdvanceChar();
					while ((c = text.PeekChar()) != '\uffff' && c != '\r' && c != '\n')
					{
						text.AdvanceChar();
					}
					if (c != '\uffff')
					{
						text.AdvanceChar();
						if (c == '\r' && text.PeekChar() == '\n')
						{
							text.AdvanceChar();
							IncreaseLineNumber();
						}
					}
					token = Token.t_linecom;
					if (skipWS)
					{
						token = Token.t_none;
						text.Start();
					}
				}
				continue;
			case '<':
				text.AdvanceChar();
				token = Token.t_complt;
				if (text.PeekChar() == '=')
				{
					text.AdvanceChar();
					token = Token.t_comple;
				}
				else if (text.PeekChar() == '>')
				{
					text.AdvanceChar();
					token = Token.t_compne;
				}
				continue;
			case '=':
				text.AdvanceChar();
				token = Token.t_compeq;
				continue;
			case '>':
				text.AdvanceChar();
				token = Token.t_compgt;
				if (text.PeekChar() == '=')
				{
					text.AdvanceChar();
					token = Token.t_compge;
				}
				continue;
			case ':':
				text.AdvanceChar();
				token = Token.t_colon;
				if (text.PeekChar() == '=')
				{
					text.AdvanceChar();
					token = Token.t_assign;
				}
				else if (text.PeekChar() == ':')
				{
					text.AdvanceChar();
					token = Token.t_strsetval;
				}
				continue;
			case '(':
				text.AdvanceChar();
				token = Token.t_left;
				continue;
			case ')':
				text.AdvanceChar();
				token = Token.t_right;
				continue;
			case ',':
				text.AdvanceChar();
				token = Token.t_comma;
				continue;
			case ';':
				text.AdvanceChar();
				token = Token.t_semi;
				continue;
			case '.':
				text.AdvanceChar();
				token = Token.t_period;
				if (text.PeekChar() == '.')
				{
					text.AdvanceChar();
					token = Token.t_range;
				}
				continue;
			case '[':
				text.AdvanceChar();
				token = Token.t_left2;
				continue;
			case ']':
				text.AdvanceChar();
				token = Token.t_right2;
				continue;
			case '"':
				text.AdvanceChar();
				while ((c = text.PeekChar()) != '\uffff')
				{
					if (c == '\r' || c == '\n')
					{
						IncreaseLineNumber();
					}
					text.AdvanceChar();
					if (text.PeekChar() == '"')
					{
						text.AdvanceChar();
						break;
					}
				}
				token = Token.t_id;
				continue;
			case '{':
			{
				text.AdvanceChar();
				int num = 1;
				token = Token.t_startcom;
				while (true)
				{
					c = text.PeekChar();
					if (c == '\uffff')
					{
						break;
					}
					switch (text.PeekChar())
					{
					case '{':
						num++;
						goto IL_07c3;
					case '}':
						num--;
						if (num != 0)
						{
							goto IL_07c3;
						}
						break;
					default:
						goto IL_07c3;
					}
					token = Token.t_comment;
					text.AdvanceChar();
					if (skipWS)
					{
						token = Token.t_none;
						text.Start();
					}
					break;
					IL_07c3:
					text.AdvanceChar();
				}
				continue;
			}
			}
			if (char.IsLetter(c) || c == '_')
			{
				while (char.IsLetter(c = text.PeekChar()) || char.IsDigit(c) || c == '_')
				{
					text.AdvanceChar();
				}
				token = LookUpId();
			}
			else
			{
				text.AdvanceChar();
				token = Token.t_unknown;
			}
		}
		while (token == Token.t_none);
		if (token != Token.t_endofinput)
		{
			linePosition = text.Position - lineStartIndex;
		}
		return token;
	}

	private void IncreaseLineNumber()
	{
		LineNo++;
		lineStartIndex = text.Position;
	}

	private static void InitKeyWords()
	{
		if (keywords == null)
		{
			Dictionary<string, Token> dictionary = new Dictionary<string, Token>(300);
			dictionary.Add("FALSE", Token.t_false);
			dictionary.Add("TRUE", Token.t_true);
			dictionary.Add("DIV", Token.t_idiv);
			dictionary.Add("MOD", Token.t_mod);
			dictionary.Add("AND", Token.t_and);
			dictionary.Add("OR", Token.t_or);
			dictionary.Add("XOR", Token.t_xor);
			dictionary.Add("NOT", Token.t_not);
			dictionary.Add("EXIT", Token.t_exit);
			dictionary.Add("BEGIN", Token.t_begin);
			dictionary.Add("CASE", Token.t_case);
			dictionary.Add("DO", Token.t_do);
			dictionary.Add("DOWNTO", Token.t_downto);
			dictionary.Add("ELSE", Token.t_else);
			dictionary.Add("END", Token.t_end);
			dictionary.Add("FOR", Token.t_for);
			dictionary.Add("FOREACH", Token.t_foreach);
			dictionary.Add("IF", Token.t_if);
			dictionary.Add("IN", Token.t_in);
			dictionary.Add("OF", Token.t_of);
			dictionary.Add("REPEAT", Token.t_repeat);
			dictionary.Add("THEN", Token.t_then);
			dictionary.Add("TO", Token.t_to);
			dictionary.Add("UNTIL", Token.t_until);
			dictionary.Add("WITH", Token.t_with);
			dictionary.Add("WHILE", Token.t_while);
			dictionary.Add("ASSERTERROR", Token.t_asserterror);
			dictionary.Add("EVENT", Token.t_event);
			dictionary.Add("TEMPORARY", Token.t_temp);
			dictionary.Add("INDATASET", Token.t_indataset);
			dictionary.Add("NONDEBUGGABLE", Token.t_nondebuggable);
			dictionary.Add("PROGRAM", Token.t_program);
			dictionary.Add("PROCEDURE", Token.t_procedure);
			dictionary.Add("FUNCTION", Token.t_function);
			dictionary.Add("VAR", Token.t_var);
			dictionary.Add("ARRAY", Token.t_array);
			dictionary.Add("LOCAL", Token.t_local);
			dictionary.Add("WITHEVENTS", Token.t_withevents);
			dictionary.Add("RUNONCLIENT", Token.t_runonclient);
			dictionary.Add("SUPPRESSDISPOSE", Token.t_suppressdispose);
			dictionary.Add("SECURITYFILTERING", Token.t_securityfiltering);
			if (keywords == null)
			{
				keywords = dictionary;
			}
			if (keywordsAllowsAsIdentifiers == null)
			{
				keywordsAllowsAsIdentifiers = new SortedSet<Token>(new Token[9]
				{
					Token.t_program,
					Token.t_procedure,
					Token.t_function,
					Token.t_var,
					Token.t_array,
					Token.t_local,
					Token.t_withevents,
					Token.t_runonclient,
					Token.t_securityfiltering
				});
			}
		}
	}

	private Token LookUpId()
	{
		if (keywords.TryGetValue(text.GetText().ToUpperInvariant(), out var value))
		{
			return value;
		}
		return Token.t_id;
	}
}
