namespace Microsoft.CodeAnalysis.Operations;

public interface ITupleBinaryOperation : IOperation
{
	BinaryOperatorKind OperatorKind { get; }

	IOperation LeftOperand { get; }

	IOperation RightOperand { get; }
}
