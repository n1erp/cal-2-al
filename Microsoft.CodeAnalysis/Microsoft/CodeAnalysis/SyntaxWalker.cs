namespace Microsoft.CodeAnalysis;

public abstract class SyntaxWalker
{
	protected SyntaxWalkerDepth Depth { get; }

	protected SyntaxWalker(SyntaxWalkerDepth depth = SyntaxWalkerDepth.Node)
	{
		Depth = depth;
	}

	public virtual void Visit(SyntaxNode node)
	{
		foreach (SyntaxNodeOrToken item in node.ChildNodesAndTokens())
		{
			if (item.IsNode)
			{
				if (Depth >= SyntaxWalkerDepth.Node)
				{
					Visit(item.AsNode());
				}
			}
			else if (item.IsToken && Depth >= SyntaxWalkerDepth.Token)
			{
				VisitToken(item.AsToken());
			}
		}
	}

	protected virtual void VisitToken(SyntaxToken token)
	{
		if (Depth >= SyntaxWalkerDepth.Trivia)
		{
			VisitLeadingTrivia(in token);
			VisitTrailingTrivia(in token);
		}
	}

	private void VisitLeadingTrivia(in SyntaxToken token)
	{
		if (token.HasLeadingTrivia)
		{
			foreach (SyntaxTrivia leadingTrivium in token.LeadingTrivia)
			{
				VisitTrivia(leadingTrivium);
			}
		}
	}

	private void VisitTrailingTrivia(in SyntaxToken token)
	{
		if (token.HasTrailingTrivia)
		{
			foreach (SyntaxTrivia trailingTrivium in token.TrailingTrivia)
			{
				VisitTrivia(trailingTrivium);
			}
		}
	}

	protected virtual void VisitTrivia(SyntaxTrivia trivia)
	{
		if (Depth >= SyntaxWalkerDepth.StructuredTrivia && trivia.HasStructure)
		{
			Visit(trivia.GetStructure());
		}
	}
}
