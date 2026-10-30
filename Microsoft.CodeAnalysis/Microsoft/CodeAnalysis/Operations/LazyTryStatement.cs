using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyTryStatement : BaseTryStatement, ITryOperation, IOperation
{
	private readonly Lazy<IBlockOperation> _lazyBody;

	private readonly Lazy<ImmutableArray<ICatchClauseOperation>> _lazyCatches;

	private readonly Lazy<IBlockOperation> _lazyFinallyHandler;

	public override IBlockOperation Body => Operation.SetParentOperation(_lazyBody.Value, this);

	public override ImmutableArray<ICatchClauseOperation> Catches => Operation.SetParentOperation(_lazyCatches.Value, this);

	public override IBlockOperation Finally => Operation.SetParentOperation(_lazyFinallyHandler.Value, this);

	public LazyTryStatement(Lazy<IBlockOperation> body, Lazy<ImmutableArray<ICatchClauseOperation>> catches, Lazy<IBlockOperation> finallyHandler, ILabelSymbol exitLabel, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(exitLabel, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyBody = body ?? throw new ArgumentNullException("body");
		_lazyCatches = catches;
		_lazyFinallyHandler = finallyHandler ?? throw new ArgumentNullException("finallyHandler");
	}
}
