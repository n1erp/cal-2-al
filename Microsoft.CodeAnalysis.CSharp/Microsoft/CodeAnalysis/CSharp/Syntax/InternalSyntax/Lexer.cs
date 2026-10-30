using System;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal class Lexer : AbstractLexer
{
	internal struct TokenInfo
	{
		internal SyntaxKind Kind;

		internal SyntaxKind ContextualKind;

		internal string Text;

		internal SpecialType ValueKind;

		internal bool RequiresTextForXmlEntity;

		internal bool HasIdentifierEscapeSequence;

		internal string StringValue;

		internal char CharValue;

		internal int IntValue;

		internal uint UintValue;

		internal long LongValue;

		internal ulong UlongValue;

		internal float FloatValue;

		internal double DoubleValue;

		internal decimal DecimalValue;

		internal bool IsVerbatim;
	}

	internal struct Interpolation
	{
		public readonly int OpenBracePosition;

		public readonly int ColonPosition;

		public readonly int CloseBracePosition;

		public readonly bool CloseBraceMissing;

		public bool ColonMissing => ColonPosition <= 0;

		public bool HasColon => ColonPosition > 0;

		public int LastPosition
		{
			get
			{
				if (!CloseBraceMissing)
				{
					return CloseBracePosition;
				}
				return CloseBracePosition - 1;
			}
		}

		public int FormatEndPosition => CloseBracePosition - 1;

		public Interpolation(int openBracePosition, int colonPosition, int closeBracePosition, bool closeBraceMissing)
		{
			OpenBracePosition = openBracePosition;
			ColonPosition = colonPosition;
			CloseBracePosition = closeBracePosition;
			CloseBraceMissing = closeBraceMissing;
		}
	}

	private class InterpolatedStringScanner
	{
		public readonly Lexer lexer;

		public bool isVerbatim;

		public bool allowNewlines;

		public SyntaxDiagnosticInfo error;

		public InterpolatedStringScanner(Lexer lexer, bool isVerbatim)
		{
			this.lexer = lexer;
			this.isVerbatim = isVerbatim;
			allowNewlines = isVerbatim;
		}

		private bool IsAtEnd()
		{
			return IsAtEnd(isVerbatim && allowNewlines);
		}

		private bool IsAtEnd(bool allowNewline)
		{
			char c = lexer.TextWindow.PeekChar();
			if (allowNewline || !SyntaxFacts.IsNewLine(c))
			{
				if (c == '\uffff')
				{
					return lexer.TextWindow.IsReallyAtEnd();
				}
				return false;
			}
			return true;
		}

		internal void ScanInterpolatedStringLiteralTop(ArrayBuilder<Interpolation> interpolations, ref TokenInfo info, out bool closeQuoteMissing)
		{
			lexer.TextWindow.AdvanceChar();
			if (isVerbatim)
			{
				lexer.TextWindow.AdvanceChar();
			}
			lexer.TextWindow.AdvanceChar();
			ScanInterpolatedStringLiteralContents(interpolations);
			if (lexer.TextWindow.PeekChar() != '"')
			{
				if (error == null)
				{
					int position = (IsAtEnd(allowNewline: true) ? (lexer.TextWindow.Position - 1) : lexer.TextWindow.Position);
					error = lexer.MakeError(position, 1, isVerbatim ? ErrorCode.ERR_UnterminatedStringLit : ErrorCode.ERR_NewlineInConst);
				}
				closeQuoteMissing = true;
			}
			else
			{
				lexer.TextWindow.AdvanceChar();
				closeQuoteMissing = false;
			}
			info.Kind = SyntaxKind.InterpolatedStringToken;
		}

		private void ScanInterpolatedStringLiteralContents(ArrayBuilder<Interpolation> interpolations)
		{
			while (!IsAtEnd())
			{
				switch (lexer.TextWindow.PeekChar())
				{
				case '"':
					if (isVerbatim && lexer.TextWindow.PeekChar(1) == '"')
					{
						lexer.TextWindow.AdvanceChar();
						lexer.TextWindow.AdvanceChar();
						continue;
					}
					return;
				case '}':
				{
					int position2 = lexer.TextWindow.Position;
					lexer.TextWindow.AdvanceChar();
					if (lexer.TextWindow.PeekChar() == '}')
					{
						lexer.TextWindow.AdvanceChar();
					}
					else if (error == null)
					{
						error = lexer.MakeError(position2, 1, ErrorCode.ERR_UnescapedCurly, "}");
					}
					continue;
				}
				case '{':
				{
					if (lexer.TextWindow.PeekChar(1) == '{')
					{
						lexer.TextWindow.AdvanceChar();
						lexer.TextWindow.AdvanceChar();
						continue;
					}
					int position3 = lexer.TextWindow.Position;
					lexer.TextWindow.AdvanceChar();
					int colonPosition = 0;
					ScanInterpolatedStringLiteralHoleBalancedText('}', isHole: true, ref colonPosition);
					int position4 = lexer.TextWindow.Position;
					bool closeBraceMissing = false;
					if (lexer.TextWindow.PeekChar() == '}')
					{
						lexer.TextWindow.AdvanceChar();
					}
					else
					{
						closeBraceMissing = true;
						if (error == null)
						{
							error = lexer.MakeError(position3 - 1, 2, ErrorCode.ERR_UnclosedExpressionHole);
						}
					}
					interpolations?.Add(new Interpolation(position3, colonPosition, position4, closeBraceMissing));
					continue;
				}
				case '\\':
					if (!isVerbatim)
					{
						int position = lexer.TextWindow.Position;
						char c = lexer.ScanEscapeSequence(out var _);
						if ((c == '{' || c == '}') && error == null)
						{
							error = lexer.MakeError(position, lexer.TextWindow.Position - position, ErrorCode.ERR_EscapedCurly, c);
						}
						continue;
					}
					break;
				}
				lexer.TextWindow.AdvanceChar();
			}
		}

		private void ScanFormatSpecifier()
		{
			lexer.TextWindow.AdvanceChar();
			while (true)
			{
				char c = lexer.TextWindow.PeekChar();
				if (c == '\\' && !isVerbatim)
				{
					int position = lexer.TextWindow.Position;
					c = lexer.ScanEscapeSequence(out var _);
					if ((c == '{' || c == '}') && error == null)
					{
						error = lexer.MakeError(position, 1, ErrorCode.ERR_EscapedCurly, c);
					}
					continue;
				}
				switch (c)
				{
				case '"':
					if (isVerbatim && lexer.TextWindow.PeekChar(1) == '"')
					{
						lexer.TextWindow.AdvanceChar();
						lexer.TextWindow.AdvanceChar();
						break;
					}
					return;
				case '{':
				{
					int position2 = lexer.TextWindow.Position;
					lexer.TextWindow.AdvanceChar();
					if (lexer.TextWindow.PeekChar() == '{')
					{
						lexer.TextWindow.AdvanceChar();
					}
					else if (error == null)
					{
						error = lexer.MakeError(position2, 1, ErrorCode.ERR_UnescapedCurly, "{");
					}
					break;
				}
				case '}':
					if (lexer.TextWindow.PeekChar(1) == '}')
					{
						lexer.TextWindow.AdvanceChar();
						lexer.TextWindow.AdvanceChar();
						break;
					}
					return;
				default:
					if (IsAtEnd())
					{
						return;
					}
					lexer.TextWindow.AdvanceChar();
					break;
				}
			}
		}

		private void ScanInterpolatedStringLiteralHoleBalancedText(char endingChar, bool isHole, ref int colonPosition)
		{
			while (!IsAtEnd())
			{
				char c = lexer.TextWindow.PeekChar();
				switch (c)
				{
				case '#':
					if (error == null)
					{
						error = lexer.MakeError(lexer.TextWindow.Position, 1, ErrorCode.ERR_SyntaxError, endingChar.ToString());
					}
					lexer.TextWindow.AdvanceChar();
					continue;
				case '$':
					if (lexer.TextWindow.PeekChar(1) == '"' || (lexer.TextWindow.PeekChar(1) == '@' && lexer.TextWindow.PeekChar(2) == '"'))
					{
						bool flag = lexer.TextWindow.PeekChar(1) == '@';
						ArrayBuilder<Interpolation> interpolations = null;
						TokenInfo info = default;
						bool flag2 = isVerbatim;
						bool flag3 = allowNewlines;
						try
						{
							isVerbatim = flag;
							allowNewlines &= isVerbatim;
							ScanInterpolatedStringLiteralTop(interpolations, ref info, out var _);
						}
						finally
						{
							isVerbatim = flag2;
							allowNewlines = flag3;
						}
						continue;
					}
					break;
				case ':':
					if (isHole)
					{
						colonPosition = lexer.TextWindow.Position;
						ScanFormatSpecifier();
						return;
					}
					break;
				case ')':
				case ']':
				case '}':
					if (c == endingChar)
					{
						return;
					}
					if (error == null)
					{
						error = lexer.MakeError(lexer.TextWindow.Position, 1, ErrorCode.ERR_SyntaxError, endingChar.ToString());
					}
					break;
				case '"':
				case '\'':
					ScanInterpolatedStringLiteralNestedString();
					continue;
				case '@':
					if (lexer.TextWindow.PeekChar(1) == '"')
					{
						ScanInterpolatedStringLiteralNestedVerbatimString();
						continue;
					}
					break;
				case '/':
					switch (lexer.TextWindow.PeekChar(1))
					{
					case '/':
						if (isVerbatim && allowNewlines)
						{
							lexer.TextWindow.AdvanceChar();
							lexer.TextWindow.AdvanceChar();
							while (!IsAtEnd(allowNewline: false))
							{
								lexer.TextWindow.AdvanceChar();
							}
							break;
						}
						if (error == null)
						{
							error = lexer.MakeError(lexer.TextWindow.Position, 2, ErrorCode.ERR_SingleLineCommentInExpressionHole);
						}
						lexer.TextWindow.AdvanceChar();
						lexer.TextWindow.AdvanceChar();
						break;
					case '*':
						ScanInterpolatedStringLiteralNestedComment();
						break;
					default:
						lexer.TextWindow.AdvanceChar();
						break;
					}
					continue;
				case '{':
					ScanInterpolatedStringLiteralHoleBracketed('{', '}');
					continue;
				case '(':
					ScanInterpolatedStringLiteralHoleBracketed('(', ')');
					continue;
				case '[':
					ScanInterpolatedStringLiteralHoleBracketed('[', ']');
					continue;
				}
				lexer.TextWindow.AdvanceChar();
			}
		}

		private void ScanInterpolatedStringLiteralNestedComment()
		{
			lexer.TextWindow.AdvanceChar();
			lexer.TextWindow.AdvanceChar();
			char c;
			do
			{
				if (IsAtEnd())
				{
					return;
				}
				c = lexer.TextWindow.PeekChar();
				lexer.TextWindow.AdvanceChar();
			}
			while (c != '*' || lexer.TextWindow.PeekChar() != '/');
			lexer.TextWindow.AdvanceChar();
		}

		private void ScanInterpolatedStringLiteralNestedString()
		{
			TokenInfo info = default;
			lexer.ScanStringLiteral(ref info);
		}

		private void ScanInterpolatedStringLiteralNestedVerbatimString()
		{
			TokenInfo info = default;
			lexer.ScanVerbatimStringLiteral(ref info, allowNewlines);
		}

		private void ScanInterpolatedStringLiteralHoleBracketed(char start, char end)
		{
			lexer.TextWindow.AdvanceChar();
			int colonPosition = 0;
			ScanInterpolatedStringLiteralHoleBalancedText(end, isHole: false, ref colonPosition);
			if (lexer.TextWindow.PeekChar() == end)
			{
				lexer.TextWindow.AdvanceChar();
			}
		}
	}

	private enum QuickScanState : byte
	{
		Initial,
		FollowingWhite,
		FollowingCR,
		Ident,
		Number,
		Punctuation,
		Dot,
		CompoundPunctStart,
		DoneAfterNext,
		Done,
		Bad
	}

	private enum CharFlags : byte
	{
		White,
		CR,
		LF,
		Letter,
		Digit,
		Punct,
		Dot,
		CompoundPunctStart,
		Slash,
		Complex,
		EndOfFile
	}

	private const int TriviaListInitialCapacity = 8;

	private readonly CSharpParseOptions _options;

	private LexerMode _mode;

	private readonly StringBuilder _builder;

	private char[] _identBuffer;

	private int _identLen;

	private DirectiveStack _directives;

	private readonly LexerCache _cache;

	private readonly bool _allowPreprocessorDirectives;

	private readonly bool _interpolationFollowedByColon;

	private DocumentationCommentParser _xmlParser;

	private int _badTokenCount;

	private SyntaxListBuilder _leadingTriviaCache = new SyntaxListBuilder(10);

	private SyntaxListBuilder _trailingTriviaCache = new SyntaxListBuilder(10);

	private static readonly int s_conflictMarkerLength = "<<<<<<<".Length;

	private Func<SyntaxTrivia> _createWhitespaceTriviaFunction;

	internal const int MaxCachedTokenSize = 42;

	private static readonly byte[,] s_stateTransitions = new byte[9, 11]
	{
		{
			0, 0, 0, 3, 4, 5, 6, 7, 10, 10,
			10
		},
		{
			1, 2, 8, 9, 9, 9, 9, 9, 10, 10,
			9
		},
		{
			9, 9, 8, 9, 9, 9, 9, 9, 9, 9,
			9
		},
		{
			1, 2, 8, 3, 3, 9, 9, 9, 10, 10,
			9
		},
		{
			1, 2, 8, 10, 4, 9, 10, 9, 10, 10,
			9
		},
		{
			1, 2, 8, 9, 9, 9, 9, 9, 10, 10,
			9
		},
		{
			1, 2, 8, 9, 4, 9, 9, 9, 10, 10,
			9
		},
		{
			1, 2, 8, 9, 9, 10, 9, 10, 10, 10,
			9
		},
		{
			9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
			9
		}
	};

	private readonly Func<SyntaxToken> _createQuickTokenFunction;

	private static readonly byte[] s_charProperties = new byte[384]
	{
		9, 9, 9, 9, 9, 9, 9, 9, 9, 0,
		2, 0, 0, 1, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 0, 7, 9, 9, 9, 7, 7, 9,
		5, 5, 7, 7, 5, 7, 6, 8, 4, 4,
		4, 4, 4, 4, 4, 4, 4, 4, 7, 5,
		7, 7, 7, 7, 9, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 5, 9, 5, 7, 3, 9, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 5, 7, 5, 7, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		3, 9, 9, 9, 9, 9, 9, 9, 9, 9,
		9, 3, 9, 9, 9, 9, 3, 9, 9, 9,
		9, 9, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 9, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 9, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
		3, 3, 3, 3
	};

	public bool SuppressDocumentationCommentParse => (int)_options.DocumentationMode < 1;

	public CSharpParseOptions Options => _options;

	public DirectiveStack Directives => _directives;

	public bool InterpolationFollowedByColon => _interpolationFollowedByColon;

	private bool InDocumentationComment
	{
		get
		{
			switch (ModeOf(_mode))
			{
			case LexerMode.XmlDocComment:
			case LexerMode.XmlElementTag:
			case LexerMode.XmlAttributeTextQuote:
			case LexerMode.XmlAttributeTextDoubleQuote:
			case LexerMode.XmlCrefQuote:
			case LexerMode.XmlCrefDoubleQuote:
			case LexerMode.XmlNameQuote:
			case LexerMode.XmlNameDoubleQuote:
			case LexerMode.XmlCDataSectionText:
			case LexerMode.XmlCommentText:
			case LexerMode.XmlProcessingInstructionText:
			case LexerMode.XmlCharacter:
				return true;
			default:
				return false;
			}
		}
	}

	private bool InXmlCrefOrNameAttributeValue
	{
		get
		{
			switch (_mode & LexerMode.MaskLexMode)
			{
			case LexerMode.XmlCrefQuote:
			case LexerMode.XmlCrefDoubleQuote:
			case LexerMode.XmlNameQuote:
			case LexerMode.XmlNameDoubleQuote:
				return true;
			default:
				return false;
			}
		}
	}

	private bool InXmlNameAttributeValue
	{
		get
		{
			LexerMode lexerMode = _mode & LexerMode.MaskLexMode;
			if (lexerMode == LexerMode.XmlNameQuote || lexerMode == LexerMode.XmlNameDoubleQuote)
			{
				return true;
			}
			return false;
		}
	}

	public Lexer(SourceText text, CSharpParseOptions options, bool allowPreprocessorDirectives = true, bool interpolationFollowedByColon = false)
		: base(text)
	{
		_options = options;
		_builder = new StringBuilder();
		_identBuffer = new char[32];
		_cache = new LexerCache();
		_createQuickTokenFunction = CreateQuickToken;
		_allowPreprocessorDirectives = allowPreprocessorDirectives;
		_interpolationFollowedByColon = interpolationFollowedByColon;
	}

	public override void Dispose()
	{
		_cache.Free();
		if (_xmlParser != null)
		{
			_xmlParser.Dispose();
		}
		base.Dispose();
	}

	public void Reset(int position, DirectiveStack directives)
	{
		TextWindow.Reset(position);
		_directives = directives;
	}

	private static LexerMode ModeOf(LexerMode mode)
	{
		return mode & LexerMode.MaskLexMode;
	}

	private bool ModeIs(LexerMode mode)
	{
		return ModeOf(_mode) == mode;
	}

	private static XmlDocCommentLocation LocationOf(LexerMode mode)
	{
		return (XmlDocCommentLocation)((int)(mode & LexerMode.MaskXmlDocCommentLocation) >> 16);
	}

	private bool LocationIs(XmlDocCommentLocation location)
	{
		return LocationOf(_mode) == location;
	}

	private void MutateLocation(XmlDocCommentLocation location)
	{
		_mode &= ~LexerMode.MaskXmlDocCommentLocation;
		_mode |= (LexerMode)((int)location << 16);
	}

	private static XmlDocCommentStyle StyleOf(LexerMode mode)
	{
		return (XmlDocCommentStyle)((int)(mode & LexerMode.MaskXmlDocCommentStyle) >> 20);
	}

	private bool StyleIs(XmlDocCommentStyle style)
	{
		return StyleOf(_mode) == style;
	}

	public SyntaxToken Lex(ref LexerMode mode)
	{
		SyntaxToken result = Lex(mode);
		mode = _mode;
		return result;
	}

	public SyntaxToken Lex(LexerMode mode)
	{
		_mode = mode;
		switch (_mode)
		{
		case LexerMode.Syntax:
		case LexerMode.DebuggerSyntax:
			return QuickScanSyntaxToken() ?? LexSyntaxToken();
		case LexerMode.Directive:
			return LexDirectiveToken();
		default:
			switch (ModeOf(_mode))
			{
			case LexerMode.XmlDocComment:
				return LexXmlToken();
			case LexerMode.XmlElementTag:
				return LexXmlElementTagToken();
			case LexerMode.XmlAttributeTextQuote:
			case LexerMode.XmlAttributeTextDoubleQuote:
				return LexXmlAttributeTextToken();
			case LexerMode.XmlCDataSectionText:
				return LexXmlCDataSectionTextToken();
			case LexerMode.XmlCommentText:
				return LexXmlCommentTextToken();
			case LexerMode.XmlProcessingInstructionText:
				return LexXmlProcessingInstructionTextToken();
			case LexerMode.XmlCrefQuote:
			case LexerMode.XmlCrefDoubleQuote:
				return LexXmlCrefOrNameToken();
			case LexerMode.XmlNameQuote:
			case LexerMode.XmlNameDoubleQuote:
				return LexXmlCrefOrNameToken();
			case LexerMode.XmlCharacter:
				return LexXmlCharacter();
			default:
				throw ExceptionUtilities.UnexpectedValue(ModeOf(_mode));
			}
		}
	}

	private static int GetFullWidth(SyntaxListBuilder builder)
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

	private SyntaxToken LexSyntaxToken()
	{
		_leadingTriviaCache.Clear();
		LexSyntaxTrivia(TextWindow.Position > 0, isTrailing: false, ref _leadingTriviaCache);
		SyntaxListBuilder leadingTriviaCache = _leadingTriviaCache;
		TokenInfo info = default;
		Start();
		ScanSyntaxToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors(GetFullWidth(leadingTriviaCache));
		_trailingTriviaCache.Clear();
		LexSyntaxTrivia(afterFirstToken: true, isTrailing: true, ref _trailingTriviaCache);
		SyntaxListBuilder trailingTriviaCache = _trailingTriviaCache;
		return Create(ref info, leadingTriviaCache, trailingTriviaCache, errors);
	}

	internal SyntaxTriviaList LexSyntaxLeadingTrivia()
	{
		_leadingTriviaCache.Clear();
		LexSyntaxTrivia(TextWindow.Position > 0, isTrailing: false, ref _leadingTriviaCache);
		return new SyntaxTriviaList(default(Microsoft.CodeAnalysis.SyntaxToken), _leadingTriviaCache.ToListNode(), 0);
	}

	internal SyntaxTriviaList LexSyntaxTrailingTrivia()
	{
		_trailingTriviaCache.Clear();
		LexSyntaxTrivia(afterFirstToken: true, isTrailing: true, ref _trailingTriviaCache);
		return new SyntaxTriviaList(default(Microsoft.CodeAnalysis.SyntaxToken), _trailingTriviaCache.ToListNode(), 0);
	}

	private SyntaxToken Create(ref TokenInfo info, SyntaxListBuilder leading, SyntaxListBuilder trailing, SyntaxDiagnosticInfo[] errors)
	{
		GreenNode leading2 = leading?.ToListNode();
		GreenNode trailing2 = trailing?.ToListNode();
		SyntaxToken syntaxToken;
		if (info.RequiresTextForXmlEntity)
		{
			syntaxToken = SyntaxFactory.Token(leading2, info.Kind, info.Text, info.StringValue, trailing2);
		}
		else
		{
			switch (info.Kind)
			{
			case SyntaxKind.IdentifierToken:
				syntaxToken = SyntaxFactory.Identifier(info.ContextualKind, leading2, info.Text, info.StringValue, trailing2);
				break;
			case SyntaxKind.NumericLiteralToken:
				syntaxToken = info.ValueKind switch
				{
					SpecialType.System_Int32 => SyntaxFactory.Literal(leading2, info.Text, info.IntValue, trailing2), 
					SpecialType.System_UInt32 => SyntaxFactory.Literal(leading2, info.Text, info.UintValue, trailing2), 
					SpecialType.System_Int64 => SyntaxFactory.Literal(leading2, info.Text, info.LongValue, trailing2), 
					SpecialType.System_UInt64 => SyntaxFactory.Literal(leading2, info.Text, info.UlongValue, trailing2), 
					SpecialType.System_Single => SyntaxFactory.Literal(leading2, info.Text, info.FloatValue, trailing2), 
					SpecialType.System_Double => SyntaxFactory.Literal(leading2, info.Text, info.DoubleValue, trailing2), 
					SpecialType.System_Decimal => SyntaxFactory.Literal(leading2, info.Text, info.DecimalValue, trailing2), 
					_ => throw ExceptionUtilities.UnexpectedValue(info.ValueKind), 
				};
				break;
			case SyntaxKind.InterpolatedStringToken:
				syntaxToken = SyntaxFactory.Literal(leading2, info.Text, info.Kind, info.Text, trailing2);
				break;
			case SyntaxKind.StringLiteralToken:
				syntaxToken = SyntaxFactory.Literal(leading2, info.Text, info.Kind, info.StringValue, trailing2);
				break;
			case SyntaxKind.CharacterLiteralToken:
				syntaxToken = SyntaxFactory.Literal(leading2, info.Text, info.CharValue, trailing2);
				break;
			case SyntaxKind.XmlTextLiteralNewLineToken:
				syntaxToken = SyntaxFactory.XmlTextNewLine(leading2, info.Text, info.StringValue, trailing2);
				break;
			case SyntaxKind.XmlTextLiteralToken:
				syntaxToken = SyntaxFactory.XmlTextLiteral(leading2, info.Text, info.StringValue, trailing2);
				break;
			case SyntaxKind.XmlEntityLiteralToken:
				syntaxToken = SyntaxFactory.XmlEntity(leading2, info.Text, info.StringValue, trailing2);
				break;
			case SyntaxKind.EndOfDocumentationCommentToken:
			case SyntaxKind.EndOfFileToken:
				syntaxToken = SyntaxFactory.Token(leading2, info.Kind, trailing2);
				break;
			case SyntaxKind.None:
				syntaxToken = SyntaxFactory.BadToken(leading2, info.Text, trailing2);
				break;
			default:
				syntaxToken = SyntaxFactory.Token(leading2, info.Kind, trailing2);
				break;
			}
		}
		if (errors != null && ((int)_options.DocumentationMode >= 2 || !InDocumentationComment))
		{
			syntaxToken = syntaxToken.WithDiagnosticsGreen(errors);
		}
		return syntaxToken;
	}

	private void ScanSyntaxToken(ref TokenInfo info)
	{
		info.Kind = SyntaxKind.None;
		info.ContextualKind = SyntaxKind.None;
		info.Text = null;
		char surrogateCharacter = '\uffff';
		bool flag = false;
		int position = TextWindow.Position;
		char c = TextWindow.PeekChar();
		switch (c)
		{
		case '"':
		case '\'':
			ScanStringLiteral(ref info);
			break;
		case '/':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.SlashEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.SlashToken;
			}
			break;
		case '.':
			if (!ScanNumericLiteral(ref info))
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.DotToken;
			}
			break;
		case ',':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CommaToken;
			break;
		case ':':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == ':')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.ColonColonToken;
			}
			else
			{
				info.Kind = SyntaxKind.ColonToken;
			}
			break;
		case ';':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.SemicolonToken;
			break;
		case '~':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.TildeToken;
			break;
		case '!':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.ExclamationEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.ExclamationToken;
			}
			break;
		case '=':
			TextWindow.AdvanceChar();
			if ((c = TextWindow.PeekChar()) == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.EqualsEqualsToken;
			}
			else if (c == '>')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.EqualsGreaterThanToken;
			}
			else
			{
				info.Kind = SyntaxKind.EqualsToken;
			}
			break;
		case '*':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.AsteriskEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.AsteriskToken;
			}
			break;
		case '(':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.OpenParenToken;
			break;
		case ')':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CloseParenToken;
			break;
		case '{':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.OpenBraceToken;
			break;
		case '}':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CloseBraceToken;
			break;
		case '[':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.OpenBracketToken;
			break;
		case ']':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CloseBracketToken;
			break;
		case '?':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '?')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.QuestionQuestionToken;
			}
			else
			{
				info.Kind = SyntaxKind.QuestionToken;
			}
			break;
		case '+':
			TextWindow.AdvanceChar();
			if ((c = TextWindow.PeekChar()) == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.PlusEqualsToken;
			}
			else if (c == '+')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.PlusPlusToken;
			}
			else
			{
				info.Kind = SyntaxKind.PlusToken;
			}
			break;
		case '-':
			TextWindow.AdvanceChar();
			if ((c = TextWindow.PeekChar()) == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.MinusEqualsToken;
				break;
			}
			switch (c)
			{
			case '-':
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.MinusMinusToken;
				break;
			case '>':
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.MinusGreaterThanToken;
				break;
			default:
				info.Kind = SyntaxKind.MinusToken;
				break;
			}
			break;
		case '%':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.PercentEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.PercentToken;
			}
			break;
		case '&':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.AmpersandEqualsToken;
			}
			else if (TextWindow.PeekChar() == '&')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.AmpersandAmpersandToken;
			}
			else
			{
				info.Kind = SyntaxKind.AmpersandToken;
			}
			break;
		case '^':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.CaretEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.CaretToken;
			}
			break;
		case '|':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.BarEqualsToken;
			}
			else if (TextWindow.PeekChar() == '|')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.BarBarToken;
			}
			else
			{
				info.Kind = SyntaxKind.BarToken;
			}
			break;
		case '<':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.LessThanEqualsToken;
			}
			else if (TextWindow.PeekChar() == '<')
			{
				TextWindow.AdvanceChar();
				if (TextWindow.PeekChar() == '=')
				{
					TextWindow.AdvanceChar();
					info.Kind = SyntaxKind.LessThanLessThanEqualsToken;
				}
				else
				{
					info.Kind = SyntaxKind.LessThanLessThanToken;
				}
			}
			else
			{
				info.Kind = SyntaxKind.LessThanToken;
			}
			break;
		case '>':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.GreaterThanEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.GreaterThanToken;
			}
			break;
		case '@':
			if (TextWindow.PeekChar(1) == '"')
			{
				ScanVerbatimStringLiteral(ref info);
			}
			else if (!ScanIdentifierOrKeyword(ref info))
			{
				TextWindow.AdvanceChar();
				info.Text = TextWindow.GetText(intern: true);
				AddError(ErrorCode.ERR_ExpectedVerbatimLiteral);
			}
			break;
		case '$':
			if (TextWindow.PeekChar(1) == '"')
			{
				ScanInterpolatedStringLiteral(isVerbatim: false, ref info);
				CheckFeatureAvailability(MessageID.IDS_FeatureInterpolatedStrings);
				break;
			}
			if (TextWindow.PeekChar(1) == '@' && TextWindow.PeekChar(2) == '"')
			{
				ScanInterpolatedStringLiteral(isVerbatim: true, ref info);
				CheckFeatureAvailability(MessageID.IDS_FeatureInterpolatedStrings);
				break;
			}
			if (ModeIs(LexerMode.DebuggerSyntax))
			{
				goto case 'A';
			}
			goto default;
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
		case '\\':
			c = TextWindow.PeekCharOrUnicodeEscape(out surrogateCharacter);
			flag = true;
			if (SyntaxFacts.IsIdentifierStartCharacter(c))
			{
				goto case 'A';
			}
			goto default;
		case '\uffff':
			if (TextWindow.IsReallyAtEnd())
			{
				if (_directives.HasUnfinishedIf())
				{
					AddError(ErrorCode.ERR_EndifDirectiveExpected);
				}
				if (_directives.HasUnfinishedRegion())
				{
					AddError(ErrorCode.ERR_EndRegionDirectiveExpected);
				}
				info.Kind = SyntaxKind.EndOfFileToken;
				break;
			}
			goto default;
		default:
			if (!SyntaxFacts.IsIdentifierStartCharacter(c))
			{
				if (flag)
				{
					TextWindow.NextCharOrUnicodeEscape(out surrogateCharacter, out var info2);
					AddError(info2);
				}
				else
				{
					TextWindow.AdvanceChar();
				}
				if (_badTokenCount++ > 200)
				{
					int length = TextWindow.Text.Length;
					int length2 = length - position;
					info.Text = TextWindow.Text.ToString(new TextSpan(position, length2));
					TextWindow.Reset(length);
				}
				else
				{
					info.Text = TextWindow.GetText(intern: true);
				}
				AddError(ErrorCode.ERR_UnexpectedCharacter, info.Text);
				break;
			}
			goto case 'A';
		}
	}

	private void CheckFeatureAvailability(MessageID feature)
	{
		if (!Options.IsFeatureEnabled(feature))
		{
			LanguageVersion languageVersion = Options.LanguageVersion;
			LanguageVersion languageVersion2 = feature.RequiredVersion();
			if (languageVersion < languageVersion2)
			{
				LocalizableErrorArgument localizableErrorArgument = feature.Localize();
				AddError(languageVersion.GetErrorCode(), localizableErrorArgument, new CSharpRequiredLanguageVersion(languageVersion2));
			}
		}
	}

	private bool ScanInteger()
	{
		int position = TextWindow.Position;
		char c;
		while ((c = TextWindow.PeekChar()) >= '0' && c <= '9')
		{
			TextWindow.AdvanceChar();
		}
		return position < TextWindow.Position;
	}

	private void ScanNumericLiteralSingleInteger(ref bool underscoreInWrongPlace, ref bool usedUnderscore, ref bool firstCharWasUnderscore, bool isHex, bool isBinary)
	{
		if (TextWindow.PeekChar() == '_')
		{
			if (isHex | isBinary)
			{
				firstCharWasUnderscore = true;
			}
			else
			{
				underscoreInWrongPlace = true;
			}
		}
		bool flag = false;
		while (true)
		{
			char c = TextWindow.PeekChar();
			if (c == '_')
			{
				usedUnderscore = true;
				flag = true;
			}
			else
			{
				if (!(isHex ? SyntaxFacts.IsHexDigit(c) : (isBinary ? SyntaxFacts.IsBinaryDigit(c) : SyntaxFacts.IsDecDigit(c))))
				{
					break;
				}
				_builder.Append(c);
				flag = false;
			}
			TextWindow.AdvanceChar();
		}
		if (flag)
		{
			underscoreInWrongPlace = true;
		}
	}

	private bool ScanNumericLiteral(ref TokenInfo info)
	{
		int position = TextWindow.Position;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		info.Text = null;
		info.ValueKind = SpecialType.None;
		_builder.Clear();
		bool flag5 = false;
		bool flag6 = false;
		bool underscoreInWrongPlace = false;
		bool usedUnderscore = false;
		bool firstCharWasUnderscore = false;
		char c = TextWindow.PeekChar();
		if (c == '0')
		{
			switch (TextWindow.PeekChar(1))
			{
			case 'X':
			case 'x':
				TextWindow.AdvanceChar(2);
				flag = true;
				break;
			case 'B':
			case 'b':
				CheckFeatureAvailability(MessageID.IDS_FeatureBinaryLiteral);
				TextWindow.AdvanceChar(2);
				flag2 = true;
				break;
			}
		}
		if (flag | flag2)
		{
			ScanNumericLiteralSingleInteger(ref underscoreInWrongPlace, ref usedUnderscore, ref firstCharWasUnderscore, flag, flag2);
			if ((c = TextWindow.PeekChar()) == 'L' || c == 'l')
			{
				if (c == 'l')
				{
					AddError(TextWindow.Position, 1, ErrorCode.WRN_LowercaseEllSuffix);
				}
				TextWindow.AdvanceChar();
				flag6 = true;
				if ((c = TextWindow.PeekChar()) == 'u' || c == 'U')
				{
					TextWindow.AdvanceChar();
					flag5 = true;
				}
			}
			else if ((c = TextWindow.PeekChar()) == 'u' || c == 'U')
			{
				TextWindow.AdvanceChar();
				flag5 = true;
				if ((c = TextWindow.PeekChar()) == 'L' || c == 'l')
				{
					TextWindow.AdvanceChar();
					flag6 = true;
				}
			}
		}
		else
		{
			ScanNumericLiteralSingleInteger(ref underscoreInWrongPlace, ref usedUnderscore, ref firstCharWasUnderscore, isHex: false, isBinary: false);
			if (ModeIs(LexerMode.DebuggerSyntax) && TextWindow.PeekChar() == '#')
			{
				TextWindow.AdvanceChar();
				info.StringValue = (info.Text = TextWindow.GetText(intern: true));
				info.Kind = SyntaxKind.IdentifierToken;
				AddError(AbstractLexer.MakeError(ErrorCode.ERR_LegacyObjectIdSyntax));
				return true;
			}
			if ((c = TextWindow.PeekChar()) == '.')
			{
				char c2 = TextWindow.PeekChar(1);
				if (c2 >= '0' && c2 <= '9')
				{
					flag3 = true;
					_builder.Append(c);
					TextWindow.AdvanceChar();
					ScanNumericLiteralSingleInteger(ref underscoreInWrongPlace, ref usedUnderscore, ref firstCharWasUnderscore, isHex: false, isBinary: false);
				}
				else if (_builder.Length == 0)
				{
					info.Kind = SyntaxKind.DotToken;
					TextWindow.Reset(position);
					return false;
				}
			}
			if ((c = TextWindow.PeekChar()) == 'E' || c == 'e')
			{
				_builder.Append(c);
				TextWindow.AdvanceChar();
				flag4 = true;
				if ((c = TextWindow.PeekChar()) == '-' || c == '+')
				{
					_builder.Append(c);
					TextWindow.AdvanceChar();
				}
				if (((c = TextWindow.PeekChar()) < '0' || c > '9') && c != '_')
				{
					AddError(AbstractLexer.MakeError(ErrorCode.ERR_InvalidReal));
					_builder.Append('0');
				}
				else
				{
					ScanNumericLiteralSingleInteger(ref underscoreInWrongPlace, ref usedUnderscore, ref firstCharWasUnderscore, isHex: false, isBinary: false);
				}
			}
			if (flag4 | flag3)
			{
				if ((c = TextWindow.PeekChar()) == 'f' || c == 'F')
				{
					TextWindow.AdvanceChar();
					info.ValueKind = SpecialType.System_Single;
				}
				else
				{
					switch (c)
					{
					case 'D':
					case 'd':
						TextWindow.AdvanceChar();
						info.ValueKind = SpecialType.System_Double;
						break;
					case 'M':
					case 'm':
						TextWindow.AdvanceChar();
						info.ValueKind = SpecialType.System_Decimal;
						break;
					default:
						info.ValueKind = SpecialType.System_Double;
						break;
					}
				}
			}
			else if ((c = TextWindow.PeekChar()) == 'f' || c == 'F')
			{
				TextWindow.AdvanceChar();
				info.ValueKind = SpecialType.System_Single;
			}
			else
			{
				switch (c)
				{
				case 'D':
				case 'd':
					TextWindow.AdvanceChar();
					info.ValueKind = SpecialType.System_Double;
					break;
				case 'M':
				case 'm':
					TextWindow.AdvanceChar();
					info.ValueKind = SpecialType.System_Decimal;
					break;
				case 'L':
				case 'l':
					if (c == 'l')
					{
						AddError(TextWindow.Position, 1, ErrorCode.WRN_LowercaseEllSuffix);
					}
					TextWindow.AdvanceChar();
					flag6 = true;
					if ((c = TextWindow.PeekChar()) == 'u' || c == 'U')
					{
						TextWindow.AdvanceChar();
						flag5 = true;
					}
					break;
				case 'U':
				case 'u':
					flag5 = true;
					TextWindow.AdvanceChar();
					if ((c = TextWindow.PeekChar()) == 'L' || c == 'l')
					{
						TextWindow.AdvanceChar();
						flag6 = true;
					}
					break;
				}
			}
		}
		if (underscoreInWrongPlace)
		{
			AddError(MakeError(position, TextWindow.Position - position, ErrorCode.ERR_InvalidNumber));
		}
		else if (firstCharWasUnderscore)
		{
			CheckFeatureAvailability(MessageID.IDS_FeatureLeadingDigitSeparator);
		}
		else if (usedUnderscore)
		{
			CheckFeatureAvailability(MessageID.IDS_FeatureDigitSeparator);
		}
		info.Kind = SyntaxKind.NumericLiteralToken;
		info.Text = TextWindow.GetText(intern: true);
		string text = TextWindow.Intern(_builder);
		switch (info.ValueKind)
		{
		case SpecialType.System_Single:
			info.FloatValue = GetValueSingle(text);
			break;
		case SpecialType.System_Double:
			info.DoubleValue = GetValueDouble(text);
			break;
		case SpecialType.System_Decimal:
			info.DecimalValue = GetValueDecimal(text, position, TextWindow.Position);
			break;
		default:
		{
			ulong num;
			if (string.IsNullOrEmpty(text))
			{
				if (!underscoreInWrongPlace)
				{
					AddError(AbstractLexer.MakeError(ErrorCode.ERR_InvalidNumber));
				}
				num = 0uL;
			}
			else
			{
				num = GetValueUInt64(text, flag, flag2);
			}
			if (!flag5 && !flag6)
			{
				if (num <= int.MaxValue)
				{
					info.ValueKind = SpecialType.System_Int32;
					info.IntValue = (int)num;
				}
				else if (num <= uint.MaxValue)
				{
					info.ValueKind = SpecialType.System_UInt32;
					info.UintValue = (uint)num;
				}
				else if (num <= long.MaxValue)
				{
					info.ValueKind = SpecialType.System_Int64;
					info.LongValue = (long)num;
				}
				else
				{
					info.ValueKind = SpecialType.System_UInt64;
					info.UlongValue = num;
				}
			}
			else if (flag5 && !flag6)
			{
				if (num <= uint.MaxValue)
				{
					info.ValueKind = SpecialType.System_UInt32;
					info.UintValue = (uint)num;
				}
				else
				{
					info.ValueKind = SpecialType.System_UInt64;
					info.UlongValue = num;
				}
			}
			else if (!flag5 & flag6)
			{
				if (num <= long.MaxValue)
				{
					info.ValueKind = SpecialType.System_Int64;
					info.LongValue = (long)num;
				}
				else
				{
					info.ValueKind = SpecialType.System_UInt64;
					info.UlongValue = num;
				}
			}
			else
			{
				info.ValueKind = SpecialType.System_UInt64;
				info.UlongValue = num;
			}
			break;
		}
		}
		return true;
	}

	private static bool TryParseBinaryUInt64(string text, out ulong value)
	{
		value = 0uL;
		foreach (char c in text)
		{
			if ((value & 0x8000000000000000uL) != 0L)
			{
				return false;
			}
			ulong num = (ulong)SyntaxFacts.BinaryValue(c);
			value = (value << 1) | num;
		}
		return true;
	}

	private int GetValueInt32(string text, bool isHex)
	{
		if (!int.TryParse(text, isHex ? NumberStyles.AllowHexSpecifier : NumberStyles.None, CultureInfo.InvariantCulture, out var result))
		{
			AddError(AbstractLexer.MakeError(ErrorCode.ERR_IntOverflow));
		}
		return result;
	}

	private ulong GetValueUInt64(string text, bool isHex, bool isBinary)
	{
		ulong result;
		if (isBinary)
		{
			if (!TryParseBinaryUInt64(text, out result))
			{
				AddError(AbstractLexer.MakeError(ErrorCode.ERR_IntOverflow));
			}
		}
		else if (!ulong.TryParse(text, isHex ? NumberStyles.AllowHexSpecifier : NumberStyles.None, CultureInfo.InvariantCulture, out result))
		{
			AddError(AbstractLexer.MakeError(ErrorCode.ERR_IntOverflow));
		}
		return result;
	}

	private double GetValueDouble(string text)
	{
		if (!RealParser.TryParseDouble(text, out var d))
		{
			AddError(AbstractLexer.MakeError(ErrorCode.ERR_FloatOverflow, "double"));
		}
		return d;
	}

	private float GetValueSingle(string text)
	{
		if (!RealParser.TryParseFloat(text, out var f))
		{
			AddError(AbstractLexer.MakeError(ErrorCode.ERR_FloatOverflow, "float"));
		}
		return f;
	}

	private decimal GetValueDecimal(string text, int start, int end)
	{
		if (!decimal.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out var result))
		{
			AddError(MakeError(start, end - start, ErrorCode.ERR_FloatOverflow, "decimal"));
		}
		return result;
	}

	private void ResetIdentBuffer()
	{
		_identLen = 0;
	}

	private void AddIdentChar(char ch)
	{
		if (_identLen >= _identBuffer.Length)
		{
			GrowIdentBuffer();
		}
		_identBuffer[_identLen++] = ch;
	}

	private void GrowIdentBuffer()
	{
		char[] array = new char[_identBuffer.Length * 2];
		Array.Copy(_identBuffer, array, _identBuffer.Length);
		_identBuffer = array;
	}

	private bool ScanIdentifier(ref TokenInfo info)
	{
		if (!ScanIdentifier_FastPath(ref info))
		{
			if (!InXmlCrefOrNameAttributeValue)
			{
				return ScanIdentifier_SlowPath(ref info);
			}
			return ScanIdentifier_CrefSlowPath(ref info);
		}
		return true;
	}

	private bool ScanIdentifier_FastPath(ref TokenInfo info)
	{
		if ((_mode & LexerMode.MaskLexMode) == LexerMode.DebuggerSyntax)
		{
			return false;
		}
		int i = TextWindow.Offset;
		char[] characterWindow = TextWindow.CharacterWindow;
		int characterWindowCount = TextWindow.CharacterWindowCount;
		int num = i;
		for (; i != characterWindowCount; i++)
		{
			switch (characterWindow[i])
			{
			case '&':
				if (InXmlCrefOrNameAttributeValue)
				{
					return false;
				}
				goto case '\0';
			case '\0':
			case '\t':
			case '\n':
			case '\r':
			case ' ':
			case '!':
			case '"':
			case '%':
			case '\'':
			case '(':
			case ')':
			case '*':
			case '+':
			case ',':
			case '-':
			case '.':
			case '/':
			case ':':
			case ';':
			case '<':
			case '=':
			case '>':
			case '?':
			case '[':
			case ']':
			case '^':
			case '{':
			case '|':
			case '}':
			case '~':
			{
				int num2 = i - num;
				TextWindow.AdvanceChar(num2);
				info.Text = (info.StringValue = TextWindow.Intern(characterWindow, num, num2));
				info.IsVerbatim = false;
				return true;
			}
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
				if (i == num)
				{
					return false;
				}
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
				break;
			default:
				return false;
			}
		}
		return false;
	}

	private bool ScanIdentifier_SlowPath(ref TokenInfo info)
	{
		int position = TextWindow.Position;
		ResetIdentBuffer();
		info.IsVerbatim = TextWindow.PeekChar() == '@';
		if (info.IsVerbatim)
		{
			TextWindow.AdvanceChar();
		}
		bool flag = false;
		while (true)
		{
			char surrogateCharacter = '\uffff';
			bool flag2 = false;
			char c = TextWindow.PeekChar();
			while (true)
			{
				switch (c)
				{
				case '\\':
					if (!flag2 && TextWindow.IsUnicodeEscape())
					{
						goto IL_0249;
					}
					goto default;
				case '$':
					if (ModeIs(LexerMode.DebuggerSyntax) && _identLen <= 0)
					{
						goto case 'A';
					}
					goto case '\t';
				case '\uffff':
					if (!TextWindow.IsReallyAtEnd())
					{
						goto default;
					}
					goto case '\t';
				case '0':
					if (_identLen == 0)
					{
						if (!info.IsVerbatim || !ModeIs(LexerMode.DebuggerSyntax) || char.ToLower(TextWindow.PeekChar(1)) != 'x')
						{
							goto case '\t';
						}
						flag = true;
					}
					goto case 'A';
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					if (_identLen != 0)
					{
						goto case 'A';
					}
					goto case '\t';
				case '<':
					if (_identLen == 0 && ModeIs(LexerMode.DebuggerSyntax) && TextWindow.PeekChar(1) == '>')
					{
						TextWindow.AdvanceChar(2);
						AddIdentChar('<');
						AddIdentChar('>');
						break;
					}
					goto case '\t';
				default:
					if (_identLen != 0 || c <= '\u007f' || !SyntaxFacts.IsIdentifierStartCharacter(c))
					{
						if (_identLen <= 0 || c <= '\u007f' || !SyntaxFacts.IsIdentifierPartCharacter(c))
						{
							goto case '\t';
						}
						if (UnicodeCharacterUtilities.IsFormattingChar(c))
						{
							if (flag2)
							{
								TextWindow.NextCharOrUnicodeEscape(out surrogateCharacter, out var info3);
								AddError(info3);
							}
							else
							{
								TextWindow.AdvanceChar();
							}
							break;
						}
					}
					goto case 'A';
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
					if (flag2)
					{
						TextWindow.NextCharOrUnicodeEscape(out surrogateCharacter, out var info2);
						AddError(info2);
					}
					else
					{
						TextWindow.AdvanceChar();
					}
					AddIdentChar(c);
					if (surrogateCharacter != '\uffff')
					{
						AddIdentChar(surrogateCharacter);
					}
					break;
				case '\t':
				case ' ':
				case '(':
				case ')':
				case ',':
				case '.':
				case ';':
					{
						int width = TextWindow.Width;
						if (_identLen > 0)
						{
							info.Text = TextWindow.GetInternedText();
							if (_identLen == width)
							{
								info.StringValue = info.Text;
							}
							else
							{
								info.StringValue = TextWindow.Intern(_identBuffer, 0, _identLen);
							}
							if (flag)
							{
								string text = TextWindow.Intern(_identBuffer, 2, _identLen - 2);
								if (text.Length == 0 || !text.All(IsValidHexDigit))
								{
									goto IL_0497;
								}
								GetValueUInt64(text, isHex: true, isBinary: false);
							}
							return true;
						}
						goto IL_0497;
					}
					IL_0497:
					info.Text = null;
					info.StringValue = null;
					TextWindow.Reset(position);
					return false;
				}
				break;
				IL_0249:
				info.HasIdentifierEscapeSequence = true;
				flag2 = true;
				c = TextWindow.PeekUnicodeEscape(out surrogateCharacter);
			}
		}
	}

	private static bool IsValidHexDigit(char c)
	{
		if (c >= '0' && c <= '9')
		{
			return true;
		}
		c = char.ToLower(c);
		if (c >= 'a')
		{
			return c <= 'f';
		}
		return false;
	}

	private bool ScanIdentifier_CrefSlowPath(ref TokenInfo info)
	{
		int position = TextWindow.Position;
		ResetIdentBuffer();
		if (AdvanceIfMatches('@'))
		{
			if (InXmlNameAttributeValue)
			{
				AddIdentChar('@');
			}
			else
			{
				info.IsVerbatim = true;
			}
		}
		while (true)
		{
			int position2 = TextWindow.Position;
			char ch;
			char surrogate;
			if (TextWindow.PeekChar() == '&')
			{
				if (!TextWindow.TryScanXmlEntity(out ch, out surrogate))
				{
					TextWindow.Reset(position2);
					break;
				}
			}
			else
			{
				ch = TextWindow.NextChar();
				surrogate = '\uffff';
			}
			bool flag = false;
			while (true)
			{
				switch (ch)
				{
				case '\\':
					if (!flag && TextWindow.Position == position2 + 1 && (TextWindow.PeekChar() == 'u' || TextWindow.PeekChar() == 'U'))
					{
						info.HasIdentifierEscapeSequence = true;
						TextWindow.Reset(position2);
						flag = true;
						ch = TextWindow.NextUnicodeEscape(out surrogate, out var info2);
						AddCrefError(info2);
						continue;
					}
					goto IL_0318;
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
					break;
				case '\t':
				case ' ':
				case '$':
				case '(':
				case ')':
				case ',':
				case '.':
				case ';':
				case '<':
					goto IL_02ef;
				case '\uffff':
					goto IL_02fd;
				default:
					goto IL_0318;
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
					goto IL_035e;
				}
				break;
			}
			if (_identLen == 0)
			{
				TextWindow.Reset(position2);
				break;
			}
			goto IL_035e;
			IL_02fd:
			if (TextWindow.IsReallyAtEnd())
			{
				TextWindow.Reset(position2);
				break;
			}
			goto IL_0318;
			IL_02ef:
			TextWindow.Reset(position2);
			break;
			IL_035e:
			AddIdentChar(ch);
			if (surrogate != '\uffff')
			{
				AddIdentChar(surrogate);
			}
			continue;
			IL_0318:
			if (_identLen != 0 || ch <= '\u007f' || !SyntaxFacts.IsIdentifierStartCharacter(ch))
			{
				if (_identLen <= 0 || ch <= '\u007f' || !SyntaxFacts.IsIdentifierPartCharacter(ch))
				{
					TextWindow.Reset(position2);
					break;
				}
				if (UnicodeCharacterUtilities.IsFormattingChar(ch))
				{
					continue;
				}
			}
			goto IL_035e;
		}
		if (_identLen > 0)
		{
			int width = TextWindow.Width;
			if (_identLen == width)
			{
				info.StringValue = TextWindow.GetInternedText();
				info.Text = info.StringValue;
			}
			else
			{
				info.StringValue = TextWindow.Intern(_identBuffer, 0, _identLen);
				info.Text = TextWindow.GetText(intern: false);
			}
			return true;
		}
		info.Text = null;
		info.StringValue = null;
		TextWindow.Reset(position);
		return false;
	}

	private bool ScanIdentifierOrKeyword(ref TokenInfo info)
	{
		info.ContextualKind = SyntaxKind.None;
		if (ScanIdentifier(ref info))
		{
			if (!info.IsVerbatim && !info.HasIdentifierEscapeSequence)
			{
				if (ModeIs(LexerMode.Directive))
				{
					SyntaxKind preprocessorKeywordKind = SyntaxFacts.GetPreprocessorKeywordKind(info.Text);
					if (SyntaxFacts.IsPreprocessorContextualKeyword(preprocessorKeywordKind))
					{
						info.Kind = SyntaxKind.IdentifierToken;
						info.ContextualKind = preprocessorKeywordKind;
					}
					else
					{
						info.Kind = preprocessorKeywordKind;
					}
				}
				else if (!_cache.TryGetKeywordKind(info.Text, out info.Kind))
				{
					info.ContextualKind = (info.Kind = SyntaxKind.IdentifierToken);
				}
				else if (SyntaxFacts.IsContextualKeyword(info.Kind))
				{
					info.ContextualKind = info.Kind;
					info.Kind = SyntaxKind.IdentifierToken;
				}
				if (info.Kind == SyntaxKind.None)
				{
					info.Kind = SyntaxKind.IdentifierToken;
				}
			}
			else
			{
				info.ContextualKind = (info.Kind = SyntaxKind.IdentifierToken);
			}
			return true;
		}
		info.Kind = SyntaxKind.None;
		return false;
	}

	private void LexSyntaxTrivia(bool afterFirstToken, bool isTrailing, ref SyntaxListBuilder triviaList)
	{
		bool flag = !isTrailing;
		while (true)
		{
			Start();
			char c = TextWindow.PeekChar();
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
			case '/':
			{
				if ((c = TextWindow.PeekChar(1)) == '/')
				{
					if (!SuppressDocumentationCommentParse && TextWindow.PeekChar(2) == '/' && TextWindow.PeekChar(3) != '/')
					{
						if (isTrailing)
						{
							return;
						}
						AddTrivia(LexXmlDocComment(XmlDocCommentStyle.SingleLine), ref triviaList);
					}
					else
					{
						ScanToEndOfLine();
						string text = TextWindow.GetText(intern: false);
						AddTrivia(SyntaxFactory.Comment(text), ref triviaList);
						flag = false;
					}
					break;
				}
				if (c != '*')
				{
					return;
				}
				if (!SuppressDocumentationCommentParse && TextWindow.PeekChar(2) == '*' && TextWindow.PeekChar(3) != '*' && TextWindow.PeekChar(3) != '/')
				{
					if (isTrailing)
					{
						return;
					}
					AddTrivia(LexXmlDocComment(XmlDocCommentStyle.Delimited), ref triviaList);
					break;
				}
				ScanMultiLineComment(out var isTerminated);
				if (!isTerminated)
				{
					AddError(ErrorCode.ERR_OpenEndedComment);
				}
				string text2 = TextWindow.GetText(intern: false);
				AddTrivia(SyntaxFactory.Comment(text2), ref triviaList);
				flag = false;
				break;
			}
			case '\n':
			case '\r':
				AddTrivia(ScanEndOfLine(), ref triviaList);
				if (isTrailing)
				{
					return;
				}
				flag = true;
				break;
			case '#':
				if (_allowPreprocessorDirectives)
				{
					LexDirectiveAndExcludedTrivia(afterFirstToken, isTrailing || !flag, ref triviaList);
					break;
				}
				return;
			case '<':
			case '=':
				if (!isTrailing && IsConflictMarkerTrivia())
				{
					LexConflictMarkerTrivia(ref triviaList);
					break;
				}
				return;
			}
		}
	}

	private bool IsConflictMarkerTrivia()
	{
		int position = TextWindow.Position;
		SourceText text = TextWindow.Text;
		if (position == 0 || SyntaxFacts.IsNewLine(text[position - 1]))
		{
			char c = text[position];
			if (position + s_conflictMarkerLength <= text.Length)
			{
				int i = 0;
				for (int num = s_conflictMarkerLength; i < num; i++)
				{
					if (text[position + i] != c)
					{
						return false;
					}
				}
				if (c == '=')
				{
					return true;
				}
				if (position + s_conflictMarkerLength < text.Length)
				{
					return text[position + s_conflictMarkerLength] == ' ';
				}
				return false;
			}
		}
		return false;
	}

	private void LexConflictMarkerTrivia(ref SyntaxListBuilder triviaList)
	{
		Start();
		AddError(TextWindow.Position, s_conflictMarkerLength, ErrorCode.ERR_Merge_conflict_marker_encountered);
		char c = TextWindow.PeekChar();
		LexConflictMarkerHeader(ref triviaList);
		LexConflictMarkerEndOfLine(ref triviaList);
		if (c == '=')
		{
			LexConflictMarkerDisabledText(ref triviaList);
		}
	}

	private SyntaxListBuilder LexConflictMarkerDisabledText(ref SyntaxListBuilder triviaList)
	{
		Start();
		bool flag = false;
		while (true)
		{
			switch (TextWindow.PeekChar())
			{
			case '>':
				if (!IsConflictMarkerTrivia())
				{
					goto IL_002d;
				}
				flag = true;
				break;
			default:
				goto IL_002d;
			case '\uffff':
				break;
			}
			break;
			IL_002d:
			TextWindow.AdvanceChar();
		}
		if (TextWindow.Width > 0)
		{
			AddTrivia(SyntaxFactory.DisabledText(TextWindow.GetText(intern: false)), ref triviaList);
		}
		if (flag)
		{
			LexConflictMarkerTrivia(ref triviaList);
		}
		return triviaList;
	}

	private void LexConflictMarkerEndOfLine(ref SyntaxListBuilder triviaList)
	{
		Start();
		while (SyntaxFacts.IsNewLine(TextWindow.PeekChar()))
		{
			TextWindow.AdvanceChar();
		}
		if (TextWindow.Width > 0)
		{
			AddTrivia(SyntaxFactory.EndOfLine(TextWindow.GetText(intern: false)), ref triviaList);
		}
	}

	private void LexConflictMarkerHeader(ref SyntaxListBuilder triviaList)
	{
		while (true)
		{
			char c = TextWindow.PeekChar();
			if (c == '\uffff' || SyntaxFacts.IsNewLine(c))
			{
				break;
			}
			TextWindow.AdvanceChar();
		}
		AddTrivia(SyntaxFactory.ConflictMarker(TextWindow.GetText(intern: false)), ref triviaList);
	}

	private void AddTrivia(CSharpSyntaxNode trivia, ref SyntaxListBuilder list)
	{
		if (HasErrors)
		{
			CSharpSyntaxNode node = trivia;
			DiagnosticInfo[] errors = GetErrors(0);
			trivia = node.WithDiagnosticsGreen(errors);
		}
		if (list == null)
		{
			list = new SyntaxListBuilder(8);
		}
		list.Add(trivia);
	}

	private bool ScanMultiLineComment(out bool isTerminated)
	{
		if (TextWindow.PeekChar() == '/' && TextWindow.PeekChar(1) == '*')
		{
			TextWindow.AdvanceChar(2);
			while (true)
			{
				char c;
				if ((c = TextWindow.PeekChar()) == '\uffff' && TextWindow.IsReallyAtEnd())
				{
					isTerminated = false;
					break;
				}
				if (c == '*' && TextWindow.PeekChar(1) == '/')
				{
					TextWindow.AdvanceChar(2);
					isTerminated = true;
					break;
				}
				TextWindow.AdvanceChar();
			}
			return true;
		}
		isTerminated = false;
		return false;
	}

	private void ScanToEndOfLine()
	{
		char c;
		while (!SyntaxFacts.IsNewLine(c = TextWindow.PeekChar()) && (c != '\uffff' || !TextWindow.IsReallyAtEnd()))
		{
			TextWindow.AdvanceChar();
		}
	}

	private CSharpSyntaxNode ScanEndOfLine()
	{
		char ch;
		switch (ch = TextWindow.PeekChar())
		{
		case '\r':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '\n')
			{
				TextWindow.AdvanceChar();
				return SyntaxFactory.CarriageReturnLineFeed;
			}
			return SyntaxFactory.CarriageReturn;
		case '\n':
			TextWindow.AdvanceChar();
			return SyntaxFactory.LineFeed;
		default:
			if (SyntaxFacts.IsNewLine(ch))
			{
				TextWindow.AdvanceChar();
				return SyntaxFactory.EndOfLine(ch.ToString());
			}
			return null;
		}
	}

	private SyntaxTrivia ScanWhitespace()
	{
		if (_createWhitespaceTriviaFunction == null)
		{
			_createWhitespaceTriviaFunction = CreateWhitespaceTrivia;
		}
		int hashCode = -2128831035;
		bool flag = true;
		while (true)
		{
			char c = TextWindow.PeekChar();
			switch (c)
			{
			default:
				if (c != '\u001a')
				{
					if (c == ' ')
					{
						goto IL_0059;
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
				goto IL_0059;
			case '\n':
			case '\r':
				break;
			}
			break;
			IL_0059:
			TextWindow.AdvanceChar();
			hashCode = Hash.CombineFNVHash(hashCode, c);
		}
		if ((TextWindow.Width == 1) & flag)
		{
			return SyntaxFactory.Space;
		}
		int width = TextWindow.Width;
		if (width < 42)
		{
			return _cache.LookupTrivia(TextWindow.CharacterWindow, TextWindow.LexemeRelativeStart, width, hashCode, _createWhitespaceTriviaFunction);
		}
		return _createWhitespaceTriviaFunction();
	}

	private SyntaxTrivia CreateWhitespaceTrivia()
	{
		return SyntaxFactory.Whitespace(TextWindow.GetText(intern: true));
	}

	private void LexDirectiveAndExcludedTrivia(bool afterFirstToken, bool afterNonWhitespaceOnLine, ref SyntaxListBuilder triviaList)
	{
		if (LexSingleDirective(isActive: true, endIsActive: true, afterFirstToken, afterNonWhitespaceOnLine, ref triviaList) is BranchingDirectiveTriviaSyntax { BranchTaken: false })
		{
			LexExcludedDirectivesAndTrivia(endIsActive: true, ref triviaList);
		}
	}

	private void LexExcludedDirectivesAndTrivia(bool endIsActive, ref SyntaxListBuilder triviaList)
	{
		while (true)
		{
			CSharpSyntaxNode cSharpSyntaxNode = LexDisabledText(out var followedByDirective);
			if (cSharpSyntaxNode != null)
			{
				AddTrivia(cSharpSyntaxNode, ref triviaList);
			}
			if (followedByDirective)
			{
				CSharpSyntaxNode cSharpSyntaxNode2 = LexSingleDirective(isActive: false, endIsActive, afterFirstToken: false, afterNonWhitespaceOnLine: false, ref triviaList);
				BranchingDirectiveTriviaSyntax branchingDirectiveTriviaSyntax = cSharpSyntaxNode2 as BranchingDirectiveTriviaSyntax;
				if (cSharpSyntaxNode2.Kind != SyntaxKind.EndIfDirectiveTrivia && (branchingDirectiveTriviaSyntax == null || !branchingDirectiveTriviaSyntax.BranchTaken))
				{
					if (cSharpSyntaxNode2.Kind == SyntaxKind.IfDirectiveTrivia)
					{
						LexExcludedDirectivesAndTrivia(endIsActive: false, ref triviaList);
					}
					continue;
				}
				break;
			}
			break;
		}
	}

	private CSharpSyntaxNode LexSingleDirective(bool isActive, bool endIsActive, bool afterFirstToken, bool afterNonWhitespaceOnLine, ref SyntaxListBuilder triviaList)
	{
		if (SyntaxFacts.IsWhitespace(TextWindow.PeekChar()))
		{
			Start();
			AddTrivia(ScanWhitespace(), ref triviaList);
		}
		LexerMode mode = _mode;
		CSharpSyntaxNode cSharpSyntaxNode;
		using (DirectiveParser directiveParser = new DirectiveParser(this, _directives))
		{
			cSharpSyntaxNode = directiveParser.ParseDirective(isActive, endIsActive, afterFirstToken, afterNonWhitespaceOnLine);
		}
		AddTrivia(cSharpSyntaxNode, ref triviaList);
		_directives = cSharpSyntaxNode.ApplyDirectives(_directives);
		_mode = mode;
		return cSharpSyntaxNode;
	}

	private CSharpSyntaxNode LexDisabledText(out bool followedByDirective)
	{
		Start();
		int position = TextWindow.Position;
		int num = 0;
		bool flag = true;
		while (true)
		{
			char c = TextWindow.PeekChar();
			if ((uint)c <= 13u)
			{
				if (c == '\n' || c == '\r')
				{
					goto IL_00cb;
				}
			}
			else
			{
				switch (c)
				{
				case '\uffff':
					if (TextWindow.IsReallyAtEnd())
					{
						followedByDirective = false;
						if (TextWindow.Width <= 0)
						{
							return null;
						}
						return SyntaxFactory.DisabledText(TextWindow.GetText(intern: false));
					}
					break;
				case '#':
					if (!_allowPreprocessorDirectives)
					{
						break;
					}
					followedByDirective = true;
					if (position >= TextWindow.Position || flag)
					{
						TextWindow.Reset(position);
						if (TextWindow.Width <= 0)
						{
							return null;
						}
						return SyntaxFactory.DisabledText(TextWindow.GetText(intern: false));
					}
					break;
				}
			}
			if (!SyntaxFacts.IsNewLine(c))
			{
				flag = flag && SyntaxFacts.IsWhitespace(c);
				TextWindow.AdvanceChar();
				continue;
			}
			goto IL_00cb;
			IL_00cb:
			ScanEndOfLine();
			position = TextWindow.Position;
			flag = true;
			num++;
		}
	}

	private SyntaxToken LexDirectiveToken()
	{
		Start();
		TokenInfo info = default;
		ScanDirectiveToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors(0);
		SyntaxListBuilder trailing = LexDirectiveTrailingTrivia(info.Kind == SyntaxKind.EndOfDirectiveToken);
		return Create(ref info, null, trailing, errors);
	}

	private bool ScanDirectiveToken(ref TokenInfo info)
	{
		bool flag = false;
		char ch;
		char surrogateCharacter;
		switch (ch = TextWindow.PeekChar())
		{
		case '\uffff':
			if (TextWindow.IsReallyAtEnd())
			{
				info.Kind = SyntaxKind.EndOfDirectiveToken;
				break;
			}
			goto default;
		case '\n':
		case '\r':
			info.Kind = SyntaxKind.EndOfDirectiveToken;
			break;
		case '#':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.HashToken;
			break;
		case '(':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.OpenParenToken;
			break;
		case ')':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CloseParenToken;
			break;
		case ',':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.CommaToken;
			break;
		case '!':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.ExclamationEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.ExclamationToken;
			}
			break;
		case '=':
			TextWindow.AdvanceChar();
			if (TextWindow.PeekChar() == '=')
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.EqualsEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.EqualsToken;
			}
			break;
		case '&':
			if (TextWindow.PeekChar(1) == '&')
			{
				TextWindow.AdvanceChar(2);
				info.Kind = SyntaxKind.AmpersandAmpersandToken;
				break;
			}
			goto default;
		case '|':
			if (TextWindow.PeekChar(1) == '|')
			{
				TextWindow.AdvanceChar(2);
				info.Kind = SyntaxKind.BarBarToken;
				break;
			}
			goto default;
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
			ScanInteger();
			info.Kind = SyntaxKind.NumericLiteralToken;
			info.Text = TextWindow.GetText(intern: true);
			info.ValueKind = SpecialType.System_Int32;
			info.IntValue = GetValueInt32(info.Text, isHex: false);
			break;
		case '"':
			ScanStringLiteral(ref info, allowEscapes: false);
			break;
		case '\\':
			ch = TextWindow.PeekCharOrUnicodeEscape(out surrogateCharacter);
			flag = true;
			if (SyntaxFacts.IsIdentifierStartCharacter(ch))
			{
				ScanIdentifierOrKeyword(ref info);
				break;
			}
			goto default;
		default:
			if (flag || !SyntaxFacts.IsNewLine(ch))
			{
				if (SyntaxFacts.IsIdentifierStartCharacter(ch))
				{
					ScanIdentifierOrKeyword(ref info);
					break;
				}
				if (flag)
				{
					TextWindow.NextCharOrUnicodeEscape(out surrogateCharacter, out var info2);
					AddError(info2);
				}
				else
				{
					TextWindow.AdvanceChar();
				}
				info.Kind = SyntaxKind.None;
				info.Text = TextWindow.GetText(intern: true);
				break;
			}
			goto case '\n';
		}
		return info.Kind != SyntaxKind.None;
	}

	private SyntaxListBuilder LexDirectiveTrailingTrivia(bool includeEndOfLine)
	{
		SyntaxListBuilder list = null;
		while (true)
		{
			int position = TextWindow.Position;
			CSharpSyntaxNode cSharpSyntaxNode = LexDirectiveTrivia();
			if (cSharpSyntaxNode == null)
			{
				break;
			}
			if (cSharpSyntaxNode.Kind == SyntaxKind.EndOfLineTrivia)
			{
				if (includeEndOfLine)
				{
					AddTrivia(cSharpSyntaxNode, ref list);
				}
				else
				{
					TextWindow.Reset(position);
				}
				break;
			}
			AddTrivia(cSharpSyntaxNode, ref list);
		}
		return list;
	}

	private CSharpSyntaxNode LexDirectiveTrivia()
	{
		CSharpSyntaxNode result = null;
		Start();
		char c = TextWindow.PeekChar();
		switch (c)
		{
		case '/':
			if (TextWindow.PeekChar(1) == '/')
			{
				ScanToEndOfLine();
				result = SyntaxFactory.Comment(TextWindow.GetText(intern: false));
			}
			break;
		case '\n':
		case '\r':
			result = ScanEndOfLine();
			break;
		case '\t':
		case '\v':
		case '\f':
		case ' ':
			result = ScanWhitespace();
			break;
		default:
			if (!SyntaxFacts.IsWhitespace(c))
			{
				if (!SyntaxFacts.IsNewLine(c))
				{
					break;
				}
				goto case '\n';
			}
			goto case '\t';
		}
		return result;
	}

	private CSharpSyntaxNode LexXmlDocComment(XmlDocCommentStyle style)
	{
		LexerMode mode = _mode;
		LexerMode modeflags = ((style != XmlDocCommentStyle.SingleLine) ? LexerMode.XmlDocCommentStyleDelimited : LexerMode.XmlDocCommentLocationStart);
		if (_xmlParser == null)
		{
			_xmlParser = new DocumentationCommentParser(this, modeflags);
		}
		else
		{
			_xmlParser.ReInitialize(modeflags);
		}
		DocumentationCommentTriviaSyntax result = _xmlParser.ParseDocumentationComment(out var isTerminated);
		_mode = mode;
		if (!isTerminated)
		{
			AddError(TextWindow.LexemeStartPosition, TextWindow.Width, ErrorCode.ERR_OpenEndedComment);
		}
		return result;
	}

	private SyntaxToken LexXmlToken()
	{
		TokenInfo info = default;
		SyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTrivia(ref trivia);
		Start();
		ScanXmlToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors(GetFullWidth(trivia));
		return Create(ref info, trivia, null, errors);
	}

	private bool ScanXmlToken(ref TokenInfo info)
	{
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		char c = (ch = TextWindow.PeekChar());
		if ((uint)c <= 13u)
		{
			if (c == '\n' || c == '\r')
			{
				goto IL_0069;
			}
			goto IL_00b9;
		}
		if (c != '&')
		{
			if (c != '<')
			{
				if (c != '\uffff' || !TextWindow.IsReallyAtEnd())
				{
					goto IL_00b9;
				}
				info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			}
			else
			{
				ScanXmlTagStart(ref info);
			}
		}
		else
		{
			ScanXmlEntity(ref info);
			info.Kind = SyntaxKind.XmlEntityLiteralToken;
		}
		goto IL_00d3;
		IL_0069:
		ScanEndOfLine();
		info.StringValue = (info.Text = TextWindow.GetText(intern: false));
		info.Kind = SyntaxKind.XmlTextLiteralNewLineToken;
		MutateLocation(XmlDocCommentLocation.Exterior);
		goto IL_00d3;
		IL_00b9:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_0069;
		}
		ScanXmlText(ref info);
		info.Kind = SyntaxKind.XmlTextLiteralToken;
		goto IL_00d3;
		IL_00d3:
		return info.Kind != SyntaxKind.None;
	}

	private void ScanXmlTagStart(ref TokenInfo info)
	{
		if (TextWindow.PeekChar(1) == '!')
		{
			if (TextWindow.PeekChar(2) == '-' && TextWindow.PeekChar(3) == '-')
			{
				TextWindow.AdvanceChar(4);
				info.Kind = SyntaxKind.XmlCommentStartToken;
			}
			else if (TextWindow.PeekChar(2) == '[' && TextWindow.PeekChar(3) == 'C' && TextWindow.PeekChar(4) == 'D' && TextWindow.PeekChar(5) == 'A' && TextWindow.PeekChar(6) == 'T' && TextWindow.PeekChar(7) == 'A' && TextWindow.PeekChar(8) == '[')
			{
				TextWindow.AdvanceChar(9);
				info.Kind = SyntaxKind.XmlCDataStartToken;
			}
			else
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.LessThanToken;
			}
		}
		else if (TextWindow.PeekChar(1) == '/')
		{
			TextWindow.AdvanceChar(2);
			info.Kind = SyntaxKind.LessThanSlashToken;
		}
		else if (TextWindow.PeekChar(1) == '?')
		{
			TextWindow.AdvanceChar(2);
			info.Kind = SyntaxKind.XmlProcessingInstructionStartToken;
		}
		else
		{
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.LessThanToken;
		}
	}

	private void ScanXmlEntity(ref TokenInfo info)
	{
		info.StringValue = null;
		TextWindow.AdvanceChar();
		_builder.Clear();
		XmlParseErrorCode? xmlParseErrorCode = null;
		object[] array = null;
		char c;
		if (IsXmlNameStartChar(c = TextWindow.PeekChar()))
		{
			while (IsXmlNameChar(c = TextWindow.PeekChar()))
			{
				TextWindow.AdvanceChar();
				_builder.Append(c);
			}
			switch (_builder.ToString())
			{
			case "lt":
				info.StringValue = "<";
				break;
			case "gt":
				info.StringValue = ">";
				break;
			case "amp":
				info.StringValue = "&";
				break;
			case "apos":
				info.StringValue = "'";
				break;
			case "quot":
				info.StringValue = "\"";
				break;
			default:
			{
				xmlParseErrorCode = XmlParseErrorCode.XML_RefUndefinedEntity_1;
				object[] array2 = new string[1] { _builder.ToString() };
				array = array2;
				break;
			}
			}
		}
		else if (c == '#')
		{
			TextWindow.AdvanceChar();
			bool flag = TextWindow.PeekChar() == 'x';
			uint num = 0u;
			if (flag)
			{
				TextWindow.AdvanceChar();
				while (SyntaxFacts.IsHexDigit(c = TextWindow.PeekChar()))
				{
					TextWindow.AdvanceChar();
					if (num <= 134217727)
					{
						num = (num << 4) + (uint)SyntaxFacts.HexValue(c);
					}
				}
			}
			else
			{
				while (SyntaxFacts.IsDecDigit(c = TextWindow.PeekChar()))
				{
					TextWindow.AdvanceChar();
					if (num <= 134217727)
					{
						num = (num << 3) + (num << 1) + (uint)SyntaxFacts.DecValue(c);
					}
				}
			}
			if (TextWindow.PeekChar() != ';')
			{
				xmlParseErrorCode = XmlParseErrorCode.XML_InvalidCharEntity;
			}
			if (MatchesProductionForXmlChar(num))
			{
				char charsFromUtf = SlidingTextWindow.GetCharsFromUtf32(num, out var lowSurrogate);
				_builder.Append(charsFromUtf);
				if (lowSurrogate != '\uffff')
				{
					_builder.Append(lowSurrogate);
				}
				info.StringValue = _builder.ToString();
			}
			else if (!xmlParseErrorCode.HasValue)
			{
				xmlParseErrorCode = XmlParseErrorCode.XML_InvalidUnicodeChar;
			}
		}
		else if (SyntaxFacts.IsWhitespace(c) || SyntaxFacts.IsNewLine(c))
		{
			if (!xmlParseErrorCode.HasValue)
			{
				xmlParseErrorCode = XmlParseErrorCode.XML_InvalidWhitespace;
			}
		}
		else if (!xmlParseErrorCode.HasValue)
		{
			xmlParseErrorCode = XmlParseErrorCode.XML_InvalidToken;
			object[] array2 = new string[1] { c.ToString() };
			array = array2;
		}
		c = TextWindow.PeekChar();
		if (c == ';')
		{
			TextWindow.AdvanceChar();
		}
		else if (!xmlParseErrorCode.HasValue)
		{
			xmlParseErrorCode = XmlParseErrorCode.XML_InvalidToken;
			object[] array2 = new string[1] { c.ToString() };
			array = array2;
		}
		info.Text = TextWindow.GetText(intern: true);
		if (info.StringValue == null)
		{
			info.StringValue = info.Text;
		}
		if (xmlParseErrorCode.HasValue)
		{
			AddError(xmlParseErrorCode.Value, array ?? Array.Empty<object>());
		}
	}

	private static bool MatchesProductionForXmlChar(uint charValue)
	{
		if (charValue != 9 && charValue != 10 && charValue != 13 && (charValue < 32 || charValue > 55295) && (charValue < 57344 || charValue > 65533))
		{
			if (charValue >= 65536)
			{
				return charValue <= 1114111;
			}
			return false;
		}
		return true;
	}

	private void ScanXmlText(ref TokenInfo info)
	{
		if (TextWindow.PeekChar() == ']' && TextWindow.PeekChar(1) == ']' && TextWindow.PeekChar(2) == '>')
		{
			TextWindow.AdvanceChar(3);
			info.StringValue = (info.Text = TextWindow.GetText(intern: false));
			AddError(XmlParseErrorCode.XML_CDataEndTagNotAllowed);
			return;
		}
		while (true)
		{
			char c = TextWindow.PeekChar();
			if ((uint)c <= 38u)
			{
				if (c == '\n' || c == '\r' || c == '&')
				{
					goto IL_00d7;
				}
			}
			else if ((uint)c <= 60u)
			{
				if (c != '*')
				{
					if (c == '<')
					{
						goto IL_00d7;
					}
				}
				else if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
				{
					break;
				}
			}
			else
			{
				switch (c)
				{
				case '\uffff':
					if (TextWindow.IsReallyAtEnd())
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				case ']':
					if (TextWindow.PeekChar(1) == ']' && TextWindow.PeekChar(2) == '>')
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				}
			}
			if (!SyntaxFacts.IsNewLine(c))
			{
				TextWindow.AdvanceChar();
				continue;
			}
			goto IL_00d7;
			IL_00d7:
			info.StringValue = (info.Text = TextWindow.GetText(intern: false));
			return;
		}
		info.StringValue = (info.Text = TextWindow.GetText(intern: false));
	}

	private SyntaxToken LexXmlElementTagToken()
	{
		TokenInfo info = default;
		SyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTriviaWithWhitespace(ref trivia);
		Start();
		ScanXmlElementTagToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors(GetFullWidth(trivia));
		if (errors == null && info.ContextualKind == SyntaxKind.None && info.Kind == SyntaxKind.IdentifierToken)
		{
			SyntaxToken syntaxToken = DocumentationCommentXmlTokens.LookupToken(info.Text, trivia);
			if (syntaxToken != null)
			{
				return syntaxToken;
			}
		}
		return Create(ref info, trivia, null, errors);
	}

	private bool ScanXmlElementTagToken(ref TokenInfo info)
	{
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		switch (ch = TextWindow.PeekChar())
		{
		case '<':
			ScanXmlTagStart(ref info);
			break;
		case '>':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.GreaterThanToken;
			break;
		case '/':
			if (TextWindow.PeekChar(1) == '>')
			{
				TextWindow.AdvanceChar(2);
				info.Kind = SyntaxKind.SlashGreaterThanToken;
				break;
			}
			goto default;
		case '"':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.DoubleQuoteToken;
			break;
		case '\'':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.SingleQuoteToken;
			break;
		case '=':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.EqualsToken;
			break;
		case ':':
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.ColonToken;
			break;
		case '\uffff':
			if (TextWindow.IsReallyAtEnd())
			{
				info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
				break;
			}
			goto default;
		case '*':
			if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
			{
				break;
			}
			goto default;
		default:
			if (IsXmlNameStartChar(ch))
			{
				ScanXmlName(ref info);
				info.StringValue = info.Text;
				info.Kind = SyntaxKind.IdentifierToken;
			}
			else if (!SyntaxFacts.IsWhitespace(ch) && !SyntaxFacts.IsNewLine(ch))
			{
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.None;
				info.StringValue = (info.Text = TextWindow.GetText(intern: false));
			}
			break;
		case '\n':
		case '\r':
			break;
		}
		return info.Kind != SyntaxKind.None;
	}

	private void ScanXmlName(ref TokenInfo info)
	{
		int position = TextWindow.Position;
		while (true)
		{
			char c = TextWindow.PeekChar();
			if (c == ':' || !IsXmlNameChar(c))
			{
				break;
			}
			TextWindow.AdvanceChar();
		}
		info.Text = TextWindow.GetText(position, TextWindow.Position - position, intern: true);
	}

	private static bool IsXmlNameStartChar(char ch)
	{
		return XmlCharType.IsStartNCNameCharXml4e(ch);
	}

	private static bool IsXmlNameChar(char ch)
	{
		return XmlCharType.IsNCNameCharXml4e(ch);
	}

	private SyntaxToken LexXmlAttributeTextToken()
	{
		TokenInfo info = default;
		SyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTrivia(ref trivia);
		Start();
		ScanXmlAttributeTextToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors(GetFullWidth(trivia));
		return Create(ref info, trivia, null, errors);
	}

	private bool ScanXmlAttributeTextToken(ref TokenInfo info)
	{
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		char c = (ch = TextWindow.PeekChar());
		if ((uint)c <= 34u)
		{
			if (c == '\n' || c == '\r')
			{
				goto IL_00eb;
			}
			if (c != '"' || !ModeIs(LexerMode.XmlAttributeTextDoubleQuote))
			{
				goto IL_013b;
			}
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.DoubleQuoteToken;
		}
		else if ((uint)c <= 39u)
		{
			if (c != '&')
			{
				if (c != '\'' || !ModeIs(LexerMode.XmlAttributeTextQuote))
				{
					goto IL_013b;
				}
				TextWindow.AdvanceChar();
				info.Kind = SyntaxKind.SingleQuoteToken;
			}
			else
			{
				ScanXmlEntity(ref info);
				info.Kind = SyntaxKind.XmlEntityLiteralToken;
			}
		}
		else if (c != '<')
		{
			if (c != '\uffff' || !TextWindow.IsReallyAtEnd())
			{
				goto IL_013b;
			}
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
		}
		else
		{
			TextWindow.AdvanceChar();
			info.Kind = SyntaxKind.LessThanToken;
		}
		goto IL_0155;
		IL_013b:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_00eb;
		}
		ScanXmlAttributeText(ref info);
		info.Kind = SyntaxKind.XmlTextLiteralToken;
		goto IL_0155;
		IL_0155:
		return info.Kind != SyntaxKind.None;
		IL_00eb:
		ScanEndOfLine();
		info.StringValue = (info.Text = TextWindow.GetText(intern: false));
		info.Kind = SyntaxKind.XmlTextLiteralNewLineToken;
		MutateLocation(XmlDocCommentLocation.Exterior);
		goto IL_0155;
	}

	private void ScanXmlAttributeText(ref TokenInfo info)
	{
		while (true)
		{
			char c = TextWindow.PeekChar();
			switch (c)
			{
			case '"':
				if (ModeIs(LexerMode.XmlAttributeTextDoubleQuote))
				{
					info.StringValue = (info.Text = TextWindow.GetText(intern: false));
					return;
				}
				goto default;
			case '\'':
				if (ModeIs(LexerMode.XmlAttributeTextQuote))
				{
					info.StringValue = (info.Text = TextWindow.GetText(intern: false));
					return;
				}
				goto default;
			case '\n':
			case '\r':
			case '&':
			case '<':
				info.StringValue = (info.Text = TextWindow.GetText(intern: false));
				return;
			case '\uffff':
				if (TextWindow.IsReallyAtEnd())
				{
					info.StringValue = (info.Text = TextWindow.GetText(intern: false));
					return;
				}
				goto default;
			case '*':
				if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
				{
					info.StringValue = (info.Text = TextWindow.GetText(intern: false));
					return;
				}
				goto default;
			default:
				if (!SyntaxFacts.IsNewLine(c))
				{
					break;
				}
				goto case '\n';
			}
			TextWindow.AdvanceChar();
		}
	}

	private SyntaxToken LexXmlCharacter()
	{
		TokenInfo info = default;
		SyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTriviaWithWhitespace(ref trivia);
		Start();
		ScanXmlCharacter(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors(GetFullWidth(trivia));
		return Create(ref info, trivia, null, errors);
	}

	private bool ScanXmlCharacter(ref TokenInfo info)
	{
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char c = TextWindow.PeekChar();
		if (c != '&')
		{
			if (c == '\uffff' && TextWindow.IsReallyAtEnd())
			{
				info.Kind = SyntaxKind.EndOfFileToken;
			}
			else
			{
				info.Kind = SyntaxKind.XmlTextLiteralToken;
				info.Text = (info.StringValue = TextWindow.NextChar().ToString());
			}
		}
		else
		{
			ScanXmlEntity(ref info);
			info.Kind = SyntaxKind.XmlEntityLiteralToken;
		}
		return true;
	}

	private SyntaxToken LexXmlCrefOrNameToken()
	{
		TokenInfo info = default;
		SyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTriviaWithWhitespace(ref trivia);
		Start();
		ScanXmlCrefToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors(GetFullWidth(trivia));
		return Create(ref info, trivia, null, errors);
	}

	private bool ScanXmlCrefToken(ref TokenInfo info)
	{
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		int position = TextWindow.Position;
		char ch = TextWindow.NextChar();
		char surrogate = '\uffff';
		if ((uint)ch <= 38u)
		{
			if ((uint)ch <= 13u)
			{
				if (ch == '\n' || ch == '\r')
				{
					goto IL_0134;
				}
				goto IL_01bd;
			}
			if (ch != '"')
			{
				if (ch != '&')
				{
					goto IL_01bd;
				}
				TextWindow.Reset(position);
				if (!TextWindow.TryScanXmlEntity(out ch, out surrogate))
				{
					TextWindow.Reset(position);
					ScanXmlEntity(ref info);
					info.Kind = SyntaxKind.XmlEntityLiteralToken;
					return true;
				}
			}
			else if (ModeIs(LexerMode.XmlCrefDoubleQuote) || ModeIs(LexerMode.XmlNameDoubleQuote))
			{
				info.Kind = SyntaxKind.DoubleQuoteToken;
				return true;
			}
		}
		else if ((uint)ch <= 60u)
		{
			if (ch != '\'')
			{
				if (ch != '<')
				{
					goto IL_01bd;
				}
				info.Text = TextWindow.GetText(intern: false);
				AddError(XmlParseErrorCode.XML_LessThanInAttributeValue, info.Text);
				return true;
			}
			if (ModeIs(LexerMode.XmlCrefQuote) || ModeIs(LexerMode.XmlNameQuote))
			{
				info.Kind = SyntaxKind.SingleQuoteToken;
				return true;
			}
		}
		else if (ch != '{')
		{
			if (ch != '}')
			{
				if (ch != '\uffff' || !TextWindow.IsReallyAtEnd())
				{
					goto IL_01bd;
				}
				info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
				return true;
			}
			ch = '>';
		}
		else
		{
			ch = '<';
		}
		goto IL_01c8;
		IL_01c8:
		switch (ch)
		{
		case '(':
			info.Kind = SyntaxKind.OpenParenToken;
			break;
		case ')':
			info.Kind = SyntaxKind.CloseParenToken;
			break;
		case '[':
			info.Kind = SyntaxKind.OpenBracketToken;
			break;
		case ']':
			info.Kind = SyntaxKind.CloseBracketToken;
			break;
		case ',':
			info.Kind = SyntaxKind.CommaToken;
			break;
		case '.':
			info.Kind = SyntaxKind.DotToken;
			break;
		case '?':
			info.Kind = SyntaxKind.QuestionToken;
			break;
		case '&':
			info.Kind = SyntaxKind.AmpersandToken;
			break;
		case '*':
			info.Kind = SyntaxKind.AsteriskToken;
			break;
		case '|':
			info.Kind = SyntaxKind.BarToken;
			break;
		case '^':
			info.Kind = SyntaxKind.CaretToken;
			break;
		case '%':
			info.Kind = SyntaxKind.PercentToken;
			break;
		case '/':
			info.Kind = SyntaxKind.SlashToken;
			break;
		case '~':
			info.Kind = SyntaxKind.TildeToken;
			break;
		case '{':
			info.Kind = SyntaxKind.LessThanToken;
			break;
		case '}':
			info.Kind = SyntaxKind.GreaterThanToken;
			break;
		case ':':
			if (AdvanceIfMatches(':'))
			{
				info.Kind = SyntaxKind.ColonColonToken;
			}
			else
			{
				info.Kind = SyntaxKind.ColonToken;
			}
			break;
		case '=':
			if (AdvanceIfMatches('='))
			{
				info.Kind = SyntaxKind.EqualsEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.EqualsToken;
			}
			break;
		case '!':
			if (AdvanceIfMatches('='))
			{
				info.Kind = SyntaxKind.ExclamationEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.ExclamationToken;
			}
			break;
		case '>':
			if (AdvanceIfMatches('='))
			{
				info.Kind = SyntaxKind.GreaterThanEqualsToken;
			}
			else
			{
				info.Kind = SyntaxKind.GreaterThanToken;
			}
			break;
		case '<':
			if (AdvanceIfMatches('='))
			{
				info.Kind = SyntaxKind.LessThanEqualsToken;
			}
			else if (AdvanceIfMatches('<'))
			{
				info.Kind = SyntaxKind.LessThanLessThanToken;
			}
			else
			{
				info.Kind = SyntaxKind.LessThanToken;
			}
			break;
		case '+':
			if (AdvanceIfMatches('+'))
			{
				info.Kind = SyntaxKind.PlusPlusToken;
			}
			else
			{
				info.Kind = SyntaxKind.PlusToken;
			}
			break;
		case '-':
			if (AdvanceIfMatches('-'))
			{
				info.Kind = SyntaxKind.MinusMinusToken;
			}
			else
			{
				info.Kind = SyntaxKind.MinusToken;
			}
			break;
		}
		if (info.Kind != SyntaxKind.None)
		{
			string text = SyntaxFacts.GetText(info.Kind);
			string text2 = TextWindow.GetText(intern: false);
			if (!string.IsNullOrEmpty(text) && text2 != text)
			{
				info.RequiresTextForXmlEntity = true;
				info.Text = text2;
				info.StringValue = text;
			}
		}
		else
		{
			TextWindow.Reset(position);
			if (ScanIdentifier(ref info) && info.Text.Length > 0)
			{
				if (!InXmlNameAttributeValue && !info.IsVerbatim && !info.HasIdentifierEscapeSequence && _cache.TryGetKeywordKind(info.StringValue, out var kind))
				{
					if (SyntaxFacts.IsContextualKeyword(kind))
					{
						info.Kind = SyntaxKind.IdentifierToken;
						info.ContextualKind = kind;
					}
					else
					{
						info.Kind = kind;
						info.RequiresTextForXmlEntity = info.Text != info.StringValue;
					}
				}
				else
				{
					info.ContextualKind = (info.Kind = SyntaxKind.IdentifierToken);
				}
			}
			else if (ch == '@')
			{
				if (TextWindow.PeekChar() == '@')
				{
					TextWindow.NextChar();
					info.Text = TextWindow.GetText(intern: true);
					info.StringValue = "";
				}
				else
				{
					ScanXmlEntity(ref info);
				}
				info.Kind = SyntaxKind.IdentifierToken;
				AddError(ErrorCode.ERR_ExpectedVerbatimLiteral);
			}
			else if (TextWindow.PeekChar() == '&')
			{
				ScanXmlEntity(ref info);
				info.Kind = SyntaxKind.XmlEntityLiteralToken;
				AddCrefError(ErrorCode.ERR_UnexpectedCharacter, info.Text);
			}
			else
			{
				char charValue = TextWindow.NextChar();
				info.Text = TextWindow.GetText(intern: false);
				if (MatchesProductionForXmlChar(charValue))
				{
					AddCrefError(ErrorCode.ERR_UnexpectedCharacter, info.Text);
				}
				else
				{
					AddError(XmlParseErrorCode.XML_InvalidUnicodeChar);
				}
			}
		}
		return info.Kind != SyntaxKind.None;
		IL_01bd:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_0134;
		}
		goto IL_01c8;
		IL_0134:
		TextWindow.Reset(position);
		ScanEndOfLine();
		info.StringValue = (info.Text = TextWindow.GetText(intern: false));
		info.Kind = SyntaxKind.XmlTextLiteralNewLineToken;
		MutateLocation(XmlDocCommentLocation.Exterior);
		goto IL_01c8;
	}

	private bool AdvanceIfMatches(char ch)
	{
		char c = TextWindow.PeekChar();
		if (c == ch || (c == '{' && ch == '<') || (c == '}' && ch == '>'))
		{
			TextWindow.AdvanceChar();
			return true;
		}
		if (c == '&')
		{
			int position = TextWindow.Position;
			if (TextWindow.TryScanXmlEntity(out var ch2, out var surrogate) && ch2 == ch && surrogate == '\uffff')
			{
				return true;
			}
			TextWindow.Reset(position);
		}
		return false;
	}

	private void AddCrefError(ErrorCode code, params object[] args)
	{
		AddCrefError(AbstractLexer.MakeError(code, args));
	}

	private void AddCrefError(DiagnosticInfo info)
	{
		if (info != null)
		{
			AddError(ErrorCode.WRN_ErrorOverride, info, info.Code);
		}
	}

	private SyntaxToken LexXmlCDataSectionTextToken()
	{
		TokenInfo info = default;
		SyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTrivia(ref trivia);
		Start();
		ScanXmlCDataSectionTextToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors(GetFullWidth(trivia));
		return Create(ref info, trivia, null, errors);
	}

	private bool ScanXmlCDataSectionTextToken(ref TokenInfo info)
	{
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		char c = (ch = TextWindow.PeekChar());
		if ((uint)c <= 13u)
		{
			if (c == '\n' || c == '\r')
			{
				goto IL_0083;
			}
			goto IL_00d3;
		}
		if (c != ']')
		{
			if (c != '\uffff' || !TextWindow.IsReallyAtEnd())
			{
				goto IL_00d3;
			}
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
		}
		else
		{
			if (TextWindow.PeekChar(1) != ']' || TextWindow.PeekChar(2) != '>')
			{
				goto IL_00d3;
			}
			TextWindow.AdvanceChar(3);
			info.Kind = SyntaxKind.XmlCDataEndToken;
		}
		goto IL_00ed;
		IL_00ed:
		return true;
		IL_00d3:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_0083;
		}
		ScanXmlCDataSectionText(ref info);
		info.Kind = SyntaxKind.XmlTextLiteralToken;
		goto IL_00ed;
		IL_0083:
		ScanEndOfLine();
		info.StringValue = (info.Text = TextWindow.GetText(intern: false));
		info.Kind = SyntaxKind.XmlTextLiteralNewLineToken;
		MutateLocation(XmlDocCommentLocation.Exterior);
		goto IL_00ed;
	}

	private void ScanXmlCDataSectionText(ref TokenInfo info)
	{
		while (true)
		{
			char c = TextWindow.PeekChar();
			if ((uint)c <= 13u)
			{
				if (c == '\n' || c == '\r')
				{
					break;
				}
			}
			else
			{
				switch (c)
				{
				case ']':
					if (TextWindow.PeekChar(1) == ']' && TextWindow.PeekChar(2) == '>')
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				case '\uffff':
					if (TextWindow.IsReallyAtEnd())
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				case '*':
					if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				}
			}
			if (SyntaxFacts.IsNewLine(c))
			{
				break;
			}
			TextWindow.AdvanceChar();
		}
		info.StringValue = (info.Text = TextWindow.GetText(intern: false));
	}

	private SyntaxToken LexXmlCommentTextToken()
	{
		TokenInfo info = default;
		SyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTrivia(ref trivia);
		Start();
		ScanXmlCommentTextToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors(GetFullWidth(trivia));
		return Create(ref info, trivia, null, errors);
	}

	private bool ScanXmlCommentTextToken(ref TokenInfo info)
	{
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		char c = (ch = TextWindow.PeekChar());
		if ((uint)c <= 13u)
		{
			if (c == '\n' || c == '\r')
			{
				goto IL_00a5;
			}
			goto IL_00f5;
		}
		if (c != '-')
		{
			if (c != '\uffff' || !TextWindow.IsReallyAtEnd())
			{
				goto IL_00f5;
			}
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
		}
		else
		{
			if (TextWindow.PeekChar(1) != '-')
			{
				goto IL_00f5;
			}
			if (TextWindow.PeekChar(2) == '>')
			{
				TextWindow.AdvanceChar(3);
				info.Kind = SyntaxKind.XmlCommentEndToken;
			}
			else
			{
				TextWindow.AdvanceChar(2);
				info.Kind = SyntaxKind.MinusMinusToken;
			}
		}
		goto IL_010f;
		IL_010f:
		return true;
		IL_00f5:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_00a5;
		}
		ScanXmlCommentText(ref info);
		info.Kind = SyntaxKind.XmlTextLiteralToken;
		goto IL_010f;
		IL_00a5:
		ScanEndOfLine();
		info.StringValue = (info.Text = TextWindow.GetText(intern: false));
		info.Kind = SyntaxKind.XmlTextLiteralNewLineToken;
		MutateLocation(XmlDocCommentLocation.Exterior);
		goto IL_010f;
	}

	private void ScanXmlCommentText(ref TokenInfo info)
	{
		while (true)
		{
			char c = TextWindow.PeekChar();
			if ((uint)c <= 13u)
			{
				if (c == '\n' || c == '\r')
				{
					break;
				}
			}
			else
			{
				switch (c)
				{
				case '-':
					if (TextWindow.PeekChar(1) == '-')
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				case '\uffff':
					if (TextWindow.IsReallyAtEnd())
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				case '*':
					if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				}
			}
			if (SyntaxFacts.IsNewLine(c))
			{
				break;
			}
			TextWindow.AdvanceChar();
		}
		info.StringValue = (info.Text = TextWindow.GetText(intern: false));
	}

	private SyntaxToken LexXmlProcessingInstructionTextToken()
	{
		TokenInfo info = default;
		SyntaxListBuilder trivia = null;
		LexXmlDocCommentLeadingTrivia(ref trivia);
		Start();
		ScanXmlProcessingInstructionTextToken(ref info);
		SyntaxDiagnosticInfo[] errors = GetErrors(GetFullWidth(trivia));
		return Create(ref info, trivia, null, errors);
	}

	private bool ScanXmlProcessingInstructionTextToken(ref TokenInfo info)
	{
		if (LocationIs(XmlDocCommentLocation.End))
		{
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
			return true;
		}
		char ch;
		char c = (ch = TextWindow.PeekChar());
		if ((uint)c <= 13u)
		{
			if (c == '\n' || c == '\r')
			{
				goto IL_0070;
			}
			goto IL_00c0;
		}
		if (c != '?')
		{
			if (c != '\uffff' || !TextWindow.IsReallyAtEnd())
			{
				goto IL_00c0;
			}
			info.Kind = SyntaxKind.EndOfDocumentationCommentToken;
		}
		else
		{
			if (TextWindow.PeekChar(1) != '>')
			{
				goto IL_00c0;
			}
			TextWindow.AdvanceChar(2);
			info.Kind = SyntaxKind.XmlProcessingInstructionEndToken;
		}
		goto IL_00da;
		IL_0070:
		ScanEndOfLine();
		info.StringValue = (info.Text = TextWindow.GetText(intern: false));
		info.Kind = SyntaxKind.XmlTextLiteralNewLineToken;
		MutateLocation(XmlDocCommentLocation.Exterior);
		goto IL_00da;
		IL_00da:
		return true;
		IL_00c0:
		if (SyntaxFacts.IsNewLine(ch))
		{
			goto IL_0070;
		}
		ScanXmlProcessingInstructionText(ref info);
		info.Kind = SyntaxKind.XmlTextLiteralToken;
		goto IL_00da;
	}

	private void ScanXmlProcessingInstructionText(ref TokenInfo info)
	{
		while (true)
		{
			char c = TextWindow.PeekChar();
			if ((uint)c <= 13u)
			{
				if (c == '\n' || c == '\r')
				{
					break;
				}
			}
			else
			{
				switch (c)
				{
				case '?':
					if (TextWindow.PeekChar(1) == '>')
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				case '\uffff':
					if (TextWindow.IsReallyAtEnd())
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				case '*':
					if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
					{
						info.StringValue = (info.Text = TextWindow.GetText(intern: false));
						return;
					}
					break;
				}
			}
			if (SyntaxFacts.IsNewLine(c))
			{
				break;
			}
			TextWindow.AdvanceChar();
		}
		info.StringValue = (info.Text = TextWindow.GetText(intern: false));
	}

	private void LexXmlDocCommentLeadingTrivia(ref SyntaxListBuilder trivia)
	{
		int position = TextWindow.Position;
		Start();
		if (LocationIs(XmlDocCommentLocation.Start) && StyleIs(XmlDocCommentStyle.Delimited))
		{
			if (TextWindow.PeekChar() == '/' && TextWindow.PeekChar(1) == '*' && TextWindow.PeekChar(2) == '*' && TextWindow.PeekChar(3) != '*')
			{
				TextWindow.AdvanceChar(3);
				string text = TextWindow.GetText(intern: true);
				AddTrivia(SyntaxFactory.DocumentationCommentExteriorTrivia(text), ref trivia);
				MutateLocation(XmlDocCommentLocation.Interior);
			}
		}
		else if (LocationIs(XmlDocCommentLocation.Start) || LocationIs(XmlDocCommentLocation.Exterior))
		{
			while (true)
			{
				char c = TextWindow.PeekChar();
				switch (c)
				{
				case '\t':
				case '\v':
				case '\f':
				case ' ':
					goto IL_00fb;
				case '/':
					if (StyleIs(XmlDocCommentStyle.SingleLine) && TextWindow.PeekChar(1) == '/' && TextWindow.PeekChar(2) == '/' && TextWindow.PeekChar(3) != '/')
					{
						TextWindow.AdvanceChar(3);
						string text3 = TextWindow.GetText(intern: true);
						AddTrivia(SyntaxFactory.DocumentationCommentExteriorTrivia(text3), ref trivia);
						MutateLocation(XmlDocCommentLocation.Interior);
						return;
					}
					break;
				case '*':
					if (StyleIs(XmlDocCommentStyle.Delimited))
					{
						while (TextWindow.PeekChar() == '*' && TextWindow.PeekChar(1) != '/')
						{
							TextWindow.AdvanceChar();
						}
						string text2 = TextWindow.GetText(intern: true);
						if (!string.IsNullOrEmpty(text2))
						{
							AddTrivia(SyntaxFactory.DocumentationCommentExteriorTrivia(text2), ref trivia);
						}
						if (TextWindow.PeekChar() == '*' && TextWindow.PeekChar(1) == '/')
						{
							TextWindow.AdvanceChar(2);
							AddTrivia(SyntaxFactory.DocumentationCommentExteriorTrivia("*/"), ref trivia);
							MutateLocation(XmlDocCommentLocation.End);
						}
						else
						{
							MutateLocation(XmlDocCommentLocation.Interior);
						}
						return;
					}
					break;
				}
				if (!SyntaxFacts.IsWhitespace(c))
				{
					break;
				}
				goto IL_00fb;
				IL_00fb:
				TextWindow.AdvanceChar();
			}
			if (StyleIs(XmlDocCommentStyle.SingleLine))
			{
				TextWindow.Reset(position);
				MutateLocation(XmlDocCommentLocation.End);
				return;
			}
			string text4 = TextWindow.GetText(intern: true);
			if (!string.IsNullOrEmpty(text4))
			{
				AddTrivia(SyntaxFactory.DocumentationCommentExteriorTrivia(text4), ref trivia);
			}
			MutateLocation(XmlDocCommentLocation.Interior);
		}
		else if (!LocationIs(XmlDocCommentLocation.End) && StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar() == '*' && TextWindow.PeekChar(1) == '/')
		{
			TextWindow.AdvanceChar(2);
			string text5 = TextWindow.GetText(intern: true);
			AddTrivia(SyntaxFactory.DocumentationCommentExteriorTrivia(text5), ref trivia);
			MutateLocation(XmlDocCommentLocation.End);
		}
	}

	private void LexXmlDocCommentLeadingTriviaWithWhitespace(ref SyntaxListBuilder trivia)
	{
		while (true)
		{
			LexXmlDocCommentLeadingTrivia(ref trivia);
			char ch = TextWindow.PeekChar();
			if (LocationIs(XmlDocCommentLocation.Interior) && (SyntaxFacts.IsWhitespace(ch) || SyntaxFacts.IsNewLine(ch)))
			{
				LexXmlWhitespaceAndNewLineTrivia(ref trivia);
				continue;
			}
			break;
		}
	}

	private void LexXmlWhitespaceAndNewLineTrivia(ref SyntaxListBuilder trivia)
	{
		Start();
		if (!LocationIs(XmlDocCommentLocation.Interior))
		{
			return;
		}
		char c = TextWindow.PeekChar();
		switch (c)
		{
		case '\t':
		case '\v':
		case '\f':
		case ' ':
			AddTrivia(ScanWhitespace(), ref trivia);
			break;
		case '\n':
		case '\r':
			AddTrivia(ScanEndOfLine(), ref trivia);
			MutateLocation(XmlDocCommentLocation.Exterior);
			break;
		case '*':
			if (StyleIs(XmlDocCommentStyle.Delimited) && TextWindow.PeekChar(1) == '/')
			{
				break;
			}
			goto default;
		default:
			if (SyntaxFacts.IsWhitespace(c))
			{
				goto case '\t';
			}
			if (!SyntaxFacts.IsNewLine(c))
			{
				break;
			}
			goto case '\n';
		}
	}

	private void ScanStringLiteral(ref TokenInfo info, bool allowEscapes = true)
	{
		char c = TextWindow.PeekChar();
		if (c == '\'' || c == '"')
		{
			TextWindow.AdvanceChar();
			_builder.Length = 0;
			while (true)
			{
				char c2 = TextWindow.PeekChar();
				if ((c2 == '\\') & allowEscapes)
				{
					c2 = ScanEscapeSequence(out var surrogateCharacter);
					_builder.Append(c2);
					if (surrogateCharacter != '\uffff')
					{
						_builder.Append(surrogateCharacter);
					}
					continue;
				}
				if (c2 == c)
				{
					TextWindow.AdvanceChar();
					break;
				}
				if (SyntaxFacts.IsNewLine(c2) || (c2 == '\uffff' && TextWindow.IsReallyAtEnd()))
				{
					AddError(ErrorCode.ERR_NewlineInConst);
					break;
				}
				TextWindow.AdvanceChar();
				_builder.Append(c2);
			}
			info.Text = TextWindow.GetText(intern: true);
			if (c == '\'')
			{
				info.Kind = SyntaxKind.CharacterLiteralToken;
				if (_builder.Length != 1)
				{
					AddError((_builder.Length != 0) ? ErrorCode.ERR_TooManyCharsInConst : ErrorCode.ERR_EmptyCharConst);
				}
				if (_builder.Length > 0)
				{
					info.StringValue = TextWindow.Intern(_builder);
					info.CharValue = info.StringValue[0];
				}
				else
				{
					info.StringValue = string.Empty;
					info.CharValue = '\uffff';
				}
			}
			else
			{
				info.Kind = SyntaxKind.StringLiteralToken;
				if (_builder.Length > 0)
				{
					info.StringValue = TextWindow.Intern(_builder);
				}
				else
				{
					info.StringValue = string.Empty;
				}
			}
		}
		else
		{
			info.Kind = SyntaxKind.None;
			info.Text = null;
		}
	}

	private char ScanEscapeSequence(out char surrogateCharacter)
	{
		int position = TextWindow.Position;
		surrogateCharacter = '\uffff';
		TextWindow.NextChar();
		char c = TextWindow.NextChar();
		switch (c)
		{
		case '0':
			c = '\0';
			break;
		case 'a':
			c = '\a';
			break;
		case 'b':
			c = '\b';
			break;
		case 'f':
			c = '\f';
			break;
		case 'n':
			c = '\n';
			break;
		case 'r':
			c = '\r';
			break;
		case 't':
			c = '\t';
			break;
		case 'v':
			c = '\v';
			break;
		case 'U':
		case 'u':
		case 'x':
		{
			TextWindow.Reset(position);
			c = TextWindow.NextUnicodeEscape(out surrogateCharacter, out var info);
			AddError(info);
			break;
		}
		default:
			AddError(position, TextWindow.Position - position, ErrorCode.ERR_IllegalEscape);
			break;
		case '"':
		case '\'':
		case '\\':
			break;
		}
		return c;
	}

	private void ScanVerbatimStringLiteral(ref TokenInfo info, bool allowNewlines = true)
	{
		_builder.Length = 0;
		if (TextWindow.PeekChar() == '@' && TextWindow.PeekChar(1) == '"')
		{
			TextWindow.AdvanceChar(2);
			bool flag = false;
			_builder.Length = 0;
			while (!flag)
			{
				char c;
				switch (c = TextWindow.PeekChar())
				{
				case '"':
					TextWindow.AdvanceChar();
					if (TextWindow.PeekChar() == '"')
					{
						TextWindow.AdvanceChar();
						_builder.Append(c);
					}
					else
					{
						flag = true;
					}
					continue;
				case '\uffff':
					if (TextWindow.IsReallyAtEnd())
					{
						AddError(ErrorCode.ERR_UnterminatedStringLit);
						flag = true;
						continue;
					}
					break;
				}
				if (!allowNewlines && SyntaxFacts.IsNewLine(c))
				{
					AddError(ErrorCode.ERR_UnterminatedStringLit);
					flag = true;
				}
				else
				{
					TextWindow.AdvanceChar();
					_builder.Append(c);
				}
			}
			info.Kind = SyntaxKind.StringLiteralToken;
			info.Text = TextWindow.GetText(intern: false);
			info.StringValue = _builder.ToString();
		}
		else
		{
			info.Kind = SyntaxKind.None;
			info.Text = null;
			info.StringValue = null;
		}
	}

	private void ScanInterpolatedStringLiteral(bool isVerbatim, ref TokenInfo info)
	{
		SyntaxDiagnosticInfo error = null;
		ScanInterpolatedStringLiteralTop(null, isVerbatim, ref info, ref error, out var _);
		AddError(error);
	}

	internal void ScanInterpolatedStringLiteralTop(ArrayBuilder<Interpolation> interpolations, bool isVerbatim, ref TokenInfo info, ref SyntaxDiagnosticInfo error, out bool closeQuoteMissing)
	{
		InterpolatedStringScanner interpolatedStringScanner = new InterpolatedStringScanner(this, isVerbatim);
		interpolatedStringScanner.ScanInterpolatedStringLiteralTop(interpolations, ref info, out closeQuoteMissing);
		error = interpolatedStringScanner.error;
		info.Text = TextWindow.GetText(intern: false);
	}

	internal static SyntaxToken RescanInterpolatedString(InterpolatedStringExpressionSyntax interpolatedString)
	{
		string text = interpolatedString.ToString();
		SyntaxKind kind = SyntaxKind.InterpolatedStringToken;
		return SyntaxFactory.Literal(interpolatedString.GetFirstToken().GetLeadingTrivia(), text, kind, text, interpolatedString.GetLastToken().GetTrailingTrivia());
	}

	private SyntaxToken QuickScanSyntaxToken()
	{
		Start();
		QuickScanState quickScanState = QuickScanState.Initial;
		int num = TextWindow.Offset;
		int characterWindowCount = TextWindow.CharacterWindowCount;
		characterWindowCount = Math.Min(characterWindowCount, num + 42);
		int num2 = -2128831035;
		char[] characterWindow = TextWindow.CharacterWindow;
		int num3 = s_charProperties.Length;
		while (true)
		{
			if (num < characterWindowCount)
			{
				int num4 = characterWindow[num];
				CharFlags charFlags = (CharFlags)((num4 < num3) ? s_charProperties[num4] : 9);
				quickScanState = (QuickScanState)s_stateTransitions[(uint)quickScanState, (uint)charFlags];
				if ((int)quickScanState >= 9)
				{
					break;
				}
				num2 = (num2 ^ num4) * 16777619;
				num++;
				continue;
			}
			quickScanState = QuickScanState.Bad;
			break;
		}
		TextWindow.AdvanceChar(num - TextWindow.Offset);
		if (quickScanState == QuickScanState.Done)
		{
			return _cache.LookupToken(TextWindow.CharacterWindow, TextWindow.LexemeRelativeStart, num - TextWindow.LexemeRelativeStart, num2, _createQuickTokenFunction);
		}
		TextWindow.Reset(TextWindow.LexemeStartPosition);
		return null;
	}

	private SyntaxToken CreateQuickToken()
	{
		TextWindow.Reset(TextWindow.LexemeStartPosition);
		return LexSyntaxToken();
	}
}
