using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseUsingStatement : Operation, IUsingOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Resources != null)
			{
				yield return Resources;
			}
			if (Body != null)
			{
				yield return Body;
			}
		}
	}

	public abstract IOperation Resources { get; }

	public abstract IOperation Body { get; }

	public ImmutableArray<ILocalSymbol> Locals { get; }

	protected BaseUsingStatement(ImmutableArray<ILocalSymbol> locals, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Using, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Locals = locals;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitUsing(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitUsing(this, argument);
	}
}
