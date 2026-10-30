using System;
using System.Runtime.InteropServices;

namespace Microsoft.DiaSymReader;

[StructLayout(LayoutKind.Explicit)]
internal struct VariantStructure
{
	[FieldOffset(0)]
	private readonly short _type;

	[FieldOffset(8)]
	private readonly long _longValue;

	[FieldOffset(8)]
	private readonly VariantPadding _padding;

	[FieldOffset(0)]
	private readonly decimal _decimalValue;

	[FieldOffset(8)]
	private readonly bool _boolValue;

	[FieldOffset(8)]
	private readonly long _intValue;

	[FieldOffset(8)]
	private readonly double _doubleValue;

	public VariantStructure(DateTime date)
	{
		this = default;
		_longValue = date.Ticks;
		_type = 7;
	}
}
