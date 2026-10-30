namespace Microsoft.CodeAnalysis;

internal static class SourceCodeKindExtensions
{
	internal static SourceCodeKind MapSpecifiedToEffectiveKind(this SourceCodeKind kind)
	{
		if (kind != SourceCodeKind.Regular && (uint)(kind - 1) <= 1u)
		{
			return SourceCodeKind.Script;
		}
		return SourceCodeKind.Regular;
	}

	internal static bool IsValid(this SourceCodeKind value)
	{
		if (value >= SourceCodeKind.Regular)
		{
			return value <= SourceCodeKind.Script;
		}
		return false;
	}
}
