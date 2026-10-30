using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetaQueryDataItem
{
	public static readonly FieldList TableBasedColumnLayout = new FieldList();

	private static readonly FiltersAndMarks filtersAndMarksNotEvaluated = new FiltersAndMarks(null, null);

	private readonly NCLMetaQueryDefinition parentQueryDefinition;

	private readonly AppGroupAwareLookup<NCLMetaTable> metaTableLookup;

	private readonly int tableNo;

	private readonly string name;

	private readonly FieldList definedQueryColumns;

	private readonly FiltersAndMarks tableFiltersAndMarks;

	private readonly ReadOnlyCollection<NCLMetaDataItemLink> dataItemLinks;

	private readonly DataItemLinkType dataItemLinkType;

	private readonly NCLMetaQueryDefinition subQueryDefinition;

	private readonly DataLockState lockState;

	private readonly bool columnLayoutIsTableBased;

	private string sqlDataItemName;

	private ReadOnlyCollection<NCLMetaQueryColumn> actualQueryColumns;

	private ReadOnlyCollection<NCLMetaQueryOrderBy> missingOrderByColumnsForUniqueness;

	private int hashCode;

	private bool hashCodeCalculated;

	private int valueInsensitiveHashCode;

	private bool valueInsensitiveHashCodeCalculated;

	private readonly List<MetaQueryFieldFilter> metaQueryFieldFilters;

	public ReadOnlyCollection<NCLMetaQueryOrderBy> MissingOrderByColumns
	{
		get
		{
			EnsureOrderByColumnsAndActualColumnListComputed();
			return missingOrderByColumnsForUniqueness;
		}
	}

	private bool IsFirstDataItemInQuery => this == parentQueryDefinition.RootQueryDefinition.DataItems[0];

	public NCLMetaQueryDefinition ParentQueryDefinition => parentQueryDefinition;

	public int TableNo => tableNo;

	public int AppGroup { get; private set; }

	public NCLMetaTable MetaTable
	{
		get
		{
			NavAppGroup appGroup = (NavRecord.IsSystemTable(tableNo) ? NavAppGroup.BaseGroup : NavCurrentThread.ResolveAppGroup());
			return metaTableLookup.GetReferenceUsingSpecificAppGroup(appGroup);
		}
	}

	public ReadOnlyCollection<NCLMetaDataItemLink> DataItemLinks => dataItemLinks;

	public ReadOnlyCollection<NCLMetaQueryColumn> DefinedColumns
	{
		get
		{
			if (definedQueryColumns != null)
			{
				return definedQueryColumns.AsNCLMetaQueryColumns();
			}
			return null;
		}
	}

	public ReadOnlyCollection<NCLMetaQueryColumn> QueryColumns
	{
		get
		{
			EnsureOrderByColumnsAndActualColumnListComputed();
			return actualQueryColumns;
		}
	}

	public bool AnyAggregates
	{
		get
		{
			if (columnLayoutIsTableBased || DefinedColumns == null)
			{
				return false;
			}
			return DefinedColumns.Any((NCLMetaQueryColumn column) => column.IsAggregated);
		}
	}

	internal FiltersAndMarks TableFiltersAndMarks
	{
		get
		{
			if (tableFiltersAndMarks != filtersAndMarksNotEvaluated)
			{
				return tableFiltersAndMarks;
			}
			bool tableFilterHasVariables;
			return NCLMetaQuery.CreateTableFiltersAndMarksFromDataItemFieldFilters(this, metaQueryFieldFilters, allowExpressionEvaluation: true, out tableFilterHasVariables);
		}
	}

	public DataItemLinkType DataItemLinkType => dataItemLinkType;

	public string Name => name;

	public NCLMetaQueryDefinition SubQueryDefinition => subQueryDefinition;

	internal DataLockState LockState => lockState;

	public string SqlDataItemName
	{
		get
		{
			if (sqlDataItemName == null)
			{
				sqlDataItemName = NavSqlStatementHelper.ConvertToSqlIdentifier(name);
			}
			return sqlDataItemName;
		}
	}

	internal static FiltersAndMarks FiltersAndMarksNotEvaluated => filtersAndMarksNotEvaluated;

	public bool CheckIsStale()
	{
		if (subQueryDefinition != null)
		{
			return subQueryDefinition.CheckIsStale();
		}
		IEnumerable<NCLMetaTable> source = from r in metaTableLookup.GetAllKnownReferences()
			select r.Item2;
		return source.Where((NCLMetaTable t) => t != null).Any((NCLMetaTable t) => t.Stale);
	}

	private ReadOnlyCollection<NCLMetaQueryOrderBy> GetOrderByColumnsForQueryWithAggregates()
	{
		if (DefinedColumns == null)
		{
			return new ReadOnlyCollection<NCLMetaQueryOrderBy>(new NCLMetaQueryOrderBy[0]);
		}
		IEnumerable<NCLMetaQueryColumn> source = ((parentQueryDefinition.DefinedOrderBy == null) ? DefinedColumns.Where((NCLMetaQueryColumn column) => !column.IsAggregated && column.ColumnType != QueryColumnType.ConstValue && column.ColumnType != QueryColumnType.FilterOnly) : DefinedColumns.Where((NCLMetaQueryColumn column) => !column.IsAggregated && column.ColumnType != QueryColumnType.ConstValue && column.ColumnType != QueryColumnType.FilterOnly && !parentQueryDefinition.DefinedOrderBy.Any((NCLMetaQueryOrderBy orderByColumn) => orderByColumn.Field == column)));
		return new ReadOnlyCollection<NCLMetaQueryOrderBy>(source.Select((NCLMetaQueryColumn column) => NCLMetaQueryOrderBy.Create(SortOrder.Ascending, column, extraColumnAutomaticallyAddedForUniqueness: false)).ToArray());
	}

	private ReadOnlyCollection<NCLMetaQueryOrderBy> GetOrderByColumnsForNonAggregatedQuery()
	{
		List<NCLMetaQueryOrderBy> list = new List<NCLMetaQueryOrderBy>();
		IEnumerable<NCLMetaField> enumerable = MetaTable.PrimaryKey.KeyFieldsList;
		if (ParentQueryDefinition.DataItems.Count == 1 && ParentQueryDefinition.DefinedOrderBy != null)
		{
			NCLMetaTable metaTable = MetaTable;
			NCLMetaField[] array = (from nCLMetaQueryOrderBy in ParentQueryDefinition.DefinedOrderBy
				where ((NCLMetaQueryColumn)nCLMetaQueryOrderBy.Field).SourceTableField.Parent.TableId == TableNo
				select ((NCLMetaQueryColumn)nCLMetaQueryOrderBy.Field).SourceTableField).ToArray();
			array.ForEach((NCLMetaField metaField) =>
			{
				ObsolescenceGuard.ThrowIfObsoleted(metaField);
			});
			NCLMetaKey bestKey = null;
			if (DataProviderHelper.TryFindBestKey(metaTable, array, out bestKey))
			{
				enumerable = bestKey.KeyFieldsWithPrimaryKeyFields.AsNclMetaFields();
			}
		}
		foreach (NCLMetaField field in enumerable)
		{
			if (ParentQueryDefinition.DefinedOrderBy == null || !ParentQueryDefinition.DefinedOrderBy.Any((NCLMetaQueryOrderBy orderByColumn) =>
			{
				NCLMetaQueryColumn nCLMetaQueryColumn2 = (NCLMetaQueryColumn)orderByColumn.Field;
				return nCLMetaQueryColumn2.ParentDataItem == this && nCLMetaQueryColumn2.SourceTableField == field;
			}))
			{
				NCLMetaQueryColumn nCLMetaQueryColumn = ((DefinedColumns == null) ? null : DefinedColumns.FirstOrDefault((NCLMetaQueryColumn nCLMetaQueryColumn2) => nCLMetaQueryColumn2.ColumnType == QueryColumnType.Normal && nCLMetaQueryColumn2.SourceTableField == field));
				if (nCLMetaQueryColumn == null)
				{
					NCLMetaQueryColumn column = NCLMetaQueryColumn.Create(this, field.FieldNo, string.Format(CultureInfo.InvariantCulture, "{0}${1}", name, field.FieldName), 0, filterOnly: false);
					list.Add(NCLMetaQueryOrderBy.Create(SortOrder.Ascending, column, extraColumnAutomaticallyAddedForUniqueness: true));
				}
				else
				{
					list.Add(NCLMetaQueryOrderBy.Create(SortOrder.Ascending, nCLMetaQueryColumn, extraColumnAutomaticallyAddedForUniqueness: false));
				}
			}
		}
		return new ReadOnlyCollection<NCLMetaQueryOrderBy>(list);
	}

	private ReadOnlyCollection<NCLMetaQueryOrderBy> GetOrderByColumnsForSubQuery()
	{
		if (subQueryDefinition.TopRows == 1)
		{
			return new ReadOnlyCollection<NCLMetaQueryOrderBy>(new NCLMetaQueryOrderBy[0]);
		}
		List<NCLMetaQueryOrderBy> list = new List<NCLMetaQueryOrderBy>();
		foreach (NCLMetaQueryColumn subQuerySortingColumn in subQueryDefinition.OrderBy.Select((NCLMetaQueryOrderBy orderByColumn) => orderByColumn.Column))
		{
			NCLMetaQueryColumn nCLMetaQueryColumn = (NCLMetaQueryColumn)definedQueryColumns.FirstOrDefault((INavFieldMetadata existingColumn) => ((NCLMetaQueryColumn)existingColumn).SourceColumn == subQuerySortingColumn);
			if (nCLMetaQueryColumn == null)
			{
				nCLMetaQueryColumn = NCLMetaQueryColumn.Create(this, subQuerySortingColumn.Id, string.Format(CultureInfo.InvariantCulture, "{0}${1}", name, subQuerySortingColumn.Name), 0, filterOnly: false);
			}
			list.Add(NCLMetaQueryOrderBy.Create(SortOrder.Ascending, nCLMetaQueryColumn, extraColumnAutomaticallyAddedForUniqueness: true));
		}
		return new ReadOnlyCollection<NCLMetaQueryOrderBy>(list);
	}

	private void EnsureOrderByColumnsAndActualColumnListComputed()
	{
		if (actualQueryColumns != null)
		{
			return;
		}
		ReadOnlyCollection<NCLMetaQueryOrderBy> source;
		if (subQueryDefinition != null)
		{
			source = GetOrderByColumnsForSubQuery();
		}
		else
		{
			source = ((!parentQueryDefinition.AnyAggregates) ? GetOrderByColumnsForNonAggregatedQuery() : GetOrderByColumnsForQueryWithAggregates());
		}
		if (parentQueryDefinition.DefinedOrderBy == null)
		{
			missingOrderByColumnsForUniqueness = source;
		}
		else
		{
			IEnumerable<NCLMetaQueryOrderBy> source2 = source.Where((NCLMetaQueryOrderBy orderByColumn) => !parentQueryDefinition.DefinedOrderBy.Any((NCLMetaQueryOrderBy definedOrderByColumn) => definedOrderByColumn == orderByColumn.Field));
			missingOrderByColumnsForUniqueness = new ReadOnlyCollection<NCLMetaQueryOrderBy>(source2.ToArray());
		}
		if (DefinedColumns == null)
		{
			actualQueryColumns = new ReadOnlyCollection<NCLMetaQueryColumn>(missingOrderByColumnsForUniqueness.Select((NCLMetaQueryOrderBy orderByColumn) => (NCLMetaQueryColumn)orderByColumn.Field).ToArray());
			return;
		}
		List<NCLMetaQueryColumn> list = new List<NCLMetaQueryColumn>(DefinedColumns);
		IEnumerable<NCLMetaQueryOrderBy> source3 = missingOrderByColumnsForUniqueness.Where((NCLMetaQueryOrderBy orderByColumn) => !DefinedColumns.Any((NCLMetaQueryColumn definedColumn) => definedColumn == orderByColumn.Field && definedColumn.ColumnType != QueryColumnType.FilterOnly));
		list.AddRange(source3.Select((NCLMetaQueryOrderBy missingColumn) => (NCLMetaQueryColumn)missingColumn.Field));
		actualQueryColumns = new ReadOnlyCollection<NCLMetaQueryColumn>(list);
	}

	internal NCLMetaKey GetCoveringSIFTIndex(Dictionary<int, FilterFieldDictionary> globalAndSecurityFilters, FilterFieldDictionary queryFiltersOnFlowFilters)
	{
		if (!MetaTable.AnySiftIndexes)
		{
			return null;
		}
		if (QueryColumns.Any((NCLMetaQueryColumn column) => column.AggregationType != AggregationType.None && column.AggregationType != AggregationType.Average && column.AggregationType != AggregationType.Sum && column.AggregationType != AggregationType.Count))
		{
			return null;
		}
		if (ParentQueryDefinition.DataItems.Any((NCLMetaQueryDataItem dataItem) => this != dataItem && dataItem.AnyAggregates))
		{
			return null;
		}
		HashSet<INavFieldMetadata> hashSet = new HashSet<INavFieldMetadata>();
		if (TableFiltersAndMarks != null)
		{
			if (TableFiltersAndMarks.Filters != null)
			{
				foreach (Tuple<INavFieldMetadata, FilterExpression> item2 in TableFiltersAndMarks.Filters.Items.Where((Tuple<INavFieldMetadata, FilterExpression> tuple2) => tuple2.Item1.FieldClass == FieldClass.Normal))
				{
					hashSet.Add(item2.Item1);
				}
			}
			if (TableFiltersAndMarks.MarkedRecords != null)
			{
				foreach (NCLMetaField definedSortingField in MetaTable.PrimaryKey.DefinedSortingFields)
				{
					hashSet.Add(definedSortingField);
				}
			}
		}
		if (globalAndSecurityFilters != null)
		{
			FilterFieldDictionary value = null;
			if (globalAndSecurityFilters.TryGetValue(TableNo, out value) && value != null)
			{
				Tuple<INavFieldMetadata, FilterExpression>[] items = value.Items;
				foreach (Tuple<INavFieldMetadata, FilterExpression> tuple in items)
				{
					hashSet.Add(tuple.Item1);
				}
			}
		}
		if (DataItemLinks != null)
		{
			foreach (NCLMetaDataItemLink item3 in DataItemLinks.Where((NCLMetaDataItemLink link) => link.DestinationColumn.SourceTableField.FieldClass == FieldClass.Normal))
			{
				if (item3.LinkType != NCLMetaFilterType.Field || item3.SourceColumn.SourceTableField.FieldClass != FieldClass.FlowFilter || (!(queryFiltersOnFlowFilters == null) && queryFiltersOnFlowFilters.TryGetValue(item3.SourceColumn, out var _)))
				{
					hashSet.Add(item3.DestinationColumn.SourceTableField);
				}
			}
		}
		foreach (NCLMetaQueryDataItem item4 in from possibleLinkingDataItem in NCLMetaQueryDefinition.GetAllDataItems(ParentQueryDefinition.RootQueryDefinition)
			where possibleLinkingDataItem != this
			select possibleLinkingDataItem)
		{
			if (item4.DataItemLinks == null)
			{
				continue;
			}
			foreach (NCLMetaDataItemLink item5 in item4.DataItemLinks.Where((NCLMetaDataItemLink linkSource) => linkSource.LinkType == NCLMetaFilterType.Field && linkSource.SourceDataItem == this))
			{
				if (item5.SourceColumn.SourceTableField.FieldClass != FieldClass.FlowFilter)
				{
					hashSet.Add(item5.SourceColumn.SourceTableField);
				}
			}
		}
		FilterFieldDictionary filters = ((hashSet.Count == 0) ? null : new FilterFieldDictionary(hashSet.Select((INavFieldMetadata field) => Tuple.Create<INavFieldMetadata, FilterExpression>(field, null))));
		INavFieldMetadata[] array = (from queryColumn in QueryColumns
			where (queryColumn.ColumnType != QueryColumnType.ConstValue && queryColumn.AggregationType == AggregationType.Sum) || queryColumn.AggregationType == AggregationType.Average
			select queryColumn.SourceTableField).ToArray();
		INavFieldMetadata[] array2 = array;
		array = (from queryColumn in QueryColumns
			where queryColumn.ColumnType != QueryColumnType.ConstValue && queryColumn.AggregationType == AggregationType.None && queryColumn.SourceTableField.FieldClass == FieldClass.Normal
			select queryColumn.SourceTableField).ToArray();
		INavFieldMetadata[] array3 = array;
		return FlowFieldsHelper.CalculateBestSIFTIndex(MetaTable, (array2.Length == 0) ? null : new FieldList(array2), (array3.Length == 0) ? null : new FieldList(array3), filters);
	}

	private int GetValueAgnosticHashCode()
	{
		return HashCodeHelper.CombineHashCodes(TableNo, (int)((!IsFirstDataItemInQuery) ? dataItemLinkType : DataItemLinkType.LeftOuterJoin));
	}

	public override int GetHashCode()
	{
		if (hashCodeCalculated)
		{
			hashCode = GetValueAgnosticHashCode();
			if (definedQueryColumns != null)
			{
				if (columnLayoutIsTableBased)
				{
					hashCode = HashCodeHelper.CombineHashCodes(hashCode, int.MaxValue);
				}
				else
				{
					for (int i = 0; i < definedQueryColumns.Count; i++)
					{
						hashCode = HashCodeHelper.CombineHashCodes(hashCode, definedQueryColumns[i].GetHashCode());
					}
				}
			}
			if (tableFiltersAndMarks != null)
			{
				hashCode = HashCodeHelper.CombineHashCodes(hashCode, tableFiltersAndMarks.GetHashCode());
			}
			if (subQueryDefinition != null)
			{
				hashCode = HashCodeHelper.CombineHashCodes(hashCode, subQueryDefinition.GetHashCode());
			}
			if (!IsFirstDataItemInQuery && dataItemLinks != null)
			{
				foreach (NCLMetaDataItemLink dataItemLink in dataItemLinks)
				{
					hashCode = HashCodeHelper.CombineHashCodes(hashCode, dataItemLink.GetHashCode());
				}
			}
			hashCodeCalculated = true;
		}
		return hashCode;
	}

	public int GetValueInsensitiveHashCode()
	{
		if (!valueInsensitiveHashCodeCalculated)
		{
			valueInsensitiveHashCode = GetValueAgnosticHashCode();
			if (definedQueryColumns != null)
			{
				if (columnLayoutIsTableBased)
				{
					valueInsensitiveHashCode = HashCodeHelper.CombineHashCodes(hashCode, int.MaxValue);
				}
				else
				{
					ReadOnlyCollection<NCLMetaQueryColumn> readOnlyCollection = definedQueryColumns.AsNCLMetaQueryColumns();
					for (int i = 0; i < readOnlyCollection.Count; i++)
					{
						valueInsensitiveHashCode = HashCodeHelper.CombineHashCodes(valueInsensitiveHashCode, readOnlyCollection[i].GetValueInsensitiveHashCode());
					}
				}
			}
			if (tableFiltersAndMarks != null)
			{
				valueInsensitiveHashCode = HashCodeHelper.CombineHashCodes(valueInsensitiveHashCode, tableFiltersAndMarks.GetValueInsensitiveHashCode());
			}
			if (subQueryDefinition != null)
			{
				valueInsensitiveHashCode = HashCodeHelper.CombineHashCodes(valueInsensitiveHashCode, subQueryDefinition.GetValueInsensitiveHashCode());
			}
			if (!IsFirstDataItemInQuery && dataItemLinks != null)
			{
				foreach (NCLMetaDataItemLink dataItemLink in dataItemLinks)
				{
					valueInsensitiveHashCode = HashCodeHelper.CombineHashCodes(valueInsensitiveHashCode, dataItemLink.GetValueInsensitiveHashCode());
				}
			}
			valueInsensitiveHashCodeCalculated = true;
		}
		return valueInsensitiveHashCode;
	}

	private bool ValueAgnosticEquals(NCLMetaQueryDataItem otherDataItem)
	{
		if (TableNo != otherDataItem.TableNo || tableFiltersAndMarks == null != (otherDataItem.tableFiltersAndMarks == null) || dataItemLinks == null != (otherDataItem.dataItemLinks == null) || lockState != otherDataItem.lockState || definedQueryColumns == null != (otherDataItem.definedQueryColumns == null) || subQueryDefinition == null != (otherDataItem.subQueryDefinition == null))
		{
			return false;
		}
		if (definedQueryColumns != null && definedQueryColumns.Count != otherDataItem.definedQueryColumns.Count)
		{
			return false;
		}
		if (!IsFirstDataItemInQuery && (dataItemLinkType != otherDataItem.dataItemLinkType || (dataItemLinks != null && dataItemLinks.Count != otherDataItem.dataItemLinks.Count)))
		{
			return false;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is NCLMetaQueryDataItem nCLMetaQueryDataItem) || GetHashCode() != nCLMetaQueryDataItem.GetHashCode() || !ValueAgnosticEquals(nCLMetaQueryDataItem))
		{
			return false;
		}
		if (definedQueryColumns != null && definedQueryColumns != nCLMetaQueryDataItem.definedQueryColumns)
		{
			IEnumerator<NCLMetaQueryColumn> enumerator = nCLMetaQueryDataItem.definedQueryColumns.AsNCLMetaQueryColumns().GetEnumerator();
			foreach (NCLMetaQueryColumn item in definedQueryColumns.AsNCLMetaQueryColumns())
			{
				enumerator.MoveNext();
				if (!item.Equals(enumerator.Current))
				{
					return false;
				}
			}
		}
		if (tableFiltersAndMarks != null && !tableFiltersAndMarks.Equals(nCLMetaQueryDataItem.tableFiltersAndMarks))
		{
			return false;
		}
		if (subQueryDefinition != null && !subQueryDefinition.Equals(nCLMetaQueryDataItem.subQueryDefinition))
		{
			return false;
		}
		if (!IsFirstDataItemInQuery && dataItemLinks != null)
		{
			IEnumerator<NCLMetaDataItemLink> enumerator3 = nCLMetaQueryDataItem.dataItemLinks.GetEnumerator();
			foreach (NCLMetaDataItemLink dataItemLink in dataItemLinks)
			{
				enumerator3.MoveNext();
				if (!enumerator3.Current.Equals(dataItemLink))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool ValueInsensitiveEquals(object other)
	{
		if (this == other)
		{
			return true;
		}
		NCLMetaQueryDataItem nCLMetaQueryDataItem = other as NCLMetaQueryDataItem;
		if (GetValueInsensitiveHashCode() != nCLMetaQueryDataItem.GetValueInsensitiveHashCode() || !ValueAgnosticEquals(nCLMetaQueryDataItem))
		{
			return false;
		}
		if (definedQueryColumns != null && definedQueryColumns != nCLMetaQueryDataItem.definedQueryColumns)
		{
			IEnumerator<NCLMetaQueryColumn> enumerator = nCLMetaQueryDataItem.definedQueryColumns.AsNCLMetaQueryColumns().GetEnumerator();
			foreach (NCLMetaQueryColumn item in definedQueryColumns.AsNCLMetaQueryColumns())
			{
				enumerator.MoveNext();
				if (!item.ValueInsensitiveEquals(enumerator.Current))
				{
					return false;
				}
			}
		}
		if (tableFiltersAndMarks != null && !tableFiltersAndMarks.ValueInsensitiveEquals(nCLMetaQueryDataItem.tableFiltersAndMarks))
		{
			return false;
		}
		if (subQueryDefinition != null && !subQueryDefinition.ValueInsensitiveEquals(nCLMetaQueryDataItem.subQueryDefinition))
		{
			return false;
		}
		if (!IsFirstDataItemInQuery && dataItemLinks != null)
		{
			IEnumerator<NCLMetaDataItemLink> enumerator3 = nCLMetaQueryDataItem.dataItemLinks.GetEnumerator();
			foreach (NCLMetaDataItemLink dataItemLink in dataItemLinks)
			{
				enumerator3.MoveNext();
				if (!enumerator3.Current.ValueInsensitiveEquals(dataItemLink))
				{
					return false;
				}
			}
		}
		return true;
	}

	private NCLMetaQueryDataItem(NCLMetaQueryDefinition parentQueryDefinition, DataItemLinkType dataItemLinkType, int tableNo, DataLockState lockState, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, FiltersAndMarks> tableFiltersAndMarksCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, List<MetaQueryFieldFilter> metaQueryFieldFilters, int appGroup)
	{
		this.parentQueryDefinition = parentQueryDefinition;
		this.tableNo = tableNo;
		AppGroup = appGroup;
		this.lockState = lockState;
		this.name = name;
		this.dataItemLinkType = dataItemLinkType;
		metaTableLookup = AppGroupAwareLookup<NCLMetaTable>.CreateWithDefaultRefreshLogic(ResolveMetaTable);
		if (queryColumnsCreator != null)
		{
			definedQueryColumns = queryColumnsCreator(this);
			columnLayoutIsTableBased = definedQueryColumns == TableBasedColumnLayout;
			if (columnLayoutIsTableBased)
			{
				this.parentQueryDefinition.NextColumnIndex = MetaTable.AllFields.Max((NCLMetaField field) => field.ColumnIndex) + 1;
				INavFieldMetadata[] fields = (from field in MetaTable.AllFields
					where field.FieldActive && field.FieldClass != FieldClass.FlowField && (!field.FieldIsTimestamp || field.Parent.SqlHasTimestamp)
					select NCLMetaQueryColumn.Create(this, field, field.FieldClass != FieldClass.Normal, field.ColumnIndex)).ToArray();
				definedQueryColumns = new FieldList(fields);
			}
		}
		if (tableFiltersAndMarksCreator != null)
		{
			tableFiltersAndMarks = tableFiltersAndMarksCreator(this);
			if (tableFiltersAndMarks == filtersAndMarksNotEvaluated)
			{
				this.metaQueryFieldFilters = metaQueryFieldFilters;
			}
		}
		if (dataItemLinksCreator != null)
		{
			dataItemLinks = dataItemLinksCreator(this);
		}
	}

	private NCLMetaQueryDataItem(NCLMetaQueryDefinition parentQueryDefinition, DataItemLinkType dataItemLinkType, Func<NCLMetaQueryDataItem, NCLMetaQueryDefinition> subQueryCreator, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, int appGroup)
	{
		this.parentQueryDefinition = parentQueryDefinition;
		subQueryDefinition = subQueryCreator(this);
		AppGroup = appGroup;
		metaTableLookup = AppGroupAwareLookup<NCLMetaTable>.CreateWithDefaultRefreshLogic(ResolveMetaTable);
		if (subQueryDefinition != null)
		{
			this.name = name;
			this.dataItemLinkType = dataItemLinkType;
			if (queryColumnsCreator != null)
			{
				definedQueryColumns = queryColumnsCreator(this);
			}
			if (dataItemLinksCreator != null)
			{
				dataItemLinks = dataItemLinksCreator(this);
			}
		}
	}

	private NCLMetaTable ResolveMetaTable(NavAppGroup appGroup)
	{
		return NavGlobal.NCLMetadata.GetMetaTableById(tableNo, appGroup.GroupId, requireCompiled: false);
	}

	internal static NCLMetaQueryDataItem CreateFromMetadata(NCLMetaQueryDefinition parentQueryDefinition, DataItemLinkType dateItemLinkType, int tableNo, DataLockState lockState, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, FiltersAndMarks> tableFiltersAndMarksCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, List<MetaQueryFieldFilter> fieldFilters, int appGroup = -1)
	{
		return new NCLMetaQueryDataItem(parentQueryDefinition, dateItemLinkType, tableNo, lockState, name, queryColumnsCreator, tableFiltersAndMarksCreator, dataItemLinksCreator, fieldFilters, appGroup);
	}

	internal static NCLMetaQueryDataItem Create(NCLMetaQueryDefinition parentQueryDefinition, DataItemLinkType dateItemLinkType, int tableNo, DataLockState lockState, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, FiltersAndMarks> tableFiltersAndMarksCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItem(parentQueryDefinition, dateItemLinkType, tableNo, lockState, name, queryColumnsCreator, tableFiltersAndMarksCreator, dataItemLinksCreator, null, appGroup);
	}

	internal static NCLMetaQueryDataItem Create(NCLMetaQueryDefinition parentQueryDefinition, int tableNo, DataLockState lockState, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, FiltersAndMarks> tableFiltersAndMarksCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItem(parentQueryDefinition, DataItemLinkType.None, tableNo, lockState, name, queryColumnsCreator, tableFiltersAndMarksCreator, dataItemLinksCreator, null, appGroup);
	}

	internal static NCLMetaQueryDataItem Create(NCLMetaQueryDefinition parentQueryDefinition, NCLMetaTable table, DataLockState lockState, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, FiltersAndMarks> tableFiltersAndMarksCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItem(parentQueryDefinition, DataItemLinkType.None, table.TableId, lockState, name, queryColumnsCreator, tableFiltersAndMarksCreator, dataItemLinksCreator, null, appGroup);
	}

	internal static NCLMetaQueryDataItem CreateWithSubQuery(NCLMetaQueryDefinition parentQueryDefinition, DataItemLinkType dataItemLinkType, Func<NCLMetaQueryDataItem, NCLMetaQueryDefinition> subQueryCreator, string name, Func<NCLMetaQueryDataItem, FieldList> queryColumnsCreator, Func<NCLMetaQueryDataItem, ReadOnlyCollection<NCLMetaDataItemLink>> dataItemLinksCreator, int appGroup = -1)
	{
		return new NCLMetaQueryDataItem(parentQueryDefinition, dataItemLinkType, subQueryCreator, name, queryColumnsCreator, dataItemLinksCreator, appGroup);
	}
}
