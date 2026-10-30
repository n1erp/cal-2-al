using System.Collections.Generic;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class FlowCapture : Operation, IFlowCaptureOperation, IOperation
{
	public CaptureId Id { get; }

	public IOperation Value { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			yield return Value;
		}
	}

	public FlowCapture(int id, SyntaxNode syntax, IOperation value)
		: base(OperationKind.FlowCapture, null, syntax, null, default, isImplicit: true)
	{
		Id = new CaptureId(id);
		Value = Operation.SetParentOperation(value, this);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitFlowCapture(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitFlowCapture(this, argument);
	}
}
