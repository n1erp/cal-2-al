namespace Microsoft.CodeAnalysis.Operations;

public interface IRelationalCaseClauseOperation : ICaseClauseOperation, IOperation
{
	IOperation Value { get; }

	BinaryOperatorKind Relation { get; }
}
