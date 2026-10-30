using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseArrayElementReferenceExpression : Operation, IArrayElementReferenceOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (ArrayReference != null)
			{
				yield return ArrayReference;
			}
			foreach (IOperation index in Indices)
			{
				if (index != null)
				{
					yield return index;
				}
			}
		}
	}

	public abstract IOperation ArrayReference { get; }

	public abstract ImmutableArray<IOperation> Indices { get; }

	protected BaseArrayElementReferenceExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.ArrayElementReference, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitArrayElementReference(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitArrayElementReference(this, argument);
	}
}
