using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundPointerIndirectionOperator : BoundExpression
{
	protected override ImmutableArray<BoundNode> Children => ImmutableArray.Create((BoundNode)Operand);

	public BoundExpression Operand { get; }

	public BoundPointerIndirectionOperator(SyntaxNode syntax, BoundExpression operand, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.PointerIndirectionOperator, syntax, type, hasErrors || operand.HasErrors())
	{
		Operand = operand;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitPointerIndirectionOperator(this);
	}

	public BoundPointerIndirectionOperator Update(BoundExpression operand, TypeSymbol type)
	{
		if (operand != Operand || type != Type)
		{
			return new BoundPointerIndirectionOperator(Syntax, operand, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
