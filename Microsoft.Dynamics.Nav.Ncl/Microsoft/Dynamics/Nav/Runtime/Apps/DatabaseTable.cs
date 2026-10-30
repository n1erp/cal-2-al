using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class DatabaseTable
{
	private readonly List<DatabaseColumn> allColumns;

	private readonly List<DatabaseIndex> allIndexes;

	private readonly List<string> allConstraints;

	public string Name { get; }

	public IEnumerable<DatabaseColumn> ColumnNames => allColumns;

	public IEnumerable<DatabaseIndex> Indexes => allIndexes;

	public IEnumerable<string> DefaultConstraints => allConstraints;

	public DatabaseTable(string name, IEnumerable<DatabaseColumn> columnNames, IEnumerable<DatabaseIndex> indexes, IEnumerable<string> defaultConstraints)
	{
		Name = name;
		allColumns = columnNames?.ToList();
		allIndexes = indexes?.ToList();
		allConstraints = defaultConstraints?.ToList();
	}

	public void RemoveColumns(string columnName)
	{
		allColumns.RemoveAll((DatabaseColumn column) => column.Name.Equals(columnName, StringComparison.CurrentCultureIgnoreCase));
	}

	public void RemoveIndex(DatabaseIndex index)
	{
		allIndexes.Remove(index);
	}
}
