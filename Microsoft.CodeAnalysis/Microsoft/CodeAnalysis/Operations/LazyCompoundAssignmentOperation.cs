using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyCompoundAssignmentOperation : BaseCompoundAssignmentExpression
{
	private readonly Lazy<IOperation> _lazyTarget;

	private readonly Lazy<IOperation> _lazyValue;

	public override IOperation Target => Operation.SetParentOperation(_lazyTarget.Value, this);

	public override IOperation Value => Operation.SetParentOperation(_lazyValue.Value, this);

	public LazyCompoundAssignmentOperation(Lazy<IOperation> target, Lazy<IOperation> value, IConvertibleConversion inConversionConvertible, IConvertibleConversion outConversionConvertible, BinaryOperatorKind operatorKind, bool isLifted, bool isChecked, IMethodSymbol operatorMethod, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(inConversionConvertible, outConversionConvertible, operatorKind, isLifted, isChecked, operatorMethod, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyTarget = target;
		_lazyValue = value;
	}
}
