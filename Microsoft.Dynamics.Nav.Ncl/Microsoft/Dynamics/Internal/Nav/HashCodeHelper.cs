namespace Microsoft.Dynamics.Internal.Nav;

internal static class HashCodeHelper
{
	private const int DJB2SEED = 5381;

	public static int Djb2HashCode(byte[] value)
	{
		if (value == null)
		{
			return 0;
		}
		int num = 5381;
		for (int i = 0; i < value.Length; i++)
		{
			num = (num << 5) + num + value[i];
		}
		return num;
	}

	public static int CombineHashCodes(int h1, int h2)
	{
		return ((h1 << 5) + h1 + (h1 >> 27)) ^ h2;
	}

	public static int CombineHashCodes(int h1, int h2, int h3)
	{
		return CombineHashCodes(CombineHashCodes(h1, h2), h3);
	}

	internal static int CombineHashCodes(int h1, int h2, int h3, int h4)
	{
		return CombineHashCodes(CombineHashCodes(h1, h2, h3), h4);
	}

	internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5)
	{
		return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4), h5);
	}

	internal static int CombineHashCodes(int h1, int h2, int h3, int h4, int h5, int h6)
	{
		return CombineHashCodes(CombineHashCodes(h1, h2, h3, h4, h5), h6);
	}
}
