using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ParameterReferenceExpression : Operation, IParameterReferenceOperation, IOperation
{
	public IParameterSymbol Parameter { get; }

	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public ParameterReferenceExpression(IParameterSymbol parameter, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.ParameterReference, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Parameter = parameter;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitParameterReference(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitParameterReference(this, argument);
	}
}
