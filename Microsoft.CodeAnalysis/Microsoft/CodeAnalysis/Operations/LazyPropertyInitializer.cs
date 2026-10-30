using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyPropertyInitializer : BasePropertyInitializer, IPropertyInitializerOperation, ISymbolInitializerOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyValue;

	public override IOperation Value => Operation.SetParentOperation(_lazyValue.Value, this);

	public LazyPropertyInitializer(ImmutableArray<ILocalSymbol> locals, ImmutableArray<IPropertySymbol> initializedProperties, Lazy<IOperation> value, OperationKind kind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, initializedProperties, kind, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyValue = value ?? throw new ArgumentNullException("value");
	}
}
