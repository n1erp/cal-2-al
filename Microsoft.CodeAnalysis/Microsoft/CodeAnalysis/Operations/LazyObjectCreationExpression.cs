using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyObjectCreationExpression : BaseObjectCreationExpression, IObjectCreationOperation, IOperation
{
	private readonly Lazy<IObjectOrCollectionInitializerOperation> _lazyInitializer;

	private readonly Lazy<ImmutableArray<IArgumentOperation>> _lazyArguments;

	public override IObjectOrCollectionInitializerOperation Initializer => Operation.SetParentOperation(_lazyInitializer.Value, this);

	public override ImmutableArray<IArgumentOperation> Arguments => Operation.SetParentOperation(_lazyArguments.Value, this);

	public LazyObjectCreationExpression(IMethodSymbol constructor, Lazy<IObjectOrCollectionInitializerOperation> initializer, Lazy<ImmutableArray<IArgumentOperation>> arguments, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(constructor, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyInitializer = initializer;
		_lazyArguments = arguments;
	}
}
