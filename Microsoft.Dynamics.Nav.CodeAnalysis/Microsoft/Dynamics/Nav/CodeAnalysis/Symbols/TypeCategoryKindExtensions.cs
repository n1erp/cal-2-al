namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal static class TypeCategoryKindExtensions
{
	public static bool IsTypeWithTriggers(this TypeCategoryKind kind)
	{
		if (kind == TypeCategoryKind.ApplicationObject || kind == TypeCategoryKind.RequestPage)
		{
			return true;
		}
		return false;
	}
}
