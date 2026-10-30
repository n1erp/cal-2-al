using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

internal class LanguageParser : ParserBase
{
	protected delegate PostSkipAction SkipBadSeparatedListDelegate<T>(ref InternalSyntaxToken startToken, InternalSeparatedSyntaxListBuilder<T> list, SyntaxKind expected) where T : InternalSyntaxNode;

	protected InternalInt32SignedLiteralValueSyntax ParseInt32LiteralValue(InternalSyntaxToken signToken = null)
	{
		signToken = ParseMinusSignIfNotParsed(signToken);
		return ContextAwareSyntaxFactory.Int32SignedLiteralValue(signToken, EatToken(SyntaxKind.Int32LiteralToken));
	}

	protected InternalInt64SignedLiteralValueSyntax ParseInt64LiteralValue(InternalSyntaxToken signToken = null)
	{
		signToken = ParseMinusSignIfNotParsed(signToken);
		return ContextAwareSyntaxFactory.Int64SignedLiteralValue(signToken, EatToken(SyntaxKind.Int64LiteralToken));
	}

	protected InternalDecimalSignedLiteralValueSyntax ParseDecimalLiteralValue(InternalSyntaxToken signToken = null)
	{
		signToken = ParseMinusSignIfNotParsed(signToken);
		return ContextAwareSyntaxFactory.DecimalSignedLiteralValue(signToken, EatToken(SyntaxKind.DecimalLiteralToken));
	}

	protected InternalStringLiteralValueSyntax ParseStringLiteralValue()
	{
		return ContextAwareSyntaxFactory.StringLiteralValue(EatToken(SyntaxKind.StringLiteralToken));
	}

	protected InternalBooleanLiteralValueSyntax ParseBooleanLiteralValue()
	{
		SyntaxKind kind = CurrentToken.Kind;
		if ((uint)(kind - 2) <= 1u)
		{
			return ContextAwareSyntaxFactory.BooleanLiteralValue(EatToken());
		}
		return ContextAwareSyntaxFactory.BooleanLiteralValue(EatToken(SyntaxKind.TrueKeyword, ErrorCode.ERR_ExpectedBooleanLiteral));
	}

	protected InternalDateLiteralValueSyntax ParseDateLiteralValue()
	{
		return ContextAwareSyntaxFactory.DateLiteralValue(EatToken(SyntaxKind.DateLiteralToken));
	}

	protected InternalTimeLiteralValueSyntax ParseTimeLiteralValue()
	{
		return ContextAwareSyntaxFactory.TimeLiteralValue(EatToken(SyntaxKind.TimeLiteralToken));
	}

	protected InternalDateTimeLiteralValueSyntax ParseDateTimeLiteralValue()
	{
		return ContextAwareSyntaxFactory.DateTimeLiteralValue(EatToken(SyntaxKind.DateTimeLiteralToken));
	}

	protected InternalLiteralValueSyntax ParseNumericLiteralValue()
	{
		InternalSyntaxToken internalSyntaxToken = ((CurrentToken.Kind == SyntaxKind.MinusToken) ? EatToken() : null);
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.Int32LiteralToken:
			return ParseInt32LiteralValue(internalSyntaxToken);
		case SyntaxKind.Int64LiteralToken:
			return ParseInt64LiteralValue(internalSyntaxToken);
		case SyntaxKind.DecimalLiteralToken:
			return ParseDecimalLiteralValue(internalSyntaxToken);
		default:
		{
			DebugAssertHelper.Assert(internalSyntaxToken != null, "This switch should only be reached if we have a minus sign and no number after it: " + CurrentToken.Kind);
			InternalSyntaxToken number = EatToken(SyntaxKind.Int32LiteralToken, ErrorCode.ERR_ExpectedNumericLiteral);
			return ContextAwareSyntaxFactory.Int32SignedLiteralValue(internalSyntaxToken, number);
		}
		}
	}

	protected InternalLiteralValueSyntax ParseLiteralValue()
	{
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.Int32LiteralToken:
		case SyntaxKind.Int64LiteralToken:
		case SyntaxKind.DecimalLiteralToken:
		case SyntaxKind.MinusToken:
			return ParseNumericLiteralValue();
		case SyntaxKind.StringLiteralToken:
			return ParseStringLiteralValue();
		case SyntaxKind.TimeLiteralToken:
			return ParseTimeLiteralValue();
		case SyntaxKind.DateLiteralToken:
			return ParseDateLiteralValue();
		case SyntaxKind.DateTimeLiteralToken:
			return ParseDateTimeLiteralValue();
		case SyntaxKind.FalseKeyword:
		case SyntaxKind.TrueKeyword:
			return ParseBooleanLiteralValue();
		default:
		{
			InternalSyntaxToken value = EatToken(SyntaxKind.StringLiteralToken, ErrorCode.ERR_ExpectedLiteral);
			return ContextAwareSyntaxFactory.StringLiteralValue(value);
		}
		}
	}

	private InternalSyntaxToken ParseMinusSignIfNotParsed(InternalSyntaxToken signToken)
	{
		if (signToken == null)
		{
			signToken = ((CurrentToken.Kind != SyntaxKind.MinusToken) ? null : EatToken());
		}
		return signToken;
	}

	public LanguageParser(Lexer lexer, LexerMode mode, SyntaxNode oldTree, IEnumerable<TextChangeRange> changes, CancellationToken cancellationToken = default(CancellationToken))
		: base(lexer, mode, oldTree, changes, allowModeReset: false, preLexIfNotIncremental: false, cancellationToken)
	{
	}

	protected InternalSeparatedSyntaxList<T> ParseSeparatedList<T>(ref InternalSyntaxToken startToken, SyntaxKind listRecoveryKind, Func<InternalSyntaxToken, bool> isListMember, SyntaxKind listSeparator, SyntaxKind closeTokenKind, SkipBadSeparatedListDelegate<T> skipBadTokens, Func<T> parseListMember, bool shouldParseMemberIfLastElementIsSeparator = false) where T : InternalSyntaxNode
	{
		InternalSeparatedSyntaxListBuilder<T> internalSeparatedSyntaxListBuilder = Pool.AllocateSeparated<T>();
		try
		{
			bool flag = true;
			while (CurrentToken.Kind != closeTokenKind && !IsTerminator())
			{
				if (flag)
				{
					if (isListMember(CurrentToken))
					{
						flag = false;
						internalSeparatedSyntaxListBuilder.Add(parseListMember());
					}
					else if (skipBadTokens(ref startToken, internalSeparatedSyntaxListBuilder, listRecoveryKind) == PostSkipAction.Abort)
					{
						break;
					}
				}
				else if (CurrentToken.Kind == listSeparator)
				{
					flag = true;
					internalSeparatedSyntaxListBuilder.AddSeparator(EatToken());
				}
				else if (skipBadTokens(ref startToken, internalSeparatedSyntaxListBuilder, listSeparator) == PostSkipAction.Abort)
				{
					break;
				}
			}
			if (flag && internalSeparatedSyntaxListBuilder.Count > 0)
			{
				if (shouldParseMemberIfLastElementIsSeparator)
				{
					internalSeparatedSyntaxListBuilder.Add(parseListMember());
				}
				else
				{
					InternalSyntaxNode node = internalSeparatedSyntaxListBuilder[internalSeparatedSyntaxListBuilder.Count - 1];
					internalSeparatedSyntaxListBuilder[internalSeparatedSyntaxListBuilder.Count - 1] = AddError(node, ErrorCode.ERR_ListCannotEndWithSeparator, listSeparator.GetText());
				}
			}
			return internalSeparatedSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSeparatedSyntaxListBuilder);
		}
	}

	protected InternalSeparatedSyntaxList<InternalIdentifierNameSyntax> ParseCommaSeparatedIdentifierNames(ref InternalSyntaxToken startToken, SyntaxKind closeTokenKind = SyntaxKind.SemicolonToken)
	{
		return ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => token.Kind.IsTokenIdentifier(), SyntaxKind.CommaToken, closeTokenKind, SkipBadCommaSeparatedToken, ParseIdentifierName);
	}

	protected PostSkipAction SkipBadCommaSeparatedToken<T>(ref InternalSyntaxToken startToken, InternalSeparatedSyntaxListBuilder<T> list, SyntaxKind expected) where T : InternalSyntaxNode
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || (expected.IsTokenIdentifier() && !p.CurrentToken.Kind.IsTokenIdentifier()), (ParserBase p) => p.CurrentToken.IsKind(SyntaxKind.SemicolonToken) || p.CurrentToken.IsKind(SyntaxKind.CloseParenToken) || p.CurrentToken.IsKind(SyntaxKind.BeginKeyword) || p.CurrentToken.IsKind(SyntaxKind.VarKeyword) || p.CurrentToken.IsKind(SyntaxKind.CloseBraceToken) || p.IsTerminator(), expected);
	}

	protected PostSkipAction SkipBadCommaSeparatedStringLiteralToken<T>(ref InternalSyntaxToken startToken, InternalSeparatedSyntaxListBuilder<T> list, SyntaxKind expected) where T : InternalSyntaxNode
	{
		return SkipBadSeparatedListTokensWithExpectedKind(ref startToken, list, (ParserBase p) => (expected == SyntaxKind.CommaToken && p.CurrentToken.Kind != SyntaxKind.CommaToken) || expected == SyntaxKind.StringLiteralToken, (ParserBase p) => p.CurrentToken.IsKind(SyntaxKind.SemicolonToken) || p.IsTerminator(), expected);
	}

	protected internal InternalIdentifierNameOrEmptySyntax ParseIdentifierNameOrEmptySyntax()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.IdentifierNameOrEmpty)
		{
			return (InternalIdentifierNameOrEmptySyntax)EatNode();
		}
		InternalSyntaxToken internalSyntaxToken = ParseIdentifierToken(allowMissingIdentifier: true);
		if (internalSyntaxToken == null)
		{
			InternalSyntaxToken empty = InternalSyntaxFactory.Token(SyntaxKind.EmptyToken);
			return ContextAwareSyntaxFactory.IdentifierNameOrEmpty(null, empty);
		}
		InternalIdentifierNameSyntax identifierName = ContextAwareSyntaxFactory.IdentifierName(internalSyntaxToken);
		return ContextAwareSyntaxFactory.IdentifierNameOrEmpty(identifierName, null);
	}

	protected internal InternalIdentifierNameSyntax ParseIdentifierName()
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.IdentifierName)
		{
			return (InternalIdentifierNameSyntax)EatNode();
		}
		InternalSyntaxToken identifier = ParseIdentifierToken();
		return ContextAwareSyntaxFactory.IdentifierName(identifier);
	}

	internal InternalStatementSyntax ParseStatement()
	{
		if (IsIncremental && CurrentNode is StatementSyntax)
		{
			return (InternalStatementSyntax)EatNode();
		}
		return CurrentToken.Kind switch
		{
			SyntaxKind.BeginKeyword => ParseBlock(), 
			SyntaxKind.IfKeyword => ParseIf(), 
			SyntaxKind.ElseKeyword => ParseOrphanedElse(), 
			SyntaxKind.CaseKeyword => ParseCase(), 
			SyntaxKind.WhileKeyword => ParseWhile(), 
			SyntaxKind.WithKeyword => ParseWith(), 
			SyntaxKind.ForKeyword => ParseFor(), 
			SyntaxKind.ForEachKeyword => ParseForeach(), 
			SyntaxKind.RepeatKeyword => ParseRepeat(), 
			SyntaxKind.ExitKeyword => ParseExit(), 
			SyntaxKind.SemicolonToken => ContextAwareSyntaxFactory.EmptyStatement(EatToken(SyntaxKind.SemicolonToken)), 
			SyntaxKind.AssertErrorKeyword => ParseAssertError(), 
			SyntaxKind.BreakKeyword => ParseBreakStatement(), 
			_ => ParseAssign(), 
		};
	}

	private InternalStatementSyntax ParseBreakStatement()
	{
		return ContextAwareSyntaxFactory.BreakStatement(EatToken(SyntaxKind.BreakKeyword), SemicolonOrNothing());
	}

	private InternalSyntaxList<InternalStatementSyntax> ParseStatementList(ref InternalSyntaxToken previousNode, SyntaxKind toToken)
	{
		InternalSyntaxListBuilder<InternalStatementSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalStatementSyntax>();
		try
		{
			bool flag = true;
			while (CurrentToken.Kind != toToken && CurrentToken.Kind != SyntaxKind.EndOfFileToken)
			{
				if (CurrentToken.Kind.IsPossibleStatement())
				{
					InternalStatementSyntax internalStatementSyntax = ParseStatement();
					if (!flag)
					{
						internalStatementSyntax = AddErrorToFirstToken(internalStatementSyntax, ErrorCode.ERR_SemicolonExpected);
					}
					flag = internalStatementSyntax.HasSemicolon;
					internalSyntaxListBuilder.Add(internalStatementSyntax);
					continue;
				}
				PostSkipAction postSkipAction = SkipBadStatementListTokens(internalSyntaxListBuilder, SyntaxKind.EndKeyword, out var trailingTrivia);
				if (trailingTrivia != null)
				{
					previousNode = AddTrailingSkippedSyntax(previousNode, trailingTrivia);
				}
				if (postSkipAction == PostSkipAction.Abort)
				{
					break;
				}
			}
			return internalSyntaxListBuilder.ToList();
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private PostSkipAction SkipBadStatementListTokens(InternalSyntaxListBuilder<InternalStatementSyntax> statements, SyntaxKind expected, out InternalSyntaxNode trailingTrivia)
	{
		return SkipBadListTokensWithExpectedKindHelper(statements, (ParserBase p) => !p.CurrentToken.Kind.IsPossibleStatement(), (ParserBase p) => p.CurrentToken.Kind == SyntaxKind.EndKeyword || p.CurrentToken.Kind == SyntaxKind.CloseBraceToken || p.CurrentToken.Kind == SyntaxKind.VarKeyword || p.CurrentToken.Kind.IsPossibleMember() || p.IsTerminator(), expected, out trailingTrivia);
	}

	private InternalStatementSyntax ParseAssign()
	{
		InternalCodeExpressionSyntax internalCodeExpressionSyntax = ParseVar();
		switch (CurrentToken.Kind)
		{
		case SyntaxKind.AssignToken:
		{
			InternalSyntaxToken assignmentToken2 = CurrentToken;
			EatToken();
			InternalCodeExpressionSyntax source2 = ParseValue();
			return ContextAwareSyntaxFactory.AssignmentStatement(internalCodeExpressionSyntax, assignmentToken2, source2, SemicolonOrNothing());
		}
		case SyntaxKind.AssignRDivToken:
		case SyntaxKind.AssignPlusToken:
		case SyntaxKind.AssignMinusToken:
		case SyntaxKind.AssignMultiplyToken:
		{
			InternalSyntaxToken assignmentToken = CurrentToken;
			EatToken();
			InternalCodeExpressionSyntax source = ParseValue();
			return ContextAwareSyntaxFactory.CompoundAssignmentStatement(internalCodeExpressionSyntax, assignmentToken, source, SemicolonOrNothing());
		}
		default:
			return ContextAwareSyntaxFactory.ExpressionStatement(internalCodeExpressionSyntax, SemicolonOrNothing());
		}
	}

	private InternalIfStatementSyntax ParseIf()
	{
		InternalSyntaxToken ifKeywordToken = CurrentToken;
		InternalSyntaxToken elseKeywordToken = null;
		EatToken();
		InternalCodeExpressionSyntax condition = ParseValue();
		InternalSyntaxToken thenKeywordToken = EatToken(SyntaxKind.ThenKeyword);
		InternalStatementSyntax elseStatement = null;
		InternalStatementSyntax internalStatementSyntax;
		if (CurrentToken.Kind != SyntaxKind.EndOfFileToken)
		{
			internalStatementSyntax = ParseStatement();
			if (CurrentToken.Kind == SyntaxKind.ElseKeyword && !internalStatementSyntax.HasSemicolon)
			{
				elseKeywordToken = CurrentToken;
				EatToken();
				elseStatement = ParseStatement();
			}
		}
		else
		{
			internalStatementSyntax = ContextAwareSyntaxFactory.EmptyStatement(ParserBase.MissingToken(SyntaxKind.SemicolonToken));
		}
		return ContextAwareSyntaxFactory.IfStatement(ifKeywordToken, condition, thenKeywordToken, internalStatementSyntax, elseKeywordToken, elseStatement);
	}

	private InternalOrphanedElseStatementSyntax ParseOrphanedElse()
	{
		InternalSyntaxToken node = EatToken(SyntaxKind.ElseKeyword);
		node = AddError(node, ErrorCode.ERR_OrphanedElseStatement);
		InternalStatementSyntax elseStatement = ParseStatement();
		return ContextAwareSyntaxFactory.OrphanedElseStatement(node, elseStatement);
	}

	private InternalCaseStatementSyntax ParseCase()
	{
		InternalSyntaxToken caseKeywordToken = EatToken();
		InternalCodeExpressionSyntax expression = ParseValue();
		InternalSyntaxToken ofKeywordToken = EatToken(SyntaxKind.OfKeyword);
		InternalSyntaxListBuilder<InternalCaseLineSyntax> internalSyntaxListBuilder = Pool.Allocate<InternalCaseLineSyntax>();
		try
		{
			bool flag = true;
			while (!IsEndOfCaseLines())
			{
				InternalSeparatedSyntaxListBuilder<InternalCodeExpressionSyntax> item = Pool.AllocateSeparated<InternalCodeExpressionSyntax>();
				try
				{
					while (true)
					{
						InternalCodeExpressionSyntax internalCodeExpressionSyntax = ParseValue();
						if (!flag)
						{
							internalCodeExpressionSyntax = AddErrorToFirstToken(internalCodeExpressionSyntax, ErrorCode.ERR_SemicolonExpected);
						}
						if (CurrentToken.Kind == SyntaxKind.DotDotToken)
						{
							InternalSyntaxToken internalSyntaxToken = CurrentToken;
							EatToken();
							InternalCodeExpressionSyntax right = ParseValue();
							internalCodeExpressionSyntax = ContextAwareSyntaxFactory.BinaryExpression(SyntaxFacts.GetBinaryExpression(internalSyntaxToken.Kind), internalCodeExpressionSyntax, internalSyntaxToken, right);
						}
						item.Add(internalCodeExpressionSyntax);
						if (CurrentToken.Kind != SyntaxKind.CommaToken)
						{
							break;
						}
						item.AddSeparator(EatToken());
					}
					InternalSyntaxToken colonToken = EatToken(SyntaxKind.ColonToken);
					InternalStatementSyntax internalStatementSyntax = ParseStatement();
					flag = internalStatementSyntax.HasSemicolon;
					internalSyntaxListBuilder.Add(ContextAwareSyntaxFactory.CaseLine(item.ToList(), colonToken, internalStatementSyntax));
				}
				finally
				{
					Pool.Free(item);
				}
			}
			InternalCaseElseSyntax caseElse = null;
			if (CurrentToken.Kind == SyntaxKind.ElseKeyword)
			{
				InternalSyntaxToken previousNode = EatToken();
				InternalSyntaxList<InternalStatementSyntax> elseStatements = ParseStatementList(ref previousNode, SyntaxKind.EndKeyword);
				caseElse = ContextAwareSyntaxFactory.CaseElse(previousNode, elseStatements);
			}
			InternalSyntaxToken endKeywordToken = EatToken(SyntaxKind.EndKeyword);
			return ContextAwareSyntaxFactory.CaseStatement(caseKeywordToken, expression, ofKeywordToken, internalSyntaxListBuilder, caseElse, endKeywordToken, SemicolonOrNothing());
		}
		finally
		{
			Pool.Free(internalSyntaxListBuilder);
		}
	}

	private InternalRepeatStatementSyntax ParseRepeat()
	{
		InternalSyntaxToken previousNode = EatToken(SyntaxKind.RepeatKeyword);
		InternalSyntaxList<InternalStatementSyntax> statements = ParseStatementList(ref previousNode, SyntaxKind.UntilKeyword);
		return ContextAwareSyntaxFactory.RepeatStatement(previousNode, statements, EatToken(SyntaxKind.UntilKeyword), ParseValue(), SemicolonOrNothing());
	}

	private InternalWhileStatementSyntax ParseWhile()
	{
		return ContextAwareSyntaxFactory.WhileStatement(EatToken(), ParseValue(), EatToken(SyntaxKind.DoKeyword), ParseStatement());
	}

	private InternalForStatementSyntax ParseFor()
	{
		InternalSyntaxToken forKeywordToken = CurrentToken;
		EatToken();
		InternalCodeExpressionSyntax loopVariable = ParseVar();
		InternalSyntaxToken assignToken = EatToken(SyntaxKind.AssignToken);
		InternalCodeExpressionSyntax initialValue = ParseValue();
		InternalSyntaxToken operatorKeywordToken = CurrentToken;
		if (CurrentToken.Kind != SyntaxKind.ToKeyword && CurrentToken.Kind != SyntaxKind.DownToKeyword)
		{
			operatorKeywordToken = EatToken(SyntaxKind.ToKeyword, ErrorCode.ERR_ForStatementToOrDownToExpected);
		}
		else
		{
			EatToken();
		}
		InternalCodeExpressionSyntax endValue = ParseValue();
		InternalSyntaxToken doKeywordToken = EatToken(SyntaxKind.DoKeyword);
		InternalStatementSyntax statement = ParseStatement();
		return ContextAwareSyntaxFactory.ForStatement(forKeywordToken, loopVariable, assignToken, initialValue, operatorKeywordToken, endValue, doKeywordToken, statement);
	}

	private InternalForEachStatementSyntax ParseForeach()
	{
		return ContextAwareSyntaxFactory.ForEachStatement(EatToken(SyntaxKind.ForEachKeyword), ParseIdentifierName(), EatToken(SyntaxKind.InKeyword), ParseValue(), EatToken(SyntaxKind.DoKeyword), ParseStatement());
	}

	private InternalExitStatementSyntax ParseExit()
	{
		InternalSyntaxToken exitKeywordToken = EatToken(SyntaxKind.ExitKeyword);
		if (CurrentToken.Kind == SyntaxKind.OpenParenToken)
		{
			InternalSyntaxToken openParenthesisToken = EatToken();
			InternalCodeExpressionSyntax exitValue = ParseValue();
			InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
			return ContextAwareSyntaxFactory.ExitStatement(exitKeywordToken, openParenthesisToken, exitValue, closeParenthesisToken, SemicolonOrNothing());
		}
		return ContextAwareSyntaxFactory.ExitStatement(exitKeywordToken, null, null, null, SemicolonOrNothing());
	}

	internal InternalBlockSyntax ParseBlock(bool semicolonRequired = false)
	{
		if (IsIncremental && CurrentNodeKind == SyntaxKind.Block)
		{
			return (InternalBlockSyntax)EatNode();
		}
		InternalSyntaxToken previousNode = EatToken(SyntaxKind.BeginKeyword);
		InternalSyntaxList<InternalStatementSyntax> statements = ParseStatementList(ref previousNode, SyntaxKind.EndKeyword);
		InternalSyntaxToken endKeywordToken = EatToken(SyntaxKind.EndKeyword);
		return ContextAwareSyntaxFactory.Block(previousNode, statements, endKeywordToken, semicolonRequired ? EatToken(SyntaxKind.SemicolonToken) : SemicolonOrNothing());
	}

	private InternalWithStatementSyntax ParseWith()
	{
		InternalSyntaxToken withKeywordToken = CurrentToken;
		EatToken();
		InternalCodeExpressionSyntax withId = ParseVar();
		InternalSyntaxToken doKeywordToken = EatToken(SyntaxKind.DoKeyword);
		InternalStatementSyntax statement = ParseStatement();
		return ContextAwareSyntaxFactory.WithStatement(withKeywordToken, withId, doKeywordToken, statement);
	}

	private InternalStatementSyntax ParseAssertError()
	{
		return ContextAwareSyntaxFactory.AssertErrorStatement(EatToken(), ParseStatement());
	}

	internal InternalArgumentListSyntax ParseParenthesizedArgumentList()
	{
		InternalSyntaxToken startToken = EatToken(SyntaxKind.OpenParenToken);
		InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> arguments = ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => true, SyntaxKind.CommaToken, SyntaxKind.CloseParenToken, SkipBadCommaSeparatedToken, ParseValue);
		InternalSyntaxToken closeParenthesisToken = EatToken(SyntaxKind.CloseParenToken);
		return ContextAwareSyntaxFactory.ArgumentList(startToken, arguments, closeParenthesisToken);
	}

	internal InternalBracketedArgumentListSyntax ParseBracketedArgumentList()
	{
		InternalSyntaxToken startToken = EatToken(SyntaxKind.OpenBracketToken);
		InternalSeparatedSyntaxList<InternalCodeExpressionSyntax> arguments = ParseSeparatedList(ref startToken, SyntaxKind.IdentifierToken, (InternalSyntaxToken token) => true, SyntaxKind.CommaToken, SyntaxKind.CloseBracketToken, SkipBadCommaSeparatedToken, ParseValue);
		InternalSyntaxToken internalSyntaxToken = EatToken(SyntaxKind.CloseBracketToken);
		if (arguments.Count == 0)
		{
			internalSyntaxToken = AddError(internalSyntaxToken, ErrorCode.ERR_IndexersMustHaveAtLeastOneValue);
		}
		return ContextAwareSyntaxFactory.BracketedArgumentList(startToken, arguments, internalSyntaxToken);
	}

	private InternalCodeExpressionSyntax ParseValue()
	{
		return ParseExpression();
	}

	internal InternalCodeExpressionSyntax ParseExpression()
	{
		InternalCodeExpressionSyntax internalCodeExpressionSyntax = ParseSum();
		while (SyntaxFacts.IsBinaryComparisonExpressionOperatorToken(CurrentToken.Kind))
		{
			if (CurrentToken.Kind == SyntaxKind.InKeyword)
			{
				internalCodeExpressionSyntax = ParseInListExpression(internalCodeExpressionSyntax);
				continue;
			}
			InternalSyntaxToken internalSyntaxToken = EatToken();
			internalCodeExpressionSyntax = ContextAwareSyntaxFactory.BinaryExpression(SyntaxFacts.GetBinaryExpression(internalSyntaxToken.Kind), internalCodeExpressionSyntax, internalSyntaxToken, ParseSum());
		}
		return internalCodeExpressionSyntax;
	}

	private InternalCodeExpressionSyntax ParseSum()
	{
		InternalCodeExpressionSyntax internalCodeExpressionSyntax = ParseProd();
		while (CurrentToken.Kind == SyntaxKind.PlusToken || CurrentToken.Kind == SyntaxKind.MinusToken || CurrentToken.Kind == SyntaxKind.OrKeyword || CurrentToken.Kind == SyntaxKind.XorKeyword)
		{
			InternalSyntaxToken internalSyntaxToken = CurrentToken;
			EatToken();
			InternalCodeExpressionSyntax right = ParseProd();
			internalCodeExpressionSyntax = ContextAwareSyntaxFactory.BinaryExpression(SyntaxFacts.GetBinaryExpression(internalSyntaxToken.Kind), internalCodeExpressionSyntax, internalSyntaxToken, right);
		}
		return internalCodeExpressionSyntax;
	}

	private InternalCodeExpressionSyntax ParseProd()
	{
		InternalCodeExpressionSyntax internalCodeExpressionSyntax = ParseFactor();
		while (CurrentToken.Kind == SyntaxKind.MultiplyToken || CurrentToken.Kind == SyntaxKind.IDivToken || CurrentToken.Kind == SyntaxKind.RDivToken || CurrentToken.Kind == SyntaxKind.ModuloToken || CurrentToken.Kind == SyntaxKind.AndKeyword)
		{
			InternalSyntaxToken internalSyntaxToken = EatToken();
			internalCodeExpressionSyntax = ContextAwareSyntaxFactory.BinaryExpression(SyntaxFacts.GetBinaryExpression(internalSyntaxToken.Kind), internalCodeExpressionSyntax, internalSyntaxToken, ParseFactor());
		}
		return internalCodeExpressionSyntax;
	}

	private InternalCodeExpressionSyntax ParseFactor()
	{
		SyntaxKind kind = CurrentToken.Kind;
		switch ((int)kind)
		{
		case 14:
		case 15:
		case 46:
			return ContextAwareSyntaxFactory.UnaryExpression(SyntaxFacts.GetUnaryExpression(CurrentToken.Kind), EatToken(), ParseFactor());
		case 11:
			return ParseVar();
		case 37:
			return ContextAwareSyntaxFactory.ParenthesizedExpression(EatToken(SyntaxKind.OpenParenToken), ParseExpression(), EatToken(SyntaxKind.CloseParenToken));
		default:
			if (kind.IsSignedOrSignInvariantLiteralToken())
			{
				return ContextAwareSyntaxFactory.LiteralExpression(ParseLiteralValue());
			}
			return AddTrailingSkippedSyntax(ParserBase.CreateMissingIdentifierName(), AddError(EatToken(), ErrorCode.ERR_ExpressionExpected));
		}
	}

	protected InternalCodeExpressionSyntax ParseVar()
	{
		if (!CurrentToken.Kind.IsTokenIdentifier())
		{
			return AddTrailingSkippedSyntax(ParserBase.CreateMissingIdentifierName(), AddError(EatToken(), ErrorCode.ERR_IdentifierExpected));
		}
		InternalCodeExpressionSyntax internalCodeExpressionSyntax = ParseInvocationExpression(ParseIdentifierName());
		while (true)
		{
			switch (CurrentToken.Kind)
			{
			case SyntaxKind.DotToken:
			{
				InternalSyntaxToken dotToken = EatToken(SyntaxKind.DotToken);
				InternalIdentifierNameSyntax name2 = (CurrentToken.Kind.IsTokenIdentifier() ? ParseIdentifierName() : AddTrailingSkippedSyntax(ParserBase.CreateMissingIdentifierName(), AddError(EatToken(), ErrorCode.ERR_IdentifierExpected)));
				internalCodeExpressionSyntax = ContextAwareSyntaxFactory.MemberAccessExpression(internalCodeExpressionSyntax, dotToken, name2);
				internalCodeExpressionSyntax = ParseInvocationExpression(internalCodeExpressionSyntax);
				break;
			}
			case SyntaxKind.OpenBracketToken:
				internalCodeExpressionSyntax = ContextAwareSyntaxFactory.ElementAccessExpression(internalCodeExpressionSyntax, ParseBracketedArgumentList());
				break;
			case SyntaxKind.ColonColonToken:
			{
				InternalSyntaxToken colonColonToken = EatToken();
				InternalIdentifierNameSyntax name = (CurrentToken.Kind.IsTokenIdentifier() ? ParseIdentifierName() : AddError(ParserBase.CreateMissingIdentifierName(), ErrorCode.ERR_IdentifierExpected));
				return ContextAwareSyntaxFactory.OptionAccessExpression(internalCodeExpressionSyntax, colonColonToken, name);
			}
			case SyntaxKind.OpenParenToken:
			{
				InternalSyntaxToken internalSyntaxToken = EatToken();
				internalSyntaxToken = AddError(internalSyntaxToken, ErrorCode.ERR_UnexpectedToken, internalSyntaxToken.Text);
				return AddTrailingSkippedSyntax(internalCodeExpressionSyntax, internalSyntaxToken);
			}
			default:
				return internalCodeExpressionSyntax;
			}
		}
	}

	protected InternalNameSyntax ParseQualifiedName()
	{
		InternalNameSyntax internalNameSyntax = ParseIdentifierName();
		while (CurrentToken.Kind == SyntaxKind.DotToken)
		{
			InternalSyntaxToken separator = EatToken();
			internalNameSyntax = ParseQualifiedNameRight(internalNameSyntax, separator);
		}
		return internalNameSyntax;
	}

	private InternalNameSyntax ParseQualifiedNameRight(InternalNameSyntax left, InternalSyntaxToken separator)
	{
		InternalIdentifierNameSyntax right = ParseIdentifierName();
		if (separator.Kind == SyntaxKind.DotToken)
		{
			return ContextAwareSyntaxFactory.QualifiedName(left, separator, right);
		}
		return left;
	}

	private InternalCodeExpressionSyntax ParseInListExpression(InternalCodeExpressionSyntax expression)
	{
		InternalSyntaxToken inKeywordToken = EatToken(SyntaxKind.InKeyword);
		InternalSyntaxToken openBracketToken = EatToken(SyntaxKind.OpenBracketToken);
		InternalSeparatedSyntaxListBuilder<InternalCodeExpressionSyntax> item = Pool.AllocateSeparated<InternalCodeExpressionSyntax>();
		try
		{
			while (true)
			{
				InternalCodeExpressionSyntax internalCodeExpressionSyntax = ParseValue();
				if (CurrentToken.Kind == SyntaxKind.DotDotToken)
				{
					internalCodeExpressionSyntax = ContextAwareSyntaxFactory.BinaryExpression(SyntaxFacts.GetBinaryExpression(SyntaxKind.DotDotToken), internalCodeExpressionSyntax, EatToken(SyntaxKind.DotDotToken), ParseValue());
				}
				item.Add(internalCodeExpressionSyntax);
				if (CurrentToken.Kind != SyntaxKind.CommaToken)
				{
					break;
				}
				item.AddSeparator(EatToken(SyntaxKind.CommaToken));
			}
			InternalSyntaxToken closeBracketToken = EatToken(SyntaxKind.CloseBracketToken);
			return ContextAwareSyntaxFactory.InListExpression(expression, inKeywordToken, ContextAwareSyntaxFactory.BracketedArgumentList(openBracketToken, item.ToList(), closeBracketToken));
		}
		finally
		{
			Pool.Free(item);
		}
	}

	private InternalCodeExpressionSyntax ParseInvocationExpression(InternalCodeExpressionSyntax expression)
	{
		if (CurrentToken.Kind != SyntaxKind.OpenParenToken)
		{
			return expression;
		}
		InternalArgumentListSyntax argumentList = ParseParenthesizedArgumentList();
		return ContextAwareSyntaxFactory.InvocationExpression(expression, argumentList);
	}

	private InternalSyntaxToken SemicolonOrNothing()
	{
		if (CurrentToken.Kind != SyntaxKind.SemicolonToken)
		{
			return null;
		}
		return EatToken();
	}

	private bool IsEndOfCaseLines()
	{
		if (CurrentToken.Kind != SyntaxKind.ElseKeyword && CurrentToken.Kind != SyntaxKind.EndKeyword)
		{
			return CurrentToken.Kind == SyntaxKind.EndOfFileToken;
		}
		return true;
	}

	internal override bool IsTokenIdentifier(SyntaxKind kind)
	{
		return kind.IsTokenIdentifier();
	}
}
