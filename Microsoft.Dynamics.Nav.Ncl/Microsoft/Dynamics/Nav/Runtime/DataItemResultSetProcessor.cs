using System;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class DataItemResultSetProcessor : IResultSetProcessor, IDisposable
{
	private NavDataTableWriter DataTableWriter { get; set; }

	protected internal int RowCounter { get; set; }

	public NavDataSet DataSet { get; protected set; }

	private void AddRowToTable()
	{
		if (DataTableWriter.AddCurrentRowToCollection(DataSet.Tables[0].Rows))
		{
			RowCounter++;
		}
		DataTableWriter.ResetRow();
	}

	protected DataItemResultSetProcessor(NavDataSet dataSet)
	{
		if (dataSet == null)
		{
			throw new ArgumentNullException("dataSet");
		}
		DataSet = dataSet;
		DataTableWriter = new NavDataTableWriter(dataSet.Tables[0]);
	}

	public virtual void SetValue(int columnIndex, object value)
	{
		DataTableWriter.SetValue(columnIndex, value);
	}

	public void SetValue(MetaDataItemColumn metaColumn, object value)
	{
		SetValue(metaColumn.DataSetIndex, value);
	}

	public void SetValue(MetaDataItemColumn metaColumn, object value, string decimalFormatter)
	{
		SetValue(metaColumn.DataSetIndex, value);
	}

	public void StartLevel(string elementName, string name)
	{
	}

	public void EndLevel()
	{
	}

	public virtual void Start()
	{
		RowCounter = 0;
		DataTableWriter.Clear();
		DataTableWriter.ResetRow();
		DataSet.Clear();
	}

	public virtual void Abort()
	{
	}

	public virtual void CleanupOnError()
	{
	}

	public void InsertRepeatedData(int[] columnIndexes)
	{
		DataTableWriter.InsertRepeatedData(columnIndexes);
	}

	public virtual void Finish()
	{
		DataTableWriter = null;
	}

	public virtual void EndOfRow()
	{
		AddRowToTable();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
	}
}
