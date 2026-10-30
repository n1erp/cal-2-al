using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types.Data;

[Serializable]
[DebuggerDisplay("{TableName}")]
public class NavDataTable
{
	private readonly NavDataColumnCollection columns;

	private readonly NavDataRowCollection rows;

	private CultureInfo culture;

	private IFormatProvider formatProvider;

	[NonSerialized]
	private NavDataSet dataSet;

	internal IFormatProvider FormatProvider
	{
		get
		{
			if (formatProvider == null)
			{
				if (culture.IsNeutralCulture)
				{
					formatProvider = CultureInfo.InvariantCulture;
				}
				else
				{
					formatProvider = culture;
				}
			}
			return formatProvider;
		}
	}

	internal NavDataSet DataSet
	{
		get
		{
			return dataSet;
		}
		set
		{
			dataSet = value;
		}
	}

	public CultureInfo Locale
	{
		get
		{
			return culture;
		}
		set
		{
			if (value != null && !culture.Equals(value) && dataSet != null)
			{
				culture = value;
			}
		}
	}

	public string TableName { get; set; }

	public NavDataColumnCollection Columns
	{
		[DebuggerStepperBoundary]
		get
		{
			return columns;
		}
	}

	public virtual NavDataRowCollection Rows => rows;

	public NavDataTable()
		: this(string.Empty)
	{
	}

	public NavDataTable(string tableName)
	{
		culture = CultureInfo.CurrentCulture;
		columns = new NavDataColumnCollection(this);
		rows = new NavDataRowCollection(this);
		TableName = tableName ?? string.Empty;
	}

	public DataTable GetDataTable(bool mediaColumnsAsByteArray = false)
	{
		DataTable dataTable = BuildDataTable(mediaColumnsAsByteArray);
		NavDataTableReader navDataTableReader = new NavDataTableReader(this, updateRow: false);
		foreach (NavDataRow row in Rows)
		{
			DataRow dataRow = dataTable.NewRow();
			int count = Columns.Count;
			for (int i = 0; i < count; i++)
			{
				dataRow[i] = navDataTableReader.GetValue(row, i);
			}
			dataTable.Rows.Add(dataRow);
		}
		return dataTable;
	}

	public DataTable BuildDataTable()
	{
		return BuildDataTable(mediaColumnsAsByteArray: false);
	}

	public DataTable BuildDataTable(bool mediaColumnsAsByteArray)
	{
		DataTable dataTable = new DataTable(TableName)
		{
			Locale = Locale
		};
		foreach (NavDataColumn column in Columns)
		{
			Type dataType;
			if (column.NavTypeCode != NavTypeCode.Media && column.NavTypeCode != NavTypeCode.MediaSet)
			{
				dataType = column.DataType;
			}
			else
			{
				dataType = (mediaColumnsAsByteArray ? typeof(byte[]) : typeof(Guid));
			}
			DataColumn dataColumn = new DataColumn(column.ColumnName, dataType);
			if (column.DataType == typeof(DateTime))
			{
				dataColumn.DateTimeMode = DataSetDateTime.Local;
			}
			if (column.HasExtendedProperties)
			{
				foreach (object key in column.ExtendedProperties.Keys)
				{
					dataColumn.ExtendedProperties.Add(key, column.ExtendedProperties[key]);
				}
			}
			dataTable.Columns.Add(dataColumn);
		}
		return dataTable;
	}

	public void Clear()
	{
		Rows.Clear();
	}

	public NavDataRow NewRow()
	{
		return new NavDataRow(this);
	}

	public NavDataRow NewRow(object[] rowData)
	{
		return new NavDataRow(this, rowData);
	}

	public void Decompress(NavDataRow rowToInit)
	{
		NavDataRow navDataRow = rowToInit;
		foreach (NavDataRow row in Rows)
		{
			if (navDataRow == null)
			{
				navDataRow = row;
				continue;
			}
			for (int i = 0; i < row.Columns.Count - 1; i++)
			{
				if (row[i] == DBNotChanged.Value)
				{
					row[i] = navDataRow[i];
				}
			}
			navDataRow = row;
		}
	}

	public NavDataTable Clone()
	{
		NavDataTable navDataTable = new NavDataTable(TableName);
		navDataTable.Locale = Locale;
		navDataTable.formatProvider = FormatProvider;
		int count = Columns.Count;
		for (int i = 0; i < count; i++)
		{
			NavDataColumn navDataColumn = Columns[i].Clone();
			navDataColumn.Table = null;
			navDataTable.Columns.Add(navDataColumn);
		}
		return navDataTable;
	}

	public NavDataTable Copy()
	{
		NavDataTable navDataTable = Clone();
		int i = 0;
		for (int count = Rows.Count; i < count; i++)
		{
			navDataTable.Rows.AddClone(Rows[i]);
		}
		return navDataTable;
	}
}
