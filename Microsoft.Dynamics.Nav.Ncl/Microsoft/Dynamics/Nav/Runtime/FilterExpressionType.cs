namespace Microsoft.Dynamics.Nav.Runtime;

public enum FilterExpressionType
{
	Equal,
	NotEqual,
	GreaterThan,
	LessThan,
	GreaterThanOrEqual,
	LessThanOrEqual,
	And,
	Or,
	RangeBetweenInclusive,
	RangeFromInclusive,
	RangeToInclusive,
	Wildcard,
	BooleanConstant,
	FieldEqualsField
}
