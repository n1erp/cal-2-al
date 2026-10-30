namespace Microsoft.CodeAnalysis.Operations;

public interface IRangeCaseClauseOperation : ICaseClauseOperation, IOperation
{
	IOperation MinimumValue { get; }

	IOperation MaximumValue { get; }
}
