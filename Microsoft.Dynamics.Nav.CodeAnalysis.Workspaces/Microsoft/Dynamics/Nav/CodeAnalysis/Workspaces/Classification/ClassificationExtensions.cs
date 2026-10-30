using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Classification;

internal static class ClassificationExtensions
{
	public static string GetClassification(this ITypeSymbol type)
	{
		if (type.IsUserType())
		{
			return "application object name";
		}
		if (type.NavTypeKind == NavTypeKind.Option)
		{
			return "option name";
		}
		if (type.Kind == SymbolKind.Parameter)
		{
			return "parameter name";
		}
		if (type.Kind == SymbolKind.Method)
		{
			return "function name";
		}
		if (type.Kind == SymbolKind.Property)
		{
			return "property name";
		}
		return null;
	}
}
