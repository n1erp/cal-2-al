using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseForLoopStatement : LoopStatement, IForLoopOperation, ILoopOperation, IOperation
{
	public ImmutableArray<ILocalSymbol> ConditionLocals { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			foreach (IOperation item in Before)
			{
				if (item != null)
				{
					yield return item;
				}
			}
			if (Condition != null)
			{
				yield return Condition;
			}
			if (Body != null)
			{
				yield return Body;
			}
			foreach (IOperation item2 in AtLoopBottom)
			{
				if (item2 != null)
				{
					yield return item2;
				}
			}
		}
	}

	public abstract ImmutableArray<IOperation> Before { get; }

	public abstract IOperation Condition { get; }

	public abstract ImmutableArray<IOperation> AtLoopBottom { get; }

	public BaseForLoopStatement(ImmutableArray<ILocalSymbol> locals, ImmutableArray<ILocalSymbol> conditionLocals, ILabelSymbol continueLabel, ILabelSymbol exitLabel, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(LoopKind.For, locals, continueLabel, exitLabel, OperationKind.Loop, semanticModel, syntax, type, constantValue, isImplicit)
	{
		ConditionLocals = conditionLocals;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitForLoop(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitForLoop(this, argument);
	}
}
