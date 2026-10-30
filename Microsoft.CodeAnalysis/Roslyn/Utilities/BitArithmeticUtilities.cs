namespace Roslyn.Utilities;

internal static class BitArithmeticUtilities
{
	public static int CountBits(int v)
	{
		return CountBits((uint)v);
	}

	public static int CountBits(uint v)
	{
		v -= (v >> 1) & 0x55555555;
		v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
		return (int)(((v + (v >> 4)) & 0xF0F0F0F) * 16843009) >> 24;
	}

	public static int CountBits(long v)
	{
		return CountBits((ulong)v);
	}

	public static int CountBits(ulong v)
	{
		v = (v & 0x5555555555555555L) + ((v >> 1) & 0x5555555555555555L);
		v = (v & 0x3333333333333333L) + ((v >> 2) & 0x3333333333333333L);
		v = (v & 0xF0F0F0F0F0F0F0FL) + ((v >> 4) & 0xF0F0F0F0F0F0F0FL);
		v = (v & 0xFF00FF00FF00FFL) + ((v >> 8) & 0xFF00FF00FF00FFL);
		v = (v & 0xFFFF0000FFFFL) + ((v >> 16) & 0xFFFF0000FFFFL);
		v = (v & 0xFFFFFFFFu) + ((v >> 32) & 0xFFFFFFFFu);
		return (int)v;
	}

	internal static uint Align(uint position, uint alignment)
	{
		uint num = position & ~(alignment - 1);
		if (num == position)
		{
			return num;
		}
		return num + alignment;
	}

	internal static int Align(int position, int alignment)
	{
		int num = position & ~(alignment - 1);
		if (num == position)
		{
			return num;
		}
		return num + alignment;
	}
}
