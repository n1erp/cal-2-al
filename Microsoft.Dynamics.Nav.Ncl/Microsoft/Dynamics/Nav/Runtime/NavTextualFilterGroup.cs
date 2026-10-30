using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavTextualFilterGroup
{
	private List<NavFilterData> filterDatas = new List<NavFilterData>();

	private NavTextualFilterGroup(IList<string> textualFilters)
	{
		foreach (string textualFilter in textualFilters)
		{
			filterDatas.Add(NavFilterData.GetFilterData(textualFilter));
		}
	}

	public static NavTextualFilterGroup GetTextualFilterGroup(IList<string> textualFilters, DataItemIterator dataItemIterator)
	{
		try
		{
			NavTextualFilterGroup navTextualFilterGroup = new NavTextualFilterGroup(textualFilters);
			navTextualFilterGroup.ValidateForDataItemIterator(dataItemIterator);
			return navTextualFilterGroup;
		}
		catch (NavNCLInvalidFilterDataException innerException)
		{
			throw new NavNCLInvalidTextualFilterGroupException(Lang.InvalidFilterString, innerException);
		}
	}

	public Dictionary<DataItem, NavTableView> GetDataItemsTableViews()
	{
		Dictionary<DataItem, NavTableView> dictionary = new Dictionary<DataItem, NavTableView>();
		foreach (NavFilterData filterData in filterDatas)
		{
			NavFilter navFilter = new NavFilter();
			navFilter.FilterField = filterData.FieldId;
			navFilter.FilterValue = filterData.FilterValue;
			if (!dictionary.TryGetValue(filterData.DataItem, out var value))
			{
				NavTableView value2 = CreateTableView(filterData.DataItem, navFilter);
				dictionary.Add(filterData.DataItem, value2);
			}
			else
			{
				UpdateTableView(navFilter, value);
			}
		}
		return dictionary;
	}

	private void ValidateForDataItemIterator(DataItemIterator dataItemIterator)
	{
		try
		{
			foreach (NavFilterData filterData in filterDatas)
			{
				filterData.ValidateForDataItemIterator(dataItemIterator);
			}
		}
		catch (NavNCLInvalidFilterDataException innerException)
		{
			throw new NavNCLInvalidTextualFilterGroupException(Lang.InvalidFilterString, innerException);
		}
	}

	private static void UpdateTableView(NavFilter filter, NavTableView tableView)
	{
		NavFilter[] filters = tableView.CurrentFilters[0].Filters;
		NavFilter[] array = new NavFilter[filters.Length + 1];
		filters.CopyTo(array, 0);
		array[filters.Length] = filter;
		tableView.CurrentFilters[0].Filters = array;
	}

	private static NavTableView CreateTableView(DataItem dataItem, NavFilter filter)
	{
		NavFilterGroup navFilterGroup = new NavFilterGroup();
		navFilterGroup.FilterGroupNo = 0;
		navFilterGroup.Filters = new NavFilter[1];
		navFilterGroup.Filters[0] = filter;
		NavTableView navTableView = new NavTableView();
		navTableView.CurrentFilters = new NavFilterGroup[1];
		navTableView.CurrentFilters[0] = navFilterGroup;
		navTableView.CurrentSortingFieldIds = dataItem.Record.CurrentSortingFieldIds;
		navTableView.Ascending = dataItem.Record.ALAscending;
		return navTableView;
	}
}
