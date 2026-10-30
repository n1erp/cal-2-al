namespace Microsoft.CodeAnalysis.Operations;

public interface IConditionalAccessOperation : IOperation
{
	IOperation Operation { get; }

	IOperation WhenNotNull { get; }
}
