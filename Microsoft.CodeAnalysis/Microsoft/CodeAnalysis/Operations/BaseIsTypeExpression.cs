using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseIsTypeExpression : Operation, IIsTypeOperation, IOperation
{
	public ITypeSymbol TypeOperand { get; }

	public bool IsNegated { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (ValueOperand != null)
			{
				yield return ValueOperand;
			}
		}
	}

	public abstract IOperation ValueOperand { get; }

	protected BaseIsTypeExpression(ITypeSymbol typeOperand, bool isNegated, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.IsType, semanticModel, syntax, type, constantValue, isImplicit)
	{
		TypeOperand = typeOperand;
		IsNegated = isNegated;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitIsType(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitIsType(this, argument);
	}
}
