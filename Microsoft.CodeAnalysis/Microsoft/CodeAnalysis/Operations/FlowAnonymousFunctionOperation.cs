using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class FlowAnonymousFunctionOperation : Operation, IFlowAnonymousFunctionOperation, IOperation
{
	public readonly ControlFlowGraphBuilder.Context Context;

	public readonly IAnonymousFunctionOperation Original;

	public IMethodSymbol Symbol => Original.Symbol;

	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public FlowAnonymousFunctionOperation(in ControlFlowGraphBuilder.Context context, IAnonymousFunctionOperation original, bool isImplicit)
		: base(OperationKind.FlowAnonymousFunction, null, original.Syntax, original.Type, original.ConstantValue, isImplicit)
	{
		Context = context;
		Original = original;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitFlowAnonymousFunction(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitFlowAnonymousFunction(this, argument);
	}
}
