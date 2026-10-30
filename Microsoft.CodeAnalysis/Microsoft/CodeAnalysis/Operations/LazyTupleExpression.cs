using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyTupleExpression : BaseTupleExpression, ITupleOperation, IOperation
{
	private readonly Lazy<ImmutableArray<IOperation>> _lazyElements;

	public override ImmutableArray<IOperation> Elements => Operation.SetParentOperation(_lazyElements.Value, this);

	public LazyTupleExpression(Lazy<ImmutableArray<IOperation>> elements, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, ITypeSymbol naturalType, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, naturalType, constantValue, isImplicit)
	{
		_lazyElements = elements;
	}
}
