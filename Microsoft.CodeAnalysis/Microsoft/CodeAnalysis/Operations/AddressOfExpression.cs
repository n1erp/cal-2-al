namespace Microsoft.CodeAnalysis.Operations;

internal sealed class AddressOfExpression : BaseAddressOfExpression, IAddressOfOperation, IOperation
{
	public override IOperation Reference { get; }

	public AddressOfExpression(IOperation reference, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Reference = Operation.SetParentOperation(reference, this);
	}
}
