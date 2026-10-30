namespace Microsoft.CodeAnalysis.Operations;

internal sealed class NoPiaObjectCreationOperation : BaseNoPiaObjectCreationOperation
{
	public override IObjectOrCollectionInitializerOperation Initializer { get; }

	public NoPiaObjectCreationOperation(IObjectOrCollectionInitializerOperation initializer, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Initializer = Operation.SetParentOperation(initializer, this);
	}
}
