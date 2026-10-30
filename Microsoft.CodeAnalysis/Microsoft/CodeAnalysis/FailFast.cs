using System;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis;

internal static class FailFast
{
	[DebuggerHidden]
	internal static void OnFatalException(Exception exception)
	{
		if (Debugger.IsAttached)
		{
			Debugger.Break();
		}
		if (exception is AggregateException ex && ex.InnerExceptions.Count == 1)
		{
			exception = ex.InnerExceptions[0];
		}
		Environment.FailFast(exception.ToString(), exception);
	}

	[Conditional("DEBUG")]
	[DebuggerHidden]
	internal static void Assert(bool condition, string message = null)
	{
		if (!condition)
		{
			if (Debugger.IsAttached)
			{
				Debugger.Break();
			}
			Environment.FailFast("ASSERT FAILED" + Environment.NewLine + message);
		}
	}
}
