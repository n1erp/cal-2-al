using System.Threading;

namespace Roslyn.Utilities;

internal static class ThreadSafeFlagOperations
{
	public static bool Set(ref int flags, int toSet)
	{
		int num;
		int num2;
		do
		{
			num = flags;
			num2 = num | toSet;
			if (num2 == num)
			{
				return false;
			}
		}
		while (Interlocked.CompareExchange(ref flags, num2, num) != num);
		return true;
	}

	public static bool Clear(ref int flags, int toClear)
	{
		int num;
		int num2;
		do
		{
			num = flags;
			num2 = num & ~toClear;
			if (num2 == num)
			{
				return false;
			}
		}
		while (Interlocked.CompareExchange(ref flags, num2, num) != num);
		return true;
	}
}
