namespace Microsoft.CodeAnalysis.Operations;

internal sealed class FieldReferenceExpression : BaseFieldReferenceExpression, IFieldReferenceOperation, IMemberReferenceOperation, IOperation
{
	public override IOperation Instance { get; }

	public FieldReferenceExpression(IFieldSymbol field, bool isDeclaration, IOperation instance, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(field, isDeclaration, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Instance = Operation.SetParentOperation(instance, this);
	}
}
