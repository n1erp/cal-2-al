using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseUnaryOperatorExpression : Operation, IUnaryOperation, IOperation
{
	public UnaryOperatorKind OperatorKind { get; }

	public IMethodSymbol OperatorMethod { get; }

	public bool IsLifted { get; }

	public bool IsChecked { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Operand != null)
			{
				yield return Operand;
			}
		}
	}

	public abstract IOperation Operand { get; }

	protected BaseUnaryOperatorExpression(UnaryOperatorKind unaryOperationKind, bool isLifted, bool isChecked, IMethodSymbol operatorMethod, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.UnaryOperator, semanticModel, syntax, type, constantValue, isImplicit)
	{
		OperatorKind = unaryOperationKind;
		IsLifted = isLifted;
		IsChecked = isChecked;
		OperatorMethod = operatorMethod;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitUnaryOperator(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitUnaryOperator(this, argument);
	}
}
