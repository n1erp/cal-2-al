using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class Boxes
{
	public static readonly object BoxedTrue = true;

	public static readonly object BoxedFalse = false;

	public static readonly object BoxedByteZero = (byte)0;

	public static readonly object BoxedSByteZero = (sbyte)0;

	public static readonly object BoxedInt16Zero = (short)0;

	public static readonly object BoxedUInt16Zero = (ushort)0;

	public static readonly object BoxedInt32Zero = 0;

	public static readonly object BoxedInt32One = 1;

	public static readonly object BoxedUInt32Zero = 0u;

	public static readonly object BoxedInt64Zero = 0L;

	public static readonly object BoxedUInt64Zero = 0uL;

	public static readonly object BoxedSingleZero = 0f;

	public static readonly object BoxedDoubleZero = 0.0;

	public static readonly object BoxedDecimalZero = 0m;

	private static readonly object[] BoxedAsciiChars = new object[128];

	public static object Box(bool b)
	{
		if (!b)
		{
			return BoxedFalse;
		}
		return BoxedTrue;
	}

	public static object Box(byte b)
	{
		if (b != 0)
		{
			return b;
		}
		return BoxedByteZero;
	}

	public static object Box(sbyte sb)
	{
		if (sb != 0)
		{
			return sb;
		}
		return BoxedSByteZero;
	}

	public static object Box(short s)
	{
		if (s != 0)
		{
			return s;
		}
		return BoxedInt16Zero;
	}

	public static object Box(ushort us)
	{
		if (us != 0)
		{
			return us;
		}
		return BoxedUInt16Zero;
	}

	public static object Box(int i)
	{
		return i switch
		{
			0 => BoxedInt32Zero, 
			1 => BoxedInt32One, 
			_ => i, 
		};
	}

	public static object Box(uint u)
	{
		if (u != 0)
		{
			return u;
		}
		return BoxedUInt32Zero;
	}

	public static object Box(long l)
	{
		if (l != 0L)
		{
			return l;
		}
		return BoxedInt64Zero;
	}

	public static object Box(ulong ul)
	{
		if (ul != 0L)
		{
			return ul;
		}
		return BoxedUInt64Zero;
	}

	public static object Box(double d)
	{
		if (BitConverter.DoubleToInt64Bits(d) != 0L)
		{
			return d;
		}
		return BoxedDoubleZero;
	}

	public static object Box(char c)
	{
		if (c >= '\u0080')
		{
			return c;
		}
		return BoxedAsciiChars[(uint)c] ?? (BoxedAsciiChars[(uint)c] = c);
	}

	public static object Box(decimal d)
	{
		return d;
	}
}
