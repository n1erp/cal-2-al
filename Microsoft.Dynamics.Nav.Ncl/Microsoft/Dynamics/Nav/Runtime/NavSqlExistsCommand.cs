using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlExistsCommand : NavSqlCommand
{
	private class ExistsCommandCacheKey : NavSqlCollationSensitiveCommandCacheKey
	{
		private readonly int companyIndex;

		public NCLMetaTable Table { get; private set; }

		public FilterExpression Filter { get; private set; }

		public string Hints { get; private set; }

		public ExistsCommandCacheKey(NavDatabase database, NCLMetaTable table, int companyIndex, FilterExpression filter, string statementHints)
			: base(CachedCommandType.Exists, database, (table.TableId % 20000 << 8) | (companyIndex ^ ValueInsensitiveFilterExpressionEqualityComparer.Instance.GetHashCode(filter)))
		{
			Table = table;
			this.companyIndex = companyIndex;
			Filter = filter;
			Hints = statementHints;
		}

		public ExistsCommandCacheKey(NavDatabase database, NCLMetaTable table, int companyIndex, string statementHints)
			: base(CachedCommandType.Exists, database, (table.TableId % 20000 << 8) | companyIndex)
		{
			Table = table;
			this.companyIndex = companyIndex;
			Hints = statementHints;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ExistsCommandCacheKey existsCommandCacheKey))
			{
				return false;
			}
			if (!base.Equals(existsCommandCacheKey) || companyIndex != existsCommandCacheKey.companyIndex || !Table.Equals(existsCommandCacheKey.Table) || !string.Equals(Hints, existsCommandCacheKey.Hints))
			{
				return false;
			}
			if (Filter == null != (existsCommandCacheKey.Filter == null))
			{
				return false;
			}
			if (Filter != null && !ValueInsensitiveFilterExpressionEqualityComparer.Instance.Equals(Filter, existsCommandCacheKey.Filter))
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	private static string selectExistsStr = "SELECT TOP 1 NULL ";

	private ExistsCommandCacheKey cacheKey;

	public override bool Stale => cacheKey.Table.Stale;

	private NavSqlExistsCommand(NavSqlConnection parentConnection, ExistsCommandCacheKey cacheKey)
		: base(parentConnection)
	{
		this.cacheKey = cacheKey;
	}

	private static NavSqlExistsCommand Create(NavSqlConnection parentConnection, ExistsCommandCacheKey cacheKey)
	{
		return new NavSqlExistsCommand(parentConnection, cacheKey);
	}

	private int ExecuteExists(FilterExpression filter, NavDatabase database, int companyIndex, CompanyId? companyId, HashSet<NavAppOrigin> distinctCompanionAppsOrigin, bool isMultiField)
	{
		NavSqlParameterHandler navSqlParameterHandler = new NavSqlParameterHandler();
		if (State == NavSqlCommandState.Created)
		{
			StringBuilder stringBuilder = new StringBuilder(selectExistsStr);
			string defaultMainTableAlias = NavGlobal.SystemTenant.NavSqlTableAliasMap.GetDefaultMainTableAlias(cacheKey.Table.TableId);
			if (!isMultiField && !FeatureSwitches.Instance.DisableCompanionTableJoinOptimization)
			{
				NavSqlStatementHelper.AppendFromClauseWithCompanionTableJoinsIncluded(stringBuilder, cacheKey.Table, database, companyIndex, cacheKey.Hints, distinctCompanionAppsOrigin, qualifyTableWithDatabase: true, defaultMainTableAlias);
			}
			else
			{
				NavSqlStatementHelper.AppendFromClauseWithCompanionTableJoinsIncluded(stringBuilder, cacheKey.Table, database, companyIndex, cacheKey.Hints, cacheKey.Table.DistinctCompanionTableOrigins, qualifyTableWithDatabase: true, defaultMainTableAlias);
			}
			StringBuilder stringBuilder2 = new StringBuilder(" WHERE ");
			if (companyId.HasValue)
			{
				stringBuilder2.Append('"').Append(defaultMainTableAlias).Append("\".");
				stringBuilder2.Append(NavSqlStatementHelper.SqlStatementCompanyIdEqualsToParameterNo(navSqlParameterHandler.GetCompanyParameterNo(companyId.Value))).Append((filter == null) ? "" : " AND ");
			}
			if (filter != null)
			{
				stringBuilder2.Append('(');
				NavSqlWhereClauseVisitorInputOutput input = NavSqlWhereClauseVisitorInputOutput.CreateForTextualWhereClauseGeneration(stringBuilder2, navSqlParameterHandler, null, treatNullAsDefaultValue: false, string.Empty, cacheKey.SqlSortingProperties, shouldAliasQualifyFields: true);
				filter.Visit(NavSqlWhereClauseVisitor.DefaultVisitor, input);
				stringBuilder2.Append(") ");
			}
			if (stringBuilder2.Length > 7)
			{
				stringBuilder.Append(stringBuilder2);
			}
			NavSqlStatementHelper.AppendSelectStatementOptions(stringBuilder, addFastOption: false);
			SetCommandText(stringBuilder.ToString());
		}
		navSqlParameterHandler = new NavSqlParameterHandler();
		if (companyId.HasValue)
		{
			NavSqlStatementHelper.SetSqlParameter(NavSqlStatementHelper.GetOrCreateSqlParameterNo(Command.Parameters, navSqlParameterHandler.GetCompanyParameterNo(companyId.Value)), companyId.Value);
		}
		if (filter != null)
		{
			NavSqlWhereClauseVisitorInputOutput input2 = NavSqlWhereClauseVisitorInputOutput.CreateForParameterAssignment(Command.Parameters, navSqlParameterHandler, null, treatNullAsDefaultValue: false, cacheKey.SqlSortingProperties);
			filter.Visit(NavSqlWhereClauseVisitor.DefaultVisitor, input2);
		}
		PrepareStatement();
		using (ExecuteReader())
		{
			if (ReadNextRow())
			{
				CloseReader(forceClose: false);
				return 0;
			}
			return 85132272;
		}
	}

	private int ExecuteExistsRecord(FilterExpression filter, NavDatabase database, int companyIndex, CompanyId? companyId)
	{
		if (State == NavSqlCommandState.Created)
		{
			StringBuilder stringBuilder = new StringBuilder(selectExistsStr);
			stringBuilder.Append(" FROM ");
			NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, cacheKey.Table, database, companyIndex, NavAppTableContext.Main);
			stringBuilder.Append(cacheKey.Hints);
			NavSqlWhereClauseBuilder.AppendFilterClause(stringBuilder, filter, new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId);
			NavSqlStatementHelper.AppendSelectStatementOptions(stringBuilder, addFastOption: false);
			SetCommandText(stringBuilder.ToString());
		}
		NavSqlWhereClauseBuilder.AddOrSetFilterParameters(filter, Command.Parameters, new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId);
		PrepareStatement();
		using (ExecuteReader())
		{
			if (ReadNextRow())
			{
				CloseReader(forceClose: false);
				return 0;
			}
			return 85132272;
		}
	}

	internal static int RecordExists(NavDatabase database, NCLMetaTable table, int companyIndex, MutableRecordBuffer inputRecord, bool locking)
	{
		NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, locking);
		try
		{
			string statementHints = NavSqlStatementHelper.GenerateSelectStatementHints(locking, noExpand: false, conScope.Connection);
			FilterExpression existsFilter = NavSqlWhereClauseBuilder.CreateRecordClause(FilterExpressionType.Equal, inputRecord, table.PrimaryKey.SortingFieldsWithPrimaryKeyFields, useOriginalValues: false);
			ExistsCommandCacheKey commandCacheKey = new ExistsCommandCacheKey(database, table, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), statementHints);
			return conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) => ((NavSqlExistsCommand)command).ExecuteExistsRecord(existsFilter, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex)));
		}
		finally
		{
			if (conScope != null)
			{
				((IDisposable)conScope).Dispose();
			}
		}
	}

	internal static bool Exists(NavDatabase database, NCLMetaTable table, int companyIndex, FiltersAndMarks filtersAndMarks, DataLockState lockState)
	{
		int num = Exists(database, table, companyIndex, filtersAndMarks, lockState == DataLockState.LockExclusive);
		return num switch
		{
			0 => true, 
			85132272 => false, 
			_ => throw new NavCSideException(num), 
		};
	}

	internal static int Exists(NavDatabase database, NCLMetaTable table, int companyIndex, FiltersAndMarks filtersAndMarks, bool locking, bool withXLock = false)
	{
		database.CheckTableInSync(table);
		NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, locking);
		try
		{
			FilterExpression filter = filtersAndMarks.GetCombinedFilter();
			string statementHints = NavSqlStatementHelper.GenerateSelectStatementHints(locking, noExpand: false, conScope.Connection, withXLock);
			ExistsCommandCacheKey commandCacheKey = new ExistsCommandCacheKey(database, table, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), filter, statementHints);
			HashSet<NavAppOrigin> distinctOrigins = new HashSet<NavAppOrigin>();
			bool isMultiField = false;
			if (!FeatureSwitches.Instance.DisableCompanionTableJoinOptimization)
			{
				isMultiField = filtersAndMarks?.IsMultiField() ?? false;
				if (!isMultiField)
				{
					filtersAndMarks?.Filters?.GetCompanionAppsOriginForTheUsedFilters().ForEach((NavAppOrigin o) =>
					{
						distinctOrigins.Add(o);
					});
				}
			}
			return conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) => ((NavSqlExistsCommand)command).ExecuteExists(filter, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex), distinctOrigins, isMultiField));
		}
		finally
		{
			if (conScope != null)
			{
				((IDisposable)conScope).Dispose();
			}
		}
	}

	internal static int Exists(NavDatabase database, NCLMetaTable table, int companyIndex, FilterExpression filter, bool locking, HashSet<NavAppOrigin> distinctCompanionAppsOrigin, bool withXLock = false)
	{
		database.CheckTableInSync(table);
		NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, locking);
		try
		{
			string statementHints = NavSqlStatementHelper.GenerateSelectStatementHints(locking, noExpand: false, conScope.Connection, withXLock);
			ExistsCommandCacheKey commandCacheKey = new ExistsCommandCacheKey(database, table, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), filter, statementHints);
			return conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) => ((NavSqlExistsCommand)command).ExecuteExists(filter, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex), distinctCompanionAppsOrigin, isMultiField: false));
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
