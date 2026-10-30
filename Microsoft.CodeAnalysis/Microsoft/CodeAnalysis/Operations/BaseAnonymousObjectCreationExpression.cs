using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseAnonymousObjectCreationExpression : Operation, IAnonymousObjectCreationOperation, IOperation
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

	protected BaseAnonymousObjectCreationExpression(SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.AnonymousObjectCreation, semanticModel, syntax, type, constantValue, isImplicit)
	{
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitAnonymousObjectCreation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitAnonymousObjectCreation(this, argument);
	}
}
