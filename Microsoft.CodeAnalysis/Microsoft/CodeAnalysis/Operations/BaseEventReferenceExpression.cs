using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseEventReferenceExpression : MemberReferenceExpression, IEventReferenceOperation, IMemberReferenceOperation, IOperation
{
	public IEventSymbol Event => (IEventSymbol)Member;

	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Instance != null)
			{
				yield return Instance;
			}
		}
	}

	public BaseEventReferenceExpression(IEventSymbol @event, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(@event, OperationKind.EventReference, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitEventReference(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitEventReference(this, argument);
	}
}
