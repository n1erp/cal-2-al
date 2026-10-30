namespace Microsoft.Dynamics.Nav.Types;

public static class ByteArrayHelper
{
	public unsafe static bool Compare(byte[] binary1, byte[] binary2)
	{
		if (binary1 == null && binary2 == null)
		{
			return true;
		}
		if (binary1 == null || binary2 == null)
		{
			return false;
		}
		if (binary1.Length != binary2.Length)
		{
			return false;
		}
		int num = binary1.Length / 8;
		int num2 = binary1.Length % 8;
		int num3 = num * 8;
		fixed (byte* ptr = binary1)
		{
			fixed (byte* ptr2 = binary2)
			{
				long* ptr3 = (long*)ptr;
				long* ptr4 = (long*)ptr2;
				for (int i = 0; i < num; i++)
				{
					if (ptr4[i] != ptr3[i])
					{
						return false;
					}
				}
			}
		}
		for (int j = num3; j < num2; j++)
		{
			if (binary1[j] != binary2[j])
			{
				return false;
			}
		}
		return true;
	}
}
