namespace Microsoft.CodeAnalysis;

internal static class ThreeStateHelpers
{
	public static ThreeState ToThreeState(this bool value)
	{
		if (!value)
		{
			return ThreeState.False;
		}
		return ThreeState.True;
	}

	public static bool HasValue(this ThreeState value)
	{
		return value != ThreeState.Unknown;
	}

	public static bool Value(this ThreeState value)
	{
		return value == ThreeState.True;
	}
}
