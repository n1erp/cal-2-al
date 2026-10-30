using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class DatabaseAppSyncSnapshotRepository : IAppSyncSnapshotRepository
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

	public const string TableName = "[dbo].[$ndo$navappschemasnapshot]";

	private const string PrimaryKeyName = "PK$ndo$navappschemasnapshot";

	private static readonly Lazy<DataContractSerializer> serializer = new Lazy<DataContractSerializer>(() => new DataContractSerializer(typeof(MetaTable)));

	private readonly NavDatabase tenantDatabase;

	private readonly NavSqlConnectionScope conScope;

	private static readonly FieldDefinition[] fieldDefinitions = new FieldDefinition[7]
	{
		new FieldDefinition("appid", NavNclType.NavGuid, SqlDbType.UniqueIdentifier, 0, allowNull: false),
		new FieldDefinition("tableid", NavNclType.NavInteger, SqlDbType.Int, 0, allowNull: false),
		new FieldDefinition("istableextension", NavNclType.NavBoolean, SqlDbType.TinyInt, 0, allowNull: false),
		new FieldDefinition("metadata", NavNclType.NavBlob, SqlDbType.Image, int.MaxValue, allowNull: true),
		new FieldDefinition("targettablename", NavNclType.NavText, SqlDbType.NVarChar, 50, allowNull: false, "N''"),
		new FieldDefinition("originalobjectid", NavNclType.NavInteger, SqlDbType.Int, 0, allowNull: false, "0"),
		new FieldDefinition("originalobjectname", NavNclType.NavText, SqlDbType.NVarChar, 50, allowNull: false, "N''")
	};

	public DatabaseAppSyncSnapshotRepository(NavDatabase tenantDatabase, NavSqlConnectionScope conScope)
	{
		this.tenantDatabase = tenantDatabase;
		this.conScope = conScope;
	}

	private static void TraceSqlStatement(string statement)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000017L", Category.Extensions, Verbosity.Verbose, PrivacyClassification.SystemMetadata, statement);
	}

	public void CreateTable()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'{0}') AND OBJECTPROPERTY(id, N'IsTable') = 1) ", "[dbo].[$ndo$navappschemasnapshot]");
		stringBuilder.Append("CREATE TABLE ");
		stringBuilder.Append("[dbo].[$ndo$navappschemasnapshot]");
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
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, "PK$ndo$navappschemasnapshot");
		stringBuilder.Append(" PRIMARY KEY (");
		stringBuilder.Append(fieldDefinitions[0].Name);
		stringBuilder.Append(", ");
		stringBuilder.Append(fieldDefinitions[1].Name);
		stringBuilder.AppendLine("));");
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		TraceSqlStatement(navSqlCommand.Command.CommandText);
		navSqlCommand.ExecuteNonQuery();
	}

	public void AlterTableToTrackAdditionalNameAndIdData()
	{
		for (int i = 4; i <= 6; i++)
		{
			FieldDefinition fieldDefinition = fieldDefinitions[i];
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'{0}' AND OBJECT_ID = OBJECT_ID(N'{1}')) BEGIN ALTER TABLE {1} ADD ", fieldDefinition.Name, "[dbo].[$ndo$navappschemasnapshot]");
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, fieldDefinition.Name);
			stringBuilder.Append(' ');
			NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder, fieldDefinition.NavType, SqlDataType.Default, fieldDefinition.Length);
			stringBuilder.Append(" DEFAULT ");
			stringBuilder.Append(fieldDefinition.AlterDefault);
			stringBuilder.Append(fieldDefinition.AllowNull ? " NULL" : " NOT NULL");
			stringBuilder.Append(" END ");
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
			navSqlCommand.SetCommandText(stringBuilder.ToString());
			navSqlCommand.ExecuteNonQuery();
			TraceSqlStatement(navSqlCommand.Command.CommandText);
		}
	}

	public IEnumerable<AppSyncSnapshotRecord> GetAllForApp(AppId appId)
	{
		List<AppSyncSnapshotRecord> list = new List<AppSyncSnapshotRecord>();
		DiagnosticParameter name = DiagnosticParameter.SystemMetadata("Getting all snapshot records for app -- AppId:" + (object)appId);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000017M", name, MonitorType.SqlReadCount, Category.Extensions, createNewActivity: false))
		{
			string query = string.Format(CultureInfo.InvariantCulture, "SELECT [tableid], [istableextension], [metadata], [targettablename], [originalobjectid], [originalobjectname] FROM {0} WHERE [appid] = @0", "[dbo].[$ndo$navappschemasnapshot]");
			NavSqlCommand navSqlCommand = conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("SchemaSnapshotRepository-GetAllForApp", tenantDatabase), () => string.Format(CultureInfo.InvariantCulture, query, "[dbo].[$ndo$navappschemasnapshot]"), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", fieldDefinitions[0].SqlType));
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)appId).Value;
			}, (NavSqlCommand cmd) =>
			{
				cmd.ExecuteReader();
				return cmd;
			});
			while (navSqlCommand.ReadNextRow())
			{
				AppSyncSnapshotRecord appSyncSnapshotRecord = new AppSyncSnapshotRecord();
				appSyncSnapshotRecord.AppId = appId;
				appSyncSnapshotRecord.TargetTableId = navSqlCommand.Reader.GetInt32(0);
				appSyncSnapshotRecord.IsTableExtension = navSqlCommand.Reader.GetByte(1) != 0;
				appSyncSnapshotRecord.TableSnapshot = ReadAndDeserializeMetaTable(navSqlCommand, 2);
				appSyncSnapshotRecord.TargetTableName = navSqlCommand.Reader.GetString(3);
				appSyncSnapshotRecord.OriginalObjectId = navSqlCommand.Reader.GetInt32(4);
				appSyncSnapshotRecord.OriginalObjectName = navSqlCommand.Reader.GetString(5);
				list.Add(appSyncSnapshotRecord);
			}
			return list;
		}
	}

	public void InsertOrUpdate(AppSyncSnapshotRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		string content = string.Format(CultureInfo.InvariantCulture, "Inserting or updating snapshot record -- AppId:{0}; TableId:{1}; IsTableExtension:{2}; TargetTableName:{3}; OriginalObjectId:{4}; OriginalTableName:{5}", ((WrappedValue<Guid>)(object)record.AppId).Value, record.TargetTableId, record.IsTableExtension, record.TargetTableName, record.OriginalObjectId, record.OriginalObjectName);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000017N", DiagnosticParameter.Oii(content), MonitorType.SqlReadCount, Category.Extensions, createNewActivity: false))
		{
			string query = "UPDATE {0} SET [istableextension] = @2, [metadata] = @3, [targettablename] = @4, [originalobjectid] = @5, [originalobjectname] = @6 where [appid] = @0 AND [tableid] = @1 IF @@ROWCOUNT = 0 INSERT INTO {0} ([appid], [tableid], [istableextension], [metadata], [targettablename], [originalobjectid], [originalobjectname]) VALUES (@0, @1, @2, @3, @4, @5, @6)";
			XmlDocument xmlDocument = SerializeMetaTable(record.TableSnapshot);
			using NavBLOB navBLOB = new NavBLOB(Encoding.UTF8.GetBytes(xmlDocument.OuterXml));
			NavBLOB localBlob = navBLOB;
			localBlob.IsDirty = true;
			conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("SchemaSnapshotRepository-InsertOrUpdate", tenantDatabase), () => string.Format(CultureInfo.InvariantCulture, query, "[dbo].[$ndo$navappschemasnapshot]"), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", fieldDefinitions[0].SqlType));
				cmd.Command.Parameters.Add(new SqlParameter("@1", fieldDefinitions[1].SqlType));
				cmd.Command.Parameters.Add(new SqlParameter("@2", fieldDefinitions[2].SqlType));
				cmd.Command.Parameters.Add(new SqlParameter("@3", fieldDefinitions[3].SqlType, fieldDefinitions[3].Length));
				cmd.Command.Parameters.Add(new SqlParameter("@4", fieldDefinitions[4].SqlType, fieldDefinitions[4].Length));
				cmd.Command.Parameters.Add(new SqlParameter("@5", fieldDefinitions[5].SqlType));
				cmd.Command.Parameters.Add(new SqlParameter("@6", fieldDefinitions[6].SqlType, fieldDefinitions[6].Length));
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)record.AppId).Value;
				cmd.Command.Parameters[1].Value = record.TargetTableId;
				cmd.Command.Parameters[2].Value = (record.IsTableExtension ? 1 : 0);
				cmd.Command.Parameters[3].Value = localBlob.GetSqlWritableValue(SqlDataType.Default, int.MaxValue, compressed: true);
				cmd.Command.Parameters[4].Value = record.TargetTableName ?? string.Empty;
				cmd.Command.Parameters[5].Value = record.OriginalObjectId;
				cmd.Command.Parameters[6].Value = record.OriginalObjectName ?? string.Empty;
			}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		}
	}

	public void DeleteAllForApp(AppId appId)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Deleting snapshot records for app -- AppId:{0}", appId);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000017O", DiagnosticParameter.SystemMetadata(content), MonitorType.SqlReadCount, Category.Extensions, createNewActivity: false))
		{
			string query = "DELETE FROM {0} WHERE [appid] = @0";
			conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("SchemaSnapshotRepository-DeleteAllForApp", tenantDatabase), () => string.Format(CultureInfo.InvariantCulture, query, "[dbo].[$ndo$navappschemasnapshot]"), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", fieldDefinitions[0].SqlType));
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)appId).Value;
			}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		}
	}

	public void Delete(AppId appId, int tableId)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Deleting snapshot record for app -- AppId:{0}; TableId:{1}", appId, tableId);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00001R5", DiagnosticParameter.SystemMetadata(content), MonitorType.SqlReadCount, Category.Extensions, createNewActivity: false))
		{
			string query = "DELETE FROM {0} WHERE [appid] = @0 AND [tableid] = @1";
			conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("SchemaSnapshotRepository-Delete", tenantDatabase), () => string.Format(CultureInfo.InvariantCulture, query, "[dbo].[$ndo$navappschemasnapshot]"), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", fieldDefinitions[0].SqlType));
				cmd.Command.Parameters.Add(new SqlParameter("@1", fieldDefinitions[1].SqlType));
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = ((WrappedValue<Guid>)(object)appId).Value;
				cmd.Command.Parameters[1].Value = tableId;
			}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		}
	}

	private static MetaTable ReadAndDeserializeMetaTable(NavSqlCommand command, int ordinal)
	{
		using NavBLOB navBLOB = new NavBLOB();
		command.GetBlobDataFromReader(isCompressed: true, ordinal, navBLOB);
		XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
		xmlDocument.Load(XmlHelper.SafeXmlReader(navBLOB.GetNewStream()));
		return DeserializeMetaTable(xmlDocument.DocumentElement);
	}

	internal static MetaTable DeserializeMetaTable(XmlElement root)
	{
		return NavSqlObjectMetadataSnapshotHandler.GetMetaTableFromMetadata(root, 2);
	}

	internal static XmlDocument SerializeMetaTable(MetaTable metaTable)
	{
		metaTable.SkipValidatingEvents = true;
		XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
		XmlWriter xmlWriter = xmlDocument.CreateNavigator().AppendChild();
		try
		{
			serializer.Value.WriteObject(xmlWriter, metaTable);
		}
		catch (InvalidOperationException innerException)
		{
			throw new InvalidOperationException(string.Format(NavCurrentThread.Session.RuntimeLanguage, "Cannot serialize the MetaTable"), innerException);
		}
		xmlWriter.Close();
		return xmlDocument;
	}
}
