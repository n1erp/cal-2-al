using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyBinaryOperatorExpression : BaseBinaryOperatorExpression, IBinaryOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyLeftOperand;

	private readonly Lazy<IOperation> _lazyRightOperand;

	public override IOperation LeftOperand => Operation.SetParentOperation(_lazyLeftOperand.Value, this);

	public override IOperation RightOperand => Operation.SetParentOperation(_lazyRightOperand.Value, this);

	public LazyBinaryOperatorExpression(BinaryOperatorKind operatorKind, Lazy<IOperation> leftOperand, Lazy<IOperation> rightOperand, bool isLifted, bool isChecked, bool isCompareText, IMethodSymbol operatorMethod, IMethodSymbol unaryOperatorMethod, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(operatorKind, isLifted, isChecked, isCompareText, operatorMethod, unaryOperatorMethod, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyLeftOperand = leftOperand ?? throw new ArgumentNullException("leftOperand");
		_lazyRightOperand = rightOperand ?? throw new ArgumentNullException("rightOperand");
	}
}
