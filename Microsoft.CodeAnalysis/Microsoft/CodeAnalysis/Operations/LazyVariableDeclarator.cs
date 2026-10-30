using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyVariableDeclarator : BaseVariableDeclarator
{
	private readonly Lazy<IVariableInitializerOperation> _lazyInitializer;

	private readonly Lazy<ImmutableArray<IOperation>> _lazyIgnoredArguments;

	public override IVariableInitializerOperation Initializer => Operation.SetParentOperation(_lazyInitializer.Value, this);

	public override ImmutableArray<IOperation> IgnoredArguments => Operation.SetParentOperation(_lazyIgnoredArguments.Value, this);

	public LazyVariableDeclarator(ILocalSymbol symbol, Lazy<IVariableInitializerOperation> initializer, Lazy<ImmutableArray<IOperation>> ignoredArguments, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(symbol, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyInitializer = initializer ?? throw new ArgumentNullException("initializer");
		_lazyIgnoredArguments = ignoredArguments ?? throw new ArgumentNullException("ignoredArguments");
	}
}
