using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundSwitchLabel : BoundNode
{
	public LabelSymbol Label { get; }

	public BoundExpression ExpressionOpt { get; }

	public ConstantValue ConstantValueOpt { get; }

	public BoundSwitchLabel(SyntaxNode syntax, LabelSymbol label, BoundExpression expressionOpt, ConstantValue constantValueOpt, bool hasErrors = false)
		: base(BoundKind.SwitchLabel, syntax, hasErrors || expressionOpt.HasErrors())
	{
		Label = label;
		ExpressionOpt = expressionOpt;
		ConstantValueOpt = constantValueOpt;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitSwitchLabel(this);
	}

	public BoundSwitchLabel Update(LabelSymbol label, BoundExpression expressionOpt, ConstantValue constantValueOpt)
	{
		if (label != Label || expressionOpt != ExpressionOpt || constantValueOpt != ConstantValueOpt)
		{
			return new BoundSwitchLabel(Syntax, label, expressionOpt, constantValueOpt, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
