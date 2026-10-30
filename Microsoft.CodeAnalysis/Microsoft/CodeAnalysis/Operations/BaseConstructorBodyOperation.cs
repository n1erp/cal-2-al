using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseConstructorBodyOperation : BaseMethodBodyBaseOperation, IConstructorBodyOperation, IMethodBodyBaseOperation, IOperation
{
	public ImmutableArray<ILocalSymbol> Locals { get; }

	public abstract IOperation Initializer { get; }

	public sealed override IEnumerable<IOperation> Children
	{
		get
		{
			IOperation initializer = Initializer;
			if (initializer != null)
			{
				yield return initializer;
			}
			IBlockOperation blockBody = BlockBody;
			if (blockBody != null)
			{
				yield return blockBody;
			}
			IBlockOperation expressionBody = ExpressionBody;
			if (expressionBody != null)
			{
				yield return expressionBody;
			}
		}
	}

	protected BaseConstructorBodyOperation(ImmutableArray<ILocalSymbol> locals, SemanticModel semanticModel, SyntaxNode syntax)
		: base(OperationKind.ConstructorBodyOperation, semanticModel, syntax)
	{
		Locals = locals;
	}

	public sealed override void Accept(OperationVisitor visitor)
	{
		visitor.VisitConstructorBodyOperation(this);
	}

	public sealed override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitConstructorBodyOperation(this, argument);
	}
}
