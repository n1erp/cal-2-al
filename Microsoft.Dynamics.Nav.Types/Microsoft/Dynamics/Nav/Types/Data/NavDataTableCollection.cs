using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types.Data;

[Serializable]
[DebuggerDisplay("Tables : {Count}")]
public class NavDataTableCollection : ICollection<NavDataTable>, IEnumerable<NavDataTable>, IEnumerable
{
	[NonSerialized]
	private readonly NavDataSet dataSet;

	private readonly List<NavDataTable> list;

	public NavDataTable this[int index]
	{
		get
		{
			try
			{
				return list[index];
			}
			catch (ArgumentOutOfRangeException innerException)
			{
				throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture, "Cannot find table {0}.", index), innerException);
			}
		}
	}

	public NavDataTable this[string tableName]
	{
		get
		{
			if (!string.IsNullOrEmpty(tableName))
			{
				for (int i = 0; i < Count; i++)
				{
					if (tableName == this[i].TableName)
					{
						return this[i];
					}
				}
				return null;
			}
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Table {0} does not belong to this dataset.", tableName), "tableName");
		}
	}

	public int Count => list.Count;

	public bool IsReadOnly => false;

	public NavDataTableCollection(NavDataSet dataSet)
	{
		if (dataSet == null)
		{
			throw new ArgumentNullException("dataSet");
		}
		this.dataSet = dataSet;
		list = new List<NavDataTable>();
	}

	public NavDataTable Add(string name)
	{
		NavDataTable navDataTable = new NavDataTable(name);
		Add(navDataTable);
		return navDataTable;
	}

	public void Clear()
	{
		for (int i = 0; i < list.Count; i++)
		{
			list[i].DataSet = null;
		}
		list.Clear();
	}

	public void Add(NavDataTable table)
	{
		if (table == null)
		{
			throw new ArgumentNullException("table");
		}
		if (table.DataSet == dataSet)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "DataTable '{0}' already belongs to this dataset.", table.TableName), "table");
		}
		if (table.DataSet != null)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "DataTable '{0}' already belongs to another dataset.", table.TableName), "table");
		}
		CultureInfo locale = dataSet.Locale;
		for (int i = 0; i < list.Count; i++)
		{
			if (string.Compare(list[i].TableName, table.TableName, ignoreCase: true, locale) == 0)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "A DataTable named '{0}' already belongs to this dataset.", table.TableName), "table");
			}
		}
		table.DataSet = dataSet;
		list.Add(table);
	}

	public bool Contains(NavDataTable item)
	{
		return list.Contains(item);
	}

	public bool Contains(string tableName)
	{
		return FindTable(tableName) != null;
	}

	public void CopyTo(NavDataTable[] array, int arrayIndex)
	{
		list.CopyTo(array, arrayIndex);
	}

	public bool Remove(NavDataTable item)
	{
		if (item != null)
		{
			item.DataSet = null;
		}
		return list.Remove(item);
	}

	public bool Remove(string tableName)
	{
		return Remove(FindTable(tableName));
	}

	private NavDataTable FindTable(string tableName)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (string.Compare(list[i].TableName, tableName, ignoreCase: true, list[i].Locale) == 0)
			{
				return list[i];
			}
		}
		return null;
	}

	public IEnumerator<NavDataTable> GetEnumerator()
	{
		return list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return list.GetEnumerator();
	}
}
