using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyRangeCaseClause : BaseRangeCaseClause, IRangeCaseClauseOperation, ICaseClauseOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyMinimumValue;

	private readonly Lazy<IOperation> _lazyMaximumValue;

	public override IOperation MinimumValue => Operation.SetParentOperation(_lazyMinimumValue.Value, this);

	public override IOperation MaximumValue => Operation.SetParentOperation(_lazyMaximumValue.Value, this);

	public LazyRangeCaseClause(Lazy<IOperation> minimumValue, Lazy<IOperation> maximumValue, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyMinimumValue = minimumValue ?? throw new ArgumentNullException("minimumValue");
		_lazyMaximumValue = maximumValue ?? throw new ArgumentNullException("maximumValue");
	}
}
