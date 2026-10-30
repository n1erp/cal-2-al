namespace Microsoft.CodeAnalysis;

internal enum ConstantValueTypeDiscriminator : byte
{
	Nothing = 0,
	Null = Nothing,
	Bad = 1,
	SByte = 2,
	Byte = 3,
	Int16 = 4,
	UInt16 = 5,
	Int32 = 6,
	UInt32 = 7,
	Int64 = 8,
	UInt64 = 9,
	Char = 10,
	Boolean = 11,
	Single = 12,
	Double = 13,
	String = 14,
	Decimal = 15,
	DateTime = 16
}
