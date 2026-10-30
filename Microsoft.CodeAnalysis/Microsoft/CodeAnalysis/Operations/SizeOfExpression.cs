using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class SizeOfExpression : Operation, ISizeOfOperation, IOperation
{
	public ITypeSymbol TypeOperand { get; }

	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public SizeOfExpression(ITypeSymbol typeOperand, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.SizeOf, semanticModel, syntax, type, constantValue, isImplicit)
	{
		TypeOperand = typeOperand;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitSizeOf(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitSizeOf(this, argument);
	}
}
