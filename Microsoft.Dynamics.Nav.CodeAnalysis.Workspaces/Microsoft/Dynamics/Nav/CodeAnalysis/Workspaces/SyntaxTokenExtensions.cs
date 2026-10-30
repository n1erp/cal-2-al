using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal static class SyntaxTokenExtensions
{
	public static SyntaxNode GetAncestor(this SyntaxToken token, Func<SyntaxNode, bool> predicate)
	{
		return token.GetAncestor<SyntaxNode>(predicate);
	}

	public static T GetAncestor<T>(this SyntaxToken token, Func<T, bool> predicate = null) where T : SyntaxNode
	{
		if (token.Parent == null)
		{
			return null;
		}
		return token.Parent.FirstAncestorOrSelf(predicate);
	}

	public static IEnumerable<SyntaxNode> GetAncestors(this SyntaxToken token, Func<SyntaxNode, bool> predicate)
	{
		if (token.Parent == null)
		{
			return SpecializedCollections.EmptyEnumerable<SyntaxNode>();
		}
		return token.Parent.AncestorsAndSelf().Where(predicate);
	}

	public static SyntaxNode GetCommonRoot(this SyntaxToken token1, SyntaxToken token2)
	{
		Contract.ThrowIfTrue(token1.Kind == SyntaxKind.None || token2.Kind == SyntaxKind.None);
		if (token1.Parent == null || token2.Parent == null)
		{
			return null;
		}
		return token1.Parent.GetCommonRoot(token2.Parent);
	}

	public static bool CheckParent<T>(this SyntaxToken token, Func<T, bool> valueChecker) where T : SyntaxNode
	{
		if (!(token.Parent is T arg))
		{
			return false;
		}
		return valueChecker(arg);
	}

	public static int Width(this SyntaxToken token)
	{
		return token.Span.Length;
	}

	public static int FullWidth(this SyntaxToken token)
	{
		return token.FullSpan.Length;
	}

	public static SyntaxToken FindTokenFromEnd(this SyntaxNode root, int position, bool includeZeroWidth = true, bool findInsideTrivia = false)
	{
		SyntaxToken result = root.FindToken(position, findInsideTrivia);
		SyntaxToken previousToken = result.GetPreviousToken(includeZeroWidth, findInsideTrivia);
		if (result.SpanStart == position && previousToken.Kind != SyntaxKind.None && previousToken.Span.End == position)
		{
			return previousToken;
		}
		return result;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "includeDocumentationComments")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "includeDirectives")]
	public static SyntaxToken GetNextTokenOrEndOfFile(this SyntaxToken token, bool includeZeroWidth = false, bool includeSkipped = false, bool includeDirectives = false, bool includeDocumentationComments = false)
	{
		SyntaxToken nextToken = token.GetNextToken(includeZeroWidth, includeSkipped);
		if (nextToken.Kind != SyntaxKind.None)
		{
			return nextToken;
		}
		return ((CompilationUnitSyntax)token.Parent.SyntaxTree.GetRoot(CancellationToken.None)).GetEndOfFileToken();
	}

	public static SyntaxToken WithoutTrivia(this SyntaxToken token)
	{
		if (!token.LeadingTrivia.Any() && !token.TrailingTrivia.Any())
		{
			return token;
		}
		return token.With(default, default);
	}

	public static SyntaxToken With(this SyntaxToken token, SyntaxTriviaList leading, SyntaxTriviaList trailing)
	{
		return token.WithLeadingTrivia(leading).WithTrailingTrivia(trailing);
	}

	public static SyntaxToken WithPrependedLeadingTrivia(this SyntaxToken token, params SyntaxTrivia[] trivia)
	{
		if (trivia.Length == 0)
		{
			return token;
		}
		return token.WithPrependedLeadingTrivia((IEnumerable<SyntaxTrivia>)trivia);
	}

	public static SyntaxToken WithPrependedLeadingTrivia(this SyntaxToken token, SyntaxTriviaList trivia)
	{
		if (trivia.Count == 0)
		{
			return token;
		}
		return token.WithLeadingTrivia(trivia.Concat(token.LeadingTrivia));
	}

	public static SyntaxToken WithPrependedLeadingTrivia(this SyntaxToken token, IEnumerable<SyntaxTrivia> trivia)
	{
		SyntaxTriviaList trivia2 = default(SyntaxTriviaList).AddRange(trivia);
		return token.WithPrependedLeadingTrivia(trivia2);
	}

	public static SyntaxToken WithAppendedTrailingTrivia(this SyntaxToken token, params SyntaxTrivia[] trivia)
	{
		return token.WithAppendedTrailingTrivia((IEnumerable<SyntaxTrivia>)trivia);
	}

	public static SyntaxToken WithAppendedTrailingTrivia(this SyntaxToken token, IEnumerable<SyntaxTrivia> trivia)
	{
		return token.WithTrailingTrivia(token.TrailingTrivia.Concat(trivia));
	}

	public static bool IsVarSectionVarKeyword(this SyntaxToken token)
	{
		if (token.Kind == SyntaxKind.VarKeyword)
		{
			return token.Parent.Kind == SyntaxKind.VarSection;
		}
		return false;
	}

	public static bool IsGlobalVarSectionVarKeyword(this SyntaxToken token)
	{
		if (token.IsVarSectionVarKeyword())
		{
			return token.Parent.Parent is ObjectSyntax;
		}
		return false;
	}

	public static bool IsAfterAttribute(this SyntaxToken currentToken)
	{
		if (currentToken.Kind == SyntaxKind.CloseBracketToken)
		{
			return currentToken.Parent.Kind == SyntaxKind.MemberAttribute;
		}
		return false;
	}

	public static bool IsScopeMember(this SyntaxNode node)
	{
		switch (node?.Kind)
		{
		case SyntaxKind.TriggerDeclaration:
		case SyntaxKind.MethodDeclaration:
			return true;
		default:
			return false;
		}
	}

	public static bool IsStartOfCurlyBraceBlock(this SyntaxToken token)
	{
		return token.Kind == SyntaxKind.OpenBraceToken;
	}

	public static bool IsLastTokenOfGlobalVarSection(this SyntaxToken token)
	{
		if (token.Kind == SyntaxKind.SemicolonToken && token.Parent.Kind == SyntaxKind.VariableDeclaration)
		{
			SyntaxNode parent = token.Parent;
			if (parent != null)
			{
				SyntaxKind? syntaxKind = parent.Parent?.Kind;
				SyntaxKind syntaxKind2 = SyntaxKind.VarSection;
				if (syntaxKind.GetValueOrDefault() == syntaxKind2 && syntaxKind.HasValue && ((VarSectionSyntax)token.Parent.Parent).Variables.Last() == token.Parent)
				{
					return token.Parent.Parent.Parent is ObjectSyntax;
				}
			}
		}
		return false;
	}

	public static bool IsParentPageMoveChange(this SyntaxToken token)
	{
		SyntaxNode parent = token.Parent;
		if (parent != null)
		{
			if (parent is ControlMoveChangeSyntax controlMoveChangeSyntax)
			{
				ControlMoveChangeSyntax controlMoveChangeSyntax2 = controlMoveChangeSyntax;
				if (controlMoveChangeSyntax2.Kind.IsMoveChange())
				{
					return controlMoveChangeSyntax2.ChangeKeyword.Kind.IsPageMoveChangeKeyword();
				}
			}
			if (parent is ActionMoveChangeSyntax actionMoveChangeSyntax)
			{
				ActionMoveChangeSyntax actionMoveChangeSyntax2 = actionMoveChangeSyntax;
				if (actionMoveChangeSyntax2.Kind.IsMoveChange())
				{
					return actionMoveChangeSyntax2.ChangeKeyword.Kind.IsPageMoveChangeKeyword();
				}
			}
		}
		return false;
	}

	public static bool IsParentLabelRename(this SyntaxToken token)
	{
		return token.ParentIsKind(SyntaxKind.ReportLabelMultilanguage);
	}
}
