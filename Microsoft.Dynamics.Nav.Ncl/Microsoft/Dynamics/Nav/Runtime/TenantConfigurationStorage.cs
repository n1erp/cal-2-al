using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class TenantConfigurationStorage
{
	private const string TenantsTableName = "$ndo$tenants";

	public const int TenantIdMaxLength = 128;

	private const string CreateTenantsTableStatement = "IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[{1}].[{2}]') AND OBJECTPROPERTY(id, N'IsTable') = 1) CREATE TABLE [{0}].[{1}].[{2}]( [timestamp] TIMESTAMP, [tenantid] NVARCHAR(128), [tenantconfigurationstate] INT, [tenantsettings] IMAGE, [version] INT, CONSTRAINT [PK{2}] PRIMARY KEY CLUSTERED(tenantid));IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{2}' AND COLUMN_NAME = 'exclusiveaccessownerserverid')ALTER TABLE [{0}].[{1}].[{2}] ADD [exclusiveaccessownerserverid] INT NULL";

	private const string SelectTenantConfigurationsStatement = "SELECT CAST([timestamp] as bigint), [tenantid], [tenantconfigurationstate], [tenantsettings], [version], [exclusiveaccessownerserverid] FROM [{0}].[{1}].[{2}] WHERE [timestamp] > CAST(@0 as timestamp);";

	private const string SelectMaxTenantConfigurationsStatement = "SELECT MAX(CAST([timestamp] as bigint)) FROM [{0}].[{1}].[{2}] WITH(NOLOCK)";

	private const string CountTenantConfigurationsStatement = "IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[{1}].[{2}]') AND OBJECTPROPERTY(id, N'IsTable') = 1) SELECT COUNT(*) FROM [{0}].[{1}].[{2}] ELSE SELECT 0";

	private const string InsertTenantConfigurationStatemnt = "INSERT INTO [{0}].[{1}].[{2}] ([tenantid], [tenantconfigurationstate], [tenantsettings], [version]) OUTPUT CAST(INSERTED.[timestamp] as bigint) VALUES (@0,@1,@2,@3)";

	private const string UpdateTenantConfigurationStatemnt = "UPDATE [{0}].[{1}].[{2}] SET [tenantconfigurationstate] = @0, [exclusiveaccessownerserverid] = @1 OUTPUT CAST(INSERTED.[timestamp] as bigint) WHERE [tenantid] = @2 AND [timestamp] = CAST(@3 as timestamp)";

	private const string GetTenantConfigurationWithLockStatement = "SELECT CAST([timestamp] as bigint), [tenantid], [tenantconfigurationstate], [tenantsettings], [version], [exclusiveaccessownerserverid] FROM [{0}].[{1}].[{2}] WITH(UPDLOCK) WHERE [tenantid] = @0";

	private const string ConvertOptimisticToPessimisticSharedLockStatement = "SELECT 1 FROM [{0}].[{1}].[{2}] WITH(READCOMMITTED) WHERE [tenantid] = @0 AND [timestamp] = CAST(@1 as timestamp)";

	private const string GetTenantConfigurationStatement = "SELECT CAST([timestamp] as bigint), [tenantid], [tenantconfigurationstate], [tenantsettings], [version], [exclusiveaccessownerserverid] FROM [{0}].[{1}].[{2}] WHERE [tenantid] = @0";

	private const string DeleteTenantConfigurationStatement = "DELETE FROM [{0}].[{1}].[{2}] WHERE [tenantid] = @0 and [timestamp] = CAST(@1 as timestamp)";

	private const string UpdateTenantConfigurationTimestampStatement = "UPDATE [{0}].[{1}].[{2}] SET [tenantid] = @0 OUTPUT CAST(INSERTED.[timestamp] as bigint) WHERE [tenantid] = @0";

	private static readonly string RecoverFromExclusiveAccessModeStatement = "IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[Server Instance]') AND OBJECTPROPERTY(id, N'IsTable') = 1)\r\n            UPDATE [{0}].[{1}].[{2}] SET tenantconfigurationstate = " + 0 + ", exclusiveaccessownerserverid = NULL\r\n            WHERE tenantconfigurationstate = " + 5 + " AND exclusiveaccessownerserverid IS NOT NULL AND\r\n            NOT EXISTS(select 1 from [Server Instance] s WHERE exclusiveaccessownerserverid = s.[Server Instance ID])";

	private static readonly string DeleteRemovedTenantConfigurationIfAcknowledgedStatement = "DELETE FROM [{0}].[{1}].[{2}] WHERE [tenantconfigurationstate] = " + 1 + " AND [timestamp] <= @0 SELECT @@ROWCOUNT";

	private const int CurrentVersion = 1000;

	private const int AcceptableVersionIncreaseWithoutConversion = 9;

	private readonly NavDatabase database;

	private bool tableChecked;

	public TenantConfigurationStorage(NavDatabase serviceDatabase)
	{
		database = serviceDatabase;
	}

	private void CheckTable(NavSqlConnectionScope conScope)
	{
		if (tableChecked)
		{
			return;
		}
		tableChecked = true;
		try
		{
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
			navSqlCommand.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[{1}].[{2}]') AND OBJECTPROPERTY(id, N'IsTable') = 1) CREATE TABLE [{0}].[{1}].[{2}]( [timestamp] TIMESTAMP, [tenantid] NVARCHAR(128), [tenantconfigurationstate] INT, [tenantsettings] IMAGE, [version] INT, CONSTRAINT [PK{2}] PRIMARY KEY CLUSTERED(tenantid));IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{2}' AND COLUMN_NAME = 'exclusiveaccessownerserverid')ALTER TABLE [{0}].[{1}].[{2}] ADD [exclusiveaccessownerserverid] INT NULL", database.DatabaseName, "dbo", "$ndo$tenants"));
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

	private static TenantConfiguration GetTenantConfigurationFromReader(SqlDataReader reader)
	{
		long @int = reader.GetInt64(0);
		TenantConfigurationState int2 = (TenantConfigurationState)reader.GetInt32(2);
		BlobReaderStream documentStream = new BlobReaderStream(reader, 3);
		DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(NavTenantSettings));
		NavTenantSettings settings;
		using (XmlReader reader2 = XmlHelper.SafeXmlReader(documentStream))
		{
			settings = (NavTenantSettings)dataContractSerializer.ReadObject(reader2);
		}
		int int3 = reader.GetInt32(4);
		int? exclusiveAccessOwnerServerId = null;
		if (!reader.IsDBNull(5))
		{
			exclusiveAccessOwnerServerId = reader.GetInt32(5);
		}
		if (int3 > 1009)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "This version of {0} cannot read the {1}", ProductNames.ProductNameShort, "$ndo$tenants"));
		}
		return new TenantConfiguration(settings, @int, int2, exclusiveAccessOwnerServerId);
	}

	public ConcurrentDictionary<string, TenantConfiguration> LoadUnknownOrChangedConfigurations(NavSqlConnectionScope conScope, long fromConfigurationTimestamp)
	{
		ConcurrentDictionary<string, TenantConfiguration> tenants = new ConcurrentDictionary<string, TenantConfiguration>(StringComparer.OrdinalIgnoreCase);
		CheckTable(conScope);
		conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("LoadTenantConfigurations", database), () => string.Format(CultureInfo.InvariantCulture, "SELECT CAST([timestamp] as bigint), [tenantid], [tenantconfigurationstate], [tenantsettings], [version], [exclusiveaccessownerserverid] FROM [{0}].[{1}].[{2}] WHERE [timestamp] > CAST(@0 as timestamp);", database.DatabaseName, "dbo", "$ndo$tenants"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.BigInt, 8));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = fromConfigurationTimestamp;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			while (cmd.ReadNextRow())
			{
				TenantConfiguration tenantConfigurationFromReader = GetTenantConfigurationFromReader(cmd.Reader);
				tenants.TryAdd(tenantConfigurationFromReader.Settings.Id, tenantConfigurationFromReader);
			}
			return cmd;
		});
		return tenants;
	}

	public long GetMaxTenantConfigurationTimestamp(NavSqlConnectionScope conScope)
	{
		CheckTable(conScope);
		long maxTimestamp = 0L;
		conScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("GetMaxTenantConfigurationTimestamp", database), () => string.Format(CultureInfo.InvariantCulture, "SELECT MAX(CAST([timestamp] as bigint)) FROM [{0}].[{1}].[{2}] WITH(NOLOCK)", database.DatabaseName, "dbo", "$ndo$tenants"), (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			if (cmd.ReadNextRow() && !cmd.Reader.IsDBNull(0))
			{
				maxTimestamp = cmd.Reader.GetInt64(0);
			}
			return cmd;
		});
		return maxTimestamp;
	}

	internal static int CountTenantConfigurations(NavSqlConnectionScope conScope)
	{
		return conScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("CountTenantConfigurationsStatement", conScope.Connection.Database), () => string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'[{1}].[{2}]') AND OBJECTPROPERTY(id, N'IsTable') = 1) SELECT COUNT(*) FROM [{0}].[{1}].[{2}] ELSE SELECT 0", conScope.Connection.Database.DatabaseName, "dbo", "$ndo$tenants"), (NavSqlCommand cmd) => cmd.ExecuteScalar<int>());
	}

	private static void CheckTenantSetting(NavTenantSettings tenantSettings)
	{
		if (!string.IsNullOrEmpty(tenantSettings.TenantDatabaseId))
		{
			tenantSettings.DatabaseName = string.Empty;
			tenantSettings.DatabaseServer = string.Empty;
			tenantSettings.DatabaseUserName = string.Empty;
			tenantSettings.ProtectedDatabasePassword = null;
		}
	}

	public TenantConfiguration AddTenantConfiguration(NavSqlConnectionScope conScope, NavTenantSettings tenantSettings)
	{
		CheckTable(conScope);
		CheckTenantSetting(tenantSettings);
		return conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("InsertTenantConfiguration", database), () => string.Format(CultureInfo.InvariantCulture, "INSERT INTO [{0}].[{1}].[{2}] ([tenantid], [tenantconfigurationstate], [tenantsettings], [version]) OUTPUT CAST(INSERTED.[timestamp] as bigint) VALUES (@0,@1,@2,@3)", database.DatabaseName, "dbo", "$ndo$tenants"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 128));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Binary, int.MaxValue));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Int));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = tenantSettings.Id.ToUpperInvariant();
			cmd.Command.Parameters[1].Value = TenantConfigurationState.Active;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(NavTenantSettings));
				using (XmlWriter writer = XmlWriter.Create(memoryStream))
				{
					dataContractSerializer.WriteObject(writer, tenantSettings);
				}
				cmd.Command.Parameters[2].Value = memoryStream.ToArray();
			}
			cmd.Command.Parameters[3].Value = 1000;
		}, (NavSqlCommand cmd) =>
		{
			long timestamp = (long)cmd.ExecuteScalar();
			return new TenantConfiguration(tenantSettings, timestamp, TenantConfigurationState.Active);
		});
	}

	public TenantConfiguration UpdateTenantConfiguration(NavSqlConnectionScope conScope, TenantConfiguration inputTenantConfiguration, TenantConfigurationState newState, int? exclusiveAccessOwnerServerInstanceId = null)
	{
		CheckTable(conScope);
		CheckTenantSetting(inputTenantConfiguration.Settings);
		long expectedTimestamp = inputTenantConfiguration.Timestamp;
		return conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("UpdateTenantConfiguration", database), () => string.Format(CultureInfo.InvariantCulture, "UPDATE [{0}].[{1}].[{2}] SET [tenantconfigurationstate] = @0, [exclusiveaccessownerserverid] = @1 OUTPUT CAST(INSERTED.[timestamp] as bigint) WHERE [tenantid] = @2 AND [timestamp] = CAST(@3 as timestamp)", database.DatabaseName, "dbo", "$ndo$tenants"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
			cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.NVarChar, 128));
			cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.BigInt));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = newState;
			cmd.Command.Parameters[1].Value = (exclusiveAccessOwnerServerInstanceId.HasValue ? ((object)exclusiveAccessOwnerServerInstanceId.Value) : DBNull.Value);
			cmd.Command.Parameters[2].Value = inputTenantConfiguration.Settings.Id.ToUpperInvariant();
			cmd.Command.Parameters[3].Value = expectedTimestamp;
		}, (NavSqlCommand cmd) =>
		{
			object obj = cmd.ExecuteScalar();
			if (obj == null || obj == DBNull.Value)
			{
				return (TenantConfiguration)null;
			}
			long timestamp = (long)obj;
			return new TenantConfiguration(inputTenantConfiguration.Settings, timestamp, newState, exclusiveAccessOwnerServerInstanceId);
		});
	}

	public void RecoverTenantsFromExclusiveAccessState(NavSqlConnectionScope conScope)
	{
		CheckTable(conScope);
		conScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("RecoverTenantsFromExclusiveAccessState", database), () => string.Format(CultureInfo.InvariantCulture, RecoverFromExclusiveAccessModeStatement, database.DatabaseName, "dbo", "$ndo$tenants"), (NavSqlCommand cmd) => cmd.ExecuteScalar());
	}

	public int DeleteRemovedTenantConfigurationsIfAcknowledged(NavSqlConnectionScope conScope, long timestampAcknowledgedByAllServerInstances)
	{
		CheckTable(conScope);
		return conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("DeleteRemovedTenantConfigurationIfAcknowledged", database), () => string.Format(CultureInfo.InvariantCulture, DeleteRemovedTenantConfigurationIfAcknowledgedStatement, database.DatabaseName, "dbo", "$ndo$tenants"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.BigInt));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = timestampAcknowledgedByAllServerInstances;
		}, (NavSqlCommand cmd) => cmd.ExecuteScalar<int>());
	}

	public void RemoveTenantConfiguration(NavSqlConnectionScope conScope, string tenantId, long timestamp)
	{
		CheckTable(conScope);
		conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("RemoveTenantConfiguration", database), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}].[{1}].[{2}] WHERE [tenantid] = @0 and [timestamp] = CAST(@1 as timestamp)", database.DatabaseName, "dbo", "$ndo$tenants"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 128));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.BigInt, 8));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = tenantId.ToUpperInvariant();
			cmd.Command.Parameters[1].Value = timestamp;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
	}

	public TenantConfiguration GetTenantConfiguration(NavSqlConnectionScope conScope, string tenantId, bool withLock = false)
	{
		CheckTable(conScope);
		if (withLock && !conScope.Connection.CurrentTransactionIsWriteTransaction)
		{
			throw new InvalidOperationException("Tenants must be accessed only within write transaction, in order to avoid holding a lock on $ndo$tenants");
		}
		return conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey(withLock ? "LockTenantConfiguration" : "GetTenantConfiguration", database), () => string.Format(CultureInfo.InvariantCulture, withLock ? "SELECT CAST([timestamp] as bigint), [tenantid], [tenantconfigurationstate], [tenantsettings], [version], [exclusiveaccessownerserverid] FROM [{0}].[{1}].[{2}] WITH(UPDLOCK) WHERE [tenantid] = @0" : "SELECT CAST([timestamp] as bigint), [tenantid], [tenantconfigurationstate], [tenantsettings], [version], [exclusiveaccessownerserverid] FROM [{0}].[{1}].[{2}] WHERE [tenantid] = @0", database.DatabaseName, "dbo", "$ndo$tenants"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 128));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = tenantId.ToUpperInvariant();
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			if (!cmd.ReadNextRow())
			{
				return (TenantConfiguration)null;
			}
			TenantConfiguration tenantConfigurationFromReader = GetTenantConfigurationFromReader(cmd.Reader);
			cmd.CloseReader(forceClose: false);
			return tenantConfigurationFromReader;
		});
	}

	public bool ConvertOptimisticToPessimisticSharedLock(NavSqlConnectionScope conScope, string tenantId, long expectedTenantConfigurationTimestamp)
	{
		CheckTable(conScope);
		return conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("ConvertOptimisticToPessimisticSharedLock", database), () => string.Format(CultureInfo.InvariantCulture, "SELECT 1 FROM [{0}].[{1}].[{2}] WITH(READCOMMITTED) WHERE [tenantid] = @0 AND [timestamp] = CAST(@1 as timestamp)", database.DatabaseName, "dbo", "$ndo$tenants"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 128));
			cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.BigInt));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = tenantId.ToUpperInvariant();
			cmd.Command.Parameters[1].Value = expectedTenantConfigurationTimestamp;
		}, (NavSqlCommand cmd) =>
		{
			object obj = cmd.ExecuteScalar();
			return (obj != null && obj != DBNull.Value) ? true : false;
		});
	}

	public long RefreshTenantConfigurationTimestamp(NavSqlConnectionScope conScope, string tenantId)
	{
		CheckTable(conScope);
		return conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("UpdateTenantConfigurationTimestamp", database), () => string.Format(CultureInfo.InvariantCulture, "UPDATE [{0}].[{1}].[{2}] SET [tenantid] = @0 OUTPUT CAST(INSERTED.[timestamp] as bigint) WHERE [tenantid] = @0", database.DatabaseName, "dbo", "$ndo$tenants"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.NVarChar, 128));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = tenantId.ToUpperInvariant();
		}, (NavSqlCommand cmd) =>
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000Y1", Category.Tenant, Verbosity.Normal, PrivacyClassification.SystemMetadata, "TenantSettings refreshed: " + cmd.Command.CommandText);
			return cmd.ExecuteScalar<long>();
		});
	}
}
