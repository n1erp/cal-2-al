namespace Microsoft.CodeAnalysis.Operations;

internal enum PlaceholderKind
{
	Unspecified,
	SwitchOperationExpression,
	ForToLoopBinaryOperatorLeftOperand,
	ForToLoopBinaryOperatorRightOperand,
	AggregationGroup
}
