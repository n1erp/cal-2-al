namespace Microsoft.CodeAnalysis.Operations;

internal sealed class PatternCaseClause : BasePatternCaseClause, IPatternCaseClauseOperation, ICaseClauseOperation, IOperation
{
	public override IPatternOperation Pattern { get; }

	public override IOperation Guard { get; }

	public PatternCaseClause(ILabelSymbol label, IPatternOperation pattern, IOperation guardExpression, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(label, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Pattern = Operation.SetParentOperation(pattern, this);
		Guard = Operation.SetParentOperation(guardExpression, this);
	}
}
