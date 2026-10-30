using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class FlowCaptureReference : Operation, IFlowCaptureReferenceOperation, IOperation
{
	public CaptureId Id { get; }

	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public FlowCaptureReference(int id, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue)
		: base(OperationKind.FlowCaptureReference, null, syntax, type, constantValue, isImplicit: true)
	{
		Id = new CaptureId(id);
	}

	public FlowCaptureReference(CaptureId id, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue)
		: base(OperationKind.FlowCaptureReference, null, syntax, type, constantValue, isImplicit: true)
	{
		Id = id;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitFlowCaptureReference(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitFlowCaptureReference(this, argument);
	}
}
