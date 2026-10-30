using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class BlockStatement : BaseBlockStatement, IBlockOperation, IOperation
{
	public override ImmutableArray<IOperation> Operations { get; }

	public BlockStatement(ImmutableArray<IOperation> operations, ImmutableArray<ILocalSymbol> locals, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(locals, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Operations = Operation.SetParentOperation(operations, this);
	}
}
