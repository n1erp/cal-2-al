namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public enum BinaryOperandsKind
{
	None = 0,
	Integer = 512,
	Decimal = 1280,
	Boolean = 1536,
	Enum = 1792,
	String = 3328,
	Invalid = 65280
}
