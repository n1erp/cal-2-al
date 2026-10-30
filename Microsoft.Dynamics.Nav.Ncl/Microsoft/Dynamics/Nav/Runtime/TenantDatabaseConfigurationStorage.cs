using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class TenantDatabaseConfigurationStorage
{
	internal const string TableName = "$ndo$tenantdatabases";

	private const int IdMaxLength = 128;

	private const int UserNameMaxLength = 128;

	private const int PasswordMaxLength = 1024;

	private const int DatabaseServerMaxLength = 128;

	private const int DatabaseNameMaxLength = 128;

	private const string CaseInsensitiveCollation = "Latin1_General_100_CI_AS";

	private const string CheckTenantDatabaseTableStatement = "SELECT CASE WHEN EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[{0}].[{1}].[{2}]') AND OBJECTPROPERTY(id, N'IsTable') = 1) THEN 1 ELSE 0 END";

	private const string CreateTenantDatabaseTableStatement = "\r\n            CREATE TABLE [{0}].[{1}].[{2}]\r\n            (\r\n                [timestamp] TIMESTAMP,\r\n                [id] NVARCHAR(128) NOT NULL,\r\n                [databaseserver] NVARCHAR(128) COLLATE {3},\r\n                [databasename] NVARCHAR(128) COLLATE {3},\r\n                [databaseusername] NVARCHAR(128),\r\n                [databasepassword] NVARCHAR(1024),\r\n                CONSTRAINT [PK{2}] PRIMARY KEY CLUSTERED (id)\r\n            )\r\n            CREATE UNIQUE INDEX [UC_DATABASE] ON [{0}].[{1}].[{2}] (databaseserver, databasename);";

	private const string SelectStatementStatement = "SELECT [timestamp], [id], [databaseserver], [databasename], [databaseusername], [databasepassword] FROM [{0}].[{1}].[{2}]";

	private const string InsertStatemnt = "INSERT INTO [{0}].[{1}].[{2}] ([id], [databaseserver], [databasename], [databaseusername], [databasepassword]) OUTPUT INSERTED.[timestamp] VALUES (@0,@1,@2,@3,@4)";

	private const string GetTenantDatabaseWithLockStatement = "SELECT [timestamp], [id], [databaseserver], [databasename], [databaseusername], [databasepassword] FROM [{0}].[{1}].[{2}] WITH(UPDLOCK) WHERE [id] = @0";

	private const string GetTenantDatabaseStatement = "SELECT [timestamp], [id], [databaseserver], [databasename], [databaseusername], [databasepassword] FROM [{0}].[{1}].[{2}] WITH(READUNCOMMITTED) WHERE [id] = @0";

	private const string DeleteStatement = "DELETE FROM [{0}].[{1}].[{2}] WHERE [id] = @0";

	private const string TruncateStatement = "TRUNCATE TABLE [{0}].[{1}].[{2}]";

	private const string CountStatement = "SELECT COUNT(*) FROM [{0}].[{1}].[{2}]";

	private readonly NavDatabase applicationDatabase;

	private bool tableChecked;

	public int TenantDatabaseCount
	{
		get
		{
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(applicationDatabase, updateOrlockingActivity: true);
			CheckTable(navSqlConnectionScope);
			return navSqlConnectionScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("CountTenantDatabases", applicationDatabase), () => string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM [{0}].[{1}].[{2}]", applicationDatabase.DatabaseName, "dbo", "$ndo$tenantdatabases"), (NavSqlCommand cmd) => ((int?)cmd.ExecuteScalar()).GetValueOrDefault());
		}
	}

	public TenantDatabaseConfigurationStorage(NavDatabase applicationDatabase)
	{
		if (applicationDatabase == null)
		{
			throw new ArgumentNullException("applicationDatabase");
		}
		this.applicationDatabase = applicationDatabase;
	}

	private void CheckTable(NavSqlConnectionScope conScope)
	{
		if (tableChecked)
		{
			return;
		}
		int num;
		using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection))
		{
			num = (int)navSqlCommand.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, "SELECT CASE WHEN EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[{0}].[{1}].[{2}]') AND OBJECTPROPERTY(id, N'IsTable') = 1) THEN 1 ELSE 0 END", applicationDatabase.DatabaseName, "dbo", "$ndo$tenantdatabases"));
		}
		if (num == 0)
		{
			try
			{
				using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(conScope.Connection.Database, TransactionType.Update);
				using NavSqlCommand navSqlCommand2 = NavSqlCommand.Create(navSqlConnectionScope.Connection);
				navSqlCommand2.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "\r\n            CREATE TABLE [{0}].[{1}].[{2}]\r\n            (\r\n                [timestamp] TIMESTAMP,\r\n                [id] NVARCHAR(128) NOT NULL,\r\n                [databaseserver] NVARCHAR(128) COLLATE {3},\r\n                [databasename] NVARCHAR(128) COLLATE {3},\r\n                [databaseusername] NVARCHAR(128),\r\n                [databasepassword] NVARCHAR(1024),\r\n                CONSTRAINT [PK{2}] PRIMARY KEY CLUSTERED (id)\r\n            )\r\n            CREATE UNIQUE INDEX [UC_DATABASE] ON [{0}].[{1}].[{2}] (databaseserver, databasename);", applicationDatabase.DatabaseName, "dbo", "$ndo$tenantdatabases", "Latin1_General_100_CI_AS"));
			}
			catch (NavSqlException ex) when (((Func<bool>)delegate
			{
				// Could not convert BlockContainer to single expression
				SqlException ex2 = (SqlException)ex.InnerException;
				return ex2 != null && ex2.Number == 2714;
			}).Invoke())
			{
			}
		}
		tableChecked = true;
	}

	private static TenantDatabaseConfiguration GetTenantDatabaseFromReader(SqlDataReader reader)
	{
		byte[] timestamp = (byte[])reader.GetValue(0);
		string id = reader.GetString(1);
		string databaseServer = (reader.IsDBNull(2) ? null : reader.GetString(2));
		string databaseName = (reader.IsDBNull(3) ? null : reader.GetString(3));
		string databaseUserName = (reader.IsDBNull(4) ? null : reader.GetString(4));
		ProtectedDatabasePassword databasePassword = (reader.IsDBNull(5) ? null : new ProtectedDatabasePassword(reader.GetString(5)));
		return new TenantDatabaseConfiguration(timestamp, id, databaseServer, databaseName, databaseUserName, databasePassword);
	}

	public IList<TenantDatabaseConfiguration> SelectTenantDatabases(NavSqlConnectionScope conScope)
	{
		IList<TenantDatabaseConfiguration> tenantdatabases = new List<TenantDatabaseConfiguration>();
		CheckTable(conScope);
		conScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("SelectTenantDatabases", applicationDatabase), () => string.Format(CultureInfo.InvariantCulture, "SELECT [timestamp], [id], [databaseserver], [databasename], [databaseusername], [databasepassword] FROM [{0}].[{1}].[{2}]", applicationDatabase.DatabaseName, "dbo", "$ndo$tenantdatabases"), (NavSqlCommand cmd) =>
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000RY", Category.TenantDatabaseManagement, Verbosity.Normal, PrivacyClassification.SystemMetadata, "SelectTenantDatabases: " + cmd.Command.CommandText);
			cmd.ExecuteReader();
			while (cmd.ReadNextRow())
			{
				TenantDatabaseConfiguration tenantDatabaseFromReader = GetTenantDatabaseFromReader(cmd.Reader);
				tenantdatabases.Add(tenantDatabaseFromReader);
			}
			return cmd;
		});
		return tenantdatabases;
	}

	public TenantDatabaseConfiguration InsertTenantDatabase(NavSqlConnectionScope conScope, TenantDatabaseConfiguration tenantDatabaseConfiguration)
	{
		if (tenantDatabaseConfiguration == null)
		{
			throw new ArgumentNullException("tenantDatabaseConfiguration");
		}
		CheckTable(conScope);
		return conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("InsertTenantDatabase", applicationDatabase), () => string.Format(CultureInfo.InvariantCulture, "INSERT INTO [{0}].[{1}].[{2}] ([id], [databaseserver], [databasename], [databaseusername], [databasepassword]) OUTPUT INSERTED.[timestamp] VALUES (@0,@1,@2,@3,@4)", applicationDatabase.DatabaseName, "dbo", "$ndo$tenantdatabases"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 128));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.NVarChar, 128));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.NVarChar, 128));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.NVarChar, 128));
			cmd.Command.Parameters.Add(new SqlParameter("@4", SqlDbType.NVarChar, 1024));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = tenantDatabaseConfiguration.Id.ToUpperInvariant();
			cmd.Command.Parameters[1].Value = ((object)tenantDatabaseConfiguration.DatabaseServer) ?? ((object)DBNull.Value);
			cmd.Command.Parameters[2].Value = ((object)tenantDatabaseConfiguration.DatabaseName) ?? ((object)DBNull.Value);
			cmd.Command.Parameters[3].Value = ((object)tenantDatabaseConfiguration.DatabaseUserName) ?? ((object)DBNull.Value);
			cmd.Command.Parameters[4].Value = ((object)tenantDatabaseConfiguration.DatabasePassword?.Value) ?? ((object)DBNull.Value);
		}, (NavSqlCommand cmd) =>
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000RZ", Category.TenantDatabaseManagement, Verbosity.Normal, PrivacyClassification.SystemMetadata, "InsertTenantDatabase: " + cmd.Command.CommandText);
			byte[] timestamp = (byte[])cmd.ExecuteScalar();
			return new TenantDatabaseConfiguration(timestamp, tenantDatabaseConfiguration.Id.ToUpperInvariant(), tenantDatabaseConfiguration.DatabaseServer, tenantDatabaseConfiguration.DatabaseName, tenantDatabaseConfiguration.DatabaseUserName, tenantDatabaseConfiguration.DatabasePassword);
		});
	}

	public bool DeleteTenantDatabase(NavSqlConnectionScope conScope, string id)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			throw new ArgumentNullException("id");
		}
		CheckTable(conScope);
		return conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("DeleteTenantDatabase", applicationDatabase), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}].[{1}].[{2}] WHERE [id] = @0", applicationDatabase.DatabaseName, "dbo", "$ndo$tenantdatabases"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 128));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = id.ToUpperInvariant();
		}, (NavSqlCommand cmd) =>
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000S2", Category.TenantDatabaseManagement, Verbosity.Normal, PrivacyClassification.SystemMetadata, "DeleteTenantDatabase: " + cmd.Command.CommandText);
			return cmd.ExecuteNonQuery();
		}) > 0;
	}

	internal void TruncateTenantDatabase(NavSqlConnectionScope conScope)
	{
		CheckTable(conScope);
		conScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("TruncateTenantDatabase", applicationDatabase), () => string.Format(CultureInfo.InvariantCulture, "TRUNCATE TABLE [{0}].[{1}].[{2}]", applicationDatabase.DatabaseName, "dbo", "$ndo$tenantdatabases"), (NavSqlCommand cmd) =>
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000S3", Category.TenantDatabaseManagement, Verbosity.Normal, PrivacyClassification.SystemMetadata, "TruncateTenantDatabase: " + cmd.Command.CommandText);
			return cmd.ExecuteNonQuery();
		});
	}

	public TenantDatabaseConfiguration GetTenantDatabase(NavSqlConnectionScope conScope, string id)
	{
		return GetTenantDatabase(conScope, id, withLock: false);
	}

	private TenantDatabaseConfiguration GetTenantDatabase(NavSqlConnectionScope conScope, string id, bool withLock)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			throw new ArgumentNullException("id");
		}
		if (withLock && !conScope.Connection.CurrentTransactionIsWriteTransaction)
		{
			throw new InvalidOperationException("A lock on tenant database was requested, but not inside an write transaction.");
		}
		CheckTable(conScope);
		return conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("LockSelectTenantDatabase", applicationDatabase), () => string.Format(CultureInfo.InvariantCulture, withLock ? "SELECT [timestamp], [id], [databaseserver], [databasename], [databaseusername], [databasepassword] FROM [{0}].[{1}].[{2}] WITH(UPDLOCK) WHERE [id] = @0" : "SELECT [timestamp], [id], [databaseserver], [databasename], [databaseusername], [databasepassword] FROM [{0}].[{1}].[{2}] WITH(READUNCOMMITTED) WHERE [id] = @0", applicationDatabase.DatabaseName, "dbo", "$ndo$tenantdatabases"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 128));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = id.ToUpperInvariant();
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			if (!cmd.ReadNextRow())
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000225", Category.TenantDatabaseManagement, Verbosity.Normal, PrivacyClassification.SystemMetadata, "GetTenantDatabase: id {0}, Tenant Configuration not found, SQL {1}", id, cmd.Command.CommandText);
				return (TenantDatabaseConfiguration)null;
			}
			TenantDatabaseConfiguration tenantDatabaseFromReader = GetTenantDatabaseFromReader(cmd.Reader);
			cmd.CloseReader(forceClose: false);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000S4", Category.TenantDatabaseManagement, Verbosity.Normal, PrivacyClassification.SystemMetadata, "GetTenantDatabase: id {0}, databaseserver {1}, databasename {2}, SQL {3}", tenantDatabaseFromReader.Id, tenantDatabaseFromReader.DatabaseServer, tenantDatabaseFromReader.DatabaseName, cmd.Command.CommandText);
			return tenantDatabaseFromReader;
		});
	}

	public void FillDatabaseInformation(NavTenantSettings tenantSettings)
	{
		if (string.IsNullOrEmpty(tenantSettings.TenantDatabaseId))
		{
			if (string.IsNullOrEmpty(tenantSettings.DatabaseServer))
			{
				tenantSettings.DatabaseServer = ServerUserSettings.Instance.ServerName;
			}
			return;
		}
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(applicationDatabase);
		TenantDatabaseConfiguration tenantDatabase = GetTenantDatabase(conScope, tenantSettings.TenantDatabaseId);
		if (tenantDatabase == null)
		{
			throw new NavTenantDatabaseException(string.Format(CultureInfo.CurrentCulture, Lang.TenantDatabaseEntryNotFound, tenantSettings.TenantDatabaseId));
		}
		tenantSettings.DatabaseName = tenantDatabase.DatabaseName;
		tenantSettings.DatabaseServer = tenantDatabase.DatabaseServer;
		tenantSettings.DatabaseUserName = tenantDatabase.DatabaseUserName;
		tenantSettings.ProtectedDatabasePassword = tenantDatabase.DatabasePassword;
	}
}
