using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ArrayCreationExpression : BaseArrayCreationExpression, IArrayCreationOperation, IOperation
{
	public override ImmutableArray<IOperation> DimensionSizes { get; }

	public override IArrayInitializerOperation Initializer { get; }

	public ArrayCreationExpression(ImmutableArray<IOperation> dimensionSizes, IArrayInitializerOperation initializer, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		DimensionSizes = Operation.SetParentOperation(dimensionSizes, this);
		Initializer = Operation.SetParentOperation(initializer, this);
	}
}
