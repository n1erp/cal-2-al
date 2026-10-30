using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class StackGuard
{
	public const int MaxUncheckedRecursionDepth = 20;

	public static void EnsureSufficientExecutionStack(int recursionDepth)
	{
		if (recursionDepth > 20)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
		}
	}

	public static bool IsInsufficientExecutionStackException(Exception ex)
	{
		return ex.GetType().Name == "InsufficientExecutionStackException";
	}
}
