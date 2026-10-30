namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class CompilationOptionsExtensions
{
	internal static bool IsValidationRelaxedForBackwardsCompatibility(this CompilationOptions options)
	{
		return options.Target == CompilationTarget.Internal;
	}
}
