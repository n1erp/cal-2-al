using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundRefValueOperator : BoundExpression
{
	protected override ImmutableArray<BoundNode> Children => ImmutableArray.Create((BoundNode)Operand);

	public BoundExpression Operand { get; }

	public BoundRefValueOperator(SyntaxNode syntax, BoundExpression operand, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.RefValueOperator, syntax, type, hasErrors || operand.HasErrors())
	{
		Operand = operand;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitRefValueOperator(this);
	}

	public BoundRefValueOperator Update(BoundExpression operand, TypeSymbol type)
	{
		if (operand != Operand || type != Type)
		{
			return new BoundRefValueOperator(Syntax, operand, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
