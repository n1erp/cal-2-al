using System.Collections.Immutable;
using System.Reflection.Emit;

namespace Microsoft.Cci;

internal static class InstructionOperandTypes
{
	internal static readonly byte[] OneByte = new byte[255]
	{
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 18, 18, 18, 18, 18, 18,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 16, 2, 3, 17, 7, 0, 5, 5, 4,
		4, 9, 5, 15, 15, 15, 15, 15, 15, 15,
		15, 15, 15, 15, 15, 15, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 11,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 4, 13, 13, 10, 4, 13, 13, 5, 0,
		0, 13, 5, 1, 1, 1, 1, 1, 1, 13,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		13, 13, 5, 13, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 5, 5, 13, 13, 13, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 5,
		5, 5, 5, 5, 5, 5, 5, 0, 0, 0,
		0, 0, 0, 0, 13, 5, 0, 0, 13, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 12, 5,
		5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
		5, 0, 15, 5, 5, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0
	};

	internal static readonly byte[] TwoByte = new byte[31]
	{
		5, 5, 5, 5, 5, 5, 4, 4, 0, 14,
		14, 14, 14, 14, 14, 5, 0, 5, 16, 5,
		5, 13, 13, 5, 5, 0, 5, 0, 13, 5,
		5
	};

	internal static OperandType ReadOperandType(ImmutableArray<byte> il, ref int position)
	{
		byte b = il[position++];
		if (b == 254)
		{
			return (OperandType)TwoByte[il[position++]];
		}
		return (OperandType)OneByte[b];
	}
}
