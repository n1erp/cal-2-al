using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlTenantPropertiesSync
{
	public enum DatabaseVersion
	{
		None = 0,
		ConvertedFromPreviousVersion = 1000,
		InitialVersionNav8 = 1010,
		VersionLicenseAdded = 1020,
		VersionRecreateTableIndexesWithDisabledIndexes = VersionLicenseAdded,
		VersionEncryptionKeyFileNameAdded = 1040,
		VersionEncryptionKeyHashAdded = 1050,
		VersionExchangeAuthenticationMetadataUrl = 1060,
		VersionRefactoredTenantProperties = 1070,
		VersionMetadataSnapshotNeedsAltering = 1075,
		ApplicationVersionAdded = 1080,
		VersionAddingCompanyIdPreSharedDb = 1090,
		VersionAddingExtensionV2SyncTablesPreSharedDb = 1091,
		VersionModifyingExtensionV2SyncTablesPreSharedDb = 1092,
		VersionAddingAppUninstallTrackingPreSharedDb = 1093,
		VersionAddingContextToExtensionV2SnapshotPreSharedDb = 1094,
		VersionSharedDatabase = 1100,
		VersionSharedDbAddingCompanyId = 1110,
		VersionAddingExtensionV2SyncTablesPostSharedDb = 1120,
		VersionModifyingExtensionV2SyncTablesPostSharedDb = 1130,
		VersionAddingAppUninstallTrackingPostSharedDb = 1140,
		VersionAddingContextToExtensionV2SnapshotPostSharedDb = 1150,
		VersionOneUpgradePath = 1160,
		VersionTenantCompaniesTable = 1170,
		VersionSystemScheduledTaskTable = 1180,
		VersionTenantCompaniesTableCompanyState = 1190,
		VersionTenantPropertiesLastModified = 1200,
		VersionTenantPropertiesLicenseInfo = 1210,
		VersionRenameV2TablesToHaveAppId = 1220,
		VersionMetadataSnapshotNeedsAlteringForSchemaHash = 1240,
		VersionRenameDefaultConstraintToUseHashCode = 1250
	}

	public const string TenantDatabasePropertiesTableName = "[dbo].[$ndo$tenantdatabaseproperty]";

	public const string TenantPropertiesTableName = "[dbo].[$ndo$tenantproperty]";

	public const string ObjectMetadataSnapshotTableName = "[dbo].[Object Metadata Snapshot]";

	private readonly NavDatabase tenantDatabase;

	private readonly object lockObj = new object();

	public static DatabaseVersion CurrentVersion => DatabaseVersion.VersionRenameDefaultConstraintToUseHashCode;

	public bool UpgradeFromNav2013 { get; private set; }

	public bool FillTenantCompanyTable { get; private set; }

	public bool AlterObjectMetadataSnapshot { get; private set; }

	public bool UpgradeAddCompanyId { get; private set; }

	public bool AddSchemaHashToObjectMetadata { get; private set; }

	private NavSqlTenantPropertiesSync(NavDatabase tenantDatabase)
	{
		if (tenantDatabase == null)
		{
			throw new ArgumentNullException("tenantDatabase");
		}
		if (tenantDatabase.DatabaseType != NavDatabaseType.Tenant)
		{
			throw new ArgumentException("Given database is not a tenant database");
		}
		this.tenantDatabase = tenantDatabase;
	}

	public static void CheckTenantDatabaseVersion(NavDatabase tenantDatabase)
	{
		if (tenantDatabase == null)
		{
			throw new ArgumentNullException("tenantDatabase");
		}
		if (tenantDatabase.DatabaseType != NavDatabaseType.Tenant)
		{
			throw new ArgumentException("Given database is not a tenant database");
		}
		if (tenantDatabase.TenantDatabaseProperties.VersionNo != (int)CurrentVersion)
		{
			tenantDatabase.TenantDatabaseProperties.Refresh();
			if (tenantDatabase.TenantDatabaseProperties.VersionNo != (int)CurrentVersion)
			{
				throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.UnSupportedTenantDatabaseVersion, tenantDatabase.DatabaseName, tenantDatabase.TenantDatabaseProperties.VersionNo, ProductNames.ProductNameServer));
			}
		}
	}

	public static NavSqlTenantPropertiesSync Sync(NavDatabase tenantDatabase)
	{
		NavSqlTenantPropertiesSync navSqlTenantPropertiesSync = new NavSqlTenantPropertiesSync(tenantDatabase);
		navSqlTenantPropertiesSync.Sync();
		return navSqlTenantPropertiesSync;
	}

	private void Sync()
	{
		lock (lockObj)
		{
			bool flag = false;
			if (!tenantDatabase.TenantDatabaseProperties.TableExists)
			{
				using (NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true))
				{
					if (tenantDatabase.IsNewTenantDatabase)
					{
						GetCreateTenantDatabasePropertiesTableCommand(conScope, (int)CurrentVersion).ExecuteNonQuery();
						GetCreateTenantPropertiesTableCommand(conScope).ExecuteNonQuery();
						GetCreateTenantDatabaseVersionTableCommand(conScope).ExecuteNonQuery();
						new DatabaseAppSyncSnapshotRepository(tenantDatabase, conScope).CreateTable();
						new DatabaseAppSyncTrackingRepository(tenantDatabase, conScope).CreateTable(forceDropOld: false);
						new DatabaseUninstalledAppRepository(tenantDatabase, conScope).CreateTable();
						GetGenerateDefaultTenantDatabasePropertiesRecordCommand(conScope).ExecuteNonQuery();
						GetGenerateDefaultTenantDatabaseVersionRecordCommand(conScope).ExecuteNonQuery();
						NavSqlSystemScheduledTaskUpgrade.GetCreateTableCommand(conScope).ExecuteNonQuery();
						RefreshPropertiesFromDatabase();
						FillTenantCompanyTable = NavSqlTenantCompaniesUpgrade.CreateTable(conScope);
					}
					else
					{
						UpgradeTenantPropertiesBeforeRefactoredTenantDatabaseProperties(conScope);
						flag = true;
					}
				}
				RefreshPropertiesFromDatabase();
			}
			if (tenantDatabase.TenantDatabaseProperties.VersionNo < 1160)
			{
				using (NavSqlConnectionScope conScope2 = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true))
				{
					UpgradeAlongSplitPath(conScope2);
				}
				RefreshPropertiesFromDatabase();
			}
			if (tenantDatabase.TenantDatabaseProperties.VersionNo < (int)CurrentVersion)
			{
				using (NavSqlConnectionScope conScope3 = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true))
				{
					Upgrade(conScope3);
				}
				RefreshPropertiesFromDatabase();
			}
			else if (tenantDatabase.TenantDatabaseProperties.VersionNo > (int)CurrentVersion)
			{
				throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.UnSupportedTenantDatabaseVersion, tenantDatabase.DatabaseName, tenantDatabase.TenantDatabaseProperties.VersionNo, ProductNames.ProductNameServer));
			}
			if (flag)
			{
				tenantDatabase.TenantProperties.SetTenantDataVersion(NavSqlTenantProperties.UninitializedVersion.ToString());
			}
		}
	}

	private void RefreshPropertiesFromDatabase()
	{
		tenantDatabase.TenantDatabaseProperties.Refresh();
		tenantDatabase.TenantProperties.Refresh();
		tenantDatabase.TenantDatabaseVersion.Refresh();
	}

	private void Upgrade(NavSqlConnectionScope conScope)
	{
		lock (lockObj)
		{
			Queue<NavSqlCommand> commandQueue = new Queue<NavSqlCommand>();
			if (tenantDatabase.TenantDatabaseProperties.VersionNo < 1170)
			{
				FillTenantCompanyTable = NavSqlTenantCompaniesUpgrade.CreateTableBaseSchema(conScope);
			}
			if (tenantDatabase.TenantDatabaseProperties.VersionNo < 1180)
			{
				commandQueue.Enqueue(NavSqlSystemScheduledTaskUpgrade.GetCreateTableCommand(conScope));
			}
			if (tenantDatabase.TenantDatabaseProperties.VersionNo < 1190)
			{
				commandQueue.Enqueue(NavSqlTenantCompaniesUpgrade.GetAddCompanyStateSqlCommand(conScope));
			}
			if (tenantDatabase.TenantDatabaseProperties.VersionNo < 1200)
			{
				commandQueue.Enqueue(AlterTenantPropertiesTableAddField(conScope, 7, skipIfAlreadyExists: false, " NOT NULL DEFAULT getutcdate()"));
			}
			if (tenantDatabase.TenantDatabaseProperties.VersionNo < 1210)
			{
				commandQueue.Enqueue(AlterTenantPropertiesTableAddField(conScope, 8, skipIfAlreadyExists: true));
			}
			if (tenantDatabase.TenantDatabaseProperties.VersionNo < 1220)
			{
				DatabaseAppSyncTrackingRepository trackingRepo = new DatabaseAppSyncTrackingRepository(tenantDatabase, conScope);
				DatabaseAppSyncSnapshotRepository snapshotRepo = new DatabaseAppSyncSnapshotRepository(tenantDatabase, conScope);
				V2TableRenameCommandFactory v2TableRenameCommandFactory = new V2TableRenameCommandFactory(tenantDatabase, trackingRepo, snapshotRepo);
				IEnumerable<string> commandsToAddAppIdSuffixToAllSyncedApps = v2TableRenameCommandFactory.GetCommandsToAddAppIdSuffixToAllSyncedApps();
				commandsToAddAppIdSuffixToAllSyncedApps.ForEach((string ct) =>
				{
					NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
					navSqlCommand.SetCommandText(ct);
					commandQueue.Enqueue(navSqlCommand);
				});
			}
			if (tenantDatabase.TenantDatabaseProperties.VersionNo < 1250)
			{
				IEnumerable<string> renameDefaultConstraintCommands = GetRenameDefaultConstraintCommands(conScope, tenantDatabase);
				renameDefaultConstraintCommands.ForEach((string ct) =>
				{
					NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
					navSqlCommand.SetCommandText(ct);
					commandQueue.Enqueue(navSqlCommand);
				});
			}
			NavSqlCommand updateTenantDatabasePropertiesVersionCommand = GetUpdateTenantDatabasePropertiesVersionCommand(conScope, CurrentVersion);
			commandQueue.Enqueue(updateTenantDatabasePropertiesVersionCommand);
			foreach (NavSqlCommand item in commandQueue)
			{
				item.ExecuteNonQuery();
			}
			if (tenantDatabase.TenantDatabaseProperties.VersionNo < 1240)
			{
				AddSchemaHashToObjectMetadata = true;
			}
		}
	}

	private static NavSqlCommand AlterTenantPropertiesTableAddField(NavSqlConnectionScope conScope, int fieldNo, bool skipIfAlreadyExists, string appendAfterFieldLength = null)
	{
		NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		StringBuilder stringBuilder = new StringBuilder();
		if (skipIfAlreadyExists)
		{
			stringBuilder.Append("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'");
			stringBuilder.Append("[dbo].[$ndo$tenantproperty]");
			stringBuilder.Append("') AND name = '").Append(NavSqlTenantProperties.FieldNames[fieldNo]).Append("') ");
			stringBuilder.AppendLine("BEGIN");
		}
		stringBuilder.Append("ALTER TABLE ");
		stringBuilder.Append("[dbo].[$ndo$tenantproperty]");
		stringBuilder.Append(" ADD ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, NavSqlTenantProperties.FieldNames[fieldNo]);
		stringBuilder.Append(' ');
		NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder, NavSqlTenantProperties.FieldTypes[fieldNo], SqlDataType.Default, NavSqlTenantProperties.FieldLengths[fieldNo]);
		if (appendAfterFieldLength != null)
		{
			stringBuilder.Append(appendAfterFieldLength);
		}
		if (skipIfAlreadyExists)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("END");
		}
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		return navSqlCommand;
	}

	private void UpgradeAlongSplitPath(NavSqlConnectionScope conScope)
	{
		Queue<NavSqlCommand> queue = new Queue<NavSqlCommand>();
		int versionNo = tenantDatabase.TenantDatabaseProperties.VersionNo;
		if (versionNo < 1075)
		{
			AlterObjectMetadataSnapshot = true;
		}
		if (versionNo < 1080)
		{
			GetCreateTenantDatabaseVersionTableCommand(conScope).ExecuteNonQuery();
			GetGenerateDefaultTenantDatabaseVersionRecordCommand(conScope).ExecuteNonQuery();
			NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
			StringBuilder stringBuilder = new StringBuilder();
			AddApplicationVersionField(stringBuilder, "[dbo].[$ndo$tenantproperty]");
			AddApplicationVersionField(stringBuilder, "[dbo].[$ndo$tenantdatabaseproperty]");
			navSqlCommand.SetCommandText(stringBuilder.ToString());
			queue.Enqueue(navSqlCommand);
		}
		if (versionNo >= 1100)
		{
			if (versionNo < 1110)
			{
				UpgradeAddCompanyId = true;
			}
			if (versionNo < 1092 || (versionNo < 1130 && versionNo >= 1100))
			{
				if (versionNo < 1120)
				{
					new DatabaseAppSyncSnapshotRepository(tenantDatabase, conScope).CreateTable();
					new DatabaseAppSyncTrackingRepository(tenantDatabase, conScope).CreateTable(forceDropOld: false);
				}
				if (versionNo < 1130)
				{
					new DatabaseAppSyncTrackingRepository(tenantDatabase, conScope).CreateTable(forceDropOld: true);
				}
			}
			if (versionNo < 1093 || (versionNo < 1140 && versionNo >= 1100))
			{
				new DatabaseUninstalledAppRepository(tenantDatabase, conScope).CreateTable();
			}
			if (versionNo < 1150)
			{
				new DatabaseAppSyncSnapshotRepository(tenantDatabase, conScope).AlterTableToTrackAdditionalNameAndIdData();
				new DatabaseAppSyncTrackingRepository(tenantDatabase, conScope).AlterTableToRemoveDeprecatedFieldsChangePkAndAddBaselineVersion();
			}
		}
		else
		{
			if (versionNo < 1090)
			{
				UpgradeAddCompanyId = true;
			}
			if (versionNo < 1091)
			{
				new DatabaseAppSyncSnapshotRepository(tenantDatabase, conScope).CreateTable();
				new DatabaseAppSyncTrackingRepository(tenantDatabase, conScope).CreateTable(forceDropOld: false);
			}
			if (versionNo < 1092)
			{
				new DatabaseAppSyncTrackingRepository(tenantDatabase, conScope).CreateTable(forceDropOld: true);
			}
			if (versionNo < 1093)
			{
				new DatabaseUninstalledAppRepository(tenantDatabase, conScope).CreateTable();
			}
			if (versionNo < 1094)
			{
				new DatabaseAppSyncSnapshotRepository(tenantDatabase, conScope).AlterTableToTrackAdditionalNameAndIdData();
				new DatabaseAppSyncTrackingRepository(tenantDatabase, conScope).AlterTableToRemoveDeprecatedFieldsChangePkAndAddBaselineVersion();
			}
			if (versionNo < 1160)
			{
				NavSqlCommand navSqlCommand2 = NavSqlCommand.Create(conScope.Connection);
				StringBuilder stringBuilder2 = new StringBuilder();
				stringBuilder2.AppendLine(FormattableString.Invariant(FormattableStringFactory.Create("\r\n                        ALTER TABLE {0}\r\n                        ADD [{1}]\r\n                            TINYINT NOT NULL DEFAULT 1;", "[dbo].[$ndo$tenantdatabaseproperty]", NavSqlTenantDatabaseProperties.FieldNames[2])));
				stringBuilder2.AppendLine(FormattableString.Invariant(FormattableStringFactory.Create("\r\n                        ALTER TABLE {0}\r\n                        ADD [{1}] INT NOT NULL IDENTITY(1,1);", "[dbo].[$ndo$tenantproperty]", NavSqlTenantProperties.FieldNames[4])));
				stringBuilder2.AppendLine(FormattableString.Invariant(FormattableStringFactory.Create("\r\n                        ALTER TABLE {0}\r\n                        ADD [{1}]\r\n                            INT NOT NULL DEFAULT {2};", "[dbo].[$ndo$tenantproperty]", NavSqlTenantProperties.FieldNames[6], 0)));
				stringBuilder2.AppendLine(FormattableString.Invariant(FormattableStringFactory.Create("\r\n                        CREATE UNIQUE INDEX IXDATASET ON {0} ([{1}]);", "[dbo].[$ndo$tenantproperty]", NavSqlTenantProperties.FieldNames[4])));
				navSqlCommand2.SetCommandText(stringBuilder2.ToString());
				queue.Enqueue(navSqlCommand2);
			}
		}
		NavSqlCommand updateTenantDatabasePropertiesVersionCommand = GetUpdateTenantDatabasePropertiesVersionCommand(conScope, DatabaseVersion.VersionOneUpgradePath);
		queue.Enqueue(updateTenantDatabasePropertiesVersionCommand);
		foreach (NavSqlCommand item in queue)
		{
			item.ExecuteNonQuery();
			item.Dispose();
		}
	}

	private void UpgradeTenantPropertiesBeforeRefactoredTenantDatabaseProperties(NavSqlConnectionScope conScope)
	{
		Queue<NavSqlCommand> queue = new Queue<NavSqlCommand>();
		int num = NavSqlTenantDatabaseProperties.ReadLegacyTenantDatabaseVersion(conScope, takeUpdateLock: true);
		if (num == 1000)
		{
			UpgradeFromNav2013 = true;
		}
		if (num < 1020)
		{
			NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
			StringBuilder stringBuilder = new StringBuilder("ALTER TABLE ");
			stringBuilder.Append("[dbo].[$ndo$tenantproperty]");
			stringBuilder.Append(" ADD ");
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, "License");
			NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder, NavNclType.NavBlob, SqlDataType.Default);
			navSqlCommand.SetCommandText(stringBuilder.ToString());
			queue.Enqueue(navSqlCommand);
		}
		if (num < 1040)
		{
			NavSqlCommand navSqlCommand2 = NavSqlCommand.Create(conScope.Connection);
			StringBuilder stringBuilder2 = new StringBuilder("ALTER TABLE ");
			stringBuilder2.Append("[dbo].[$ndo$tenantproperty]");
			stringBuilder2.Append(" ADD ");
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder2, "encryptionkeyfilename");
			NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder2, NavNclType.NavText, SqlDataType.Default, 128);
			navSqlCommand2.SetCommandText(stringBuilder2.ToString());
			queue.Enqueue(navSqlCommand2);
		}
		if (num < 1050)
		{
			NavSqlCommand navSqlCommand3 = NavSqlCommand.Create(conScope.Connection);
			StringBuilder stringBuilder3 = new StringBuilder("ALTER TABLE ");
			stringBuilder3.Append("[dbo].[$ndo$tenantproperty]");
			stringBuilder3.Append(" ADD ");
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder3, "encryptionkeyhash");
			NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder3, NavNclType.NavText, SqlDataType.Default, 128);
			navSqlCommand3.SetCommandText(stringBuilder3.ToString());
			queue.Enqueue(navSqlCommand3);
		}
		if (num < 1060)
		{
			NavSqlCommand navSqlCommand4 = NavSqlCommand.Create(conScope.Connection);
			string commandText = string.Format(CultureInfo.InvariantCulture, "IF (OBJECT_ID('{0}', 'U') IS NOT NULL) DELETE FROM {0} WHERE [Object Type] = 1 AND [Object ID] IN ({1})", "[dbo].[Object Metadata Snapshot]", string.Join(", ", SystemTables.VirtualTables));
			navSqlCommand4.SetCommandText(commandText);
			navSqlCommand4.ExecuteNonQuery();
		}
		if (num < 1070)
		{
			NavSqlCommand createTenantDatabasePropertiesTableCommand = GetCreateTenantDatabasePropertiesTableCommand(conScope, 1070);
			queue.Enqueue(createTenantDatabasePropertiesTableCommand);
			NavSqlCommand navSqlCommand5 = NavSqlCommand.Create(conScope.Connection);
			StringBuilder stringBuilder4 = new StringBuilder();
			string value = "versionno, collation";
			string value2 = 1070 + " AS versionno, collation";
			stringBuilder4.Append("INSERT INTO ").Append("[dbo].[$ndo$tenantdatabaseproperty]").Append('(')
				.Append(value)
				.AppendLine(") ");
			stringBuilder4.Append("SELECT TOP 1 ").Append(value2).Append(" FROM ")
				.Append("[dbo].[$ndo$tenantproperty]");
			navSqlCommand5.SetCommandText(stringBuilder4.ToString());
			queue.Enqueue(navSqlCommand5);
			NavSqlCommand navSqlCommand6 = NavSqlCommand.Create(conScope.Connection);
			StringBuilder stringBuilder5 = new StringBuilder();
			stringBuilder5.Append("ALTER TABLE ");
			stringBuilder5.Append("[dbo].[$ndo$tenantproperty]");
			stringBuilder5.Append(" DROP CONSTRAINT ");
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder5, "PK$ndo$tenantproperty");
			stringBuilder5.AppendLine(";");
			stringBuilder5.AppendLine("\r\nDECLARE @Table nvarchar(50) = '$ndo$tenantproperty'\r\nDECLARE @SQL nvarchar(MAX) = '';\r\nSELECT @SQL += 'ALTER TABLE [' + @Table + '] DROP CONSTRAINT ' + name +'; ' FROM sys.default_constraints d WHERE d.parent_object_id = OBJECT_ID(@Table);\r\nEXEC sp_executesql @SQL;");
			stringBuilder5.Append("ALTER TABLE ");
			stringBuilder5.Append("[dbo].[$ndo$tenantproperty]");
			stringBuilder5.Append(" DROP COLUMN ");
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder5, "tenantversionno");
			stringBuilder5.AppendLine(";");
			stringBuilder5.Append("ALTER TABLE ");
			stringBuilder5.Append("[dbo].[$ndo$tenantproperty]");
			stringBuilder5.Append(" DROP COLUMN ");
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder5, "collation");
			stringBuilder5.AppendLine(";");
			stringBuilder5.Append("ALTER TABLE ").Append("[dbo].[$ndo$tenantproperty]").Append(" ALTER COLUMN tenantid ");
			NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder5, NavNclType.NavText, SqlDataType.Default, 128);
			stringBuilder5.AppendLine(" COLLATE Latin1_General_100_CI_AS NOT NULL;");
			stringBuilder5.AppendFormat(CultureInfo.InvariantCulture, "ALTER TABLE {0} ADD CONSTRAINT PK$ndo$tenantproperty PRIMARY KEY ([tenantid]);", "[dbo].[$ndo$tenantproperty]").AppendLine();
			navSqlCommand6.SetCommandText(stringBuilder5.ToString());
			queue.Enqueue(navSqlCommand6);
		}
		foreach (NavSqlCommand item in queue)
		{
			item.ExecuteNonQuery();
			item.Dispose();
		}
	}

	private static void AddApplicationVersionField(StringBuilder alterCommandText, string tableName)
	{
		alterCommandText.Append("ALTER TABLE ");
		alterCommandText.Append(tableName);
		alterCommandText.Append(" ADD [").Append("applicationversion").Append(']');
		alterCommandText.AppendLine(" NVARCHAR(50) NOT NULL DEFAULT N'" + NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion + "';");
	}

	private static NavSqlCommand GetCreateTenantDatabasePropertiesTableCommand(NavSqlConnectionScope conScope, int version)
	{
		StringBuilder stringBuilder = new StringBuilder("CREATE TABLE ");
		stringBuilder.Append("[dbo].[$ndo$tenantdatabaseproperty]");
		stringBuilder.Append(" (");
		for (int i = 0; i < NavSqlTenantDatabaseProperties.FieldNames.Length; i++)
		{
			switch (i)
			{
			case 2:
				if (version >= 1100)
				{
					AppendToCreateTenantDatabasePropertiesTableCommand(stringBuilder, i);
				}
				break;
			case 3:
				if (version >= 1080)
				{
					AppendToCreateTenantDatabasePropertiesTableCommand(stringBuilder, i, " NOT NULL DEFAULT N'" + NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion + "'");
				}
				break;
			default:
				AppendToCreateTenantDatabasePropertiesTableCommand(stringBuilder, i);
				break;
			}
		}
		stringBuilder.Length--;
		stringBuilder.AppendLine(")");
		NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		return navSqlCommand;
	}

	private static void AppendToCreateTenantDatabasePropertiesTableCommand(StringBuilder commandText, int fieldNo, string appendAfterFieldLength = null)
	{
		NavSqlStatementHelper.AppendQuotedIdentifier(commandText, NavSqlTenantDatabaseProperties.FieldNames[fieldNo]);
		commandText.Append(' ');
		NavSqlStatementHelper.AppendSqlColumnTypeAndLength(commandText, NavSqlTenantDatabaseProperties.FieldTypes[fieldNo], SqlDataType.Default, NavSqlTenantDatabaseProperties.FieldLengths[fieldNo]);
		if (appendAfterFieldLength != null)
		{
			commandText.Append(appendAfterFieldLength);
		}
		commandText.Append(',');
	}

	private static NavSqlCommand GetGenerateDefaultTenantDatabasePropertiesRecordCommand(NavSqlConnectionScope conScope)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(FormattableString.Invariant(FormattableStringFactory.Create("DELETE FROM {0};", "[dbo].[$ndo$tenantdatabaseproperty]")));
		stringBuilder.AppendLine(FormattableString.Invariant(FormattableStringFactory.Create("\r\n                INSERT INTO {0}\r\n                VALUES(\r\n                    {1}\r\n                   ,'{2}'\r\n                   ,{3}\r\n                   ,'{4}'\r\n                );", "[dbo].[$ndo$tenantdatabaseproperty]", (int)CurrentVersion, NavGlobal.AppDatabase.SqlSortingProperties.Collation, (!conScope.Connection.Database.IsSharedTenantSchema) ? 1 : 0, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion)));
		NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		return navSqlCommand;
	}

	private static NavSqlCommand GetGenerateDefaultTenantDatabaseVersionRecordCommand(NavSqlConnectionScope conScope)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("DELETE FROM ").Append("[dbo].[$ndo$tenantdatabaseversion]").AppendLine();
		stringBuilder.AppendLine(NavSqlTenantDatabaseVersion.InsertTenantDatabaseVersionStatement);
		NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.Command.Parameters.Add("@0", SqlDbType.NVarChar, NavSqlTenantDatabaseVersion.FieldLengths[0]);
		navSqlCommand.Command.Parameters.Add("@1", SqlDbType.NVarChar, NavSqlTenantDatabaseVersion.FieldLengths[1]);
		navSqlCommand.Command.Parameters[0].Value = NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion;
		navSqlCommand.Command.Parameters[1].Value = NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily;
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		return navSqlCommand;
	}

	private static NavSqlCommand GetCreateTenantDatabaseVersionTableCommand(NavSqlConnectionScope conScope)
	{
		StringBuilder stringBuilder = new StringBuilder("CREATE TABLE ");
		stringBuilder.Append("[dbo].[$ndo$tenantdatabaseversion]");
		stringBuilder.Append(" (");
		for (int i = 0; i < NavSqlTenantDatabaseVersion.FieldNames.Length; i++)
		{
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, NavSqlTenantDatabaseVersion.FieldNames[i]);
			stringBuilder.Append(' ');
			NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder, NavSqlTenantDatabaseVersion.FieldTypes[i], SqlDataType.Default, NavSqlTenantDatabaseVersion.FieldLengths[i]);
			stringBuilder.Append(" NOT NULL,");
		}
		stringBuilder.Append(" CONSTRAINT ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, "PK$ndo$tenantdatabaseversion");
		stringBuilder.Append(" PRIMARY KEY (");
		stringBuilder.Append("applicationversion");
		stringBuilder.AppendLine("));");
		NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		return navSqlCommand;
	}

	private static NavSqlCommand GetCreateTenantPropertiesTableCommand(NavSqlConnectionScope conScope)
	{
		StringBuilder stringBuilder = new StringBuilder("CREATE TABLE ");
		stringBuilder.Append("[dbo].[$ndo$tenantproperty]");
		stringBuilder.Append(" (");
		for (int i = 0; i < NavSqlTenantProperties.FieldNames.Length; i++)
		{
			switch (i)
			{
			case 4:
				NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, NavSqlTenantProperties.FieldNames[4]);
				stringBuilder.Append(" INT NOT NULL IDENTITY(1,1) ");
				break;
			case 5:
				AppendToCreateTenantPropertiesTableCommand(stringBuilder, i, " NOT NULL DEFAULT N'" + NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion + "'");
				break;
			case 0:
				AppendToCreateTenantPropertiesTableCommand(stringBuilder, i);
				stringBuilder.Append(" COLLATE Latin1_General_100_CI_AS NOT NULL");
				break;
			case 6:
				AppendToCreateTenantPropertiesTableCommand(stringBuilder, i, " NOT NULL DEFAULT " + 0);
				break;
			case 7:
				AppendToCreateTenantPropertiesTableCommand(stringBuilder, i, " NOT NULL DEFAULT getutcdate()");
				break;
			default:
				AppendToCreateTenantPropertiesTableCommand(stringBuilder, i);
				break;
			}
			stringBuilder.Append(',');
		}
		stringBuilder.Append(" CONSTRAINT ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, "PK$ndo$tenantproperty");
		stringBuilder.Append(" PRIMARY KEY (");
		stringBuilder.Append(NavSqlTenantProperties.FieldNames[0]);
		stringBuilder.AppendLine("));");
		stringBuilder.AppendLine(FormattableString.Invariant(FormattableStringFactory.Create("CREATE UNIQUE INDEX IX$TENANTID ON {0} ([{1}]);", "[dbo].[$ndo$tenantproperty]", NavSqlTenantProperties.FieldNames[4])));
		NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		return navSqlCommand;
	}

	private static void AppendToCreateTenantPropertiesTableCommand(StringBuilder commandText, int fieldNo, string appendAfterFieldLength = null)
	{
		NavSqlStatementHelper.AppendQuotedIdentifier(commandText, NavSqlTenantProperties.FieldNames[fieldNo]);
		commandText.Append(' ');
		NavSqlStatementHelper.AppendSqlColumnTypeAndLength(commandText, NavSqlTenantProperties.FieldTypes[fieldNo], SqlDataType.Default, NavSqlTenantProperties.FieldLengths[fieldNo]);
		if (appendAfterFieldLength != null)
		{
			commandText.Append(appendAfterFieldLength);
		}
	}

	private static NavSqlCommand GetUpdateTenantDatabasePropertiesVersionCommand(NavSqlConnectionScope conScope, DatabaseVersion databaseVersion)
	{
		NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		StringBuilder stringBuilder = new StringBuilder("UPDATE ");
		stringBuilder.Append("[dbo].[$ndo$tenantdatabaseproperty]");
		stringBuilder.Append(" SET ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, "versionno");
		stringBuilder.Append(" = ").Append((int)databaseVersion);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		return navSqlCommand;
	}

	internal static List<string> GetRenameDefaultConstraintCommands(NavSqlConnectionScope conScope, NavDatabase database)
	{
		List<string> list = new List<string>();
		string defaultConstrainsQuery = "SELECT dc.name FROM sys.default_constraints dc WHERE dc.name NOT LIKE 'DF__$ndo$%' and dc.name NOT LIKE '$ndodefault$%' and dc.name LIKE '%$%'";
		NavSqlCommand navSqlCommand = conScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("TenantPropertiesSync_RenameDefaultConstraint", database), () => defaultConstrainsQuery, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			return cmd;
		});
		while (navSqlCommand.ReadNextRow())
		{
			string text = navSqlCommand.Reader.GetString(0);
			if (!string.IsNullOrEmpty(text))
			{
				string text2 = NavSqlStatementHelper.GenerateConstraintNameHash(text);
				string item = string.Format(CultureInfo.InvariantCulture, "IF (SELECT OBJECTPROPERTY(OBJECT_ID(N'{0}.\"{1}\"'), N'IsDefaultCnst'))=1 BEGIN EXEC sp_rename N'{0}.\"{1}\"', N'{2}' END", "dbo", text, text2);
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00007A9", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, text, text2);
				list.Add(item);
			}
		}
		return list;
	}
}
