namespace Microsoft.CodeAnalysis.Operations;

public interface IIncrementOrDecrementOperation : IOperation
{
	IOperation Target { get; }

	IMethodSymbol OperatorMethod { get; }

	bool IsPostfix { get; }

	bool IsLifted { get; }

	bool IsChecked { get; }
}
