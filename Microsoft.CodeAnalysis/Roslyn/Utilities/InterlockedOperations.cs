using System.Collections.Immutable;
using System.Threading;

namespace Roslyn.Utilities;

internal static class InterlockedOperations
{
	public static T Initialize<T>(ref T target, T value) where T : class
	{
		return Interlocked.CompareExchange(ref target, value, null) ?? value;
	}

	public static T Initialize<T>(ref T target, T initializedValue, T uninitializedValue) where T : class
	{
		T val = Interlocked.CompareExchange(ref target, initializedValue, uninitializedValue);
		if (val != uninitializedValue)
		{
			return val;
		}
		return initializedValue;
	}

	public static ImmutableArray<T> Initialize<T>(ref ImmutableArray<T> target, ImmutableArray<T> initializedValue)
	{
		ImmutableArray<T> result = ImmutableInterlocked.InterlockedCompareExchange(ref target, initializedValue, default);
		if (!result.IsDefault)
		{
			return result;
		}
		return initializedValue;
	}
}
