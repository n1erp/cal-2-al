using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class SkippedTokensTriviaSyntax : StructuredTriviaSyntax
{
	private SyntaxNode _tokens;

	public SyntaxNode Tokens => GetRedAtZero(ref _tokens);

	internal SkippedTokensTriviaSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref _tokens);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return _tokens;
		}
		return null;
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitSkippedTokensTrivia(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitSkippedTokensTrivia(this);
	}

	public SkippedTokensTriviaSyntax Update(SyntaxNode tokens)
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

	public SkippedTokensTriviaSyntax WithTokens(SyntaxNode tokens)
	{
		return Update(tokens);
	}
}
