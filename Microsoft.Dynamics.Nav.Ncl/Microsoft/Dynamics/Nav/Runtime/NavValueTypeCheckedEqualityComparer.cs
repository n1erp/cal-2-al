using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavValueTypeCheckedEqualityComparer : IEqualityComparer<Tuple<SqlDataType, NavValue>>
{
	public static readonly NavValueTypeCheckedEqualityComparer Instance = new NavValueTypeCheckedEqualityComparer();

	private NavValueTypeCheckedEqualityComparer()
	{
	}

	public int GetHashCode(Tuple<SqlDataType, NavValue> obj)
	{
		return HashCodeHelper.CombineHashCodes(MapDefaultSqlDataTypeForNavCode(obj).GetHashCode(), obj.Item2.GetHashCode());
	}

	public bool Equals(Tuple<SqlDataType, NavValue> x, Tuple<SqlDataType, NavValue> y)
	{
		SqlDataType item = x.Item1;
		SqlDataType item2 = y.Item1;
		NavValue item3 = x.Item2;
		NavValue item4 = y.Item2;
		if (item3 == item4 && item == item2)
		{
			return true;
		}
		if (item3 == null != (item4 == null))
		{
			return false;
		}
		if (item3.NclType != item4.NclType)
		{
			return false;
		}
		SqlDataType sqlDataType = MapDefaultSqlDataTypeForNavCode(x);
		SqlDataType sqlDataType2 = MapDefaultSqlDataTypeForNavCode(y);
		if (sqlDataType != sqlDataType2)
		{
			return false;
		}
		return item3.Equals(item4);
	}

	private static SqlDataType MapDefaultSqlDataTypeForNavCode(Tuple<SqlDataType, NavValue> value)
	{
		if (value.Item2.NclType == NavNclType.NavCode || value.Item2.NclType == NavNclType.NavOemCode)
		{
			if (value.Item1 != SqlDataType.Default)
			{
				return value.Item1;
			}
			return SqlDataType.VarChar;
		}
		return value.Item1;
	}
}
