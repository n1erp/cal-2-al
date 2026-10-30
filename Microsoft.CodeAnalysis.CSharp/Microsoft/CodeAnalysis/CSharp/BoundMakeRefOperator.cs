using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundMakeRefOperator : BoundExpression
{
	protected override ImmutableArray<BoundNode> Children => ImmutableArray.Create((BoundNode)Operand);

	public BoundExpression Operand { get; }

	public BoundMakeRefOperator(SyntaxNode syntax, BoundExpression operand, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.MakeRefOperator, syntax, type, hasErrors || operand.HasErrors())
	{
		Operand = operand;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitMakeRefOperator(this);
	}

	public BoundMakeRefOperator Update(BoundExpression operand, TypeSymbol type)
	{
		if (operand != Operand || type != Type)
		{
			return new BoundMakeRefOperator(Syntax, operand, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
