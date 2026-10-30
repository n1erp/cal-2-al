namespace Microsoft.CodeAnalysis.Operations;

internal sealed class IsTypeExpression : BaseIsTypeExpression, IIsTypeOperation, IOperation
{
	public override IOperation ValueOperand { get; }

	public IsTypeExpression(IOperation valueOperand, ITypeSymbol typeOperand, bool isNegated, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(typeOperand, isNegated, semanticModel, syntax, type, constantValue, isImplicit)
	{
		ValueOperand = Operation.SetParentOperation(valueOperand, this);
	}
}
