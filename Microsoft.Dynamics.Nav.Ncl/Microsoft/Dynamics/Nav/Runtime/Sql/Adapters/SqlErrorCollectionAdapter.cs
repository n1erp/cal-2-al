using System.Data.SqlClient;

namespace Microsoft.Dynamics.Nav.Runtime.Sql.Adapters;

internal abstract class SqlErrorCollectionAdapter
{
	protected internal SqlErrorCollection SqlErrorCollection { get; private set; }

	protected internal abstract int Count { get; }

	protected internal abstract SqlErrorAdapter this[int index] { get; }

	protected internal SqlErrorCollectionAdapter()
	{
		SqlErrorCollection = null;
	}

	protected internal SqlErrorCollectionAdapter(SqlErrorCollection sqlErrorCollection)
	{
		SqlErrorCollection = sqlErrorCollection;
	}
}
