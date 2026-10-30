namespace Microsoft.CodeAnalysis.Operations;

internal sealed class CoalesceExpression : BaseCoalesceExpression, ICoalesceOperation, IOperation
{
	public override IOperation Value { get; }

	public override IOperation WhenNull { get; }

	public CoalesceExpression(IOperation value, IOperation whenNull, IConvertibleConversion convertibleValueConversion, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(convertibleValueConversion, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Value = Operation.SetParentOperation(value, this);
		WhenNull = Operation.SetParentOperation(whenNull, this);
	}
}
