using System;
using System.Collections;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Types.Data;

[Serializable]
[DebuggerDisplay("{columnName}")]
public class NavDataColumn
{
	private string columnName;

	private Type dataType;

	private NavDataTable table;

	private Hashtable extendedProperties;

	[NonSerialized]
	private int tableColumnIndex = -1;

	public bool HasExtendedProperties
	{
		get
		{
			if (extendedProperties != null)
			{
				return extendedProperties.Count > 0;
			}
			return false;
		}
	}

	public Hashtable ExtendedProperties
	{
		get
		{
			if (extendedProperties == null)
			{
				extendedProperties = new Hashtable();
			}
			return extendedProperties;
		}
	}

	public string ColumnName
	{
		get
		{
			return columnName;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			if (table != null)
			{
				if (value.Length == 0)
				{
					throw new ArgumentException("NavColumnName is required when it is part of a NavDataTable.", "value");
				}
				if (string.Compare(columnName, value, ignoreCase: true, table.Locale) != 0)
				{
					table.Columns.RegisterColumnName(value, this);
					if (columnName.Length != 0)
					{
						table.Columns.UnRegisterColumnName(columnName);
					}
				}
			}
			columnName = value;
		}
	}

	public Type DataType
	{
		get
		{
			return dataType;
		}
		set
		{
			dataType = value;
			if (value != null)
			{
				NavTypeCode = NavSerializer.GetTypeCode(value.TypeHandle);
			}
		}
	}

	public NavTypeCode NavTypeCode { get; private set; }

	internal NavDataTable Table
	{
		get
		{
			return table;
		}
		set
		{
			table = value;
		}
	}

	public int ColumnIndex
	{
		get
		{
			if (table == null)
			{
				return -1;
			}
			if (tableColumnIndex == -1)
			{
				tableColumnIndex = table.Columns.IndexOf(this);
			}
			else if (tableColumnIndex >= table.Columns.Count || table.Columns[tableColumnIndex].ColumnName != ColumnName)
			{
				tableColumnIndex = table.Columns.IndexOf(this);
			}
			return tableColumnIndex;
		}
	}

	public NavDataColumn()
		: this(null, null, null)
	{
	}

	public NavDataColumn(string columnName, NavTypeCode typeCode)
		: this(columnName, NavSerializer.GetType(typeCode), typeCode, null)
	{
	}

	public NavDataColumn(string columnName, Type dataType)
		: this(columnName, dataType, null)
	{
	}

	public NavDataColumn(string columnName, Type dataType, NavDataTable table)
	{
		this.columnName = columnName ?? string.Empty;
		DataType = dataType ?? typeof(string);
		this.table = table;
	}

	internal NavDataColumn(string columnName, Type dataType, NavTypeCode typeCode, NavDataTable table)
	{
		this.columnName = columnName ?? string.Empty;
		if (dataType == null)
		{
			DataType = typeof(string);
		}
		else
		{
			this.dataType = dataType;
			NavTypeCode = typeCode;
		}
		this.table = table;
	}

	public NavDataColumn Clone()
	{
		NavDataColumn navDataColumn = new NavDataColumn(columnName, dataType, NavTypeCode, table);
		navDataColumn.tableColumnIndex = tableColumnIndex;
		if (HasExtendedProperties)
		{
			navDataColumn.extendedProperties = (Hashtable)ExtendedProperties.Clone();
		}
		return navDataColumn;
	}
}
