using System.Collections.Generic;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

public class SortingFieldEqualityComparer : IEqualityComparer<INavSortingField>
{
	public int GetHashCode(INavSortingField obj)
	{
		return HashCodeHelper.CombineHashCodes(obj.Field.GetHashCode(), (int)obj.Order);
	}

	public bool Equals(INavSortingField x, INavSortingField y)
	{
		if (x == y)
		{
			return true;
		}
		if (x.Field == y.Field)
		{
			return x.Order == y.Order;
		}
		return false;
	}
}
