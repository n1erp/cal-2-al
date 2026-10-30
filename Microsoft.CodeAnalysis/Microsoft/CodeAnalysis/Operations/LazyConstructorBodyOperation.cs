using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyConstructorBodyOperation : BaseConstructorBodyOperation
{
	private readonly Lazy<IOperation> _lazyInitializer;

	private readonly Lazy<IBlockOperation> _lazyBlockBody;

	private readonly Lazy<IBlockOperation> _lazyExpressionBody;

	public override IOperation Initializer => Operation.SetParentOperation(_lazyInitializer.Value, this);

	public override IBlockOperation BlockBody => Operation.SetParentOperation(_lazyBlockBody.Value, this);

	public override IBlockOperation ExpressionBody => Operation.SetParentOperation(_lazyExpressionBody.Value, this);

	public LazyConstructorBodyOperation(ImmutableArray<ILocalSymbol> locals, SemanticModel semanticModel, SyntaxNode syntax, Lazy<IOperation> initializer, Lazy<IBlockOperation> blockBody, Lazy<IBlockOperation> expressionBody)
		: base(locals, semanticModel, syntax)
	{
		_lazyInitializer = initializer;
		_lazyBlockBody = blockBody;
		_lazyExpressionBody = expressionBody;
	}
}
