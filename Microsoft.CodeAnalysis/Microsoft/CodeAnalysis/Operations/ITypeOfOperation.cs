namespace Microsoft.CodeAnalysis.Operations;

public interface ITypeOfOperation : IOperation
{
	ITypeSymbol TypeOperand { get; }
}
