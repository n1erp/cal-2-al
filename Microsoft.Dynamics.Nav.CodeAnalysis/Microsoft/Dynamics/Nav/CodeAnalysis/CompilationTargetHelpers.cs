using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class CompilationTargetHelpers
{
	internal static CompilationTarget ParseSafe(string value)
	{
		if (!Enum.TryParse<CompilationTarget>(value, out var result))
		{
			return CompilationTarget.Extension;
		}
		return result;
	}

	internal static bool IsPersonalizationTarget(this CompilationTarget target)
	{
		return target == CompilationTarget.Personalization;
	}

	internal static bool IsExtensionTarget(this CompilationTarget target)
	{
		return target == CompilationTarget.Extension;
	}

	internal static bool IsInternalTarget(this CompilationTarget target)
	{
		return target == CompilationTarget.Internal;
	}

	internal static bool IsSolutionTarget(this CompilationTarget target)
	{
		return target == CompilationTarget.Solution;
	}
}
