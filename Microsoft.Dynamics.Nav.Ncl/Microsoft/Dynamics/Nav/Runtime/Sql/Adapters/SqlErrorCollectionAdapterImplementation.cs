using System.Data.SqlClient;

namespace Microsoft.Dynamics.Nav.Runtime.Sql.Adapters;

internal class SqlErrorCollectionAdapterImplementation : SqlErrorCollectionAdapter
{
	protected internal override int Count
	{
		get
		{
			if (SqlErrorCollection != null)
			{
				return SqlErrorCollection.Count;
			}
			return 0;
		}
	}

	protected internal override SqlErrorAdapter this[int index]
	{
		get
		{
			if (SqlErrorCollection != null)
			{
				return new SqlErrorAdapterImplementation(SqlErrorCollection[index]);
			}
			return null;
		}
	}

	protected internal SqlErrorCollectionAdapterImplementation()
	{
	}

	protected internal SqlErrorCollectionAdapterImplementation(SqlErrorCollection sqlErrorCollection)
		: base(sqlErrorCollection)
	{
	}
}
