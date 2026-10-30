namespace Microsoft.CodeAnalysis.Operations;

internal sealed class TranslatedQueryExpression : BaseTranslatedQueryExpression, ITranslatedQueryOperation, IOperation
{
	public override IOperation Operation { get; }

	public TranslatedQueryExpression(IOperation operation, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		Operation = Microsoft.CodeAnalysis.Operation.SetParentOperation(operation, this);
	}
}
