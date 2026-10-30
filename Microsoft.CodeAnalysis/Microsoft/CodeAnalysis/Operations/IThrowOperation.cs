namespace Microsoft.CodeAnalysis.Operations;

public interface IThrowOperation : IOperation
{
	IOperation Exception { get; }
}
