namespace Microsoft.CodeAnalysis.Operations;

internal sealed class MethodBodyOperation : BaseMethodBodyOperation
{
	public override IBlockOperation BlockBody { get; }

	public override IBlockOperation ExpressionBody { get; }

	public MethodBodyOperation(SemanticModel semanticModel, SyntaxNode syntax, IBlockOperation blockBody, IBlockOperation expressionBody)
		: base(semanticModel, syntax)
	{
		BlockBody = Operation.SetParentOperation(blockBody, this);
		ExpressionBody = Operation.SetParentOperation(expressionBody, this);
	}
}
