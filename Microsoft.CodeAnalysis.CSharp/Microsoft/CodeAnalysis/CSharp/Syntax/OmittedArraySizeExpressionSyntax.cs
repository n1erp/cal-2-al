using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class OmittedArraySizeExpressionSyntax : ExpressionSyntax
{
	public SyntaxToken OmittedArraySizeExpressionToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OmittedArraySizeExpressionSyntax)Green).omittedArraySizeExpressionToken, Position, 0);

	internal OmittedArraySizeExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
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
		return visitor.VisitOmittedArraySizeExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitOmittedArraySizeExpression(this);
	}

	public OmittedArraySizeExpressionSyntax Update(SyntaxToken omittedArraySizeExpressionToken)
	{
		if (omittedArraySizeExpressionToken != OmittedArraySizeExpressionToken)
		{
			OmittedArraySizeExpressionSyntax omittedArraySizeExpressionSyntax = SyntaxFactory.OmittedArraySizeExpression(omittedArraySizeExpressionToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return omittedArraySizeExpressionSyntax.WithAnnotations(annotations);
			}
			return omittedArraySizeExpressionSyntax;
		}
		return this;
	}

	public OmittedArraySizeExpressionSyntax WithOmittedArraySizeExpressionToken(SyntaxToken omittedArraySizeExpressionToken)
	{
		return Update(omittedArraySizeExpressionToken);
	}
}
