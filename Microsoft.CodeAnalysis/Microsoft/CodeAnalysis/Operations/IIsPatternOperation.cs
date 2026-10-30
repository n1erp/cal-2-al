namespace Microsoft.CodeAnalysis.Operations;

public interface IIsPatternOperation : IOperation
{
	IOperation Value { get; }

	IPatternOperation Pattern { get; }
}
