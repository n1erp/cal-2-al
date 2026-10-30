using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal static class FormattingRangeHelper
{
	public static (SyntaxToken, SyntaxToken)? FindAppropriateRange(SyntaxToken endToken, bool useDefaultRange = true)
	{
		Contract.ThrowIfTrue(endToken.Kind == SyntaxKind.None);
		return FixupOpenBrace(FindAppropriateRangeWorker(endToken, useDefaultRange));
	}

	private static (SyntaxToken, SyntaxToken)? FixupOpenBrace((SyntaxToken, SyntaxToken)? tokenRange)
	{
		if (!tokenRange.HasValue)
		{
			return tokenRange;
		}
		SyntaxToken item = tokenRange.Value.Item1;
		SyntaxToken previousToken = item.GetPreviousToken();
		while (item.Kind != SyntaxKind.CloseBraceToken && previousToken.Kind == SyntaxKind.OpenBraceToken)
		{
			(SyntaxToken, SyntaxToken) bracePair = previousToken.Parent.GetBracePair();
			if (bracePair.Item2.Kind == SyntaxKind.None || !AreTwoTokensOnSameLine(previousToken, bracePair.Item2))
			{
				return ValueTuple.Create(item, tokenRange.Value.Item2);
			}
			item = previousToken;
			previousToken = item.GetPreviousToken();
		}
		return ValueTuple.Create(item, tokenRange.Value.Item2);
	}

	private static (SyntaxToken, SyntaxToken)? FindAppropriateRangeWorker(SyntaxToken endToken, bool useDefaultRange)
	{
		switch (endToken.Kind)
		{
		case SyntaxKind.CloseBraceToken:
			return FindAppropriateRangeForCloseBrace(endToken);
		case SyntaxKind.SemicolonToken:
			return FindAppropriateRangeForSemicolon(endToken);
		case SyntaxKind.ColonToken:
			return FindAppropriateRangeForColon(endToken);
		default:
		{
			if (!useDefaultRange)
			{
				return null;
			}
			if (endToken.Kind == SyntaxKind.SkippedTokensTrivia)
			{
				return null;
			}
			SyntaxNode parent = endToken.Parent;
			if (parent == null)
			{
				return null;
			}
			return ValueTuple.Create(GetAppropriatePreviousToken(parent.GetFirstToken()), parent.GetLastToken());
		}
		}
	}

	private static (SyntaxToken, SyntaxToken)? FindAppropriateRangeForSemicolon(SyntaxToken endToken)
	{
		_ = endToken.Parent?.Kind;
		return null;
	}

	private static (SyntaxToken, SyntaxToken)? FindAppropriateRangeForCloseBrace(SyntaxToken endToken)
	{
		SyntaxNode parent = endToken.Parent;
		if (parent == null || parent.Kind == SyntaxKind.SkippedTokensTrivia)
		{
			return null;
		}
		if (parent is BlockSyntax)
		{
			return ValueTuple.Create(GetPreviousTokenIfNotFirstTokenInTree(parent.GetFirstToken()), parent.GetLastToken());
		}
		return null;
	}

	private static (SyntaxToken, SyntaxToken)? FindAppropriateRangeForColon(SyntaxToken endToken)
	{
		_ = endToken.Parent?.Kind;
		return null;
	}

	private static SyntaxToken GetPreviousTokenIfNotFirstTokenInTree(SyntaxToken token)
	{
		SyntaxToken previousToken = token.GetPreviousToken();
		if (previousToken.Kind != SyntaxKind.None)
		{
			return previousToken;
		}
		return token;
	}

	public static bool AreTwoTokensOnSameLine(SyntaxToken token1, SyntaxToken token2)
	{
		SyntaxTree syntaxTree = token1.SyntaxTree;
		if (syntaxTree != null && syntaxTree.TryGetText(out var text))
		{
			return text.AreOnSameLine(token1, token2);
		}
		return CommonFormattingHelpers.GetTextBetween(token1, token2).ContainsLineBreak();
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "canTokenBeFirstInABlock")]
	private static SyntaxToken GetAppropriatePreviousToken(SyntaxToken startToken, bool canTokenBeFirstInABlock = false)
	{
		SyntaxToken previousToken = startToken.GetPreviousToken();
		if (previousToken.Kind == SyntaxKind.None)
		{
			return startToken;
		}
		if (AreTwoTokensOnSameLine(previousToken, startToken))
		{
			return startToken;
		}
		return previousToken;
	}
}
