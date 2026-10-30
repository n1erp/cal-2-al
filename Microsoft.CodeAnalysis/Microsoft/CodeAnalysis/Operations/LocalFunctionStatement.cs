namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LocalFunctionStatement : BaseLocalFunctionStatement, ILocalFunctionOperation, IOperation
{
	public override IBlockOperation Body { get; }

	public override IBlockOperation IgnoredBody { get; }

	public LocalFunctionStatement(IMethodSymbol symbol, IBlockOperation body, IBlockOperation ignoredBody, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(symbol, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Body = Operation.SetParentOperation(body, this);
		IgnoredBody = Operation.SetParentOperation(ignoredBody, this);
	}
}
