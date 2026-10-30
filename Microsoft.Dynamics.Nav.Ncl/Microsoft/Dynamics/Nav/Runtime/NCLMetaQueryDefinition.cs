using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetaQueryDefinition
{
	public const Func<NCLMetaQueryDefinition, ReadOnlyCollection<NCLMetaQueryOrderBy>> DefaultOrderByCreator = null;

	private NCLMetaQueryDataItem parentDataItem;

	private readonly int topRows;

	private readonly NCLMetaQueryDataItemCollection dataItems;

	private readonly ReadOnlyCollection<NCLMetaQueryOrderBy> definedOrderBy;

	private ReadOnlyCollection<NCLMetaQueryOrderBy> uniqueOrderBy;

	private SortingFieldList positioningFields;

	private ReadOnlyCollection<NCLMetaTable> includedTables;

	private ReadOnlyCollection<NCLMetaQueryColumn> definedColumns;

	private ReadOnlyCollection<NCLMetaQueryColumn> actualColumnsIncludingColumnsRequiredForUniqueness;

	private int nextColumnIndex;

	private bool anyAggregates;

	private bool anyAggregatesCalculated;

	private bool anyOuterJoinedDataItems;

	private bool anyOuterJoinedDataItemsCalculated;

	private int valueAgnosticHashCode;

	private bool valueAgnosticHashCodeCalculated;

	private int hashCode;

	private bool hashCodeCalculated;

	private int valueInsensitiveHashCode;

	private bool valueInsensitiveHashCodeCalculated;

	public int NextColumnIndex
	{
		get
		{
			return nextColumnIndex;
		}
		set
		{
			nextColumnIndex = value;
		}
	}

	internal int TopRows => topRows;

	internal bool AnyAggregates
	{
		get
		{
			if (!anyAggregatesCalculated)
			{
				anyAggregates = DataItems.Any((NCLMetaQueryDataItem dataItem) => dataItem.AnyAggregates);
				anyAggregatesCalculated = true;
			}
			return anyAggregates;
		}
	}

	internal bool AnyOuterJoinedDataItems
	{
		get
		{
			if (!anyOuterJoinedDataItemsCalculated)
			{
				anyOuterJoinedDataItems = dataItems.Count > 1 && dataItems.Any((NCLMetaQueryDataItem dataItem) => dataItem.DataItemLinkType == DataItemLinkType.FullOuterJoin || dataItem.DataItemLinkType == DataItemLinkType.LeftOuterJoin || dataItem.DataItemLinkType == DataItemLinkType.OuterApply || dataItem.DataItemLinkType == DataItemLinkType.RightOuterJoin);
				anyOuterJoinedDataItemsCalculated = true;
			}
			return anyOuterJoinedDataItems;
		}
	}

	internal ReadOnlyCollection<NCLMetaQueryDataItem> DataItems => dataItems;

	internal bool IsSubquery => parentDataItem != null;

	internal NCLMetaQueryDefinition RootQueryDefinition
	{
		get
		{
			if (!IsSubquery)
			{
				return this;
			}
			return parentDataItem.ParentQueryDefinition.RootQueryDefinition;
		}
	}

	internal ReadOnlyCollection<NCLMetaQueryOrderBy> DefinedOrderBy => definedOrderBy;

	public ReadOnlyCollection<NCLMetaQueryOrderBy> OrderBy
	{
		get
		{
			if (uniqueOrderBy == null)
			{
				List<NCLMetaQueryOrderBy> list = new List<NCLMetaQueryOrderBy>();
				if (definedOrderBy != null)
				{
					list.AddRange(definedOrderBy);
				}
				for (int i = 0; i < DataItems.Count; i++)
				{
					NCLMetaQueryDataItem nCLMetaQueryDataItem = DataItems[i];
					list.AddRange(nCLMetaQueryDataItem.MissingOrderByColumns);
				}
				uniqueOrderBy = new ReadOnlyCollection<NCLMetaQueryOrderBy>(list);
			}
			return uniqueOrderBy;
		}
	}

	public SortingFieldList PositioningFields
	{
		get
		{
			if (positioningFields == null)
			{
				positioningFields = SortingFieldList.Create(OrderBy.Where((NCLMetaQueryOrderBy orderByColumn) => orderByColumn.Column.ColumnType == QueryColumnType.Normal));
			}
			return positioningFields;
		}
	}

	public ReadOnlyCollection<NCLMetaQueryColumn> DefinedColumns
	{
		get
		{
			if (definedColumns == null)
			{
				List<NCLMetaQueryColumn> list = new List<NCLMetaQueryColumn>();
				foreach (NCLMetaQueryDataItem item in DataItems.Where((NCLMetaQueryDataItem dataItem) => dataItem.DefinedColumns != null))
				{
					list.AddRange(item.DefinedColumns);
				}
				definedColumns = new ReadOnlyCollection<NCLMetaQueryColumn>(list);
			}
			return definedColumns;
		}
	}

	public ReadOnlyCollection<NCLMetaQueryColumn> Columns
	{
		get
		{
			if (actualColumnsIncludingColumnsRequiredForUniqueness == null)
			{
				List<NCLMetaQueryColumn> list = new List<NCLMetaQueryColumn>();
				foreach (NCLMetaQueryDataItem dataItem in DataItems)
				{
					list.AddRange(dataItem.QueryColumns.Where((NCLMetaQueryColumn column) => column.ColumnType != QueryColumnType.FilterOnly));
				}
				actualColumnsIncludingColumnsRequiredForUniqueness = new ReadOnlyCollection<NCLMetaQueryColumn>(list);
			}
			return actualColumnsIncludingColumnsRequiredForUniqueness;
		}
	}

	internal ReadOnlyCollection<NCLMetaTable> IncludedTables
	{
		get
		{
			if (includedTables == null)
			{
				HashSet<NCLMetaTable> hashSet = new HashSet<NCLMetaTable>();
				foreach (NCLMetaQueryDataItem allDataItem in GetAllDataItems(RootQueryDefinition))
				{
					hashSet.Add(allDataItem.MetaTable);
				}
				includedTables = new ReadOnlyCollection<NCLMetaTable>(hashSet.ToArray());
			}
			return includedTables;
		}
	}

	private NCLMetaQueryDefinition(NCLMetaQueryDataItem parentDataItem, int topRows, Func<NCLMetaQueryDefinition, NCLMetaQueryDataItemCollection> dataItemsCreator, Func<NCLMetaQueryDefinition, ReadOnlyCollection<NCLMetaQueryOrderBy>> orderBysCreator)
	{
		this.parentDataItem = parentDataItem;
		this.topRows = topRows;
		if (dataItemsCreator != null)
		{
			dataItems = dataItemsCreator(this);
		}
		if (orderBysCreator != null)
		{
			definedOrderBy = orderBysCreator(this);
		}
	}

	public static NCLMetaQueryDefinition Create(int topRows, Func<NCLMetaQueryDefinition, NCLMetaQueryDataItemCollection> dataItemsCreator, Func<NCLMetaQueryDefinition, ReadOnlyCollection<NCLMetaQueryOrderBy>> orderBysCreator)
	{
		return new NCLMetaQueryDefinition(null, topRows, dataItemsCreator, orderBysCreator);
	}

	public static NCLMetaQueryDefinition Create(int topRows, NCLMetaQueryDataItem parentDataItem, Func<NCLMetaQueryDefinition, NCLMetaQueryDataItemCollection> dataItemsCreator, Func<NCLMetaQueryDefinition, ReadOnlyCollection<NCLMetaQueryOrderBy>> orderBysCreator)
	{
		return new NCLMetaQueryDefinition(parentDataItem, topRows, dataItemsCreator, orderBysCreator);
	}

	public bool CheckIsStale()
	{
		if (dataItems != null)
		{
			return dataItems.Any((NCLMetaQueryDataItem dataItem) => dataItem.CheckIsStale());
		}
		return false;
	}

	public NCLMetaQueryColumn GetColumnByIndex(int columnIndex)
	{
		NCLMetaQueryColumn nCLMetaQueryColumn = DefinedColumns.ElementAtOrDefault(columnIndex);
		if (nCLMetaQueryColumn == null)
		{
			throw new NavNCLColumnNotFoundException(string.Format(CultureInfo.CurrentCulture, Lang.QueryColumnCouldNotBeFoundIndex, columnIndex));
		}
		return nCLMetaQueryColumn;
	}

	public NCLMetaQueryColumn GetColumnByNo(int columnNumber)
	{
		if (DefinedColumns.Count == 0)
		{
			throw NavNCLColumnNotFoundException.Create(CultureInfo.CurrentCulture, columnNumber);
		}
		NCLMetaQueryColumn nCLMetaQueryColumn = null;
		try
		{
			nCLMetaQueryColumn = DefinedColumns.SingleOrDefault((NCLMetaQueryColumn t) => t.Id == columnNumber);
		}
		catch (InvalidOperationException)
		{
		}
		if (nCLMetaQueryColumn == null)
		{
			throw NavNCLColumnNotFoundException.Create(CultureInfo.CurrentCulture, columnNumber);
		}
		return nCLMetaQueryColumn;
	}

	public NCLMetaQueryColumn GetColumnByName(string columnName)
	{
		if (Columns.Count == 0)
		{
			throw NavNCLColumnNotFoundException.Create(CultureInfo.CurrentCulture, columnName);
		}
		NCLMetaQueryColumn nCLMetaQueryColumn = null;
		try
		{
			nCLMetaQueryColumn = Columns.SingleOrDefault((NCLMetaQueryColumn t) => t.Name == columnName);
		}
		catch (InvalidOperationException)
		{
		}
		if (nCLMetaQueryColumn == null)
		{
			throw NavNCLColumnNotFoundException.Create(CultureInfo.CurrentCulture, columnName);
		}
		return nCLMetaQueryColumn;
	}

	public static IEnumerable<NCLMetaQueryDataItem> GetAllDataItems(NCLMetaQueryDefinition queryDefinition)
	{
		foreach (NCLMetaQueryDataItem dataItem in queryDefinition.DataItems)
		{
			if (dataItem.SubQueryDefinition != null)
			{
				foreach (NCLMetaQueryDataItem dataItem2 in dataItem.SubQueryDefinition.DataItems)
				{
					yield return dataItem2;
				}
			}
			else
			{
				yield return dataItem;
			}
		}
	}

	private int GetValueAgnosticHashCode()
	{
		if (!valueAgnosticHashCodeCalculated)
		{
			valueAgnosticHashCode = topRows.GetHashCode();
			for (int i = 0; i < OrderBy.Count; i++)
			{
				NCLMetaQueryOrderBy nCLMetaQueryOrderBy = OrderBy[i];
				valueAgnosticHashCode = HashCodeHelper.CombineHashCodes(valueAgnosticHashCode, nCLMetaQueryOrderBy.GetHashCode());
			}
			valueAgnosticHashCodeCalculated = true;
		}
		return valueAgnosticHashCode;
	}

	public override int GetHashCode()
	{
		if (!hashCodeCalculated)
		{
			hashCode = HashCodeHelper.CombineHashCodes(GetValueAgnosticHashCode(), dataItems.GetHashCode());
			hashCodeCalculated = true;
		}
		return hashCode;
	}

	public int GetValueInsensitiveHashCode()
	{
		if (!valueInsensitiveHashCodeCalculated)
		{
			valueInsensitiveHashCode = HashCodeHelper.CombineHashCodes(GetValueAgnosticHashCode(), dataItems.GetValueInsensitiveHashCode());
			valueInsensitiveHashCodeCalculated = true;
		}
		return valueInsensitiveHashCode;
	}

	private bool ValueAgnosticEquals(NCLMetaQueryDefinition otherQueryDefinition)
	{
		if (dataItems.Count != otherQueryDefinition.dataItems.Count || definedOrderBy == null != (otherQueryDefinition.definedOrderBy == null) || topRows != otherQueryDefinition.topRows)
		{
			return false;
		}
		if (definedOrderBy != null)
		{
			if (definedOrderBy.Count != otherQueryDefinition.definedOrderBy.Count)
			{
				return false;
			}
			IEnumerator<NCLMetaQueryOrderBy> enumerator = otherQueryDefinition.definedOrderBy.GetEnumerator();
			foreach (NCLMetaQueryOrderBy item in definedOrderBy)
			{
				enumerator.MoveNext();
				if (!item.Equals(enumerator.Current))
				{
					return false;
				}
			}
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is NCLMetaQueryDefinition nCLMetaQueryDefinition))
		{
			return false;
		}
		if (GetHashCode() == nCLMetaQueryDefinition.GetHashCode() && ValueAgnosticEquals(nCLMetaQueryDefinition))
		{
			return dataItems.Equals(nCLMetaQueryDefinition.dataItems);
		}
		return false;
	}

	public bool ValueInsensitiveEquals(object other)
	{
		if (this == other)
		{
			return true;
		}
		if (!(other is NCLMetaQueryDefinition nCLMetaQueryDefinition))
		{
			return false;
		}
		if (GetValueInsensitiveHashCode() == nCLMetaQueryDefinition.GetValueInsensitiveHashCode() && ValueAgnosticEquals(nCLMetaQueryDefinition))
		{
			return dataItems.ValueInsensitiveEquals(nCLMetaQueryDefinition.dataItems);
		}
		return false;
	}
}
