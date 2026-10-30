using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class TupleExpression : BaseTupleExpression, ITupleOperation, IOperation
{
	public override ImmutableArray<IOperation> Elements { get; }

	public TupleExpression(ImmutableArray<IOperation> elements, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, ITypeSymbol naturalType, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, naturalType, constantValue, isImplicit)
	{
		Elements = Operation.SetParentOperation(elements, this);
	}
}
