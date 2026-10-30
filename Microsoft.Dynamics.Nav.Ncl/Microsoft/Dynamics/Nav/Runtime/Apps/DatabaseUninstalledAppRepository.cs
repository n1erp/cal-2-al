using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Utility;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class DatabaseUninstalledAppRepository : IUninstalledAppRepository
{
	private class Key
	{
		public string TenantId { get; set; }

		public Guid AppId { get; set; }

		public string Version { get; set; }
	}

	private class FieldDefinition
	{
		public string Name { get; }

		public NavNclType NavType { get; }

		public SqlDbType SqlType { get; }

		public int Length { get; }

		public bool AllowNull { get; }

		public FieldDefinition(string name, NavNclType navType, SqlDbType sqlType, int length, bool allowNull)
		{
			Name = name;
			NavType = navType;
			SqlType = sqlType;
			Length = length;
			AllowNull = allowNull;
		}
	}

	public const string TableName = "[dbo].[$ndo$navappuninstalledapp]";

	private const string PrimaryKeyName = "PK$ndo$navappuninstalledapp";

	private readonly NavDatabase tenantDatabase;

	private readonly NavSqlConnectionScope conScope;

	private static readonly FieldDefinition[] fieldDefinitions = new FieldDefinition[7]
	{
		new FieldDefinition("tenantid", NavNclType.NavText, SqlDbType.NVarChar, 128, allowNull: false),
		new FieldDefinition("appid", NavNclType.NavGuid, SqlDbType.UniqueIdentifier, 0, allowNull: false),
		new FieldDefinition("version", NavNclType.NavText, SqlDbType.NVarChar, 50, allowNull: false),
		new FieldDefinition("name", NavNclType.NavText, SqlDbType.NVarChar, 500, allowNull: false),
		new FieldDefinition("publisher", NavNclType.NavText, SqlDbType.NVarChar, 500, allowNull: false),
		new FieldDefinition("timeofuninstall", NavNclType.NavDateTime, SqlDbType.DateTime, 0, allowNull: false),
		new FieldDefinition("hasdata", NavNclType.NavBoolean, SqlDbType.Bit, 0, allowNull: false)
	};

	private static FieldDefinition TenantIdDef => fieldDefinitions[0];

	private static FieldDefinition AppIdDef => fieldDefinitions[1];

	private static FieldDefinition VersionDef => fieldDefinitions[2];

	public DatabaseUninstalledAppRepository(NavDatabase tenantDatabase, NavSqlConnectionScope conScope)
	{
		this.tenantDatabase = tenantDatabase;
		this.conScope = conScope;
	}

	public void CreateTable()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'{0}') AND OBJECTPROPERTY(id, N'IsTable') = 1) ", "[dbo].[$ndo$navappuninstalledapp]");
		stringBuilder.Append("CREATE TABLE ");
		stringBuilder.Append("[dbo].[$ndo$navappuninstalledapp]");
		stringBuilder.Append(" (");
		for (int i = 0; i < fieldDefinitions.Length; i++)
		{
			FieldDefinition fieldDefinition = fieldDefinitions[i];
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, fieldDefinition.Name);
			stringBuilder.Append(' ');
			NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder, fieldDefinition.NavType, SqlDataType.Default, fieldDefinition.Length);
			stringBuilder.Append(fieldDefinition.AllowNull ? " NULL," : " NOT NULL,");
		}
		stringBuilder.Append(" CONSTRAINT ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, "PK$ndo$navappuninstalledapp");
		stringBuilder.Append(" PRIMARY KEY (");
		stringBuilder.Append(fieldDefinitions[0].Name);
		stringBuilder.Append(", ");
		stringBuilder.Append(fieldDefinitions[1].Name);
		stringBuilder.Append(", ");
		stringBuilder.Append(fieldDefinitions[2].Name);
		stringBuilder.AppendLine("));");
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		navSqlCommand.ExecuteNonQuery();
	}

	public void InsertOrUpdate(UninstalledAppRecord record)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		string query = "UPDATE {0} SET [name] = @3, [publisher] = @4, [timeofuninstall] = @5, [hasdata] = @6 where [tenantid] = @0 AND [appid] = @1 AND [version] = @2 IF @@ROWCOUNT = 0 INSERT INTO {0} ([tenantid], [appid], [version], [name], [publisher], [timeofuninstall], [hasdata]) VALUES (@0, @1, @2, @3, @4, @5, @6)";
		conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("UninstalledAppRepository-InsertOrUpdate", tenantDatabase), () => string.Format(CultureInfo.InvariantCulture, query, "[dbo].[$ndo$navappuninstalledapp]"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", fieldDefinitions[0].SqlType, fieldDefinitions[0].Length));
			cmd.Command.Parameters.Add(new SqlParameter("@1", fieldDefinitions[1].SqlType, fieldDefinitions[1].Length));
			cmd.Command.Parameters.Add(new SqlParameter("@2", fieldDefinitions[2].SqlType, fieldDefinitions[2].Length));
			cmd.Command.Parameters.Add(new SqlParameter("@3", fieldDefinitions[3].SqlType, fieldDefinitions[3].Length));
			cmd.Command.Parameters.Add(new SqlParameter("@4", fieldDefinitions[4].SqlType, fieldDefinitions[4].Length));
			cmd.Command.Parameters.Add(new SqlParameter("@5", fieldDefinitions[5].SqlType, fieldDefinitions[5].Length));
			cmd.Command.Parameters.Add(new SqlParameter("@6", fieldDefinitions[6].SqlType, fieldDefinitions[6].Length));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = tenantDatabase.Tenant.Id;
			cmd.Command.Parameters[1].Value = ((WrappedValue<Guid>)(object)record.AppId).Value;
			cmd.Command.Parameters[2].Value = record.Version.ToString();
			cmd.Command.Parameters[3].Value = record.Name.ToString();
			cmd.Command.Parameters[4].Value = record.Publisher.ToString();
			cmd.Command.Parameters[5].Value = record.TimeOfUninstall;
			cmd.Command.Parameters[6].Value = record.HasData;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000074M", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Inserting or updating uninstalled record -- AppId:{0}; Name:{1}; Publisher:{2}; Version:{3}; Tenant:{4}; TimeOfUninstall:{5}; HasData:{6}; Elapsed:{7}", ((WrappedValue<Guid>)(object)record.AppId).Value, record.Name, record.Publisher, record.Version, record.TenantId, record.TimeOfUninstall.ToString("yyyy-MM-dd_HH.mm.ss", CultureInfo.InvariantCulture), record.HasData, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	public IEnumerable<UninstalledAppRecord> GetAllForTenant(string tenantId)
	{
		string tenantIdInProperCase = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
		List<UninstalledAppRecord> list = new List<UninstalledAppRecord>();
		string content = string.Format(CultureInfo.InvariantCulture, "Getting all uninstalled records for tenant -- Tenant:{0}", tenantIdInProperCase);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000017V", DiagnosticParameter.Oii(content), MonitorType.SqlReadCount, Category.Extensions, createNewActivity: false))
		{
			string query = string.Format(CultureInfo.InvariantCulture, "SELECT [tenantid], [appid], [version], [name], [publisher], [timeofuninstall], [hasdata] FROM {0} WHERE [tenantid] = @0", "[dbo].[$ndo$navappuninstalledapp]");
			NavSqlCommand navSqlCommand = conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("UninstalledAppRepository-GetAllForTenant" + tenantIdInProperCase, tenantDatabase), () => string.Format(CultureInfo.InvariantCulture, query, "[dbo].[$ndo$navappuninstalledapp]"), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", fieldDefinitions[0].SqlType, fieldDefinitions[0].Length));
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = tenantIdInProperCase;
			}, (NavSqlCommand cmd) =>
			{
				cmd.ExecuteReader();
				return cmd;
			});
			while (navSqlCommand.ReadNextRow())
			{
				UninstalledAppRecord uninstalledAppRecord = new UninstalledAppRecord();
				uninstalledAppRecord.TenantId = navSqlCommand.Reader.GetString(0);
				uninstalledAppRecord.AppId = AppId.op_Implicit(navSqlCommand.Reader.GetGuid(1));
				uninstalledAppRecord.Version = Version.Parse(navSqlCommand.Reader.GetString(2));
				uninstalledAppRecord.Name = navSqlCommand.Reader.GetString(3);
				uninstalledAppRecord.Publisher = navSqlCommand.Reader.GetString(4);
				uninstalledAppRecord.TimeOfUninstall = navSqlCommand.Reader.GetDateTime(5);
				uninstalledAppRecord.HasData = navSqlCommand.Reader.GetByte(6) != 0;
				list.Add(uninstalledAppRecord);
			}
			return list;
		}
	}

	public IEnumerable<UninstalledAppRecord> GetAll()
	{
		List<UninstalledAppRecord> list = new List<UninstalledAppRecord>();
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000017W", DiagnosticParameter.SystemMetadata("Getting all uninstalled records"), MonitorType.SqlReadCount, Category.Extensions, createNewActivity: false))
		{
			string query = "SELECT [tenantid], [appid], [version], [name], [publisher], [timeofuninstall], [hasdata] FROM " + "[dbo].[$ndo$navappuninstalledapp]";
			NavSqlCommand navSqlCommand = conScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("UninstalledAppRepository-GetAll", tenantDatabase), () => string.Format(CultureInfo.InvariantCulture, query, "[dbo].[$ndo$navappuninstalledapp]"), (NavSqlCommand cmd) =>
			{
				cmd.ExecuteReader();
				return cmd;
			});
			while (navSqlCommand.ReadNextRow())
			{
				UninstalledAppRecord uninstalledAppRecord = new UninstalledAppRecord();
				uninstalledAppRecord.TenantId = navSqlCommand.Reader.GetString(0);
				uninstalledAppRecord.AppId = AppId.op_Implicit(navSqlCommand.Reader.GetGuid(1));
				uninstalledAppRecord.Version = Version.Parse(navSqlCommand.Reader.GetString(2));
				uninstalledAppRecord.Name = navSqlCommand.Reader.GetString(3);
				uninstalledAppRecord.Publisher = navSqlCommand.Reader.GetString(4);
				uninstalledAppRecord.TimeOfUninstall = navSqlCommand.Reader.GetDateTime(5);
				uninstalledAppRecord.HasData = navSqlCommand.Reader.GetByte(6) != 0;
				list.Add(uninstalledAppRecord);
			}
			return list;
		}
	}

	public void DeleteAllForApp(AppId appId, bool deleteForAllTenants = false)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		string content = string.Format(CultureInfo.InvariantCulture, "Deleting uninstalled record -- AppId:{0}; ForAllTenants:{1}; Tenant:{2}", appId, deleteForAllTenants, tenantDatabase.Tenant.Id);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000017X", DiagnosticParameter.Oii(content), MonitorType.SqlReadCount, Category.Extensions, createNewActivity: false))
		{
			string query = "DELETE FROM {0} WHERE [appid] = @0";
			if (!deleteForAllTenants)
			{
				query += " AND [tenantid] = @1";
			}
			conScope.Connection.ExecuteCommandWithParameters(new NavSqlCombinedStringBasedCommandCacheKey(tenantDatabase, "UninstalledAppRepository-DeleteAllForApp", deleteForAllTenants.ToString(CultureInfo.InvariantCulture)), () => string.Format(CultureInfo.InvariantCulture, query, "[dbo].[$ndo$navappuninstalledapp]"), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", fieldDefinitions[1].SqlType));
				if (!deleteForAllTenants)
				{
					cmd.Command.Parameters.Add(new SqlParameter("@1", fieldDefinitions[0].SqlType, fieldDefinitions[0].Length));
				}
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)appId).Value;
				if (!deleteForAllTenants)
				{
					cmd.Command.Parameters[1].Value = tenantDatabase.Tenant.Id;
				}
			}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000074N", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Deleting uninstalled record -- Tenant:{0}; PackageId:{1}; Elapsed:{2}", tenantDatabase.Tenant.Id, ((WrappedValue<Guid>)(object)appId).Value, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	public void DeleteAllForTenant()
	{
		DiagnosticParameter name = DiagnosticParameter.Oii(string.Format(CultureInfo.InvariantCulture, "Deleting uninstalled records for tenant -- Tenant:{0}", tenantDatabase.Tenant.Id));
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00001QI", name, MonitorType.SqlReadCount, Category.Extensions, createNewActivity: false))
		{
			string query = string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE [tenantid] = @0", "[dbo].[$ndo$navappuninstalledapp]");
			conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("UninstalledAppRepository-DeleteAllForTenant" + tenantDatabase.Tenant.Id, tenantDatabase), () => string.Format(CultureInfo.InvariantCulture, query, "[dbo].[$ndo$navappuninstalledapp]"), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", fieldDefinitions[0].SqlType, fieldDefinitions[0].Length));
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = tenantDatabase.Tenant.Id;
			}, (NavSqlCommand cmd) =>
			{
				cmd.ExecuteReader();
				return cmd;
			});
		}
	}

	public void ChangeTenantId(string oldId, string newId)
	{
		if (!DoesTableExist())
		{
			return;
		}
		oldId = NavTenant.ConvertTenantIdToExpectedCase(oldId);
		newId = NavTenant.ConvertTenantIdToExpectedCase(newId);
		DiagnosticParameter name = DiagnosticParameter.Oii(string.Format(CultureInfo.InvariantCulture, "Changing uninstalled tenant ID -- Old:{0}; New:{1}", oldId, newId));
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00001VH", name, MonitorType.SqlWriteCount, Category.Extensions, createNewActivity: false))
		{
			IEnumerable<Key> recordsForBothTenantIds = GetRecordsForBothTenantIds(oldId, newId);
			foreach (Key item in recordsForBothTenantIds)
			{
				DeleteByKey(item);
			}
			UpdateTenantId(oldId, newId);
		}
	}

	private bool DoesTableExist()
	{
		return NavSqlMetadataCommands.TableExists(conScope, tenantDatabase, "[dbo].[$ndo$navappuninstalledapp]");
	}

	private IEnumerable<Key> GetRecordsForBothTenantIds(string oldId, string newId)
	{
		NavSqlCommand navSqlCommand = conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("UninstalledAppRepository-GetRecordsForBothTenantIds", tenantDatabase), () => string.Format(CultureInfo.InvariantCulture, "SELECT [tenantid], [appid], [version] FROM [$ndo$navappuninstalledapp] main WHERE [tenantid] = @0 AND EXISTS( SELECT [tenantid], [appid], [version] FROM [$ndo$navappuninstalledapp] sub WHERE sub.tenantid = @1 AND sub.[appid] = main.[appid] AND sub.[version] = main.[version])", "[dbo].[$ndo$navappuninstalledapp]"), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", TenantIdDef.SqlType, TenantIdDef.Length));
			cmd.Command.Parameters.Add(new SqlParameter("@1", TenantIdDef.SqlType, TenantIdDef.Length));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = oldId;
			cmd.Command.Parameters[1].Value = newId;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			return cmd;
		});
		SqlDataReader reader = navSqlCommand.Reader;
		List<Key> list = new List<Key>();
		while (reader.Read())
		{
			list.Add(new Key
			{
				TenantId = reader.GetString(0),
				AppId = reader.GetGuid(1),
				Version = reader.GetString(2)
			});
		}
		return list;
	}

	private void DeleteByKey(Key key)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001VS", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Deleting uninstalled app record -- TenantId:{0}; AppId:{1}; Version:{2}", key.TenantId, key.AppId, key.Version);
		conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("UninstalledAppRepository-DeleteByKey", tenantDatabase), () => "DELETE FROM [$ndo$navappuninstalledapp] WHERE [tenantid] = @0 AND [appid] = @1 AND [version] = @2", (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", TenantIdDef.SqlType, TenantIdDef.Length));
			cmd.Command.Parameters.Add(new SqlParameter("@1", AppIdDef.SqlType, AppIdDef.Length));
			cmd.Command.Parameters.Add(new SqlParameter("@2", VersionDef.SqlType, VersionDef.Length));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = key.TenantId;
			cmd.Command.Parameters[1].Value = key.AppId;
			cmd.Command.Parameters[2].Value = key.Version;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
	}

	private void UpdateTenantId(string oldId, string newId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001VT", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Updating uninstalled app record -- OldTenantId:{0}; NewTenantId:{1}", oldId, newId);
		conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("UninstalledAppRepository-UpdateTenantId", tenantDatabase), () => "UPDATE [$ndo$navappuninstalledapp] SET [tenantid] = @1 WHERE [tenantid] = @0", (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add(new SqlParameter("@0", TenantIdDef.SqlType, TenantIdDef.Length));
			cmd.Command.Parameters.Add(new SqlParameter("@1", TenantIdDef.SqlType, TenantIdDef.Length));
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = oldId;
			cmd.Command.Parameters[1].Value = newId;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
	}
}
