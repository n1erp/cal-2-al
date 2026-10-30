using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundConditionalGoto : BoundStatement
{
	public BoundExpression Condition { get; }

	public bool JumpIfTrue { get; }

	public LabelSymbol Label { get; }

	public BoundConditionalGoto(SyntaxNode syntax, BoundExpression condition, bool jumpIfTrue, LabelSymbol label, bool hasErrors = false)
		: base(BoundKind.ConditionalGoto, syntax, hasErrors || condition.HasErrors())
	{
		Condition = condition;
		JumpIfTrue = jumpIfTrue;
		Label = label;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitConditionalGoto(this);
	}

	public BoundConditionalGoto Update(BoundExpression condition, bool jumpIfTrue, LabelSymbol label)
	{
		if (condition != Condition || jumpIfTrue != JumpIfTrue || label != Label)
		{
			return new BoundConditionalGoto(Syntax, condition, jumpIfTrue, label, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
