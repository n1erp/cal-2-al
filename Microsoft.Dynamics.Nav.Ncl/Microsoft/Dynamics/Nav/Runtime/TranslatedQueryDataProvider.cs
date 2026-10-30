using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class TranslatedQueryDataProvider : VirtualDataProvider
{
	protected enum QueryType
	{
		Normal,
		FirstOnly,
		Count,
		Exists
	}

	private class TranslatedQueryCommandCacheKey : NavSqlCommandCacheKey
	{
		private readonly Type dataProviderType;

		private readonly SortingFieldList sortingFields;

		private readonly FilterExpression filter;

		private readonly QueryType queryType;

		public TranslatedQueryCommandCacheKey(Type dataProviderType, FilterExpression filter, QueryType queryType)
			: base(CachedCommandType.TranslatedQueryDataProvider, NavGlobal.AppDatabase.DatabaseName, ComputeHashCode(dataProviderType, filter, null, queryType), isSharedTenantSchema: false)
		{
			this.dataProviderType = dataProviderType;
			this.filter = filter;
			this.queryType = queryType;
		}

		public TranslatedQueryCommandCacheKey(Type dataProviderType, FilterExpression filter, SortingFieldList sortingFields, QueryType queryType)
			: base(CachedCommandType.TranslatedQueryDataProvider, NavGlobal.AppDatabase.DatabaseName, ComputeHashCode(dataProviderType, filter, sortingFields, queryType), isSharedTenantSchema: false)
		{
			this.dataProviderType = dataProviderType;
			this.filter = filter;
			this.sortingFields = sortingFields;
			this.queryType = queryType;
		}

		private static int ComputeHashCode(Type dataProviderType, FilterExpression combinedFilter, SortingFieldList sortingFields, QueryType queryType)
		{
			return HashCodeHelper.CombineHashCodes(dataProviderType.GetHashCode(), (combinedFilter != null) ? ValueInsensitiveFilterExpressionEqualityComparer.Instance.GetHashCode(combinedFilter) : 0, sortingFields?.GetHashCode() ?? 0, (int)queryType);
		}

		public override bool Equals(object obj)
		{
			if (obj is TranslatedQueryCommandCacheKey translatedQueryCommandCacheKey && base.Equals(translatedQueryCommandCacheKey) && dataProviderType == translatedQueryCommandCacheKey.dataProviderType && sortingFields == null == (translatedQueryCommandCacheKey.sortingFields == null) && filter == null == (translatedQueryCommandCacheKey.filter == null) && (sortingFields == null || sortingFields.Equals(translatedQueryCommandCacheKey.sortingFields)) && (filter == null || ValueInsensitiveFilterExpressionEqualityComparer.Instance.Equals(filter, translatedQueryCommandCacheKey.filter)))
			{
				return queryType == translatedQueryCommandCacheKey.queryType;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	protected abstract NavDatabase Database { get; }

	public override bool TryGetByPrimaryKey(RecordIdProviderRequest request, out ReadOnlyRecordBuffer recordBuffer)
	{
		FilterExpression filterExpression = request.FiltersAndMarks.GetCombinedFilter();
		for (int i = 0; i < MetaTable.PrimaryKey.KeyFieldCount; i++)
		{
			NCLMetaField keyFieldByIndex = MetaTable.PrimaryKey.GetKeyFieldByIndex(i);
			NavValue value = request.RecordId.FieldValueByIndex(keyFieldByIndex, i);
			filterExpression = FilterExpression.And(filterExpression, FilterExpression.Equal(value, keyFieldByIndex.ExpressionContext));
		}
		recordBuffer = ExecuteFindQuery(filterExpression, MetaTable.PrimaryKey.DefinedSortingFields, firstOnly: true).FirstOrDefault();
		return recordBuffer != null;
	}

	public override IEnumerable<ReadOnlyRecordBuffer> Find(FindProviderRequest request, Func<bool> onlyCurrentKeyNeededForNextRow = null)
	{
		return ExecuteFindQuery(request.FiltersAndMarks.GetCombinedFilter(), request.SortingFields, request.FindType == FindType.FirstOnly);
	}

	public override IEnumerable<ReadOnlyRecordBuffer> FindFromPosition(PositionedFindProviderRequest request, Func<bool> onlyCurrentKeyNeededForNextRow = null)
	{
		return ExecuteFindQuery(request.FiltersAndMarks.GetCombinedFilter(), request.SortingFields, request.FindType == FindType.FirstOnly, request.StartingPosition, request.IncludeCurrent);
	}

	public override bool Exists(ExistsProviderRequest request)
	{
		return ExecuteExistsQuery(request.FiltersAndMarks.GetCombinedFilter());
	}

	public override int Count(DataProviderRequest request)
	{
		return ExecuteCountQuery(request.FiltersAndMarks.GetCombinedFilter());
	}

	protected abstract string CreateStatement(FilterExpression combinedFilter, SortingFieldList sortingFields, QueryType queryType);

	protected abstract void AddorSetParameters(SqlParameterCollection parameters, FilterExpression combinedFilter, SortingFieldList sortingFields, QueryType queryType);

	protected IEnumerable<ReadOnlyRecordBuffer> ExecuteFindQuery(FilterExpression filter, SortingFieldList sortingFields, bool firstOnly, MutableRecordBuffer startingPosition = null, bool includeCurrent = false)
	{
		QueryType queryType = (firstOnly ? QueryType.FirstOnly : QueryType.Normal);
		NavSqlCommand command = null;
		int savedReaderToken = 0;
		ReadOnlyRecordBuffer currentRecordToYield = null;
		while (true)
		{
			using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(Database))
			{
				if (command == null || command.IsDisposed || command.ReaderToken != savedReaderToken || command.State != NavSqlCommandState.ExecutedWithPendingResults || command.Connection != navSqlConnectionScope.Connection)
				{
					MutableRecordBuffer mutableRecordBuffer = ((currentRecordToYield == null) ? startingPosition : new MutableRecordBuffer(currentRecordToYield));
					if (currentRecordToYield != null)
					{
						includeCurrent = false;
					}
					FilterExpression combinedFilter = ((mutableRecordBuffer == null) ? filter : FilterExpression.And(filter, DataProviderHelper.BuildPositioningFilterFromRecordBuffer(mutableRecordBuffer, sortingFields, includeCurrent)));
					command = navSqlConnectionScope.Connection.ExecuteCommandAndCacheOnSuccess(new TranslatedQueryCommandCacheKey(GetType(), combinedFilter, sortingFields, queryType), () => CreateStatement(combinedFilter, sortingFields, queryType), (NavSqlCommand commandToExecute) =>
					{
						AddorSetParameters(commandToExecute.Command.Parameters, combinedFilter, sortingFields, queryType);
					}, (NavSqlCommand commandToExecute) =>
					{
						commandToExecute.ExecuteReader();
						return commandToExecute;
					});
					savedReaderToken = command.ReaderToken;
				}
				if (!command.ReadNextRow())
				{
					break;
				}
				navSqlConnectionScope.Connection.CachedCommands.SetLastUsedCachedCommand(command);
				currentRecordToYield = command.ReaderToRecord(MetaTable);
			}
			yield return currentRecordToYield;
		}
	}

	private int ExecuteCountQuery(FilterExpression combinedFilter)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(Database);
		return navSqlConnectionScope.Connection.ExecuteCommandAndCacheOnSuccess(new TranslatedQueryCommandCacheKey(GetType(), combinedFilter, QueryType.Count), () => CreateStatement(combinedFilter, SortingFieldList.Empty, QueryType.Count), (NavSqlCommand command) =>
		{
			AddorSetParameters(command.Command.Parameters, combinedFilter, SortingFieldList.Empty, QueryType.Count);
		}, (NavSqlCommand command) => (int)command.ExecuteScalar());
	}

	private bool ExecuteExistsQuery(FilterExpression combinedFilter)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(Database);
		return navSqlConnectionScope.Connection.ExecuteCommandAndCacheOnSuccess(new TranslatedQueryCommandCacheKey(GetType(), combinedFilter, QueryType.Exists), () => CreateStatement(combinedFilter, MetaTable.PrimaryKey.DefinedSortingFields, QueryType.Exists), (NavSqlCommand command) =>
		{
			AddorSetParameters(command.Command.Parameters, combinedFilter, MetaTable.PrimaryKey.DefinedSortingFields, QueryType.Exists);
		}, (NavSqlCommand command) =>
		{
			command.ExecuteReader();
			return command.ReadNextRow();
		});
	}
}
