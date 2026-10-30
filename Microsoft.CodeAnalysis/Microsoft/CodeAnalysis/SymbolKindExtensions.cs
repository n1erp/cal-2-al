using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal static class SymbolKindExtensions
{
	public static int ToSortOrder(this SymbolKind kind)
	{
		return kind switch
		{
			SymbolKind.Field => 0, 
			SymbolKind.Method => 1, 
			SymbolKind.Property => 2, 
			SymbolKind.Event => 3, 
			SymbolKind.NamedType => 4, 
			SymbolKind.Namespace => 5, 
			SymbolKind.Alias => 6, 
			SymbolKind.ArrayType => 7, 
			SymbolKind.Assembly => 8, 
			SymbolKind.Label => 10, 
			SymbolKind.Local => 11, 
			SymbolKind.NetModule => 12, 
			SymbolKind.Parameter => 13, 
			SymbolKind.RangeVariable => 14, 
			SymbolKind.TypeParameter => 15, 
			SymbolKind.PointerType => 16, 
			SymbolKind.DynamicType => 17, 
			SymbolKind.Preprocessing => 18, 
			_ => throw ExceptionUtilities.UnexpectedValue(kind), 
		};
	}
}
