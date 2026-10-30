using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseForToLoopStatement : LoopStatement, IForToLoopOperation, ILoopOperation, IOperation
{
	public bool IsChecked { get; }

	public (ILocalSymbol LoopObject, ForToLoopOperationUserDefinedInfo UserDefinedInfo) Info { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (LoopControlVariable != null)
			{
				yield return LoopControlVariable;
			}
			if (InitialValue != null)
			{
				yield return InitialValue;
			}
			if (LimitValue != null)
			{
				yield return LimitValue;
			}
			if (StepValue != null)
			{
				yield return StepValue;
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

	public abstract IOperation LoopControlVariable { get; }

	public abstract IOperation InitialValue { get; }

	public abstract IOperation LimitValue { get; }

	public abstract IOperation StepValue { get; }

	public abstract ImmutableArray<IOperation> NextVariables { get; }

	public BaseForToLoopStatement(ImmutableArray<ILocalSymbol> locals, bool isChecked, (ILocalSymbol LoopObject, ForToLoopOperationUserDefinedInfo UserDefinedInfo) info, ILabelSymbol continueLabel, ILabelSymbol exitLabel, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(LoopKind.ForTo, locals, continueLabel, exitLabel, OperationKind.Loop, semanticModel, syntax, type, constantValue, isImplicit)
	{
		IsChecked = isChecked;
		Info = info;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitForToLoop(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitForToLoop(this, argument);
	}
}
