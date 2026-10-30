using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyInvalidOperation : BaseInvalidOperation, IInvalidOperation, IOperation
{
	private readonly Lazy<ImmutableArray<IOperation>> _lazyChildren;

	public override IEnumerable<IOperation> Children => Operation.SetParentOperation(_lazyChildren.Value, this);

	public LazyInvalidOperation(Lazy<ImmutableArray<IOperation>> children, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyChildren = children;
	}
}
