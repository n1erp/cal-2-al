using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class StaticLocalInitializationSemaphoreOperation : Operation, IStaticLocalInitializationSemaphoreOperation, IOperation
{
	public ILocalSymbol Local { get; }

	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public StaticLocalInitializationSemaphoreOperation(ILocalSymbol local, SyntaxNode syntax, ITypeSymbol type)
		: base(OperationKind.StaticLocalInitializationSemaphore, null, syntax, type, default, isImplicit: true)
	{
		Local = local;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitStaticLocalInitializationSemaphore(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitStaticLocalInitializationSemaphore(this, argument);
	}
}
