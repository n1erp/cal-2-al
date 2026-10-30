namespace Microsoft.CodeAnalysis.Operations;

internal sealed class AwaitExpression : BaseAwaitExpression, IAwaitOperation, IOperation
{
	public override IOperation Operation { get; }

	public AwaitExpression(IOperation operation, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Operation = Microsoft.CodeAnalysis.Operation.SetParentOperation(operation, this);
	}
}
