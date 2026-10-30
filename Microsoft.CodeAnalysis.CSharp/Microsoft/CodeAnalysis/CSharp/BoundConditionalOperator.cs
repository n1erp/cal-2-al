using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundConditionalOperator : BoundExpression
{
	public override ConstantValue ConstantValue => ConstantValueOpt;

	public bool IsDynamic
	{
		get
		{
			if (Condition.Kind == BoundKind.UnaryOperator)
			{
				return ((BoundUnaryOperator)Condition).OperatorKind.IsDynamic();
			}
			return false;
		}
	}

	public bool IsRef { get; }

	public BoundExpression Condition { get; }

	public BoundExpression Consequence { get; }

	public BoundExpression Alternative { get; }

	public ConstantValue ConstantValueOpt { get; }

	public BoundConditionalOperator(SyntaxNode syntax, bool isRef, BoundExpression condition, BoundExpression consequence, BoundExpression alternative, ConstantValue constantValueOpt, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.ConditionalOperator, syntax, type, hasErrors || condition.HasErrors() || consequence.HasErrors() || alternative.HasErrors())
	{
		IsRef = isRef;
		Condition = condition;
		Consequence = consequence;
		Alternative = alternative;
		ConstantValueOpt = constantValueOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitConditionalOperator(this);
	}

	public BoundConditionalOperator Update(bool isRef, BoundExpression condition, BoundExpression consequence, BoundExpression alternative, ConstantValue constantValueOpt, TypeSymbol type)
	{
		if (isRef != IsRef || condition != Condition || consequence != Consequence || alternative != Alternative || constantValueOpt != ConstantValueOpt || type != Type)
		{
			return new BoundConditionalOperator(Syntax, isRef, condition, consequence, alternative, constantValueOpt, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
