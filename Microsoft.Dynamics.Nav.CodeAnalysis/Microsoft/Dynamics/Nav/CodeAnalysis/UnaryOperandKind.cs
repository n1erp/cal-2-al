namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public enum UnaryOperandKind
{
	None = 0,
	OperatorMethod = 256,
	Integer = 512,
	Decimal = 1280,
	Boolean = 1536,
	Enum = 1792,
	Invalid = 65280
}
