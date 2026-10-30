using System;
using System.IO;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class SaveAsRenderer : IResultSetProcessor, IDisposable
{
	private NavDataSet dataSet;

	private NavDataTableWriter dataTableWriter;

	private string fileName;

	private NavOutStream outStream;

	private bool disposed;

	public NavDataSet DataSet => dataSet;

	public string FileName => fileName;

	public NavOutStream OutStream => outStream;

	protected NavDataTableWriter DataTableWriter
	{
		get
		{
			return dataTableWriter;
		}
		set
		{
			dataTableWriter = value;
		}
	}

	protected SaveAsRenderer(string fileName, NavDataSet dataSet)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			throw new ArgumentNullException("fileName");
		}
		if (dataSet == null)
		{
			throw new ArgumentNullException("dataSet");
		}
		this.dataSet = dataSet;
		this.fileName = NavFile.ExpandFileName(DataError.ThrowError, fileName);
		dataTableWriter = new NavDataTableWriter(this.dataSet.Tables[0]);
	}

	protected SaveAsRenderer(NavOutStream outStream, NavDataSet dataSet)
	{
		if (outStream == null)
		{
			throw new ArgumentNullException("outStream");
		}
		if (dataSet == null)
		{
			throw new ArgumentNullException("dataSet");
		}
		this.dataSet = dataSet;
		this.outStream = outStream;
		dataTableWriter = new NavDataTableWriter(this.dataSet.Tables[0]);
	}

	public virtual void SetValue(int columnIndex, object value)
	{
		dataTableWriter.SetValue(columnIndex, value);
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
		dataTableWriter.Clear();
		dataTableWriter.ResetRow();
		dataSet.Clear();
	}

	public void InsertRepeatedData(int[] columnIndexes)
	{
		dataTableWriter.InsertRepeatedData(columnIndexes);
	}

	private void AddRowToTable()
	{
		dataTableWriter.DecompressRow();
		dataTableWriter.AddCurrentRowToCollection(DataSet.Tables[0].Rows);
		dataTableWriter.ResetRow();
	}

	public virtual void Finish()
	{
		dataTableWriter = null;
		Render();
	}

	public virtual void EndOfRow()
	{
		AddRowToTable();
	}

	public void Abort()
	{
		dataSet = null;
	}

	protected abstract void Render();

	protected virtual void Dispose(bool disposing)
	{
		if (!disposed)
		{
			disposed = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public void CleanupOnError()
	{
		Dispose(disposing: true);
		if (File.Exists(fileName))
		{
			File.Delete(fileName);
		}
	}
}
