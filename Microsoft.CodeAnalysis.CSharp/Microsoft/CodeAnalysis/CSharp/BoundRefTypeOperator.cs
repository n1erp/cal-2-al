using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class BoundRefTypeOperator : BoundExpression
{
	protected override ImmutableArray<BoundNode> Children => ImmutableArray.Create((BoundNode)Operand);

	public BoundExpression Operand { get; }

	public MethodSymbol GetTypeFromHandle { get; }

	public BoundRefTypeOperator(SyntaxNode syntax, BoundExpression operand, MethodSymbol getTypeFromHandle, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.RefTypeOperator, syntax, type, hasErrors || operand.HasErrors())
	{
		Operand = operand;
		GetTypeFromHandle = getTypeFromHandle;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitRefTypeOperator(this);
	}

	public BoundRefTypeOperator Update(BoundExpression operand, MethodSymbol getTypeFromHandle, TypeSymbol type)
	{
		if (operand != Operand || getTypeFromHandle != GetTypeFromHandle || type != Type)
		{
			return new BoundRefTypeOperator(Syntax, operand, getTypeFromHandle, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
