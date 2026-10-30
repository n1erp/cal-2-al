namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LabeledStatement : BaseLabeledStatement, ILabeledOperation, IOperation
{
	public override IOperation Operation { get; }

	public LabeledStatement(ILabelSymbol label, IOperation operation, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(label, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Operation = Microsoft.CodeAnalysis.Operation.SetParentOperation(operation, this);
	}
}
