namespace Microsoft.CodeAnalysis.Operations;

internal sealed class ConversionOperation : BaseConversionExpression
{
	public override IOperation Operand { get; }

	public ConversionOperation(IOperation operand, IConvertibleConversion convertibleConversion, bool isTryCast, bool isChecked, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(convertibleConversion, isTryCast, isChecked, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Operand = Operation.SetParentOperation(operand, this);
	}
}
