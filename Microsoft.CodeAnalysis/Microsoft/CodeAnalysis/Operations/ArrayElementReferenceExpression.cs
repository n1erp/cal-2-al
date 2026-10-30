using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ArrayElementReferenceExpression : BaseArrayElementReferenceExpression, IArrayElementReferenceOperation, IOperation
{
	public override IOperation ArrayReference { get; }

	public override ImmutableArray<IOperation> Indices { get; }

	public ArrayElementReferenceExpression(IOperation arrayReference, ImmutableArray<IOperation> indices, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		ArrayReference = Operation.SetParentOperation(arrayReference, this);
		Indices = Operation.SetParentOperation(indices, this);
	}
}
