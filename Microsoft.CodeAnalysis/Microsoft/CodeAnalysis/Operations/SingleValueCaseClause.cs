namespace Microsoft.CodeAnalysis.Operations;

internal sealed class SingleValueCaseClause : BaseSingleValueCaseClause, ISingleValueCaseClauseOperation, ICaseClauseOperation, IOperation
{
	public override IOperation Value { get; }

	public SingleValueCaseClause(ILabelSymbol label, IOperation value, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(label, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Value = Operation.SetParentOperation(value, this);
	}
}
