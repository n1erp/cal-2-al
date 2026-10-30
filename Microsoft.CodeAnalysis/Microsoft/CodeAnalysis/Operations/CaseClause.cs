namespace Microsoft.CodeAnalysis.Operations;

internal abstract class CaseClause : Operation, ICaseClauseOperation, IOperation
{
	public CaseKind CaseKind { get; }

	public abstract ILabelSymbol Label { get; }

	protected CaseClause(CaseKind caseKind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.CaseClause, semanticModel, syntax, type, constantValue, isImplicit)
	{
		CaseKind = caseKind;
	}
}
