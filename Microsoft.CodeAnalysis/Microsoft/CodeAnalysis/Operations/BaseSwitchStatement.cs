using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseSwitchStatement : Operation, ISwitchOperation, IOperation
{
	public ImmutableArray<ILocalSymbol> Locals { get; }

	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (Value != null)
			{
				yield return Value;
			}
			foreach (ISwitchCaseOperation @case in Cases)
			{
				if (@case != null)
				{
					yield return @case;
				}
			}
		}
	}

	public ILabelSymbol ExitLabel { get; }

	public abstract IOperation Value { get; }

	public abstract ImmutableArray<ISwitchCaseOperation> Cases { get; }

	protected BaseSwitchStatement(ImmutableArray<ILocalSymbol> locals, ILabelSymbol exitLabel, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Switch, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Locals = locals;
		ExitLabel = exitLabel;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitSwitch(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitSwitch(this, argument);
	}
}
