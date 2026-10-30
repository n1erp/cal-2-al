using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DataCacheDatabaseState
{
	private readonly NavDatabase database;

	private readonly List<Dictionary<int, DataCacheTableState>> globalTableStates;

	private int overallGlobalTableVersionToken;

	internal NavDatabase Database => database;

	internal LRUCache LRUCache => NavEnvironment.Instance.LRUCache;

	internal int OverallGlobalTableVersionToken => overallGlobalTableVersionToken;

	internal DataCacheDatabaseState(NavDatabase database)
	{
		if (database == null)
		{
			throw new ArgumentNullException("database");
		}
		this.database = database;
		globalTableStates = new List<Dictionary<int, DataCacheTableState>>(10);
	}

	internal PrimaryKeyCache GlobalPrimaryKeyCache(int companyToken, int tableId)
	{
		DataCacheTableState globalTableState = GetGlobalTableState(companyToken, tableId);
		return globalTableState.PrimaryKeyCache;
	}

	internal ResultSetGroupCache GlobalResultSetGroupCache(int companyToken, int tableId)
	{
		DataCacheTableState globalTableState = GetGlobalTableState(companyToken, tableId);
		return globalTableState.ResultSetGroupCache;
	}

	internal DataCacheTableState GetGlobalTableState(int companyToken, int tableId)
	{
		Dictionary<int, DataCacheTableState> dictionary = GetGlobalTableStates(companyToken);
		lock (dictionary)
		{
			if (!dictionary.TryGetValue(tableId, out var value))
			{
				value = new DataCacheTableState(this, companyToken, tableId, isGlobal: true);
				dictionary.Add(tableId, value);
			}
			return value;
		}
	}

	private Dictionary<int, DataCacheTableState> GetGlobalTableStates(int companyToken)
	{
		if (companyToken < globalTableStates.Count)
		{
			Dictionary<int, DataCacheTableState> dictionary = globalTableStates[companyToken];
			if (dictionary != null)
			{
				return dictionary;
			}
		}
		lock (globalTableStates)
		{
			while (companyToken >= globalTableStates.Count)
			{
				globalTableStates.Add(null);
			}
			Dictionary<int, DataCacheTableState> dictionary = globalTableStates[companyToken];
			if (dictionary == null)
			{
				dictionary = new Dictionary<int, DataCacheTableState>(17);
				globalTableStates[companyToken] = dictionary;
			}
			return dictionary;
		}
	}

	internal void ClearGlobalCache()
	{
		List<DataCacheTableState> list = new List<DataCacheTableState>(1024);
		lock (globalTableStates)
		{
			foreach (Dictionary<int, DataCacheTableState> globalTableState in globalTableStates)
			{
				if (globalTableState == null)
				{
					continue;
				}
				lock (globalTableState)
				{
					foreach (KeyValuePair<int, DataCacheTableState> item in globalTableState)
					{
						list.Add(item.Value);
					}
					globalTableState.Clear();
				}
			}
			globalTableStates.Clear();
		}
		foreach (DataCacheTableState item2 in list)
		{
			item2.Clear();
		}
	}

	internal void ClearGlobalCacheAndEnsureNonTransactionalCacheIsNotPromoted()
	{
		lock (globalTableStates)
		{
			foreach (Dictionary<int, DataCacheTableState> globalTableState in globalTableStates)
			{
				if (globalTableState == null)
				{
					continue;
				}
				lock (globalTableState)
				{
					foreach (KeyValuePair<int, DataCacheTableState> item in globalTableState)
					{
						DataCacheTableState value = item.Value;
						ClearTableStateAndIncrementGlobalTableVersionToken(value);
					}
				}
			}
		}
	}

	private void ClearTableStateAndIncrementGlobalTableVersionToken(DataCacheTableState tableState)
	{
		if (tableState != null && tableState.IsGlobal)
		{
			tableState.IncrementGlobalTableVersionToken();
			IncrementOverallGlobalTableVersionToken();
			tableState.Clear();
		}
	}

	internal void ClearGlobalCacheByTableId(int companyToken, int tableId)
	{
		DataCacheTableState globalTableState = GetGlobalTableState(companyToken, tableId);
		if (globalTableState != null && globalTableState.IsGlobal)
		{
			globalTableState.Clear();
		}
	}

	internal void ClearGlobalCacheAndEnsureNonTransactionalCacheIsNotPromoted(int companyToken, int tableId)
	{
		DataCacheTableState globalTableState = GetGlobalTableState(companyToken, tableId);
		ClearTableStateAndIncrementGlobalTableVersionToken(globalTableState);
	}

	internal int IncrementOverallGlobalTableVersionToken()
	{
		return Interlocked.Increment(ref overallGlobalTableVersionToken);
	}
}
