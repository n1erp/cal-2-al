namespace Microsoft.CodeAnalysis.Operations;

internal sealed class RangeCaseClause : BaseRangeCaseClause, IRangeCaseClauseOperation, ICaseClauseOperation, IOperation
{
	public override IOperation MinimumValue { get; }

	public override IOperation MaximumValue { get; }

	public RangeCaseClause(IOperation minimumValue, IOperation maximumValue, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		MinimumValue = Operation.SetParentOperation(minimumValue, this);
		MaximumValue = Operation.SetParentOperation(maximumValue, this);
	}
}
