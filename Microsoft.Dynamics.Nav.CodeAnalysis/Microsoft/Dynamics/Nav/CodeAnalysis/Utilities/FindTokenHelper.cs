using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class FindTokenHelper
{
	public static SyntaxToken FindTokenOnRightOfPosition<TRoot>(this SyntaxNode root, int position, Func<SyntaxTriviaList, int, SyntaxToken> skippedTokenFinder, bool includeSkipped = false) where TRoot : SyntaxNode
	{
		Func<SyntaxTriviaList, int, SyntaxToken> func = skippedTokenFinder ?? ((Func<SyntaxTriviaList, int, SyntaxToken>)((SyntaxTriviaList l, int p) => default));
		SyntaxToken syntaxToken = GetInitialToken<TRoot>(root, position, includeSkipped);
		if (position < syntaxToken.SpanStart)
		{
			SyntaxToken syntaxToken2 = func(syntaxToken.LeadingTrivia, position);
			syntaxToken = ((syntaxToken2.Kind != SyntaxKind.None) ? syntaxToken2 : syntaxToken);
		}
		else if (syntaxToken.Span.End <= position)
		{
			do
			{
				SyntaxToken syntaxToken3 = func(syntaxToken.TrailingTrivia, position);
				syntaxToken = ((syntaxToken3.Kind != SyntaxKind.None) ? syntaxToken3 : syntaxToken.GetNextToken(includeZeroWidth: false, includeSkipped));
			}
			while (syntaxToken.Kind != SyntaxKind.None && syntaxToken.Span.End <= position && syntaxToken.Span.End <= root.FullSpan.End);
		}
		if (syntaxToken.Span.Length == 0)
		{
			syntaxToken = syntaxToken.GetNextToken();
		}
		return syntaxToken;
	}

	public static SyntaxToken FindTokenOnLeftOfPosition<TRoot>(this SyntaxNode root, int position, Func<SyntaxTriviaList, int, SyntaxToken> skippedTokenFinder, bool includeSkipped = false) where TRoot : SyntaxNode
	{
		Func<SyntaxTriviaList, int, SyntaxToken> func = skippedTokenFinder ?? ((Func<SyntaxTriviaList, int, SyntaxToken>)((SyntaxTriviaList l, int p) => default));
		SyntaxToken syntaxToken = GetInitialToken<TRoot>(root, position, includeSkipped);
		if (position <= syntaxToken.SpanStart)
		{
			do
			{
				SyntaxToken syntaxToken2 = func(syntaxToken.LeadingTrivia, position);
				syntaxToken = ((syntaxToken2.Kind != SyntaxKind.None) ? syntaxToken2 : syntaxToken.GetPreviousToken(includeZeroWidth: false, includeSkipped));
			}
			while (position <= syntaxToken.SpanStart && root.FullSpan.Start < syntaxToken.SpanStart);
		}
		else if (syntaxToken.Span.End < position)
		{
			SyntaxToken syntaxToken3 = func(syntaxToken.TrailingTrivia, position);
			syntaxToken = ((syntaxToken3.Kind != SyntaxKind.None) ? syntaxToken3 : syntaxToken);
		}
		if (syntaxToken.Span.Length == 0)
		{
			syntaxToken = syntaxToken.GetPreviousToken();
		}
		return syntaxToken;
	}

	public static SyntaxToken FindSkippedTokenBackward(IEnumerable<SyntaxToken> skippedTokenList, int position)
	{
		SyntaxToken result = default;
		foreach (SyntaxToken skippedToken in skippedTokenList)
		{
			if (skippedToken.Span.Length > 0 && skippedToken.SpanStart <= position)
			{
				result = skippedToken;
			}
		}
		return result;
	}

	public static SyntaxToken FindSkippedTokenForward(IEnumerable<SyntaxToken> skippedTokenList, int position)
	{
		SyntaxToken syntaxToken = skippedTokenList.FirstOrDefault((SyntaxToken skipped) => skipped.Span.Length > 0 && position <= skipped.Span.End);
		if (syntaxToken != default(SyntaxToken))
		{
			return syntaxToken;
		}
		return default;
	}

	private static SyntaxToken GetInitialToken<TRoot>(SyntaxNode root, int position, bool includeSkipped = false, bool includeDirectives = false, bool includeDocumentationComments = false) where TRoot : SyntaxNode
	{
		if (position >= root.FullSpan.End && root is TRoot)
		{
			return root.GetLastToken(includeZeroWidth: true, includeSkipped: true).GetPreviousToken(includeZeroWidth: false, includeSkipped);
		}
		return root.FindToken(position, includeSkipped | includeDirectives | includeDocumentationComments);
	}
}
