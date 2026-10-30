using System;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface IResultSetProcessor : IDisposable
{
	void SetValue(int columnIndex, object value);

	void SetValue(MetaDataItemColumn metaColumn, object value);

	void SetValue(MetaDataItemColumn metaColumn, object value, string decimalFormatter);

	void StartLevel(string elementName, string name);

	void EndLevel();

	void Start();

	void InsertRepeatedData(int[] columnIndexes);

	void Finish();

	void EndOfRow();

	void Abort();

	void CleanupOnError();
}
