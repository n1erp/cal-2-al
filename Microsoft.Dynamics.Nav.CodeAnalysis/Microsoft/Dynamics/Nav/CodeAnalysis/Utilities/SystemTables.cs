using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class SystemTables
{
	internal const int Fields = 2000000041;

	internal const int AllControlFields = 2000000202;

	internal const int VirtualTableIds = 2000000000;

	internal const int SystemTableIds = 2000000100;

	public static bool IsOptionalOrdinalValuesAllowed(int tableId)
	{
		if (tableId != 2000000041)
		{
			return tableId == 2000000202;
		}
		return true;
	}

	public static bool IsSystemOrVirtualTable(ISymbol symbol)
	{
		if (symbol.Kind == SymbolKind.Table)
		{
			return ((TableTypeSymbol)symbol).Id >= 2000000000;
		}
		return false;
	}
}
