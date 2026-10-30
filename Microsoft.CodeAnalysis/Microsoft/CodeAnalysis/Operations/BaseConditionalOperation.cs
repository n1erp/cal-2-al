using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseConditionalOperation : Operation, IConditionalOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Condition != null)
			{
				yield return Condition;
			}
			if (WhenTrue != null)
			{
				yield return WhenTrue;
			}
			if (WhenFalse != null)
			{
				yield return WhenFalse;
			}
		}
	}

	public abstract IOperation Condition { get; }

	public abstract IOperation WhenTrue { get; }

	public abstract IOperation WhenFalse { get; }

	public bool IsRef { get; }

	protected BaseConditionalOperation(bool isRef, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Conditional, semanticModel, syntax, type, constantValue, isImplicit)
	{
		IsRef = isRef;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitConditional(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitConditional(this, argument);
	}
}
