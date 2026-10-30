namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class ArrayExtensions
{
	internal static int BinarySearchUpperBound(this int[] array, int value)
	{
		int num = 0;
		int num2 = array.Length - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num >> 1);
			if (array[num3] > value)
			{
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
		}
		return num;
	}

	internal static int GetLengthSafe<T>(this T[] values)
	{
		return values?.Length ?? 0;
	}
}
