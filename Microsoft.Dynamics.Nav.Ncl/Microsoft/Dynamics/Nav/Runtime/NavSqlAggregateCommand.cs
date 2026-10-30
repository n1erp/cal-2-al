using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlAggregateCommand : NavSqlCommand
{
	internal class AggregateCommandCacheKey : NavSqlCollationSensitiveCommandCacheKey
	{
		private readonly int companyIndex;

		private FiltersAndMarks filter;

		private string hints;

		public NCLMetaCalculationMethod AggregateMethod { get; private set; }

		public NCLMetaTable Table { get; private set; }

		public FieldList AggregateFields { get; private set; }

		public bool Locking { get; private set; }

		public bool FixedAggregationMethod { get; private set; }

		public AggregateCommandCacheKey(NavDatabase database, NCLMetaTable table, int companyIndex, FiltersAndMarks filter, bool fixedAggregationMethod, NCLMetaCalculationMethod aggregateMethod, FieldList aggregateFields, bool locking, string hints)
			: base(CachedCommandType.Aggregate, database, (table.TableId % 20000 << 8) | aggregateFields.Aggregate(0, (int acc, INavFieldMetadata field) => HashCodeHelper.CombineHashCodes(acc, field.GetHashCode())) | (companyIndex ^ filter.GetValueInsensitiveHashCode()))
		{
			if (aggregateFields == null)
			{
				throw new ArgumentNullException("aggregateFields");
			}
			AggregateMethod = aggregateMethod;
			Table = table;
			this.companyIndex = companyIndex;
			AggregateFields = aggregateFields;
			Locking = locking;
			this.filter = filter;
			this.hints = hints;
			FixedAggregationMethod = fixedAggregationMethod;
		}

		public AggregateCommandCacheKey(NavDatabase database, NCLMetaTable table, int companyIndex, FiltersAndMarks filter, bool fixedAggregationMethod, NCLMetaCalculationMethod aggregateMethod, bool locking, string hints)
			: base(CachedCommandType.Aggregate, database, (table.TableId % 20000 << 8) | (companyIndex ^ filter.GetValueInsensitiveHashCode()))
		{
			AggregateMethod = aggregateMethod;
			Table = table;
			this.companyIndex = companyIndex;
			Locking = locking;
			this.filter = filter;
			this.hints = hints;
			FixedAggregationMethod = fixedAggregationMethod;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is AggregateCommandCacheKey aggregateCommandCacheKey))
			{
				return false;
			}
			if (!base.Equals(aggregateCommandCacheKey) || AggregateMethod != aggregateCommandCacheKey.AggregateMethod || companyIndex != aggregateCommandCacheKey.companyIndex || FixedAggregationMethod != aggregateCommandCacheKey.FixedAggregationMethod || !Table.Equals(aggregateCommandCacheKey.Table) || Locking != aggregateCommandCacheKey.Locking || !filter.ValueInsensitiveEquals(aggregateCommandCacheKey.filter) || !string.Equals(hints, aggregateCommandCacheKey.hints))
			{
				return false;
			}
			if (AggregateFields == null != (aggregateCommandCacheKey.AggregateFields == null))
			{
				return false;
			}
			if (AggregateFields != null)
			{
				if (AggregateFields.Count != aggregateCommandCacheKey.AggregateFields.Count)
				{
					return false;
				}
				for (int i = 0; i < AggregateFields.Count; i++)
				{
					if (!AggregateFields[i].Equals(aggregateCommandCacheKey.AggregateFields[i]))
					{
						return false;
					}
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	private const string SelectStr = "SELECT ";

	private const string FromStr = " FROM ";

	private NCLMetaKey sumKey;

	private readonly AggregateCommandCacheKey cacheKey;

	public override bool Stale => cacheKey.Table.Stale;

	private NavSqlAggregateCommand(NavSqlConnection parentConnection, AggregateCommandCacheKey cacheKey)
		: base(parentConnection)
	{
		this.cacheKey = cacheKey;
	}

	private static NavSqlAggregateCommand Create(NavSqlConnection parentConnection, AggregateCommandCacheKey cacheKey)
	{
		return new NavSqlAggregateCommand(parentConnection, cacheKey);
	}

	private void CalculateBestSumIndex(FiltersAndMarks filter)
	{
		FieldList aggregatedFields = cacheKey.AggregateFields;
		if (cacheKey.FixedAggregationMethod)
		{
			if (cacheKey.AggregateMethod == NCLMetaCalculationMethod.Min || cacheKey.AggregateMethod == NCLMetaCalculationMethod.Max)
			{
				return;
			}
		}
		else
		{
			if (cacheKey.AggregateFields.AsNclMetaFields().Any((NCLMetaField field) => field.CalculationFormula.CalculationMethod == NCLMetaCalculationMethod.Min || field.CalculationFormula.CalculationMethod == NCLMetaCalculationMethod.Max))
			{
				return;
			}
			List<NCLMetaField> list = new List<NCLMetaField>();
			foreach (NCLMetaField aggregateField in cacheKey.AggregateFields)
			{
				NCLMetaCalculationMethod calculationMethod = aggregateField.CalculationFormula.CalculationMethod;
				if (calculationMethod != NCLMetaCalculationMethod.Count)
				{
					list.Add(aggregateField.CalculationFormula.SourceField);
				}
			}
			INavFieldMetadata[] fields = list.ToArray();
			aggregatedFields = new FieldList(fields);
		}
		FilterFieldDictionary filterFieldDictionary = filter.Filters;
		MarkedRecords markedRecords = filter.MarkedRecords;
		if (markedRecords != null)
		{
			if (markedRecords.Kind == MarkedRecords.MarkedRecordsKind.SimpleListOfMarks || markedRecords.Kind == MarkedRecords.MarkedRecordsKind.SimpleListOfExclusions)
			{
				filterFieldDictionary = DataProviderHelper.MergeFilterFieldDictionaries(filterFieldDictionary, new FilterFieldDictionary(cacheKey.Table.PrimaryKey.KeyFieldsList.Select((NCLMetaField field) => new Tuple<INavFieldMetadata, FilterExpression>(field, null))));
			}
			if (markedRecords.Kind == MarkedRecords.MarkedRecordsKind.SimpleListOfExclusions || markedRecords.Kind == MarkedRecords.MarkedRecordsKind.SimpleListOfMarks)
			{
				filterFieldDictionary = DataProviderHelper.MergeFilterFieldDictionaries(filterFieldDictionary, markedRecords.AllMarksFilter);
			}
		}
		sumKey = FlowFieldsHelper.CalculateBestSIFTIndex(cacheKey.Table, aggregatedFields, null, filterFieldDictionary);
	}

	private static NavSqlColumnListType GetAggregateColumnListType(AggregateCommandCacheKey cacheKey, NCLMetaKey sumKey)
	{
		if (cacheKey.FixedAggregationMethod)
		{
			if (sumKey != null)
			{
				return cacheKey.AggregateMethod switch
				{
					NCLMetaCalculationMethod.Sum => NavSqlColumnListType.SiftSum, 
					NCLMetaCalculationMethod.Average => NavSqlColumnListType.SiftAverage, 
					_ => throw new NotSupportedException(), 
				};
			}
			return cacheKey.AggregateMethod switch
			{
				NCLMetaCalculationMethod.Average => NavSqlColumnListType.Average, 
				NCLMetaCalculationMethod.Max => NavSqlColumnListType.Max, 
				NCLMetaCalculationMethod.Min => NavSqlColumnListType.Min, 
				NCLMetaCalculationMethod.Sum => NavSqlColumnListType.Sum, 
				_ => throw new NotSupportedException(), 
			};
		}
		return (sumKey == null) ? NavSqlColumnListType.BaseTableAggregation : NavSqlColumnListType.SiftAggregation;
	}

	internal static string BuildAggregateStatement(AggregateCommandCacheKey cacheKey, string hints, FiltersAndMarks filter, NCLMetaKey sumKey, NavDatabase database, int companyIndex, CompanyId? companyId)
	{
		StringBuilder stringBuilder = new StringBuilder("SELECT ");
		NavSqlColumnListType aggregateColumnListType = GetAggregateColumnListType(cacheKey, sumKey);
		string text = null;
		bool shouldAliasQualifyFields = false;
		IEnumerable<NCLMetaField> enumerable = cacheKey.AggregateFields.AsNclMetaFields();
		if (sumKey == null)
		{
			text = NavGlobal.SystemTenant.NavSqlTableAliasMap.GetDefaultMainTableAlias(cacheKey.Table.TableId);
			NavSqlStatementHelper.AppendFieldList(stringBuilder, enumerable, aggregateColumnListType, text);
			if (!filter.IsMultiField())
			{
				HashSet<NavAppOrigin> distinctCompanionAppsUsedByFiltersOrAggregateFieldsList = GetDistinctCompanionAppsUsedByFiltersOrAggregateFieldsList(filter, enumerable);
				NavSqlStatementHelper.AppendFromClauseWithCompanionTableJoinsIncluded(stringBuilder, cacheKey.Table, database, companyIndex, hints, distinctCompanionAppsUsedByFiltersOrAggregateFieldsList, qualifyTableWithDatabase: true, text);
				shouldAliasQualifyFields = distinctCompanionAppsUsedByFiltersOrAggregateFieldsList != null && distinctCompanionAppsUsedByFiltersOrAggregateFieldsList.Count() > 0;
			}
			else
			{
				NavSqlStatementHelper.AppendFromClauseWithCompanionTableJoinsIncluded(stringBuilder, cacheKey.Table, database, companyIndex, hints, cacheKey.Table.DistinctCompanionTableOrigins, qualifyTableWithDatabase: true, text);
				shouldAliasQualifyFields = true;
			}
		}
		else
		{
			NavSqlStatementHelper.AppendFieldList(stringBuilder, enumerable, aggregateColumnListType, null);
			stringBuilder.Append(" FROM ");
			NavSqlStatementHelper.AppendQuotedSiftIndexNameWithDatabaseAndSchemaQualification(stringBuilder, sumKey, database, companyIndex);
			stringBuilder.Append(hints);
		}
		NavSqlWhereClauseBuilder.AppendFilterClause(stringBuilder, filter.GetCombinedFilter(), new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId, shouldAliasQualifyFields, text);
		NavSqlStatementHelper.AppendSelectStatementOptions(stringBuilder, addFastOption: false);
		return stringBuilder.ToString();
	}

	private static HashSet<NavAppOrigin> GetDistinctCompanionAppsUsedByFiltersOrAggregateFieldsList(FiltersAndMarks filter, IEnumerable<NCLMetaField> aggregatedFieldsList)
	{
		HashSet<NavAppOrigin> hashSet = filter?.Filters?.GetCompanionAppsOriginForTheUsedFilters();
		if (hashSet == null)
		{
			hashSet = new HashSet<NavAppOrigin>();
		}
		HashSet<NavAppOrigin> aggregatedFieldsDistinctCompanionAppsOrigins = new HashSet<NavAppOrigin>();
		ElementExtension.ForEach(from f in aggregatedFieldsList
			where f.SourceTableField.IsCompanionTableField
			select f.SourceTableField?.AppOrigin, (NavAppOrigin o) =>
		{
			aggregatedFieldsDistinctCompanionAppsOrigins.Add(o);
		});
		hashSet.UnionWith(aggregatedFieldsDistinctCompanionAppsOrigins);
		return hashSet;
	}

	private FieldDictionary<NavValue> ExecuteAggregateWithoutOptimization(FiltersAndMarks filter, NavDatabase database, int companyIndex, CompanyId? companyId)
	{
		if (State == NavSqlCommandState.Created)
		{
			StringBuilder stringBuilder = new StringBuilder("SELECT ");
			CalculateBestSumIndex(filter);
			NavSqlColumnListType aggregateColumnListType = GetAggregateColumnListType(cacheKey, sumKey);
			string text = NavSqlStatementHelper.GenerateSelectStatementHints(cacheKey.Locking, sumKey != null, Connection);
			string text2 = null;
			if (sumKey == null)
			{
				text2 = NavGlobal.SystemTenant.NavSqlTableAliasMap.GetDefaultMainTableAlias(cacheKey.Table.TableId);
				NavSqlStatementHelper.AppendFieldList(stringBuilder, cacheKey.AggregateFields.AsNclMetaFields(), aggregateColumnListType, text2);
				NavSqlStatementHelper.AppendFromClauseWithCompanionTableJoinsIncluded(stringBuilder, cacheKey.Table, database, companyIndex, text, cacheKey.Table.DistinctCompanionTableOrigins, qualifyTableWithDatabase: true, text2);
			}
			else
			{
				NavSqlStatementHelper.AppendFieldList(stringBuilder, cacheKey.AggregateFields.AsNclMetaFields(), aggregateColumnListType, null);
				stringBuilder.Append(" FROM ");
				NavSqlStatementHelper.AppendQuotedSiftIndexNameWithDatabaseAndSchemaQualification(stringBuilder, sumKey, database, companyIndex);
				stringBuilder.Append(text);
			}
			bool shouldAliasQualifyFields = sumKey == null;
			NavSqlWhereClauseBuilder.AppendFilterClause(stringBuilder, filter.GetCombinedFilter(), new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId, shouldAliasQualifyFields, text2);
			NavSqlStatementHelper.AppendSelectStatementOptions(stringBuilder, addFastOption: false);
			SetCommandText(stringBuilder.ToString());
		}
		NavSqlWhereClauseBuilder.AddOrSetFilterParameters(filter.GetCombinedFilter(), Command.Parameters, new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId);
		PrepareStatement();
		using (ExecuteReader())
		{
			if (!ReadNextRow())
			{
				throw new NavCSideException("The generated SQL statement should always return a resultset even in the case of no records.");
			}
			Tuple<INavFieldMetadata, NavValue>[] array = new Tuple<INavFieldMetadata, NavValue>[cacheKey.AggregateFields.Count];
			for (int i = 0; i < cacheKey.AggregateFields.Count; i++)
			{
				NCLMetaField nCLMetaField = (NCLMetaField)cacheKey.AggregateFields[i];
				NavValue navValue;
				if (cacheKey.FixedAggregationMethod)
				{
					navValue = CreateNavValueFromReader(nCLMetaField, i);
				}
				else
				{
					if (nCLMetaField.CalculationFormula.CalculationMethod != NCLMetaCalculationMethod.Count)
					{
						navValue = CreateNavValueFromReader(nCLMetaField.CalculationFormula.SourceField, i);
					}
					else if (Reader.IsDBNull(i))
					{
						navValue = NavInteger.Default;
					}
					else
					{
						navValue = ((sumKey != null) ? NavInteger.Create((int)Reader.GetInt64(i)) : NavInteger.Create(Reader.GetInt32(i)));
					}
					if (nCLMetaField.CalculationFormula.NegateResult)
					{
						navValue = FlowFieldsHelper.NegateValue(navValue, nCLMetaField.CalculationFormula.SourceField.FieldNavType);
					}
				}
				array[i] = new Tuple<INavFieldMetadata, NavValue>(nCLMetaField, navValue);
			}
			CloseReader(forceClose: false);
			return new FieldDictionary<NavValue>(array);
		}
	}

	private FieldDictionary<NavValue> ExecuteAggregate(FiltersAndMarks filter, NavDatabase database, int companyIndex, CompanyId? companyId)
	{
		if (State == NavSqlCommandState.Created)
		{
			CalculateBestSumIndex(filter);
			string hints = NavSqlStatementHelper.GenerateSelectStatementHints(cacheKey.Locking, sumKey != null, Connection);
			string commandText = BuildAggregateStatement(cacheKey, hints, filter, sumKey, database, companyIndex, companyId);
			SetCommandText(commandText);
		}
		NavSqlWhereClauseBuilder.AddOrSetFilterParameters(filter.GetCombinedFilter(), Command.Parameters, new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId);
		PrepareStatement();
		using (ExecuteReader())
		{
			if (!ReadNextRow())
			{
				throw new NavCSideException("The generated SQL statement should always return a resultset even in the case of no records.");
			}
			Tuple<INavFieldMetadata, NavValue>[] array = new Tuple<INavFieldMetadata, NavValue>[cacheKey.AggregateFields.Count];
			for (int i = 0; i < cacheKey.AggregateFields.Count; i++)
			{
				NCLMetaField nCLMetaField = (NCLMetaField)cacheKey.AggregateFields[i];
				NavValue navValue;
				if (cacheKey.FixedAggregationMethod)
				{
					navValue = CreateNavValueFromReader(nCLMetaField, i);
				}
				else
				{
					if (nCLMetaField.CalculationFormula.CalculationMethod != NCLMetaCalculationMethod.Count)
					{
						navValue = CreateNavValueFromReader(nCLMetaField.CalculationFormula.SourceField, i);
					}
					else if (Reader.IsDBNull(i))
					{
						navValue = NavInteger.Default;
					}
					else
					{
						navValue = ((sumKey != null) ? NavInteger.Create((int)Reader.GetInt64(i)) : NavInteger.Create(Reader.GetInt32(i)));
					}
					if (nCLMetaField.CalculationFormula.NegateResult)
					{
						navValue = FlowFieldsHelper.NegateValue(navValue, nCLMetaField.CalculationFormula.SourceField.FieldNavType);
					}
				}
				array[i] = new Tuple<INavFieldMetadata, NavValue>(nCLMetaField, navValue);
			}
			CloseReader(forceClose: false);
			return new FieldDictionary<NavValue>(array);
		}
	}

	internal static string BuildCountStatement(AggregateCommandCacheKey cacheKey, string hints, FiltersAndMarks filter, NCLMetaKey sumKey, NavDatabase database, int companyIndex, CompanyId? companyId)
	{
		StringBuilder stringBuilder = new StringBuilder("SELECT ");
		NavSqlColumnListType columnListType = ((sumKey == null) ? NavSqlColumnListType.Count : NavSqlColumnListType.SiftCount);
		string text = null;
		NavSqlStatementHelper.AppendFieldList(stringBuilder, null, columnListType, text);
		bool shouldAliasQualifyFields = false;
		if (sumKey == null)
		{
			text = NavGlobal.SystemTenant.NavSqlTableAliasMap.GetDefaultMainTableAlias(cacheKey.Table.TableId);
			if (!filter.IsMultiField())
			{
				HashSet<NavAppOrigin> hashSet = filter?.Filters?.GetCompanionAppsOriginForTheUsedFilters();
				NavSqlStatementHelper.AppendFromClauseWithCompanionTableJoinsIncluded(stringBuilder, cacheKey.Table, database, companyIndex, hints, hashSet, qualifyTableWithDatabase: true, text);
				shouldAliasQualifyFields = hashSet != null && hashSet.Count() > 0;
			}
			else
			{
				NavSqlStatementHelper.AppendFromClauseWithCompanionTableJoinsIncluded(stringBuilder, cacheKey.Table, database, companyIndex, hints, cacheKey.Table.DistinctCompanionTableOrigins, qualifyTableWithDatabase: true, text);
				shouldAliasQualifyFields = true;
			}
		}
		else
		{
			stringBuilder.Append(" FROM ");
			NavSqlStatementHelper.AppendQuotedSiftIndexNameWithDatabaseAndSchemaQualification(stringBuilder, sumKey, database, companyIndex);
			stringBuilder.Append(hints);
		}
		NavSqlWhereClauseBuilder.AppendFilterClause(stringBuilder, filter.GetCombinedFilter(), new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId, shouldAliasQualifyFields, text);
		NavSqlStatementHelper.AppendSelectStatementOptions(stringBuilder, addFastOption: false);
		return stringBuilder.ToString();
	}

	private int ExecuteCount(FiltersAndMarks filter, NavDatabase database, int companyIndex, CompanyId? companyId)
	{
		if (State == NavSqlCommandState.Created)
		{
			CalculateBestSumIndex(filter);
			string hints = NavSqlStatementHelper.GenerateSelectStatementHints(cacheKey.Locking, sumKey != null, Connection);
			string commandText = BuildCountStatement(cacheKey, hints, filter, sumKey, database, companyIndex, companyId);
			SetCommandText(commandText);
		}
		NavSqlWhereClauseBuilder.AddOrSetFilterParameters(filter.GetCombinedFilter(), Command.Parameters, new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId);
		PrepareStatement();
		using (ExecuteReader())
		{
			if (!ReadNextRow())
			{
				throw new NavCSideException("The generated SQL statement should always return a resultset even in the case of no records.");
			}
			int result = 0;
			if (!Reader.IsDBNull(0))
			{
				result = (int)((sumKey != null) ? Reader.GetInt64(0) : Reader.GetInt32(0));
			}
			CloseReader(forceClose: false);
			return result;
		}
	}

	private int ExecuteCountWithoutOptimization(FiltersAndMarks filter, NavDatabase database, int companyIndex, CompanyId? companyId)
	{
		if (State == NavSqlCommandState.Created)
		{
			StringBuilder stringBuilder = new StringBuilder("SELECT ");
			CalculateBestSumIndex(filter);
			NavSqlColumnListType columnListType = ((sumKey == null) ? NavSqlColumnListType.Count : NavSqlColumnListType.SiftCount);
			string text = null;
			string text2 = NavSqlStatementHelper.GenerateSelectStatementHints(cacheKey.Locking, sumKey != null, Connection);
			NavSqlStatementHelper.AppendFieldList(stringBuilder, null, columnListType, text);
			if (sumKey == null)
			{
				text = NavGlobal.SystemTenant.NavSqlTableAliasMap.GetDefaultMainTableAlias(cacheKey.Table.TableId);
				NavSqlStatementHelper.AppendFromClauseWithCompanionTableJoinsIncluded(stringBuilder, cacheKey.Table, database, companyIndex, text2, cacheKey.Table.DistinctCompanionTableOrigins, qualifyTableWithDatabase: true, text);
			}
			else
			{
				stringBuilder.Append(" FROM ");
				NavSqlStatementHelper.AppendQuotedSiftIndexNameWithDatabaseAndSchemaQualification(stringBuilder, sumKey, database, companyIndex);
				stringBuilder.Append(text2);
			}
			bool shouldAliasQualifyFields = sumKey == null;
			NavSqlWhereClauseBuilder.AppendFilterClause(stringBuilder, filter.GetCombinedFilter(), new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId, shouldAliasQualifyFields, text);
			NavSqlStatementHelper.AppendSelectStatementOptions(stringBuilder, addFastOption: false);
			SetCommandText(stringBuilder.ToString());
		}
		NavSqlWhereClauseBuilder.AddOrSetFilterParameters(filter.GetCombinedFilter(), Command.Parameters, new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId);
		PrepareStatement();
		using (ExecuteReader())
		{
			if (!ReadNextRow())
			{
				throw new NavCSideException("The generated SQL statement should always return a resultset even in the case of no records.");
			}
			int result = 0;
			if (!Reader.IsDBNull(0))
			{
				result = (int)((sumKey != null) ? Reader.GetInt64(0) : Reader.GetInt32(0));
			}
			CloseReader(forceClose: false);
			return result;
		}
	}

	private static FieldDictionary<NavValue> AggregateInternal(NavDatabase database, NCLMetaTable table, int companyIndex, FiltersAndMarks filter, bool nonFlowFields, NCLMetaCalculationMethod aggregationMethod, FieldList aggregateFields, bool locking)
	{
		database.CheckTableInSync(table);
		NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, locking);
		try
		{
			string hints = NavSqlStatementHelper.GenerateSelectStatementHints(locking, noExpand: false, conScope.Connection);
			AggregateCommandCacheKey commandCacheKey = new AggregateCommandCacheKey(database, table, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), filter, nonFlowFields, aggregationMethod, aggregateFields, locking, hints);
			if (FeatureSwitches.Instance.DisableCompanionTableJoinOptimization)
			{
				return conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) => ((NavSqlAggregateCommand)command).ExecuteAggregateWithoutOptimization(filter, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex)));
			}
			return conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) => ((NavSqlAggregateCommand)command).ExecuteAggregate(filter, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex)));
		}
		finally
		{
			if (conScope != null)
			{
				((IDisposable)conScope).Dispose();
			}
		}
	}

	internal static FieldDictionary<NavValue> AggregateFlowFields(NavDatabase database, NCLMetaTable sourceTable, int companyIndex, FiltersAndMarks filter, FieldList flowFields, bool locking)
	{
		return AggregateInternal(database, sourceTable, companyIndex, filter, nonFlowFields: false, NCLMetaCalculationMethod.Sum, flowFields, locking);
	}

	internal static FieldDictionary<NavValue> AggregateNonFlowFields(NavDatabase database, NCLMetaTable table, int companyIndex, FiltersAndMarks filtersAndMarks, DataLockState lockState, NCLMetaCalculationMethod aggregationMethod, FieldList fieldsToCalculate)
	{
		return AggregateInternal(database, table, companyIndex, filtersAndMarks, nonFlowFields: true, aggregationMethod, fieldsToCalculate, lockState == DataLockState.LockExclusive);
	}

	internal static int AggregateCount(NavDatabase database, NCLMetaTable table, int companyIndex, FiltersAndMarks filter, DataLockState lockState)
	{
		database.CheckTableInSync(table);
		bool flag = lockState == DataLockState.LockExclusive;
		NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, flag);
		try
		{
			string hints = NavSqlStatementHelper.GenerateSelectStatementHints(flag, noExpand: false, conScope.Connection);
			AggregateCommandCacheKey commandCacheKey = new AggregateCommandCacheKey(database, table, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), filter, fixedAggregationMethod: true, NCLMetaCalculationMethod.Count, flag, hints);
			if (FeatureSwitches.Instance.DisableCompanionTableJoinOptimization)
			{
				return conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) => ((NavSqlAggregateCommand)command).ExecuteCountWithoutOptimization(filter, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex)));
			}
			return conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) => ((NavSqlAggregateCommand)command).ExecuteCount(filter, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex)));
		}
		finally
		{
			if (conScope != null)
			{
				((IDisposable)conScope).Dispose();
			}
		}
	}
}
