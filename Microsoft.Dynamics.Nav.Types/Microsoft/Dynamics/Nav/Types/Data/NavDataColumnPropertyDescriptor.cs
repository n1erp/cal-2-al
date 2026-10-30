using System;
using System.ComponentModel;

namespace Microsoft.Dynamics.Nav.Types.Data;

public class NavDataColumnPropertyDescriptor : PropertyDescriptor
{
	private NavDataTableReader dataTableReader;

	private int columnIndex;

	public override Type ComponentType => null;

	public override bool IsReadOnly => false;

	public override Type PropertyType => null;

	public NavDataColumnPropertyDescriptor(NavDataTableReader dataTableReader, NavDataColumn column)
		: base(column.ColumnName, null)
	{
		columnIndex = column.ColumnIndex;
		this.dataTableReader = dataTableReader;
	}

	public override object GetValue(object component)
	{
		NavDataRow row = (NavDataRow)component;
		return dataTableReader.GetValue(row, columnIndex);
	}

	public override bool CanResetValue(object component)
	{
		throw new NotImplementedException("The method or operation is not implemented.");
	}

	public override void ResetValue(object component)
	{
		throw new NotImplementedException("The method or operation is not implemented.");
	}

	public override void SetValue(object component, object value)
	{
		throw new NotImplementedException("The method or operation is not implemented.");
	}

	public override bool ShouldSerializeValue(object component)
	{
		throw new NotImplementedException("The method or operation is not implemented.");
	}
}
