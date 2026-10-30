namespace Microsoft.CodeAnalysis.Operations;

public interface IConditionalOperation : IOperation
{
	IOperation Condition { get; }

	IOperation WhenTrue { get; }

	IOperation WhenFalse { get; }

	bool IsRef { get; }
}
