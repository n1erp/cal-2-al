namespace Microsoft.CodeAnalysis.Operations;

internal sealed class MemberInitializerExpression : BaseMemberInitializerExpression, IMemberInitializerOperation, IOperation
{
	public override IOperation InitializedMember { get; }

	public override IObjectOrCollectionInitializerOperation Initializer { get; }

	public MemberInitializerExpression(IOperation initializedMember, IObjectOrCollectionInitializerOperation initializer, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		InitializedMember = Operation.SetParentOperation(initializedMember, this);
		Initializer = Operation.SetParentOperation(initializer, this);
	}
}
