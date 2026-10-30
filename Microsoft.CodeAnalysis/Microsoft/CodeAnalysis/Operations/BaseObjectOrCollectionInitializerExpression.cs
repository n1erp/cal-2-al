using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseObjectOrCollectionInitializerExpression : Operation, IObjectOrCollectionInitializerOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			foreach (IOperation initializer in Initializers)
			{
				if (initializer != null)
				{
					yield return initializer;
				}
			}
		}
	}

	public abstract ImmutableArray<IOperation> Initializers { get; }

	protected BaseObjectOrCollectionInitializerExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.ObjectOrCollectionInitializer, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitObjectOrCollectionInitializer(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitObjectOrCollectionInitializer(this, argument);
	}
}
