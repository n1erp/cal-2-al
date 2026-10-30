using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyCoalesceExpression : BaseCoalesceExpression, ICoalesceOperation, IOperation
{
	private readonly Lazy<IOperation> _lazyValue;

	private readonly Lazy<IOperation> _lazyWhenNull;

	public override IOperation Value => Operation.SetParentOperation(_lazyValue.Value, this);

	public override IOperation WhenNull => Operation.SetParentOperation(_lazyWhenNull.Value, this);

	public LazyCoalesceExpression(Lazy<IOperation> value, Lazy<IOperation> whenNull, IConvertibleConversion convertibleValueConversion, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(convertibleValueConversion, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyValue = value ?? throw new ArgumentNullException("value");
		_lazyWhenNull = whenNull ?? throw new ArgumentNullException("whenNull");
	}
}
