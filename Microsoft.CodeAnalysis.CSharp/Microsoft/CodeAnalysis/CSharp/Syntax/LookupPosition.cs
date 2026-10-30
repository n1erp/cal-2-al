using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class LookupPosition
{
	internal static bool IsInBlock(int position, BlockSyntax blockOpt)
	{
		if (blockOpt != null)
		{
			return IsBeforeToken(position, blockOpt, blockOpt.CloseBraceToken);
		}
		return false;
	}

	internal static bool IsInExpressionBody(int position, ArrowExpressionClauseSyntax expressionBodyOpt, SyntaxToken semicolonToken)
	{
		if (expressionBodyOpt != null)
		{
			return IsBeforeToken(position, expressionBodyOpt, semicolonToken);
		}
		return false;
	}

	private static bool IsInBody(int position, BlockSyntax blockOpt, ArrowExpressionClauseSyntax exprOpt, SyntaxToken semiOpt)
	{
		if (!IsInExpressionBody(position, exprOpt, semiOpt))
		{
			return IsInBlock(position, blockOpt);
		}
		return true;
	}

	internal static bool IsInBody(int position, PropertyDeclarationSyntax property)
	{
		return IsInBody(position, null, property.GetExpressionBodySyntax(), property.SemicolonToken);
	}

	internal static bool IsInBody(int position, IndexerDeclarationSyntax indexer)
	{
		return IsInBody(position, null, indexer.GetExpressionBodySyntax(), indexer.SemicolonToken);
	}

	internal static bool IsInBody(int position, AccessorDeclarationSyntax method)
	{
		return IsInBody(position, method.Body, method.GetExpressionBodySyntax(), method.SemicolonToken);
	}

	internal static bool IsInBody(int position, BaseMethodDeclarationSyntax method)
	{
		return IsInBody(position, method.Body, method.GetExpressionBodySyntax(), method.SemicolonToken);
	}

	internal static bool IsBetweenTokens(int position, SyntaxToken firstIncluded, SyntaxToken firstExcluded)
	{
		if (position >= firstIncluded.SpanStart)
		{
			return IsBeforeToken(position, firstExcluded);
		}
		return false;
	}

	private static bool IsBeforeToken(int position, CSharpSyntaxNode node, SyntaxToken firstExcluded)
	{
		if (IsBeforeToken(position, firstExcluded))
		{
			return position >= node.SpanStart;
		}
		return false;
	}

	private static bool IsBeforeToken(int position, SyntaxToken firstExcluded)
	{
		if (firstExcluded.Kind() != SyntaxKind.None)
		{
			return position < firstExcluded.SpanStart;
		}
		return true;
	}

	internal static bool IsInAttributeSpecification(int position, SyntaxList<AttributeListSyntax> attributesSyntaxList)
	{
		int count = attributesSyntaxList.Count;
		if (count == 0)
		{
			return false;
		}
		SyntaxToken openBracketToken = attributesSyntaxList[0].OpenBracketToken;
		SyntaxToken closeBracketToken = attributesSyntaxList[count - 1].CloseBracketToken;
		return IsBetweenTokens(position, openBracketToken, closeBracketToken);
	}

	internal static bool IsInTypeParameterList(int position, TypeDeclarationSyntax typeDecl)
	{
		TypeParameterListSyntax typeParameterList = typeDecl.TypeParameterList;
		if (typeParameterList != null)
		{
			return IsBeforeToken(position, typeParameterList, typeParameterList.GreaterThanToken);
		}
		return false;
	}

	internal static bool IsInParameterList(int position, BaseMethodDeclarationSyntax methodDecl)
	{
		ParameterListSyntax parameterList = methodDecl.ParameterList;
		return IsBeforeToken(position, parameterList, parameterList.CloseParenToken);
	}

	internal static bool IsInMethodDeclaration(int position, BaseMethodDeclarationSyntax methodDecl)
	{
		BlockSyntax body = methodDecl.Body;
		if (body == null)
		{
			return IsBeforeToken(position, methodDecl, methodDecl.SemicolonToken);
		}
		if (!IsBeforeToken(position, methodDecl, body.CloseBraceToken))
		{
			return IsInExpressionBody(position, methodDecl.GetExpressionBodySyntax(), methodDecl.SemicolonToken);
		}
		return true;
	}

	internal static bool IsInMethodDeclaration(int position, AccessorDeclarationSyntax accessorDecl)
	{
		SyntaxToken firstExcluded = accessorDecl.Body?.CloseBraceToken ?? accessorDecl.SemicolonToken;
		return IsBeforeToken(position, accessorDecl, firstExcluded);
	}

	internal static bool IsInDelegateDeclaration(int position, DelegateDeclarationSyntax delegateDecl)
	{
		return IsBeforeToken(position, delegateDecl, delegateDecl.SemicolonToken);
	}

	internal static bool IsInTypeDeclaration(int position, BaseTypeDeclarationSyntax typeDecl)
	{
		return IsBeforeToken(position, typeDecl, typeDecl.CloseBraceToken);
	}

	internal static bool IsInNamespaceDeclaration(int position, NamespaceDeclarationSyntax namespaceDecl)
	{
		return IsBetweenTokens(position, namespaceDecl.NamespaceKeyword, namespaceDecl.CloseBraceToken);
	}

	internal static bool IsInConstructorParameterScope(int position, ConstructorDeclarationSyntax constructorDecl)
	{
		ConstructorInitializerSyntax initializer = constructorDecl.Initializer;
		if (constructorDecl.Body == null && constructorDecl.ExpressionBody == null)
		{
			SyntaxToken nextToken = SyntaxNavigator.Instance.GetNextToken(constructorDecl, null, null);
			if (initializer != null)
			{
				return IsBetweenTokens(position, initializer.ColonToken, nextToken);
			}
			if (position >= constructorDecl.ParameterList.CloseParenToken.Span.End)
			{
				return IsBeforeToken(position, nextToken);
			}
			return false;
		}
		if (initializer != null)
		{
			return IsBetweenTokens(position, initializer.ColonToken, (constructorDecl.SemicolonToken.Kind() == SyntaxKind.None) ? constructorDecl.Body.CloseBraceToken : constructorDecl.SemicolonToken);
		}
		return IsInBody(position, constructorDecl);
	}

	internal static bool IsInMethodTypeParameterScope(int position, MethodDeclarationSyntax methodDecl)
	{
		if (methodDecl.TypeParameterList == null)
		{
			return false;
		}
		if (methodDecl.ReturnType.FullSpan.Contains(position))
		{
			return true;
		}
		if (IsInAttributeSpecification(position, methodDecl.AttributeLists))
		{
			return false;
		}
		SyntaxToken firstIncluded = methodDecl.ExplicitInterfaceSpecifier?.GetFirstToken() ?? methodDecl.Identifier;
		SyntaxToken firstExcluded = methodDecl.TypeParameterList?.LessThanToken ?? methodDecl.ParameterList.OpenParenToken;
		return !IsBetweenTokens(position, firstIncluded, firstExcluded);
	}

	internal static bool IsInStatementScope(int position, StatementSyntax statement)
	{
		if (statement.Kind() == SyntaxKind.EmptyStatement)
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

	internal static bool IsInSwitchSectionScope(int position, SwitchSectionSyntax section)
	{
		return section.Span.Contains(position);
	}

	internal static bool IsInCatchBlockScope(int position, CatchClauseSyntax catchClause)
	{
		return IsBetweenTokens(position, catchClause.Block.OpenBraceToken, catchClause.Block.CloseBraceToken);
	}

	internal static bool IsInCatchFilterScope(int position, CatchFilterClauseSyntax filterClause)
	{
		return IsBetweenTokens(position, filterClause.OpenParenToken, filterClause.CloseParenToken);
	}

	private static SyntaxToken GetFirstIncludedToken(StatementSyntax statement)
	{
		switch (statement.Kind())
		{
		case SyntaxKind.Block:
			return ((BlockSyntax)statement).OpenBraceToken;
		case SyntaxKind.BreakStatement:
			return ((BreakStatementSyntax)statement).BreakKeyword;
		case SyntaxKind.CheckedStatement:
		case SyntaxKind.UncheckedStatement:
			return ((CheckedStatementSyntax)statement).Keyword;
		case SyntaxKind.ContinueStatement:
			return ((ContinueStatementSyntax)statement).ContinueKeyword;
		case SyntaxKind.LocalDeclarationStatement:
		case SyntaxKind.ExpressionStatement:
			return statement.GetFirstToken();
		case SyntaxKind.DoStatement:
			return ((DoStatementSyntax)statement).DoKeyword;
		case SyntaxKind.EmptyStatement:
			return default;
		case SyntaxKind.FixedStatement:
			return ((FixedStatementSyntax)statement).FixedKeyword;
		case SyntaxKind.ForEachStatement:
		case SyntaxKind.ForEachVariableStatement:
			return ((CommonForEachStatementSyntax)statement).OpenParenToken.GetNextToken();
		case SyntaxKind.ForStatement:
			return ((ForStatementSyntax)statement).OpenParenToken.GetNextToken();
		case SyntaxKind.GotoStatement:
		case SyntaxKind.GotoCaseStatement:
		case SyntaxKind.GotoDefaultStatement:
			return ((GotoStatementSyntax)statement).GotoKeyword;
		case SyntaxKind.IfStatement:
			return ((IfStatementSyntax)statement).IfKeyword;
		case SyntaxKind.LabeledStatement:
			return ((LabeledStatementSyntax)statement).Identifier;
		case SyntaxKind.LockStatement:
			return ((LockStatementSyntax)statement).LockKeyword;
		case SyntaxKind.ReturnStatement:
			return ((ReturnStatementSyntax)statement).ReturnKeyword;
		case SyntaxKind.SwitchStatement:
			return ((SwitchStatementSyntax)statement).Expression.GetFirstToken();
		case SyntaxKind.ThrowStatement:
			return ((ThrowStatementSyntax)statement).ThrowKeyword;
		case SyntaxKind.TryStatement:
			return ((TryStatementSyntax)statement).TryKeyword;
		case SyntaxKind.UnsafeStatement:
			return ((UnsafeStatementSyntax)statement).UnsafeKeyword;
		case SyntaxKind.UsingStatement:
			return ((UsingStatementSyntax)statement).UsingKeyword;
		case SyntaxKind.WhileStatement:
			return ((WhileStatementSyntax)statement).WhileKeyword;
		case SyntaxKind.YieldReturnStatement:
		case SyntaxKind.YieldBreakStatement:
			return ((YieldStatementSyntax)statement).YieldKeyword;
		case SyntaxKind.LocalFunctionStatement:
			return statement.GetFirstToken();
		default:
			throw ExceptionUtilities.UnexpectedValue(statement.Kind());
		}
	}

	internal static SyntaxToken GetFirstExcludedToken(StatementSyntax statement)
	{
		switch (statement.Kind())
		{
		case SyntaxKind.Block:
			return ((BlockSyntax)statement).CloseBraceToken;
		case SyntaxKind.BreakStatement:
			return ((BreakStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.CheckedStatement:
		case SyntaxKind.UncheckedStatement:
			return ((CheckedStatementSyntax)statement).Block.CloseBraceToken;
		case SyntaxKind.ContinueStatement:
			return ((ContinueStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.LocalDeclarationStatement:
			return ((LocalDeclarationStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.DoStatement:
			return ((DoStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.EmptyStatement:
			return ((EmptyStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.ExpressionStatement:
			return ((ExpressionStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.FixedStatement:
			return GetFirstExcludedToken(((FixedStatementSyntax)statement).Statement);
		case SyntaxKind.ForEachStatement:
		case SyntaxKind.ForEachVariableStatement:
			return GetFirstExcludedToken(((CommonForEachStatementSyntax)statement).Statement);
		case SyntaxKind.ForStatement:
			return GetFirstExcludedToken(((ForStatementSyntax)statement).Statement);
		case SyntaxKind.GotoStatement:
		case SyntaxKind.GotoCaseStatement:
		case SyntaxKind.GotoDefaultStatement:
			return ((GotoStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.IfStatement:
		{
			IfStatementSyntax ifStatementSyntax = (IfStatementSyntax)statement;
			ElseClauseSyntax elseClauseSyntax = ifStatementSyntax.Else;
			return GetFirstExcludedToken((elseClauseSyntax == null) ? ifStatementSyntax.Statement : elseClauseSyntax.Statement);
		}
		case SyntaxKind.LabeledStatement:
			return GetFirstExcludedToken(((LabeledStatementSyntax)statement).Statement);
		case SyntaxKind.LockStatement:
			return GetFirstExcludedToken(((LockStatementSyntax)statement).Statement);
		case SyntaxKind.ReturnStatement:
			return ((ReturnStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.SwitchStatement:
			return ((SwitchStatementSyntax)statement).CloseBraceToken;
		case SyntaxKind.ThrowStatement:
			return ((ThrowStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.TryStatement:
		{
			TryStatementSyntax tryStatementSyntax = (TryStatementSyntax)statement;
			return tryStatementSyntax.Finally?.Block.CloseBraceToken ?? tryStatementSyntax.Catches.LastOrDefault()?.Block.CloseBraceToken ?? tryStatementSyntax.Block.CloseBraceToken;
		}
		case SyntaxKind.UnsafeStatement:
			return ((UnsafeStatementSyntax)statement).Block.CloseBraceToken;
		case SyntaxKind.UsingStatement:
			return GetFirstExcludedToken(((UsingStatementSyntax)statement).Statement);
		case SyntaxKind.WhileStatement:
			return GetFirstExcludedToken(((WhileStatementSyntax)statement).Statement);
		case SyntaxKind.YieldReturnStatement:
		case SyntaxKind.YieldBreakStatement:
			return ((YieldStatementSyntax)statement).SemicolonToken;
		case SyntaxKind.LocalFunctionStatement:
		{
			LocalFunctionStatementSyntax localFunctionStatementSyntax = (LocalFunctionStatementSyntax)statement;
			if (localFunctionStatementSyntax.Body != null)
			{
				return GetFirstExcludedToken(localFunctionStatementSyntax.Body);
			}
			if (localFunctionStatementSyntax.SemicolonToken != default(SyntaxToken))
			{
				return localFunctionStatementSyntax.SemicolonToken;
			}
			return localFunctionStatementSyntax.ParameterList.GetLastToken();
		}
		default:
			throw ExceptionUtilities.UnexpectedValue(statement.Kind());
		}
	}

	internal static bool IsInAnonymousFunctionOrQuery(int position, SyntaxNode lambdaExpressionOrQueryNode)
	{
		CSharpSyntaxNode cSharpSyntaxNode;
		SyntaxToken nextToken;
		switch (lambdaExpressionOrQueryNode.Kind())
		{
		case SyntaxKind.SimpleLambdaExpression:
		{
			SimpleLambdaExpressionSyntax simpleLambdaExpressionSyntax = (SimpleLambdaExpressionSyntax)lambdaExpressionOrQueryNode;
			nextToken = simpleLambdaExpressionSyntax.ArrowToken;
			cSharpSyntaxNode = simpleLambdaExpressionSyntax.Body;
			break;
		}
		case SyntaxKind.ParenthesizedLambdaExpression:
		{
			ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpressionSyntax = (ParenthesizedLambdaExpressionSyntax)lambdaExpressionOrQueryNode;
			nextToken = parenthesizedLambdaExpressionSyntax.ArrowToken;
			cSharpSyntaxNode = parenthesizedLambdaExpressionSyntax.Body;
			break;
		}
		case SyntaxKind.AnonymousMethodExpression:
			cSharpSyntaxNode = ((AnonymousMethodExpressionSyntax)lambdaExpressionOrQueryNode).Block;
			nextToken = cSharpSyntaxNode.GetFirstToken(includeZeroWidth: true);
			break;
		default:
			nextToken = lambdaExpressionOrQueryNode.GetFirstToken().GetNextToken();
			return IsBetweenTokens(position, nextToken, lambdaExpressionOrQueryNode.GetLastToken().GetNextToken());
		}
		SyntaxToken firstExcluded = ((cSharpSyntaxNode is StatementSyntax statement) ? GetFirstExcludedToken(statement) : SyntaxNavigator.Instance.GetNextToken(cSharpSyntaxNode, null, null));
		return IsBetweenTokens(position, nextToken, firstExcluded);
	}

	internal static bool IsInXmlAttributeValue(int position, XmlAttributeSyntax attribute)
	{
		return IsBetweenTokens(position, attribute.StartQuoteToken, attribute.EndQuoteToken);
	}
}
