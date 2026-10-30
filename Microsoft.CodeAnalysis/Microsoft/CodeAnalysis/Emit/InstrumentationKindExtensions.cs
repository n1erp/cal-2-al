namespace Microsoft.CodeAnalysis.Emit;

internal static class InstrumentationKindExtensions
{
	internal static bool IsValid(this InstrumentationKind value)
	{
		if (value >= InstrumentationKind.None)
		{
			return value <= InstrumentationKind.TestCoverage;
		}
		return false;
	}
}
