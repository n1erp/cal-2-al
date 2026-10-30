using System;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class DataCacheTableState
{
	private readonly DataCacheDatabaseState databaseState;

	private readonly int companyToken;

	private readonly int tableId;

	private DataLockState lockState;

	private readonly bool isGlobal;

	private CacheClear clearGlobalCacheOnCommit;

	private int transactionBumperToken;

	private PrimaryKeyCache primaryKeyCache;

	private ResultSetGroupCache resultSetGroupCache;

	private DataCacheTableState globalTableState;

	private int globalTableVersionToken;

	internal int TableId => tableId;

	internal DataLockState LockState => lockState;

	internal bool IsGlobal => isGlobal;

	internal CacheClear ClearGlobalCacheOnCommit
	{
		get
		{
			return clearGlobalCacheOnCommit;
		}
		set
		{
			clearGlobalCacheOnCommit = value;
		}
	}

	internal int TransactionBumperToken => transactionBumperToken;

	internal PrimaryKeyCache PrimaryKeyCache
	{
		get
		{
			if (primaryKeyCache == null && lockState != DataLockState.None)
			{
				primaryKeyCache = new PrimaryKeyCache(databaseState.LRUCache, databaseState.Database, transactionBumperToken);
			}
			return primaryKeyCache;
		}
	}

	internal ResultSetGroupCache ResultSetGroupCache
	{
		get
		{
			if (resultSetGroupCache == null && !isGlobal)
			{
				resultSetGroupCache = new ResultSetGroupCache(databaseState, transactionBumperToken, this);
			}
			return resultSetGroupCache;
		}
	}

	internal DataCacheDatabaseState DatabaseState => databaseState;

	internal DataCacheTableState GlobalTableState
	{
		get
		{
			if (globalTableState == null)
			{
				globalTableState = (isGlobal ? this : databaseState.GetGlobalTableState(companyToken, tableId));
			}
			return globalTableState;
		}
	}

	internal bool IsNonTransactionalNonGlobal
	{
		get
		{
			if (lockState == DataLockState.None)
			{
				return !isGlobal;
			}
			return false;
		}
	}

	internal bool IsPrivateTransactional => lockState != DataLockState.None;

	internal int GlobalTableVersionToken
	{
		get
		{
			if (!isGlobal)
			{
				return 0;
			}
			return globalTableVersionToken;
		}
	}

	internal DataCacheTableState(DataCacheDatabaseState databaseState, int companyToken, int tableId, bool isGlobal)
	{
		this.databaseState = databaseState;
		this.companyToken = companyToken;
		this.tableId = tableId;
		this.isGlobal = isGlobal;
		if (isGlobal)
		{
			primaryKeyCache = new PrimaryKeyCache(databaseState.LRUCache, databaseState.Database, 0);
			resultSetGroupCache = new ResultSetGroupCache(databaseState, 0, this);
		}
	}

	internal int IncrementGlobalTableVersionToken()
	{
		if (isGlobal)
		{
			return Interlocked.Increment(ref globalTableVersionToken);
		}
		return 0;
	}

	internal void Clear()
	{
		Clear(CacheClear.ClearAll);
	}

	internal void Clear(CacheClear cacheClear, MutableRecordBuffer recordBufferCausingWrite = null)
	{
		if ((cacheClear & CacheClear.ClearPrimaryKeyCache) != 0 && primaryKeyCache != null)
		{
			primaryKeyCache.Clear();
		}
		if ((cacheClear & CacheClear.ClearResultSetCache) != 0 && resultSetGroupCache != null)
		{
			resultSetGroupCache.Clear(recordBufferCausingWrite);
		}
	}

	internal void SetLockStateAndTransactionToken(DataLockState lockStateParameter, int transactionBumperTokenParameter)
	{
		if (isGlobal)
		{
			throw new InvalidOperationException();
		}
		lockState = lockStateParameter;
		if (lockState == DataLockState.None)
		{
			transactionBumperToken = 0;
			if (primaryKeyCache != null)
			{
				primaryKeyCache.Clear();
				primaryKeyCache = null;
			}
			if (resultSetGroupCache != null)
			{
				resultSetGroupCache.Clear();
				resultSetGroupCache = null;
			}
		}
		else
		{
			transactionBumperToken = transactionBumperTokenParameter;
			clearGlobalCacheOnCommit = CacheClear.DoNotClear;
		}
	}
}
