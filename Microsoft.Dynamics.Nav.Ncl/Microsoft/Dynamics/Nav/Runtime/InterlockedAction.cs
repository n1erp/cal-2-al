using System;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class InterlockedAction
{
	public static bool Dispose<T>(ref T instance) where T : class, IDisposable
	{
		T val = Volatile.Read(ref instance);
		if (val == null)
		{
			return false;
		}
		if (Interlocked.CompareExchange(ref instance, null, val) == null)
		{
			return false;
		}
		val.Dispose();
		return true;
	}
}
