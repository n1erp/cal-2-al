using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class DatabaseAppSyncTrackingRepository : IAppSyncTrackingRepository
{
	private class FieldDefinition
	{
		public string Name { get; }

		public NavNclType NavType { get; }

		public SqlDbType SqlType { get; }

		public int Length { get; }

		public bool AllowNull { get; }

		public string AlterDefault { get; }

		public FieldDefinition(string name, NavNclType navType, SqlDbType sqlType, int length, bool allowNull, string alterDefault = null)
		{
			Name = name;
			NavType = navType;
			SqlType = sqlType;
			Length = length;
			AllowNull = allowNull;
			AlterDefault = alterDefault;
		}
	}

	public const string TableName = "[dbo].[$ndo$navappschematracking]";

	private const string PrimaryKeyName = "PK$ndo$navappschematracking";

	private readonly NavDatabase tenantDatabase;

	private readonly NavSqlConnectionScope conScope;

	private static readonly FieldDefinition[] fieldDefinitions = new FieldDefinition[5]
	{
		new FieldDefinition("appid", NavNclType.NavGuid, SqlDbType.UniqueIdentifier, 0, allowNull: false),
		new FieldDefinition("version", NavNclType.NavText, SqlDbType.NVarChar, 50, allowNull: false),
		new FieldDefinition("name", NavNclType.NavText, SqlDbType.NVarChar, 500, allowNull: false),
		new FieldDefinition("publisher", NavNclType.NavText, SqlDbType.NVarChar, 500, allowNull: false),
		new FieldDefinition("baselineversion", NavNclType.NavText, SqlDbType.NVarChar, 50, allowNull: false, "N''")
	};

	private static readonly FieldDefinition[] deprecatedFieldDefinitions = new FieldDefinition[2]
	{
		new FieldDefinition("applicationversion", NavNclType.NavText, SqlDbType.NVarChar, 50, allowNull: true),
		new FieldDefinition("isremoved", NavNclType.NavBoolean, SqlDbType.Bit, 0, allowNull: false)
	};

	public DatabaseAppSyncTrackingRepository(NavDatabase tenantDatabase, NavSqlConnectionScope conScope)
	{
		this.tenantDatabase = tenantDatabase;
		this.conScope = conScope;
	}

	private static void TraceSqlStatement(string statement)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000017P", Category.Extensions, Verbosity.Verbose, PrivacyClassification.SystemMetadata, statement);
	}

	public void CreateTable(bool forceDropOld)
	{
		if (forceDropOld)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'{0}') AND OBJECTPROPERTY(id, N'IsTable') = 1) ", "[dbo].[$ndo$navappschematracking]");
			stringBuilder.Append("DROP TABLE ");
			stringBuilder.Append("[dbo].[$ndo$navappschematracking]");
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
			navSqlCommand.SetCommandText(stringBuilder.ToString());
			navSqlCommand.ExecuteNonQuery();
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.AppendFormat(CultureInfo.InvariantCulture, "IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'{0}') AND OBJECTPROPERTY(id, N'IsTable') = 1) ", "[dbo].[$ndo$navappschematracking]");
		stringBuilder2.Append("CREATE TABLE ");
		stringBuilder2.Append("[dbo].[$ndo$navappschematracking]");
		stringBuilder2.Append(" (");
		for (int i = 0; i < fieldDefinitions.Length; i++)
		{
			FieldDefinition fieldDefinition = fieldDefinitions[i];
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder2, fieldDefinition.Name);
			stringBuilder2.Append(' ');
			NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder2, fieldDefinition.NavType, SqlDataType.Default, fieldDefinition.Length);
			stringBuilder2.Append(fieldDefinition.AllowNull ? " NULL," : " NOT NULL,");
		}
		stringBuilder2.Append(" CONSTRAINT ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder2, "PK$ndo$navappschematracking");
		stringBuilder2.Append(" PRIMARY KEY (");
		stringBuilder2.Append(fieldDefinitions[0].Name);
		stringBuilder2.AppendLine("));");
		using NavSqlCommand navSqlCommand2 = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand2.SetCommandText(stringBuilder2.ToString());
		TraceSqlStatement(navSqlCommand2.Command.CommandText);
		navSqlCommand2.ExecuteNonQuery();
	}

	public void AlterTableToRemoveDeprecatedFieldsChangePkAndAddBaselineVersion()
	{
		for (int i = 0; i <= 1; i++)
		{
			FieldDefinition fieldDefinition = deprecatedFieldDefinitions[i];
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'{0}' AND OBJECT_ID = OBJECT_ID(N'{1}')) BEGIN ALTER TABLE {1} DROP COLUMN {0} END", fieldDefinition.Name, "[dbo].[$ndo$navappschematracking]");
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
			navSqlCommand.SetCommandText(stringBuilder.ToString());
			TraceSqlStatement(navSqlCommand.Command.CommandText);
			navSqlCommand.ExecuteNonQuery();
		}
		FieldDefinition fieldDefinition2 = fieldDefinitions[4];
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.AppendFormat(CultureInfo.InvariantCulture, "IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'{0}' AND OBJECT_ID = OBJECT_ID(N'{1}')) BEGIN ALTER TABLE {1} ADD ", fieldDefinition2.Name, "[dbo].[$ndo$navappschematracking]");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder2, fieldDefinition2.Name);
		stringBuilder2.Append(' ');
		NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder2, fieldDefinition2.NavType, SqlDataType.Default, fieldDefinition2.Length);
		stringBuilder2.Append(" DEFAULT ");
		stringBuilder2.Append(fieldDefinition2.AlterDefault);
		stringBuilder2.Append(fieldDefinition2.AllowNull ? " NULL" : " NOT NULL");
		stringBuilder2.Append(" END ");
		using (NavSqlCommand navSqlCommand2 = NavSqlCommand.Create(conScope.Connection))
		{
			navSqlCommand2.SetCommandText(stringBuilder2.ToString());
			navSqlCommand2.ExecuteNonQuery();
			TraceSqlStatement(navSqlCommand2.Command.CommandText);
		}
		using (NavSqlCommand navSqlCommand3 = NavSqlCommand.Create(conScope.Connection))
		{
			navSqlCommand3.SetCommandText(string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} DROP CONSTRAINT {1}", "[dbo].[$ndo$navappschematracking]", "PK$ndo$navappschematracking"));
			TraceSqlStatement(navSqlCommand3.Command.CommandText);
			navSqlCommand3.ExecuteNonQuery();
		}
		using NavSqlCommand navSqlCommand4 = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand4.SetCommandText(string.Format(CultureInfo.InvariantCulture, "ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2})", "[dbo].[$ndo$navappschematracking]", "PK$ndo$navappschematracking", fieldDefinitions[0].Name));
		TraceSqlStatement(navSqlCommand4.Command.CommandText);
		navSqlCommand4.ExecuteNonQuery();
	}

	public IEnumerable<AppSyncTrackingRecord> GetAll(bool hasBaseline = true)
	{
		List<AppSyncTrackingRecord> list = new List<AppSyncTrackingRecord>();
		string content = "Getting all tracking records";
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000017Q", DiagnosticParameter.SystemMetadata(content), MonitorType.SqlReadCount, Category.Extensions, createNewActivity: false))
		{
			string query = "IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'{0}') AND OBJECTPROPERTY(id, N'IsTable') = 1) SELECT [appid], [version], [name], [publisher] {1} FROM {0}";
			NavSqlCommand navSqlCommand = conScope.Connection.ExecuteCommandWithoutParameters(new NavSqlCombinedStringBasedCommandCacheKey(tenantDatabase, "SchemaTrackingRepository-GetAll", hasBaseline.ToString(CultureInfo.InvariantCulture)), () => string.Format(CultureInfo.InvariantCulture, query, "[dbo].[$ndo$navappschematracking]", hasBaseline ? ", [baselineversion]" : ""), (NavSqlCommand cmd) =>
			{
				cmd.ExecuteReader();
				return cmd;
			});
			while (navSqlCommand.ReadNextRow())
			{
				AppSyncTrackingRecord appSyncTrackingRecord = new AppSyncTrackingRecord();
				appSyncTrackingRecord.AppId = AppId.op_Implicit(navSqlCommand.Reader.GetGuid(0));
				appSyncTrackingRecord.Version = Version.Parse(navSqlCommand.Reader.GetString(1));
				appSyncTrackingRecord.Name = navSqlCommand.Reader.GetString(2);
				appSyncTrackingRecord.Publisher = navSqlCommand.Reader.GetString(3);
				appSyncTrackingRecord.BaselineVersion = ReadBaselineVersion(navSqlCommand.Reader, hasBaseline);
				list.Add(appSyncTrackingRecord);
			}
			return list;
		}
	}

	private static Version ReadBaselineVersion(SqlDataReader reader, bool hasBaseline = true)
	{
		string text = (hasBaseline ? reader.GetString(4) : null);
		if (!string.IsNullOrWhiteSpace(text))
		{
			return Version.Parse(text);
		}
		return new Version(0, 0, 0, 0);
	}

	public AppSyncTrackingRecord GetForApp(AppId appId)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Getting tracking record for app -- AppId:{0}", appId);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000017R", DiagnosticParameter.SystemMetadata(content), MonitorType.SqlReadCount, Category.Extensions, createNewActivity: false))
		{
			string commandText = string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'{0}') AND OBJECTPROPERTY(id, N'IsTable') = 1) SELECT [appid], [version], [name], [publisher], [baselineversion] FROM {0} WHERE [appid] = @0", "[dbo].[$ndo$navappschematracking]");
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
			navSqlCommand.Command.CommandText = commandText;
			navSqlCommand.Command.Parameters.Add(new SqlParameter("@0", fieldDefinitions[0].SqlType));
			navSqlCommand.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)appId).Value;
			using SqlDataReader sqlDataReader = navSqlCommand.ExecuteReader();
			if (sqlDataReader.Read())
			{
				AppSyncTrackingRecord appSyncTrackingRecord = new AppSyncTrackingRecord();
				appSyncTrackingRecord.AppId = AppId.op_Implicit(sqlDataReader.GetGuid(0));
				appSyncTrackingRecord.Version = Version.Parse(sqlDataReader.GetString(1));
				appSyncTrackingRecord.Name = sqlDataReader.GetString(2);
				appSyncTrackingRecord.Publisher = sqlDataReader.GetString(3);
				appSyncTrackingRecord.BaselineVersion = ReadBaselineVersion(sqlDataReader);
				return appSyncTrackingRecord;
			}
		}
		return null;
	}

	public void InsertOrUpdate(AppSyncTrackingRecord record, bool forceUpdateBaseline)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Inserting or updating tracking record -- AppId:{0}; Name:{1}; Version:{2}; Publisher:{3}; BaselineVersion:{4}; ForceUpdateBaseline:{5}", ((WrappedValue<Guid>)(object)record.AppId).Value, record.Name, record.Version, record.Publisher, record.BaselineVersion, forceUpdateBaseline);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000017S", DiagnosticParameter.SystemMetadata(content), MonitorType.SqlReadCount, Category.Extensions, createNewActivity: false))
		{
			string text = (forceUpdateBaseline ? "UPDATE {0} SET [version] = @1, [name] = @2, [publisher] = @3, [baselineversion] = @4 where [appid] = @0 " : "UPDATE {0} SET [version] = @1, [name] = @2, [publisher] = @3 where [appid] = @0 ");
			string query = text + "IF @@ROWCOUNT = 0 INSERT INTO {0} ([appid], [version], [name], [publisher], [baselineversion]) VALUES (@0, @1, @2, @3, @4)";
			conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("SchemaTrackingRepository-InsertOrUpdate-" + forceUpdateBaseline, tenantDatabase), () => string.Format(CultureInfo.InvariantCulture, query, "[dbo].[$ndo$navappschematracking]"), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", fieldDefinitions[0].SqlType));
				cmd.Command.Parameters.Add(new SqlParameter("@1", fieldDefinitions[1].SqlType, fieldDefinitions[1].Length));
				cmd.Command.Parameters.Add(new SqlParameter("@2", fieldDefinitions[2].SqlType, fieldDefinitions[2].Length));
				cmd.Command.Parameters.Add(new SqlParameter("@3", fieldDefinitions[3].SqlType, fieldDefinitions[3].Length));
				cmd.Command.Parameters.Add(new SqlParameter("@4", fieldDefinitions[4].SqlType, fieldDefinitions[4].Length));
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)record.AppId).Value;
				cmd.Command.Parameters[1].Value = record.Version.ToString();
				cmd.Command.Parameters[2].Value = record.Name.ToString();
				cmd.Command.Parameters[3].Value = record.Publisher.ToString();
				cmd.Command.Parameters[4].Value = record.BaselineVersion.ToString();
			}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		}
	}

	public void DeleteForApp(AppId appId)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Deleting tracking record for app -- AppId:{0}", appId);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000017T", DiagnosticParameter.SystemMetadata(content), MonitorType.SqlReadCount, Category.Extensions, createNewActivity: false))
		{
			string query = "DELETE FROM {0} WHERE [appid] = @0";
			conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("SchemaTrackingRepository-DeleteForApp", tenantDatabase), () => string.Format(CultureInfo.InvariantCulture, query, "[dbo].[$ndo$navappschematracking]"), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", fieldDefinitions[0].SqlType));
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)appId).Value;
			}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		}
	}
}
