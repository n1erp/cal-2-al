using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyLockStatement : BaseLockStatement, ILockOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyLockedValue;

	private readonly Lazy<IOperation> _lazyBody;

	public override IOperation LockedValue => Operation.SetParentOperation(_lazyLockedValue.Value, this);

	public override IOperation Body => Operation.SetParentOperation(_lazyBody.Value, this);

	public LazyLockStatement(Lazy<IOperation> lockedValue, Lazy<IOperation> body, ILocalSymbol lockTakenSymbol, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(lockTakenSymbol, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyLockedValue = lockedValue ?? throw new ArgumentNullException("lockedValue");
		_lazyBody = body ?? throw new ArgumentNullException("body");
	}
}
