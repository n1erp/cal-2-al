using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class InterpolationAlignmentClauseSyntax : CSharpSyntaxNode
{
	private ExpressionSyntax value;

	public SyntaxToken CommaToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.InterpolationAlignmentClauseSyntax)Green).commaToken, Position, 0);

	public ExpressionSyntax Value => GetRed(ref value, 1);

	internal InterpolationAlignmentClauseSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref value, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return value;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitInterpolationAlignmentClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitInterpolationAlignmentClause(this);
	}

	public InterpolationAlignmentClauseSyntax Update(SyntaxToken commaToken, ExpressionSyntax value)
	{
		if (commaToken != CommaToken || value != Value)
		{
			InterpolationAlignmentClauseSyntax interpolationAlignmentClauseSyntax = SyntaxFactory.InterpolationAlignmentClause(commaToken, value);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return interpolationAlignmentClauseSyntax.WithAnnotations(annotations);
			}
			return interpolationAlignmentClauseSyntax;
		}
		return this;
	}

	public InterpolationAlignmentClauseSyntax WithCommaToken(SyntaxToken commaToken)
	{
		return Update(commaToken, Value);
	}

	public InterpolationAlignmentClauseSyntax WithValue(ExpressionSyntax value)
	{
		return Update(CommaToken, value);
	}
}
