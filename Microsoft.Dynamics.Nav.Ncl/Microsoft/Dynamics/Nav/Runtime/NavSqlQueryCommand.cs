using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlQueryCommand : NavSqlCommand
{
	private class ReferenceBasedDataItemEqualityComparer : EqualityComparer<NCLMetaQueryDataItem>
	{
		public override int GetHashCode(NCLMetaQueryDataItem obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}

		public override bool Equals(NCLMetaQueryDataItem x, NCLMetaQueryDataItem y)
		{
			return x == y;
		}
	}

	private class CachingNavFieldMetadataProvider : INavFieldMetadata, INavValueMetadata
	{
		private readonly INavFieldMetadata source;

		private SqlDataType? sqlDataType;

		private NavNclType? nclType;

		private NavValue emptyValue;

		private NCLOptionMetadata navOptionMetadata;

		private int? navDefinedLengthMetadata;

		private int? columnIndex;

		public FilterExpressionContext ExpressionContext => source.ExpressionContext;

		public FieldClass FieldClass => source.FieldClass;

		public NavValue InitValue => source.InitValue;

		public string DescriptiveName => source.DescriptiveName;

		public string DescriptiveParentName => source.DescriptiveParentName;

		public NavType NavType => source.NavType;

		public NavValue EmptyValue => emptyValue ?? (emptyValue = source.EmptyValue);

		public NCLOptionMetadata NavOptionMetadata => navOptionMetadata ?? (navOptionMetadata = source.NavOptionMetadata);

		public SqlDataType SqlDataType
		{
			get
			{
				if (!this.sqlDataType.HasValue)
				{
					SqlDataType? sqlDataType = (this.sqlDataType = source.SqlDataType);
					return sqlDataType.Value;
				}
				return this.sqlDataType.Value;
			}
		}

		public NavNclType NclType
		{
			get
			{
				if (!nclType.HasValue)
				{
					NavNclType? navNclType = (nclType = source.NclType);
					return navNclType.Value;
				}
				return nclType.Value;
			}
		}

		public int NavDefinedLengthMetadata
		{
			get
			{
				if (!navDefinedLengthMetadata.HasValue)
				{
					int? num = (navDefinedLengthMetadata = source.NavDefinedLengthMetadata);
					return num.Value;
				}
				return navDefinedLengthMetadata.Value;
			}
		}

		public int ColumnIndex
		{
			get
			{
				if (!columnIndex.HasValue)
				{
					int? num = (columnIndex = source.ColumnIndex);
					return num.Value;
				}
				return columnIndex.Value;
			}
		}

		public int ParentTable { get; }

		public CachingNavFieldMetadataProvider(NCLMetaQueryColumn source)
		{
			this.source = source;
			ParentTable = source.ParentDataItem.TableNo;
		}
	}

	private class QueryCommandCacheKey : NavSqlCollationSensitiveCommandCacheKey
	{
		private readonly int companyToken;

		public NCLMetaQueryDefinition Query { get; private set; }

		public bool CountRows { get; private set; }

		public int SkipNumberOfRows { get; private set; }

		public int TopNumberOfRowsToReturn { get; private set; }

		public FilterFieldDictionary QueryFiltersOnFlowFilters { get; private set; }

		public FilterExpression QueryAndPositioningFilter { get; private set; }

		public FilterExpression FilterOnlyColumnsFilter { get; private set; }

		public bool AddExplicitJoinHints { get; private set; }

		public int AppGroupId { get; private set; }

		public Dictionary<int, FilterFieldDictionary> GlobalAndSecurityFilters { get; private set; }

		public SortingFieldList SortingFieldList { get; private set; }

		public QueryCommandCacheKey(NCLMetaQueryDefinition query, NavDatabase database, int companyToken, int topNumberOfRowsToReturn, int skipNumberOfRows, bool countRows, FilterFieldDictionary queryFiltersOnFlowFilters, FilterExpression queryAndPositioningFilter, FilterExpression filterOnlyColumnsFilter, Dictionary<int, FilterFieldDictionary> globalAndSecurityFilters, SortingFieldList sortingFieldList, bool addExplicitJoinHints, int appGroupId)
			: base(CachedCommandType.Query, database, HashCodeHelper.CombineHashCodes((query.GetHashCode() << 8) | companyToken, ValueInsensitiveFilterExpressionEqualityComparer.Instance.GetHashCode(queryAndPositioningFilter), ValueInsensitiveFilterExpressionEqualityComparer.Instance.GetHashCode(filterOnlyColumnsFilter), GetFlowFiltersHashCode(queryFiltersOnFlowFilters), GetGlobalAndSecurityFilterHashCode(globalAndSecurityFilters), appGroupId))
		{
			Query = query;
			this.companyToken = companyToken;
			TopNumberOfRowsToReturn = topNumberOfRowsToReturn;
			SkipNumberOfRows = skipNumberOfRows;
			CountRows = countRows;
			QueryFiltersOnFlowFilters = queryFiltersOnFlowFilters;
			QueryAndPositioningFilter = queryAndPositioningFilter;
			FilterOnlyColumnsFilter = filterOnlyColumnsFilter;
			GlobalAndSecurityFilters = globalAndSecurityFilters;
			SortingFieldList = sortingFieldList;
			AddExplicitJoinHints = addExplicitJoinHints;
			AppGroupId = appGroupId;
		}

		private static int GetGlobalAndSecurityFilterHashCode(Dictionary<int, FilterFieldDictionary> globaAndSecuritylFilters)
		{
			if (globaAndSecuritylFilters == null)
			{
				return int.MaxValue;
			}
			int num = 0;
			foreach (FilterFieldDictionary value in globaAndSecuritylFilters.Values)
			{
				num = HashCodeHelper.CombineHashCodes(num, value.ValueInsensitiveHashCodeNormalFields);
			}
			return num;
		}

		private static int GetFlowFiltersHashCode(FilterFieldDictionary queryFiltersOnFlowFilters)
		{
			if (queryFiltersOnFlowFilters == null)
			{
				return int.MaxValue;
			}
			return queryFiltersOnFlowFilters.ValueInsensitiveHashCodeNormalFields;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is QueryCommandCacheKey queryCommandCacheKey))
			{
				return false;
			}
			if (!base.Equals(queryCommandCacheKey) || companyToken != queryCommandCacheKey.companyToken || !Query.ValueInsensitiveEquals(queryCommandCacheKey.Query) || TopNumberOfRowsToReturn != queryCommandCacheKey.TopNumberOfRowsToReturn || SkipNumberOfRows != queryCommandCacheKey.SkipNumberOfRows || CountRows != queryCommandCacheKey.CountRows || GlobalAndSecurityFilters == null != (queryCommandCacheKey.GlobalAndSecurityFilters == null) || SortingFieldList == null != (queryCommandCacheKey.SortingFieldList == null) || QueryFiltersOnFlowFilters == null != (queryCommandCacheKey.QueryFiltersOnFlowFilters == null) || AddExplicitJoinHints != queryCommandCacheKey.AddExplicitJoinHints || AppGroupId != queryCommandCacheKey.AppGroupId)
			{
				return false;
			}
			if (!ValueInsensitiveFilterExpressionEqualityComparer.Instance.Equals(QueryAndPositioningFilter, queryCommandCacheKey.QueryAndPositioningFilter) || !ValueInsensitiveFilterExpressionEqualityComparer.Instance.Equals(FilterOnlyColumnsFilter, queryCommandCacheKey.FilterOnlyColumnsFilter))
			{
				return false;
			}
			if (QueryFiltersOnFlowFilters != null && !QueryFiltersOnFlowFilters.FiltersOnNormalFieldsAreValueInsensitiveEqualTo(queryCommandCacheKey.QueryFiltersOnFlowFilters))
			{
				return false;
			}
			if (GlobalAndSecurityFilters != null)
			{
				if (GlobalAndSecurityFilters.Count != queryCommandCacheKey.GlobalAndSecurityFilters.Count)
				{
					return false;
				}
				IEnumerator<FilterFieldDictionary> enumerator = queryCommandCacheKey.GlobalAndSecurityFilters.Values.GetEnumerator();
				foreach (FilterFieldDictionary value in GlobalAndSecurityFilters.Values)
				{
					enumerator.MoveNext();
					if (!value.FiltersOnNormalFieldsAreValueInsensitiveEqualTo(enumerator.Current))
					{
						return false;
					}
				}
			}
			if (SortingFieldList != null)
			{
				if (SortingFieldList.Count != queryCommandCacheKey.SortingFieldList.Count)
				{
					return false;
				}
				if (!SortingFieldList.Equals(queryCommandCacheKey.SortingFieldList))
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	internal const int DefaultFindTopSize = 50;

	private readonly QueryCommandCacheKey cacheKey;

	private readonly CachingNavFieldMetadataProvider[] fieldMetadataCache;

	private Dictionary<NCLMetaQueryDataItem, NCLMetaKey> coveringSIFTIndexes;

	private NavSqlParameterHandler sqlParameterHandler;

	public override bool Stale => IsStale(cacheKey.Query);

	private NavSqlQueryCommand(NavSqlConnection parentConnection, QueryCommandCacheKey cacheKey)
		: base(parentConnection)
	{
		this.cacheKey = cacheKey;
		fieldMetadataCache = new CachingNavFieldMetadataProvider[cacheKey.Query.NextColumnIndex];
	}

	private static NavSqlQueryCommand Create(NavSqlConnection parentConnection, QueryCommandCacheKey cacheKey)
	{
		return new NavSqlQueryCommand(parentConnection, cacheKey);
	}

	private static bool IsStale(NCLMetaQueryDefinition queryDefinition)
	{
		foreach (NCLMetaQueryDataItem dataItem in queryDefinition.DataItems)
		{
			if (dataItem.SubQueryDefinition != null)
			{
				return IsStale(dataItem.SubQueryDefinition);
			}
			if (dataItem.MetaTable.Stale)
			{
				return true;
			}
		}
		return false;
	}

	private static void CheckQueryCapabilities(NCLMetaQueryDefinition query)
	{
		if (query.DataItems.Count == 0 || !query.DefinedColumns.Any((NCLMetaQueryColumn column) => column.ColumnType != QueryColumnType.FilterOnly))
		{
			throw new NavNotSupportedException(Lang.QueryStructureIsEmpty);
		}
		bool firstMetaTableInQueryIsAppTable = SystemTables.ApplicationDatabaseTables.Contains(query.IncludedTables[0].TableId);
		if (query.IncludedTables.Any((NCLMetaTable table) => SystemTables.ApplicationDatabaseTables.Contains(table.TableId) != firstMetaTableInQueryIsAppTable))
		{
			throw new NavNotSupportedException(Lang.CannotExecuteSqlQueryAcrossDatabases);
		}
		if (query.DefinedColumns.Where((NCLMetaQueryColumn column) => column.ColumnType != QueryColumnType.ConstValue).Any((NCLMetaQueryColumn column) => column.SourceTableField != null && column.SourceTableField.FieldClass == FieldClass.FlowField))
		{
			throw new NavNotSupportedException("A flow field is part of the query column list, this is not supported");
		}
	}

	internal static Dictionary<int, FilterFieldDictionary> GetSecurityAndGlobalFilters(NCLMetaQueryDefinition query, int companyToken, SecurityFiltering securityFiltering)
	{
		GlobalFilters globalFilters = ((NavCurrentThread.Session != null) ? NavCurrentThread.Session.DataAccessSource.GlobalFilters : new GlobalFilters());
		if (!globalFilters.AnyGlobalFilters && securityFiltering == SecurityFiltering.Ignored)
		{
			return null;
		}
		ReadOnlyCollection<NCLMetaTable> includedTables = query.IncludedTables;
		Dictionary<int, FilterFieldDictionary> dictionary = new Dictionary<int, FilterFieldDictionary>();
		for (int i = 0; i < includedTables.Count; i++)
		{
			int tableId = includedTables[i].TableId;
			FilterFieldDictionary globalFilters2 = globalFilters.GetGlobalFilters(tableId);
			FilterFieldDictionary filterFieldDictionary = null;
			switch (securityFiltering)
			{
			case SecurityFiltering.Filtered:
			case SecurityFiltering.Disallowed:
				filterFieldDictionary = NavCurrentThread.Session.GetSecurityFilters(companyToken, tableId, SecurityFilterType.Read, NavCurrentThread.Session.CurrentMethodScope.ApplicationObject, null);
				break;
			}
			FilterFieldDictionary filterFieldDictionary2 = DataProviderHelper.MergeFilterFieldDictionaries(globalFilters2, filterFieldDictionary);
			if (filterFieldDictionary2 != null)
			{
				dictionary.Add(tableId, filterFieldDictionary2);
			}
		}
		if (dictionary.Count != 0)
		{
			return dictionary;
		}
		return null;
	}

	private void CalculateCoveringSIFTIndexes(FilterFieldDictionary queryFiltersOnFlowFilters)
	{
		if (coveringSIFTIndexes != null)
		{
			return;
		}
		coveringSIFTIndexes = new Dictionary<NCLMetaQueryDataItem, NCLMetaKey>(new ReferenceBasedDataItemEqualityComparer());
		foreach (NCLMetaQueryDataItem allDataItem in NCLMetaQueryDefinition.GetAllDataItems(cacheKey.Query))
		{
			if (allDataItem.SubQueryDefinition == null && allDataItem.ParentQueryDefinition.AnyAggregates)
			{
				NCLMetaKey coveringSIFTIndex = allDataItem.GetCoveringSIFTIndex(cacheKey.GlobalAndSecurityFilters, queryFiltersOnFlowFilters);
				if (coveringSIFTIndex != null)
				{
					coveringSIFTIndexes.Add(allDataItem, coveringSIFTIndex);
				}
			}
		}
	}

	private static IEnumerable<NavAppOrigin> GetAllNeededCompanionTableOrigins(NCLMetaQueryDataItem targetItem, IEnumerable<NCLMetaQueryDataItem> allDataItems, FilterFieldDictionary globalAndSecurityFilters)
	{
		HashSet<NavAppOrigin> distinctOrigins = new HashSet<NavAppOrigin>();
		if (targetItem.TableNo > 0)
		{
			NCLMetaTable targetTable = targetItem.MetaTable;
			foreach (NCLMetaQueryDataItem allDataItem in allDataItems)
			{
				if (allDataItem.TableNo == targetTable.TableId)
				{
					if (allDataItem.DefinedColumns != null)
					{
						(from c in allDataItem.DefinedColumns
							where c.ColumnType != QueryColumnType.ConstValue && c.SourceTableField.IsCompanionTableField
							select c.SourceTableField.AppOrigin).ForEach((NavAppOrigin o) =>
						{
							distinctOrigins.Add(o);
						});
					}
					if (allDataItem.TableFiltersAndMarks?.Filters != null)
					{
						distinctOrigins.UnionWith(allDataItem.TableFiltersAndMarks.Filters.GetCompanionAppsOriginForTheUsedFilters());
					}
					if (globalAndSecurityFilters != null)
					{
						distinctOrigins.UnionWith(globalAndSecurityFilters.GetCompanionAppsOriginForTheUsedFilters());
					}
				}
				if (allDataItem.DataItemLinks != null)
				{
					List<NCLMetaQueryColumn> list = new List<NCLMetaQueryColumn>();
					list.AddRange(from link in allDataItem.DataItemLinks
						where link.LinkType == NCLMetaFilterType.Field
						select link.SourceColumn);
					list.AddRange(allDataItem.DataItemLinks.Select((NCLMetaDataItemLink link) => link.DestinationColumn));
					(from c in list
						where c != null && c.ColumnType != QueryColumnType.ConstValue && c.SourceTableField.Parent.TableId == targetTable.TableId && c.SourceTableField.IsCompanionTableField
						select c.SourceTableField.AppOrigin).ForEach((NavAppOrigin o) =>
					{
						distinctOrigins.Add(o);
					});
				}
			}
		}
		return distinctOrigins;
	}

	private static void AppendCompanionTableJoins(StringBuilder tableListAndJoinClause, NCLMetaQueryDataItem dataItem, IEnumerable<NavAppOrigin> companionOrigins, NavDatabase database, int companyToken, string hints)
	{
		string sqlDataItemName = dataItem.SqlDataItemName;
		foreach (NavAppOrigin companionOrigin in companionOrigins)
		{
			NavSqlStatementHelper.AppendCompanionTableJoinStatement(tableListAndJoinClause, dataItem.MetaTable, database, companyToken, hints, companionOrigin, qualifyTableWithDatabase: true, sqlDataItemName);
		}
	}

	private void AppendColumns(StringBuilder fieldList, StringBuilder groupByClause, NCLMetaQueryDataItem dataItem, bool treatNullAsDefaultValue)
	{
		foreach (NCLMetaQueryColumn item in dataItem.QueryColumns.Where((NCLMetaQueryColumn queryColumn) => queryColumn.ColumnType != QueryColumnType.FilterOnly))
		{
			string text = NavSqlStatementHelper.QueryColumnSourceExpression(item, coveringSIFTIndexes, treatNullAsDefaultValue, sqlParameterHandler);
			if (item.NclType == NavNclType.NavBlob && item.ParentDataItem.SubQueryDefinition == null)
			{
				text = string.Format(CultureInfo.InvariantCulture, "DATALENGTH({0})", text);
			}
			fieldList.Append(text);
			if (!item.IsAggregated && item.ColumnType != QueryColumnType.ConstValue && dataItem.ParentQueryDefinition.AnyAggregates)
			{
				groupByClause.Append(text);
				groupByClause.Append(',');
			}
			if (!string.IsNullOrEmpty(item.Name))
			{
				fieldList.Append(" AS ");
				NavSqlStatementHelper.AppendQuotedIdentifier(fieldList, item.SqlColumnName);
			}
			fieldList.Append(',');
		}
	}

	private void AppendFilter(FilterExpression filter, StringBuilder whereClause, bool treatNullAsDefaultValue, string fixedParentName = null)
	{
		if (filter != null)
		{
			if (whereClause.Length > 0)
			{
				whereClause.Append(" AND ");
			}
			whereClause.Append('(');
			NavSqlWhereClauseVisitorInputOutput input = NavSqlWhereClauseVisitorInputOutput.CreateForTextualWhereClauseGeneration(whereClause, sqlParameterHandler, coveringSIFTIndexes, treatNullAsDefaultValue, fixedParentName, cacheKey.SqlSortingProperties, shouldAliasQualifyFields: true);
			filter.Visit(NavSqlWhereClauseVisitor.DefaultVisitor, input);
			whereClause.Append(')');
		}
	}

	private static void AppendCompanyIdFilter(StringBuilder whereClause, string sqlDataItemName, NavSqlParameterHandler sqlParameterHandler, CompanyId companyId)
	{
		if (whereClause.Length > 0)
		{
			whereClause.Append(" AND ");
		}
		whereClause.Append("(").Append(NavSqlStatementHelper.QuoteIdentifier(sqlDataItemName)).Append(".")
			.Append(NavSqlStatementHelper.SqlStatementCompanyIdEqualsToParameterNo(sqlParameterHandler.GetCompanyParameterNo(companyId)))
			.Append(" OR ")
			.Append(NavSqlStatementHelper.QuoteIdentifier(sqlDataItemName))
			.Append(".\"$companyId\"")
			.Append(" IS NULL)");
	}

	private static void AppendCompanyJoinFilter(StringBuilder whereClause, string sqlDataItemName, string sqlDataItemName2)
	{
		if (whereClause.Length > 0)
		{
			whereClause.Append(" AND ");
		}
		whereClause.Append(NavSqlStatementHelper.QuoteIdentifier(sqlDataItemName)).Append(".\"$companyId\" = ").Append(NavSqlStatementHelper.QuoteIdentifier(sqlDataItemName2))
			.Append(".\"$companyId\"");
	}

	private static void UpdatePerformanceCountersWithQueryRepositionResult(bool repositionResult)
	{
		NavEnvironment.Instance.PerformanceCounterSetter.UpdatePerformanceCountersWithQueryRepositionResult(repositionResult);
	}

	private static void AppendOrderByColumns(StringBuilder fieldList, ReadOnlyCollection<NCLMetaQueryOrderBy> orderByColumns)
	{
		fieldList.Append(" ORDER BY ");
		for (int i = 0; i < orderByColumns.Count; i++)
		{
			NCLMetaQueryOrderBy nCLMetaQueryOrderBy = orderByColumns[i];
			NCLMetaQueryColumn column = nCLMetaQueryOrderBy.Column;
			NavSqlStatementHelper.AppendQuotedIdentifier(fieldList, column.SqlColumnName);
			fieldList.Append(' ');
			if (nCLMetaQueryOrderBy.Order == SortOrder.Descending)
			{
				fieldList.Append("DESC");
			}
			else
			{
				fieldList.Append("ASC");
			}
			fieldList.Append(',');
		}
		fieldList.Length--;
	}

	private void CreateSqlStatement(StringBuilder selectStmt, NCLMetaQueryDefinition queryDefinition, FilterExpression queryAndPositioningFilter, FilterExpression filterOnlyColumnFilters, FilterFieldDictionary queryFiltersOnFlowFilters, NavDatabase database, int companyToken, SortingFieldList sortingFields)
	{
		StringBuilder stringBuilder = new StringBuilder(" FROM ");
		StringBuilder stringBuilder2 = new StringBuilder();
		StringBuilder stringBuilder3 = new StringBuilder();
		StringBuilder stringBuilder4 = new StringBuilder();
		StringBuilder stringBuilder5 = new StringBuilder();
		bool anyOuterJoinedDataItems = cacheKey.Query.AnyOuterJoinedDataItems;
		bool flag = cacheKey.SkipNumberOfRows > 0 && !queryDefinition.IsSubquery;
		bool flag2 = cacheKey.CountRows && !queryDefinition.IsSubquery;
		ReadOnlyCollection<NCLMetaQueryOrderBy> readOnlyCollection = queryDefinition.OrderBy;
		if (sortingFields != null)
		{
			readOnlyCollection = new ReadOnlyCollection<NCLMetaQueryOrderBy>(sortingFields.Cast<NCLMetaQueryOrderBy>().ToList());
			if (readOnlyCollection.Any((NCLMetaQueryOrderBy ob) => ob == null))
			{
				throw new NavNotSupportedException("Sorting fields are not valid query columns.");
			}
		}
		if (flag && readOnlyCollection.Count <= 0)
		{
			throw new NavNotSupportedException(Lang.SkipValueWithNoOrderyByNotSupported);
		}
		if (flag2 && readOnlyCollection.Count <= 0 && GetTopRowsForQueryDefinition(queryDefinition) > 0)
		{
			throw new NavNotSupportedException(Lang.TopValueWithNoOrderyByNotSupported);
		}
		if (GetTopRowsForQueryDefinition(queryDefinition) > 0 && !flag && !flag2)
		{
			stringBuilder2.AppendFormat(CultureInfo.InvariantCulture, "TOP ({0}) ", NavSqlStatementHelper.ParameterName(sqlParameterHandler.GetNextParameterNo));
		}
		for (int num = 0; num < queryDefinition.DataItems.Count; num++)
		{
			NCLMetaQueryDataItem nCLMetaQueryDataItem = queryDefinition.DataItems[num];
			coveringSIFTIndexes.TryGetValue(nCLMetaQueryDataItem, out var value);
			string text = NavSqlStatementHelper.GenerateSelectStatementHints(nCLMetaQueryDataItem.LockState, value != null, Connection);
			bool flag3 = false;
			if (num > 0)
			{
				switch (nCLMetaQueryDataItem.DataItemLinkType)
				{
				case DataItemLinkType.InnerJoin:
					stringBuilder.Append(" JOIN ");
					flag3 = true;
					break;
				case DataItemLinkType.LeftOuterJoin:
					stringBuilder.Append(" LEFT OUTER JOIN ");
					flag3 = true;
					break;
				case DataItemLinkType.RightOuterJoin:
					stringBuilder.Append(" RIGHT OUTER JOIN ");
					flag3 = true;
					break;
				case DataItemLinkType.FullOuterJoin:
					stringBuilder.Append(" FULL OUTER JOIN ");
					flag3 = true;
					break;
				case DataItemLinkType.CrossJoin:
					stringBuilder.Append(" CROSS JOIN ");
					break;
				case DataItemLinkType.CrossApply:
					stringBuilder.Append(" CROSS APPLY ");
					break;
				case DataItemLinkType.OuterApply:
					stringBuilder.Append(" OUTER APPLY ");
					break;
				}
			}
			FilterFieldDictionary value2 = null;
			if (nCLMetaQueryDataItem.SubQueryDefinition != null)
			{
				stringBuilder.Append('(');
				CreateSqlStatement(stringBuilder, nCLMetaQueryDataItem.SubQueryDefinition, null, null, queryFiltersOnFlowFilters, database, companyToken, null);
				stringBuilder.Append(") AS ");
				NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, nCLMetaQueryDataItem.SqlDataItemName);
				stringBuilder.Append(' ');
			}
			else
			{
				if (value == null)
				{
					NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, nCLMetaQueryDataItem.MetaTable, database, companyToken, NavAppTableContext.Main);
				}
				else
				{
					NavSqlStatementHelper.AppendQuotedSiftIndexNameWithDatabaseAndSchemaQualification(stringBuilder, value, database, companyToken);
				}
				stringBuilder.Append(" AS ");
				NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, nCLMetaQueryDataItem.SqlDataItemName);
				stringBuilder.Append(text);
				if (cacheKey.GlobalAndSecurityFilters != null && cacheKey.GlobalAndSecurityFilters.TryGetValue(nCLMetaQueryDataItem.TableNo, out value2) && value2 != null)
				{
					AppendFilter(value2.ConcatenatedFilterNormalFields, stringBuilder3, treatNullAsDefaultValue: false, nCLMetaQueryDataItem.Name);
				}
				if (nCLMetaQueryDataItem.TableFiltersAndMarks != null)
				{
					AppendFilter(nCLMetaQueryDataItem.TableFiltersAndMarks.GetCombinedFilter(), stringBuilder3, treatNullAsDefaultValue: false, nCLMetaQueryDataItem.Name);
				}
				if (database.HasCompanyIdColumn(nCLMetaQueryDataItem.MetaTable))
				{
					CompanyId? companyId = database.GetCompanyId(nCLMetaQueryDataItem.MetaTable, companyToken);
					if (companyId.HasValue)
					{
						AppendCompanyIdFilter(stringBuilder3, nCLMetaQueryDataItem.SqlDataItemName, sqlParameterHandler, companyId.Value);
					}
				}
			}
			if (value == null)
			{
				IEnumerable<NavAppOrigin> allNeededCompanionTableOrigins = GetAllNeededCompanionTableOrigins(nCLMetaQueryDataItem, queryDefinition.DataItems, value2);
				AppendCompanionTableJoins(stringBuilder, nCLMetaQueryDataItem, allNeededCompanionTableOrigins, database, companyToken, text);
			}
			AppendColumns(stringBuilder4, stringBuilder5, nCLMetaQueryDataItem, anyOuterJoinedDataItems);
			if (nCLMetaQueryDataItem.DataItemLinks == null)
			{
				continue;
			}
			if (flag3)
			{
				stringBuilder.Append(" ON ");
				StringBuilder stringBuilder6 = new StringBuilder();
				AppendFilter(GetJoinExpression(nCLMetaQueryDataItem.DataItemLinks, queryFiltersOnFlowFilters), stringBuilder6, anyOuterJoinedDataItems);
				if (database.IsSharedTenantSchema)
				{
					AppendCompanyFilterForJoinParties(database, stringBuilder6, nCLMetaQueryDataItem);
				}
				stringBuilder.Append(stringBuilder6);
			}
			else
			{
				AppendFilter(GetJoinExpression(nCLMetaQueryDataItem.DataItemLinks, queryFiltersOnFlowFilters), stringBuilder3, anyOuterJoinedDataItems);
			}
		}
		selectStmt.Append("SELECT ");
		selectStmt.Append(stringBuilder2);
		selectStmt.Append(stringBuilder4);
		selectStmt.Length--;
		selectStmt.Append(stringBuilder);
		if (filterOnlyColumnFilters != null)
		{
			AppendFilter(filterOnlyColumnFilters, stringBuilder3, anyOuterJoinedDataItems);
		}
		StringBuilder stringBuilder7 = new StringBuilder();
		if (queryDefinition.AnyAggregates)
		{
			AppendFilter(queryAndPositioningFilter, stringBuilder7, anyOuterJoinedDataItems);
		}
		else
		{
			AppendFilter(queryAndPositioningFilter, stringBuilder3, anyOuterJoinedDataItems);
		}
		if (stringBuilder3.Length > 0)
		{
			selectStmt.Append(" WHERE ");
			selectStmt.Append(stringBuilder3);
		}
		if (stringBuilder5.Length > 0)
		{
			selectStmt.Append(" GROUP BY ");
			stringBuilder5.Length--;
			selectStmt.Append(stringBuilder5);
		}
		if (stringBuilder7.Length > 0)
		{
			selectStmt.Append(" HAVING ");
			selectStmt.Append(stringBuilder7);
		}
		if (readOnlyCollection.Count > 0 && (queryDefinition.TopRows > 0 || !queryDefinition.IsSubquery))
		{
			AppendOrderByColumns(selectStmt, readOnlyCollection);
			if (flag | flag2)
			{
				selectStmt.AppendFormat(CultureInfo.InvariantCulture, " OFFSET {0} ROWS ", NavSqlStatementHelper.ParameterName(sqlParameterHandler.GetNextParameterNo));
				if (GetTopRowsForQueryDefinition(queryDefinition) > 0)
				{
					selectStmt.AppendFormat(CultureInfo.InvariantCulture, " FETCH NEXT {0} ROWS ONLY ", NavSqlStatementHelper.ParameterName(sqlParameterHandler.GetNextParameterNo));
				}
			}
		}
		if (flag2)
		{
			string arg = selectStmt.ToString();
			selectStmt.Clear();
			selectStmt.AppendFormat(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM ( {0} ) QUERY_COUNT", arg);
		}
	}

	private int GetTopRowsForQueryDefinition(NCLMetaQueryDefinition queryDefinition)
	{
		int num = queryDefinition.TopRows;
		if (!queryDefinition.IsSubquery && cacheKey.TopNumberOfRowsToReturn > 0)
		{
			num = ((num == 0) ? cacheKey.TopNumberOfRowsToReturn : Math.Min(num, cacheKey.TopNumberOfRowsToReturn));
		}
		return num;
	}

	private static void AppendCompanyFilterForJoinParties(NavDatabase database, StringBuilder companyFilterClause, NCLMetaQueryDataItem dataItem)
	{
		HashSet<string> hashSet = new HashSet<string>();
		if (dataItem.SubQueryDefinition != null || !database.HasCompanyIdColumn(dataItem.MetaTable))
		{
			return;
		}
		foreach (NCLMetaDataItemLink item in dataItem.DataItemLinks.Where((NCLMetaDataItemLink l) => l.LinkType == NCLMetaFilterType.Field && l.SourceDataItem.SubQueryDefinition == null && database.HasCompanyIdColumn(l.SourceDataItem.MetaTable) && l.SourceDataItem.MetaTable.DataPerCompany == dataItem.MetaTable.DataPerCompany))
		{
			NCLMetaQueryDataItem sourceDataItem = item.SourceDataItem;
			if (!hashSet.Contains(sourceDataItem.SqlDataItemName))
			{
				hashSet.Add(sourceDataItem.SqlDataItemName);
				AppendCompanyJoinFilter(companyFilterClause, dataItem.SqlDataItemName, sourceDataItem.SqlDataItemName);
			}
		}
	}

	private void SetOrAddExpressionParameters(SqlParameterCollection parameters, FilterExpression expression, bool treatNullAsDefaultValue)
	{
		if (expression != null)
		{
			NavSqlWhereClauseVisitorInputOutput input = NavSqlWhereClauseVisitorInputOutput.CreateForParameterAssignment(parameters, sqlParameterHandler, null, treatNullAsDefaultValue, cacheKey.SqlSortingProperties);
			expression.Visit(NavSqlWhereClauseVisitor.DefaultVisitor, input);
		}
	}

	private void SetOrAddColumnParameters(NCLMetaQueryDataItem dataItem, SqlParameterCollection parameters, bool treatNullAsDefaultValue)
	{
		IEnumerable<NCLMetaQueryColumn> enumerable = (treatNullAsDefaultValue ? dataItem.QueryColumns.Where((NCLMetaQueryColumn column) => column.ColumnType == QueryColumnType.ConstValue || column.ColumnType == QueryColumnType.Normal) : dataItem.QueryColumns.Where((NCLMetaQueryColumn column) => column.ColumnType == QueryColumnType.ConstValue));
		foreach (NCLMetaQueryColumn item in enumerable)
		{
			if (item.ColumnType == QueryColumnType.Normal && item.NclType == NavNclType.NavBlob)
			{
				continue;
			}
			int num = ((item.ColumnType == QueryColumnType.ConstValue) ? sqlParameterHandler.GetNextParameterNo : sqlParameterHandler.GetParameterNoForEmptyValue(item));
			SqlParameter sqlParameter;
			if (num >= parameters.Count)
			{
				sqlParameter = new SqlParameter
				{
					ParameterName = NavSqlStatementHelper.ParameterName(num)
				};
				parameters.Add(sqlParameter);
			}
			else
			{
				sqlParameter = parameters[num];
			}
			if (item.ColumnType == QueryColumnType.ConstValue)
			{
				NavSqlStatementHelper.SetSqlParameter(sqlParameter, item, item.ConstValue.GetSqlWritableValue(item.SqlDataType, item.NavDefinedLengthMetadata), isWildcardValue: false, isFilterValue: false);
				continue;
			}
			Func<INavFieldMetadata, object> getEmptySqlValue = (INavFieldMetadata f) => f.EmptyValue.GetSqlWritableValue(f.SqlDataType, f.NavDefinedLengthMetadata);
			NavSqlStatementHelper.SetSqlParameterWithEmptyValue(sqlParameter, item, getEmptySqlValue);
		}
	}

	private static FilterExpression GetJoinExpression(ReadOnlyCollection<NCLMetaDataItemLink> links, FilterFieldDictionary queryFiltersOnFlowFilters)
	{
		FilterExpression filterExpression = null;
		foreach (NCLMetaDataItemLink link in links)
		{
			FilterExpression value = null;
			switch (link.LinkType)
			{
			case NCLMetaFilterType.Field:
				if (link.SourceColumn.SourceTableField.FieldClass == FieldClass.FlowFilter)
				{
					if (!(queryFiltersOnFlowFilters != null) || !queryFiltersOnFlowFilters.TryGetValue(link.SourceColumn, out value))
					{
						break;
					}
					value = value.TransferToDifferentContext(link.DestinationColumn.ExpressionContext);
					if (link.OnlyMaxLimit)
					{
						RangeList rangeList = value.ToRangeList();
						UnaryFilterExpression unaryFilterExpression = DataProviderHelper.CreateSingleUpperBoundedUnaryFilterExpressionForOnlyMaxLimitMetadataSetting(rangeList, link.DestinationColumn.ExpressionContext);
						if (unaryFilterExpression == null)
						{
							throw new NavCSideException(18023434, string.Format(CultureInfo.CurrentCulture, Lang.QueryFilterMustSpecifyASingleRange, link.DestinationColumn.DescriptiveName, link.DestinationColumn.DescriptiveParentName));
						}
						value = unaryFilterExpression;
					}
				}
				else
				{
					value = link.Expression;
				}
				break;
			case NCLMetaFilterType.Const:
			case NCLMetaFilterType.Expression:
				value = link.Expression;
				break;
			}
			filterExpression = ((filterExpression == null) ? value : FilterExpression.And(filterExpression, value));
		}
		return filterExpression;
	}

	private void SetOrAddParameters(NCLMetaQueryDefinition queryDefinition, FilterExpression queryAndPositioningFilter, FilterExpression filterOnlyColumnFilters, FilterFieldDictionary queryFiltersOnFlowFilters, Dictionary<int, FilterFieldDictionary> globalAndSecurityFilters, NavDatabase database, int companyToken)
	{
		int topRowsForQueryDefinition = GetTopRowsForQueryDefinition(queryDefinition);
		bool flag = cacheKey.SkipNumberOfRows > 0 && !queryDefinition.IsSubquery;
		bool flag2 = cacheKey.CountRows && !queryDefinition.IsSubquery;
		if (topRowsForQueryDefinition > 0 && !flag && !flag2)
		{
			NavSqlStatementHelper.SetSqlParameter(NavSqlStatementHelper.GetOrCreateSqlParameterNo(Command.Parameters, sqlParameterHandler.GetNextParameterNo), topRowsForQueryDefinition, SqlDbType.BigInt);
		}
		bool anyOuterJoinedDataItems = cacheKey.Query.AnyOuterJoinedDataItems;
		for (int i = 0; i < queryDefinition.DataItems.Count; i++)
		{
			NCLMetaQueryDataItem nCLMetaQueryDataItem = queryDefinition.DataItems[i];
			if (nCLMetaQueryDataItem.SubQueryDefinition != null)
			{
				SetOrAddParameters(nCLMetaQueryDataItem.SubQueryDefinition, null, null, queryFiltersOnFlowFilters, globalAndSecurityFilters, database, companyToken);
			}
			else
			{
				if (globalAndSecurityFilters != null && globalAndSecurityFilters.TryGetValue(nCLMetaQueryDataItem.TableNo, out var value) && value != null)
				{
					SetOrAddExpressionParameters(Command.Parameters, value.ConcatenatedFilterNormalFields, treatNullAsDefaultValue: false);
				}
				if (nCLMetaQueryDataItem.TableFiltersAndMarks != null)
				{
					SetOrAddExpressionParameters(Command.Parameters, nCLMetaQueryDataItem.TableFiltersAndMarks.GetCombinedFilter(), treatNullAsDefaultValue: false);
				}
				if (database.HasCompanyIdColumn(nCLMetaQueryDataItem.MetaTable))
				{
					CompanyId? companyId = database.GetCompanyId(nCLMetaQueryDataItem.MetaTable, companyToken);
					if (companyId.HasValue)
					{
						NavSqlStatementHelper.SetSqlParameter(NavSqlStatementHelper.GetOrCreateSqlParameterNo(Command.Parameters, sqlParameterHandler.GetCompanyParameterNo(companyId.Value)), companyId.Value);
					}
				}
			}
			SetOrAddColumnParameters(nCLMetaQueryDataItem, Command.Parameters, anyOuterJoinedDataItems);
			if (nCLMetaQueryDataItem.DataItemLinks != null)
			{
				SetOrAddExpressionParameters(Command.Parameters, GetJoinExpression(nCLMetaQueryDataItem.DataItemLinks, queryFiltersOnFlowFilters), anyOuterJoinedDataItems);
			}
		}
		if (filterOnlyColumnFilters != null)
		{
			SetOrAddExpressionParameters(Command.Parameters, filterOnlyColumnFilters, anyOuterJoinedDataItems);
		}
		if (queryAndPositioningFilter != null)
		{
			SetOrAddExpressionParameters(Command.Parameters, queryAndPositioningFilter, anyOuterJoinedDataItems);
		}
		if (flag | flag2)
		{
			NavSqlStatementHelper.SetSqlParameter(NavSqlStatementHelper.GetOrCreateSqlParameterNo(Command.Parameters, sqlParameterHandler.GetNextParameterNo), cacheKey.SkipNumberOfRows, SqlDbType.BigInt);
			if (topRowsForQueryDefinition > 0)
			{
				NavSqlStatementHelper.SetSqlParameter(NavSqlStatementHelper.GetOrCreateSqlParameterNo(Command.Parameters, sqlParameterHandler.GetNextParameterNo), topRowsForQueryDefinition, SqlDbType.BigInt);
			}
		}
	}

	private ReadOnlyRecordBuffer CreateRecordBufferFromReader()
	{
		ReadOnlyCollection<NCLMetaQueryColumn> columns = cacheKey.Query.Columns;
		NavValue[] array = new NavValue[cacheKey.Query.NextColumnIndex];
		for (int i = 0; i < columns.Count; i++)
		{
			CachingNavFieldMetadataProvider cachingNavFieldMetadataProvider = fieldMetadataCache[i] ?? (fieldMetadataCache[i] = new CachingNavFieldMetadataProvider(columns[i]));
			array[cachingNavFieldMetadataProvider.ColumnIndex] = CreateNavValueFromReader(cachingNavFieldMetadataProvider, i, cachingNavFieldMetadataProvider.ParentTable);
		}
		return new ReadOnlyRecordBuffer(array);
	}

	private void EnsureCommandIsCreatedAndPrepared(NCLMetaQueryDefinition query, FilterExpression queryAndPositioningFilter, FilterExpression filterOnlyColumnFilters, FilterFieldDictionary queryFiltersOnFlowFilters, Dictionary<int, FilterFieldDictionary> globalAndSecurityFilters, bool addExplicitJoinHints, NavDatabase database, int companyToken, SortingFieldList sortingFields)
	{
		if (State == NavSqlCommandState.ExecutedWithPendingResults)
		{
			CloseReader(forceClose: true);
		}
		if (State == NavSqlCommandState.Created)
		{
			CheckQueryCapabilities(query);
			CalculateCoveringSIFTIndexes(queryFiltersOnFlowFilters);
			StringBuilder stringBuilder = new StringBuilder();
			sqlParameterHandler = new NavSqlParameterHandler();
			CreateSqlStatement(stringBuilder, query, queryAndPositioningFilter, filterOnlyColumnFilters, queryFiltersOnFlowFilters, database, companyToken, sortingFields);
			bool flag = addExplicitJoinHints;
			if (flag && query.AnyOuterJoinedDataItems && query.DataItems.Any((NCLMetaQueryDataItem dataItem) => dataItem.DataItemLinkType == DataItemLinkType.RightOuterJoin || dataItem.DataItemLinkType == DataItemLinkType.FullOuterJoin))
			{
				flag = false;
			}
			NavSqlStatementHelper.AppendSelectStatementOptions(stringBuilder, addFastOption: true, addExplicitJoinHints, flag);
			SetCommandText(stringBuilder.ToString());
		}
		sqlParameterHandler = new NavSqlParameterHandler();
		SetOrAddParameters(query, queryAndPositioningFilter, filterOnlyColumnFilters, queryFiltersOnFlowFilters, globalAndSecurityFilters, database, companyToken);
		PrepareStatement();
	}

	private static FilterFieldDictionary GetQueryFiltesOnFlowFilters(FiltersAndMarks queryFilters)
	{
		if (queryFilters == null || queryFilters.Filters == null)
		{
			return null;
		}
		IEnumerable<Tuple<INavFieldMetadata, FilterExpression>> enumerable = queryFilters.Filters.Items.Where((Tuple<INavFieldMetadata, FilterExpression> filter) =>
		{
			NCLMetaQueryColumn nCLMetaQueryColumn = (NCLMetaQueryColumn)filter.Item1;
			return nCLMetaQueryColumn.ColumnType != QueryColumnType.ConstValue && nCLMetaQueryColumn.SourceTableField.FieldClass == FieldClass.FlowFilter;
		});
		if (!enumerable.Any())
		{
			return null;
		}
		return new FilterFieldDictionary(enumerable);
	}

	private static bool GetQueryAndPositioningFilter(NCLMetaQueryDefinition query, MutableRecordBuffer startingPosition, bool includeCurrent, FiltersAndMarks queryFilters, SortingFieldList sortingFields, out FilterExpression queryAndPositioningFilter, out FilterExpression filterOnlyColumnFilters)
	{
		queryAndPositioningFilter = null;
		filterOnlyColumnFilters = null;
		if (startingPosition != null)
		{
			if ((sortingFields != null && sortingFields.Count == 0) || query.OrderBy.Count == 0)
			{
				return false;
			}
			if (queryFilters != null && queryFilters.Filters != null)
			{
				FiltersAndMarks filtersAndMarks = new FiltersAndMarks(new FilterFieldDictionary(queryFilters.Filters.Items.Where((Tuple<INavFieldMetadata, FilterExpression> item) =>
				{
					NCLMetaQueryColumn nCLMetaQueryColumn = (NCLMetaQueryColumn)item.Item1;
					return nCLMetaQueryColumn.ColumnType == QueryColumnType.Normal && nCLMetaQueryColumn.SourceTableField.FieldClass == FieldClass.Normal;
				})));
				queryAndPositioningFilter = DataProviderHelper.BuildOptimizedCompletePositioningFilter(startingPosition, sortingFields ?? query.PositioningFields, includeCurrent, filtersAndMarks);
				if (queryAndPositioningFilter != null && queryAndPositioningFilter.IsConstantFalse())
				{
					return false;
				}
			}
			else
			{
				queryAndPositioningFilter = DataProviderHelper.BuildPositioningFilterFromRecordBuffer(startingPosition, sortingFields ?? query.PositioningFields, includeCurrent);
			}
		}
		else if (queryFilters != null && queryFilters.Filters != null)
		{
			queryAndPositioningFilter = queryFilters.Filters.GetConcatenatedFilterExpression((Tuple<INavFieldMetadata, FilterExpression> filter) =>
			{
				NCLMetaQueryColumn nCLMetaQueryColumn = (NCLMetaQueryColumn)filter.Item1;
				return nCLMetaQueryColumn.ColumnType == QueryColumnType.Normal && nCLMetaQueryColumn.SourceTableField.FieldClass == FieldClass.Normal;
			});
		}
		queryAndPositioningFilter = ValueInsensitiveFilterExpressionEqualityComparer.GetExpressionWithValueTokens(queryAndPositioningFilter);
		if (queryFilters != null && queryFilters.Filters != null)
		{
			filterOnlyColumnFilters = queryFilters.Filters.GetConcatenatedFilterExpression((Tuple<INavFieldMetadata, FilterExpression> filter) =>
			{
				NCLMetaQueryColumn nCLMetaQueryColumn = (NCLMetaQueryColumn)filter.Item1;
				return nCLMetaQueryColumn.ColumnType == QueryColumnType.FilterOnly && nCLMetaQueryColumn.SourceTableField.FieldClass == FieldClass.Normal;
			});
		}
		return true;
	}

	private static NavSqlQueryCommand QueryExecute(NavSqlConnectionScope conScope, NCLMetaQueryDefinition query, NavDatabase database, int companyToken, int topNumberOfRowsToReturn, int skipNumberOfRows, bool countRows, FiltersAndMarks queryFilters, TableFilterDictionary globalAndSecurityFilters, bool includeCurrent, MutableRecordBuffer startingPosition, SecurityFiltering securityFiltering, bool addExplicitJoinHints, SortingFieldList sortingFields, int? timeout = null)
	{
		if (!conScope.Connection.CurrentTransactionIsWriteTransaction && query.DataItems.Any((NCLMetaQueryDataItem q) => q.LockState != DataLockState.None))
		{
			NCLMetaQueryDataItem nCLMetaQueryDataItem = query.DataItems.FirstOrDefault((NCLMetaQueryDataItem q) => q.LockState != DataLockState.None);
			string original = string.Format(CultureInfo.InvariantCulture, "Attempt to acquire lock in read transaction, that will not be ended after the sql connection is returned to the connection pool.\r\nQuery dataItem={0}, lockState={1}", nCLMetaQueryDataItem.Name, nCLMetaQueryDataItem.LockState);
			NavCurrentThread.Session.Tenant.Diagnostics.SendTraceTag("00000KJ", Category.Sql, Verbosity.Warning, PrivacyClassification.SystemMetadata, TraceStringHelper.AppendStackTrace(original));
		}
		if (!GetQueryAndPositioningFilter(query, startingPosition, includeCurrent, queryFilters, sortingFields, out var queryAndPositioningFilter, out var filterOnlyColumnFilters))
		{
			return null;
		}
		FilterFieldDictionary queryFiltersOnFlowFilters = GetQueryFiltesOnFlowFilters(queryFilters);
		Dictionary<int, FilterFieldDictionary> globalAndSecurityFiltersDictionary = ((globalAndSecurityFilters == null) ? GetSecurityAndGlobalFilters(query, companyToken, securityFiltering) : globalAndSecurityFilters.ToDictionary());
		CheckDisallowedSecurityFiltering(globalAndSecurityFiltersDictionary, securityFiltering);
		int groupId = NavCurrentThread.ResolveAppGroup().GroupId;
		QueryCommandCacheKey commandCacheKey = new QueryCommandCacheKey(query, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyToken), topNumberOfRowsToReturn, skipNumberOfRows, countRows, queryFiltersOnFlowFilters, queryAndPositioningFilter, filterOnlyColumnFilters, globalAndSecurityFiltersDictionary, sortingFields, addExplicitJoinHints, groupId);
		NavSqlQueryCommand queryCommand = null;
		if (conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () =>
		{
			queryCommand = Create(conScope.Connection, commandCacheKey);
			queryCommand.EnsureCommandIsCreatedAndPrepared(query, queryAndPositioningFilter, filterOnlyColumnFilters, queryFiltersOnFlowFilters, globalAndSecurityFiltersDictionary, addExplicitJoinHints, database, companyToken, sortingFields);
			return queryCommand;
		}, (NavSqlCommand command) =>
		{
			if (queryCommand == null)
			{
				queryCommand = (NavSqlQueryCommand)command;
				queryCommand.EnsureCommandIsCreatedAndPrepared(query, queryAndPositioningFilter, filterOnlyColumnFilters, queryFiltersOnFlowFilters, globalAndSecurityFiltersDictionary, addExplicitJoinHints, database, companyToken, sortingFields);
			}
			queryCommand.SetCommandTimeout(timeout);
			queryCommand.ExecuteReader();
			return (!queryCommand.ReadNextRow()) ? 85132272 : 0;
		}) == 85132272)
		{
			return null;
		}
		return queryCommand;
	}

	private static void CheckDisallowedSecurityFiltering(Dictionary<int, FilterFieldDictionary> globalAndSecurityFiltersDictionary, SecurityFiltering securityFiltering)
	{
		if (securityFiltering == SecurityFiltering.Disallowed && globalAndSecurityFiltersDictionary != null && globalAndSecurityFiltersDictionary.Count > 0)
		{
			KeyValuePair<int, FilterFieldDictionary> keyValuePair = globalAndSecurityFiltersDictionary.First();
			string tableNameByTableId = NavGlobal.NCLMetadata.GetTableNameByTableId(keyValuePair.Key);
			throw new NavPermissionException(string.Format(CultureInfo.CurrentCulture, Lang.SecurityFilterNotAllowed, tableNameByTableId));
		}
	}

	internal static IEnumerable<ReadOnlyRecordBuffer> Query(NCLMetaQueryDefinition query, NavDatabase database, int companyToken, int topNumberOfRowsToReturn, int skipNumberOfRows, FiltersAndMarks queryFilters, TableFilterDictionary globalAndSecurityFilters, bool includeCurrent, FindType findType, MutableRecordBuffer startingPosition, SecurityFiltering securityFiltering, bool addExplicitJoinHints, int? timeout = null, SortingFieldList sortingFields = null)
	{
		NavSqlQueryCommand command = null;
		int savedReaderToken = 0;
		ReadOnlyRecordBuffer recordBuffer = null;
		bool optimizedTopRows = false;
		bool topNumberOfRowsSpecified = topNumberOfRowsToReturn > 0;
		switch (findType)
		{
		case FindType.FirstOnly:
			topNumberOfRowsToReturn = 1;
			topNumberOfRowsSpecified = true;
			break;
		case FindType.Normal:
			if (!topNumberOfRowsSpecified)
			{
				topNumberOfRowsToReturn = 50;
				optimizedTopRows = true;
			}
			break;
		}
		database.CheckTablesInSync(query.IncludedTables);
		bool lockingActivity = query.DataItems.Any((NCLMetaQueryDataItem q) => q.LockState != DataLockState.None);
		IReference<NavSqlConnection> lastUsedSqlConnection = null;
		do
		{
			IL_00cc:
			using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, lockingActivity, ref lastUsedSqlConnection))
			{
				if (command == null || command.IsDisposed || command.ReaderToken != savedReaderToken || command.State != NavSqlCommandState.ExecutedWithPendingResults || command.Connection != navSqlConnectionScope.Connection)
				{
					if (command != null)
					{
						if (command.IsDisposed || command.Connection == navSqlConnectionScope.Connection)
						{
							NavEnvironment.Instance.PerformanceCounterSetter.UpdatePerformanceCountersWithPreferredCacheCommandReuseResult(tryGetResult: false);
						}
						startingPosition = new MutableRecordBuffer(recordBuffer);
						includeCurrent = false;
					}
					UpdatePerformanceCountersWithQueryRepositionResult(command != null);
					command = QueryExecute(navSqlConnectionScope, query, database, companyToken, topNumberOfRowsToReturn, skipNumberOfRows, countRows: false, queryFilters, globalAndSecurityFilters, includeCurrent, startingPosition, securityFiltering, addExplicitJoinHints, sortingFields, timeout);
					if (command != null)
					{
						savedReaderToken = command.ReaderToken;
						goto IL_024a;
					}
					break;
				}
				NavEnvironment.Instance.PerformanceCounterSetter.UpdatePerformanceCountersWithPreferredCacheCommandReuseResult(tryGetResult: true);
				if (command.ReadNextRow())
				{
					goto IL_024a;
				}
				if (!((topNumberOfRowsToReturn == 0) & optimizedTopRows))
				{
					break;
				}
				optimizedTopRows = false;
				goto end_IL_00e4;
				IL_024a:
				recordBuffer = command.CreateRecordBufferFromReader();
				goto IL_0267;
				end_IL_00e4:;
			}
			goto IL_00cc;
			IL_0267:
			topNumberOfRowsToReturn--;
			yield return recordBuffer;
		}
		while (!topNumberOfRowsSpecified || topNumberOfRowsToReturn != 0);
	}

	internal static int QueryCount(NCLMetaQueryDefinition query, NavDatabase database, int companyToken, int topNumberOfRowsToReturn, int skipNumberOfRows, FiltersAndMarks queryFilters, TableFilterDictionary globalAndSecurityFilters, bool includeCurrent, FindType findType, MutableRecordBuffer startingPosition, SecurityFiltering securityFiltering, bool addExplicitJoinHints)
	{
		int result = 0;
		bool flag = topNumberOfRowsToReturn > 0;
		switch (findType)
		{
		case FindType.FirstOnly:
			topNumberOfRowsToReturn = 1;
			break;
		case FindType.Normal:
			if (!flag)
			{
				topNumberOfRowsToReturn = 50;
			}
			break;
		}
		database.CheckTablesInSync(query.IncludedTables);
		using (NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database))
		{
			NavSqlQueryCommand navSqlQueryCommand = QueryExecute(conScope, query, database, companyToken, topNumberOfRowsToReturn, skipNumberOfRows, countRows: true, queryFilters, globalAndSecurityFilters, includeCurrent, startingPosition, securityFiltering, addExplicitJoinHints, null);
			if (navSqlQueryCommand != null)
			{
				try
				{
					result = navSqlQueryCommand.Reader.GetInt32(0);
				}
				finally
				{
					navSqlQueryCommand.CloseReader(forceClose: false);
				}
			}
		}
		return result;
	}

	internal static IEnumerable<ReadOnlyRecordBuffer> QueryWithSingleScope(NCLMetaQueryDefinition query, NavDatabase database, int companyToken, int topNumberOfRowsToReturn, int skipNumberOfRows, FiltersAndMarks queryFilters, TableFilterDictionary globalAndSecurityFilters, bool includeCurrent, FindType findType, MutableRecordBuffer startingPosition, SecurityFiltering securityFiltering, bool addExplicitJoinHints)
	{
		bool optimizedTopRows = false;
		bool topNumberOfRowsSpecified = topNumberOfRowsToReturn > 0;
		switch (findType)
		{
		case FindType.FirstOnly:
			topNumberOfRowsToReturn = 1;
			topNumberOfRowsSpecified = true;
			break;
		case FindType.Normal:
			if (!topNumberOfRowsSpecified)
			{
				topNumberOfRowsToReturn = 50;
				optimizedTopRows = true;
			}
			break;
		}
		database.CheckTablesInSync(query.IncludedTables);
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database);
		NavSqlQueryCommand command = QueryExecute(conScope, query, database, companyToken, topNumberOfRowsToReturn, skipNumberOfRows, countRows: false, queryFilters, globalAndSecurityFilters, includeCurrent, startingPosition, securityFiltering, addExplicitJoinHints, null);
		if (command == null)
		{
			yield break;
		}
		try
		{
			UpdatePerformanceCountersWithQueryRepositionResult(repositionResult: true);
			while (true)
			{
				ReadOnlyRecordBuffer readOnlyRecordBuffer = command.CreateRecordBufferFromReader();
				topNumberOfRowsToReturn--;
				yield return readOnlyRecordBuffer;
				if (topNumberOfRowsSpecified && topNumberOfRowsToReturn == 0)
				{
					break;
				}
				if (!command.ReadNextRow())
				{
					if (!((topNumberOfRowsToReturn == 0) & optimizedTopRows))
					{
						break;
					}
					optimizedTopRows = false;
				}
			}
		}
		finally
		{
			command.CloseReader(forceClose: false);
		}
	}
}
