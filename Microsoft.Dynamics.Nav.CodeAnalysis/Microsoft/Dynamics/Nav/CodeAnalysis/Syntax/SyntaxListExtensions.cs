using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

internal static class SyntaxListExtensions
{
	internal static ImmutableArray<PropertySymbol> CreatePropertySymbols(this SyntaxList<PropertySyntaxOrEmpty> properties, Symbol owner)
	{
		return properties.CreatePropertySymbols<PropertySyntaxOrEmpty>(owner);
	}

	internal static ImmutableArray<PropertySymbol> CreatePropertySymbols(this SyntaxList<PropertySyntax> properties, Symbol owner)
	{
		return properties.CreatePropertySymbols<PropertySyntax>(owner);
	}

	private static ImmutableArray<PropertySymbol> CreatePropertySymbols<T>(this SyntaxList<T> properties, Symbol owner) where T : PropertySyntaxOrEmpty
	{
		_ = ImmutableArray<PropertySymbol>.Empty;
		ArrayBuilder<PropertySymbol> instance = ArrayBuilder<PropertySymbol>.GetInstance(properties.Count);
		ImmutableArray<PropertySymbol> result;
		try
		{
			foreach (T item in properties)
			{
				if (item.Kind != SyntaxKind.EmptyProperty)
				{
					PropertySyntax syntax = item as PropertySyntax;
					instance.Add(SourcePropertySymbol.Create(owner, syntax));
				}
			}
		}
		finally
		{
			result = instance.ToImmutableAndFree();
		}
		return result;
	}
}
