using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyDelegateCreationExpression : BaseDelegateCreationExpression
{
	private readonly Lazy<IOperation> _lazyTarget;

	public override IOperation Target => Operation.SetParentOperation(_lazyTarget.Value, this);

	public LazyDelegateCreationExpression(Lazy<IOperation> lazyTarget, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyTarget = lazyTarget;
	}
}
