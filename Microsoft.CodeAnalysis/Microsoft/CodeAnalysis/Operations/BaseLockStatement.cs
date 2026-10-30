using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Operations;

internal abstract class BaseLockStatement : Operation, ILockOperation, IOperation
{
	public override IEnumerable<IOperation> Children
	{
		get
		{
			if (LockedValue != null)
			{
				yield return LockedValue;
			}
			if (Body != null)
			{
				yield return Body;
			}
		}
	}

	public abstract IOperation LockedValue { get; }

	public abstract IOperation Body { get; }

	public ILocalSymbol LockTakenSymbol { get; }

	protected BaseLockStatement(ILocalSymbol lockTakenSymbol, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(OperationKind.Lock, semanticModel, syntax, type, constantValue, isImplicit)
	{
		LockTakenSymbol = lockTakenSymbol;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitLock(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitLock(this, argument);
	}
}
