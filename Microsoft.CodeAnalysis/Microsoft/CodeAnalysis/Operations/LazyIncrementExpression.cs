using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyIncrementExpression : BaseIncrementExpression, IIncrementOrDecrementOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyTarget;

	public override IOperation Target => Operation.SetParentOperation(_lazyTarget.Value, this);

	public LazyIncrementExpression(bool isDecrement, bool isPostfix, bool isLifted, bool isChecked, Lazy<IOperation> target, IMethodSymbol operatorMethod, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(isDecrement, isPostfix, isLifted, isChecked, operatorMethod, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyTarget = target ?? throw new ArgumentNullException("target");
	}
}
