using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class NavFilter : ICloneable, IEqualityComparer<NavFilter>
{
	public const string EmptyFilterValue = "''";

	public const string ValueOrEmptyFilterValue = "%1|''";

	public const string AnyFilterValue = "*";

	public const char BlankSpace = ' ';

	private int filterField;

	private string filterValue;

	private string userTypedFilterValue;

	private RuntimeFilterType filterType;

	private bool isExactValue;

	private string optionsAsCaptionsFilterValue;

	public int FilterField
	{
		[DebuggerStepThrough]
		get
		{
			return filterField;
		}
		[DebuggerStepThrough]
		set
		{
			filterField = value;
		}
	}

	public string FilterValue
	{
		[DebuggerStepThrough]
		get
		{
			return filterValue;
		}
		[DebuggerStepThrough]
		set
		{
			filterValue = value;
		}
	}

	public string UserTypedFilterValue
	{
		[DebuggerStepThrough]
		get
		{
			return userTypedFilterValue;
		}
		[DebuggerStepThrough]
		set
		{
			userTypedFilterValue = value;
		}
	}

	public string OptionsAsCaptionsFilterValue
	{
		[DebuggerStepThrough]
		get
		{
			return optionsAsCaptionsFilterValue;
		}
		[DebuggerStepThrough]
		set
		{
			optionsAsCaptionsFilterValue = value;
		}
	}

	public RuntimeFilterType RuntimeFilter
	{
		[DebuggerStepThrough]
		get
		{
			return filterType;
		}
		[DebuggerStepThrough]
		set
		{
			filterType = value;
		}
	}

	public bool IsExactValue
	{
		[DebuggerStepThrough]
		get
		{
			return isExactValue;
		}
		[DebuggerStepThrough]
		set
		{
			isExactValue = value;
		}
	}

	public NavFilter()
	{
	}

	public NavFilter(int filterField, string filterValue)
	{
		FilterField = filterField;
		FilterValue = filterValue;
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is NavFilter navFilter))
		{
			return false;
		}
		if (FilterField == navFilter.FilterField && FilterValue == navFilter.FilterValue)
		{
			return UserTypedFilterValue == navFilter.UserTypedFilterValue;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return FilterField.GetHashCode() ^ FilterValue.GetHashCode();
	}

	public static bool operator ==(NavFilter filter1, NavFilter filter2)
	{
		if ((object)filter1 == null || (object)filter2 == null)
		{
			return (object)filter1 == filter2;
		}
		return filter1.Equals(filter2);
	}

	public static bool operator !=(NavFilter filter1, NavFilter filter2)
	{
		if ((object)filter1 == null || (object)filter2 == null)
		{
			return (object)filter1 != filter2;
		}
		return !filter1.Equals(filter2);
	}

	public override string ToString()
	{
		return filterField.ToString(CultureInfo.InvariantCulture) + "=" + filterValue;
	}

	public bool Equals(NavFilter x, NavFilter y)
	{
		return x == y;
	}

	public int GetHashCode(NavFilter obj)
	{
		if (!(obj == null))
		{
			return obj.GetHashCode();
		}
		return 0;
	}
}
