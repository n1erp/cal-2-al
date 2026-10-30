namespace Microsoft.CodeAnalysis.Operations;

internal sealed class MethodReferenceExpression : BaseMethodReferenceExpression, IMethodReferenceOperation, IMemberReferenceOperation, IOperation
{
	public override IOperation Instance { get; }

	public MethodReferenceExpression(IMethodSymbol method, bool isVirtual, IOperation instance, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(method, isVirtual, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Instance = Operation.SetParentOperation(instance, this);
	}
}
