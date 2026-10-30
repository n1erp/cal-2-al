using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

internal static class LookupPosition
{
	internal static bool IsInBlock(int position, BlockSyntax blockOpt)
	{
		if (blockOpt != null)
		{
			return IsBeforeToken(position, blockOpt, blockOpt.EndKeywordToken);
		}
		return false;
	}

	internal static bool IsInBody(int position, MethodOrTriggerDeclarationSyntax method)
	{
		return IsInBlock(position, method.Body);
	}

	internal static bool IsBetweenTokens(int position, SyntaxToken firstIncluded, SyntaxToken firstExcluded)
	{
		if (position >= firstIncluded.SpanStart)
		{
			return IsBeforeToken(position, firstExcluded);
		}
		return false;
	}

	private static bool IsBeforeToken(int position, SyntaxNode node, SyntaxToken firstExcluded)
	{
		if (node != null && IsBeforeToken(position, firstExcluded))
		{
			return position >= node.SpanStart;
		}
		return false;
	}

	private static bool IsBeforeToken(int position, SyntaxToken firstExcluded)
	{
		if (firstExcluded.Kind != SyntaxKind.None)
		{
			return position < firstExcluded.SpanStart;
		}
		return true;
	}

	internal static bool IsInParameterList(int position, MethodDeclarationSyntax methodDecl)
	{
		ParameterListSyntax parameterList = methodDecl.ParameterList;
		return IsBeforeToken(position, parameterList.Parameters.Node, methodDecl.ParameterList.CloseParenthesisToken);
	}

	internal static bool IsInMethodDeclaration(int position, MethodOrTriggerDeclarationSyntax methodDecl)
	{
		DebugAssertHelper.Assert(methodDecl != null);
		BlockSyntax body = methodDecl.Body;
		if (body == null)
		{
			return IsBeforeToken(position, methodDecl, methodDecl.SemicolonToken);
		}
		return IsBeforeToken(position, methodDecl.Body, body.EndKeywordToken);
	}

	internal static bool IsInProperty(int position, PropertySyntax property)
	{
		DebugAssertHelper.Assert(property != null);
		return IsBetweenTokens(position, property.Name.Identifier, property.SemicolonToken);
	}

	internal static bool IsInStatementScope(int position, StatementSyntax statement)
	{
		DebugAssertHelper.Assert(statement != null);
		if (statement.Kind == SyntaxKind.EmptyStatement)
		{
			return false;
		}
		SyntaxToken firstIncludedToken = GetFirstIncludedToken(statement);
		if (firstIncludedToken != default(SyntaxToken))
		{
			return IsBetweenTokens(position, firstIncludedToken, GetFirstExcludedToken(statement));
		}
		return false;
	}

	private static SyntaxToken GetFirstIncludedToken(StatementSyntax statement, bool inRecursiveCall = false)
	{
		DebugAssertHelper.Assert(statement != null);
		switch (statement.Kind)
		{
		case SyntaxKind.Block:
			return ((BlockSyntax)statement).BeginKeywordToken;
		case SyntaxKind.BreakStatement:
			return ((BreakStatementSyntax)statement).BreakKeyword;
		case SyntaxKind.AssignmentStatement:
		case SyntaxKind.CompoundAssignmentStatement:
		case SyntaxKind.ExpressionStatement:
			return statement.GetFirstToken();
		case SyntaxKind.WhileStatement:
			return ((WhileStatementSyntax)statement).WhileKeywordToken;
		case SyntaxKind.ForEachStatement:
			return ((ForEachStatementSyntax)statement).ForEachKeywordToken;
		case SyntaxKind.RepeatStatement:
			return ((RepeatStatementSyntax)statement).RepeatKeywordToken;
		case SyntaxKind.WithStatement:
			return ((WithStatementSyntax)statement).WithKeywordToken;
		case SyntaxKind.IfStatement:
			return ((IfStatementSyntax)statement).IfKeywordToken;
		case SyntaxKind.CaseStatement:
			return ((CaseStatementSyntax)statement).CaseKeywordToken;
		case SyntaxKind.AssertErrorStatement:
			return ((AssertErrorStatementSyntax)statement).AssertErrorKeywordToken;
		case SyntaxKind.ExitStatement:
			return ((ExitStatementSyntax)statement).ExitKeywordToken;
		case SyntaxKind.ForStatement:
			return ((ForStatementSyntax)statement).ForKeywordToken;
		case SyntaxKind.OrphanedElseStatement:
			return ((OrphanedElseStatementSyntax)statement).ElseKeywordToken;
		case SyntaxKind.EmptyStatement:
			return default;
		default:
			throw new InvalidOperationException(statement.Kind.ToString());
		}
	}

	private static SyntaxToken GetFirstExcludedToken(StatementSyntax statement)
	{
		DebugAssertHelper.Assert(statement != null);
		switch (statement.Kind)
		{
		case SyntaxKind.Block:
			return ((BlockSyntax)statement).SemicolonToken;
		case SyntaxKind.BreakStatement:
			return ((BreakStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.ExpressionStatement:
			return ((ExpressionStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.AssignmentStatement:
			return ((AssignmentStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.CompoundAssignmentStatement:
			return ((CompoundAssignmentStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.WhileStatement:
			return GetFirstExcludedToken(((WhileStatementSyntax)statement).Statement);
		case SyntaxKind.ForEachStatement:
			return GetFirstExcludedToken(((ForEachStatementSyntax)statement).Statement);
		case SyntaxKind.RepeatStatement:
			return ((RepeatStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.WithStatement:
			return GetFirstExcludedToken(((WithStatementSyntax)statement).Statement);
		case SyntaxKind.IfStatement:
		{
			IfStatementSyntax ifStatementSyntax = (IfStatementSyntax)statement;
			if (ifStatementSyntax.ElseStatement == null)
			{
				return GetFirstExcludedToken(ifStatementSyntax.Statement);
			}
			return GetFirstExcludedToken(ifStatementSyntax.ElseStatement);
		}
		case SyntaxKind.CaseStatement:
			return ((CaseStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.AssertErrorStatement:
			return GetFirstExcludedToken(((AssertErrorStatementSyntax)statement).Statement);
		case SyntaxKind.ExitStatement:
			return ((ExitStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.ForStatement:
			return GetFirstExcludedToken(((ForStatementSyntax)statement).Statement);
		case SyntaxKind.OrphanedElseStatement:
			return GetFirstExcludedToken(((OrphanedElseStatementSyntax)statement).ElseStatement);
		case SyntaxKind.EmptyStatement:
			return default;
		default:
			throw new InvalidOperationException(statement.Kind.ToString());
		}
	}
}
