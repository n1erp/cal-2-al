namespace Microsoft.CodeAnalysis.Operations;

public interface IBinaryOperation : IOperation
{
	BinaryOperatorKind OperatorKind { get; }

	IOperation LeftOperand { get; }

	IOperation RightOperand { get; }

	IMethodSymbol OperatorMethod { get; }

	bool IsLifted { get; }

	bool IsChecked { get; }

	bool IsCompareText { get; }
}
