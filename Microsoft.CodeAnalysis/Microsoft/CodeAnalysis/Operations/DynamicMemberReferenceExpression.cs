using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class DynamicMemberReferenceExpression : BaseDynamicMemberReferenceExpression, IDynamicMemberReferenceOperation, IOperation
{
	public override IOperation Instance { get; }

	public DynamicMemberReferenceExpression(IOperation instance, string memberName, ImmutableArray<ITypeSymbol> typeArguments, ITypeSymbol containingType, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(memberName, typeArguments, containingType, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Instance = Operation.SetParentOperation(instance, this);
	}
}
