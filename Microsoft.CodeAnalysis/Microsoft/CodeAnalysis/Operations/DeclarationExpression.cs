namespace Microsoft.CodeAnalysis.Operations;

internal sealed class DeclarationExpression : BaseDeclarationExpression, IDeclarationExpressionOperation, IOperation
{
	public override IOperation Expression { get; }

	public DeclarationExpression(IOperation expression, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Expression = Operation.SetParentOperation(expression, this);
	}
}
