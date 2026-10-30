using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.Syntax;

public sealed class PrefixUnaryExpressionSyntax : ExpressionSyntax
{
	private ExpressionSyntax operand;

	public SyntaxToken OperatorToken => new SyntaxToken(this, ((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.PrefixUnaryExpressionSyntax)Green).operatorToken, Position, 0);

	public ExpressionSyntax Operand => GetRed(ref operand, 1);

	internal PrefixUnaryExpressionSyntax(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.CSharpSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		if (index == 1)
		{
			return GetRed(ref operand, 1);
		}
		return null;
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		if (index == 1)
		{
			return operand;
		}
		return null;
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitPrefixUnaryExpression(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitPrefixUnaryExpression(this);
	}

	public PrefixUnaryExpressionSyntax Update(SyntaxToken operatorToken, ExpressionSyntax operand)
	{
		if (operatorToken != OperatorToken || operand != Operand)
		{
			PrefixUnaryExpressionSyntax prefixUnaryExpressionSyntax = SyntaxFactory.PrefixUnaryExpression(Kind(), operatorToken, operand);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return prefixUnaryExpressionSyntax.WithAnnotations(annotations);
			}
			return prefixUnaryExpressionSyntax;
		}
		return this;
	}

	public PrefixUnaryExpressionSyntax WithOperatorToken(SyntaxToken operatorToken)
	{
		return Update(operatorToken, Operand);
	}

	public PrefixUnaryExpressionSyntax WithOperand(ExpressionSyntax operand)
	{
		return Update(OperatorToken, operand);
	}
}
