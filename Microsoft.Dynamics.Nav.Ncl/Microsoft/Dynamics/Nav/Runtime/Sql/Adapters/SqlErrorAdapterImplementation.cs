using System.Data.SqlClient;

namespace Microsoft.Dynamics.Nav.Runtime.Sql.Adapters;

internal class SqlErrorAdapterImplementation : SqlErrorAdapter
{
	protected internal override int Number
	{
		get
		{
			if (SqlError != null)
			{
				return SqlError.Number;
			}
			return 0;
		}
	}

	protected internal SqlErrorAdapterImplementation()
	{
	}

	protected internal SqlErrorAdapterImplementation(SqlError sqlError)
		: base(sqlError)
	{
	}
}
