namespace Microsoft.CodeAnalysis.Operations;

public interface IIsTypeOperation : IOperation
{
	IOperation ValueOperand { get; }

	ITypeSymbol TypeOperand { get; }

	bool IsNegated { get; }
}
