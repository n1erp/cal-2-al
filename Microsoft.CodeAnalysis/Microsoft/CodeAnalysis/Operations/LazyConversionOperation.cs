using System;

namespace Microsoft.CodeAnalysis.Operations;

internal sealed class LazyConversionOperation : BaseConversionExpression
{
	private readonly Lazy<IOperation> _lazyOperand;

	public override IOperation Operand => Operation.SetParentOperation(_lazyOperand.Value, this);

	public LazyConversionOperation(Lazy<IOperation> lazyOperand, IConvertibleConversion convertibleConversion, bool isTryCast, bool isChecked, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(convertibleConversion, isTryCast, isChecked, semanticModel, syntax, type, constantValue, isImplicit)
	{
		_lazyOperand = lazyOperand;
	}
}
