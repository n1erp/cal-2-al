namespace Microsoft.CodeAnalysis.Operations;

public enum BinaryOperatorKind
{
	None,
	Add,
	Subtract,
	Multiply,
	Divide,
	IntegerDivide,
	Remainder,
	Power,
	LeftShift,
	RightShift,
	And,
	Or,
	ExclusiveOr,
	ConditionalAnd,
	ConditionalOr,
	Concatenate,
	Equals,
	ObjectValueEquals,
	NotEquals,
	ObjectValueNotEquals,
	LessThan,
	LessThanOrEqual,
	GreaterThanOrEqual,
	GreaterThan,
	Like
}
