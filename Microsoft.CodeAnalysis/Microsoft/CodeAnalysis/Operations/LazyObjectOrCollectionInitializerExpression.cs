using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyObjectOrCollectionInitializerExpression : BaseObjectOrCollectionInitializerExpression, IObjectOrCollectionInitializerOperation, IOperation
{
	private readonly Lazy<ImmutableArray<IOperation>> _lazyInitializers;

	public override ImmutableArray<IOperation> Initializers => Operation.SetParentOperation(_lazyInitializers.Value, this);

	public LazyObjectOrCollectionInitializerExpression(Lazy<ImmutableArray<IOperation>> initializers, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyInitializers = initializers ?? throw new ArgumentNullException("initializers");
	}
}
