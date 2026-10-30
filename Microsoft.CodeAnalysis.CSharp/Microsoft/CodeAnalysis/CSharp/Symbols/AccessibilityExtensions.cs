namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class AccessibilityExtensions
{
	public static bool HasProtected(this Accessibility accessibility)
	{
		if ((uint)(accessibility - 2) <= 1u || accessibility == Accessibility.ProtectedOrInternal)
		{
			return true;
		}
		return false;
	}
}
