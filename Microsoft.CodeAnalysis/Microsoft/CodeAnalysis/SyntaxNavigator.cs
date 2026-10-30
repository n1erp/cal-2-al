using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis;

internal sealed class SyntaxNavigator
{
	[Flags]
	private enum SyntaxKinds
	{
		DocComments = 1,
		Directives = 2,
		SkippedTokens = 4
	}

	private const int None = 0;

	public static readonly SyntaxNavigator Instance = new SyntaxNavigator();

	private static readonly Func<SyntaxTrivia, bool>[] s_stepIntoFunctions = new Func<SyntaxTrivia, bool>[8]
	{
		null,
		(SyntaxTrivia t) => t.IsDocumentationCommentTrivia,
		(SyntaxTrivia t) => t.IsDirective,
		(SyntaxTrivia t) => t.IsDirective || t.IsDocumentationCommentTrivia,
		(SyntaxTrivia t) => t.IsSkippedTokensTrivia,
		(SyntaxTrivia t) => t.IsSkippedTokensTrivia || t.IsDocumentationCommentTrivia,
		(SyntaxTrivia t) => t.IsSkippedTokensTrivia || t.IsDirective,
		(SyntaxTrivia t) => t.IsSkippedTokensTrivia || t.IsDirective || t.IsDocumentationCommentTrivia
	};

	private static readonly ObjectPool<Stack<ChildSyntaxList.Enumerator>> s_childEnumeratorStackPool = new ObjectPool<Stack<ChildSyntaxList.Enumerator>>(() => new Stack<ChildSyntaxList.Enumerator>(), 10);

	private static readonly ObjectPool<Stack<ChildSyntaxList.Reversed.Enumerator>> s_childReversedEnumeratorStackPool = new ObjectPool<Stack<ChildSyntaxList.Reversed.Enumerator>>(() => new Stack<ChildSyntaxList.Reversed.Enumerator>(), 10);

	private SyntaxNavigator()
	{
	}

	private static Func<SyntaxTrivia, bool> GetStepIntoFunction(bool skipped, bool directives, bool docComments)
	{
		SyntaxKinds syntaxKinds = (SyntaxKinds)((skipped ? 4 : 0) | (directives ? 2 : 0) | (docComments ? 1 : 0));
		return s_stepIntoFunctions[(int)syntaxKinds];
	}

	private static Func<SyntaxToken, bool> GetPredicateFunction(bool includeZeroWidth)
	{
		if (!includeZeroWidth)
		{
			return SyntaxToken.NonZeroWidth;
		}
		return SyntaxToken.Any;
	}

	private static bool Matches(Func<SyntaxToken, bool> predicate, SyntaxToken token)
	{
		if (predicate != null && (object)predicate != SyntaxToken.Any)
		{
			return predicate(token);
		}
		return true;
	}

	internal SyntaxToken GetFirstToken(in SyntaxNode current, bool includeZeroWidth, bool includeSkipped, bool includeDirectives, bool includeDocumentationComments)
	{
		return GetFirstToken(current, GetPredicateFunction(includeZeroWidth), GetStepIntoFunction(includeSkipped, includeDirectives, includeDocumentationComments));
	}

	internal SyntaxToken GetLastToken(in SyntaxNode current, bool includeZeroWidth, bool includeSkipped, bool includeDirectives, bool includeDocumentationComments)
	{
		return GetLastToken(current, GetPredicateFunction(includeZeroWidth), GetStepIntoFunction(includeSkipped, includeDirectives, includeDocumentationComments));
	}

	internal SyntaxToken GetPreviousToken(in SyntaxToken current, bool includeZeroWidth, bool includeSkipped, bool includeDirectives, bool includeDocumentationComments)
	{
		return GetPreviousToken(in current, GetPredicateFunction(includeZeroWidth), GetStepIntoFunction(includeSkipped, includeDirectives, includeDocumentationComments));
	}

	internal SyntaxToken GetNextToken(in SyntaxToken current, bool includeZeroWidth, bool includeSkipped, bool includeDirectives, bool includeDocumentationComments)
	{
		return GetNextToken(in current, GetPredicateFunction(includeZeroWidth), GetStepIntoFunction(includeSkipped, includeDirectives, includeDocumentationComments));
	}

	internal SyntaxToken GetPreviousToken(in SyntaxToken current, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		return GetPreviousToken(in current, predicate, stepInto != null, stepInto);
	}

	internal SyntaxToken GetNextToken(in SyntaxToken current, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		return GetNextToken(in current, predicate, stepInto != null, stepInto);
	}

	internal SyntaxToken GetFirstToken(SyntaxNode current, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		Stack<ChildSyntaxList.Enumerator> stack = s_childEnumeratorStackPool.Allocate();
		try
		{
			stack.Push(current.ChildNodesAndTokens().GetEnumerator());
			while (stack.Count > 0)
			{
				ChildSyntaxList.Enumerator item = stack.Pop();
				if (!item.MoveNext())
				{
					continue;
				}
				SyntaxNodeOrToken current2 = item.Current;
				if (current2.IsToken)
				{
					SyntaxToken firstToken = GetFirstToken(current2.AsToken(), predicate, stepInto);
					if (firstToken.RawKind != 0)
					{
						return firstToken;
					}
				}
				stack.Push(item);
				if (current2.IsNode)
				{
					stack.Push(current2.AsNode().ChildNodesAndTokens().GetEnumerator());
				}
			}
			return default;
		}
		finally
		{
			stack.Clear();
			s_childEnumeratorStackPool.Free(stack);
		}
	}

	internal SyntaxToken GetLastToken(SyntaxNode current, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		Stack<ChildSyntaxList.Reversed.Enumerator> stack = s_childReversedEnumeratorStackPool.Allocate();
		try
		{
			stack.Push(current.ChildNodesAndTokens().Reverse().GetEnumerator());
			while (stack.Count > 0)
			{
				ChildSyntaxList.Reversed.Enumerator item = stack.Pop();
				if (!item.MoveNext())
				{
					continue;
				}
				SyntaxNodeOrToken current2 = item.Current;
				if (current2.IsToken)
				{
					SyntaxToken lastToken = GetLastToken(current2.AsToken(), predicate, stepInto);
					if (lastToken.RawKind != 0)
					{
						return lastToken;
					}
				}
				stack.Push(item);
				if (current2.IsNode)
				{
					stack.Push(current2.AsNode().ChildNodesAndTokens().Reverse()
						.GetEnumerator());
				}
			}
			return default;
		}
		finally
		{
			stack.Clear();
			s_childReversedEnumeratorStackPool.Free(stack);
		}
	}

	private SyntaxToken GetFirstToken(SyntaxTriviaList triviaList, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		foreach (SyntaxTrivia item in triviaList)
		{
			if (item.HasStructure && stepInto(item))
			{
				SyntaxNode structure = item.GetStructure();
				SyntaxToken firstToken = GetFirstToken(structure, predicate, stepInto);
				if (firstToken.RawKind != 0)
				{
					return firstToken;
				}
			}
		}
		return default;
	}

	private SyntaxToken GetLastToken(SyntaxTriviaList list, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		foreach (SyntaxTrivia item in list.Reverse())
		{
			if (TryGetLastTokenForStructuredTrivia(item, predicate, stepInto, out var token))
			{
				return token;
			}
		}
		return default;
	}

	private bool TryGetLastTokenForStructuredTrivia(SyntaxTrivia trivia, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto, out SyntaxToken token)
	{
		token = default;
		if (!trivia.HasStructure || stepInto == null || !stepInto(trivia))
		{
			return false;
		}
		token = GetLastToken(trivia.GetStructure(), predicate, stepInto);
		return token.RawKind != 0;
	}

	private SyntaxToken GetFirstToken(SyntaxToken token, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		if (stepInto != null)
		{
			SyntaxToken firstToken = GetFirstToken(token.LeadingTrivia, predicate, stepInto);
			if (firstToken.RawKind != 0)
			{
				return firstToken;
			}
		}
		if (Matches(predicate, token))
		{
			return token;
		}
		if (stepInto != null)
		{
			SyntaxToken firstToken2 = GetFirstToken(token.TrailingTrivia, predicate, stepInto);
			if (firstToken2.RawKind != 0)
			{
				return firstToken2;
			}
		}
		return default;
	}

	private SyntaxToken GetLastToken(SyntaxToken token, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		if (stepInto != null)
		{
			SyntaxToken lastToken = GetLastToken(token.TrailingTrivia, predicate, stepInto);
			if (lastToken.RawKind != 0)
			{
				return lastToken;
			}
		}
		if (Matches(predicate, token))
		{
			return token;
		}
		if (stepInto != null)
		{
			SyntaxToken lastToken2 = GetLastToken(token.LeadingTrivia, predicate, stepInto);
			if (lastToken2.RawKind != 0)
			{
				return lastToken2;
			}
		}
		return default;
	}

	internal SyntaxToken GetNextToken(SyntaxTrivia current, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		bool returnNext = false;
		SyntaxToken nextToken = GetNextToken(current, current.Token.LeadingTrivia, predicate, stepInto, ref returnNext);
		if (nextToken.RawKind != 0)
		{
			return nextToken;
		}
		if (returnNext && (predicate == null || predicate == SyntaxToken.Any || predicate(current.Token)))
		{
			return current.Token;
		}
		nextToken = GetNextToken(current, current.Token.TrailingTrivia, predicate, stepInto, ref returnNext);
		if (nextToken.RawKind != 0)
		{
			return nextToken;
		}
		return GetNextToken(current.Token, predicate, searchInsideCurrentTokenTrailingTrivia: false, stepInto);
	}

	internal SyntaxToken GetPreviousToken(SyntaxTrivia current, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		bool returnPrevious = false;
		SyntaxToken previousToken = GetPreviousToken(current, current.Token.TrailingTrivia, predicate, stepInto, ref returnPrevious);
		if (previousToken.RawKind != 0)
		{
			return previousToken;
		}
		if (returnPrevious && Matches(predicate, current.Token))
		{
			return current.Token;
		}
		previousToken = GetPreviousToken(current, current.Token.LeadingTrivia, predicate, stepInto, ref returnPrevious);
		if (previousToken.RawKind != 0)
		{
			return previousToken;
		}
		return GetPreviousToken(current.Token, predicate, searchInsideCurrentTokenLeadingTrivia: false, stepInto);
	}

	private SyntaxToken GetNextToken(SyntaxTrivia current, SyntaxTriviaList list, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto, ref bool returnNext)
	{
		foreach (SyntaxTrivia item in list)
		{
			if (returnNext)
			{
				if (item.HasStructure && stepInto != null && stepInto(item))
				{
					SyntaxNode structure = item.GetStructure();
					SyntaxToken firstToken = GetFirstToken(structure, predicate, stepInto);
					if (firstToken.RawKind != 0)
					{
						return firstToken;
					}
				}
			}
			else if (item == current)
			{
				returnNext = true;
			}
		}
		return default;
	}

	private SyntaxToken GetPreviousToken(SyntaxTrivia current, SyntaxTriviaList list, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto, ref bool returnPrevious)
	{
		foreach (SyntaxTrivia item in list.Reverse())
		{
			if (returnPrevious)
			{
				if (TryGetLastTokenForStructuredTrivia(item, predicate, stepInto, out var token))
				{
					return token;
				}
			}
			else if (item == current)
			{
				returnPrevious = true;
			}
		}
		return default;
	}

	internal SyntaxToken GetNextToken(SyntaxNode node, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		while (node.Parent != null)
		{
			bool flag = false;
			foreach (SyntaxNodeOrToken item in node.Parent.ChildNodesAndTokens())
			{
				if (flag)
				{
					if (item.IsToken)
					{
						SyntaxToken firstToken = GetFirstToken(item.AsToken(), predicate, stepInto);
						if (firstToken.RawKind != 0)
						{
							return firstToken;
						}
					}
					else
					{
						SyntaxToken firstToken2 = GetFirstToken(item.AsNode(), predicate, stepInto);
						if (firstToken2.RawKind != 0)
						{
							return firstToken2;
						}
					}
				}
				else if (item.IsNode && item.AsNode() == node)
				{
					flag = true;
				}
			}
			node = node.Parent;
		}
		if (node.IsStructuredTrivia)
		{
			return GetNextToken(((IStructuredTriviaSyntax)node).ParentTrivia, predicate, stepInto);
		}
		return default;
	}

	internal SyntaxToken GetPreviousToken(SyntaxNode node, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		while (node.Parent != null)
		{
			bool flag = false;
			foreach (SyntaxNodeOrToken item in node.Parent.ChildNodesAndTokens().Reverse())
			{
				if (flag)
				{
					if (item.IsToken)
					{
						SyntaxToken lastToken = GetLastToken(item.AsToken(), predicate, stepInto);
						if (lastToken.RawKind != 0)
						{
							return lastToken;
						}
					}
					else
					{
						SyntaxToken lastToken2 = GetLastToken(item.AsNode(), predicate, stepInto);
						if (lastToken2.RawKind != 0)
						{
							return lastToken2;
						}
					}
				}
				else if (item.IsNode && item.AsNode() == node)
				{
					flag = true;
				}
			}
			node = node.Parent;
		}
		if (node.IsStructuredTrivia)
		{
			return GetPreviousToken(((IStructuredTriviaSyntax)node).ParentTrivia, predicate, stepInto);
		}
		return default;
	}

	internal SyntaxToken GetNextToken(in SyntaxToken current, Func<SyntaxToken, bool> predicate, bool searchInsideCurrentTokenTrailingTrivia, Func<SyntaxTrivia, bool> stepInto)
	{
		if (current.Parent != null)
		{
			if (searchInsideCurrentTokenTrailingTrivia)
			{
				SyntaxToken firstToken = GetFirstToken(current.TrailingTrivia, predicate, stepInto);
				if (firstToken.RawKind != 0)
				{
					return firstToken;
				}
			}
			bool flag = false;
			foreach (SyntaxNodeOrToken item in current.Parent.ChildNodesAndTokens())
			{
				if (flag)
				{
					if (item.IsToken)
					{
						SyntaxToken firstToken2 = GetFirstToken(item.AsToken(), predicate, stepInto);
						if (firstToken2.RawKind != 0)
						{
							return firstToken2;
						}
					}
					else
					{
						SyntaxToken firstToken3 = GetFirstToken(item.AsNode(), predicate, stepInto);
						if (firstToken3.RawKind != 0)
						{
							return firstToken3;
						}
					}
				}
				else if (item.IsToken && item.AsToken() == current)
				{
					flag = true;
				}
			}
			return GetNextToken(current.Parent, predicate, stepInto);
		}
		return default;
	}

	internal SyntaxToken GetPreviousToken(in SyntaxToken current, Func<SyntaxToken, bool> predicate, bool searchInsideCurrentTokenLeadingTrivia, Func<SyntaxTrivia, bool> stepInto)
	{
		if (current.Parent != null)
		{
			if (searchInsideCurrentTokenLeadingTrivia)
			{
				SyntaxToken lastToken = GetLastToken(current.LeadingTrivia, predicate, stepInto);
				if (lastToken.RawKind != 0)
				{
					return lastToken;
				}
			}
			bool flag = false;
			foreach (SyntaxNodeOrToken item in current.Parent.ChildNodesAndTokens().Reverse())
			{
				if (flag)
				{
					if (item.IsToken)
					{
						SyntaxToken lastToken2 = GetLastToken(item.AsToken(), predicate, stepInto);
						if (lastToken2.RawKind != 0)
						{
							return lastToken2;
						}
					}
					else
					{
						SyntaxToken lastToken3 = GetLastToken(item.AsNode(), predicate, stepInto);
						if (lastToken3.RawKind != 0)
						{
							return lastToken3;
						}
					}
				}
				else if (item.IsToken && item.AsToken() == current)
				{
					flag = true;
				}
			}
			return GetPreviousToken(current.Parent, predicate, stepInto);
		}
		return default;
	}
}
