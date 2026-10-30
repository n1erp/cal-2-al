using System;

namespace Microsoft.Dynamics.Nav.Types.Data;

public class NavDataTableWriter
{
	private NavDataTable table;

	private NavDataRow lastGoodValueRow;

	private readonly object[] currentRow;

	public object[] CurrentRow => currentRow;

	public NavDataTableWriter(NavDataTable table)
	{
		this.table = table;
		lastGoodValueRow = table.NewRow();
		int count = table.Columns.Count;
		currentRow = new object[count];
		for (int i = 0; i < count; i++)
		{
			currentRow[i] = DBNull.Value;
		}
	}

	public void SetValue(string fieldName, object value)
	{
		SetValue(lastGoodValueRow.Columns[fieldName].ColumnIndex, value);
	}

	public void InsertRepeatedData(int[] columnIndexes)
	{
		for (int i = 0; i < columnIndexes.Length; i++)
		{
			currentRow[columnIndexes[i]] = DBNotChanged.Value;
		}
	}

	public void SetValue(int index, object value)
	{
		object obj = lastGoodValueRow[index];
		if (obj != null && ObjectEquals(obj, value))
		{
			currentRow[index] = DBNotChanged.Value;
			return;
		}
		if (value != DBNull.Value)
		{
			lastGoodValueRow[index] = value;
		}
		currentRow[index] = value;
	}

	private static bool ObjectEquals(object lastValue, object value)
	{
		if (lastValue is byte[] binary)
		{
			return ByteArrayHelper.Compare(binary, value as byte[]);
		}
		return object.Equals(lastValue, value);
	}

	public void Clear()
	{
		lastGoodValueRow = table.NewRow();
	}

	public void ResetRow()
	{
		for (int i = 0; i < currentRow.Length; i++)
		{
			currentRow[i] = DBNull.Value;
		}
	}

	public bool AddCurrentRowToCollection(NavDataRowCollection rowCollection)
	{
		bool flag = true;
		for (int i = 0; i < currentRow.Length; i++)
		{
			if (currentRow[i] != DBNull.Value)
			{
				flag = false;
				break;
			}
		}
		if (!flag)
		{
			rowCollection.Add(CurrentRow);
			return true;
		}
		return false;
	}

	public void DecompressRow()
	{
		for (int i = 0; i < currentRow.Length; i++)
		{
			if (currentRow[i] == DBNotChanged.Value)
			{
				currentRow[i] = lastGoodValueRow[i];
			}
		}
	}
}
