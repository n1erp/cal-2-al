using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CacheSynchronization
{
	internal const int AllCachesToken = -1;

	private readonly NavDatabase database;

	private Dictionary<int, SortedSet<int>> modifiedTables = new Dictionary<int, SortedSet<int>>();

	private readonly object modifiedTablesLockObject = new object();

	public bool HasChanges => modifiedTables.Count > 0;

	public event EventHandler<TableDataChangedEventArgs> TableDataChanged;

	public CacheSynchronization(NavDatabase database)
	{
		this.database = database;
	}

	public void RegisterTableDataChanged(int tableId, int companyNameToken)
	{
		lock (modifiedTablesLockObject)
		{
			if (modifiedTables.TryGetValue(companyNameToken, out var value))
			{
				value.Add(tableId);
			}
			else
			{
				modifiedTables[companyNameToken] = new SortedSet<int> { tableId };
			}
		}
		ICacheSynchronizationController cacheSynchronizationController = NavEnvironment.Instance.CacheSynchronizationController;
		if (cacheSynchronizationController != null && cacheSynchronizationController.EmitTableDataChangesEvents)
		{
			NavEnvironment.Instance.CacheSynchronizationController.OnTenantDataChanged(database.Tenant);
		}
	}

	internal void RegisterTableDataChanged(List<TableSet> tables)
	{
		lock (modifiedTablesLockObject)
		{
			foreach (TableSet table in tables)
			{
				int companyNameToken = database.GetCompanyNameToken(table.CompanyName);
				foreach (int tableId in table.TableIds)
				{
					RegisterTableDataChanged(tableId, companyNameToken);
				}
			}
		}
	}

	internal List<TableSet> GetAndClearModifiedTables()
	{
		lock (modifiedTablesLockObject)
		{
			if (modifiedTables.Count == 0)
			{
				return new List<TableSet>(0);
			}
			List<TableSet> list = new List<TableSet>(modifiedTables.Count);
			foreach (KeyValuePair<int, SortedSet<int>> modifiedTable in modifiedTables)
			{
				list.Add(new TableSet
				{
					CompanyName = database.GetCompanyNameFromToken(modifiedTable.Key),
					TableIds = modifiedTable.Value
				});
			}
			modifiedTables.Clear();
			return list;
		}
	}

	internal void OnTableDataChanged(TableDataChangedEventArgs e)
	{
		Volatile.Read(ref TableDataChanged)?.Invoke(this, e);
	}
}
