using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseIncrementExpression : Operation, IIncrementOrDecrementOperation, IOperation
{
	public bool IsPostfix { get; }

	public bool IsLifted { get; }

	public bool IsChecked { get; }

	public IMethodSymbol OperatorMethod { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Target != null)
			{
				yield return Target;
			}
		}
	}

	public abstract IOperation Target { get; }

	public BaseIncrementExpression(bool isDecrement, bool isPostfix, bool isLifted, bool isChecked, IMethodSymbol operatorMethod, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(isDecrement ? OperationKind.Decrement : OperationKind.Increment, semanticModel, syntax, type, constantValue, isImplicit)
	{
		IsPostfix = isPostfix;
		IsLifted = isLifted;
		IsChecked = isChecked;
		OperatorMethod = operatorMethod;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitIncrementOrDecrement(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitIncrementOrDecrement(this, argument);
	}
}
