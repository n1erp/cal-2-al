using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseObjectCreationExpression : Operation, IObjectCreationOperation, IOperation
{
	public IMethodSymbol Constructor { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			foreach (IArgumentOperation argument in Arguments)
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

	public abstract ImmutableArray<IArgumentOperation> Arguments { get; }

	protected BaseObjectCreationExpression(IMethodSymbol constructor, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.ObjectCreation, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Constructor = constructor;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitObjectCreation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitObjectCreation(this, argument);
	}
}
