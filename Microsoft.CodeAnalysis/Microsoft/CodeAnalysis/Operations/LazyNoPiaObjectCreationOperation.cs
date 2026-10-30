using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyNoPiaObjectCreationOperation : BaseNoPiaObjectCreationOperation
{
	private readonly Lazy<IObjectOrCollectionInitializerOperation> _lazyInitializer;

	public override IObjectOrCollectionInitializerOperation Initializer => Operation.SetParentOperation(_lazyInitializer.Value, this);

	public LazyNoPiaObjectCreationOperation(Lazy<IObjectOrCollectionInitializerOperation> initializer, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyInitializer = initializer ?? throw new ArgumentNullException("initializer");
	}
}
