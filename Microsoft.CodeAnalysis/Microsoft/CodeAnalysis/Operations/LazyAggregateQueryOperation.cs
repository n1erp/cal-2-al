using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyAggregateQueryOperation : BaseAggregateQueryOperation
{
	private readonly Lazy<IOperation> _lazyGroup;

	private readonly Lazy<IOperation> _lazyAggregation;

	public override IOperation Group => Operation.SetParentOperation(_lazyGroup.Value, this);

	public override IOperation Aggregation => Operation.SetParentOperation(_lazyAggregation.Value, this);

	public LazyAggregateQueryOperation(Lazy<IOperation> group, Lazy<IOperation> aggregation, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyGroup = group ?? throw new ArgumentNullException("group");
		_lazyAggregation = aggregation ?? throw new ArgumentNullException("aggregation");
	}
}
