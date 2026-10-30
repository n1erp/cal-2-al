using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class V2AbnormalTableFixer
{
	private readonly NavDatabase tenantDatabase;

	private readonly NavSqlConnectionScope conScope;

	private readonly IAppSyncSnapshotRepository snapshotRepo;

	public V2AbnormalTableFixer(NavDatabase tenantDatabase, NavSqlConnectionScope conScope, IAppSyncSnapshotRepository snapshotRepo)
	{
		this.tenantDatabase = tenantDatabase;
		this.conScope = conScope;
		this.snapshotRepo = snapshotRepo;
	}

	public void RemoveNonNormalSnapshotTables(IList<AppSyncSnapshotRecord> snapshots)
	{
		if (!snapshots.Any())
		{
			return;
		}
		for (int num = snapshots.Count - 1; num >= 0; num--)
		{
			AppSyncSnapshotRecord appSyncSnapshotRecord = snapshots[num];
			if (appSyncSnapshotRecord.TableSnapshot != null && appSyncSnapshotRecord.TableSnapshot.IsExternalDataSource)
			{
				NCLMetaTable nCLMetaTable = NCLMetaTable.CreateEmptyNCLMetaTable(null, appSyncSnapshotRecord.TargetTableId, NavAppGroup.BaseGroup);
				nCLMetaTable.AssignFromMetaTable(appSyncSnapshotRecord.TableSnapshot, databaseConvertedFromNav2013: false);
				snapshotRepo.Delete(appSyncSnapshotRecord.AppId, appSyncSnapshotRecord.TargetTableId);
				string text = CreateDropCommand(nCLMetaTable);
				LogCommand(text);
				using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection))
				{
					navSqlCommand.SetCommandText(text);
					navSqlCommand.ExecuteNonQuery();
				}
				snapshots.RemoveAt(num);
			}
		}
	}

	private string CreateDropCommand(NCLMetaTable table)
	{
		string arg = BuildTableName(table, 0);
		return string.Format(CultureInfo.InvariantCulture, "IF (SELECT OBJECTPROPERTY(OBJECT_ID(N'{0}.\"{1}\"'), N'IsTable'))=1 BEGIN DROP TABLE {0}.\"{1}\" END", "dbo", arg);
	}

	private string BuildTableName(NCLMetaTable table, int companyToken)
	{
		StringBuilder stringBuilder = new StringBuilder();
		NavSqlStatementHelper.AppendTableNameCompanyQualified(stringBuilder, table, tenantDatabase, companyToken, NavAppTableContext.Main);
		return stringBuilder.ToString();
	}

	private static void LogCommand(string command)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001R6", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, command);
	}
}
