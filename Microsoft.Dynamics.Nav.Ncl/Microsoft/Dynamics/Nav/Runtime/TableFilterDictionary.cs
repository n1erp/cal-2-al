using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TableFilterDictionary : KeyValueSortedDictionary<int, FilterFieldDictionary>
{
	public static readonly TableFilterDictionary Empty = new TableFilterDictionary();

	public TableFilterDictionary(Dictionary<int, FilterFieldDictionary> lookup)
		: base(lookup, false)
	{
	}

	public TableFilterDictionary(params Tuple<int, FilterFieldDictionary>[] items)
		: base(items)
	{
	}

	public override int Compare(Tuple<int, FilterFieldDictionary> x, Tuple<int, FilterFieldDictionary> y)
	{
		return x.Item1 - y.Item1;
	}
}
