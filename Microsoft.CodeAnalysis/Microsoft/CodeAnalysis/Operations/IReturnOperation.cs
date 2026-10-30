namespace Microsoft.CodeAnalysis.Operations;

public interface IReturnOperation : IOperation
{
	IOperation ReturnedValue { get; }
}
