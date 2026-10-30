namespace Microsoft.CodeAnalysis.Operations;

internal sealed class DelegateCreationExpression : BaseDelegateCreationExpression
{
	public override IOperation Target { get; }

	public DelegateCreationExpression(IOperation target, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Target = Operation.SetParentOperation(target, this);
	}
}
