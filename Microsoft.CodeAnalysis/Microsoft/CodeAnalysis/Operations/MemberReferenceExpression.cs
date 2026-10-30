namespace Microsoft.CodeAnalysis.Operations;

internal abstract class MemberReferenceExpression : Operation, IMemberReferenceOperation, IOperation
{
	public abstract IOperation Instance { get; }

	public ISymbol Member { get; }

	protected MemberReferenceExpression(ISymbol member, OperationKind kind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(kind, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Member = member;
	}
}
