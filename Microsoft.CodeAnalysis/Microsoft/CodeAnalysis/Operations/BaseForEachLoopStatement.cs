using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseForEachLoopStatement : LoopStatement, IForEachLoopOperation, ILoopOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Collection != null)
			{
				yield return Collection;
			}
			if (LoopControlVariable != null)
			{
				yield return LoopControlVariable;
			}
			if (Body != null)
			{
				yield return Body;
			}
			foreach (IOperation nextVariable in NextVariables)
			{
				if (nextVariable != null)
				{
					yield return nextVariable;
				}
			}
		}
	}

	public ForEachLoopOperationInfo Info { get; }

	public abstract IOperation LoopControlVariable { get; }

	public abstract IOperation Collection { get; }

	public abstract ImmutableArray<IOperation> NextVariables { get; }

	public BaseForEachLoopStatement(ImmutableArray<ILocalSymbol> locals, ILabelSymbol continueLabel, ILabelSymbol exitLabel, ForEachLoopOperationInfo info, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(LoopKind.ForEach, locals, continueLabel, exitLabel, OperationKind.Loop, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Info = info;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitForEachLoop(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitForEachLoop(this, argument);
	}
}
