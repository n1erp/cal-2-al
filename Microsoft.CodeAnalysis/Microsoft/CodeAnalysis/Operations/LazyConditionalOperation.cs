using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyConditionalOperation : BaseConditionalOperation, IConditionalOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyCondition;

	private readonly Lazy<IOperation> _lazyWhenTrue;

	private readonly Lazy<IOperation> _lazyWhenFalse;

	public override IOperation Condition => Operation.SetParentOperation(_lazyCondition.Value, this);

	public override IOperation WhenTrue => Operation.SetParentOperation(_lazyWhenTrue.Value, this);

	public override IOperation WhenFalse => Operation.SetParentOperation(_lazyWhenFalse.Value, this);

	public LazyConditionalOperation(Lazy<IOperation> condition, Lazy<IOperation> whenTrue, Lazy<IOperation> whenFalse, bool isRef, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(isRef, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyCondition = condition ?? throw new ArgumentNullException("condition");
		_lazyWhenTrue = whenTrue ?? throw new ArgumentNullException("whenTrue");
		_lazyWhenFalse = whenFalse ?? throw new ArgumentNullException("whenFalse");
	}
}
