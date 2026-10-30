using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class PropertyInitializer : BasePropertyInitializer, IPropertyInitializerOperation, ISymbolInitializerOperation, IOperation
{
	public override IOperation Value { get; }

	public PropertyInitializer(ImmutableArray<ILocalSymbol> locals, ImmutableArray<IPropertySymbol> initializedProperties, IOperation value, OperationKind kind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, initializedProperties, kind, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Value = Operation.SetParentOperation(value, this);
	}
}
