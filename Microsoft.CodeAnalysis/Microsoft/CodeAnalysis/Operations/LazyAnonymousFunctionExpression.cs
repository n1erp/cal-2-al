using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyAnonymousFunctionExpression : BaseAnonymousFunctionExpression, IAnonymousFunctionOperation, IOperation
{
	private readonly Lazy<IBlockOperation> _lazyBody;

	public override IBlockOperation Body => Operation.SetParentOperation(_lazyBody.Value, this);

	public LazyAnonymousFunctionExpression(IMethodSymbol symbol, Lazy<IBlockOperation> body, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(symbol, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyBody = body ?? throw new ArgumentNullException("body");
	}
}
