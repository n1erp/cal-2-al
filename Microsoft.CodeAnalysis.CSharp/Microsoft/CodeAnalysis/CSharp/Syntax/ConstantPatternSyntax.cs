using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class ConstantPatternSyntax : PatternSyntax
{
	private ExpressionSyntax expression;

	public ExpressionSyntax Expression => GetRedAtZero(ref expression);

	internal ConstantPatternSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 0)
		{
			return GetRedAtZero(ref expression);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 0)
		{
			return expression;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitConstantPattern(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitConstantPattern(this);
	}

	public ConstantPatternSyntax Update(ExpressionSyntax expression)
	{
		if (expression != Expression)
		{
			ConstantPatternSyntax constantPatternSyntax = SyntaxFactory.ConstantPattern(expression);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return constantPatternSyntax.WithAnnotations(annotations);
			}
			return constantPatternSyntax;
		}
		return this;
	}

	public ConstantPatternSyntax WithExpression(ExpressionSyntax expression)
	{
		return Update(expression);
	}
}
