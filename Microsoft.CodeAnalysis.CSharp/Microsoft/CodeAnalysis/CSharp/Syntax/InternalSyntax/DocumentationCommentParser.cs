using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal class DocumentationCommentParser : SyntaxParser
{
	private enum SkipResult
	{
		Continue,
		Abort
	}

	private readonly SyntaxListPool _pool = new SyntaxListPool();

	private bool _isDelimited;

	private readonly HashSet<string> _attributesSeen = new HashSet<string>();

	private bool IsEndOfCrefAttribute
	{
		get
		{
			switch (CurrentToken.Kind)
			{
			case SyntaxKind.SingleQuoteToken:
				return (Mode & LexerMode.XmlCrefQuote) == LexerMode.XmlCrefQuote;
			case SyntaxKind.DoubleQuoteToken:
				return (Mode & LexerMode.XmlCrefDoubleQuote) == LexerMode.XmlCrefDoubleQuote;
			case SyntaxKind.EndOfDocumentationCommentToken:
			case SyntaxKind.EndOfFileToken:
				return true;
			case SyntaxKind.BadToken:
				if (!(CurrentToken.Text == SyntaxFacts.GetText(SyntaxKind.LessThanToken)))
				{
					return IsNonAsciiQuotationMark(CurrentToken);
				}
				return true;
			default:
				return false;
			}
		}
	}

	private bool InCref
	{
		get
		{
			LexerMode lexerMode = Mode & (LexerMode.XmlCrefQuote | LexerMode.XmlCrefDoubleQuote);
			if (lexerMode == LexerMode.XmlCrefQuote || lexerMode == LexerMode.XmlCrefDoubleQuote)
			{
				return true;
			}
			return false;
		}
	}

	private bool IsEndOfNameAttribute
	{
		get
		{
			switch (CurrentToken.Kind)
			{
			case SyntaxKind.SingleQuoteToken:
				return (Mode & LexerMode.XmlNameQuote) == LexerMode.XmlNameQuote;
			case SyntaxKind.DoubleQuoteToken:
				return (Mode & LexerMode.XmlNameDoubleQuote) == LexerMode.XmlNameDoubleQuote;
			case SyntaxKind.EndOfDocumentationCommentToken:
			case SyntaxKind.EndOfFileToken:
				return true;
			case SyntaxKind.BadToken:
				if (!(CurrentToken.Text == SyntaxFacts.GetText(SyntaxKind.LessThanToken)))
				{
					return IsNonAsciiQuotationMark(CurrentToken);
				}
				return true;
			default:
				return false;
			}
		}
	}

	internal DocumentationCommentParser(Lexer lexer, LexerMode modeflags)
		: base(lexer, LexerMode.XmlDocComment | modeflags, null, null, allowModeReset: true)
	{
		_isDelimited = (modeflags & LexerMode.XmlDocCommentStyleDelimited) != 0;
	}

	internal void ReInitialize(LexerMode modeflags)
	{
		ReInitialize();
		Mode = LexerMode.XmlDocComment | modeflags;
		_isDelimited = (modeflags & LexerMode.XmlDocCommentStyleDelimited) != 0;
	}

	private LexerMode SetMode(LexerMode mode)
	{
		LexerMode mode2 = Mode;
		Mode = mode | (mode2 & (LexerMode.MaskXmlDocCommentLocation | LexerMode.MaskXmlDocCommentStyle));
		return mode2;
	}

	private void ResetMode(LexerMode mode)
	{
		Mode = mode;
	}

	public DocumentationCommentTriviaSyntax ParseDocumentationComment(out bool isTerminated)
	{
		SyntaxListBuilder<XmlNodeSyntax> syntaxListBuilder = _pool.Allocate<XmlNodeSyntax>();
		try
		{
			ParseXmlNodes(syntaxListBuilder);
			if (CurrentToken.Kind != SyntaxKind.EndOfDocumentationCommentToken)
			{
				ParseRemainder(syntaxListBuilder);
			}
			SyntaxToken syntaxToken = EatToken(SyntaxKind.EndOfDocumentationCommentToken);
			isTerminated = !_isDelimited || (syntaxToken.LeadingTrivia.Count > 0 && syntaxToken.LeadingTrivia[syntaxToken.LeadingTrivia.Count - 1].ToString() == "*/");
			return SyntaxFactory.DocumentationCommentTrivia(_isDelimited ? SyntaxKind.MultiLineDocumentationCommentTrivia : SyntaxKind.SingleLineDocumentationCommentTrivia, syntaxListBuilder.ToList(), syntaxToken);
		}
		finally
		{
			_pool.Free(syntaxListBuilder);
		}
	}

	public void ParseRemainder(SyntaxListBuilder<XmlNodeSyntax> nodes)
	{
		bool flag = CurrentToken.Kind == SyntaxKind.LessThanSlashToken;
		LexerMode mode = SetMode(LexerMode.XmlCDataSectionText);
		SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
		try
		{
			while (CurrentToken.Kind != SyntaxKind.EndOfDocumentationCommentToken)
			{
				SyntaxToken item = EatToken();
				syntaxListBuilder.Add(item);
			}
			XmlTextSyntax node = SyntaxFactory.XmlText(syntaxListBuilder.ToList());
			XmlParseErrorCode code = (flag ? XmlParseErrorCode.XML_EndTagNotExpected : XmlParseErrorCode.XML_ExpectedEndOfXml);
			node = WithAdditionalDiagnostics(node, new XmlSyntaxDiagnosticInfo(0, 1, code));
			nodes.Add(node);
		}
		finally
		{
			_pool.Free(syntaxListBuilder);
		}
		ResetMode(mode);
	}

	private void ParseXmlNodes(SyntaxListBuilder<XmlNodeSyntax> nodes)
	{
		while (true)
		{
			XmlNodeSyntax xmlNodeSyntax = ParseXmlNode();
			if (xmlNodeSyntax == null)
			{
				break;
			}
			nodes.Add(xmlNodeSyntax);
		}
	}

	private XmlNodeSyntax ParseXmlNode()
	{
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.XmlEntityLiteralToken:
		case SyntaxKind.XmlTextLiteralToken:
		case SyntaxKind.XmlTextLiteralNewLineToken:
			return ParseXmlText();
		case SyntaxKind.LessThanToken:
			return ParseXmlElement();
		case SyntaxKind.XmlCommentStartToken:
			return ParseXmlComment();
		case SyntaxKind.XmlCDataStartToken:
			return ParseXmlCDataSection();
		case SyntaxKind.XmlProcessingInstructionStartToken:
			return ParseXmlProcessingInstruction();
		case SyntaxKind.EndOfDocumentationCommentToken:
			return null;
		default:
			return null;
		}
	}

	private bool IsXmlNodeStartOrStop()
	{
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.LessThanToken:
		case SyntaxKind.GreaterThanToken:
		case SyntaxKind.SlashGreaterThanToken:
		case SyntaxKind.LessThanSlashToken:
		case SyntaxKind.XmlCommentStartToken:
		case SyntaxKind.XmlCDataStartToken:
		case SyntaxKind.XmlProcessingInstructionStartToken:
		case SyntaxKind.EndOfDocumentationCommentToken:
			return true;
		default:
			return false;
		}
	}

	private XmlNodeSyntax ParseXmlText()
	{
		SyntaxListBuilder syntaxListBuilder = _pool.Allocate();
		while (CurrentToken.Kind == SyntaxKind.XmlTextLiteralToken || CurrentToken.Kind == SyntaxKind.XmlTextLiteralNewLineToken || CurrentToken.Kind == SyntaxKind.XmlEntityLiteralToken)
		{
			syntaxListBuilder.Add(EatToken());
		}
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<GreenNode> syntaxList = syntaxListBuilder.ToList();
		_pool.Free(syntaxListBuilder);
		return SyntaxFactory.XmlText(syntaxList);
	}

	private XmlNodeSyntax ParseXmlElement()
	{
		SyntaxToken syntaxToken = EatToken(SyntaxKind.LessThanToken);
		LexerMode mode = SetMode(LexerMode.XmlElementTag);
		XmlNameSyntax elementName = ParseXmlName();
		if (syntaxToken.GetTrailingTriviaWidth() > 0 || elementName.GetLeadingTriviaWidth() > 0)
		{
			elementName = WithXmlParseError(elementName, XmlParseErrorCode.XML_InvalidWhitespace);
		}
		SyntaxListBuilder<XmlAttributeSyntax> syntaxListBuilder = _pool.Allocate<XmlAttributeSyntax>();
		try
		{
			ParseXmlAttributes(ref elementName, syntaxListBuilder);
			if (CurrentToken.Kind == SyntaxKind.GreaterThanToken)
			{
				XmlElementStartTagSyntax startTag = SyntaxFactory.XmlElementStartTag(syntaxToken, elementName, syntaxListBuilder, EatToken());
				SetMode(LexerMode.XmlDocComment);
				SyntaxListBuilder<XmlNodeSyntax> syntaxListBuilder2 = _pool.Allocate<XmlNodeSyntax>();
				try
				{
					ParseXmlNodes(syntaxListBuilder2);
					SyntaxToken syntaxToken2 = EatToken(SyntaxKind.LessThanSlashToken, reportError: false);
					XmlNameSyntax startNode;
					SyntaxToken greaterThanToken;
					if (syntaxToken2.IsMissing)
					{
						ResetMode(mode);
						syntaxToken2 = WithXmlParseError(syntaxToken2, XmlParseErrorCode.XML_EndTagExpected, elementName.ToString());
						startNode = SyntaxFactory.XmlName(null, SyntaxFactory.MissingToken(SyntaxKind.IdentifierToken));
						greaterThanToken = SyntaxFactory.MissingToken(SyntaxKind.GreaterThanToken);
					}
					else
					{
						SetMode(LexerMode.XmlElementTag);
						startNode = ParseXmlName();
						if (syntaxToken2.GetTrailingTriviaWidth() > 0 || startNode.GetLeadingTriviaWidth() > 0)
						{
							startNode = WithXmlParseError(startNode, XmlParseErrorCode.XML_InvalidWhitespace);
						}
						if (!startNode.IsMissing && !MatchingXmlNames(elementName, startNode))
						{
							startNode = WithXmlParseError(startNode, XmlParseErrorCode.XML_ElementTypeMatch, startNode.ToString(), elementName.ToString());
						}
						if (CurrentToken.Kind != SyntaxKind.GreaterThanToken)
						{
							SkipBadTokens(ref startNode, null, (DocumentationCommentParser p) => p.CurrentToken.Kind != SyntaxKind.GreaterThanToken, (DocumentationCommentParser p) => p.IsXmlNodeStartOrStop(), XmlParseErrorCode.XML_InvalidToken);
						}
						greaterThanToken = EatToken(SyntaxKind.GreaterThanToken);
					}
					XmlElementEndTagSyntax endTag = SyntaxFactory.XmlElementEndTag(syntaxToken2, startNode, greaterThanToken);
					ResetMode(mode);
					return SyntaxFactory.XmlElement(startTag, syntaxListBuilder2.ToList(), endTag);
				}
				finally
				{
					_pool.Free(syntaxListBuilder2);
				}
			}
			SyntaxToken syntaxToken3 = EatToken(SyntaxKind.SlashGreaterThanToken, reportError: false);
			if (syntaxToken3.IsMissing && !elementName.IsMissing)
			{
				syntaxToken3 = WithXmlParseError(syntaxToken3, XmlParseErrorCode.XML_ExpectedEndOfTag, elementName.ToString());
			}
			ResetMode(mode);
			return SyntaxFactory.XmlEmptyElement(syntaxToken, elementName, syntaxListBuilder, syntaxToken3);
		}
		finally
		{
			_pool.Free(syntaxListBuilder);
		}
	}

	private static bool MatchingXmlNames(XmlNameSyntax name, XmlNameSyntax endName)
	{
		if (name == endName)
		{
			return true;
		}
		if (!name.HasLeadingTrivia && !endName.HasTrailingTrivia && name.IsEquivalentTo(endName))
		{
			return true;
		}
		return name.ToString() == endName.ToString();
	}

	private void ParseXmlAttributes(ref XmlNameSyntax elementName, SyntaxListBuilder<XmlAttributeSyntax> attrs)
	{
		_attributesSeen.Clear();
		while (true)
		{
			if (CurrentToken.Kind == SyntaxKind.IdentifierToken)
			{
				XmlAttributeSyntax xmlAttributeSyntax = ParseXmlAttribute(elementName);
				string text = xmlAttributeSyntax.Name.ToString();
				if (_attributesSeen.Contains(text))
				{
					xmlAttributeSyntax = WithXmlParseError(xmlAttributeSyntax, XmlParseErrorCode.XML_DuplicateAttribute, text);
				}
				else
				{
					_attributesSeen.Add(text);
				}
				attrs.Add(xmlAttributeSyntax);
			}
			else if (SkipBadTokens(ref elementName, attrs, (DocumentationCommentParser p) => p.CurrentToken.Kind != SyntaxKind.IdentifierName, (DocumentationCommentParser p) => p.CurrentToken.Kind == SyntaxKind.GreaterThanToken || p.CurrentToken.Kind == SyntaxKind.SlashGreaterThanToken || p.CurrentToken.Kind == SyntaxKind.LessThanToken || p.CurrentToken.Kind == SyntaxKind.LessThanSlashToken || p.CurrentToken.Kind == SyntaxKind.EndOfDocumentationCommentToken || p.CurrentToken.Kind == SyntaxKind.EndOfFileToken, XmlParseErrorCode.XML_InvalidToken) == SkipResult.Abort)
			{
				break;
			}
		}
	}

	private SkipResult SkipBadTokens<T>(ref T startNode, SyntaxListBuilder list, Func<DocumentationCommentParser, bool> isNotExpectedFunction, Func<DocumentationCommentParser, bool> abortFunction, XmlParseErrorCode error) where T : CSharpSyntaxNode
	{
		SyntaxListBuilder<SyntaxToken> syntaxListBuilder = default;
		bool flag = false;
		try
		{
			SkipResult result = SkipResult.Continue;
			while (isNotExpectedFunction(this))
			{
				if (abortFunction(this))
				{
					result = SkipResult.Abort;
					break;
				}
				if (syntaxListBuilder.IsNull)
				{
					syntaxListBuilder = _pool.Allocate<SyntaxToken>();
				}
				SyntaxToken syntaxToken = EatToken();
				if (!flag)
				{
					syntaxToken = WithXmlParseError(syntaxToken, error, syntaxToken.ToString());
					flag = true;
				}
				syntaxListBuilder.Add(syntaxToken);
			}
			if (!syntaxListBuilder.IsNull && syntaxListBuilder.Count > 0)
			{
				if (list == null || list.Count == 0)
				{
					startNode = AddTrailingSkippedSyntax(startNode, syntaxListBuilder.ToListNode());
				}
				else
				{
					list[list.Count - 1] = AddTrailingSkippedSyntax((CSharpSyntaxNode)list[list.Count - 1], syntaxListBuilder.ToListNode());
				}
				return result;
			}
			return SkipResult.Abort;
		}
		finally
		{
			if (!syntaxListBuilder.IsNull)
			{
				_pool.Free(syntaxListBuilder);
			}
		}
	}

	private XmlAttributeSyntax ParseXmlAttribute(XmlNameSyntax elementName)
	{
		XmlNameSyntax xmlNameSyntax = ParseXmlName();
		if (xmlNameSyntax.GetLeadingTriviaWidth() == 0)
		{
			xmlNameSyntax = WithXmlParseError(xmlNameSyntax, XmlParseErrorCode.XML_WhitespaceMissing);
		}
		SyntaxToken syntaxToken = EatToken(SyntaxKind.EqualsToken, reportError: false);
		if (syntaxToken.IsMissing)
		{
			syntaxToken = WithXmlParseError(syntaxToken, XmlParseErrorCode.XML_MissingEqualsAttribute);
			SyntaxKind kind = CurrentToken.Kind;
			if (kind - 8213 > SyntaxKind.List)
			{
				return SyntaxFactory.XmlTextAttribute(xmlNameSyntax, syntaxToken, SyntaxFactory.MissingToken(SyntaxKind.DoubleQuoteToken), default, SyntaxFactory.MissingToken(SyntaxKind.DoubleQuoteToken));
			}
		}
		string valueText = xmlNameSyntax.LocalName.ValueText;
		bool flag = xmlNameSyntax.Prefix == null;
		SyntaxToken startQuote;
		SyntaxToken endQuote;
		if (flag && DocumentationCommentXmlNames.AttributeEquals(valueText, "cref") && !IsVerbatimCref())
		{
			ParseCrefAttribute(out startQuote, out var cref, out endQuote);
			return SyntaxFactory.XmlCrefAttribute(xmlNameSyntax, syntaxToken, startQuote, cref, endQuote);
		}
		if (flag && DocumentationCommentXmlNames.AttributeEquals(valueText, "name") && XmlElementSupportsNameAttribute(elementName))
		{
			ParseNameAttribute(out startQuote, out var identifier, out endQuote);
			return SyntaxFactory.XmlNameAttribute(xmlNameSyntax, syntaxToken, startQuote, identifier, endQuote);
		}
		SyntaxListBuilder<SyntaxToken> syntaxListBuilder = _pool.Allocate<SyntaxToken>();
		try
		{
			ParseXmlAttributeText(out startQuote, syntaxListBuilder, out endQuote);
			return SyntaxFactory.XmlTextAttribute(xmlNameSyntax, syntaxToken, startQuote, syntaxListBuilder, endQuote);
		}
		finally
		{
			_pool.Free(syntaxListBuilder);
		}
	}

	private static bool XmlElementSupportsNameAttribute(XmlNameSyntax elementName)
	{
		if (elementName.Prefix != null)
		{
			return false;
		}
		string valueText = elementName.LocalName.ValueText;
		if (!DocumentationCommentXmlNames.ElementEquals(valueText, "param") && !DocumentationCommentXmlNames.ElementEquals(valueText, "paramref") && !DocumentationCommentXmlNames.ElementEquals(valueText, "typeparam"))
		{
			return DocumentationCommentXmlNames.ElementEquals(valueText, "typeparamref");
		}
		return true;
	}

	private bool IsVerbatimCref()
	{
		bool result = false;
		ResetPoint point = GetResetPoint();
		SyntaxToken syntaxToken = EatToken((CurrentToken.Kind == SyntaxKind.SingleQuoteToken) ? SyntaxKind.SingleQuoteToken : SyntaxKind.DoubleQuoteToken);
		SetMode(LexerMode.XmlCharacter);
		SyntaxToken currentToken = CurrentToken;
		if ((currentToken.Kind == SyntaxKind.XmlTextLiteralToken || currentToken.Kind == SyntaxKind.XmlEntityLiteralToken) && currentToken.ValueText != SyntaxFacts.GetText(syntaxToken.Kind) && currentToken.ValueText != ":")
		{
			EatToken();
			currentToken = CurrentToken;
			if ((currentToken.Kind == SyntaxKind.XmlTextLiteralToken || currentToken.Kind == SyntaxKind.XmlEntityLiteralToken) && currentToken.ValueText == ":")
			{
				result = true;
			}
		}
		Reset(ref point);
		Release(ref point);
		return result;
	}

	private void ParseCrefAttribute(out SyntaxToken startQuote, out CrefSyntax cref, out SyntaxToken endQuote)
	{
		startQuote = ParseXmlAttributeStartQuote();
		SyntaxKind kind = startQuote.Kind;
		LexerMode mode = SetMode((kind == SyntaxKind.SingleQuoteToken) ? LexerMode.XmlCrefQuote : LexerMode.XmlCrefDoubleQuote);
		cref = ParseCrefAttributeValue();
		ResetMode(mode);
		endQuote = ParseXmlAttributeEndQuote(kind);
	}

	private void ParseNameAttribute(out SyntaxToken startQuote, out IdentifierNameSyntax identifier, out SyntaxToken endQuote)
	{
		startQuote = ParseXmlAttributeStartQuote();
		SyntaxKind kind = startQuote.Kind;
		LexerMode mode = SetMode((kind == SyntaxKind.SingleQuoteToken) ? LexerMode.XmlNameQuote : LexerMode.XmlNameDoubleQuote);
		identifier = ParseNameAttributeValue();
		ResetMode(mode);
		endQuote = ParseXmlAttributeEndQuote(kind);
	}

	private void ParseXmlAttributeText(out SyntaxToken startQuote, SyntaxListBuilder<SyntaxToken> textTokens, out SyntaxToken endQuote)
	{
		startQuote = ParseXmlAttributeStartQuote();
		SyntaxKind kind = startQuote.Kind;
		if (startQuote.IsMissing && startQuote.FullWidth == 0)
		{
			endQuote = SyntaxFactory.MissingToken(kind);
			return;
		}
		LexerMode mode = SetMode((kind == SyntaxKind.SingleQuoteToken) ? LexerMode.XmlAttributeTextQuote : LexerMode.XmlAttributeTextDoubleQuote);
		while (CurrentToken.Kind == SyntaxKind.XmlTextLiteralToken || CurrentToken.Kind == SyntaxKind.XmlTextLiteralNewLineToken || CurrentToken.Kind == SyntaxKind.XmlEntityLiteralToken || CurrentToken.Kind == SyntaxKind.LessThanToken)
		{
			SyntaxToken syntaxToken = EatToken();
			if (syntaxToken.Kind == SyntaxKind.LessThanToken)
			{
				syntaxToken = WithXmlParseError(syntaxToken, XmlParseErrorCode.XML_LessThanInAttributeValue);
			}
			textTokens.Add(syntaxToken);
		}
		ResetMode(mode);
		endQuote = ParseXmlAttributeEndQuote(kind);
	}

	private SyntaxToken ParseXmlAttributeStartQuote()
	{
		if (IsNonAsciiQuotationMark(CurrentToken))
		{
			return SkipNonAsciiQuotationMark();
		}
		SyntaxKind kind = ((CurrentToken.Kind == SyntaxKind.SingleQuoteToken) ? SyntaxKind.SingleQuoteToken : SyntaxKind.DoubleQuoteToken);
		SyntaxToken syntaxToken = EatToken(kind, reportError: false);
		if (syntaxToken.IsMissing)
		{
			syntaxToken = WithXmlParseError(syntaxToken, XmlParseErrorCode.XML_StringLiteralNoStartQuote);
		}
		return syntaxToken;
	}

	private SyntaxToken ParseXmlAttributeEndQuote(SyntaxKind quoteKind)
	{
		if (IsNonAsciiQuotationMark(CurrentToken))
		{
			return SkipNonAsciiQuotationMark();
		}
		SyntaxToken syntaxToken = EatToken(quoteKind, reportError: false);
		if (syntaxToken.IsMissing)
		{
			syntaxToken = WithXmlParseError(syntaxToken, XmlParseErrorCode.XML_StringLiteralNoEndQuote);
		}
		return syntaxToken;
	}

	private SyntaxToken SkipNonAsciiQuotationMark()
	{
		SyntaxToken node = SyntaxFactory.MissingToken(SyntaxKind.DoubleQuoteToken);
		node = AddTrailingSkippedSyntax(node, EatToken());
		return WithXmlParseError(node, XmlParseErrorCode.XML_StringLiteralNonAsciiQuote);
	}

	private static bool IsNonAsciiQuotationMark(SyntaxToken token)
	{
		if (token.Text.Length == 1)
		{
			return SyntaxFacts.IsNonAsciiQuotationMark(token.Text[0]);
		}
		return false;
	}

	private XmlNameSyntax ParseXmlName()
	{
		SyntaxToken syntaxToken = EatToken(SyntaxKind.IdentifierToken);
		XmlPrefixSyntax prefix = null;
		if (CurrentToken.Kind == SyntaxKind.ColonToken)
		{
			SyntaxToken syntaxToken2 = EatToken();
			int trailingTriviaWidth = syntaxToken.GetTrailingTriviaWidth();
			int leadingTriviaWidth = syntaxToken2.GetLeadingTriviaWidth();
			if (trailingTriviaWidth > 0 || leadingTriviaWidth > 0)
			{
				int offset = -trailingTriviaWidth;
				int width = trailingTriviaWidth + leadingTriviaWidth;
				syntaxToken2 = WithAdditionalDiagnostics(syntaxToken2, new XmlSyntaxDiagnosticInfo(offset, width, XmlParseErrorCode.XML_InvalidWhitespace));
			}
			prefix = SyntaxFactory.XmlPrefix(syntaxToken, syntaxToken2);
			syntaxToken = EatToken(SyntaxKind.IdentifierToken);
			int trailingTriviaWidth2 = syntaxToken2.GetTrailingTriviaWidth();
			int leadingTriviaWidth2 = syntaxToken.GetLeadingTriviaWidth();
			if (trailingTriviaWidth2 > 0 || leadingTriviaWidth2 > 0)
			{
				int offset2 = -trailingTriviaWidth2;
				int width2 = trailingTriviaWidth2 + leadingTriviaWidth2;
				syntaxToken = WithAdditionalDiagnostics(syntaxToken, new XmlSyntaxDiagnosticInfo(offset2, width2, XmlParseErrorCode.XML_InvalidWhitespace));
			}
		}
		return SyntaxFactory.XmlName(prefix, syntaxToken);
	}

	private XmlCommentSyntax ParseXmlComment()
	{
		SyntaxToken lessThanExclamationMinusMinusToken = EatToken(SyntaxKind.XmlCommentStartToken);
		LexerMode mode = SetMode(LexerMode.XmlCommentText);
		SyntaxListBuilder<SyntaxToken> syntaxListBuilder = _pool.Allocate<SyntaxToken>();
		while (CurrentToken.Kind == SyntaxKind.XmlTextLiteralToken || CurrentToken.Kind == SyntaxKind.XmlTextLiteralNewLineToken || CurrentToken.Kind == SyntaxKind.MinusMinusToken)
		{
			SyntaxToken syntaxToken = EatToken();
			if (syntaxToken.Kind == SyntaxKind.MinusMinusToken)
			{
				syntaxToken = WithXmlParseError(syntaxToken, XmlParseErrorCode.XML_IncorrectComment);
			}
			syntaxListBuilder.Add(syntaxToken);
		}
		Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> textTokens = syntaxListBuilder.ToList();
		_pool.Free(syntaxListBuilder);
		SyntaxToken minusMinusGreaterThanToken = EatToken(SyntaxKind.XmlCommentEndToken);
		ResetMode(mode);
		return SyntaxFactory.XmlComment(lessThanExclamationMinusMinusToken, textTokens, minusMinusGreaterThanToken);
	}

	private XmlCDataSectionSyntax ParseXmlCDataSection()
	{
		SyntaxToken startCDataToken = EatToken(SyntaxKind.XmlCDataStartToken);
		LexerMode mode = SetMode(LexerMode.XmlCDataSectionText);
		SyntaxListBuilder<SyntaxToken> syntaxListBuilder = new SyntaxListBuilder<SyntaxToken>(10);
		while (CurrentToken.Kind == SyntaxKind.XmlTextLiteralToken || CurrentToken.Kind == SyntaxKind.XmlTextLiteralNewLineToken)
		{
			syntaxListBuilder.Add(EatToken());
		}
		SyntaxToken endCDataToken = EatToken(SyntaxKind.XmlCDataEndToken);
		ResetMode(mode);
		return SyntaxFactory.XmlCDataSection(startCDataToken, syntaxListBuilder, endCDataToken);
	}

	private XmlProcessingInstructionSyntax ParseXmlProcessingInstruction()
	{
		SyntaxToken startProcessingInstructionToken = EatToken(SyntaxKind.XmlProcessingInstructionStartToken);
		LexerMode mode = SetMode(LexerMode.XmlElementTag);
		XmlNameSyntax name = ParseXmlName();
		SetMode(LexerMode.XmlProcessingInstructionText);
		SyntaxListBuilder<SyntaxToken> syntaxListBuilder = new SyntaxListBuilder<SyntaxToken>(10);
		while (CurrentToken.Kind == SyntaxKind.XmlTextLiteralToken || CurrentToken.Kind == SyntaxKind.XmlTextLiteralNewLineToken)
		{
			SyntaxToken node = EatToken();
			syntaxListBuilder.Add(node);
		}
		SyntaxToken endProcessingInstructionToken = EatToken(SyntaxKind.XmlProcessingInstructionEndToken);
		ResetMode(mode);
		return SyntaxFactory.XmlProcessingInstruction(startProcessingInstructionToken, name, syntaxListBuilder, endProcessingInstructionToken);
	}

	protected override SyntaxDiagnosticInfo GetExpectedTokenError(SyntaxKind expected, SyntaxKind actual, int offset, int length)
	{
		if (InCref)
		{
			SyntaxDiagnosticInfo expectedTokenError = base.GetExpectedTokenError(expected, actual, offset, length);
			return new SyntaxDiagnosticInfo(expectedTokenError.Offset, expectedTokenError.Width, ErrorCode.WRN_ErrorOverride, expectedTokenError, expectedTokenError.Code);
		}
		if (expected == SyntaxKind.IdentifierToken)
		{
			return new XmlSyntaxDiagnosticInfo(offset, length, XmlParseErrorCode.XML_ExpectedIdentifier);
		}
		return new XmlSyntaxDiagnosticInfo(offset, length, XmlParseErrorCode.XML_InvalidToken, SyntaxFacts.GetText(actual));
	}

	protected override SyntaxDiagnosticInfo GetExpectedTokenError(SyntaxKind expected, SyntaxKind actual)
	{
		if (InCref)
		{
			GetDiagnosticSpanForMissingToken(out var offset, out var width);
			return GetExpectedTokenError(expected, actual, offset, width);
		}
		if (expected == SyntaxKind.IdentifierToken)
		{
			return new XmlSyntaxDiagnosticInfo(XmlParseErrorCode.XML_ExpectedIdentifier);
		}
		return new XmlSyntaxDiagnosticInfo(XmlParseErrorCode.XML_InvalidToken, SyntaxFacts.GetText(actual));
	}

	private TNode WithXmlParseError<TNode>(TNode node, XmlParseErrorCode code) where TNode : CSharpSyntaxNode
	{
		return WithAdditionalDiagnostics(node, new XmlSyntaxDiagnosticInfo(0, node.Width, code));
	}

	private TNode WithXmlParseError<TNode>(TNode node, XmlParseErrorCode code, params string[] args) where TNode : CSharpSyntaxNode
	{
		DiagnosticInfo[] array = new DiagnosticInfo[1];
		array[0] = new XmlSyntaxDiagnosticInfo(0, node.Width, code, args);
		return WithAdditionalDiagnostics(node, array);
	}

	private SyntaxToken WithXmlParseError(SyntaxToken node, XmlParseErrorCode code, params string[] args)
	{
		DiagnosticInfo[] array = new DiagnosticInfo[1];
		array[0] = new XmlSyntaxDiagnosticInfo(0, node.Width, code, args);
		return WithAdditionalDiagnostics(node, array);
	}

	protected override TNode WithAdditionalDiagnostics<TNode>(TNode node, params DiagnosticInfo[] diagnostics)
	{
		if ((int)Options.DocumentationMode < 2)
		{
			return node;
		}
		return base.WithAdditionalDiagnostics(node, diagnostics);
	}

	private CrefSyntax ParseCrefAttributeValue()
	{
		TypeSyntax typeSyntax = ParseCrefType(typeArgumentsMustBeIdentifiers: true, checkForMember: true);
		CrefSyntax crefSyntax;
		if (typeSyntax == null)
		{
			crefSyntax = ParseMemberCref();
		}
		else if (IsEndOfCrefAttribute)
		{
			crefSyntax = SyntaxFactory.TypeCref(typeSyntax);
		}
		else if (typeSyntax.Kind != SyntaxKind.QualifiedName && CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			CrefParameterListSyntax parameters = ParseCrefParameterList();
			crefSyntax = SyntaxFactory.NameMemberCref(typeSyntax, parameters);
		}
		else
		{
			SyntaxToken dotToken = EatToken(SyntaxKind.DotToken);
			MemberCrefSyntax member = ParseMemberCref();
			crefSyntax = SyntaxFactory.QualifiedCref(typeSyntax, dotToken, member);
		}
		bool flag = !IsEndOfCrefAttribute || crefSyntax.ContainsDiagnostics;
		if (!IsEndOfCrefAttribute)
		{
			SyntaxListBuilder<SyntaxToken> syntaxListBuilder = _pool.Allocate<SyntaxToken>();
			while (!IsEndOfCrefAttribute)
			{
				syntaxListBuilder.Add(EatToken());
			}
			crefSyntax = AddTrailingSkippedSyntax(crefSyntax, syntaxListBuilder.ToListNode());
			_pool.Free(syntaxListBuilder);
		}
		if (flag)
		{
			crefSyntax = AddError(crefSyntax, ErrorCode.WRN_BadXMLRefSyntax, crefSyntax.ToFullString());
		}
		return crefSyntax;
	}

	private MemberCrefSyntax ParseMemberCref()
	{
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.ThisKeyword:
			return ParseIndexerMemberCref();
		case SyntaxKind.OperatorKeyword:
			return ParseOperatorMemberCref();
		case SyntaxKind.ExplicitKeyword:
		case SyntaxKind.ImplicitKeyword:
			return ParseConversionOperatorMemberCref();
		default:
			return ParseNameMemberCref();
		}
	}

	private NameMemberCrefSyntax ParseNameMemberCref()
	{
		SimpleNameSyntax name = ParseCrefName(typeArgumentsMustBeIdentifiers: true);
		CrefParameterListSyntax parameters = ParseCrefParameterList();
		return SyntaxFactory.NameMemberCref(name, parameters);
	}

	private IndexerMemberCrefSyntax ParseIndexerMemberCref()
	{
		SyntaxToken thisKeyword = EatToken();
		CrefBracketedParameterListSyntax parameters = ParseBracketedCrefParameterList();
		return SyntaxFactory.IndexerMemberCref(thisKeyword, parameters);
	}

	private OperatorMemberCrefSyntax ParseOperatorMemberCref()
	{
		SyntaxToken operatorKeyword = EatToken();
		SyntaxToken syntaxToken;
		if (SyntaxFacts.IsAnyOverloadableOperator(CurrentToken.Kind))
		{
			syntaxToken = EatToken();
		}
		else
		{
			syntaxToken = SyntaxFactory.MissingToken(SyntaxKind.PlusToken);
			GetDiagnosticSpanForMissingToken(out var offset, out var width);
			if (SyntaxFacts.IsUnaryOperatorDeclarationToken(CurrentToken.Kind) || SyntaxFacts.IsBinaryExpressionOperatorToken(CurrentToken.Kind))
			{
				syntaxToken = AddTrailingSkippedSyntax(syntaxToken, EatToken());
			}
			SyntaxDiagnosticInfo syntaxDiagnosticInfo = new SyntaxDiagnosticInfo(offset, width, ErrorCode.ERR_OvlOperatorExpected);
			SyntaxDiagnosticInfo syntaxDiagnosticInfo2 = new SyntaxDiagnosticInfo(offset, width, ErrorCode.WRN_ErrorOverride, syntaxDiagnosticInfo, syntaxDiagnosticInfo.Code);
			syntaxToken = WithAdditionalDiagnostics(syntaxToken, syntaxDiagnosticInfo2);
		}
		if (syntaxToken.Kind == SyntaxKind.GreaterThanToken && syntaxToken.GetTrailingTriviaWidth() == 0 && CurrentToken.GetLeadingTriviaWidth() == 0)
		{
			if (CurrentToken.Kind == SyntaxKind.GreaterThanToken)
			{
				SyntaxToken syntaxToken2 = EatToken();
				syntaxToken = SyntaxFactory.Token(syntaxToken.GetLeadingTrivia(), SyntaxKind.GreaterThanGreaterThanToken, syntaxToken.Text + syntaxToken2.Text, syntaxToken.ValueText + syntaxToken2.ValueText, syntaxToken2.GetTrailingTrivia());
			}
			else if (CurrentToken.Kind == SyntaxKind.EqualsToken)
			{
				SyntaxToken syntaxToken3 = EatToken();
				syntaxToken = SyntaxFactory.Token(syntaxToken.GetLeadingTrivia(), SyntaxKind.GreaterThanEqualsToken, syntaxToken.Text + syntaxToken3.Text, syntaxToken.ValueText + syntaxToken3.ValueText, syntaxToken3.GetTrailingTrivia());
			}
			else if (CurrentToken.Kind == SyntaxKind.GreaterThanEqualsToken)
			{
				SyntaxToken syntaxToken4 = EatToken();
				SyntaxToken syntaxToken5 = SyntaxFactory.Token(syntaxToken.GetLeadingTrivia(), SyntaxKind.GreaterThanGreaterThanEqualsToken, syntaxToken.Text + syntaxToken4.Text, syntaxToken.ValueText + syntaxToken4.ValueText, syntaxToken4.GetTrailingTrivia());
				syntaxToken = SyntaxFactory.MissingToken(SyntaxKind.PlusToken);
				syntaxToken = AddTrailingSkippedSyntax(syntaxToken, syntaxToken5);
				int width2 = syntaxToken5.Width;
				SyntaxDiagnosticInfo syntaxDiagnosticInfo3 = new SyntaxDiagnosticInfo(0, width2, ErrorCode.ERR_OvlOperatorExpected);
				SyntaxDiagnosticInfo syntaxDiagnosticInfo4 = new SyntaxDiagnosticInfo(0, width2, ErrorCode.WRN_ErrorOverride, syntaxDiagnosticInfo3, syntaxDiagnosticInfo3.Code);
				syntaxToken = WithAdditionalDiagnostics(syntaxToken, syntaxDiagnosticInfo4);
			}
		}
		CrefParameterListSyntax parameters = ParseCrefParameterList();
		return SyntaxFactory.OperatorMemberCref(operatorKeyword, syntaxToken, parameters);
	}

	private ConversionOperatorMemberCrefSyntax ParseConversionOperatorMemberCref()
	{
		SyntaxToken implicitOrExplicitKeyword = EatToken();
		SyntaxToken operatorKeyword = EatToken(SyntaxKind.OperatorKeyword);
		TypeSyntax type = ParseCrefType(typeArgumentsMustBeIdentifiers: false);
		CrefParameterListSyntax parameters = ParseCrefParameterList();
		return SyntaxFactory.ConversionOperatorMemberCref(implicitOrExplicitKeyword, operatorKeyword, type, parameters);
	}

	private CrefParameterListSyntax ParseCrefParameterList()
	{
		return (CrefParameterListSyntax)ParseBaseCrefParameterList(useSquareBrackets: false);
	}

	private CrefBracketedParameterListSyntax ParseBracketedCrefParameterList()
	{
		return (CrefBracketedParameterListSyntax)ParseBaseCrefParameterList(useSquareBrackets: true);
	}

	private BaseCrefParameterListSyntax ParseBaseCrefParameterList(bool useSquareBrackets)
	{
		SyntaxKind syntaxKind = (useSquareBrackets ? SyntaxKind.OpenBracketToken : SyntaxKind.OpenParenToken);
		SyntaxKind syntaxKind2 = (useSquareBrackets ? SyntaxKind.CloseBracketToken : SyntaxKind.CloseParenToken);
		if (CurrentToken.Kind != syntaxKind)
		{
			return null;
		}
		SyntaxToken syntaxToken = EatToken(syntaxKind);
		SeparatedSyntaxListBuilder<CrefParameterSyntax> builder = _pool.AllocateSeparated<CrefParameterSyntax>();
		try
		{
			while (CurrentToken.Kind == SyntaxKind.CommaToken || IsPossibleCrefParameter())
			{
				builder.Add(ParseCrefParameter());
				if (CurrentToken.Kind != syntaxKind2)
				{
					SyntaxToken syntaxToken2 = EatToken(SyntaxKind.CommaToken);
					if (!syntaxToken2.IsMissing || IsPossibleCrefParameter())
					{
						builder.AddSeparator(syntaxToken2);
					}
				}
			}
			SyntaxToken syntaxToken3 = EatToken(syntaxKind2);
			return useSquareBrackets ? ((BaseCrefParameterListSyntax)SyntaxFactory.CrefBracketedParameterList(syntaxToken, builder, syntaxToken3)) : ((BaseCrefParameterListSyntax)SyntaxFactory.CrefParameterList(syntaxToken, builder, syntaxToken3));
		}
		finally
		{
			_pool.Free(in builder);
		}
	}

	private bool IsPossibleCrefParameter()
	{
		SyntaxKind kind = CurrentToken.Kind;
		if (kind - 8360 <= (SyntaxKind)2 || kind == SyntaxKind.IdentifierToken)
		{
			return true;
		}
		return SyntaxFacts.IsPredefinedType(kind);
	}

	private CrefParameterSyntax ParseCrefParameter()
	{
		SyntaxToken refKindKeyword = null;
		SyntaxKind kind = CurrentToken.Kind;
		if (kind - 8360 <= (SyntaxKind)2)
		{
			refKindKeyword = EatToken();
		}
		TypeSyntax type = ParseCrefType(typeArgumentsMustBeIdentifiers: false);
		return SyntaxFactory.CrefParameter(refKindKeyword, type);
	}

	private SimpleNameSyntax ParseCrefName(bool typeArgumentsMustBeIdentifiers)
	{
		SyntaxToken identifier = EatToken(SyntaxKind.IdentifierToken);
		if (CurrentToken.Kind != SyntaxKind.LessThanToken)
		{
			return SyntaxFactory.IdentifierName(identifier);
		}
		SyntaxToken node = EatToken();
		SeparatedSyntaxListBuilder<TypeSyntax> builder = _pool.AllocateSeparated<TypeSyntax>();
		try
		{
			while (true)
			{
				TypeSyntax typeSyntax = ParseCrefType(typeArgumentsMustBeIdentifiers);
				if (typeArgumentsMustBeIdentifiers && typeSyntax.Kind != SyntaxKind.IdentifierName)
				{
					typeSyntax = AddError(typeSyntax, ErrorCode.WRN_ErrorOverride, new SyntaxDiagnosticInfo(ErrorCode.ERR_TypeParamMustBeIdentifier), $"{81:d4}");
				}
				builder.Add(typeSyntax);
				SyntaxKind kind = CurrentToken.Kind;
				if (kind != SyntaxKind.CommaToken && kind != SyntaxKind.IdentifierToken && !SyntaxFacts.IsPredefinedType(CurrentToken.Kind))
				{
					break;
				}
				builder.AddSeparator(EatToken(SyntaxKind.CommaToken));
			}
			SyntaxToken greaterThanToken = EatToken(SyntaxKind.GreaterThanToken);
			node = CheckFeatureAvailability(node, MessageID.IDS_FeatureGenerics, forceWarning: true);
			return SyntaxFactory.GenericName(identifier, SyntaxFactory.TypeArgumentList(node, builder, greaterThanToken));
		}
		finally
		{
			_pool.Free(in builder);
		}
	}

	private TypeSyntax ParseCrefType(bool typeArgumentsMustBeIdentifiers, bool checkForMember = false)
	{
		TypeSyntax typeSyntax = ParseCrefTypeHelper(typeArgumentsMustBeIdentifiers, checkForMember);
		if (!typeArgumentsMustBeIdentifiers)
		{
			return ParseCrefTypeSuffix(typeSyntax);
		}
		return typeSyntax;
	}

	private TypeSyntax ParseCrefTypeHelper(bool typeArgumentsMustBeIdentifiers, bool checkForMember = false)
	{
		if (SyntaxFacts.IsPredefinedType(CurrentToken.Kind))
		{
			return SyntaxFactory.PredefinedType(EatToken());
		}
		NameSyntax nameSyntax;
		if (CurrentToken.Kind == SyntaxKind.IdentifierToken && PeekToken(1).Kind == SyntaxKind.ColonColonToken)
		{
			SyntaxToken syntaxToken = EatToken();
			if (syntaxToken.ContextualKind == SyntaxKind.GlobalKeyword)
			{
				syntaxToken = SyntaxParser.ConvertToKeyword(syntaxToken);
			}
			syntaxToken = CheckFeatureAvailability(syntaxToken, MessageID.IDS_FeatureGlobalNamespace, forceWarning: true);
			SyntaxToken colonColonToken = EatToken();
			SimpleNameSyntax name = ParseCrefName(typeArgumentsMustBeIdentifiers);
			nameSyntax = SyntaxFactory.AliasQualifiedName(SyntaxFactory.IdentifierName(syntaxToken), colonColonToken, name);
		}
		else
		{
			ResetPoint point = GetResetPoint();
			nameSyntax = ParseCrefName(typeArgumentsMustBeIdentifiers);
			if (checkForMember && (nameSyntax.IsMissing || CurrentToken.Kind != SyntaxKind.DotToken))
			{
				Reset(ref point);
				Release(ref point);
				return null;
			}
			Release(ref point);
		}
		while (CurrentToken.Kind == SyntaxKind.DotToken)
		{
			ResetPoint point2 = GetResetPoint();
			SyntaxToken dotToken = EatToken();
			SimpleNameSyntax simpleNameSyntax = ParseCrefName(typeArgumentsMustBeIdentifiers);
			if (checkForMember && (simpleNameSyntax.IsMissing || CurrentToken.Kind != SyntaxKind.DotToken))
			{
				Reset(ref point2);
				Release(ref point2);
				return nameSyntax;
			}
			Release(ref point2);
			nameSyntax = SyntaxFactory.QualifiedName(nameSyntax, dotToken, simpleNameSyntax);
		}
		return nameSyntax;
	}

	private TypeSyntax ParseCrefTypeSuffix(TypeSyntax type)
	{
		if (CurrentToken.Kind == SyntaxKind.QuestionToken)
		{
			type = SyntaxFactory.NullableType(type, EatToken());
		}
		while (CurrentToken.Kind == SyntaxKind.AsteriskToken)
		{
			type = SyntaxFactory.PointerType(type, EatToken());
		}
		if (CurrentToken.Kind == SyntaxKind.OpenBracketToken)
		{
			OmittedArraySizeExpressionSyntax node = SyntaxFactory.OmittedArraySizeExpression(SyntaxFactory.Token(SyntaxKind.OmittedArraySizeExpressionToken));
			SyntaxListBuilder<ArrayRankSpecifierSyntax> syntaxListBuilder = _pool.Allocate<ArrayRankSpecifierSyntax>();
			try
			{
				while (CurrentToken.Kind == SyntaxKind.OpenBracketToken)
				{
					SyntaxToken openBracketToken = EatToken();
					SeparatedSyntaxListBuilder<ExpressionSyntax> builder = _pool.AllocateSeparated<ExpressionSyntax>();
					try
					{
						while (CurrentToken.Kind != SyntaxKind.CloseBracketToken && CurrentToken.Kind == SyntaxKind.CommaToken)
						{
							builder.Add(node);
							builder.AddSeparator(EatToken());
						}
						if ((builder.Count & 1) == 0)
						{
							builder.Add(node);
						}
						SyntaxToken closeBracketToken = EatToken(SyntaxKind.CloseBracketToken);
						syntaxListBuilder.Add(SyntaxFactory.ArrayRankSpecifier(openBracketToken, builder, closeBracketToken));
					}
					finally
					{
						_pool.Free(in builder);
					}
				}
				type = SyntaxFactory.ArrayType(type, syntaxListBuilder);
			}
			finally
			{
				_pool.Free(syntaxListBuilder);
			}
		}
		return type;
	}

	private IdentifierNameSyntax ParseNameAttributeValue()
	{
		SyntaxToken syntaxToken = EatToken(SyntaxKind.IdentifierToken, reportError: false);
		if (!IsEndOfNameAttribute)
		{
			SyntaxListBuilder<SyntaxToken> syntaxListBuilder = _pool.Allocate<SyntaxToken>();
			while (!IsEndOfNameAttribute)
			{
				syntaxListBuilder.Add(EatToken());
			}
			syntaxToken = AddTrailingSkippedSyntax(syntaxToken, syntaxListBuilder.ToListNode());
			_pool.Free(syntaxListBuilder);
		}
		return SyntaxFactory.IdentifierName(syntaxToken);
	}
}
