namespace Microsoft.CodeAnalysis.Operations;

public interface ISizeOfOperation : IOperation
{
	ITypeSymbol TypeOperand { get; }
}
