using System.Data.SqlClient;

namespace Microsoft.Dynamics.Nav.Runtime.Sql.Adapters;

internal abstract class SqlErrorAdapter
{
	protected internal SqlError SqlError { get; private set; }

	protected internal abstract int Number { get; }

	protected internal SqlErrorAdapter()
	{
		SqlError = null;
	}

	protected internal SqlErrorAdapter(SqlError sqlError)
	{
		SqlError = sqlError;
	}
}
