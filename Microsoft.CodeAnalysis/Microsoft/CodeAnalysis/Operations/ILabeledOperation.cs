namespace Microsoft.CodeAnalysis.Operations;

public interface ILabeledOperation : IOperation
{
	ILabelSymbol Label { get; }

	IOperation Operation { get; }
}
