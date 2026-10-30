using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class FieldInitializer : BaseFieldInitializer, IFieldInitializerOperation, ISymbolInitializerOperation, IOperation
{
	public override IOperation Value { get; }

	public FieldInitializer(ImmutableArray<ILocalSymbol> locals, ImmutableArray<IFieldSymbol> initializedFields, IOperation value, OperationKind kind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, initializedFields, kind, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Value = Operation.SetParentOperation(value, this);
	}
}
