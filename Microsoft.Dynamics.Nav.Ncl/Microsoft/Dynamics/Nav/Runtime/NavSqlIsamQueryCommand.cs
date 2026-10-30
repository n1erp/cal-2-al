using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlIsamQueryCommand : NavSqlCommand
{
	private class IsamQueryCommandCacheKey : NavSqlCollationSensitiveCommandCacheKey
	{
		private readonly int companyIndex;

		public int FindMethod { get; private set; }

		public NCLMetaTable Table { get; private set; }

		public SortingFieldList OrderByFields { get; private set; }

		public FilterExpression Filter { get; private set; }

		public string Hints { get; private set; }

		public bool TopRows { get; private set; }

		public IsamQueryCommandCacheKey(int findMethod, NCLMetaTable table, NavDatabase database, int companyIndex, SortingFieldList orderByFields, FilterExpression filter, string hints, bool topRows)
			: base(CachedCommandType.IsamQuery, database, ComputeHashCode(table, findMethod, companyIndex, orderByFields, filter))
		{
			FindMethod = findMethod;
			Table = table;
			this.companyIndex = companyIndex;
			OrderByFields = orderByFields;
			Filter = filter;
			Hints = hints;
			TopRows = topRows;
		}

		private static int ComputeHashCode(NCLMetaTable table, int findMethod, int companyIndex, SortingFieldList orderByFields, FilterExpression filter)
		{
			return HashCodeHelper.CombineHashCodes((table.TableId % 20000 << 8) | (findMethod << 3) | companyIndex, orderByFields?.GetHashCode() ?? 0, ValueInsensitiveFilterExpressionEqualityComparer.Instance.GetHashCode(filter));
		}

		public override bool Equals(object obj)
		{
			if (!(obj is IsamQueryCommandCacheKey isamQueryCommandCacheKey))
			{
				return false;
			}
			if (!base.Equals(isamQueryCommandCacheKey) || FindMethod != isamQueryCommandCacheKey.FindMethod || companyIndex != isamQueryCommandCacheKey.companyIndex || !Table.Equals(isamQueryCommandCacheKey.Table) || !ValueInsensitiveFilterExpressionEqualityComparer.Instance.Equals(Filter, isamQueryCommandCacheKey.Filter) || !string.Equals(Hints, isamQueryCommandCacheKey.Hints) || TopRows != isamQueryCommandCacheKey.TopRows)
			{
				return false;
			}
			if (OrderByFields != null)
			{
				return OrderByFields.Equals(isamQueryCommandCacheKey.OrderByFields);
			}
			return isamQueryCommandCacheKey.OrderByFields == null;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	public const byte FindMethodEqual = 61;

	public const byte FindMethodGet = 33;

	public const byte FindMethodFirst = 45;

	public const byte FindMethodLast = 43;

	public const byte FindMethodFirstRelative = 46;

	public const byte FindMethodLastRelative = 47;

	public const byte FindMethodNext = 62;

	public const byte FindMethodPrior = 60;

	public const byte FindMethodFirstOnly = 91;

	public const byte FindMethodLastOnly = 93;

	public const byte FindMethodReadSet = 36;

	public const byte FindMethodReadSetUpdateKey = 38;

	public const byte FindMethodReadSetForUpdate = 35;

	public const byte FindMethodWithExistsCheck = 128;

	private const string SelectStr = "SELECT ";

	private const string TopStr = " TOP ({0}) ";

	private const string FromStr = " FROM ";

	private const string IfExistsStr = "IF EXISTS(SELECT TOP 1 NULL";

	private const int NumberOfConsecutiveExecutionsWithNoResultBeforeAddingExists = 3;

	private const int NumberOfConsecutiveResetWithResultsBeforeAdjustingTop = 3;

	internal const int DefaultFindNormalTopSize = 50;

	private const int MaximumNumberOfRowsToFetchInsteadOfCancellingCommandWithPendingResults = 10;

	private int consecutiveZeroRowsReturned;

	private int consecutiveCommandResetRowsFetchedWithinThreshold;

	private int minimumNumberOfRowsReadBeforeResetting;

	private int maximumNumberOfRowsReadBeforeResetting;

	private readonly IsamQueryCommandCacheKey cacheKey;

	private int topRowsAppliedAtExecution;

	public override bool Stale => cacheKey.Table.Stale;

	private NavSqlIsamQueryCommand(NavSqlConnection parentConnection, IsamQueryCommandCacheKey cacheKey)
		: base(parentConnection)
	{
		this.cacheKey = cacheKey;
	}

	private static NavSqlIsamQueryCommand Create(NavSqlConnection parentConnection, IsamQueryCommandCacheKey cacheKey)
	{
		return new NavSqlIsamQueryCommand(parentConnection, cacheKey);
	}

	private ReadOnlyRecordBuffer ExecuteFindEqualCommand(FilterExpression filter, NavDatabase database, int companyIndex, CompanyId? companyId)
	{
		if (State == NavSqlCommandState.Created)
		{
			StringBuilder stringBuilder = new StringBuilder("SELECT ");
			string defaultMainTableAlias = NavGlobal.SystemTenant.NavSqlTableAliasMap.GetDefaultMainTableAlias(cacheKey.Table.TableId);
			NavSqlStatementHelper.AppendFieldList(stringBuilder, cacheKey.Table.SqlReadableFields, NavSqlColumnListType.DataLengthInsteadOfBlobs, defaultMainTableAlias);
			NavSqlStatementHelper.AppendFromClauseWithCompanionTableJoinsIncluded(stringBuilder, cacheKey.Table, database, companyIndex, cacheKey.Hints, cacheKey.Table.DistinctCompanionTableOrigins, qualifyTableWithDatabase: true, defaultMainTableAlias);
			NavSqlWhereClauseBuilder.AppendFilterClause(stringBuilder, filter, new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId, shouldAliasQualifyFields: true, defaultMainTableAlias);
			NavSqlStatementHelper.AppendSelectStatementOptions(stringBuilder, addFastOption: false);
			SetCommandText(stringBuilder.ToString());
		}
		NavSqlWhereClauseBuilder.AddOrSetFilterParameters(filter, Command.Parameters, new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId);
		PrepareStatement();
		using (ExecuteReader())
		{
			if (ReadNextRow())
			{
				ReadOnlyRecordBuffer result = ReaderToRecord(cacheKey.Table);
				CloseReader(forceClose: false);
				return result;
			}
			return null;
		}
	}

	internal void ResetExeutionStatistics()
	{
		minimumNumberOfRowsReadBeforeResetting = 0;
		maximumNumberOfRowsReadBeforeResetting = 0;
		consecutiveCommandResetRowsFetchedWithinThreshold = 0;
	}

	internal void UpdateExecutionStatistics()
	{
		maximumNumberOfRowsReadBeforeResetting = Math.Max(maximumNumberOfRowsReadBeforeResetting, RecordsFetchedThisExecution);
		if (minimumNumberOfRowsReadBeforeResetting == 0 || minimumNumberOfRowsReadBeforeResetting > RecordsFetchedThisExecution)
		{
			minimumNumberOfRowsReadBeforeResetting = RecordsFetchedThisExecution;
		}
		if (maximumNumberOfRowsReadBeforeResetting - minimumNumberOfRowsReadBeforeResetting <= 10)
		{
			consecutiveCommandResetRowsFetchedWithinThreshold++;
		}
		else
		{
			ResetExeutionStatistics();
		}
	}

	internal override void CloseReader(bool forceClose, bool recreateCommand = true)
	{
		if (!forceClose || State != NavSqlCommandState.ExecutedWithPendingResults)
		{
			base.CloseReader(forceClose, recreateCommand);
			return;
		}
		UpdateExecutionStatistics();
		if (topRowsAppliedAtExecution > RecordsFetchedThisExecution && topRowsAppliedAtExecution - RecordsFetchedThisExecution <= 10)
		{
			while (ReadNextRow())
			{
			}
		}
		else
		{
			base.CloseReader(forceClose: true, recreateCommand);
		}
	}

	private ReadOnlyRecordBuffer ExecuteFindFirstLast(MutableRecordBuffer inputRecord, FilterExpression filter, DataLockState lockState, int topRows, bool onlyCurrentKeyFields, NavDatabase database, int companyIndex, CompanyId? companyId, int? commandTimeout)
	{
		int num = cacheKey.FindMethod & -129;
		bool flag = (cacheKey.FindMethod & 0x80) != 0;
		if (!flag && consecutiveZeroRowsReturned >= 3)
		{
			num |= 0x80;
			ReadOnlyRecordBuffer readOnlyRecordBuffer = FindFirstLast(database, cacheKey.Table, companyIndex, (byte)num, lockState, inputRecord, cacheKey.OrderByFields, filter, onlyRetrieveCurrentKeyFields: false, commandTimeout);
			if (readOnlyRecordBuffer != null)
			{
				consecutiveZeroRowsReturned = 0;
			}
			return readOnlyRecordBuffer;
		}
		if (State == NavSqlCommandState.ExecutedWithPendingResults)
		{
			CloseReader(forceClose: true);
		}
		int nextParameterNo = ((topRows != 0) ? 1 : 0);
		if (State == NavSqlCommandState.Created)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string defaultMainTableAlias = NavGlobal.SystemTenant.NavSqlTableAliasMap.GetDefaultMainTableAlias(cacheKey.Table.TableId);
			NavSqlStatementHelper.AppendFromClauseWithCompanionTableJoinsIncluded(stringBuilder, cacheKey.Table, database, companyIndex, cacheKey.Hints, cacheKey.Table.DistinctCompanionTableOrigins, qualifyTableWithDatabase: true, defaultMainTableAlias);
			NavSqlWhereClauseBuilder.AppendFilterClause(stringBuilder, filter, new NavSqlParameterHandler(nextParameterNo), cacheKey.SqlSortingProperties, companyId, shouldAliasQualifyFields: true, defaultMainTableAlias);
			StringBuilder stringBuilder2 = new StringBuilder();
			if (flag)
			{
				stringBuilder2.Append("IF EXISTS(SELECT TOP 1 NULL");
				stringBuilder2.Append(stringBuilder);
				stringBuilder2.Append(") ");
			}
			stringBuilder2.Append("SELECT ");
			if (topRows > 0)
			{
				stringBuilder2.Append(string.Format(CultureInfo.InvariantCulture, " TOP ({0}) ", NavSqlStatementHelper.ParameterName(0)));
			}
			NavSqlStatementHelper.AppendFieldList(stringBuilder2, cacheKey.Table.SqlReadableFields, NavSqlColumnListType.DataLengthInsteadOfBlobs, defaultMainTableAlias);
			stringBuilder2.Append(stringBuilder);
			NavSqlStatementHelper.AppendOrderByList(stringBuilder2, (num == 43 || num == 47) ? cacheKey.OrderByFields.Reversed : cacheKey.OrderByFields);
			NavSqlStatementHelper.AppendSelectStatementOptions(stringBuilder2, addFastOption: true);
			SetCommandText(stringBuilder2.ToString());
		}
		if (State != NavSqlCommandState.ExecutedWithPendingResults)
		{
			if (topRows > 0)
			{
				SqlParameter sqlParameter;
				if (Command.Parameters.Count == 0)
				{
					sqlParameter = new SqlParameter();
					sqlParameter.ParameterName = NavSqlStatementHelper.ParameterName(0);
					sqlParameter.SqlDbType = SqlDbType.Int;
					Command.Parameters.Add(sqlParameter);
				}
				else
				{
					sqlParameter = Command.Parameters[0];
				}
				sqlParameter.Value = topRows;
			}
			NavSqlWhereClauseBuilder.AddOrSetFilterParameters(filter, Command.Parameters, new NavSqlParameterHandler(nextParameterNo), cacheKey.SqlSortingProperties, companyId);
		}
		PrepareStatement();
		SetCommandTimeout(commandTimeout);
		if (State == NavSqlCommandState.Prepared)
		{
			topRowsAppliedAtExecution = topRows;
			ExecuteReader();
		}
		if (ReadNextRow())
		{
			FieldList keyFieldsToRead = (onlyCurrentKeyFields ? cacheKey.OrderByFields.FieldList : null);
			return ReaderToRecord(cacheKey.Table, keyFieldsToRead);
		}
		if (!flag)
		{
			consecutiveZeroRowsReturned++;
		}
		return null;
	}

	private ReadOnlyRecordBuffer ExecuteFindTop(FilterExpression filter, bool onlyCurrentKeyFieldsNeeded, NavDatabase database, int companyIndex, CompanyId? companyId)
	{
		if (State == NavSqlCommandState.Created)
		{
			StringBuilder stringBuilder = new StringBuilder("SELECT ");
			stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, " TOP ({0}) ", 1));
			string defaultMainTableAlias = NavGlobal.SystemTenant.NavSqlTableAliasMap.GetDefaultMainTableAlias(cacheKey.Table.TableId);
			NavSqlStatementHelper.AppendFieldList(stringBuilder, cacheKey.Table.SqlReadableFields, NavSqlColumnListType.DataLengthInsteadOfBlobs, defaultMainTableAlias);
			NavSqlStatementHelper.AppendFromClauseWithCompanionTableJoinsIncluded(stringBuilder, cacheKey.Table, database, companyIndex, cacheKey.Hints, cacheKey.Table.DistinctCompanionTableOrigins, qualifyTableWithDatabase: true, defaultMainTableAlias);
			NavSqlWhereClauseBuilder.AppendFilterClause(stringBuilder, filter, new NavSqlParameterHandler(0), cacheKey.SqlSortingProperties, companyId, shouldAliasQualifyFields: true, defaultMainTableAlias);
			NavSqlStatementHelper.AppendOrderByList(stringBuilder, (cacheKey.FindMethod == 93) ? cacheKey.OrderByFields.Reversed : cacheKey.OrderByFields);
			NavSqlStatementHelper.AppendSelectStatementOptions(stringBuilder, addFastOption: false);
			SetCommandText(stringBuilder.ToString());
		}
		NavSqlWhereClauseBuilder.AddOrSetFilterParameters(filter, Command.Parameters, new NavSqlParameterHandler(0), cacheKey.SqlSortingProperties, companyId);
		PrepareStatement();
		using (ExecuteReader())
		{
			if (ReadNextRow())
			{
				FieldList keyFieldsToRead = (onlyCurrentKeyFieldsNeeded ? cacheKey.OrderByFields.FieldList : null);
				ReadOnlyRecordBuffer result = ReaderToRecord(cacheKey.Table, keyFieldsToRead);
				CloseReader(forceClose: false);
				return result;
			}
			return null;
		}
	}

	private static FilterExpression CreateCombinedFilter(IRecordBuffer startingPosition, SortingFieldList sortingFields, bool includeCurrent, FiltersAndMarks filtersAndMarks)
	{
		if (startingPosition != null)
		{
			FilterExpression filter = DataProviderHelper.BuildOptimizedCompletePositioningFilter(startingPosition, sortingFields, includeCurrent, filtersAndMarks);
			return ValueInsensitiveFilterExpressionEqualityComparer.GetExpressionWithValueTokens(filter);
		}
		return filtersAndMarks.GetCombinedFilter();
	}

	private static TransactionType ConvertLockStateToTransactionType(DataLockState lockState)
	{
		return lockState switch
		{
			DataLockState.None => TransactionType.Browse, 
			DataLockState.Shared => TransactionType.Update, 
			DataLockState.LockExclusive => TransactionType.UpdateNoLocks, 
			_ => throw new ArgumentException("Unknown lock state", "lockState"), 
		};
	}

	internal static ReadOnlyRecordBuffer FindEqual(NavDatabase database, NCLMetaTable table, int companyIndex, byte findMethod, bool locking, MutableRecordBuffer inputRecord, FilterExpression filter, SortingFieldList orderByFields)
	{
		database.CheckTableInSync(table);
		NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, locking);
		try
		{
			string hints = NavSqlStatementHelper.GenerateSelectStatementHints(locking, noExpand: false, conScope.Connection);
			filter = NavSqlWhereClauseBuilder.CreateFilterBasedOnPrimaryKeyValuesFromRecordAndFilter(filter, FilterExpressionType.Equal, inputRecord, orderByFields, useOriginalValues: false);
			IsamQueryCommandCacheKey commandCacheKey = new IsamQueryCommandCacheKey(findMethod, table, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), orderByFields, filter, hints, topRows: false);
			return conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) => ((NavSqlIsamQueryCommand)command).ExecuteFindEqualCommand(filter, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex)));
		}
		finally
		{
			if (conScope != null)
			{
				((IDisposable)conScope).Dispose();
			}
		}
	}

	internal static ReadOnlyRecordBuffer FindEqual(NavDatabase database, RecordIdProviderRequest request)
	{
		NCLMetaTable nCLMetaTable = request.MetaApplicationObject as NCLMetaTable;
		database.CheckTableInSync(nCLMetaTable);
		NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, ConvertLockStateToTransactionType(request.LockState));
		try
		{
			string hints = NavSqlStatementHelper.GenerateSelectStatementHints(request.LockState, noExpand: false, conScope.Connection);
			FilterExpression filter = null;
			for (int i = 0; i < nCLMetaTable.PrimaryKey.KeyFieldCount; i++)
			{
				NCLMetaField keyFieldByIndex = nCLMetaTable.PrimaryKey.GetKeyFieldByIndex(i);
				NavValue value = request.RecordId.FieldValueByIndex(keyFieldByIndex, i);
				filter = FilterExpression.And(filter, FilterExpression.Equal(value, keyFieldByIndex.ExpressionContext));
			}
			IsamQueryCommandCacheKey commandCacheKey = new IsamQueryCommandCacheKey(33, nCLMetaTable, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, request.CompanyToken), nCLMetaTable.PrimaryKey.SortingFieldsWithPrimaryKeyFields, filter, hints, topRows: false);
			return conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) => ((NavSqlIsamQueryCommand)command).ExecuteFindEqualCommand(filter, database, request.CompanyToken, database.GetCompanyId(commandCacheKey.Table, request.CompanyToken)));
		}
		finally
		{
			if (conScope != null)
			{
				((IDisposable)conScope).Dispose();
			}
		}
	}

	internal static ReadOnlyRecordBuffer FindFirstLast(NavDatabase database, NCLMetaTable table, int companyIndex, byte findMethod, DataLockState lockState, MutableRecordBuffer inputRecord, SortingFieldList orderByFields, FilterExpression filter, bool onlyRetrieveCurrentKeyFields, int? commandTimeout)
	{
		database.CheckTableInSync(table);
		NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, lockState != DataLockState.None);
		try
		{
			string hints = NavSqlStatementHelper.GenerateSelectStatementHints(lockState, noExpand: false, conScope.Connection);
			if (findMethod == 46 || findMethod == 47)
			{
				filter = NavSqlWhereClauseBuilder.CreateFilterBasedOnPrimaryKeyValuesFromRecordAndFilter(filter, (findMethod == 46) ? FilterExpressionType.GreaterThanOrEqual : FilterExpressionType.LessThanOrEqual, inputRecord, orderByFields, useOriginalValues: false);
			}
			IsamQueryCommandCacheKey commandCacheKey = new IsamQueryCommandCacheKey(findMethod, table, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), orderByFields, filter, hints, topRows: false);
			return conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) => ((NavSqlIsamQueryCommand)command).ExecuteFindFirstLast(null, filter, lockState, 0, onlyRetrieveCurrentKeyFields, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex), commandTimeout));
		}
		finally
		{
			if (conScope != null)
			{
				((IDisposable)conScope).Dispose();
			}
		}
	}

	internal static IEnumerable<ReadOnlyRecordBuffer> FindAll(NavDatabase database, PositionedFindProviderRequest request, Func<bool> areOnlyKeyFieldsNeededForNextRow)
	{
		NCLMetaTable table = request.MetaApplicationObject as NCLMetaTable;
		TransactionType transactionType = ConvertLockStateToTransactionType(request.LockState);
		NavSqlIsamQueryCommand findAllCommand = null;
		int savedReaderToken = 0;
		ReadOnlyRecordBuffer record = null;
		int topRows = ((request.FindType != FindType.All) ? 50 : 0);
		FilterExpression filter = CreateCombinedFilter(request.StartingPosition, request.SortingFields, request.IncludeCurrent, request.FiltersAndMarks);
		if (filter != null && filter.IsConstantFalse())
		{
			yield break;
		}
		database.CheckTableInSync(table);
		IReference<NavSqlConnection> lastUsedSqlConnection = null;
		while (true)
		{
			NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, transactionType, ref lastUsedSqlConnection);
			try
			{
				if (findAllCommand == null || findAllCommand.IsDisposed || findAllCommand.ReaderToken != savedReaderToken || findAllCommand.State != NavSqlCommandState.ExecutedWithPendingResults || findAllCommand.Connection != conScope.Connection)
				{
					if (findAllCommand != null)
					{
						if (findAllCommand.IsDisposed || findAllCommand.Connection == conScope.Connection)
						{
							NavEnvironment.Instance.PerformanceCounterSetter.UpdatePerformanceCountersWithPreferredCacheCommandReuseResult(tryGetResult: false);
						}
						if (topRows > 0)
						{
							findAllCommand.ResetExeutionStatistics();
							topRows = 0;
						}
						filter = CreateCombinedFilter(record, request.SortingFields, includeCurrent: false, request.FiltersAndMarks);
						if (filter != null && filter.IsConstantFalse())
						{
							break;
						}
					}
					string hints = NavSqlStatementHelper.GenerateSelectStatementHints(request.LockState, noExpand: false, conScope.Connection);
					IsamQueryCommandCacheKey commandCacheKey = new IsamQueryCommandCacheKey(45, table, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, request.CompanyToken), request.SortingFields, filter, hints, topRows > 0);
					record = conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) =>
					{
						findAllCommand = (NavSqlIsamQueryCommand)command;
						if (findAllCommand.State == NavSqlCommandState.ExecutedWithPendingResults)
						{
							findAllCommand.CloseReader(forceClose: true);
						}
						if (topRows > 0 && findAllCommand.consecutiveCommandResetRowsFetchedWithinThreshold >= 3)
						{
							topRows = findAllCommand.minimumNumberOfRowsReadBeforeResetting + 10;
						}
						return findAllCommand.ExecuteFindFirstLast((record == null) ? null : new MutableRecordBuffer(record), filter, request.LockState, topRows, areOnlyKeyFieldsNeededForNextRow != null && areOnlyKeyFieldsNeededForNextRow(), database, request.CompanyToken, database.GetCompanyId(commandCacheKey.Table, request.CompanyToken), request.Timeout);
					});
					if (record == null)
					{
						break;
					}
					savedReaderToken = findAllCommand.ReaderToken;
				}
				else
				{
					NavEnvironment.Instance.PerformanceCounterSetter.UpdatePerformanceCountersWithPreferredCacheCommandReuseResult(tryGetResult: true);
					if (!findAllCommand.ReadNextRow())
					{
						if (findAllCommand.RecordsFetchedThisExecution > findAllCommand.minimumNumberOfRowsReadBeforeResetting + 10)
						{
							findAllCommand.ResetExeutionStatistics();
						}
						break;
					}
					FieldList keyFieldsToRead = ((areOnlyKeyFieldsNeededForNextRow != null && areOnlyKeyFieldsNeededForNextRow()) ? request.SortingFields.FieldList : null);
					record = findAllCommand.ReaderToRecord(table, keyFieldsToRead);
				}
				if (topRows > 0 && findAllCommand.RecordsFetchedThisExecution == topRows)
				{
					findAllCommand.CloseReader(forceClose: false);
				}
			}
			finally
			{
				if (conScope != null)
				{
					((IDisposable)conScope).Dispose();
				}
			}
			yield return record;
		}
	}

	internal static ReadOnlyRecordBuffer FindTop1(NavDatabase database, PositionedFindProviderRequest request, bool onlyCurrentKeyFieldsNeeded)
	{
		bool locking = request.LockState == DataLockState.LockExclusive;
		NCLMetaTable table = request.MetaApplicationObject as NCLMetaTable;
		FilterExpression filterExpression = CreateCombinedFilter(request.StartingPosition, request.SortingFields, request.IncludeCurrent, request.FiltersAndMarks);
		if (filterExpression != null && filterExpression.IsConstantFalse())
		{
			return null;
		}
		return FindTop1(database, table, request.CompanyToken, 91, locking, onlyCurrentKeyFieldsNeeded, request.SortingFields, filterExpression);
	}

	internal static ReadOnlyRecordBuffer FindTop1(NavDatabase database, NCLMetaTable table, int companyIndex, byte findMethod, bool locking, bool onlyCurrentKeyFieldsNeeded, SortingFieldList orderByFields, FilterExpression filter)
	{
		database.CheckTableInSync(table);
		NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, locking);
		try
		{
			string hints = NavSqlStatementHelper.GenerateSelectStatementHints(locking, noExpand: false, conScope.Connection);
			IsamQueryCommandCacheKey commandCacheKey = new IsamQueryCommandCacheKey(findMethod, table, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), orderByFields, filter, hints, topRows: false);
			return conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) => ((NavSqlIsamQueryCommand)command).ExecuteFindTop(filter, onlyCurrentKeyFieldsNeeded, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex)));
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
