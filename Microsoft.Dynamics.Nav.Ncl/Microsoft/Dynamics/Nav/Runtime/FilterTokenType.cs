namespace Microsoft.Dynamics.Nav.Runtime;

internal enum FilterTokenType
{
	Equal,
	NotEqual,
	GreaterThan,
	LessThan,
	GreaterThanOrEqual,
	LessThanOrEqual,
	And,
	Or,
	DotDot,
	OpenParenthesis,
	CloseParenthesis,
	Literal,
	BinaryValue,
	BinaryWildcard,
	BinaryNegatedWildcard
}
