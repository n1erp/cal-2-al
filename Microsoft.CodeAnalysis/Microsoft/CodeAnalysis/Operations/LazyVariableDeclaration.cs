using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyVariableDeclaration : BaseVariableDeclaration
{
	private readonly Lazy<ImmutableArray<IVariableDeclaratorOperation>> _lazyDeclarators;

	private readonly Lazy<IVariableInitializerOperation> _lazyInitializer;

	public override ImmutableArray<IVariableDeclaratorOperation> Declarators => Operation.SetParentOperation(_lazyDeclarators.Value, this);

	public override IVariableInitializerOperation Initializer => Operation.SetParentOperation(_lazyInitializer.Value, this);

	public LazyVariableDeclaration(Lazy<ImmutableArray<IVariableDeclaratorOperation>> declarators, Lazy<IVariableInitializerOperation> initializer, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyDeclarators = declarators;
		_lazyInitializer = initializer;
	}
}
