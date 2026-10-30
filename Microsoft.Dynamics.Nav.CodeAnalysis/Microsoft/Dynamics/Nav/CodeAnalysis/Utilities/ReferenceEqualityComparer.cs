using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal class ReferenceEqualityComparer : IEqualityComparer<object>
{
	public static readonly IEqualityComparer<object> Instance = new ReferenceEqualityComparer();

	private ReferenceEqualityComparer()
	{
	}

	bool IEqualityComparer<object>.Equals(object a, object b)
	{
		return a == b;
	}

	int IEqualityComparer<object>.GetHashCode(object a)
	{
		return RuntimeHelpers.GetHashCode(a);
	}
}
