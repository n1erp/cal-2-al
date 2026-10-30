using System.Reflection.Metadata;

namespace Microsoft.CodeAnalysis;

internal static class SynthesizedLocalKindExtensions
{
	public static bool IsLongLived(this SynthesizedLocalKind kind)
	{
		return kind >= SynthesizedLocalKind.UserDefined;
	}

	public static bool MustSurviveStateMachineSuspension(this SynthesizedLocalKind kind)
	{
		if (kind.IsLongLived())
		{
			return kind != SynthesizedLocalKind.ConditionalBranchDiscriminator;
		}
		return false;
	}

	public static bool IsSlotReusable(this SynthesizedLocalKind kind, OptimizationLevel optimizations)
	{
		return kind.IsSlotReusable(optimizations != OptimizationLevel.Release);
	}

	public static bool IsSlotReusable(this SynthesizedLocalKind kind, bool isDebug)
	{
		if (isDebug)
		{
			return !kind.IsLongLived();
		}
		if (kind == SynthesizedLocalKind.UserDefined || kind == SynthesizedLocalKind.With || kind == SynthesizedLocalKind.LambdaDisplayClass)
		{
			return false;
		}
		return true;
	}

	public static LocalVariableAttributes PdbAttributes(this SynthesizedLocalKind kind)
	{
		if (kind == SynthesizedLocalKind.LambdaDisplayClass || kind == SynthesizedLocalKind.UserDefined || kind == SynthesizedLocalKind.With)
		{
			return LocalVariableAttributes.None;
		}
		return LocalVariableAttributes.DebuggerHidden;
	}
}
