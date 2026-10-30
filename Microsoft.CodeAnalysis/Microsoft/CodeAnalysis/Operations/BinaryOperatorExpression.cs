namespace Microsoft.CodeAnalysis.Operations;

internal sealed class BinaryOperatorExpression : BaseBinaryOperatorExpression, IBinaryOperation, IOperation
{
	public override IOperation LeftOperand { get; }

	public override IOperation RightOperand { get; }

	public BinaryOperatorExpression(BinaryOperatorKind operatorKind, IOperation leftOperand, IOperation rightOperand, bool isLifted, bool isChecked, bool isCompareText, IMethodSymbol operatorMethod, IMethodSymbol unaryOperatorMethod, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(operatorKind, isLifted, isChecked, isCompareText, operatorMethod, unaryOperatorMethod, semanticModel, syntax, type, constantValue, isImplicit)
	{
		LeftOperand = Operation.SetParentOperation(leftOperand, this);
		RightOperand = Operation.SetParentOperation(rightOperand, this);
	}
}
