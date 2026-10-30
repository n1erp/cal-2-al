using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundNullCoalescingOperator : BoundExpression
{
	public BoundExpression LeftOperand { get; }

	public BoundExpression RightOperand { get; }

	public Conversion LeftConversion { get; }

	public BoundNullCoalescingOperator(SyntaxNode syntax, BoundExpression leftOperand, BoundExpression rightOperand, Conversion leftConversion, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.NullCoalescingOperator, syntax, type, hasErrors || leftOperand.HasErrors() || rightOperand.HasErrors())
	{
		LeftOperand = leftOperand;
		RightOperand = rightOperand;
		LeftConversion = leftConversion;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitNullCoalescingOperator(this);
	}

	public BoundNullCoalescingOperator Update(BoundExpression leftOperand, BoundExpression rightOperand, Conversion leftConversion, TypeSymbol type)
	{
		if (leftOperand != LeftOperand || rightOperand != RightOperand || leftConversion != LeftConversion || type != Type)
		{
			return new BoundNullCoalescingOperator(Syntax, leftOperand, rightOperand, leftConversion, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
