namespace Microsoft.CodeAnalysis.Operations;

public interface IConstantPatternOperation : IPatternOperation, IOperation
{
	IOperation Value { get; }
}
