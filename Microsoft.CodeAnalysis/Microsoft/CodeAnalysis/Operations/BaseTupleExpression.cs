using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseTupleExpression : Operation, ITupleOperation, IOperation
{
	public ITypeSymbol NaturalType { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			foreach (IOperation element in Elements)
			{
				if (element != null)
				{
					yield return element;
				}
			}
		}
	}

	public abstract ImmutableArray<IOperation> Elements { get; }

	protected BaseTupleExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, ITypeSymbol naturalType, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Tuple, semanticModel, syntax, type, constantValue, isImplicit)
	{
		NaturalType = naturalType;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitTuple(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitTuple(this, argument);
	}
}
