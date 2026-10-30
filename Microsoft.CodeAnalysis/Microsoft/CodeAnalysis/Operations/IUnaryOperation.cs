namespace Microsoft.CodeAnalysis.Operations;

public interface IUnaryOperation : IOperation
{
	UnaryOperatorKind OperatorKind { get; }

	IOperation Operand { get; }

	IMethodSymbol OperatorMethod { get; }

	bool IsLifted { get; }

	bool IsChecked { get; }
}
