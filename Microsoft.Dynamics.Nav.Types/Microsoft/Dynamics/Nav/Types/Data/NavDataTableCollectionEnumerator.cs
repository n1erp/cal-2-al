using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Types.Data;

public sealed class NavDataTableCollectionEnumerator : IEnumerable<NavDataRow>, IEnumerable, IDisposable
{
	private bool cacheData;

	private bool endSignal;

	private bool enumerationFinished;

	private int rowsAdded;

	private List<NavDataTable> dataTables = new List<NavDataTable>();

	private List<NavDataTable> cachedDataTable = new List<NavDataTable>();

	private Semaphore tableLock;

	public bool HasDeliveredRows => RowsRead > 0;

	public int RowsRead { get; private set; }

	public int RowsAdded => rowsAdded;

	public bool IsCachingData => cacheData;

	public ReadOnlyCollection<NavDataTable> CachedDataTables => new ReadOnlyCollection<NavDataTable>(cachedDataTable);

	public bool EnumerationFinished => enumerationFinished;

	public NavDataTableCollectionEnumerator(bool waitForEndSignal, bool cacheData)
	{
		endSignal = !waitForEndSignal;
		this.cacheData = cacheData;
	}

	public void FinalDataTableAdded()
	{
		endSignal = true;
		ReleaseTableLock();
	}

	public IList<NavDataRow> DecompressRows()
	{
		List<NavDataRow> list = new List<NavDataRow>();
		if (endSignal && dataTables.Count > 0)
		{
			NavDataTableReader navDataTableReader = new NavDataTableReader(dataTables[0], updateRow: false);
			foreach (NavDataTable dataTable in dataTables)
			{
				foreach (NavDataRow row in dataTable.Rows)
				{
					int i = 0;
					for (int count = row.Columns.Count; i < count; i++)
					{
						row[i] = navDataTableReader.GetValue(row, i);
					}
					list.Add(row);
				}
			}
		}
		return list;
	}

	internal DataTable CreateDataTable(bool mediaColumnsAsByteArray)
	{
		DataTable dataTable = new DataTable();
		if (!endSignal || dataTables.Count <= 0)
		{
			return dataTable;
		}
		dataTable.Locale = dataTables[0].Locale;
		return dataTables[0].GetDataTable(mediaColumnsAsByteArray);
	}

	private void ReleaseTableLock()
	{
		if (tableLock != null)
		{
			tableLock.Release();
			tableLock = null;
		}
	}

	public void AddDataTable(NavDataTable table)
	{
		lock (dataTables)
		{
			rowsAdded += table.Rows.Count;
			dataTables.Add(table);
			ReleaseTableLock();
		}
	}

	private NavDataTable GetNextNavDataTable()
	{
		while (true)
		{
			Semaphore semaphore = null;
			lock (dataTables)
			{
				if (dataTables.Count > 0)
				{
					NavDataTable navDataTable = dataTables[0];
					dataTables.RemoveAt(0);
					if (cacheData)
					{
						cachedDataTable.Add(navDataTable);
					}
					return navDataTable;
				}
				if (endSignal)
				{
					return null;
				}
				tableLock = new Semaphore(0, 1);
				semaphore = tableLock;
			}
			semaphore?.WaitOne();
		}
	}

	public IEnumerator<NavDataRow> GetEnumerator()
	{
		enumerationFinished = false;
		for (NavDataTable nextNavDataTable = GetNextNavDataTable(); nextNavDataTable != null; nextNavDataTable = GetNextNavDataTable())
		{
			foreach (NavDataRow row in nextNavDataTable.Rows)
			{
				RowsRead++;
				yield return row;
			}
		}
		enumerationFinished = true;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Dispose()
	{
		if (tableLock != null)
		{
			((IDisposable)tableLock).Dispose();
			tableLock = null;
		}
	}
}
