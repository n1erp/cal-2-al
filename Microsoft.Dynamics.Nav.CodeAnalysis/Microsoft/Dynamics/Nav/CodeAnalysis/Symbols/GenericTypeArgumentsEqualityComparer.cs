using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class GenericTypeArgumentsEqualityComparer : IEqualityComparer<TypeSymbol[]>
{
	public static GenericTypeArgumentsEqualityComparer Instance { get; } = new GenericTypeArgumentsEqualityComparer();

	private GenericTypeArgumentsEqualityComparer()
	{
	}

	public bool Equals(TypeSymbol[] x, TypeSymbol[] y)
	{
		if (x == null && y == null)
		{
			return true;
		}
		if (x == null || y == null)
		{
			return false;
		}
		if (x.Length != y.Length)
		{
			return false;
		}
		for (int i = 0; i < x.Length; i++)
		{
			if (!x[i].Equals(y[i]))
			{
				return false;
			}
		}
		return true;
	}

	public int GetHashCode(TypeSymbol[] obj)
	{
		return Hash.CombineValues(obj);
	}
}
