using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyUnaryOperatorExpression : BaseUnaryOperatorExpression, IUnaryOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyOperand;

	public override IOperation Operand => Operation.SetParentOperation(_lazyOperand.Value, this);

	public LazyUnaryOperatorExpression(UnaryOperatorKind unaryOperationKind, Lazy<IOperation> operand, bool isLifted, bool isChecked, IMethodSymbol operatorMethod, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(unaryOperationKind, isLifted, isChecked, operatorMethod, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyOperand = operand ?? throw new ArgumentNullException("operand");
	}
}
