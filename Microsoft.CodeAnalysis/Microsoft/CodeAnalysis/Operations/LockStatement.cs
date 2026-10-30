namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LockStatement : BaseLockStatement, ILockOperation, IOperation
{
	public override IOperation LockedValue { get; }

	public override IOperation Body { get; }

	public LockStatement(IOperation lockedValue, IOperation body, ILocalSymbol lockTakenSymbol, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(lockTakenSymbol, semanticModel, syntax, type, constantValue, isImplicit)
	{
		LockedValue = Operation.SetParentOperation(lockedValue, this);
		Body = Operation.SetParentOperation(body, this);
	}
}
