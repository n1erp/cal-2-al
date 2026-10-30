namespace Microsoft.CodeAnalysis.Operations;

public interface IConversionOperation : IOperation
{
	IOperation Operand { get; }

	IMethodSymbol OperatorMethod { get; }

	CommonConversion Conversion { get; }

	bool IsTryCast { get; }

	bool IsChecked { get; }
}
