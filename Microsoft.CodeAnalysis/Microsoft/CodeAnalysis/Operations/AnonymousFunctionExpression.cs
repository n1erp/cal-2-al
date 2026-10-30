namespace Microsoft.CodeAnalysis.Operations;

internal sealed class AnonymousFunctionExpression : BaseAnonymousFunctionExpression, IAnonymousFunctionOperation, IOperation
{
	public override IBlockOperation Body { get; }

	public AnonymousFunctionExpression(IMethodSymbol symbol, IBlockOperation body, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(symbol, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Body = Operation.SetParentOperation(body, this);
	}
}
