using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LocalReferenceExpression : Operation, ILocalReferenceOperation, IOperation
{
	public ILocalSymbol Local { get; }

	public bool IsDeclaration { get; }

	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public LocalReferenceExpression(ILocalSymbol local, bool isDeclaration, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.LocalReference, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Local = local;
		IsDeclaration = isDeclaration;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitLocalReference(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitLocalReference(this, argument);
	}
}
