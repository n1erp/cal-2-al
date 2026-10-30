using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SymbolNameEqualityComparer : IEqualityComparer<ISymbol>
{
	private static readonly Lazy<SymbolNameEqualityComparer> instance = new Lazy<SymbolNameEqualityComparer>();

	internal static SymbolNameEqualityComparer Instance => instance.Value;

	public bool Equals(ISymbol x, ISymbol y)
	{
		if (x == y)
		{
			return true;
		}
		if (x == null)
		{
			return y == null;
		}
		if (y == null)
		{
			return false;
		}
		return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
	}

	public int GetHashCode(ISymbol obj)
	{
		if (obj == null)
		{
			return 0;
		}
		return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name);
	}
}
