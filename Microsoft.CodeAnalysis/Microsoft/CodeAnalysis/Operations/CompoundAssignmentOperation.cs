namespace Microsoft.CodeAnalysis.Operations;

internal sealed class CompoundAssignmentOperation : BaseCompoundAssignmentExpression
{
	public override IOperation Target { get; }

	public override IOperation Value { get; }

	public CompoundAssignmentOperation(IOperation target, IOperation value, IConvertibleConversion inConversionConvertible, IConvertibleConversion outConversionConvertible, BinaryOperatorKind operatorKind, bool isLifted, bool isChecked, IMethodSymbol operatorMethod, SemanticModel semanticModel, SyntaxNode syntax, ITypeSymbol type, Optional<object> constantValue, bool isImplicit)
		: base(inConversionConvertible, outConversionConvertible, operatorKind, isLifted, isChecked, operatorMethod, semanticModel, syntax, type, constantValue, isImplicit)
	{
		Target = Operation.SetParentOperation(target, this);
		Value = Operation.SetParentOperation(value, this);
	}
}
