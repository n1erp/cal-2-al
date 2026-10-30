using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class SkippedTokensTriviaSyntax : StructuredTriviaSyntax, ISkippedTokensTriviaSyntax
{
	public SyntaxTokenList Tokens
	{
		get
		{
			GreenNode slot = Green.GetSlot(0);
			if (slot != null)
			{
				return new SyntaxTokenList(this, slot, Position, 0);
			}
			return default;
		}
	}

	internal SkippedTokensTriviaSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSkippedTokensTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitSkippedTokensTrivia(this);
	}

	public SkippedTokensTriviaSyntax Update(SyntaxTokenList tokens)
	{
		if (tokens != Tokens)
		{
			SkippedTokensTriviaSyntax skippedTokensTriviaSyntax = SyntaxFactory.SkippedTokensTrivia(tokens);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return skippedTokensTriviaSyntax.WithAnnotations(annotations);
			}
			return skippedTokensTriviaSyntax;
		}
		return this;
	}

	public SkippedTokensTriviaSyntax WithTokens(SyntaxTokenList tokens)
	{
		return Update(tokens);
	}

	public SkippedTokensTriviaSyntax AddTokens(params SyntaxToken[] items)
	{
		return WithTokens(Tokens.AddRange(items));
	}
}
