using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

internal abstract class AbstractSyntaxNavigator
{
	private const int None = 0;

	protected abstract Func<SyntaxTrivia, bool> GetStepIntoFunction(bool skipped);

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

	internal SyntaxToken GetFirstToken(SyntaxNode current, bool includeZeroWidth, bool includeSkipped)
	{
		return GetFirstToken(current, GetPredicateFunction(includeZeroWidth), GetStepIntoFunction(includeSkipped));
	}

	internal SyntaxToken GetLastToken(SyntaxNode current, bool includeZeroWidth, bool includeSkipped)
	{
		return GetLastToken(current, GetPredicateFunction(includeZeroWidth), GetStepIntoFunction(includeSkipped));
	}

	internal SyntaxToken GetPreviousToken(SyntaxToken current, bool includeZeroWidth, bool includeSkipped)
	{
		return GetPreviousToken(current, GetPredicateFunction(includeZeroWidth), GetStepIntoFunction(includeSkipped));
	}

	internal SyntaxToken GetNextToken(SyntaxToken current, bool includeZeroWidth, bool includeSkipped)
	{
		return GetNextToken(current, GetPredicateFunction(includeZeroWidth), GetStepIntoFunction(includeSkipped));
	}

	internal SyntaxToken GetPreviousToken(SyntaxToken current, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		return GetPreviousToken(current, predicate, stepInto != null, stepInto);
	}

	internal SyntaxToken GetNextToken(SyntaxToken current, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		return GetNextToken(current, predicate, stepInto != null, stepInto);
	}

	internal SyntaxToken GetFirstToken(SyntaxNode current, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		foreach (SyntaxNodeOrToken item in current.ChildNodesAndTokens())
		{
			SyntaxToken result = (item.IsToken ? GetFirstToken(item.AsToken(), predicate, stepInto) : GetFirstToken(item.AsNode(), predicate, stepInto));
			if (result.Kind != SyntaxKind.None)
			{
				return result;
			}
		}
		return default;
	}

	internal SyntaxToken GetLastToken(SyntaxNode current, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		foreach (SyntaxNodeOrToken item in current.ChildNodesAndTokens().Reverse())
		{
			SyntaxToken result = (item.IsToken ? GetLastToken(item.AsToken(), predicate, stepInto) : GetLastToken(item.AsNode(), predicate, stepInto));
			if (result.Kind != SyntaxKind.None)
			{
				return result;
			}
		}
		return default;
	}

	private SyntaxToken GetFirstToken(SyntaxTriviaList triviaList, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		DebugAssertHelper.Assert(stepInto != null);
		foreach (SyntaxTrivia item in triviaList)
		{
			if (item.HasStructure && stepInto(item))
			{
				SyntaxNode structure = item.GetStructure();
				SyntaxToken firstToken = GetFirstToken(structure, predicate, stepInto);
				if (firstToken.Kind != SyntaxKind.None)
				{
					return firstToken;
				}
			}
		}
		return default;
	}

	private SyntaxToken GetLastToken(SyntaxTriviaList list, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		DebugAssertHelper.Assert(stepInto != null);
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
		return token.Kind != SyntaxKind.None;
	}

	private SyntaxToken GetFirstToken(SyntaxToken token, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		if (stepInto != null)
		{
			SyntaxToken firstToken = GetFirstToken(token.LeadingTrivia, predicate, stepInto);
			if (firstToken.Kind != SyntaxKind.None)
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
			if (firstToken2.Kind != SyntaxKind.None)
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
			if (lastToken.Kind != SyntaxKind.None)
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
			if (lastToken2.Kind != SyntaxKind.None)
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
		if (nextToken.Kind != SyntaxKind.None)
		{
			return nextToken;
		}
		if (returnNext && (predicate == null || predicate == SyntaxToken.Any || predicate(current.Token)))
		{
			return current.Token;
		}
		nextToken = GetNextToken(current, current.Token.TrailingTrivia, predicate, stepInto, ref returnNext);
		if (nextToken.Kind != SyntaxKind.None)
		{
			return nextToken;
		}
		return GetNextToken(current.Token, predicate, searchInsideCurrentTokenTrailingTrivia: false, stepInto);
	}

	internal SyntaxToken GetPreviousToken(SyntaxTrivia current, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto)
	{
		bool returnPrevious = false;
		SyntaxToken previousToken = GetPreviousToken(current, current.Token.TrailingTrivia, predicate, stepInto, ref returnPrevious);
		if (previousToken.Kind != SyntaxKind.None)
		{
			return previousToken;
		}
		if (returnPrevious && Matches(predicate, current.Token))
		{
			return current.Token;
		}
		previousToken = GetPreviousToken(current, current.Token.LeadingTrivia, predicate, stepInto, ref returnPrevious);
		if (previousToken.Kind != SyntaxKind.None)
		{
			return previousToken;
		}
		return GetPreviousToken(current.Token, predicate, searchInsideCurrentTokenLeadingTrivia: false, stepInto);
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "stepInto", Justification = "by design for now")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "predicate", Justification = "by design for now")]
	private static SyntaxToken GetNextToken(SyntaxTrivia current, SyntaxTriviaList list, Func<SyntaxToken, bool> predicate, Func<SyntaxTrivia, bool> stepInto, ref bool returnNext)
	{
		foreach (SyntaxTrivia item in list)
		{
			if (!returnNext && item == current)
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
						if (firstToken.Kind != SyntaxKind.None)
						{
							return firstToken;
						}
					}
					else
					{
						SyntaxToken firstToken2 = GetFirstToken(item.AsNode(), predicate, stepInto);
						if (firstToken2.Kind != SyntaxKind.None)
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
						if (lastToken.Kind != SyntaxKind.None)
						{
							return lastToken;
						}
					}
					else
					{
						SyntaxToken lastToken2 = GetLastToken(item.AsNode(), predicate, stepInto);
						if (lastToken2.Kind != SyntaxKind.None)
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

	internal SyntaxToken GetNextToken(SyntaxToken current, Func<SyntaxToken, bool> predicate, bool searchInsideCurrentTokenTrailingTrivia, Func<SyntaxTrivia, bool> stepInto)
	{
		DebugAssertHelper.Assert(!searchInsideCurrentTokenTrailingTrivia || stepInto != null);
		if (current.Parent != null)
		{
			if (searchInsideCurrentTokenTrailingTrivia)
			{
				SyntaxToken firstToken = GetFirstToken(current.TrailingTrivia, predicate, stepInto);
				if (firstToken.Kind != SyntaxKind.None)
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
						if (firstToken2.Kind != SyntaxKind.None)
						{
							return firstToken2;
						}
					}
					else
					{
						SyntaxToken firstToken3 = GetFirstToken(item.AsNode(), predicate, stepInto);
						if (firstToken3.Kind != SyntaxKind.None)
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

	internal SyntaxToken GetPreviousToken(SyntaxToken current, Func<SyntaxToken, bool> predicate, bool searchInsideCurrentTokenLeadingTrivia, Func<SyntaxTrivia, bool> stepInto)
	{
		DebugAssertHelper.Assert(!searchInsideCurrentTokenLeadingTrivia || stepInto != null);
		if (current.Parent != null)
		{
			if (searchInsideCurrentTokenLeadingTrivia)
			{
				SyntaxToken lastToken = GetLastToken(current.LeadingTrivia, predicate, stepInto);
				if (lastToken.Kind != SyntaxKind.None)
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
						if (lastToken2.Kind != SyntaxKind.None)
						{
							return lastToken2;
						}
					}
					else
					{
						SyntaxToken lastToken3 = GetLastToken(item.AsNode(), predicate, stepInto);
						if (lastToken3.Kind != SyntaxKind.None)
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
