namespace Microsoft.CodeAnalysis.Operations;

internal sealed class TupleBinaryOperatorExpression : BaseTupleBinaryOperatorExpression, ITupleBinaryOperation, IOperation
{
	public override IOperation LeftOperand { get; }

	public override IOperation RightOperand { get; }

	public TupleBinaryOperatorExpression(BinaryOperatorKind operatorKind, IOperation leftOperand, IOperation rightOperand, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(operatorKind, semanticModel, syntax, type, constantValue, isImplicit)
	{
		LeftOperand = Operation.SetParentOperation(leftOperand, this);
		RightOperand = Operation.SetParentOperation(rightOperand, this);
	}
}
