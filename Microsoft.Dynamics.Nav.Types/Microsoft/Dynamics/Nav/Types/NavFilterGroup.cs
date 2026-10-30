using System;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class NavFilterGroup : ICloneable
{
	private int filterGroupNo;

	private NavFilter[] filters;

	public int FilterGroupNo
	{
		[DebuggerStepThrough]
		get
		{
			return filterGroupNo;
		}
		[DebuggerStepThrough]
		set
		{
			filterGroupNo = value;
		}
	}

	public NavFilter[] Filters
	{
		[DebuggerStepThrough]
		get
		{
			return filters;
		}
		[DebuggerStepThrough]
		set
		{
			filters = value;
		}
	}

	public NavFilterGroup()
	{
	}

	public NavFilterGroup(int filterGroupNo, NavFilter[] filters)
	{
		if (filters == null)
		{
			throw new ArgumentNullException("filters");
		}
		FilterGroupNo = filterGroupNo;
		Filters = (NavFilter[])filters.Clone();
	}

	public object Clone()
	{
		NavFilterGroup navFilterGroup = (NavFilterGroup)MemberwiseClone();
		NavFilter[] array = (NavFilter[])Filters.Clone();
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (NavFilter)array[i].Clone();
		}
		navFilterGroup.Filters = array;
		return navFilterGroup;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is NavFilterGroup navFilterGroup))
		{
			return false;
		}
		if (FilterGroupNo != navFilterGroup.FilterGroupNo)
		{
			return false;
		}
		if (Filters.Length != navFilterGroup.Filters.Length)
		{
			return false;
		}
		for (int i = 0; i < Filters.Length; i++)
		{
			if (!Filters[i].Equals(navFilterGroup.Filters[i]))
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		return FilterGroupNo.GetHashCode() ^ Filters.GetHashCode();
	}

	public static bool operator ==(NavFilterGroup group1, NavFilterGroup group2)
	{
		if ((object)group1 == null || (object)group2 == null)
		{
			return (object)group1 == group2;
		}
		return group1.Equals(group2);
	}

	public static bool operator !=(NavFilterGroup group1, NavFilterGroup group2)
	{
		if ((object)group1 == null || (object)group2 == null)
		{
			return (object)group1 != group2;
		}
		return !group1.Equals(group2);
	}
}
