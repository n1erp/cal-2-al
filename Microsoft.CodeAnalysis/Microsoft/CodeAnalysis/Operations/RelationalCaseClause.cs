namespace Microsoft.CodeAnalysis.Operations;

internal sealed class RelationalCaseClause : BaseRelationalCaseClause, IRelationalCaseClauseOperation, ICaseClauseOperation, IOperation
{
	public override IOperation Value { get; }

	public RelationalCaseClause(IOperation value, BinaryOperatorKind relation, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(relation, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Value = Operation.SetParentOperation(value, this);
	}
}
