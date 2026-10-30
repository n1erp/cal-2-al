using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseArrayCreationExpression : Operation, IArrayCreationOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			foreach (IOperation dimensionSize in DimensionSizes)
			{
				if (dimensionSize != null)
				{
					yield return dimensionSize;
				}
			}
			if (Initializer != null)
			{
				yield return Initializer;
			}
		}
	}

	public abstract ImmutableArray<IOperation> DimensionSizes { get; }

	public abstract IArrayInitializerOperation Initializer { get; }

	protected BaseArrayCreationExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.ArrayCreation, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitArrayCreation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitArrayCreation(this, argument);
	}
}
