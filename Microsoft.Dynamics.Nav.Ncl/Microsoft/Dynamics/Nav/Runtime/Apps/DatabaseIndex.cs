using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class DatabaseIndex
{
	private readonly List<string> allColumns;

	public string Name { get; }

	public IEnumerable<string> ColumnNames => allColumns.OrderBy((string column) => column);

	public DatabaseIndex(string name, IEnumerable<string> columnNames)
	{
		Name = name;
		allColumns = columnNames?.ToList();
	}

	public void RemoveColumns(string columnName)
	{
		allColumns.RemoveAll((string name) => name.Equals(columnName));
	}
}
