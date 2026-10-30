using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseRaiseEventStatement : Operation, IRaiseEventOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (EventReference != null)
			{
				yield return EventReference;
			}
			foreach (IArgumentOperation argument in Arguments)
			{
				if (argument != null)
				{
					yield return argument;
				}
			}
		}
	}

	public abstract IEventReferenceOperation EventReference { get; }

	public abstract ImmutableArray<IArgumentOperation> Arguments { get; }

	protected BaseRaiseEventStatement(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.RaiseEvent, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitRaiseEvent(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitRaiseEvent(this, argument);
	}
}
