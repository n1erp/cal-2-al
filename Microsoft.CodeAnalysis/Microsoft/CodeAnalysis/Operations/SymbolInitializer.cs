namespace Microsoft.CodeAnalysis.Operations;

internal abstract class SymbolInitializer : Operation
{
	public abstract IOperation Value { get; }

	protected SymbolInitializer(OperationKind kind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(kind, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}
}
