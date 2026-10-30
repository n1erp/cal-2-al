using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class OmittedArgumentExpression : Operation, IOmittedArgumentOperation, IOperation
{
	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public OmittedArgumentExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.OmittedArgument, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitOmittedArgument(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitOmittedArgument(this, argument);
	}
}
