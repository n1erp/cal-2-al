using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class TypeOfExpression : Operation, ITypeOfOperation, IOperation
{
	public ITypeSymbol TypeOperand { get; }

	public override IEnumerable<IOperation> Children => Array.Empty<IOperation>();

	public TypeOfExpression(ITypeSymbol typeOperand, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.TypeOf, semanticModel, syntax, type, constantValue, isImplicit)
	{
		TypeOperand = typeOperand;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitTypeOf(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitTypeOf(this, argument);
	}
}
