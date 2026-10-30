using System;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NullResultSetProcessor : IResultSetProcessor, IDisposable
{
	public void SetValue(int columnIndex, object value)
	{
	}

	public void SetValue(MetaDataItemColumn metaColumn, object value)
	{
	}

	public void SetValue(MetaDataItemColumn metaColumn, object value, string decimalFormatter)
	{
	}

	public void StartLevel(string elementName, string name)
	{
	}

	public void EndLevel()
	{
	}

	public void Start()
	{
	}

	public void InsertRepeatedData(int[] columnIndexes)
	{
	}

	public void Finish()
	{
	}

	public void EndOfRow()
	{
	}

	public void Abort()
	{
	}

	public void CleanupOnError()
	{
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}
}
