using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseTupleBinaryOperatorExpression : Operation, ITupleBinaryOperation, IOperation
{
	public BinaryOperatorKind OperatorKind { get; }

	public abstract IOperation LeftOperand { get; }

	public abstract IOperation RightOperand { get; }

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

	public BaseTupleBinaryOperatorExpression(BinaryOperatorKind operatorKind, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.TupleBinaryOperator, semanticModel, syntax, type, constantValue, isImplicit)
	{
		OperatorKind = operatorKind;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitTupleBinaryOperator(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitTupleBinaryOperator(this, argument);
	}
}
