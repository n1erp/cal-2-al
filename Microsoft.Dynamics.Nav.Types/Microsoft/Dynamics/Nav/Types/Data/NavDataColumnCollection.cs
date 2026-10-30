using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types.Data;

[Serializable]
[DebuggerDisplay("Columns : {Count}")]
public class NavDataColumnCollection : ICollection<NavDataColumn>, IEnumerable<NavDataColumn>, IEnumerable
{
	private readonly NavDataTable table;

	private readonly List<NavDataColumn> list;

	[NonSerialized]
	private readonly Dictionary<string, NavDataColumn> columnFromName;

	public NavDataColumn this[string name]
	{
		get
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (!columnFromName.TryGetValue(name, out var value))
			{
				value = this[IndexOf(name)];
				RegisterColumnName(name, value);
			}
			return value;
		}
	}

	public NavDataColumn this[int index]
	{
		get
		{
			try
			{
				return list[index];
			}
			catch (ArgumentOutOfRangeException innerException)
			{
				throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture, "Cannot find column {0}.", index), innerException);
			}
		}
	}

	public int Count => list.Count;

	public bool IsReadOnly => false;

	public NavDataColumnCollection(NavDataTable table)
	{
		if (table == null)
		{
			throw new ArgumentNullException("table");
		}
		this.table = table;
		list = new List<NavDataColumn>();
		columnFromName = new Dictionary<string, NavDataColumn>();
	}

	public int IndexOf(NavDataColumn column)
	{
		return list.IndexOf(column);
	}

	private int IndexOf(string columnName)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (string.Compare(columnName, list[i].ColumnName, ignoreCase: true, table.Locale) == 0)
			{
				return i;
			}
		}
		return -1;
	}

	public NavDataColumn Add(string columnName, Type type)
	{
		NavDataColumn navDataColumn = new NavDataColumn(columnName, type);
		Add(navDataColumn);
		return navDataColumn;
	}

	public NavDataColumn Add(string columnName, NavTypeCode typeCode)
	{
		NavDataColumn navDataColumn = new NavDataColumn(columnName, typeCode);
		Add(navDataColumn);
		return navDataColumn;
	}

	public void Add(NavDataColumn column)
	{
		if (column == null)
		{
			throw new ArgumentNullException("column");
		}
		if (column.Table == table)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Column '{0}' already belongs to this DataTable.", column.ColumnName), "column");
		}
		if (column.Table != null)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Column '{0}' already belongs to another DataTable.", column.ColumnName), "column");
		}
		if (table.Rows.Count > 0)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Column '{0}' cannot be added. There are already rows in the DataTable.", column.ColumnName), "column");
		}
		if (string.IsNullOrEmpty(column.ColumnName))
		{
			column.ColumnName = "Column" + list.Count.ToString(CultureInfo.InvariantCulture);
		}
		if (columnFromName.ContainsKey(column.ColumnName))
		{
			int num = 0;
			while (columnFromName.ContainsKey(column.ColumnName + "_" + ++num))
			{
			}
			column.ColumnName = column.ColumnName + "_" + num;
		}
		list.Add(column);
		RegisterColumnName(column.ColumnName, column);
		column.Table = table;
	}

	public void Clear()
	{
		list.Clear();
		columnFromName.Clear();
	}

	internal void RegisterColumnName(string colName, NavDataColumn column)
	{
		columnFromName.Add(colName, column);
	}

	internal void UnRegisterColumnName(string colName)
	{
		columnFromName.Remove(colName);
	}

	public bool Contains(NavDataColumn item)
	{
		return list.Contains(item);
	}

	public bool Contains(string columnName)
	{
		if (string.IsNullOrEmpty(columnName))
		{
			return false;
		}
		NavDataColumn value = null;
		return columnFromName.TryGetValue(columnName, out value);
	}

	public bool TryGetColumn(string columnName, out NavDataColumn column)
	{
		return columnFromName.TryGetValue(columnName, out column);
	}

	public void CopyTo(NavDataColumn[] array, int arrayIndex)
	{
		list.CopyTo(array, arrayIndex);
	}

	public bool Remove(NavDataColumn item)
	{
		if (list.Remove(item))
		{
			UnRegisterColumnName(item.ColumnName);
			item.Table = null;
			return true;
		}
		return false;
	}

	public IEnumerator<NavDataColumn> GetEnumerator()
	{
		return list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return list.GetEnumerator();
	}
}
