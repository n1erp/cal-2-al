namespace Microsoft.CodeAnalysis.Operations;

public interface ICaseClauseOperation : IOperation
{
	CaseKind CaseKind { get; }

	ILabelSymbol Label { get; }
}
