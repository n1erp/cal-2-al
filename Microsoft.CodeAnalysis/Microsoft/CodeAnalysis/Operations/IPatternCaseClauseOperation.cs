namespace Microsoft.CodeAnalysis.Operations;

public interface IPatternCaseClauseOperation : ICaseClauseOperation, IOperation
{
	new ILabelSymbol Label { get; }

	IPatternOperation Pattern { get; }

	IOperation Guard { get; }
}
