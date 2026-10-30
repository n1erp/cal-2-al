using System;

namespace Roslyn.Utilities;

internal static class WeakReferenceExtensions
{
	public static T GetTarget<T>(this WeakReference<T> reference) where T : class
	{
		reference.TryGetTarget(out var target);
		return target;
	}

	public static bool IsNull<T>(this WeakReference<T> reference) where T : class
	{
		T target;
		return !reference.TryGetTarget(out target);
	}
}
