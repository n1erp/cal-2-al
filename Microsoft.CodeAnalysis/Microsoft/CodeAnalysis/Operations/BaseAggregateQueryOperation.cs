using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseAggregateQueryOperation : Operation, IAggregateQueryOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Group != null)
			{
				yield return Group;
			}
			if (Aggregation != null)
			{
				yield return Aggregation;
			}
		}
	}

	public abstract IOperation Group { get; }

	public abstract IOperation Aggregation { get; }

	protected BaseAggregateQueryOperation(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.None, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitAggregateQuery(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitAggregateQuery(this, argument);
	}
}
