using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyTupleBinaryOperatorExpression : BaseTupleBinaryOperatorExpression, ITupleBinaryOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyLeftOperand;

	private readonly Lazy<IOperation> _lazyRightOperand;

	public override IOperation LeftOperand => Operation.SetParentOperation(_lazyLeftOperand.Value, this);

	public override IOperation RightOperand => Operation.SetParentOperation(_lazyRightOperand.Value, this);

	public LazyTupleBinaryOperatorExpression(BinaryOperatorKind operatorKind, Lazy<IOperation> leftOperand, Lazy<IOperation> rightOperand, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(operatorKind, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyLeftOperand = leftOperand ?? throw new ArgumentNullException("leftOperand");
		_lazyRightOperand = rightOperand ?? throw new ArgumentNullException("rightOperand");
	}
}
