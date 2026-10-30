using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class InterpolatedStringTextSyntax : InterpolatedStringContentSyntax
{
	public SyntaxToken TextToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.InterpolatedStringTextSyntax)Green).textToken, Position, 0);

	internal InterpolatedStringTextSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitInterpolatedStringText(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitInterpolatedStringText(this);
	}

	public InterpolatedStringTextSyntax Update(SyntaxToken textToken)
	{
		if (textToken != TextToken)
		{
			InterpolatedStringTextSyntax interpolatedStringTextSyntax = SyntaxFactory.InterpolatedStringText(textToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return interpolatedStringTextSyntax.WithAnnotations(annotations);
			}
			return interpolatedStringTextSyntax;
		}
		return this;
	}

	public InterpolatedStringTextSyntax WithTextToken(SyntaxToken textToken)
	{
		return Update(textToken);
	}
}
