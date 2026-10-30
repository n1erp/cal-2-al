namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ConditionalAccessExpression : BaseConditionalAccessExpression, IConditionalAccessOperation, IOperation
{
	public override IOperation Operation { get; }

	public override IOperation WhenNotNull { get; }

	public ConditionalAccessExpression(IOperation whenNotNull, IOperation operation, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		WhenNotNull = Microsoft.CodeAnalysis.Operation.SetParentOperation(whenNotNull, this);
		Operation = Microsoft.CodeAnalysis.Operation.SetParentOperation(operation, this);
	}
}
