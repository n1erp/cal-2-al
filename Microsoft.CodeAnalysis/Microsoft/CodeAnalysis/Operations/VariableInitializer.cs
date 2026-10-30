namespace Microsoft.CodeAnalysis.Operations;

internal sealed class VariableInitializer : BaseVariableInitializer, IVariableInitializerOperation, ISymbolInitializerOperation, IOperation
{
	public override IOperation Value { get; }

	public VariableInitializer(IOperation value, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Value = Operation.SetParentOperation(value, this);
	}
}
