namespace Microsoft.CodeAnalysis.Operations;

internal sealed class UnaryOperatorExpression : BaseUnaryOperatorExpression, IUnaryOperation, IOperation
{
	public override IOperation Operand { get; }

	public UnaryOperatorExpression(UnaryOperatorKind unaryOperationKind, IOperation operand, bool isLifted, bool isChecked, IMethodSymbol operatorMethod, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(unaryOperationKind, isLifted, isChecked, operatorMethod, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Operand = Operation.SetParentOperation(operand, this);
	}
}
