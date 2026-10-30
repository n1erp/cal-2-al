namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IBinaryOperatorExpression : IOperation
{
	BinaryOperationKind BinaryOperationKind { get; }

	IOperation LeftOperand { get; }

	IOperation RightOperand { get; }
}
