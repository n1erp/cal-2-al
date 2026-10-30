namespace Microsoft.CodeAnalysis.Operations;

public interface IAddressOfOperation : IOperation
{
	IOperation Reference { get; }
}
