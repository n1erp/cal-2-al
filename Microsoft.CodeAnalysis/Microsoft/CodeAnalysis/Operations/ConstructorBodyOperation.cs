using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ConstructorBodyOperation : BaseConstructorBodyOperation
{
	public override IOperation Initializer { get; }

	public override IBlockOperation BlockBody { get; }

	public override IBlockOperation ExpressionBody { get; }

	public ConstructorBodyOperation(ImmutableArray<ILocalSymbol> locals, SemanticModel semanticModel, SyntaxNode syntax, IOperation initializer, IBlockOperation blockBody, IBlockOperation expressionBody)
		: base(locals, semanticModel, syntax)
	{
		Initializer = Operation.SetParentOperation(initializer, this);
		BlockBody = Operation.SetParentOperation(blockBody, this);
		ExpressionBody = Operation.SetParentOperation(expressionBody, this);
	}
}
