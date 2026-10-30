using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyMethodBodyOperation : BaseMethodBodyOperation
{
	private readonly Lazy<IBlockOperation> _lazyBlockBody;

	private readonly Lazy<IBlockOperation> _lazyExpressionBody;

	public override IBlockOperation BlockBody => Operation.SetParentOperation(_lazyBlockBody.Value, this);

	public override IBlockOperation ExpressionBody => Operation.SetParentOperation(_lazyExpressionBody.Value, this);

	public LazyMethodBodyOperation(SemanticModel semanticModel, SyntaxNode syntax, Lazy<IBlockOperation> blockBody, Lazy<IBlockOperation> expressionBody)
		: base(semanticModel, syntax)
	{
		_lazyBlockBody = blockBody;
		_lazyExpressionBody = expressionBody;
	}
}
