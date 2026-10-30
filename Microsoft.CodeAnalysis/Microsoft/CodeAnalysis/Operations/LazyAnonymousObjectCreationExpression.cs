using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyAnonymousObjectCreationExpression : BaseAnonymousObjectCreationExpression, IAnonymousObjectCreationOperation, IOperation
{
	private readonly Lazy<ImmutableArray<IOperation>> _lazyInitializers;

	public override ImmutableArray<IOperation> Initializers => Operation.SetParentOperation(_lazyInitializers.Value, this);

	public LazyAnonymousObjectCreationExpression(Lazy<ImmutableArray<IOperation>> initializers, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyInitializers = initializers;
	}
}
