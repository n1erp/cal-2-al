using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class EndStatement : Operation, IEndOperation, IOperation
{
	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public EndStatement(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.End, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitEnd(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitEnd(this, argument);
	}
}
