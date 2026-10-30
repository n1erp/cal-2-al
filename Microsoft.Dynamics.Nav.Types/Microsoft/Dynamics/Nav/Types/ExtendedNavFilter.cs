using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types;

public class ExtendedNavFilter
{
	public int FilterGroupNo { get; private set; }

	public NavFilter NavFilter { get; private set; }

	public ExtendedNavFilter(int filterGroupNo, NavFilter navFilter)
	{
		if (navFilter == null)
		{
			throw new ArgumentNullException("navFilter");
		}
		FilterGroupNo = filterGroupNo;
		NavFilter = navFilter;
	}

	public static NavFilterGroup[] ExtendedNavFilters2ArrayNavFilterGroup(IEnumerable<ExtendedNavFilter> extendedNavFilters)
	{
		Dictionary<int, List<NavFilter>> dictionary = new Dictionary<int, List<NavFilter>>();
		foreach (ExtendedNavFilter extendedNavFilter in extendedNavFilters)
		{
			List<NavFilter> list;
			if (dictionary.ContainsKey(extendedNavFilter.FilterGroupNo))
			{
				list = dictionary[extendedNavFilter.FilterGroupNo];
			}
			else
			{
				list = new List<NavFilter>();
				dictionary.Add(extendedNavFilter.FilterGroupNo, list);
			}
			list.Add(extendedNavFilter.NavFilter);
		}
		List<NavFilterGroup> list2 = new List<NavFilterGroup>();
		foreach (int key in dictionary.Keys)
		{
			NavFilterGroup item = new NavFilterGroup(key, dictionary[key].ToArray());
			list2.Add(item);
		}
		return list2.ToArray();
	}

	public static IEnumerable<ExtendedNavFilter> ArrayNavFilterGroup2ExtendedNavFilters(NavFilterGroup[] navFilterGroups)
	{
		foreach (NavFilterGroup group in navFilterGroups)
		{
			NavFilter[] filters = group.Filters;
			foreach (NavFilter navFilter in filters)
			{
				yield return new ExtendedNavFilter(group.FilterGroupNo, navFilter);
			}
		}
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} - {1}", FilterGroupNo, NavFilter);
	}
}
