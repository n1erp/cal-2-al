using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyDynamicMemberReferenceExpression : BaseDynamicMemberReferenceExpression, IDynamicMemberReferenceOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyInstance;

	public override IOperation Instance => Operation.SetParentOperation(_lazyInstance.Value, this);

	public LazyDynamicMemberReferenceExpression(Lazy<IOperation> lazyInstance, string memberName, ImmutableArray<ITypeSymbol> typeArguments, ITypeSymbol containingType, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(memberName, typeArguments, containingType, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyInstance = lazyInstance;
	}
}
