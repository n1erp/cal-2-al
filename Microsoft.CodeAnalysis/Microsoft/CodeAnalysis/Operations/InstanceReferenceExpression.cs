using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class InstanceReferenceExpression : Operation, IInstanceReferenceOperation, IOperation
{
	public InstanceReferenceKind ReferenceKind { get; }

	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public InstanceReferenceExpression(InstanceReferenceKind referenceKind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.InstanceReference, semanticModel, syntax, type, constantValue, isImplicit)
	{
		ReferenceKind = referenceKind;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitInstanceReference(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitInstanceReference(this, argument);
	}
}
