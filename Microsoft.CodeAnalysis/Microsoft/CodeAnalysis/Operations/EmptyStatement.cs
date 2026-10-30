using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class EmptyStatement : Operation, IEmptyOperation, IOperation
{
	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public EmptyStatement(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Empty, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitEmpty(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitEmpty(this, argument);
	}
}
