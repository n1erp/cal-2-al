using System.Collections.Generic;

namespace Roslyn.Utilities;

internal sealed class EmptyComparer : IEqualityComparer<object>
{
	public static readonly EmptyComparer Instance = new EmptyComparer();

	private EmptyComparer()
	{
	}

	bool IEqualityComparer<object>.Equals(object a, object b)
	{
		return false;
	}

	int IEqualityComparer<object>.GetHashCode(object s)
	{
		return 0;
	}
}
