using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classification;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classifiers;

internal class Worker
{
	private readonly TextSpan textSpan;

	private readonly List<ClassifiedSpan> result;

	private readonly CancellationToken cancellationToken;

	private Worker(TextSpan textSpan, List<ClassifiedSpan> result, CancellationToken cancellationToken)
	{
		this.result = result;
		this.textSpan = textSpan;
		this.cancellationToken = cancellationToken;
	}

	internal static void CollectClassifiedSpans(IEnumerable<SyntaxToken> tokens, TextSpan textSpan, List<ClassifiedSpan> result, CancellationToken cancellationToken)
	{
		Worker worker = new Worker(textSpan, result, cancellationToken);
		foreach (SyntaxToken token in tokens)
		{
			worker.ClassifyToken(token);
		}
	}

	internal static void CollectClassifiedSpans(SyntaxNode node, TextSpan textSpan, List<ClassifiedSpan> result, CancellationToken cancellationToken)
	{
		new Worker(textSpan, result, cancellationToken).ClassifyNode(node);
	}

	private void AddClassification(TextSpan span, string type)
	{
		result.Add(new ClassifiedSpan(type, span));
	}

	private void AddClassification(SyntaxTrivia trivia, string type)
	{
		if (trivia.Width > 0 && textSpan.OverlapsWith(trivia.Span))
		{
			AddClassification(trivia.Span, type);
		}
	}

	private void AddClassification(SyntaxToken token, string type)
	{
		if (token.Width > 0 && textSpan.OverlapsWith(token.Span))
		{
			AddClassification(token.Span, type);
		}
	}

	private void ClassifyNodeOrToken(SyntaxNodeOrToken nodeOrToken)
	{
		if (nodeOrToken.IsToken)
		{
			ClassifyToken(nodeOrToken.AsToken());
		}
		else
		{
			ClassifyNode(nodeOrToken.AsNode());
		}
	}

	private void ClassifyNode(SyntaxNode node)
	{
		foreach (SyntaxToken item in node.DescendantTokens(textSpan))
		{
			cancellationToken.ThrowIfCancellationRequested();
			ClassifyToken(item);
		}
	}

	private void ClassifyToken(SyntaxToken token)
	{
		TextSpan span = token.Span;
		if (span.Length != 0 && textSpan.OverlapsWith(span))
		{
			string classification = ClassificationHelpers.GetClassification(token);
			if (classification != null)
			{
				AddClassification(span, classification);
			}
		}
		foreach (SyntaxTrivia leadingTrivium in token.LeadingTrivia)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ClassifyTrivia(leadingTrivium);
		}
		foreach (SyntaxTrivia trailingTrivium in token.TrailingTrivia)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ClassifyTrivia(trailingTrivium);
		}
	}

	private void ClassifyTrivia(SyntaxTrivia trivia)
	{
		if (trivia.IsSingleLineComment())
		{
			AddClassification(trivia, "single line comment");
		}
		else if (trivia.IsMultiLineComment())
		{
			AddClassification(trivia, "multi line comment");
		}
		else if (trivia.Kind == SyntaxKind.SkippedTokensTrivia)
		{
			ClassifySkippedTokens((SkippedTokensTriviaSyntax)trivia.GetStructure());
		}
	}

	private void ClassifySkippedTokens(SkippedTokensTriviaSyntax skippedTokens)
	{
		if (textSpan.OverlapsWith(skippedTokens.Span))
		{
			SyntaxTokenList syntaxTokenList = default;
			SyntaxNode cachedSlot = skippedTokens.Tokens.GetCachedSlot(0);
			if (cachedSlot != null)
			{
				syntaxTokenList = new SyntaxTokenList(skippedTokens.Tokens, cachedSlot.Node, skippedTokens.Position, 0);
			}
			foreach (SyntaxToken item in syntaxTokenList)
			{
				ClassifyToken(item);
			}
		}
	}
}
