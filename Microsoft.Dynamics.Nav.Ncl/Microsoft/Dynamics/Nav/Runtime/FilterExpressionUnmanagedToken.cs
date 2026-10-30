namespace Microsoft.Dynamics.Nav.Runtime;

internal enum FilterExpressionUnmanagedToken : byte
{
	Terminator,
	Terminator2,
	Equal,
	NotEqual,
	GreaterOrEqual,
	LessThan,
	LessThanOrEqual,
	GreaterThan,
	Wild,
	NotWild,
	And,
	Or,
	LeftParenthesis,
	RightParenthesis
}
