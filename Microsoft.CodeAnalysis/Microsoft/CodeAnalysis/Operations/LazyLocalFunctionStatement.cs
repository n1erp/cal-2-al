using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyLocalFunctionStatement : BaseLocalFunctionStatement, ILocalFunctionOperation, IOperation
{
	private readonly Lazy<IBlockOperation> _lazyBody;

	private readonly Lazy<IBlockOperation> _lazyIgnoredBody;

	public override IBlockOperation Body => Operation.SetParentOperation(_lazyBody.Value, this);

	public override IBlockOperation IgnoredBody => Operation.SetParentOperation(_lazyIgnoredBody.Value, this);

	public LazyLocalFunctionStatement(IMethodSymbol symbol, Lazy<IBlockOperation> body, Lazy<IBlockOperation> ignoredBody, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(symbol, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyBody = body ?? throw new ArgumentNullException("body");
		_lazyIgnoredBody = ignoredBody ?? throw new ArgumentNullException("ignoredBody");
	}
}
