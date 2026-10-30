namespace Microsoft.CodeAnalysis.Operations;

internal sealed class AggregateQueryOperation : BaseAggregateQueryOperation
{
	public override IOperation Group { get; }

	public override IOperation Aggregation { get; }

	public AggregateQueryOperation(IOperation group, IOperation aggregation, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Group = Operation.SetParentOperation(group, this);
		Aggregation = Operation.SetParentOperation(aggregation, this);
	}
}
