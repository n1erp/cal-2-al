namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal enum ConstantValueTypeDiscriminator : byte
{
	Nothing = 0,
	Null = Nothing,
	Bad = 1,
	Byte = 2,
	Int32 = 3,
	Int64 = 4,
	Char = 5,
	Boolean = 6,
	String = 7,
	Decimal = 8,
	DateTime = 9,
	Time = 10,
	Date = 11,
	Duration = 12
}
