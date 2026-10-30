namespace Microsoft.CodeAnalysis.Operations;

public interface IAwaitOperation : IOperation
{
	IOperation Operation { get; }
}
