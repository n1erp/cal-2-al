using System;

namespace Microsoft.Dynamics.Nav.Types.Data;

public class NavDataTableReader
{
	private NavDataRow lastGoodValueRow;

	private bool updateRow;

	public NavDataTableReader(NavDataTable table, bool updateRow)
	{
		lastGoodValueRow = table.NewRow();
		this.updateRow = updateRow;
	}

	public object GetValue(NavDataRow row, string fieldName)
	{
		return GetValue(row, row.Columns[fieldName].ColumnIndex);
	}

	public object GetValue(NavDataRow row, int index)
	{
		object obj = lastGoodValueRow[index];
		object obj2 = row[index];
		if (obj2 == DBNotChanged.Value)
		{
			if (updateRow)
			{
				row[index] = obj;
			}
			obj2 = obj;
		}
		else if (obj2 != DBNull.Value)
		{
			lastGoodValueRow[index] = obj2;
		}
		return obj2;
	}
}
