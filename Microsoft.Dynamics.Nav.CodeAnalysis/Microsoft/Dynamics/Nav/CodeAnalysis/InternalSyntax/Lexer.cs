using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class Lexer : IDisposable
{
	internal struct TokenInfo
	{
		internal SyntaxKind Kind;

		internal string Text;

		internal string StringValue;

		internal int Int32Value;

		internal long Int64Value;

		internal decimal DecimalValue;

		internal DateTime DateTimeValue;
	}

	private LexerMode mode;

	private List<SyntaxDiagnosticInfo> errors;

	private readonly StringBuilder builder = new StringBuilder(255);

	private readonly SlidingTextWindow textWindow;

	private readonly int linePosition;

	private InternalSyntaxListBuilder leadingTriviaCache = new InternalSyntaxListBuilder(10);

	private InternalSyntaxListBuilder trailingTriviaCache = new InternalSyntaxListBuilder(10);

	public int LineNo { get; private set; }

	internal SlidingTextWindow TextWindow => textWindow;

	public int Length => textWindow.Width;

	public int LinePos => linePosition;

	public ParseOptions Options { get; }

	protected bool HasErrors => errors != null;

	public Lexer(string sourceText)
		: this(SourceText.From(sourceText))
	{
	}

	public Lexer(SourceText sourceText, ParseOptions parseOptions = null)
	{
		textWindow = new SlidingTextWindow(sourceText);
		LineNo = 0;
		linePosition = 0;
		mode = (LexerMode)0;
		Options = parseOptions ?? ParseOptions.Default;
	}

	public void Reset(int position)
	{
		TextWindow.Reset(position);
	}

	public InternalSyntaxToken Lex(LexerMode lexerMode)
	{
		mode = lexerMode;
		return LexSyntaxToken();
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}

	private static int GetFullWidth(InternalSyntaxListBuilder builder)
	{
		int num = 0;
		if (builder != null)
		{
			for (int i = 0; i < builder.Count; i++)
			{
				num += builder[i].FullWidth;
			}
		}
		return num;
	}

	private InternalSyntaxToken LexSyntaxToken()
	{
		leadingTriviaCache.Clear();
		ScanSyntaxTrivia(ref leadingTriviaCache, isTrailing: false);
		TokenInfo info = default;
		ScanSyntaxToken(ref info);
		SyntaxDiagnosticInfo[] array = GetErrors(GetFullWidth(leadingTriviaCache));
		trailingTriviaCache.Clear();
		ScanSyntaxTrivia(ref trailingTriviaCache, isTrailing: true);
		return Create(ref info, leadingTriviaCache, trailingTriviaCache, array, mode);
	}

	private static InternalSyntaxToken Create(ref TokenInfo tokenInfo, InternalSyntaxListBuilder leadingTrivia, InternalSyntaxListBuilder trailingTrivia, SyntaxDiagnosticInfo[] errors, LexerMode mode)
	{
		InternalSyntaxNode leading = InternalSyntaxList.List(leadingTrivia);
		InternalSyntaxNode trailing = InternalSyntaxList.List(trailingTrivia);
		InternalSyntaxToken internalSyntaxToken = tokenInfo.Kind switch
		{
			SyntaxKind.IdentifierToken => InternalSyntaxFactory.Identifier(leading, tokenInfo.Text, trailing, mode), 
			SyntaxKind.StringLiteralToken => InternalSyntaxFactory.Literal(leading, tokenInfo.Text, tokenInfo.StringValue, trailing, mode), 
			SyntaxKind.Int32LiteralToken => InternalSyntaxFactory.Literal(leading, tokenInfo.Text, tokenInfo.Int32Value, trailing, mode), 
			SyntaxKind.Int64LiteralToken => InternalSyntaxFactory.Literal(leading, tokenInfo.Text, tokenInfo.Int64Value, trailing, mode), 
			SyntaxKind.DecimalLiteralToken => InternalSyntaxFactory.Literal(leading, tokenInfo.Text, tokenInfo.DecimalValue, trailing, mode), 
			SyntaxKind.TimeLiteralToken => InternalSyntaxFactory.Literal(tokenInfo.Kind, leading, tokenInfo.Text, tokenInfo.Int32Value, trailing, mode), 
			SyntaxKind.DateLiteralToken => InternalSyntaxFactory.Literal(tokenInfo.Kind, leading, tokenInfo.Text, tokenInfo.Int32Value, trailing, mode), 
			SyntaxKind.DateTimeLiteralToken => InternalSyntaxFactory.Literal(tokenInfo.Kind, leading, tokenInfo.Text, tokenInfo.DateTimeValue, trailing, mode), 
			SyntaxKind.TrueKeyword => InternalSyntaxFactory.Literal(tokenInfo.Kind, leading, tokenInfo.Text, value: true, trailing, mode), 
			SyntaxKind.FalseKeyword => InternalSyntaxFactory.Literal(tokenInfo.Kind, leading, tokenInfo.Text, value: false, trailing, mode), 
			SyntaxKind.None => InternalSyntaxFactory.BadToken(leading, tokenInfo.Text, trailing), 
			_ => (!tokenInfo.Kind.IsKeyword() && !tokenInfo.Kind.IsPropertyKeyword()) ? InternalSyntaxFactory.Token(leading, tokenInfo.Kind, trailing, mode) : InternalSyntaxFactory.Keyword(tokenInfo.Kind, leading, tokenInfo.Text, trailing, mode), 
		};
		if (errors != null)
		{
			internalSyntaxToken = internalSyntaxToken.WithInternalDiagnostics(errors);
		}
		return internalSyntaxToken;
	}

	private InternalSyntaxTrivia ScanWhitespace()
	{
		bool flag = true;
		while (true)
		{
			char c = textWindow.PeekChar();
			switch (c)
			{
			default:
				if (c != '\u001a')
				{
					if (c == ' ')
					{
						goto IL_0039;
					}
					if (c <= '\u007f' || !SyntaxFacts.IsWhitespace(c))
					{
						break;
					}
				}
				goto case '\t';
			case '\t':
			case '\v':
			case '\f':
				flag = false;
				goto IL_0039;
			case '\n':
			case '\r':
				break;
			}
			break;
			IL_0039:
			textWindow.AdvanceChar();
		}
		if (flag)
		{
			return InternalSyntaxFactory.Spaces(textWindow.Width);
		}
		return InternalSyntaxFactory.Whitespace(textWindow.GetText());
	}

	private void Start()
	{
		errors = null;
		textWindow.Start();
	}

	public static bool IsNewLine(char ch)
	{
		if (ch != '\n')
		{
			return ch == '\r';
		}
		return true;
	}

	private void ScanToEndOfLine()
	{
		char c;
		while (!IsNewLine(c = textWindow.PeekChar()) && (c != '\uffff' || !textWindow.IsReallyAtEnd()))
		{
			textWindow.AdvanceChar();
		}
	}

	private bool ScanMultiLineComment(out bool isTerminated)
	{
		if (textWindow.PeekChar() == '/' && textWindow.PeekChar(1) == '*')
		{
			textWindow.AdvanceChar(2);
			while (true)
			{
				char c;
				if ((c = textWindow.PeekChar()) == '\uffff' && textWindow.IsReallyAtEnd())
				{
					isTerminated = false;
					break;
				}
				if (c == '*' && textWindow.PeekChar(1) == '/')
				{
					textWindow.AdvanceChar(2);
					isTerminated = true;
					break;
				}
				textWindow.AdvanceChar();
			}
			return true;
		}
		isTerminated = false;
		return false;
	}

	private void ScanSyntaxTrivia(ref InternalSyntaxListBuilder triviaList, bool isTrailing)
	{
		while (true)
		{
			Start();
			char c = textWindow.PeekChar();
			if (c == ' ')
			{
				AddTrivia(ScanWhitespace(), ref triviaList);
				continue;
			}
			if (c > '\u007f')
			{
				if (SyntaxFacts.IsWhitespace(c))
				{
					c = ' ';
				}
				else if (SyntaxFacts.IsNewLine(c))
				{
					c = '\n';
				}
			}
			switch (c)
			{
			default:
				return;
			case '\t':
			case '\v':
			case '\f':
			case '\u001a':
			case ' ':
				AddTrivia(ScanWhitespace(), ref triviaList);
				break;
			case '\r':
				textWindow.AdvanceChar();
				if (textWindow.PeekChar() == '\n')
				{
					textWindow.AdvanceChar();
					AddTrivia(InternalSyntaxFactory.CarriageReturnLineFeed, ref triviaList);
				}
				else
				{
					AddTrivia(InternalSyntaxFactory.CarriageReturn, ref triviaList);
				}
				if (isTrailing)
				{
					return;
				}
				break;
			case '\n':
				textWindow.AdvanceChar();
				AddTrivia(InternalSyntaxFactory.LineFeed, ref triviaList);
				if (isTrailing)
				{
					return;
				}
				break;
			case '/':
			{
				char c2 = textWindow.PeekChar(1);
				if (c2 != '/' && c2 != '*')
				{
					return;
				}
				switch (c2)
				{
				case '/':
					ScanToEndOfLine();
					AddTrivia(InternalSyntaxFactory.Comment(SyntaxKind.LineCommentTrivia, textWindow.GetText()), ref triviaList);
					if (isTrailing)
					{
						return;
					}
					break;
				case '*':
				{
					ScanMultiLineComment(out var isTerminated);
					if (!isTerminated)
					{
						AddError(ErrorCode.ERR_InvalidMultilineComment);
					}
					AddTrivia(InternalSyntaxFactory.Comment(SyntaxKind.CommentTrivia, textWindow.GetText()), ref triviaList);
					break;
				}
				}
				break;
			}
			}
		}
	}

	private void ScanQuotedString(ref TokenInfo info, SyntaxKind stringKind)
	{
		char c = textWindow.PeekChar();
		if (c == '\'' || c == '"')
		{
			textWindow.AdvanceChar();
			builder.Length = 0;
			while (true)
			{
				char c2 = textWindow.PeekChar();
				if (c2 == c)
				{
					textWindow.AdvanceChar();
					if (textWindow.PeekChar() != c)
					{
						break;
					}
				}
				else if (IsNewLine(c2) || (c2 == '\uffff' && textWindow.IsReallyAtEnd()))
				{
					if (c == '\'')
					{
						AddError(ErrorCode.ERR_TextLiteralNotProperlyTerminated, builder.ToString());
					}
					else
					{
						AddError(ErrorCode.ERR_IdentifierNotProperlyTerminated, builder.ToString());
					}
					break;
				}
				textWindow.AdvanceChar();
				builder.Append(c2);
			}
			info.Text = textWindow.GetText();
			info.Kind = stringKind;
			if (builder.Length > 0)
			{
				info.StringValue = builder.ToString();
			}
			else
			{
				info.StringValue = string.Empty;
			}
		}
		else
		{
			info.Kind = SyntaxKind.None;
			info.Text = null;
		}
	}

	private void ScanNumericLiteral(ref TokenInfo info)
	{
		info.Kind = SyntaxKind.Int32LiteralToken;
		builder.Length = 0;
		char c;
		while (char.IsDigit(c = textWindow.PeekChar()))
		{
			builder.Append(c);
			textWindow.AdvanceChar();
		}
		if (c == '.' && char.IsDigit(textWindow.PeekChar(1)))
		{
			info.Kind = SyntaxKind.DecimalLiteralToken;
			textWindow.AdvanceChar();
			builder.Append(c);
			while (char.IsDigit(c = textWindow.PeekChar()))
			{
				textWindow.AdvanceChar();
				builder.Append(c);
			}
			if (c == 'T')
			{
				info.Kind = SyntaxKind.TimeLiteralToken;
				if (!DateTimeUtilities.TryParseTime(builder.ToString(), out info.Int32Value))
				{
					builder.Append(c);
					AddError(ErrorCode.ERR_InvalidTimeLiteral, builder.ToString());
				}
				textWindow.AdvanceChar();
			}
			else if (!decimal.TryParse(builder.ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out info.DecimalValue))
			{
				AddError(ErrorCode.ERR_InvalidDecimalLiteral, builder.ToString());
			}
		}
		else
		{
			switch (c)
			{
			case 'D':
				info.Kind = SyntaxKind.DateLiteralToken;
				builder.Append(c);
				textWindow.AdvanceChar();
				if (textWindow.PeekChar() == 'T')
				{
					builder.Append(textWindow.PeekChar());
					textWindow.AdvanceChar();
					if (ParseZeroDateTime(builder.ToString(), out info.Int32Value))
					{
						info.Kind = SyntaxKind.DateTimeLiteralToken;
						break;
					}
					AddError(ErrorCode.ERR_InvalidDateTimeLiteral, builder.ToString());
				}
				else if (!DateTimeUtilities.TryParseDate(builder.ToString(), out info.Int32Value))
				{
					AddError(ErrorCode.ERR_InvalidDateLiteral, builder.ToString());
				}
				break;
			case 'T':
				info.Kind = SyntaxKind.TimeLiteralToken;
				textWindow.AdvanceChar();
				if (!DateTimeUtilities.TryParseTime(builder.ToString(), out info.Int32Value))
				{
					builder.Append(c);
					AddError(ErrorCode.ERR_InvalidTimeLiteral, builder.ToString());
				}
				break;
			case 'L':
				info.Kind = SyntaxKind.Int64LiteralToken;
				textWindow.AdvanceChar();
				if (!long.TryParse(builder.ToString(), out info.Int64Value))
				{
					AddError(ErrorCode.ERR_InvalidInt64Literal, string.Concat(builder, "L"));
				}
				break;
			default:
				if (!int.TryParse(builder.ToString(), out info.Int32Value))
				{
					AddError(ErrorCode.ERR_InvalidInt32Literal, builder.ToString());
				}
				break;
			}
		}
		info.Text = textWindow.GetText();
	}

	private static bool ParseZeroDateTime(string value, out int dateTime)
	{
		dateTime = 0;
		return value == "0DT";
	}

	internal SyntaxTriviaList ScanSyntaxLeadingTrivia()
	{
		leadingTriviaCache.Clear();
		ScanSyntaxTrivia(ref leadingTriviaCache, isTrailing: false);
		return new SyntaxTriviaList(default, leadingTriviaCache.ToListNode(), 0);
	}

	internal SyntaxTriviaList ScanSyntaxTrailingTrivia()
	{
		trailingTriviaCache.Clear();
		ScanSyntaxTrivia(ref trailingTriviaCache, isTrailing: true);
		return new SyntaxTriviaList(default, trailingTriviaCache.ToListNode(), 0);
	}

	private void ScanIdentifierOrKeyword(ref TokenInfo info)
	{
		char c = textWindow.PeekChar();
		if (char.IsLetter(c) || c == '_')
		{
			builder.Length = 0;
			textWindow.AdvanceChar();
			builder.Append(char.ToUpperInvariant(c));
			while (char.IsLetter(c = textWindow.PeekChar()) || char.IsDigit(c) || c == '_')
			{
				textWindow.AdvanceChar();
				builder.Append(char.ToUpperInvariant(c));
			}
			info.Text = textWindow.GetText();
			if ((info.Kind = GetKeywordKind(builder.ToString())) == SyntaxKind.None)
			{
				info.Kind = SyntaxKind.IdentifierToken;
			}
		}
	}

	private SyntaxKind GetKeywordKind(string text)
	{
		switch (mode)
		{
		default:
			throw new InvalidOperationException("Unrecognized lexer mode");
		case LexerMode.Code:
		case LexerMode.Expression:
			return SyntaxFacts.GetALKeywordKind(text);
		case LexerMode.Object:
			return SyntaxFacts.GetObjectKeywordKind(text);
		case LexerMode.Property:
			return SyntaxFacts.GetPropertyKeywordKind(text);
		}
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	[SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
	private void ScanSyntaxToken(ref TokenInfo info)
	{
		char c = textWindow.PeekChar();
		switch (c)
		{
		case '\'':
			ScanQuotedString(ref info, SyntaxKind.StringLiteralToken);
			break;
		case '"':
			ScanQuotedString(ref info, SyntaxKind.IdentifierToken);
			break;
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
			ScanNumericLiteral(ref info);
			break;
		case 'A':
		case 'B':
		case 'C':
		case 'D':
		case 'E':
		case 'F':
		case 'G':
		case 'H':
		case 'I':
		case 'J':
		case 'K':
		case 'L':
		case 'M':
		case 'N':
		case 'O':
		case 'P':
		case 'Q':
		case 'R':
		case 'S':
		case 'T':
		case 'U':
		case 'V':
		case 'W':
		case 'X':
		case 'Y':
		case 'Z':
		case '_':
		case 'a':
		case 'b':
		case 'c':
		case 'd':
		case 'e':
		case 'f':
		case 'g':
		case 'h':
		case 'i':
		case 'j':
		case 'k':
		case 'l':
		case 'm':
		case 'n':
		case 'o':
		case 'p':
		case 'q':
		case 'r':
		case 's':
		case 't':
		case 'u':
		case 'v':
		case 'w':
		case 'x':
		case 'y':
		case 'z':
			ScanIdentifierOrKeyword(ref info);
			break;
		case '+':
			textWindow.AdvanceChar();
			if (textWindow.PeekChar() == '=')
			{
				textWindow.AdvanceChar();
				info.Kind = SyntaxKind.AssignPlusToken;
			}
			else
			{
				info.Kind = SyntaxKind.PlusToken;
			}
			break;
		case '-':
			textWindow.AdvanceChar();
			if (textWindow.PeekChar() == '=')
			{
				textWindow.AdvanceChar();
				info.Kind = SyntaxKind.AssignMinusToken;
			}
			else
			{
				info.Kind = SyntaxKind.MinusToken;
			}
			break;
		case '*':
			textWindow.AdvanceChar();
			if (textWindow.PeekChar() == '=')
			{
				textWindow.AdvanceChar();
				info.Kind = SyntaxKind.AssignMultiplyToken;
			}
			else
			{
				info.Kind = SyntaxKind.MultiplyToken;
			}
			break;
		case '/':
			textWindow.AdvanceChar();
			if (textWindow.PeekChar() == '=')
			{
				textWindow.AdvanceChar();
				info.Kind = SyntaxKind.AssignRDivToken;
			}
			else
			{
				info.Kind = SyntaxKind.RDivToken;
			}
			break;
		case '(':
			textWindow.AdvanceChar();
			info.Kind = SyntaxKind.OpenParenToken;
			break;
		case ')':
			textWindow.AdvanceChar();
			info.Kind = SyntaxKind.CloseParenToken;
			break;
		case '{':
			textWindow.AdvanceChar();
			info.Kind = SyntaxKind.OpenBraceToken;
			break;
		case '}':
			textWindow.AdvanceChar();
			info.Kind = SyntaxKind.CloseBraceToken;
			break;
		case '[':
			textWindow.AdvanceChar();
			info.Kind = SyntaxKind.OpenBracketToken;
			break;
		case ']':
			textWindow.AdvanceChar();
			info.Kind = SyntaxKind.CloseBracketToken;
			break;
		case '<':
			textWindow.AdvanceChar();
			info.Kind = SyntaxKind.LessThanToken;
			if ((c = textWindow.PeekChar()) == '=')
			{
				textWindow.AdvanceChar();
				info.Kind = SyntaxKind.LessThanEqualsToken;
			}
			else if (c == '>')
			{
				textWindow.AdvanceChar();
				info.Kind = SyntaxKind.NotEqualsToken;
			}
			break;
		case '=':
			textWindow.AdvanceChar();
			info.Kind = SyntaxKind.EqualsToken;
			break;
		case '>':
			textWindow.AdvanceChar();
			info.Kind = SyntaxKind.GreaterThanToken;
			if (textWindow.PeekChar() == '=')
			{
				textWindow.AdvanceChar();
				info.Kind = SyntaxKind.GreaterThanEqualsToken;
			}
			break;
		case ':':
			textWindow.AdvanceChar();
			info.Kind = SyntaxKind.ColonToken;
			if ((c = textWindow.PeekChar()) == '=')
			{
				textWindow.AdvanceChar();
				info.Kind = SyntaxKind.AssignToken;
			}
			else if (c == ':')
			{
				textWindow.AdvanceChar();
				info.Kind = SyntaxKind.ColonColonToken;
			}
			break;
		case ',':
			textWindow.AdvanceChar();
			info.Kind = SyntaxKind.CommaToken;
			break;
		case ';':
			textWindow.AdvanceChar();
			info.Kind = SyntaxKind.SemicolonToken;
			break;
		case '.':
			textWindow.AdvanceChar();
			info.Kind = SyntaxKind.DotToken;
			if (textWindow.PeekChar() == '.')
			{
				textWindow.AdvanceChar();
				info.Kind = SyntaxKind.DotDotToken;
			}
			break;
		case '&':
			if (mode == LexerMode.Property)
			{
				textWindow.AdvanceChar();
				info.Kind = SyntaxKind.AndFilterKeyword;
				break;
			}
			goto default;
		case '@':
			textWindow.AdvanceChar();
			info.Kind = SyntaxKind.AtToken;
			break;
		case '|':
			if (mode == LexerMode.Property)
			{
				textWindow.AdvanceChar();
				info.Kind = SyntaxKind.OrFilterKeyword;
				break;
			}
			goto default;
		case '\uffff':
			if (textWindow.IsReallyAtEnd())
			{
				info.Kind = SyntaxKind.EndOfFileToken;
				break;
			}
			goto default;
		default:
			if (UnicodeCharacterUtilities.IsIdentifierStartCharacter(c))
			{
				ScanIdentifierOrKeyword(ref info);
				break;
			}
			textWindow.AdvanceChar();
			info.Text = textWindow.GetText();
			AddError(ErrorCode.ERR_UnexpectedCharacter, info.Text);
			break;
		}
		if (info.Kind == SyntaxKind.IdentifierToken && info.Text.Length > 120)
		{
			AddError(0, info.Text.Length, ErrorCode.ERR_IdentifierTooLong, 120);
		}
	}

	protected SyntaxDiagnosticInfo[] GetErrors(int leadingTriviaWidth)
	{
		if (errors != null)
		{
			if (leadingTriviaWidth > 0)
			{
				SyntaxDiagnosticInfo[] array = new SyntaxDiagnosticInfo[errors.Count];
				for (int i = 0; i < errors.Count; i++)
				{
					array[i] = errors[i].WithOffset(errors[i].Offset + leadingTriviaWidth);
				}
				return array;
			}
			return errors.ToArray();
		}
		return null;
	}

	protected void AddError(int position, int width, ErrorCode code)
	{
		AddError(MakeError(position, width, code));
	}

	protected void AddError(int position, int width, ErrorCode code, params object[] args)
	{
		AddError(MakeError(position, width, code, args));
	}

	protected void AddError(ErrorCode code)
	{
		AddError(MakeError(code));
	}

	protected void AddError(ErrorCode code, params object[] args)
	{
		AddError(MakeError(code, args));
	}

	protected void AddError(SyntaxDiagnosticInfo error)
	{
		if (error != null)
		{
			if (errors == null)
			{
				errors = new List<SyntaxDiagnosticInfo>(8);
			}
			errors.Add(error);
		}
	}

	protected SyntaxDiagnosticInfo MakeError(int position, int width, ErrorCode code)
	{
		return new SyntaxDiagnosticInfo(GetLexemeOffsetFromPosition(position), width, code);
	}

	protected SyntaxDiagnosticInfo MakeError(int position, int width, ErrorCode code, params object[] args)
	{
		return new SyntaxDiagnosticInfo(GetLexemeOffsetFromPosition(position), width, code, args);
	}

	private int GetLexemeOffsetFromPosition(int position)
	{
		if (position < textWindow.LexemeStartPosition)
		{
			return position;
		}
		return position - textWindow.LexemeStartPosition;
	}

	protected static SyntaxDiagnosticInfo MakeError(ErrorCode code)
	{
		return new SyntaxDiagnosticInfo(code);
	}

	protected static SyntaxDiagnosticInfo MakeError(ErrorCode code, params object[] args)
	{
		return new SyntaxDiagnosticInfo(code, args);
	}

	private void AddTrivia(InternalSyntaxNode trivia, ref InternalSyntaxListBuilder list)
	{
		if (HasErrors)
		{
			DiagnosticInfo[] array = GetErrors(0);
			DiagnosticInfo[] diagnostics = array;
			trivia = trivia.WithInternalDiagnostics(diagnostics);
		}
		list.Add(trivia);
	}
}
