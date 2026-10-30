using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class LambdaUtilities
{
	public static bool IsLambda(SyntaxNode node)
	{
		switch (node.Kind())
		{
		case SyntaxKind.AnonymousMethodExpression:
		case SyntaxKind.SimpleLambdaExpression:
		case SyntaxKind.ParenthesizedLambdaExpression:
		case SyntaxKind.LetClause:
		case SyntaxKind.JoinClause:
		case SyntaxKind.WhereClause:
		case SyntaxKind.AscendingOrdering:
		case SyntaxKind.DescendingOrdering:
		case SyntaxKind.GroupClause:
		case SyntaxKind.LocalFunctionStatement:
			return true;
		case SyntaxKind.SelectClause:
		{
			SelectClauseSyntax selectClauseSyntax = (SelectClauseSyntax)node;
			return !IsReducedSelectOrGroupByClause(selectClauseSyntax, selectClauseSyntax.Expression);
		}
		case SyntaxKind.FromClause:
			return !node.Parent.IsKind(SyntaxKind.QueryExpression);
		default:
			return false;
		}
	}

	public static bool IsNotLambda(SyntaxNode node)
	{
		return !IsLambda(node);
	}

	public static SyntaxNode GetLambda(SyntaxNode lambdaBody)
	{
		SyntaxNode parent = lambdaBody.Parent;
		if (parent.Kind() == SyntaxKind.ArrowExpressionClause)
		{
			parent = parent.Parent;
		}
		return parent;
	}

	internal static SyntaxNode TryGetCorrespondingLambdaBody(SyntaxNode oldBody, SyntaxNode newLambda)
	{
		switch (newLambda.Kind())
		{
		case SyntaxKind.AnonymousMethodExpression:
		case SyntaxKind.SimpleLambdaExpression:
		case SyntaxKind.ParenthesizedLambdaExpression:
			return ((AnonymousFunctionExpressionSyntax)newLambda).Body;
		case SyntaxKind.FromClause:
			return ((FromClauseSyntax)newLambda).Expression;
		case SyntaxKind.LetClause:
			return ((LetClauseSyntax)newLambda).Expression;
		case SyntaxKind.WhereClause:
			return ((WhereClauseSyntax)newLambda).Condition;
		case SyntaxKind.AscendingOrdering:
		case SyntaxKind.DescendingOrdering:
			return ((OrderingSyntax)newLambda).Expression;
		case SyntaxKind.SelectClause:
		{
			SelectClauseSyntax selectClauseSyntax = (SelectClauseSyntax)newLambda;
			if (!IsReducedSelectOrGroupByClause(selectClauseSyntax, selectClauseSyntax.Expression))
			{
				return selectClauseSyntax.Expression;
			}
			return null;
		}
		case SyntaxKind.JoinClause:
		{
			JoinClauseSyntax joinClauseSyntax = (JoinClauseSyntax)oldBody.Parent;
			JoinClauseSyntax joinClauseSyntax2 = (JoinClauseSyntax)newLambda;
			if (joinClauseSyntax.LeftExpression != oldBody)
			{
				return joinClauseSyntax2.RightExpression;
			}
			return joinClauseSyntax2.LeftExpression;
		}
		case SyntaxKind.GroupClause:
		{
			GroupClauseSyntax groupClauseSyntax = (GroupClauseSyntax)oldBody.Parent;
			GroupClauseSyntax groupClauseSyntax2 = (GroupClauseSyntax)newLambda;
			if (groupClauseSyntax.GroupExpression != oldBody)
			{
				return groupClauseSyntax2.ByExpression;
			}
			if (!IsReducedSelectOrGroupByClause(groupClauseSyntax2, groupClauseSyntax2.GroupExpression))
			{
				return groupClauseSyntax2.GroupExpression;
			}
			return null;
		}
		case SyntaxKind.LocalFunctionStatement:
			return GetLocalFunctionBody((LocalFunctionStatementSyntax)newLambda);
		default:
			throw ExceptionUtilities.UnexpectedValue(newLambda.Kind());
		}
	}

	public static SyntaxNode GetNestedFunctionBody(SyntaxNode nestedFunction)
	{
		if (nestedFunction != null)
		{
			if (nestedFunction is AnonymousFunctionExpressionSyntax anonymousFunctionExpressionSyntax)
			{
				return anonymousFunctionExpressionSyntax.Body;
			}
			if (nestedFunction is LocalFunctionStatementSyntax localFunctionStatementSyntax)
			{
				LocalFunctionStatementSyntax localFunctionStatementSyntax2 = localFunctionStatementSyntax;
				return (SyntaxNode)(((object)localFunctionStatementSyntax2.Body) ?? ((object)localFunctionStatementSyntax2.ExpressionBody.Expression));
			}
		}
		throw ExceptionUtilities.UnexpectedValue(nestedFunction);
	}

	public static bool IsNotLambdaBody(SyntaxNode node)
	{
		return !IsLambdaBody(node);
	}

	public static bool IsLambdaBody(SyntaxNode node, bool allowReducedLambdas = false)
	{
		SyntaxNode syntaxNode = node?.Parent;
		if (syntaxNode == null)
		{
			return false;
		}
		switch (syntaxNode.Kind())
		{
		case SyntaxKind.AnonymousMethodExpression:
		case SyntaxKind.SimpleLambdaExpression:
		case SyntaxKind.ParenthesizedLambdaExpression:
			return ((AnonymousFunctionExpressionSyntax)syntaxNode).Body == node;
		case SyntaxKind.LocalFunctionStatement:
			return ((LocalFunctionStatementSyntax)syntaxNode).Body == node;
		case SyntaxKind.ArrowExpressionClause:
		{
			ArrowExpressionClauseSyntax arrowExpressionClauseSyntax = (ArrowExpressionClauseSyntax)syntaxNode;
			if (arrowExpressionClauseSyntax.Expression == node)
			{
				return arrowExpressionClauseSyntax.Parent is LocalFunctionStatementSyntax;
			}
			return false;
		}
		case SyntaxKind.FromClause:
		{
			FromClauseSyntax fromClauseSyntax = (FromClauseSyntax)syntaxNode;
			if (fromClauseSyntax.Expression == node)
			{
				return fromClauseSyntax.Parent is QueryBodySyntax;
			}
			return false;
		}
		case SyntaxKind.JoinClause:
		{
			JoinClauseSyntax joinClauseSyntax = (JoinClauseSyntax)syntaxNode;
			if (joinClauseSyntax.LeftExpression != node)
			{
				return joinClauseSyntax.RightExpression == node;
			}
			return true;
		}
		case SyntaxKind.LetClause:
			return ((LetClauseSyntax)syntaxNode).Expression == node;
		case SyntaxKind.WhereClause:
			return ((WhereClauseSyntax)syntaxNode).Condition == node;
		case SyntaxKind.AscendingOrdering:
		case SyntaxKind.DescendingOrdering:
			return ((OrderingSyntax)syntaxNode).Expression == node;
		case SyntaxKind.SelectClause:
		{
			SelectClauseSyntax selectClauseSyntax = (SelectClauseSyntax)syntaxNode;
			if (selectClauseSyntax.Expression == node)
			{
				if (!allowReducedLambdas)
				{
					return !IsReducedSelectOrGroupByClause(selectClauseSyntax, selectClauseSyntax.Expression);
				}
				return true;
			}
			return false;
		}
		case SyntaxKind.GroupClause:
		{
			GroupClauseSyntax groupClauseSyntax = (GroupClauseSyntax)syntaxNode;
			if (groupClauseSyntax.GroupExpression != node || (!allowReducedLambdas && IsReducedSelectOrGroupByClause(groupClauseSyntax, groupClauseSyntax.GroupExpression)))
			{
				return groupClauseSyntax.ByExpression == node;
			}
			return true;
		}
		default:
			return false;
		}
	}

	private static bool IsReducedSelectOrGroupByClause(SelectOrGroupClauseSyntax selectOrGroupClause, ExpressionSyntax selectOrGroupExpression)
	{
		if (!selectOrGroupExpression.IsKind(SyntaxKind.IdentifierName))
		{
			return false;
		}
		SyntaxToken identifier = ((IdentifierNameSyntax)selectOrGroupExpression).Identifier;
		CSharpSyntaxNode parent = selectOrGroupClause.Parent.Parent;
		QueryBodySyntax body;
		SyntaxToken identifier2;
		if (parent.IsKind(SyntaxKind.QueryExpression))
		{
			QueryExpressionSyntax queryExpressionSyntax = (QueryExpressionSyntax)parent;
			body = queryExpressionSyntax.Body;
			identifier2 = queryExpressionSyntax.FromClause.Identifier;
		}
		else
		{
			QueryContinuationSyntax queryContinuationSyntax = (QueryContinuationSyntax)parent;
			identifier2 = queryContinuationSyntax.Identifier;
			body = queryContinuationSyntax.Body;
		}
		if (!SyntaxFactory.AreEquivalent(identifier2, identifier))
		{
			return false;
		}
		if (selectOrGroupClause.IsKind(SyntaxKind.SelectClause) && body.Clauses.Count == 0)
		{
			return false;
		}
		foreach (QueryClauseSyntax clause in body.Clauses)
		{
			if (!clause.IsKind(SyntaxKind.WhereClause) && !clause.IsKind(SyntaxKind.OrderByClause))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsLambdaBodyStatementOrExpression(SyntaxNode node)
	{
		return IsLambdaBody(node);
	}

	public static bool IsLambdaBodyStatementOrExpression(SyntaxNode node, out SyntaxNode lambdaBody)
	{
		lambdaBody = node;
		return IsLambdaBody(node);
	}

	public static bool TryGetLambdaBodies(SyntaxNode node, out SyntaxNode lambdaBody1, out SyntaxNode lambdaBody2)
	{
		lambdaBody1 = null;
		lambdaBody2 = null;
		switch (node.Kind())
		{
		case SyntaxKind.AnonymousMethodExpression:
		case SyntaxKind.SimpleLambdaExpression:
		case SyntaxKind.ParenthesizedLambdaExpression:
			lambdaBody1 = ((AnonymousFunctionExpressionSyntax)node).Body;
			return true;
		case SyntaxKind.FromClause:
			if (node.Parent.IsKind(SyntaxKind.QueryExpression))
			{
				return false;
			}
			lambdaBody1 = ((FromClauseSyntax)node).Expression;
			return true;
		case SyntaxKind.JoinClause:
		{
			JoinClauseSyntax joinClauseSyntax = (JoinClauseSyntax)node;
			lambdaBody1 = joinClauseSyntax.LeftExpression;
			lambdaBody2 = joinClauseSyntax.RightExpression;
			return true;
		}
		case SyntaxKind.LetClause:
			lambdaBody1 = ((LetClauseSyntax)node).Expression;
			return true;
		case SyntaxKind.WhereClause:
			lambdaBody1 = ((WhereClauseSyntax)node).Condition;
			return true;
		case SyntaxKind.AscendingOrdering:
		case SyntaxKind.DescendingOrdering:
			lambdaBody1 = ((OrderingSyntax)node).Expression;
			return true;
		case SyntaxKind.SelectClause:
		{
			SelectClauseSyntax selectClauseSyntax = (SelectClauseSyntax)node;
			if (IsReducedSelectOrGroupByClause(selectClauseSyntax, selectClauseSyntax.Expression))
			{
				return false;
			}
			lambdaBody1 = selectClauseSyntax.Expression;
			return true;
		}
		case SyntaxKind.GroupClause:
		{
			GroupClauseSyntax groupClauseSyntax = (GroupClauseSyntax)node;
			if (IsReducedSelectOrGroupByClause(groupClauseSyntax, groupClauseSyntax.GroupExpression))
			{
				lambdaBody1 = groupClauseSyntax.ByExpression;
			}
			else
			{
				lambdaBody1 = groupClauseSyntax.GroupExpression;
				lambdaBody2 = groupClauseSyntax.ByExpression;
			}
			return true;
		}
		case SyntaxKind.LocalFunctionStatement:
			lambdaBody1 = GetLocalFunctionBody((LocalFunctionStatementSyntax)node);
			return true;
		default:
			return false;
		}
	}

	public static bool AreEquivalentIgnoringLambdaBodies(SyntaxNode oldNode, SyntaxNode newNode)
	{
		IEnumerable<SyntaxToken> first = oldNode.DescendantTokens((SyntaxNode node) => node == oldNode || !IsLambdaBodyStatementOrExpression(node));
		IEnumerable<SyntaxToken> second = newNode.DescendantTokens((SyntaxNode node) => node == newNode || !IsLambdaBodyStatementOrExpression(node));
		return first.SequenceEqual(second, SyntaxFactory.AreEquivalent);
	}

	internal static bool IsQueryPairLambda(SyntaxNode syntax)
	{
		if (!syntax.IsKind(SyntaxKind.GroupClause) && !syntax.IsKind(SyntaxKind.JoinClause))
		{
			return syntax.IsKind(SyntaxKind.FromClause);
		}
		return true;
	}

	internal static bool IsClosureScope(SyntaxNode node)
	{
		switch (node.Kind())
		{
		case SyntaxKind.Block:
		case SyntaxKind.ForStatement:
		case SyntaxKind.ForEachStatement:
		case SyntaxKind.UsingStatement:
		case SyntaxKind.SwitchStatement:
		case SyntaxKind.CatchClause:
		case SyntaxKind.ConstructorDeclaration:
		case SyntaxKind.ArrowExpressionClause:
		case SyntaxKind.ForEachVariableStatement:
			return true;
		case SyntaxKind.ExpressionStatement:
		case SyntaxKind.GotoCaseStatement:
		case SyntaxKind.ReturnStatement:
		case SyntaxKind.YieldReturnStatement:
		case SyntaxKind.ThrowStatement:
		case SyntaxKind.WhileStatement:
		case SyntaxKind.DoStatement:
		case SyntaxKind.FixedStatement:
		case SyntaxKind.LockStatement:
		case SyntaxKind.IfStatement:
		case SyntaxKind.BaseConstructorInitializer:
		case SyntaxKind.ThisConstructorInitializer:
			return true;
		case SyntaxKind.ClassDeclaration:
		case SyntaxKind.StructDeclaration:
			return true;
		default:
			if (node.Parent != null)
			{
				switch (node.Parent.Kind())
				{
				case SyntaxKind.EqualsValueClause:
					return true;
				case SyntaxKind.ForStatement:
					if (((ForStatementSyntax)node.Parent).Incrementors.FirstOrDefault() == node)
					{
						return true;
					}
					break;
				}
			}
			if (IsLambdaBody(node))
			{
				return true;
			}
			if (node is ExpressionSyntax && node.Parent != null && node.Parent.Parent == null)
			{
				return true;
			}
			return false;
		}
	}

	private static SyntaxNode GetLocalFunctionBody(LocalFunctionStatementSyntax localFunctionStatementSyntax)
	{
		object obj = localFunctionStatementSyntax.Body;
		if (obj == null)
		{
			ArrowExpressionClauseSyntax expressionBody = localFunctionStatementSyntax.ExpressionBody;
			if (expressionBody == null)
			{
				return null;
			}
			obj = expressionBody.Expression;
		}
		return (SyntaxNode)obj;
	}
}
