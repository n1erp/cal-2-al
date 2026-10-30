using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseBinaryOperatorExpression : Operation, IBinaryOperation, IOperation
{
	public BinaryOperatorKind OperatorKind { get; }

	public IMethodSymbol OperatorMethod { get; }

	public IMethodSymbol UnaryOperatorMethod { get; }

	public bool IsLifted { get; }

	public bool IsChecked { get; }

	public bool IsCompareText { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (LeftOperand != null)
			{
				yield return LeftOperand;
			}
			if (RightOperand != null)
			{
				yield return RightOperand;
			}
		}
	}

	public abstract IOperation LeftOperand { get; }

	public abstract IOperation RightOperand { get; }

	protected BaseBinaryOperatorExpression(BinaryOperatorKind operatorKind, bool isLifted, bool isChecked, bool isCompareText, IMethodSymbol operatorMethod, IMethodSymbol unaryOperatorMethod, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.BinaryOperator, semanticModel, syntax, type, constantValue, isImplicit)
	{
		OperatorKind = operatorKind;
		IsLifted = isLifted;
		IsChecked = isChecked;
		IsCompareText = isCompareText;
		OperatorMethod = operatorMethod;
		UnaryOperatorMethod = unaryOperatorMethod;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitBinaryOperator(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitBinaryOperator(this, argument);
	}
}
