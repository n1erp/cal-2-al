namespace Microsoft.CodeAnalysis.Operations;

public interface ISingleValueCaseClauseOperation : ICaseClauseOperation, IOperation
{
	IOperation Value { get; }
}
