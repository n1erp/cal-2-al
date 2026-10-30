using System.Collections.Generic;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class IsNullOperation : Operation, IIsNullOperation, IOperation
{
	public IOperation Operand { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			yield return Operand;
		}
	}

	public IsNullOperation(SyntaxNode syntax, IOperation operand, ITypeSymbol type, Optional<object> constantValue)
		: base(OperationKind.IsNull, null, syntax, type, constantValue, isImplicit: true)
	{
		Operand = Operation.SetParentOperation(operand, this);
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitIsNull(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitIsNull(this, argument);
	}
}
