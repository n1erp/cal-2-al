using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Media;

namespace Microsoft.Dynamics.Nav.Types.Data;

[Serializable]
public class NavDataRow : ICustomTypeDescriptor
{
	private NavDataTable table;

	private object[] items;

	public NavDataTable Table => table;

	public NavDataColumnCollection Columns => table.Columns;

	public object this[int columnIndex]
	{
		get
		{
			return items[columnIndex];
		}
		set
		{
			NavDataColumn column = Columns[columnIndex];
			SetValue(column, columnIndex, value);
		}
	}

	public object this[string columnName]
	{
		get
		{
			NavDataColumn column = Columns[columnName];
			return this[column];
		}
		set
		{
			NavDataColumn column = Columns[columnName];
			this[column] = value;
		}
	}

	public object this[NavDataColumn column]
	{
		get
		{
			Check(column);
			return items[column.ColumnIndex];
		}
		set
		{
			Check(column);
			SetValue(column, column.ColumnIndex, value);
		}
	}

	public object[] ItemArray
	{
		get
		{
			return (object[])items.Clone();
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (Columns.Count != value.Length)
			{
				throw new ArgumentException("The array is larger than the number of columns in the table.", "value");
			}
			for (int i = 0; i < value.Length; i++)
			{
				SetValue(Columns[i], i, value[i]);
			}
		}
	}

	private NavDataRow()
	{
	}

	internal NavDataRow(NavDataTable table)
	{
		this.table = table;
		items = new object[table.Columns.Count];
		for (int i = 0; i < items.Length; i++)
		{
			items[i] = DBNull.Value;
		}
	}

	internal NavDataRow(NavDataTable table, object[] rowData)
	{
		if (table.Columns.Count != rowData.Length)
		{
			throw new ArgumentException("The array is larger than the number of columns in the table.", "rowData");
		}
		this.table = table;
		items = (object[])rowData.Clone();
	}

	public bool IsNull(NavDataColumn column)
	{
		Check(column);
		return items[column.ColumnIndex] == DBNull.Value;
	}

	private void SetValue(NavDataColumn column, int columnIndex, object value)
	{
		if (value == null || value == DBNull.Value)
		{
			items[columnIndex] = DBNull.Value;
			return;
		}
		if (value == DBNotChanged.Value)
		{
			items[columnIndex] = value;
			return;
		}
		switch (column.NavTypeCode)
		{
		case NavTypeCode.Boolean:
			if (value.GetType() != typeof(bool))
			{
				value = Convert.ToBoolean(value, table.FormatProvider);
			}
			break;
		case NavTypeCode.Char:
			if (value.GetType() != typeof(char))
			{
				value = Convert.ToChar(value, table.FormatProvider);
			}
			break;
		case NavTypeCode.SByte:
			if (value.GetType() != typeof(sbyte))
			{
				value = Convert.ToSByte(value, table.FormatProvider);
			}
			break;
		case NavTypeCode.Byte:
			if (value.GetType() != typeof(byte))
			{
				value = Convert.ToByte(value, table.FormatProvider);
			}
			break;
		case NavTypeCode.Int16:
			if (value.GetType() != typeof(short))
			{
				value = Convert.ToInt16(value, table.FormatProvider);
			}
			break;
		case NavTypeCode.UInt16:
			if (value.GetType() != typeof(ushort))
			{
				value = Convert.ToUInt16(value, table.FormatProvider);
			}
			break;
		case NavTypeCode.Int32:
			if (value.GetType() != typeof(int))
			{
				value = Convert.ToInt32(value, table.FormatProvider);
			}
			break;
		case NavTypeCode.UInt32:
			if (value.GetType() != typeof(uint))
			{
				value = Convert.ToUInt32(value, table.FormatProvider);
			}
			break;
		case NavTypeCode.Int64:
			if (value.GetType() != typeof(long))
			{
				value = Convert.ToInt64(value, table.FormatProvider);
			}
			break;
		case NavTypeCode.UInt64:
			if (value.GetType() != typeof(ulong))
			{
				value = Convert.ToUInt64(value, table.FormatProvider);
			}
			break;
		case NavTypeCode.Single:
			if (value.GetType() != typeof(float))
			{
				value = Convert.ToSingle(value, table.FormatProvider);
			}
			break;
		case NavTypeCode.Double:
			if (value.GetType() != typeof(double))
			{
				value = Convert.ToDouble(value, table.FormatProvider);
			}
			break;
		case NavTypeCode.Decimal:
			if (value.GetType() != typeof(decimal))
			{
				value = Convert.ToDecimal(value, table.FormatProvider);
			}
			break;
		case NavTypeCode.DateTime:
			if (value.GetType() != typeof(DateTime))
			{
				value = Convert.ToDateTime(value, table.FormatProvider);
			}
			break;
		case NavTypeCode.String:
			if (value.GetType() != typeof(string))
			{
				value = Convert.ToString(value, table.FormatProvider);
			}
			break;
		case NavTypeCode.Guid:
			if (!(value.GetType() != typeof(Guid)))
			{
				break;
			}
			if (value is byte[] array)
			{
				value = ((array.Length == 0) ? Guid.Empty : new Guid(array));
				break;
			}
			goto default;
		case NavTypeCode.TimeSpan:
		{
			if (!(value.GetType() != typeof(TimeSpan)))
			{
				break;
			}
			long? num = value as long?;
			if (num.HasValue)
			{
				value = new TimeSpan(num.Value);
				break;
			}
			goto default;
		}
		case NavTypeCode.Media:
			if (value.GetType() == typeof(NavMediaDescriptionCollection))
			{
				value = new NSMediaWrapper((NavMediaDescriptionCollection)value);
				break;
			}
			if (value is Guid)
			{
				value = new NSMediaWrapper((Guid)value);
				break;
			}
			if (value is string)
			{
				value = new NSMediaWrapper(new Guid((string)value));
				break;
			}
			if (value is byte[] || !(value.GetType() != typeof(NSMediaWrapper)))
			{
				break;
			}
			goto default;
		case NavTypeCode.MediaSet:
			if (value.GetType() == typeof(NavMediaDescriptionCollection))
			{
				value = new NSMediaSetWrapper((NavMediaDescriptionCollection)value);
				break;
			}
			if (value is Guid)
			{
				value = new NSMediaSetWrapper((Guid)value);
				break;
			}
			if (value is string)
			{
				value = new NSMediaSetWrapper(new Guid((string)value));
				break;
			}
			if (value is byte[] || !(value.GetType() != typeof(NSMediaSetWrapper)))
			{
				break;
			}
			goto default;
		case NavTypeCode.ByteArray:
			if (!(value.GetType() != typeof(byte[])))
			{
				break;
			}
			goto default;
		default:
			value = Convert.ChangeType(value, column.DataType, table.FormatProvider);
			break;
		}
		items[columnIndex] = value;
	}

	internal void SetDeserializedValue(int columnIndex, object value)
	{
		items[columnIndex] = value;
	}

	public NavDataRow Clone()
	{
		return new NavDataRow
		{
			table = table,
			items = (object[])items.Clone()
		};
	}

	internal NavDataRow CloneTo(NavDataTable destinationTable)
	{
		return new NavDataRow
		{
			table = destinationTable,
			items = (object[])items.Clone()
		};
	}

	private void Check(NavDataColumn column)
	{
		if (column == null)
		{
			throw new ArgumentNullException("column");
		}
		if (column.Table != table)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Column '{0}' does not belong to table {1}.", column.ColumnName, table.TableName), "column");
		}
	}

	public PropertyDescriptorCollection GetProperties()
	{
		NavDataTableReader dataTableReader = new NavDataTableReader(Table, ZoomToolState.UseZoomToolForReports);
		PropertyDescriptorCollection propertyDescriptorCollection = new PropertyDescriptorCollection(null);
		foreach (NavDataColumn column in Table.Columns)
		{
			propertyDescriptorCollection.Add(new NavDataColumnPropertyDescriptor(dataTableReader, column));
		}
		return propertyDescriptorCollection;
	}

	public void WriteXml(XmlWriter xmlWriter, NavDataTableReader rowReader)
	{
		xmlWriter.WriteStartElement(Table.TableName);
		for (int i = 0; i < items.Length; i++)
		{
			object obj = ((rowReader != null) ? rowReader.GetValue(this, i) : items[i]);
			if (obj != null && !(obj is DBNull))
			{
				xmlWriter.WriteElementString(Table.Columns[i].ColumnName, NavSerializer.PrepareValueForXmlOutput(obj, table.Columns[i].NavTypeCode));
			}
		}
		xmlWriter.WriteEndElement();
	}

	public void WriteXml(XmlWriter xmlWriter)
	{
		WriteXml(xmlWriter, null);
	}

	public void WriteCsv(TextWriter streamWriter, string delimiter, bool writeColumnNames, bool positionFixed, int[] positions)
	{
		int i = 1;
		if (positionFixed && (positions == null || positions.Length < items.Length))
		{
			throw new NavNCLQuerySaveAsException(string.Format(CultureInfo.CurrentCulture, Lang.QueryOutputCouldNotBeSaved, "Invalid argument: Positions"));
		}
		for (int j = 0; j < items.Length; j++)
		{
			string text;
			if (writeColumnNames)
			{
				text = Table.Columns[j].ColumnName;
			}
			else
			{
				text = ((items[j] == null || items[j] is DBNull) ? "0" : NavSerializer.PrepareValueForOutput(items[j], table.Columns[j].NavTypeCode));
			}
			if (positionFixed)
			{
				for (; i < positions[j]; i++)
				{
					streamWriter.Write(' ');
				}
				if (j < items.Length - 1)
				{
					int num = positions[j + 1] - positions[j];
					if (text.Length <= num)
					{
						streamWriter.Write(text);
						i += text.Length;
					}
					else if (num > 0)
					{
						streamWriter.Write(text.Substring(0, num));
						i += num;
					}
				}
				else
				{
					streamWriter.Write(text);
				}
			}
			else
			{
				bool flag;
				if (flag = text.Contains("\""))
				{
					text = text.Replace("\"", "\"\"");
				}
				streamWriter.Write((flag || text.Contains(delimiter)) ? ("\"" + text + "\"") : text);
				if (j < items.Length - 1)
				{
					streamWriter.Write(delimiter);
				}
			}
		}
		streamWriter.WriteLine();
	}

	public AttributeCollection GetAttributes()
	{
		throw new NotImplementedException("The method or operation is not implemented.");
	}

	public string GetClassName()
	{
		throw new NotImplementedException("The method or operation is not implemented.");
	}

	public string GetComponentName()
	{
		throw new NotImplementedException("The method or operation is not implemented.");
	}

	public TypeConverter GetConverter()
	{
		throw new NotImplementedException("The method or operation is not implemented.");
	}

	public EventDescriptor GetDefaultEvent()
	{
		throw new NotImplementedException("The method or operation is not implemented.");
	}

	public PropertyDescriptor GetDefaultProperty()
	{
		throw new NotImplementedException("The method or operation is not implemented.");
	}

	public object GetEditor(Type editorBaseType)
	{
		throw new NotImplementedException("The method or operation is not implemented.");
	}

	public EventDescriptorCollection GetEvents(Attribute[] attributes)
	{
		throw new NotImplementedException("The method or operation is not implemented.");
	}

	public EventDescriptorCollection GetEvents()
	{
		throw new NotImplementedException("The method or operation is not implemented.");
	}

	public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
	{
		throw new NotImplementedException("The method or operation is not implemented.");
	}

	public object GetPropertyOwner(PropertyDescriptor pd)
	{
		throw new NotImplementedException("The method or operation is not implemented.");
	}
}
