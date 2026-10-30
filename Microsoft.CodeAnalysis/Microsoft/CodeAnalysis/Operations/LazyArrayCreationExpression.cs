using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyArrayCreationExpression : BaseArrayCreationExpression, IArrayCreationOperation, IOperation
{
	private readonly Lazy<ImmutableArray<IOperation>> _lazyDimensionSizes;

	private readonly Lazy<IArrayInitializerOperation> _lazyInitializer;

	public override ImmutableArray<IOperation> DimensionSizes => Operation.SetParentOperation(_lazyDimensionSizes.Value, this);

	public override IArrayInitializerOperation Initializer => Operation.SetParentOperation(_lazyInitializer.Value, this);

	public LazyArrayCreationExpression(Lazy<ImmutableArray<IOperation>> dimensionSizes, Lazy<IArrayInitializerOperation> initializer, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyDimensionSizes = dimensionSizes;
		_lazyInitializer = initializer ?? throw new ArgumentNullException("initializer");
	}
}
