using System;
using System.Collections.Immutable;
using System.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class Hash
{
	public static readonly int RandomSeed = new Random().Next(int.MinValue, int.MaxValue);

	internal const int FnvOffsetBias = -2128831035;

	internal const int FnvPrime = 16777619;

	public static int Combine(int h1, int h2)
	{
		return ((h1 << 5) + h1 + (h1 >> 27)) ^ h2;
	}

	public static int Combine(int h1, int h2, int h3)
	{
		return Combine(Combine(h1, h2), h3);
	}

	internal static int Combine(int h1, int h2, int h3, int h4)
	{
		return Combine(Combine(h1, h2, h3), h4);
	}

	internal static int Combine(int h1, int h2, int h3, int h4, int h5)
	{
		return Combine(Combine(h1, h2, h3, h4), h5);
	}

	internal static int Combine(int h1, int h2, int h3, int h4, int h5, int h6)
	{
		return Combine(Combine(h1, h2, h3, h4, h5), h6);
	}

	internal static int Combine<T>(T newKeyPart, int currentKey) where T : class
	{
		int num = currentKey * -1521134295;
		if (newKeyPart != null)
		{
			return num + newKeyPart.GetHashCode();
		}
		return num;
	}

	internal static int CombineValues(params object[] values)
	{
		if (values.Length == 0)
		{
			return 0;
		}
		int num = values[0].GetHashCode();
		for (int i = 1; i < values.Length - 1; i++)
		{
			num = Combine(num, values[i].GetHashCode());
		}
		return num;
	}

	internal static int GetFNVHashCode(string s)
	{
		return GetFNVHashCode(Encoding.Unicode.GetBytes(s));
	}

	internal static int GetFNVHashCode(byte[] data)
	{
		int num = -2128831035;
		for (int i = 0; i < data.Length; i++)
		{
			num = (num ^ data[i]) * 16777619;
		}
		return num;
	}

	internal static int GetFNVHashCode(ImmutableArray<byte> data)
	{
		int num = -2128831035;
		for (int i = 0; i < data.Length; i++)
		{
			num = (num ^ data[i]) * 16777619;
		}
		return num;
	}

	internal static int CombineFNVHash(int hashCode, char ch)
	{
		return (hashCode ^ ch) * 16777619;
	}
}
