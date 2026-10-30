using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class CaughtExceptionOperation : Operation, ICaughtExceptionOperation, IOperation
{
	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public CaughtExceptionOperation(SyntaxNode syntax, ITypeSymbol type)
		: base(OperationKind.CaughtException, null, syntax, type, default, isImplicit: true)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitCaughtException(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitCaughtException(this, argument);
	}
}
