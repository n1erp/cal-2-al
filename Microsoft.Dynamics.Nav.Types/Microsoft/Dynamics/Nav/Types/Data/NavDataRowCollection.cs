using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types.Data;

[Serializable]
[DebuggerDisplay("Rows : {Count}")]
public class NavDataRowCollection : ICollection<NavDataRow>, IEnumerable<NavDataRow>, IEnumerable
{
	private readonly NavDataTable table;

	private readonly List<NavDataRow> list;

	public NavDataRow this[int index]
	{
		get
		{
			try
			{
				return list[index];
			}
			catch (ArgumentException innerException)
			{
				throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture, "There is no row at position {0}.", index), innerException);
			}
		}
	}

	public int Count => list.Count;

	public bool IsReadOnly => false;

	public NavDataRowCollection(NavDataTable table)
	{
		this.table = table;
		list = new List<NavDataRow>();
	}

	public void Add(NavDataRow row)
	{
		if (row == null)
		{
			throw new ArgumentNullException("row");
		}
		if (row.Table != table)
		{
			throw new ArgumentException("Row already belongs to another DataTable.", "row");
		}
		list.Add(row);
	}

	public void AddClone(NavDataRow row)
	{
		if (row == null)
		{
			throw new ArgumentNullException("row");
		}
		if (row.Table == table)
		{
			Add(row.Clone());
		}
		else
		{
			Add(row.CloneTo(table));
		}
	}

	public void Add(object[] rowValues)
	{
		NavDataRow navDataRow = table.NewRow();
		navDataRow.ItemArray = rowValues;
		list.Add(navDataRow);
	}

	public void Clear()
	{
		list.Clear();
	}

	public bool Contains(NavDataRow item)
	{
		return list.Contains(item);
	}

	public void CopyTo(NavDataRow[] array, int arrayIndex)
	{
		list.CopyTo(array, arrayIndex);
	}

	public bool Remove(NavDataRow item)
	{
		return list.Remove(item);
	}

	public IEnumerator<NavDataRow> GetEnumerator()
	{
		return list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return list.GetEnumerator();
	}
}
