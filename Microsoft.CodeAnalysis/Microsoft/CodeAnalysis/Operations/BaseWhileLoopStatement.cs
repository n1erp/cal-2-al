using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseWhileLoopStatement : LoopStatement, IWhileLoopOperation, ILoopOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (ConditionIsTop && Condition != null)
			{
				yield return Condition;
			}
			if (Body != null)
			{
				yield return Body;
			}
			if (!ConditionIsTop && Condition != null)
			{
				yield return Condition;
			}
			if (IgnoredCondition != null)
			{
				yield return IgnoredCondition;
			}
		}
	}

	public abstract IOperation Condition { get; }

	public bool ConditionIsTop { get; }

	public bool ConditionIsUntil { get; }

	public abstract IOperation IgnoredCondition { get; }

	public BaseWhileLoopStatement(ImmutableArray<ILocalSymbol> locals, ILabelSymbol continueLabel, ILabelSymbol exitLabel, bool conditionIsTop, bool conditionIsUntil, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(LoopKind.While, locals, continueLabel, exitLabel, OperationKind.Loop, semanticModel, syntax, type, constantValue, isImplicit)
	{
		ConditionIsTop = conditionIsTop;
		ConditionIsUntil = conditionIsUntil;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitWhileLoop(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitWhileLoop(this, argument);
	}
}
