using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal class DirectiveParser : SyntaxParser
{
	private const int MAX_DIRECTIVE_IDENTIFIER_WIDTH = 128;

	private readonly DirectiveStack _context;

	internal DirectiveParser(Lexer lexer, DirectiveStack context)
		: base(lexer, LexerMode.Directive, null, null, allowModeReset: false)
	{
		_context = context;
	}

	public CSharpSyntaxNode ParseDirective(bool isActive, bool endIsActive, bool isAfterFirstTokenInFile, bool isAfterNonWhitespaceOnLine)
	{
		int position = lexer.TextWindow.Position;
		SyntaxToken syntaxToken = EatToken(SyntaxKind.HashToken, reportError: false);
		if (isAfterNonWhitespaceOnLine)
		{
			syntaxToken = AddError(syntaxToken, ErrorCode.ERR_BadDirectivePlacement);
		}
		SyntaxKind contextualKind = CurrentToken.ContextualKind;
		switch (contextualKind)
		{
		case SyntaxKind.IfKeyword:
			return ParseIfDirective(syntaxToken, EatContextualToken(contextualKind), isActive);
		case SyntaxKind.ElifKeyword:
			return ParseElifDirective(syntaxToken, EatContextualToken(contextualKind), isActive, endIsActive);
		case SyntaxKind.ElseKeyword:
			return ParseElseDirective(syntaxToken, EatContextualToken(contextualKind), isActive, endIsActive);
		case SyntaxKind.EndIfKeyword:
			return ParseEndIfDirective(syntaxToken, EatContextualToken(contextualKind), isActive, endIsActive);
		case SyntaxKind.RegionKeyword:
			return ParseRegionDirective(syntaxToken, EatContextualToken(contextualKind), isActive);
		case SyntaxKind.EndRegionKeyword:
			return ParseEndRegionDirective(syntaxToken, EatContextualToken(contextualKind), isActive);
		case SyntaxKind.DefineKeyword:
		case SyntaxKind.UndefKeyword:
			return ParseDefineOrUndefDirective(syntaxToken, EatContextualToken(contextualKind), isActive, isAfterFirstTokenInFile && !isAfterNonWhitespaceOnLine);
		case SyntaxKind.WarningKeyword:
		case SyntaxKind.ErrorKeyword:
			return ParseErrorOrWarningDirective(syntaxToken, EatContextualToken(contextualKind), isActive);
		case SyntaxKind.LineKeyword:
			return ParseLineDirective(syntaxToken, EatContextualToken(contextualKind), isActive);
		case SyntaxKind.PragmaKeyword:
			return ParsePragmaDirective(syntaxToken, EatContextualToken(contextualKind), isActive);
		case SyntaxKind.ReferenceKeyword:
			return ParseReferenceDirective(syntaxToken, EatContextualToken(contextualKind), isActive, isAfterFirstTokenInFile && !isAfterNonWhitespaceOnLine);
		case SyntaxKind.LoadKeyword:
			return ParseLoadDirective(syntaxToken, EatContextualToken(contextualKind), isActive, isAfterFirstTokenInFile && !isAfterNonWhitespaceOnLine);
		default:
		{
			if (lexer.Options.Kind == SourceCodeKind.Script && contextualKind == SyntaxKind.ExclamationToken && position == 0 && !syntaxToken.HasTrailingTrivia)
			{
				return ParseShebangDirective(syntaxToken, EatToken(SyntaxKind.ExclamationToken), isActive);
			}
			SyntaxToken syntaxToken2 = EatToken(SyntaxKind.IdentifierToken, reportError: false);
			SyntaxToken endOfDirectiveToken = ParseEndOfDirective(ignoreErrors: true);
			if (!isAfterNonWhitespaceOnLine)
			{
				if (!syntaxToken2.IsMissing)
				{
					syntaxToken2 = AddError(syntaxToken2, ErrorCode.ERR_PPDirectiveExpected);
				}
				else
				{
					syntaxToken = AddError(syntaxToken, ErrorCode.ERR_PPDirectiveExpected);
				}
			}
			return SyntaxFactory.BadDirectiveTrivia(syntaxToken, syntaxToken2, endOfDirectiveToken, isActive);
		}
		}
	}

	private DirectiveTriviaSyntax ParseIfDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
	{
		ExpressionSyntax expressionSyntax = ParseExpression();
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(ignoreErrors: false);
		bool flag = EvaluateBool(expressionSyntax);
		bool branchTaken = isActive & flag;
		return SyntaxFactory.IfDirectiveTrivia(hash, keyword, expressionSyntax, endOfDirectiveToken, isActive, branchTaken, flag);
	}

	private DirectiveTriviaSyntax ParseElifDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive, bool endIsActive)
	{
		ExpressionSyntax expressionSyntax = ParseExpression();
		SyntaxToken syntaxToken = ParseEndOfDirective(ignoreErrors: false);
		if (_context.HasPreviousIfOrElif())
		{
			bool flag = EvaluateBool(expressionSyntax);
			bool branchTaken = (endIsActive & flag) && !_context.PreviousBranchTaken();
			return SyntaxFactory.ElifDirectiveTrivia(hash, keyword, expressionSyntax, syntaxToken, endIsActive, branchTaken, flag);
		}
		syntaxToken = syntaxToken.TokenWithLeadingTrivia(SyntaxList.Concat(SyntaxFactory.DisabledText(expressionSyntax.ToFullString()), syntaxToken.GetLeadingTrivia()));
		if (_context.HasUnfinishedRegion())
		{
			return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, syntaxToken, isActive), ErrorCode.ERR_EndRegionDirectiveExpected);
		}
		if (_context.HasUnfinishedIf())
		{
			return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, syntaxToken, isActive), ErrorCode.ERR_EndifDirectiveExpected);
		}
		return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, syntaxToken, isActive), ErrorCode.ERR_UnexpectedDirective);
	}

	private DirectiveTriviaSyntax ParseElseDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive, bool endIsActive)
	{
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(ignoreErrors: false);
		if (_context.HasPreviousIfOrElif())
		{
			bool branchTaken = endIsActive && !_context.PreviousBranchTaken();
			return SyntaxFactory.ElseDirectiveTrivia(hash, keyword, endOfDirectiveToken, endIsActive, branchTaken);
		}
		if (_context.HasUnfinishedRegion())
		{
			return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, endOfDirectiveToken, isActive), ErrorCode.ERR_EndRegionDirectiveExpected);
		}
		if (_context.HasUnfinishedIf())
		{
			return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, endOfDirectiveToken, isActive), ErrorCode.ERR_EndifDirectiveExpected);
		}
		return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, endOfDirectiveToken, isActive), ErrorCode.ERR_UnexpectedDirective);
	}

	private DirectiveTriviaSyntax ParseEndIfDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive, bool endIsActive)
	{
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(ignoreErrors: false);
		if (_context.HasUnfinishedIf())
		{
			return SyntaxFactory.EndIfDirectiveTrivia(hash, keyword, endOfDirectiveToken, endIsActive);
		}
		if (_context.HasUnfinishedRegion())
		{
			return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, endOfDirectiveToken, isActive), ErrorCode.ERR_EndRegionDirectiveExpected);
		}
		return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, endOfDirectiveToken, isActive), ErrorCode.ERR_UnexpectedDirective);
	}

	private DirectiveTriviaSyntax ParseRegionDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
	{
		return SyntaxFactory.RegionDirectiveTrivia(hash, keyword, ParseEndOfDirectiveWithOptionalPreprocessingMessage(), isActive);
	}

	private DirectiveTriviaSyntax ParseEndRegionDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
	{
		SyntaxToken endOfDirectiveToken = ParseEndOfDirectiveWithOptionalPreprocessingMessage();
		if (_context.HasUnfinishedRegion())
		{
			return SyntaxFactory.EndRegionDirectiveTrivia(hash, keyword, endOfDirectiveToken, isActive);
		}
		if (_context.HasUnfinishedIf())
		{
			return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, endOfDirectiveToken, isActive), ErrorCode.ERR_EndifDirectiveExpected);
		}
		return AddError(SyntaxFactory.BadDirectiveTrivia(hash, keyword, endOfDirectiveToken, isActive), ErrorCode.ERR_UnexpectedDirective);
	}

	private DirectiveTriviaSyntax ParseDefineOrUndefDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive, bool isFollowingToken)
	{
		if (isFollowingToken)
		{
			keyword = AddError(keyword, ErrorCode.ERR_PPDefFollowsToken);
		}
		SyntaxToken identifier = EatToken(SyntaxKind.IdentifierToken, ErrorCode.ERR_IdentifierExpected);
		identifier = TruncateIdentifier(identifier);
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(identifier.IsMissing);
		if (keyword.Kind == SyntaxKind.DefineKeyword)
		{
			return SyntaxFactory.DefineDirectiveTrivia(hash, keyword, identifier, endOfDirectiveToken, isActive);
		}
		return SyntaxFactory.UndefDirectiveTrivia(hash, keyword, identifier, endOfDirectiveToken, isActive);
	}

	private DirectiveTriviaSyntax ParseErrorOrWarningDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive)
	{
		SyntaxToken syntaxToken = ParseEndOfDirectiveWithOptionalPreprocessingMessage();
		bool flag = keyword.Kind == SyntaxKind.ErrorKeyword;
		if (isActive)
		{
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			int num = 0;
			bool flag2 = true;
			foreach (CSharpSyntaxNode trailingTrivium in keyword.TrailingTrivia)
			{
				if (flag2)
				{
					if (trailingTrivium.Kind == SyntaxKind.WhitespaceTrivia)
					{
						continue;
					}
					flag2 = false;
				}
				trailingTrivium.WriteTo(stringWriter, leading: true, trailing: true);
				num += trailingTrivium.FullWidth;
			}
			foreach (CSharpSyntaxNode leadingTrivium in syntaxToken.LeadingTrivia)
			{
				leadingTrivium.WriteTo(stringWriter, leading: true, trailing: true);
				num += leadingTrivium.FullWidth;
			}
			int offset = syntaxToken.GetLeadingTriviaWidth() - num;
			string text = stringWriter.ToString();
			syntaxToken = AddError(syntaxToken, offset, num, flag ? ErrorCode.ERR_ErrorDirective : ErrorCode.WRN_WarningDirective, text);
			if (flag)
			{
				LanguageVersion result;
				if (text.Equals("version", StringComparison.Ordinal))
				{
					string assemblyFileVersion = CommonCompiler.GetAssemblyFileVersion(typeof(CSharpCompiler).GetTypeInfo().Assembly);
					syntaxToken = AddError(syntaxToken, offset, num, ErrorCode.ERR_CompilerAndLanguageVersion, assemblyFileVersion, Options.SpecifiedLanguageVersion.ToDisplayString());
				}
				else if (text.StartsWith("version:", StringComparison.Ordinal) && LanguageVersionFacts.TryParse(text.Substring("version:".Length), out result))
				{
					ErrorCode errorCode = Options.LanguageVersion.GetErrorCode();
					syntaxToken = AddError(syntaxToken, offset, num, errorCode, "version", new CSharpRequiredLanguageVersion(result));
				}
			}
		}
		if (flag)
		{
			return SyntaxFactory.ErrorDirectiveTrivia(hash, keyword, syntaxToken, isActive);
		}
		return SyntaxFactory.WarningDirectiveTrivia(hash, keyword, syntaxToken, isActive);
	}

	private DirectiveTriviaSyntax ParseLineDirective(SyntaxToken hash, SyntaxToken id, bool isActive)
	{
		SyntaxToken file = null;
		bool afterLineNumber = false;
		SyntaxKind kind = CurrentToken.Kind;
		SyntaxToken syntaxToken;
		if (kind == SyntaxKind.DefaultKeyword || kind == SyntaxKind.HiddenKeyword)
		{
			syntaxToken = EatToken();
		}
		else
		{
			syntaxToken = EatToken(SyntaxKind.NumericLiteralToken, ErrorCode.ERR_InvalidLineNumber, isActive);
			afterLineNumber = true;
			if (isActive && !syntaxToken.IsMissing && syntaxToken.Kind == SyntaxKind.NumericLiteralToken)
			{
				if ((int)syntaxToken.Value < 1)
				{
					syntaxToken = AddError(syntaxToken, ErrorCode.ERR_InvalidLineNumber);
				}
				else if ((int)syntaxToken.Value > 16707565)
				{
					syntaxToken = AddError(syntaxToken, ErrorCode.WRN_TooManyLinesForDebugger);
				}
			}
			if (CurrentToken.Kind == SyntaxKind.StringLiteralToken && (syntaxToken.IsMissing || syntaxToken.GetTrailingTriviaWidth() > 0 || CurrentToken.GetLeadingTriviaWidth() > 0))
			{
				file = EatToken();
				afterLineNumber = false;
			}
		}
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(syntaxToken.IsMissing || !isActive, afterPragma: false, afterLineNumber);
		return SyntaxFactory.LineDirectiveTrivia(hash, id, syntaxToken, file, endOfDirectiveToken, isActive);
	}

	private DirectiveTriviaSyntax ParseReferenceDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive, bool isFollowingToken)
	{
		if (isActive)
		{
			if (Options.Kind == SourceCodeKind.Regular)
			{
				keyword = AddError(keyword, ErrorCode.ERR_ReferenceDirectiveOnlyAllowedInScripts);
			}
			else if (isFollowingToken)
			{
				keyword = AddError(keyword, ErrorCode.ERR_PPReferenceFollowsToken);
			}
		}
		SyntaxToken syntaxToken = EatToken(SyntaxKind.StringLiteralToken, ErrorCode.ERR_ExpectedPPFile, isActive);
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(syntaxToken.IsMissing || !isActive);
		return SyntaxFactory.ReferenceDirectiveTrivia(hash, keyword, syntaxToken, endOfDirectiveToken, isActive);
	}

	private DirectiveTriviaSyntax ParseLoadDirective(SyntaxToken hash, SyntaxToken keyword, bool isActive, bool isFollowingToken)
	{
		if (isActive)
		{
			if (Options.Kind == SourceCodeKind.Regular)
			{
				keyword = AddError(keyword, ErrorCode.ERR_LoadDirectiveOnlyAllowedInScripts);
			}
			else if (isFollowingToken)
			{
				keyword = AddError(keyword, ErrorCode.ERR_PPLoadFollowsToken);
			}
		}
		SyntaxToken syntaxToken = EatToken(SyntaxKind.StringLiteralToken, ErrorCode.ERR_ExpectedPPFile, isActive);
		SyntaxToken endOfDirectiveToken = ParseEndOfDirective(syntaxToken.IsMissing || !isActive);
		return SyntaxFactory.LoadDirectiveTrivia(hash, keyword, syntaxToken, endOfDirectiveToken, isActive);
	}

	private DirectiveTriviaSyntax ParsePragmaDirective(SyntaxToken hash, SyntaxToken pragma, bool isActive)
	{
		pragma = CheckFeatureAvailability(pragma, MessageID.IDS_FeaturePragma);
		bool flag = false;
		if (CurrentToken.ContextualKind == SyntaxKind.WarningKeyword)
		{
			SyntaxToken warningKeyword = EatContextualToken(SyntaxKind.WarningKeyword);
			SyntaxToken disableOrRestoreKeyword;
			if (CurrentToken.Kind == SyntaxKind.DisableKeyword || CurrentToken.Kind == SyntaxKind.RestoreKeyword)
			{
				disableOrRestoreKeyword = EatToken();
				SeparatedSyntaxListBuilder<ExpressionSyntax> separatedSyntaxListBuilder = new SeparatedSyntaxListBuilder<ExpressionSyntax>(10);
				while (CurrentToken.Kind != SyntaxKind.EndOfDirectiveToken)
				{
					SyntaxToken syntaxToken;
					ExpressionSyntax node;
					if (CurrentToken.Kind == SyntaxKind.NumericLiteralToken)
					{
						syntaxToken = EatToken();
						node = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, syntaxToken);
					}
					else if (CurrentToken.Kind == SyntaxKind.IdentifierToken)
					{
						syntaxToken = EatToken();
						node = SyntaxFactory.IdentifierName(syntaxToken);
					}
					else
					{
						syntaxToken = EatToken(SyntaxKind.NumericLiteralToken, ErrorCode.WRN_IdentifierOrNumericLiteralExpected, isActive);
						node = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, syntaxToken);
					}
					flag = flag || syntaxToken.ContainsDiagnostics;
					separatedSyntaxListBuilder.Add(node);
					if (CurrentToken.Kind != SyntaxKind.CommaToken)
					{
						break;
					}
					separatedSyntaxListBuilder.AddSeparator(EatToken());
				}
				SyntaxToken endOfDirectiveToken = ParseEndOfDirective(flag || !isActive, afterPragma: true);
				return SyntaxFactory.PragmaWarningDirectiveTrivia(hash, pragma, warningKeyword, disableOrRestoreKeyword, separatedSyntaxListBuilder.ToList(), endOfDirectiveToken, isActive);
			}
			disableOrRestoreKeyword = EatToken(SyntaxKind.DisableKeyword, ErrorCode.WRN_IllegalPPWarning, isActive);
			SyntaxToken endOfDirectiveToken2 = ParseEndOfDirective(ignoreErrors: true, afterPragma: true);
			return SyntaxFactory.PragmaWarningDirectiveTrivia(hash, pragma, warningKeyword, disableOrRestoreKeyword, default, endOfDirectiveToken2, isActive);
		}
		if (CurrentToken.Kind == SyntaxKind.ChecksumKeyword)
		{
			SyntaxToken checksumKeyword = EatToken();
			SyntaxToken syntaxToken2 = EatToken(SyntaxKind.StringLiteralToken, ErrorCode.WRN_IllegalPPChecksum, isActive);
			SyntaxToken syntaxToken3 = EatToken(SyntaxKind.StringLiteralToken, ErrorCode.WRN_IllegalPPChecksum, isActive && !syntaxToken2.IsMissing);
			if (isActive && !syntaxToken3.IsMissing && !Guid.TryParse(syntaxToken3.ValueText, out var _))
			{
				syntaxToken3 = AddError(syntaxToken3, ErrorCode.WRN_IllegalPPChecksum);
			}
			SyntaxToken syntaxToken4 = EatToken(SyntaxKind.StringLiteralToken, ErrorCode.WRN_IllegalPPChecksum, isActive && !syntaxToken3.IsMissing);
			if (isActive && !syntaxToken4.IsMissing)
			{
				if (syntaxToken4.ValueText.Length % 2 != 0)
				{
					syntaxToken4 = AddError(syntaxToken4, ErrorCode.WRN_IllegalPPChecksum);
				}
				else
				{
					string valueText = syntaxToken4.ValueText;
					for (int i = 0; i < valueText.Length; i++)
					{
						if (!SyntaxFacts.IsHexDigit(valueText[i]))
						{
							syntaxToken4 = AddError(syntaxToken4, ErrorCode.WRN_IllegalPPChecksum);
							break;
						}
					}
				}
			}
			flag = syntaxToken2.ContainsDiagnostics | syntaxToken3.ContainsDiagnostics | syntaxToken4.ContainsDiagnostics;
			SyntaxToken endOfDirectiveToken3 = ParseEndOfDirective(flag, afterPragma: true);
			return SyntaxFactory.PragmaChecksumDirectiveTrivia(hash, pragma, checksumKeyword, syntaxToken2, syntaxToken3, syntaxToken4, endOfDirectiveToken3, isActive);
		}
		SyntaxToken warningKeyword2 = EatToken(SyntaxKind.WarningKeyword, ErrorCode.WRN_IllegalPragma, isActive);
		SyntaxToken disableOrRestoreKeyword2 = EatToken(SyntaxKind.DisableKeyword, reportError: false);
		SyntaxToken endOfDirectiveToken4 = ParseEndOfDirective(ignoreErrors: true, afterPragma: true);
		return SyntaxFactory.PragmaWarningDirectiveTrivia(hash, pragma, warningKeyword2, disableOrRestoreKeyword2, default, endOfDirectiveToken4, isActive);
	}

	private DirectiveTriviaSyntax ParseShebangDirective(SyntaxToken hash, SyntaxToken exclamation, bool isActive)
	{
		return SyntaxFactory.ShebangDirectiveTrivia(hash, exclamation, ParseEndOfDirectiveWithOptionalPreprocessingMessage(), isActive);
	}

	private SyntaxToken ParseEndOfDirectiveWithOptionalPreprocessingMessage()
	{
		StringBuilder stringBuilder = null;
		if (CurrentToken.Kind != SyntaxKind.EndOfDirectiveToken && CurrentToken.Kind != SyntaxKind.EndOfFileToken)
		{
			stringBuilder = new StringBuilder(CurrentToken.FullWidth);
			while (CurrentToken.Kind != SyntaxKind.EndOfDirectiveToken && CurrentToken.Kind != SyntaxKind.EndOfFileToken)
			{
				SyntaxToken syntaxToken = EatToken();
				stringBuilder.Append(syntaxToken.ToFullString());
			}
		}
		SyntaxToken syntaxToken2 = ((CurrentToken.Kind == SyntaxKind.EndOfDirectiveToken) ? EatToken() : SyntaxFactory.Token(SyntaxKind.EndOfDirectiveToken));
		if (stringBuilder != null)
		{
			syntaxToken2 = syntaxToken2.TokenWithLeadingTrivia(SyntaxFactory.PreprocessingMessage(stringBuilder.ToString()));
		}
		return syntaxToken2;
	}

	private SyntaxToken ParseEndOfDirective(bool ignoreErrors, bool afterPragma = false, bool afterLineNumber = false)
	{
		SyntaxListBuilder<SyntaxToken> syntaxListBuilder = default;
		if (CurrentToken.Kind != SyntaxKind.EndOfDirectiveToken && CurrentToken.Kind != SyntaxKind.EndOfFileToken)
		{
			syntaxListBuilder = new SyntaxListBuilder<SyntaxToken>(10);
			if (!ignoreErrors)
			{
				ErrorCode code = ErrorCode.ERR_EndOfPPLineExpected;
				if (afterPragma)
				{
					code = ErrorCode.WRN_EndOfPPLineExpected;
				}
				else if (afterLineNumber)
				{
					code = ErrorCode.ERR_MissingPPFile;
				}
				syntaxListBuilder.Add(AddError(EatToken().WithoutDiagnosticsGreen(), code));
			}
			while (CurrentToken.Kind != SyntaxKind.EndOfDirectiveToken && CurrentToken.Kind != SyntaxKind.EndOfFileToken)
			{
				syntaxListBuilder.Add(EatToken().WithoutDiagnosticsGreen());
			}
		}
		SyntaxToken syntaxToken = ((CurrentToken.Kind == SyntaxKind.EndOfDirectiveToken) ? EatToken() : SyntaxFactory.Token(SyntaxKind.EndOfDirectiveToken));
		if (!syntaxListBuilder.IsNull)
		{
			syntaxToken = syntaxToken.TokenWithLeadingTrivia(SyntaxFactory.SkippedTokensTrivia(syntaxListBuilder.ToList()));
		}
		return syntaxToken;
	}

	private ExpressionSyntax ParseExpression()
	{
		return ParseLogicalOr();
	}

	private ExpressionSyntax ParseLogicalOr()
	{
		ExpressionSyntax expressionSyntax = ParseLogicalAnd();
		while (CurrentToken.Kind == SyntaxKind.BarBarToken)
		{
			SyntaxToken operatorToken = EatToken();
			ExpressionSyntax right = ParseLogicalAnd();
			expressionSyntax = SyntaxFactory.BinaryExpression(SyntaxKind.LogicalOrExpression, expressionSyntax, operatorToken, right);
		}
		return expressionSyntax;
	}

	private ExpressionSyntax ParseLogicalAnd()
	{
		ExpressionSyntax expressionSyntax = ParseEquality();
		while (CurrentToken.Kind == SyntaxKind.AmpersandAmpersandToken)
		{
			SyntaxToken operatorToken = EatToken();
			ExpressionSyntax right = ParseEquality();
			expressionSyntax = SyntaxFactory.BinaryExpression(SyntaxKind.LogicalAndExpression, expressionSyntax, operatorToken, right);
		}
		return expressionSyntax;
	}

	private ExpressionSyntax ParseEquality()
	{
		ExpressionSyntax expressionSyntax = ParseLogicalNot();
		while (CurrentToken.Kind == SyntaxKind.EqualsEqualsToken || CurrentToken.Kind == SyntaxKind.ExclamationEqualsToken)
		{
			SyntaxToken syntaxToken = EatToken();
			ExpressionSyntax right = ParseEquality();
			expressionSyntax = SyntaxFactory.BinaryExpression(SyntaxFacts.GetBinaryExpression(syntaxToken.Kind), expressionSyntax, syntaxToken, right);
		}
		return expressionSyntax;
	}

	private ExpressionSyntax ParseLogicalNot()
	{
		if (CurrentToken.Kind == SyntaxKind.ExclamationToken)
		{
			SyntaxToken operatorToken = EatToken();
			return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, operatorToken, ParseLogicalNot());
		}
		return ParsePrimary();
	}

	private ExpressionSyntax ParsePrimary()
	{
		SyntaxKind kind = CurrentToken.Kind;
		switch (kind)
		{
		case SyntaxKind.OpenParenToken:
		{
			SyntaxToken openParenToken = EatToken();
			ExpressionSyntax expression = ParseExpression();
			SyntaxToken closeParenToken = EatToken(SyntaxKind.CloseParenToken);
			return SyntaxFactory.ParenthesizedExpression(openParenToken, expression, closeParenToken);
		}
		case SyntaxKind.IdentifierToken:
			return SyntaxFactory.IdentifierName(TruncateIdentifier(EatToken()));
		case SyntaxKind.TrueKeyword:
		case SyntaxKind.FalseKeyword:
			return SyntaxFactory.LiteralExpression(SyntaxFacts.GetLiteralExpression(kind), EatToken());
		default:
			return SyntaxFactory.IdentifierName(EatToken(SyntaxKind.IdentifierToken, ErrorCode.ERR_InvalidPreprocExpr));
		}
	}

	private static SyntaxToken TruncateIdentifier(SyntaxToken identifier)
	{
		if (identifier.Width > 128)
		{
			GreenNode leadingTrivia = identifier.GetLeadingTrivia();
			GreenNode trailingTrivia = identifier.GetTrailingTrivia();
			string text = identifier.ToString();
			string valueText = text.Substring(0, 128);
			identifier = SyntaxFactory.Identifier(SyntaxKind.IdentifierToken, leadingTrivia, text, valueText, trailingTrivia);
		}
		return identifier;
	}

	private bool EvaluateBool(ExpressionSyntax expr)
	{
		object obj = Evaluate(expr);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return false;
	}

	private object Evaluate(ExpressionSyntax expr)
	{
		switch (expr.Kind)
		{
		case SyntaxKind.ParenthesizedExpression:
			return Evaluate(((ParenthesizedExpressionSyntax)expr).Expression);
		case SyntaxKind.TrueLiteralExpression:
		case SyntaxKind.FalseLiteralExpression:
			return ((LiteralExpressionSyntax)expr).Token.Value;
		case SyntaxKind.LogicalAndExpression:
		case SyntaxKind.BitwiseAndExpression:
			return EvaluateBool(((BinaryExpressionSyntax)expr).Left) && EvaluateBool(((BinaryExpressionSyntax)expr).Right);
		case SyntaxKind.LogicalOrExpression:
		case SyntaxKind.BitwiseOrExpression:
			return EvaluateBool(((BinaryExpressionSyntax)expr).Left) || EvaluateBool(((BinaryExpressionSyntax)expr).Right);
		case SyntaxKind.EqualsExpression:
			return object.Equals(Evaluate(((BinaryExpressionSyntax)expr).Left), Evaluate(((BinaryExpressionSyntax)expr).Right));
		case SyntaxKind.NotEqualsExpression:
			return !object.Equals(Evaluate(((BinaryExpressionSyntax)expr).Left), Evaluate(((BinaryExpressionSyntax)expr).Right));
		case SyntaxKind.LogicalNotExpression:
			return !EvaluateBool(((PrefixUnaryExpressionSyntax)expr).Operand);
		case SyntaxKind.IdentifierName:
		{
			string valueText = ((IdentifierNameSyntax)expr).Identifier.ValueText;
			if (bool.TryParse(valueText, out var result))
			{
				return result;
			}
			return IsDefined(valueText);
		}
		default:
			return false;
		}
	}

	private bool IsDefined(string id)
	{
		return _context.IsDefined(id) switch
		{
			DefineState.Defined => true, 
			DefineState.Undefined => false, 
			_ => Options.PreprocessorSymbols.Contains(id), 
		};
	}
}
