using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseEventAssignmentOperation : Operation, IEventAssignmentOperation, IOperation
{
	public bool Adds { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (EventReference != null)
			{
				yield return EventReference;
			}
			if (HandlerValue != null)
			{
				yield return HandlerValue;
			}
		}
	}

	public abstract IOperation EventReference { get; }

	public abstract IOperation HandlerValue { get; }

	protected BaseEventAssignmentOperation(bool adds, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.EventAssignment, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Adds = adds;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitEventAssignment(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitEventAssignment(this, argument);
	}
}
