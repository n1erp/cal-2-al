using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyArrayElementReferenceExpression : BaseArrayElementReferenceExpression, IArrayElementReferenceOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyArrayReference;

	private readonly Lazy<ImmutableArray<IOperation>> _lazyIndices;

	public override IOperation ArrayReference => Operation.SetParentOperation(_lazyArrayReference.Value, this);

	public override ImmutableArray<IOperation> Indices => Operation.SetParentOperation(_lazyIndices.Value, this);

	public LazyArrayElementReferenceExpression(Lazy<IOperation> arrayReference, Lazy<ImmutableArray<IOperation>> indices, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyArrayReference = arrayReference ?? throw new ArgumentNullException("arrayReference");
		_lazyIndices = indices;
	}
}
