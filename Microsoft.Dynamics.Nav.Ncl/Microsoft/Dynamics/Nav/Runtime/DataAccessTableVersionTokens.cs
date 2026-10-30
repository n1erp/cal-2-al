using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class DataAccessTableVersionTokens
{
	private class TempDataAccessVersionTokens : DataAccessTableVersionTokens
	{
		public override Func<bool> CreateTableVersionStillValidFunction(int companyToken, int tableNumber, GlobalFilters globalFilters, bool dynamic)
		{
			int globalFilterBumperTokenSnapshot = globalFilters.GlobalFilterBumperToken;
			int overallTokenSnapshot = overallUpdateBumperToken;
			return () => globalFilterBumperTokenSnapshot == globalFilters.GlobalFilterBumperToken && overallTokenSnapshot == overallUpdateBumperToken;
		}
	}

	private class PersistedDataAccessVersionTokens : DataAccessTableVersionTokens
	{
		private readonly Dictionary<long, int> tableSpecificUpdateBumperTokens = new Dictionary<long, int>();

		public override void IncrementUpdateBumperToken(int companyToken, int tableId)
		{
			base.IncrementUpdateBumperToken(companyToken, tableId);
			GetTableSpecificUpdateBumperTokenKeyAndValue(companyToken, tableId, out var currentTableSpecificKey, out var currentTableSpecificValue);
			currentTableSpecificValue++;
			tableSpecificUpdateBumperTokens[currentTableSpecificKey] = currentTableSpecificValue;
		}

		private void GetTableSpecificUpdateBumperTokenKeyAndValue(int companyToken, int tableId, out long currentTableSpecificKey, out int currentTableSpecificValue)
		{
			currentTableSpecificKey = GetTableSpecificUpdateBumperTokenKey(companyToken, tableId);
			tableSpecificUpdateBumperTokens.TryGetValue(currentTableSpecificKey, out currentTableSpecificValue);
		}

		public override Func<bool> CreateTableVersionStillValidFunction(int companyToken, int tableNumber, GlobalFilters globalFilters, bool dynamic)
		{
			if (!dynamic)
			{
				return CreateTimestampUnchangedFunction();
			}
			return CreateDynamicTableVersionStillValidFunction(companyToken, tableNumber, globalFilters);
		}

		private Func<bool> CreateTimestampUnchangedFunction()
		{
			DateTime requiredCacheTimestampSnapshot = MinimumRequiredCacheTimestamp;
			return () => MinimumRequiredCacheTimestamp <= requiredCacheTimestampSnapshot;
		}

		private Func<bool> CreateDynamicTableVersionStillValidFunction(int companyToken, int tableNumber, GlobalFilters globalFilters)
		{
			DateTime requiredCacheTimestampSnapshot = MinimumRequiredCacheTimestamp;
			int globalFilterBumperTokenSnapshot = globalFilters.GlobalFilterBumperToken;
			int overallTokenSnapshot = overallUpdateBumperToken;
			long tableSpecificUpdateBumperTokenKey = GetTableSpecificUpdateBumperTokenKey(companyToken, tableNumber);
			tableSpecificUpdateBumperTokens.TryGetValue(tableSpecificUpdateBumperTokenKey, out var tableSpecificValueSnapshot);
			return () =>
			{
				if (MinimumRequiredCacheTimestamp > requiredCacheTimestampSnapshot)
				{
					return false;
				}
				if (globalFilterBumperTokenSnapshot != globalFilters.GlobalFilterBumperToken)
				{
					return false;
				}
				if (overallTokenSnapshot == overallUpdateBumperToken)
				{
					return true;
				}
				tableSpecificUpdateBumperTokens.TryGetValue(tableSpecificUpdateBumperTokenKey, out var value);
				if (tableSpecificValueSnapshot != value)
				{
					return false;
				}
				overallTokenSnapshot = overallUpdateBumperToken;
				return true;
			};
		}

		private static long GetTableSpecificUpdateBumperTokenKey(int companyToken, int tableId)
		{
			return ((long)companyToken << 32) | (uint)tableId;
		}
	}

	private int overallUpdateBumperToken;

	public DateTime MinimumRequiredCacheTimestamp { get; private set; }

	public static DataAccessTableVersionTokens CreateForTempTable()
	{
		return new TempDataAccessVersionTokens();
	}

	public static DataAccessTableVersionTokens CreateForNonTempTables()
	{
		return new PersistedDataAccessVersionTokens();
	}

	public void SelectLatestVersion()
	{
		MinimumRequiredCacheTimestamp = DateTime.UtcNow;
	}

	public virtual void IncrementUpdateBumperToken(int companyToken, int tableId)
	{
		overallUpdateBumperToken++;
	}

	public abstract Func<bool> CreateTableVersionStillValidFunction(int companyToken, int tableNumber, GlobalFilters globalFilters, bool dynamic);
}
