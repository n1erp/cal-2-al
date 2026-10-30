using System;

namespace Roslyn.Utilities;

internal static class Functions<T>
{
	public static readonly Func<T, T> Identity = (T t) => t;

	public static readonly Func<T, bool> True = (T t) => true;
}
