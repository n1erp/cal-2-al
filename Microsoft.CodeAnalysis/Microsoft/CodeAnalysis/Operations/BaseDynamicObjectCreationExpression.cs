using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseDynamicObjectCreationExpression : HasDynamicArgumentsExpression, IDynamicObjectCreationOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			foreach (IOperation argument in Arguments)
			{
				if (argument != null)
				{
					yield return argument;
				}
			}
			if (Initializer != null)
			{
				yield return Initializer;
			}
		}
	}

	public abstract IObjectOrCollectionInitializerOperation Initializer { get; }

	public BaseDynamicObjectCreationExpression(ImmutableArray<string> argumentNames, ImmutableArray<RefKind> argumentRefKinds, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.DynamicObjectCreation, argumentNames, argumentRefKinds, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitDynamicObjectCreation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitDynamicObjectCreation(this, argument);
	}
}
