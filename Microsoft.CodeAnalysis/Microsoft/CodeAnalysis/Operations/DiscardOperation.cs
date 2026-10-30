using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class DiscardOperation : Operation, IDiscardOperation, IOperation
{
	public IDiscardSymbol DiscardSymbol { get; }

	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public DiscardOperation(IDiscardSymbol discardSymbol, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Discard, semanticModel, syntax, type, constantValue, isImplicit)
	{
		DiscardSymbol = discardSymbol;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitDiscardOperation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitDiscardOperation(this, argument);
	}
}
