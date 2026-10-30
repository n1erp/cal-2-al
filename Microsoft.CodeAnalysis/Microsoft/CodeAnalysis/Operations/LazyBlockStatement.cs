using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyBlockStatement : BaseBlockStatement, IBlockOperation, IOperation
{
	private readonly Lazy<ImmutableArray<IOperation>> _lazyOperations;

	public override ImmutableArray<IOperation> Operations => Operation.SetParentOperation(_lazyOperations.Value, this);

	public LazyBlockStatement(Lazy<ImmutableArray<IOperation>> operations, ImmutableArray<ILocalSymbol> locals, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyOperations = operations;
	}
}
