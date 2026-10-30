using System.Data.SqlClient;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavSqlConnectionInfo
{
	public string ConnectionString { get; }

	internal SqlCredential SqlCredential { get; }

	public NavSqlConnectionInfo(string connectionString, SqlCredential sqlCredential)
	{
		ConnectionString = connectionString;
		SqlCredential = sqlCredential;
	}

	public SqlConnection CreateConnection()
	{
		return new SqlConnection(ConnectionString, SqlCredential);
	}
}
