using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class DatabasePasswordPublicEncryptionKeyStorage : IDatabasePasswordPublicEncryptionKeyStorage
{
	private const string PublicEncryptionKeyTableName = "$ndo$publicencryptionkey";

	private const string CreatePublicEncryptionKeyTableStatement = "IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[{1}].[{2}]') AND OBJECTPROPERTY(id, N'IsTable') = 1) CREATE TABLE [{0}].[{1}].[{2}]( [id] int, [publickey] NVARCHAR(512)CONSTRAINT [PK${2}] PRIMARY KEY CLUSTERED([id]))";

	private const string SelectPublicKeyStatement = "SELECT TOP 1 [publickey] FROM [{0}].[{1}].[{2}]";

	private const string InsertPublicKeyStatement = "INSERT INTO [{0}].[{1}].[{2}] ([id], [publickey]) VALUES (@0, @1)";

	private const string OverwritePublicKeyStatement = "DELETE FROM [{0}].[{1}].[{2}] WHERE [id] = @0; INSERT INTO [{0}].[{1}].[{2}] ([id], [publickey]) VALUES (@0, @1)";

	private static void CheckTable(NavSqlConnectionScope conScope)
	{
		if (conScope.Connection.Database.DatabaseType != NavDatabaseType.Application)
		{
			throw new InvalidOperationException("Table should be stored in the app. database");
		}
		try
		{
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
			navSqlCommand.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[{1}].[{2}]') AND OBJECTPROPERTY(id, N'IsTable') = 1) CREATE TABLE [{0}].[{1}].[{2}]( [id] int, [publickey] NVARCHAR(512)CONSTRAINT [PK${2}] PRIMARY KEY CLUSTERED([id]))", conScope.Connection.Database.DatabaseName, "dbo", "$ndo$publicencryptionkey"));
		}
		catch (NavSqlException ex) when (((SqlException)ex.InnerException).Number == 2714)
		{
		}
	}

	public string GetPublicKey(NavDatabase database)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, updateOrlockingActivity: true);
		CheckTable(navSqlConnectionScope);
		return (string)NavSqlCommand.Create(navSqlConnectionScope.Connection).ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "SELECT TOP 1 [publickey] FROM [{0}].[{1}].[{2}]", database.DatabaseName, "dbo", "$ndo$publicencryptionkey"));
	}

	public void PutPublicKey(NavDatabase database, string key, bool overwrite)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, updateOrlockingActivity: true);
		CheckTable(navSqlConnectionScope);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.Int)).Value = 0;
		navSqlCommand.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.NVarChar, 512)).Value = key;
		navSqlCommand.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, overwrite ? "DELETE FROM [{0}].[{1}].[{2}] WHERE [id] = @0; INSERT INTO [{0}].[{1}].[{2}] ([id], [publickey]) VALUES (@0, @1)" : "INSERT INTO [{0}].[{1}].[{2}] ([id], [publickey]) VALUES (@0, @1)", database.DatabaseName, "dbo", "$ndo$publicencryptionkey"));
	}
}
