using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseBlockStatement : Operation, IBlockOperation, IOperation
{
	public ImmutableArray<ILocalSymbol> Locals { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			foreach (IOperation operation in Operations)
			{
				if (operation != null)
				{
					yield return operation;
				}
			}
		}
	}

	public abstract ImmutableArray<IOperation> Operations { get; }

	protected BaseBlockStatement(ImmutableArray<ILocalSymbol> locals, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Block, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Locals = locals;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitBlock(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitBlock(this, argument);
	}
}
