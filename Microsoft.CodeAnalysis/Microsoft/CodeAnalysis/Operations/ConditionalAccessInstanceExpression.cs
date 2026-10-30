using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ConditionalAccessInstanceExpression : Operation, IConditionalAccessInstanceOperation, IOperation
{
	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public ConditionalAccessInstanceExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.ConditionalAccessInstance, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitConditionalAccessInstance(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitConditionalAccessInstance(this, argument);
	}
}
