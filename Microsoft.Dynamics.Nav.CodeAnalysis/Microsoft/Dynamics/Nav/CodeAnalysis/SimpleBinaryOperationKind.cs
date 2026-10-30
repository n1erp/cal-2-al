namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public enum SimpleBinaryOperationKind
{
	None = 0,
	Add = 1,
	Subtract = 2,
	Multiply = 3,
	Divide = 4,
	IntegerDivide = 5,
	Remainder = 6,
	Power = 7,
	LeftShift = 8,
	RightShift = 9,
	And = 10,
	Or = 11,
	ExclusiveOr = 12,
	ConditionalAnd = 13,
	ConditionalOr = 14,
	Concatenate = 15,
	Equals = 16,
	ObjectValueEquals = 17,
	NotEquals = 18,
	ObjectValueNotEquals = 19,
	LessThan = 20,
	LessThanOrEqual = 21,
	GreaterThanOrEqual = 22,
	GreaterThan = 23,
	Invalid = 255
}
