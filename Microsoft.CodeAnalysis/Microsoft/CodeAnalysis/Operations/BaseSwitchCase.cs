using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseSwitchCase : Operation, ISwitchCaseOperation, IOperation
{
	public ImmutableArray<ILocalSymbol> Locals { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			foreach (ICaseClauseOperation clause in Clauses)
			{
				if (clause != null)
				{
					yield return clause;
				}
			}
			foreach (IOperation item in Body)
			{
				if (item != null)
				{
					yield return item;
				}
			}
		}
	}

	public abstract ImmutableArray<ICaseClauseOperation> Clauses { get; }

	public abstract ImmutableArray<IOperation> Body { get; }

	public abstract IOperation Condition { get; }

	protected BaseSwitchCase(ImmutableArray<ILocalSymbol> locals, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.SwitchCase, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Locals = locals;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitSwitchCase(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitSwitchCase(this, argument);
	}
}
