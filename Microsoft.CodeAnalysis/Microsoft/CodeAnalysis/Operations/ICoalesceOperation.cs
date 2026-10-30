namespace Microsoft.CodeAnalysis.Operations;

public interface ICoalesceOperation : IOperation
{
	IOperation Value { get; }

	IOperation WhenNull { get; }

	CommonConversion ValueConversion { get; }
}
