using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class V2TableRenameCommandFactory
{
	private readonly NavDatabase tenantDatabase;

	private readonly IAppSyncTrackingRepository trackingRepo;

	private readonly IAppSyncSnapshotRepository snapshotRepo;

	public V2TableRenameCommandFactory(NavDatabase tenantDatabase, IAppSyncTrackingRepository trackingRepo, IAppSyncSnapshotRepository snapshotRepo)
	{
		this.tenantDatabase = tenantDatabase;
		this.trackingRepo = trackingRepo;
		this.snapshotRepo = snapshotRepo;
	}

	public List<string> GetCommandsToAddAppIdSuffixToAllSyncedApps()
	{
		List<string> list = new List<string>();
		List<AppSyncTrackingRecord> list2 = trackingRepo.GetAll(hasBaseline: false).ToList();
		if (list2.Any())
		{
			int[] companyTokens = (tenantDatabase.TenantDatabaseProperties.PerCompanySchema ? tenantDatabase.GetCompanyTokens() : new int[1] { -2147483648 });
			foreach (AppSyncTrackingRecord item in list2)
			{
				list.AddRange(GetCommandsToAddAppIdSuffixToSyncedApp(item.AppId, companyTokens));
			}
		}
		return list;
	}

	public List<string> GetCommandsToAddAppIdSuffixToSyncedApp(AppId appId, int[] companyTokens)
	{
		List<string> list = new List<string>();
		IEnumerable<AppSyncSnapshotRecord> allForApp = snapshotRepo.GetAllForApp(appId);
		foreach (AppSyncSnapshotRecord item in allForApp.Where((AppSyncSnapshotRecord s) => !s.IsTableExtension))
		{
			if (item.TableSnapshot != null)
			{
				NCLMetaTable nCLMetaTable = NCLMetaTable.CreateEmptyNCLMetaTable(null, item.TargetTableId, NavAppGroup.BaseGroup);
				nCLMetaTable.AssignFromMetaTable(item.TableSnapshot, databaseConvertedFromNav2013: false);
				list.AddRange(GetCommandsToAddAppIdSuffixToSyncedAppTable(nCLMetaTable, companyTokens));
			}
		}
		return list;
	}

	public List<string> GetCommandsToAddAppIdSuffixToSyncedAppTable(NCLMetaTable table, int[] companyTokens)
	{
		List<string> list = new List<string>();
		int[] array = (table.DataPerCompany ? companyTokens : new int[1]);
		int[] array2 = array;
		foreach (int companyToken in array2)
		{
			HandleSiftDrops(list, table, companyToken);
			HandlePKRename(list, table, companyToken);
			HandleTableRename(list, table, companyToken);
			HandleConstraintRenames(list, table, companyToken);
			HandleSiftCreates(list, table, companyToken);
		}
		return list;
	}

	private void HandleSiftDrops(List<string> commands, NCLMetaTable table, int companyToken)
	{
		string text = BuildOldTableName(table, companyToken);
		List<NCLMetaKey> list = table.Keys.Where((NCLMetaKey key) => key.MaintainSumIndex).ToList();
		foreach (NCLMetaKey item in list)
		{
			string command = string.Format(CultureInfo.InvariantCulture, "IF (SELECT OBJECTPROPERTY(OBJECT_ID(N'{0}.\"{1}{2}{3}\"'), N'IsView'))=1 DROP VIEW {0}.\"{1}{2}{3}\"", "dbo", text, "$VSIFT$", item.SqlIndexNo);
			LogAndAddCommand(commands, command);
			if (!string.IsNullOrEmpty(item.KeyName))
			{
				string command2 = string.Format(CultureInfo.InvariantCulture, "IF (SELECT OBJECTPROPERTY(OBJECT_ID(N'{0}.\"{1}{2}{3}\"'), N'IsView'))=1 DROP VIEW {0}.\"{1}{2}{3}\"", "dbo", text, "$VSIFT$", item.KeyName);
				LogAndAddCommand(commands, command2);
			}
		}
	}

	private void HandlePKRename(List<string> commands, NCLMetaTable table, int companyToken)
	{
		string text = BuildOldTableName(table, companyToken);
		string text2 = BuildNewTableName(table, companyToken);
		string command = string.Format(CultureInfo.InvariantCulture, "IF (SELECT OBJECTPROPERTY(OBJECT_ID(N'{0}.\"{1}${2}\"'), N'IsPrimaryKey'))=1 BEGIN EXEC sp_rename N'{0}.\"{1}${2}\"', N'{3}${4}' END", "dbo", text, table.PrimaryKey.KeyName, text2, table.PrimaryKey.KeyName);
		LogAndAddCommand(commands, command);
		string command2 = string.Format(CultureInfo.InvariantCulture, "IF (SELECT OBJECTPROPERTY(OBJECT_ID(N'{0}.\"{1}${2}\"'), N'IsPrimaryKey'))=1 BEGIN EXEC sp_rename N'{0}.\"{1}${2}\"', N'{3}${4}' END", "dbo", text, table.PrimaryKey.SqlIndexNo, text2, table.PrimaryKey.KeyName);
		LogAndAddCommand(commands, command2);
	}

	private void HandleTableRename(List<string> commands, NCLMetaTable table, int companyToken)
	{
		string arg = BuildOldTableName(table, companyToken);
		string arg2 = BuildNewTableName(table, companyToken);
		string command = string.Format(CultureInfo.InvariantCulture, "IF (SELECT OBJECTPROPERTY(OBJECT_ID(N'{0}.\"{1}\"'), N'IsTable'))=1 BEGIN EXEC sp_rename N'{0}.\"{1}\"', N'{2}' END", "dbo", arg, arg2);
		LogAndAddCommand(commands, command);
	}

	private void HandleConstraintRenames(List<string> commands, NCLMetaTable table, int companyToken)
	{
		string text = BuildOldTableName(table, companyToken);
		string text2 = BuildNewTableName(table, companyToken);
		IEnumerable<NCLMetaField> enumerable = table.Fields.Where((NCLMetaField f) => !f.FieldIsTimestamp && !f.FieldIsPartOfPrimaryKey && !f.FieldIsAutoIncrement && f.FieldNclType != NavNclType.NavBlob && f.FieldClass == FieldClass.Normal);
		foreach (NCLMetaField item in enumerable)
		{
			string arg = text + item.SqlColumnName;
			string arg2 = text2 + item.SqlColumnName;
			string command = string.Format(CultureInfo.InvariantCulture, "IF (SELECT OBJECTPROPERTY(OBJECT_ID(N'{0}.\"{1}\"'), N'IsDefaultCnst'))=1 BEGIN EXEC sp_rename N'{0}.\"{1}\"', N'{2}' END", "dbo", arg, arg2);
			LogAndAddCommand(commands, command);
		}
	}

	private void HandleSiftCreates(List<string> commands, NCLMetaTable table, int companyToken)
	{
		List<NCLMetaKey> list = table.Keys.Where((NCLMetaKey key) => key.MaintainSumIndex).ToList();
		foreach (NCLMetaKey item in list)
		{
			string command = NavSqlDdlCommandBuilder.BuildCreateSiftIndexCommandText(tenantDatabase, item, companyToken, NavAppTableContext.Main);
			string command2 = NavSqlDdlCommandBuilder.BuildCreateSiftClusteredIndexCommandText(tenantDatabase, item, companyToken, NavAppTableContext.Main);
			LogAndAddCommand(commands, command);
			LogAndAddCommand(commands, command2);
		}
	}

	private string BuildOldTableName(NCLMetaTable table, int companyToken)
	{
		StringBuilder stringBuilder = new StringBuilder();
		NavSqlStatementHelper.AppendTableNameCompanyQualified(stringBuilder, table, tenantDatabase, companyToken, NavAppTableContext.Main, null, skipQualifyingV2Tables: true);
		return stringBuilder.ToString();
	}

	private string BuildNewTableName(NCLMetaTable table, int companyToken)
	{
		StringBuilder stringBuilder = new StringBuilder();
		NavSqlStatementHelper.AppendTableNameCompanyQualified(stringBuilder, table, tenantDatabase, companyToken, NavAppTableContext.Main);
		return stringBuilder.ToString();
	}

	private static void LogAndAddCommand(List<string> commands, string command)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001QH", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, command);
		commands.Add(command);
	}
}
