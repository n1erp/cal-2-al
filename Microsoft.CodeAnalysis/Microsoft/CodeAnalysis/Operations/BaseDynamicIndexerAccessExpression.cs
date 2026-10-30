using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseDynamicIndexerAccessExpression : HasDynamicArgumentsExpression, IDynamicIndexerAccessOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Operation != null)
			{
				yield return Operation;
			}
			foreach (IOperation argument in Arguments)
			{
				if (argument != null)
				{
					yield return argument;
				}
			}
		}
	}

	public abstract IOperation Operation { get; }

	public BaseDynamicIndexerAccessExpression(ImmutableArray<string> argumentNames, ImmutableArray<RefKind> argumentRefKinds, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.DynamicIndexerAccess, argumentNames, argumentRefKinds, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitDynamicIndexerAccess(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitDynamicIndexerAccess(this, argument);
	}
}
