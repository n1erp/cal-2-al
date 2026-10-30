#define SQLSEQUENCE
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.ExecutionTracking.RecordTracking;
using Microsoft.Dynamics.Nav.Runtime.Media;
using Microsoft.Dynamics.Nav.Runtime.Sql.TableLocksLogger;
using Microsoft.Dynamics.Nav.Runtime.TaskScheduling.SystemTasks;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlDatabaseSync : TreeObject
{
	private enum RebuildAction
	{
		PrepareOld,
		PrepareNew,
		RemoveExistingUpgradeTables,
		CleanupOld,
		FinalizeNew
	}

	private const int UnknownBumperToken = -1;

	private readonly string unknownApplicationVersionNumber = string.Empty;

	private const int BufferSize = 4096;

	private readonly NavDatabase tenantDatabase;

	private DatabaseSyncState databaseSyncState;

	private DatabaseTenantDataOperationState databaseTenantDataOperationState;

	private NavTenantDatabaseSyncStatus detailedDatabaseSyncState = new NavTenantDatabaseSyncStatus(0, 0);

	private Dictionary<string, TablePartitionMetadata> tablePartitionsMetadata = new Dictionary<string, TablePartitionMetadata>();

	private int tableSyncMetadataBumperToken;

	private string applicationVersionNumber;

	private NavSqlDatabaseLock crossServerSyncLock;

	private object crossServerSyncLockObject = new object();

	private readonly object cloudReadyTableWhiteListObject = new object();

	private readonly object checkTableInSyncSynchObject = new object();

	private readonly int tableSyncTimeout = (int)ServerUserSettings.Instance.TableSyncTimeout.Value.TotalMilliseconds;

	private readonly object startNewSyncTaskSyncObject = new object();

	private int checkTableInSyncMetadataBumperToken;

	private HashSet<int> pendingTableChanges;

	private Task syncTask;

	private bool isDisposing;

	public DatabaseSyncState DatabaseSyncState
	{
		get
		{
			return databaseSyncState;
		}
		private set
		{
			databaseSyncState = value;
			tenantDatabase.Tenant.RefreshState();
		}
	}

	public NavTenantDatabaseSyncStatus DatabaseSyncStateDetailed => detailedDatabaseSyncState;

	public DatabaseTenantDataOperationState DatabaseTenantDataOperationState
	{
		get
		{
			return databaseTenantDataOperationState;
		}
		private set
		{
			databaseTenantDataOperationState = value;
			tenantDatabase.Tenant.RefreshState();
		}
	}

	public int SynchronizationTaskId
	{
		get
		{
			if (syncTask != null && syncTask.Status == TaskStatus.Running)
			{
				return syncTask.Id;
			}
			return 0;
		}
	}

	public bool IsCurrentMetadataInSync
	{
		get
		{
			if (DatabaseSyncState == DatabaseSyncState.Synchronized && tableSyncMetadataBumperToken == NavGlobal.NCLMetadata.BaseTableMetadataChangeBumperToken)
			{
				return applicationVersionNumber == NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion;
			}
			return false;
		}
	}

	public PendingTableChangesState AnyPendingTableChanges
	{
		get
		{
			using (new MonitorLock(checkTableInSyncSynchObject, "checkTableInSyncSynchObject", tableSyncTimeout))
			{
				if (HasPendingChanges)
				{
					return PendingTableChangesState.PendingChanges;
				}
				if (checkTableInSyncMetadataBumperToken == -1)
				{
					return PendingTableChangesState.Unknown;
				}
				if (!tenantDatabase.IsCurrentVersion)
				{
					return PendingTableChangesState.PendingChanges;
				}
				return PendingTableChangesState.NoChanges;
			}
		}
	}

	private bool HasPendingChanges
	{
		get
		{
			if (pendingTableChanges == null && (checkTableInSyncMetadataBumperToken == -1 || checkTableInSyncMetadataBumperToken == NavGlobal.NCLMetadata.BaseTableMetadataChangeBumperToken) && (!tenantDatabase.TenantDatabaseProperties.IsReady || !(tenantDatabase.TenantDatabaseProperties.Collation != NavGlobal.AppDatabase.SqlSortingProperties.Collation)))
			{
				if (tenantDatabase.TenantDatabaseProperties.IsReady)
				{
					return !tenantDatabase.IsCurrentVersion;
				}
				return false;
			}
			return true;
		}
	}

	public NavSqlDatabaseSync(NavDatabase tenantDatabase)
		: base(tenantDatabase)
	{
		this.tenantDatabase = tenantDatabase;
		checkTableInSyncMetadataBumperToken = -1;
		tableSyncMetadataBumperToken = -1;
		applicationVersionNumber = unknownApplicationVersionNumber;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!disposing || IsDisposed)
			{
				return;
			}
			Tree.DisposeAllChildren();
			lock (startNewSyncTaskSyncObject)
			{
				isDisposing = true;
				if (syncTask != null)
				{
					try
					{
						syncTask.WaitThrowingOriginalException();
						return;
					}
					finally
					{
						syncTask = null;
					}
				}
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	public static void CheckColumnChanges(NavDatabase tenantDatabase, int companyToken, IEnumerable<NavSqlFieldAndChanges> fieldsAndChanges, NavAppTableContext tableContext)
	{
		foreach (NavSqlFieldAndChanges fieldsAndChange in fieldsAndChanges)
		{
			if (fieldsAndChange.DataTypeChanged || fieldsAndChange.Excessive || fieldsAndChange.SqlDataTypeChanged)
			{
				if (fieldsAndChange.SqlDataTypeChanged && fieldsAndChange.NewField.SqlDataTypeWithDefaultMapping == SqlDataType.Variant)
				{
					if (fieldsAndChange.OldField.SqlDataTypeWithDefaultMapping != SqlDataType.VarChar)
					{
						continue;
					}
					string text = NavSqlMetadataCommands.ColumnHasNumericValues(tenantDatabase, companyToken, fieldsAndChange.OldField);
					if (text != null)
					{
						if (fieldsAndChange.NewField.Parent.DataPerCompany && tenantDatabase.TenantDatabaseProperties.PerCompanySchema)
						{
							throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.AttemptToRemoveOrChangeNonEmptyColumnCompanySpecificTable, fieldsAndChange.NewField.FieldName, fieldsAndChange.NewField.FieldNo, fieldsAndChange.NewField.Parent.TableName, tenantDatabase.DatabaseName, tenantDatabase.GetCompanyNameFromToken(companyToken)));
						}
						throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.AttemptToRemoveOrChangeNonEmptyColumn, fieldsAndChange.NewField.FieldName, fieldsAndChange.NewField.FieldNo, fieldsAndChange.NewField.Parent.TableName, tenantDatabase.DatabaseName));
					}
					continue;
				}
				NCLMetaField oldField = fieldsAndChange.OldField;
				if (NavSqlDdlCommands.DataExistsInTableColumn(tenantDatabase, oldField.Parent, oldField, companyToken, tableContext))
				{
					if (oldField.Parent.DataPerCompany && tenantDatabase.TenantDatabaseProperties.PerCompanySchema)
					{
						throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.AttemptToRemoveOrChangeNonEmptyColumnCompanySpecificTable, oldField.FieldName, oldField.FieldNo, oldField.Parent.TableName, tenantDatabase.DatabaseName, tenantDatabase.GetCompanyNameFromToken(companyToken)));
					}
					throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.AttemptToRemoveOrChangeNonEmptyColumn, oldField.FieldName, oldField.FieldNo, oldField.Parent.TableName, tenantDatabase.DatabaseName));
				}
			}
			if (!fieldsAndChange.LengthChanged || fieldsAndChange.NewField.FieldDefinedLength >= fieldsAndChange.OldField.FieldDefinedLength)
			{
				continue;
			}
			NCLMetaField oldField2 = fieldsAndChange.OldField;
			NCLMetaField newField = fieldsAndChange.NewField;
			NCLMetaTable parent = oldField2.Parent;
			StringBuilder stringBuilder = new StringBuilder("SELECT TOP 1 NULL FROM ");
			NavSqlStatementHelper.AppendQuotedTableNameSchemaQualified(stringBuilder, parent, tenantDatabase, companyToken, tableContext);
			stringBuilder.Append(" WHERE LEN(");
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, oldField2.SqlColumnName);
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, ")>{0}", newField.FieldDefinedLength);
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase);
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			if (navSqlCommand.ExecuteScalar(stringBuilder.ToString()) != null)
			{
				throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.AttemptToRemoveOrChangeNonEmptyColumn, oldField2.FieldName, oldField2.FieldNo, oldField2.Parent.TableName, tenantDatabase.DatabaseName));
			}
		}
	}

	public static void CheckColumnChanges(NavDatabase tenantDatabase, int companyToken, NavSqlTableAndChange tableAndChange)
	{
		CheckColumnChanges(tenantDatabase, companyToken, tableAndChange.FieldsAndChanges, tableAndChange.TableContext);
	}

	private void PerformCompanyWideAction(string errorMessageOnPendingSync, string operation, Action<NCLMetaTable, NavAppTableContext> actionToPerform, Action endAction = null)
	{
		using ITraceScope scope = DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00000JI", DiagnosticParameter.SystemMetadata("Company wide action, type = " + operation), (MonitorType)458755, Category.Operations, createNewActivity: false);
		try
		{
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, TransactionType.UpdateNoLocks);
			using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.UpdateNoLocks);
			NavSqlMetadata.LockAppDatabaseForChangesToMetadata(conScope);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000JJ", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Company wide action -- app DB locked");
			NavSqlMetadata.LockTenantDatabaseForChangesToMetadata(navSqlConnectionScope);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000JK", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Company wide action -- tenant DB locked");
			using (new MonitorLock(checkTableInSyncSynchObject, "checkTableInSyncSynchObject", tableSyncTimeout))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000JL", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Company wide action -- getting pending table changes");
				GetPendingTableChanges("PerformCompanyWideAction");
				if (pendingTableChanges != null)
				{
					throw new NavNotSupportedException(errorMessageOnPendingSync);
				}
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000JM", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Company wide action -- getting snapshot");
			TenantDatabaseTableMetadataRetriever tenantDatabaseTableMetadataRetriever = new TenantDatabaseTableMetadataRetriever(tenantDatabase);
			List<Tuple<NCLMetaTable, NavAppTableContext>> source = tenantDatabaseTableMetadataRetriever.GetSnapshotOfAllTenantTablesAndContexts(navSqlConnectionScope).ToList();
			foreach (Tuple<NCLMetaTable, NavAppTableContext> item in source.Where((Tuple<NCLMetaTable, NavAppTableContext> table) => !table.Item1.IsExternalDataSource && table.Item1.DataPerCompany))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000JN", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Company wide action on table -- Table Name:{0}, Table ID:{1}", item.Item1.TableName, item.Item1.TableId);
				actionToPerform(item.Item1, item.Item2);
			}
			endAction?.Invoke();
		}
		catch (Exception exception) when (NavDiagnostics.SetTraceScopeResultExceptionFilter(scope, exception))
		{
			throw;
		}
	}

	public void CreateAllTablesInCompany(int companyToken)
	{
		PerformCompanyWideAction(Lang.AllTablesMustBeSynchronizedBeforeCreatingANewCompany, "CreateAllTablesInCompany", (NCLMetaTable table, NavAppTableContext tableContext) =>
		{
			NavSqlDdlCommands.CreateTableInCompany(tenantDatabase, table, companyToken, tableContext, tenantDatabase.DefaultPartitionMetadata(table));
		});
	}

	public void CreateAllSequencesInCompany(int companyToken)
	{
		NavSqlDdlCommands sequenceCommands = new NavSqlDdlCommands(tenantDatabase);
		PerformCompanyWideAction(Lang.AllTablesMustBeSynchronizedBeforeCreatingANewCompany, "CreateAllSequencesInCompany", (NCLMetaTable table, NavAppTableContext tableContext) =>
		{
			if (tenantDatabase.HasSequencedAutoIncrementField(table, tableContext))
			{
				sequenceCommands.CreateOrAlterSequenceForAutoIncrementField(table, table.AutoIncrementField, tenantDatabase.GetCompanyId(table, companyToken).Value);
			}
		}, () =>
		{
			sequenceCommands.ExecuteNonQuery();
		});
	}

	public void DeleteAllSequencesInCompany(int companyToken)
	{
		NavSqlDdlCommands sequenceCommands = new NavSqlDdlCommands(tenantDatabase);
		PerformCompanyWideAction(Lang.AllTablesMustBeSynchronizedBeforeCreatingANewCompany, "DeleteAllSequencesInCompany", (NCLMetaTable table, NavAppTableContext tableContext) =>
		{
			if (tenantDatabase.HasSequencedAutoIncrementField(table, tableContext))
			{
				sequenceCommands.DropSequenceIfExists(tenantDatabase, table, table.AutoIncrementField, companyToken);
			}
		}, () =>
		{
			sequenceCommands.ExecuteNonQuery();
		});
	}

	public void RenameAllTablesInCompany(int fromCompanyToken, int toCompanyToken)
	{
		PerformCompanyWideAction(Lang.AllTablesMustBeSynchronizedBeforeRenamingACompany, "RenameAllTablesInCompany", (NCLMetaTable table, NavAppTableContext tableContext) =>
		{
			NavSqlDdlCommands.MoveTableToNewCompany(tenantDatabase, table, fromCompanyToken, toCompanyToken, tableContext);
		});
	}

	public void CopyAllTablesInCompany(int fromCompanyToken, int toCompanyToken)
	{
		NavSqlDdlCommands sequenceCommands = new NavSqlDdlCommands(tenantDatabase);
		List<NCLMetaTable> mediaTableList = new List<NCLMetaTable>();
		PerformCompanyWideAction(Lang.AllTablesMustBeSynchronizedBeforeCopyingACompany, "CopyAllTablesInCompany", (NCLMetaTable table, NavAppTableContext tableContext) =>
		{
			NavSqlDdlCommands.CopyTableToNewCompany(tenantDatabase, table, fromCompanyToken, toCompanyToken, tableContext, tenantDatabase.DefaultPartitionMetadata(table));
			NavSqlDdlCommands.SetupTableForIntelligentCloud(tenantDatabase, table, toCompanyToken, tableContext);
			if (tenantDatabase.HasSequencedAutoIncrementField(table, tableContext))
			{
				sequenceCommands.RestartSequenceForAutoIncrementField(table, table.AutoIncrementField, tenantDatabase.GetCompanyId(table, toCompanyToken).Value);
			}
			if (table.MediaFieldCount > 0)
			{
				mediaTableList.Add(table);
			}
		}, () =>
		{
			sequenceCommands.ExecuteNonQuery();
			NavMediaCopyCompany.UpdateMediaReferencesInTables(tenantDatabase, mediaTableList, fromCompanyToken, toCompanyToken);
			NavObjectOptionsCopyCompany.UpdateObjectOptionsAfterCompanyCopy(tenantDatabase, fromCompanyToken, toCompanyToken);
		});
	}

	public void DeleteAllTablesInCompany(int companyToken, bool individuallyCommit)
	{
		NavSqlDdlCommands sequenceCommands = new NavSqlDdlCommands(tenantDatabase);
		PerformCompanyWideAction(Lang.AllTablesMustBeSynchronizedBeforeDeletingACompany, "DeleteAllTablesInCompany", (NCLMetaTable table, NavAppTableContext tableContext) =>
		{
			if (individuallyCommit)
			{
				using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, TransactionType.Update);
				NavSqlDdlCommands.DropTableAndIndexes(tenantDatabase, table, companyToken, tableContext);
				navSqlConnectionScope.TransactionEnd(NavSqlEndTransactionType.Commit);
			}
			else
			{
				NavSqlDdlCommands.DropTableAndIndexes(tenantDatabase, table, companyToken, tableContext);
			}
			if (tenantDatabase.HasSequencedAutoIncrementField(table, tableContext))
			{
				sequenceCommands.DropSequenceIfExists(tenantDatabase, table, table.AutoIncrementField, companyToken);
			}
		}, () =>
		{
			sequenceCommands.ExecuteNonQuery();
		});
	}

	public void DeleteAllCompanyData(int companyToken, bool commitIndividually)
	{
		_ = tenantDatabase.TenantDatabaseProperties.PerCompanySchema;
		PerformCompanyWideAction(Lang.AllTablesMustBeSynchronizedBeforeDeletingACompany, "DeleteAllCompanyData", (NCLMetaTable table, NavAppTableContext tableContext) =>
		{
			if (commitIndividually)
			{
				using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, TransactionType.Update))
				{
					NavSqlDmlCommand.DeleteMultiple(tenantDatabase, table, companyToken, FiltersAndMarks.Empty, tableContext);
					navSqlConnectionScope.TransactionEnd(NavSqlEndTransactionType.Commit);
					return;
				}
			}
			NavSqlDmlCommand.DeleteMultiple(tenantDatabase, table, companyToken, FiltersAndMarks.Empty, tableContext);
		});
	}

	public void ValidateAllTablesSynchronizedBeforeCompanyDelete()
	{
		PerformCompanyWideAction(Lang.AllTablesMustBeSynchronizedBeforeDeletingACompany, "DeleteAllCompanyData", (NCLMetaTable table, NavAppTableContext tableContext) =>
		{
		});
	}

	internal void PerformTenantWideAction(string errorMessageOnPendingSync, Action<NCLMetaTable, NavAppTableContext> actionToPerform, Action<NavDatabase, NavSqlConnectionScope> actionToHandleUninstalledAppRecord = null, Action<NavDatabase, NavSqlConnectionScope, NavSqlConnectionScope> wireUpAction = null, Action endAction = null, Action<string, int, int> displayMessageAction = null, bool UninstalledAppTableIncluded = false)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, TransactionType.UpdateNoLocks);
		using NavSqlConnectionScope navSqlConnectionScope2 = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.UpdateNoLocks);
		NavSqlMetadata.LockAppDatabaseForChangesToMetadata(navSqlConnectionScope2);
		NavSqlMetadata.LockTenantDatabaseForChangesToMetadata(navSqlConnectionScope);
		using (new MonitorLock(checkTableInSyncSynchObject, "checkTableInSyncSynchObject", tableSyncTimeout))
		{
			GetPendingTableChanges("PerformTenantWideAction");
			if (pendingTableChanges != null)
			{
				throw new NavNotSupportedException(errorMessageOnPendingSync);
			}
		}
		TenantDatabaseTableMetadataRetriever tenantDatabaseTableMetadataRetriever = new TenantDatabaseTableMetadataRetriever(tenantDatabase);
		List<Tuple<NCLMetaTable, NavAppTableContext>> source = (from tableAndContext in tenantDatabaseTableMetadataRetriever.GetSnapshotOfAllTenantTablesAndContexts(navSqlConnectionScope)
			where tenantDatabase.HasCompanyIdColumn(tableAndContext.Item1)
			select tableAndContext).ToList();
		List<Tuple<NCLMetaTable, NavAppTableContext>> list = source.Where((Tuple<NCLMetaTable, NavAppTableContext> tableAndContext) => !tableAndContext.Item1.IsExternalDataSource).ToList();
		int num = list.Count;
		if (UninstalledAppTableIncluded)
		{
			num++;
		}
		int num2 = 0;
		foreach (Tuple<NCLMetaTable, NavAppTableContext> item in list)
		{
			num2++;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(item.Item1.TableName);
			if (item.Item1.SourceExtensionType == NavExtensionType.ModernDev)
			{
				stringBuilder.Append('$').Append(item.Item1.SourceAppId);
			}
			displayMessageAction?.Invoke(stringBuilder.ToString(), num2, num);
			actionToPerform(item.Item1, item.Item2);
		}
		if (actionToHandleUninstalledAppRecord != null)
		{
			displayMessageAction?.Invoke("[dbo].[$ndo$navappuninstalledapp]", num, num);
			actionToHandleUninstalledAppRecord?.Invoke(tenantDatabase, navSqlConnectionScope);
		}
		wireUpAction?.Invoke(tenantDatabase, navSqlConnectionScope, navSqlConnectionScope2);
		endAction?.Invoke();
	}

	public int StartTenantDataCopyProcessIfNotAlreadyStarted(NavTenant sourceTenant)
	{
		return StartProcessIfNotAlreadyStarted(() =>
		{
			((ITenantSessionHandler)tenantDatabase.Tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant tenant, NavSession systemSession) =>
			{
				ImportTenantTemplateData(systemSession, sourceTenant);
			}), true, false, false, (LanguageSetting)null);
		});
	}

	public int StartTenantDataMoveProcessIfNotAlreadyStarted(string destinationTenantDatabaseId, bool immediateSourceTenantRemoval)
	{
		return StartProcessIfNotAlreadyStarted(() =>
		{
			MoveTenantData(destinationTenantDatabaseId, immediateSourceTenantRemoval);
		});
	}

	public int StartTenantRemoveProcessIfNotAlreadyStarted()
	{
		return StartProcessIfNotAlreadyStarted(() =>
		{
			((ITenantSessionHandler)tenantDatabase.Tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant tenant, NavSession systemSession) =>
			{
				RemoveTenant(systemSession);
			}), true, false, false, (LanguageSetting)null);
		});
	}

	public void MoveTenantData(string destinationTenantDatabaseId, bool immediateSourceTenantRemoval)
	{
		if (string.IsNullOrEmpty(destinationTenantDatabaseId))
		{
			throw new ArgumentNullException("destinationTenantDatabaseId");
		}
		using (DiagnosticsResolver.CreateTimingScope("00000YP", Category.Tenant, DiagnosticParameter.SystemMetadata("Move tenant data")))
		{
			bool flag = false;
			NavTenant sourceTenant = tenantDatabase.Tenant;
			string tenantId = sourceTenant.Id;
			tenantDatabase.Tenant.Diagnostics.SendTraceTag("00000YQ", Category.Tenant, Verbosity.Verbose, "Move tenant: Started, {0} to database {1}", DiagnosticParameter.Oii(tenantId), DiagnosticParameter.Oii(destinationTenantDatabaseId));
			NavTenant destinationTenant = null;
			try
			{
				ValidateMoveTenantOperation(destinationTenantDatabaseId);
				DatabaseTenantDataOperationState = DatabaseTenantDataOperationState.MoveInProgress;
				NavTenantSettings tenantSettings = PrepareTargetMoveTenant(destinationTenantDatabaseId, sourceTenant, out destinationTenant);
				TenantConfiguration newConfiguration = null;
				FileStream fileStream = GetTemporaryFileStream();
				try
				{
					((ITenantSessionHandler)sourceTenant).RunTenantActionInSystemSessionWithExclusiveAccess((Action<NavTenant, NavSession, ExclusiveTenantActionScope>)((NavTenant tenant, NavSession session, ExclusiveTenantActionScope exclusiveAccessScope) =>
					{
						NavSqlImportExportData.Export(fileStream, "move data for tenant " + destinationTenant.Id, includeApplication: false, includeAppDatabaseTables: false, includeGlobalTables: true, includeAllCompanies: true, null, session, (int progress) =>
						{
							detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Format(CultureInfo.CurrentCulture, Lang.MoveTenantInProgress, tenantId, destinationTenantDatabaseId, Math.Round((double)progress / 2.0));
						}, leaveStreamOpenWhenDone: true);
						fileStream.Flush(flushToDisk: true);
						long size = fileStream.Position;
						fileStream.Position = 0L;
						tenantDatabase.Tenant.Diagnostics.SendTraceTag("00000YR", Category.Tenant, Verbosity.Verbose, "Move tenant: Exported, {0} to database {1}, size {2}", DiagnosticParameter.Oii(tenantId), DiagnosticParameter.Oii(destinationTenantDatabaseId), DiagnosticParameter.SystemMetadata(size));
						using (NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update))
						{
							session.EnsureWriteTransactionStarted();
							exclusiveAccessScope.LockToCurrentTransaction();
							Task task = NavTaskFactory.RunTask(() =>
							{
								NavSqlImportExportData.Import(fileStream, includeApplication: false, includeApplicationDatabaseTables: false, destinationTenant.Database, includeGlobalTables: true, null, (int progress) =>
								{
									detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Format(CultureInfo.CurrentCulture, Lang.MoveTenantInProgress, tenantId, destinationTenantDatabaseId, 50.0 + Math.Round((double)progress / 2.0));
								}, size, leaveStreamOpenWhenDone: false, commitPerTable: true, allCompanies: true);
								tenantDatabase.Tenant.Diagnostics.SendTraceTag("00000YS", Category.Tenant, Verbosity.Verbose, "Move tenant: Imported, {0} to database {1}", DiagnosticParameter.Oii(tenantId), DiagnosticParameter.Oii(destinationTenantDatabaseId));
							});
							task.WaitThrowingOriginalException();
							newConfiguration = NavEnvironment.Instance.Tenants.Configuration.UpdateTenantConfigurationSettings(sourceTenant.Id, tenantSettings);
							exclusiveAccessScope.Detach();
						}
					}));
					flag = true;
					tenantDatabase.Tenant.Diagnostics.SendTraceTag("00000YT", Category.Tenant, Verbosity.Verbose, "Move tenant: tenant {0} moved to database {1}", DiagnosticParameter.Oii(tenantId), DiagnosticParameter.Oii(destinationTenantDatabaseId));
				}
				finally
				{
					if (fileStream != null)
					{
						((IDisposable)fileStream).Dispose();
					}
				}
			}
			catch (Exception ex)
			{
				if (!flag && destinationTenant != null)
				{
					tenantDatabase.Tenant.Diagnostics.SendTraceTag("00000YU", Category.Tenant, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Move tenant: removing destination tenant");
					new NavSqlTenantRemoveAction(destinationTenant.Database).MarkTenantAsSoftDeleted();
					tenantDatabase.Tenant.Diagnostics.SendTraceTag("00000YV", Category.Tenant, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Move tenant: removed destination tenant");
				}
				detailedDatabaseSyncState.DatabaseSyncStateDetailed = ex.Message;
				tenantDatabase.Tenant.Diagnostics.SendExceptionTag("00000YW", Category.Tenant, ex);
				throw;
			}
			finally
			{
				DatabaseTenantDataOperationState = DatabaseTenantDataOperationState.None;
				if (flag)
				{
					RemoveSourceTenantAndRemountMovedTenant(immediateSourceTenantRemoval, sourceTenant, tenantId);
				}
			}
			tenantDatabase.Tenant.Diagnostics.SendTraceTag("00000YZ", Category.Tenant, Verbosity.Verbose, "Move tenant: Completed, {0} to database {1}", DiagnosticParameter.Oii(tenantId), DiagnosticParameter.Oii(destinationTenantDatabaseId));
		}
	}

	private void RemoveSourceTenantAndRemountMovedTenant(bool immediateSourceTenantRemoval, NavTenant sourceTenant, string tenantId)
	{
		try
		{
			if (tenantDatabase.IsSharedTenantSchema)
			{
				tenantDatabase.Tenant.Diagnostics.SendTraceTag("00000YX", Category.Tenant, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Move tenant: Marking source tenant as soft-deleted");
				string text = new NavSqlTenantRemoveAction(tenantDatabase).MarkTenantAsSoftDeleted();
				tenantDatabase.Tenant.Diagnostics.SendTraceTag("00000YY", Category.Tenant, Verbosity.Verbose, "Move tenant: Remounting source tenant {0} with new name: {1}", DiagnosticParameter.Oii(tenantId), DiagnosticParameter.Oii(text));
				new NavTenantManagementTasks().MountTenant(new MountTenantArguments(new NavTenantSettings(sourceTenant.TenantSettings, null)
				{
					Id = text
				}));
				NavTenant tenantById = NavEnvironment.Instance.Tenants.GetTenantById(text);
				tenantDatabase.Tenant.Diagnostics.SendTraceTag("00001K1", Category.Tenant, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Move tenant: Removing tenant data ({0})", immediateSourceTenantRemoval ? "Immediately" : "Using scheduled task");
				((ITenantSessionHandler)tenantById).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant tenant, NavSession session) =>
				{
					if (immediateSourceTenantRemoval)
					{
						tenant.Database.DatabaseSync.RemoveTenant(session);
					}
					else
					{
						NavTaskSchedulerUtility.ScheduleSystemTask(SystemTaskActionId.RemoveTenant, new RemoveTenantSystemTask.RemoveTenantSystemTaskParameters(), DateTime.UtcNow.AddMinutes(1.0));
					}
				}), true, false, false, (LanguageSetting)null);
			}
			detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Empty;
		}
		catch (Exception ex)
		{
			detailedDatabaseSyncState.DatabaseSyncStateDetailed = ex.Message;
			tenantDatabase.Tenant.Diagnostics.SendExceptionTag("00001KK", Category.Tenant, ex);
			throw;
		}
		finally
		{
			NavEnvironment.Instance.Tenants.Configuration.RemountFromLocalConfiguration(tenantId);
		}
	}

	private void ValidateMoveTenantOperation(string destinationTenantDatabaseId)
	{
		tenantDatabase.Tenant.RefreshState();
		if (tenantDatabase.Tenant.State != NavTenantState.Operational)
		{
			throw new NavTenantDatabaseException(string.Format(CultureInfo.InvariantCulture, "Tenant {0} is not in the Operational state. Actual state is {1}.", tenantDatabase.Tenant.Id, tenantDatabase.Tenant.State));
		}
		TenantDatabaseConfigurationStorage tenantDatabaseConfigurationStorage = new TenantDatabaseConfigurationStorage(NavGlobal.AppDatabase);
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase);
		TenantDatabaseConfiguration tenantDatabaseConfiguration = tenantDatabaseConfigurationStorage.GetTenantDatabase(conScope, destinationTenantDatabaseId);
		if (tenantDatabaseConfiguration == null)
		{
			throw new NavTenantDatabaseException(string.Format(CultureInfo.CurrentCulture, Lang.TenantDatabaseEntryNotFound, destinationTenantDatabaseId));
		}
		using (NavExternalTenant navExternalTenant = new NavExternalTenant(tenantDatabaseConfiguration))
		{
			NavTenantDatabaseSettingsDetailedState navTenantDatabaseSettingsDetailedState = TenantDatabaseConfiguration.CalculateTenantDatabaseState(navExternalTenant.Database);
			if (navTenantDatabaseSettingsDetailedState.State != NavTenantDatabaseState.Operational)
			{
				throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000104", Category.Tenant, NavTenantDataMoveException.CreateForNonoperationalDatabase(destinationTenantDatabaseId, navTenantDatabaseSettingsDetailedState));
			}
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000Z0", Category.Tenant, Verbosity.Verbose, "Tenant move: validation info. Destination tenant database Id={0}. Source tenant database version = {1}", DiagnosticParameter.Oii(destinationTenantDatabaseId), DiagnosticParameter.SystemMetadata(tenantDatabase.TenantDatabaseVersion));
	}

	private static NavTenantSettings PrepareTargetMoveTenant(string destinationTenantDatabaseId, NavTenant sourceTenant, out NavTenant destinationTenant)
	{
		NavTenantManagementTasks navTenantManagementTasks = new NavTenantManagementTasks();
		NavTenantSettings navTenantSettings = new NavTenantSettings(sourceTenant.TenantSettings)
		{
			TenantDatabaseId = destinationTenantDatabaseId
		};
		navTenantManagementTasks.NewTenant(new NewTenantArguments(navTenantSettings));
		NavEnvironment.Instance.TenantDatabases.Configuration.FillDatabaseInformation(navTenantSettings);
		NavDatabaseServer orAddDatabaseServerByName = NavEnvironment.Instance.DatabaseServers.GetOrAddDatabaseServerByName(navTenantSettings.DatabaseServer, navTenantSettings.DatabaseUserName, navTenantSettings.ProtectedDatabasePassword);
		destinationTenant = new NavTenant(navTenantSettings, orAddDatabaseServerByName);
		return navTenantSettings;
	}

	private static FileStream GetTemporaryFileStream()
	{
		string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
		return new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.RandomAccess | FileOptions.DeleteOnClose);
	}

	public void ImportTenantTemplateData(NavSession systemSession, NavTenant templateTenant)
	{
		using (DiagnosticsResolver.CreateTimingScope("00000X9", Category.Tenant, DiagnosticParameter.SystemMetadata("Copy tenant data")))
		{
			try
			{
				if (!string.Equals(templateTenant.TenantSettings.DatabaseServer, tenantDatabase.Tenant.TenantSettings.DatabaseServer, StringComparison.OrdinalIgnoreCase) || !string.Equals(templateTenant.TenantSettings.DatabaseName, tenantDatabase.Tenant.TenantSettings.DatabaseName, StringComparison.OrdinalIgnoreCase))
				{
					throw new NavTenantDataCopyException("It is only possible to copy tenant data to a tenant that is in the same tenant database.");
				}
				if (tenantDatabase.GetCompanyTokens().Length != 0)
				{
					throw new NavTenantDataCopyException(string.Format(CultureInfo.InvariantCulture, "Cannot copy the tenant data because the destination tenant {0} in tenant database {1} already contains data.", tenantDatabase.Tenant.Id, tenantDatabase.Tenant.TenantSettings.TenantDatabaseId));
				}
				DatabaseTenantDataOperationState = DatabaseTenantDataOperationState.CopyInProgress;
				int[] companyTokens = templateTenant.Database.GetCompanyTokens();
				Dictionary<int, CompanyId> newCompanyIds = new Dictionary<int, CompanyId>(companyTokens.Length + 1);
				newCompanyIds[0] = tenantDatabase.TenantCompanies.GetCompanyIdForSharedCompany();
				int[] array = companyTokens;
				foreach (int num in array)
				{
					string companyNameFromToken = templateTenant.Database.GetCompanyNameFromToken(num);
					SystemTableTriggers.CleanupDeletedCompanyData(tenantDatabase.Tenant, companyNameFromToken);
					newCompanyIds[num] = tenantDatabase.TenantCompanies.Insert(companyNameFromToken);
				}
				PerformTenantWideAction(Lang.AllTablesMustBeSynchronizedBeforeCopyingATenant, (NCLMetaTable table, NavAppTableContext tableContext) =>
				{
					NavSqlDdlCommands.CopyTenantToNewTenant(tenantDatabase, table, tableContext, templateTenant, newCompanyIds);
				}, (NavDatabase tenantDatabase, NavSqlConnectionScope tenantDatabaseScope) =>
				{
					PurgeAndInsertUninstalledRecordsForTenant(tenantDatabase, tenantDatabaseScope, templateTenant.Id);
				}, WireUpExtenions, () =>
				{
				}, (string tableName, int count, int total) =>
				{
					detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Format(CultureInfo.InvariantCulture, "Copying data for table {0} of {1}.", count, total);
				}, UninstalledAppTableIncluded: true);
				systemSession.FlushAllDataAccessCaches();
				using (new MaximizedPermissionScope(systemSession, new int[1] { 2000000006 }))
				{
					using NavRecord navRecord = new NavRecord(systemSession, 2000000006);
					navRecord.ALFind("-");
					do
					{
						MutableRecordBuffer mutableRecordBuffer = new MutableRecordBuffer(navRecord.RecordImplementation.CopyOfCurrentReadOnlyRecordBuffer());
						mutableRecordBuffer[mutableRecordBuffer.MetaTable.GetFieldIndexByNo(8000)] = new NavGuid(Guid.NewGuid());
						NavSqlDmlCommand.Modify(tenantDatabase, navRecord.MetaTable, 0, mutableRecordBuffer, retrieveTimestamp: true, recordConsistentWithCurrentTransaction: true, out var _);
					}
					while (navRecord.ALNext() > 0);
				}
				tenantDatabase.TenantProperties.ClearAppInsightsIdentifiers();
				foreach (KeyValuePair<string, Guid> appInsightsIdentifier in templateTenant.Database.TenantProperties.AppInsightsIdentifiers)
				{
					tenantDatabase.TenantProperties.AddAppInsightsIdentifier(appInsightsIdentifier.Key, appInsightsIdentifier.Value);
				}
				tenantDatabase.DatabaseState.ClearGlobalCacheAndEnsureNonTransactionalCacheIsNotPromoted();
				DatabaseTenantDataOperationState = DatabaseTenantDataOperationState.None;
				detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Empty;
			}
			catch (Exception ex)
			{
				DatabaseTenantDataOperationState = DatabaseTenantDataOperationState.CopyFailed;
				detailedDatabaseSyncState.DatabaseSyncStateDetailed = ex.Message;
				tenantDatabase.Tenant.Diagnostics.SendExceptionTag("000010Q", Category.Tenant, ex);
				throw;
			}
		}
	}

	public void RemoveTenant(NavSession systemSession)
	{
		using (DiagnosticsResolver.CreateTimingScope("00001K2", Category.Tenant, DiagnosticParameter.SystemMetadata("Remove tenant")))
		{
			using (systemSession.CreateExclusiveTenantActionScope())
			{
				try
				{
					DatabaseTenantDataOperationState = DatabaseTenantDataOperationState.RemoveInProgress;
					using (NavSqlConnectionScope.CreateForceNewConnection(tenantDatabase, updateOrlockingActivity: true))
					{
						tenantDatabase.TenantProperties.SetTenantType(NavTenantType.Deleting);
					}
					DeleteAllCompanies(systemSession);
					DeleteAllGlobalData();
					detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Empty;
					tenantDatabase.TenantProperties.SetTenantType(NavTenantType.Deleted);
					DatabaseTenantDataOperationState = DatabaseTenantDataOperationState.None;
				}
				catch (Exception ex)
				{
					DatabaseTenantDataOperationState = DatabaseTenantDataOperationState.RemoveFailed;
					detailedDatabaseSyncState.DatabaseSyncStateDetailed = ex.Message;
					tenantDatabase.Tenant.Diagnostics.SendExceptionTag("00001K0", Category.Tenant, ex);
					using (NavSqlConnectionScope.CreateForceNewConnection(tenantDatabase, updateOrlockingActivity: true))
					{
						tenantDatabase.TenantProperties.SetTenantType(NavTenantType.SoftDeleted);
					}
					throw;
				}
			}
		}
	}

	private void DeleteAllGlobalData()
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001K3", Category.DatabaseSync, Verbosity.Normal, "Deleting all global data for tenant {0}.", DiagnosticParameter.Oii(tenantDatabase.Tenant.Id));
		PerformTenantWideAction(Lang.AllTablesMustBeSynchronizedBeforeDeletingATenant, (NCLMetaTable table, NavAppTableContext tableContext) =>
		{
			if (table.DataPerCompany)
			{
				return;
			}
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true);
			NavSqlDmlCommand.DeleteMultiple(tenantDatabase, table, 0, FiltersAndMarks.Empty, tableContext);
			navSqlConnectionScope.TransactionEnd(NavSqlEndTransactionType.Commit);
		}, PurgeUninstalledRecordsForTenant, WireUpExtenions, null, (string tableName, int count, int total) =>
		{
			detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Format(CultureInfo.InvariantCulture, "Deleting global data from table {0} of {1}.", count, total);
		}, UninstalledAppTableIncluded: true);
	}

	private void DeleteAllCompanies(NavSession systemSession)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001K4", Category.DatabaseSync, Verbosity.Normal, "Deleting all companies for tenant {0}.", DiagnosticParameter.Oii(tenantDatabase.Tenant.Id));
		if (systemSession.CompanyDeletionMode == NavSession.CompanyDeletionType.Incremental)
		{
			systemSession.CompanyDeletionMode = NavSession.CompanyDeletionType.IncrementalNoSystemTask;
		}
		int num = 0;
		int num2 = tenantDatabase.GetCompanyTokens().Length;
		using (new MaximizedPermissionScope(systemSession, new int[1] { 2000000006 }))
		{
			using NavRecord navRecord = new NavRecord(systemSession, 2000000006);
			navRecord.SuppressEventListeners = true;
			if (navRecord.ALFind(DataError.TrapError, "-"))
			{
				do
				{
					num++;
					detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Format(CultureInfo.InvariantCulture, "Deleting company {0} of {1}.", num, num2);
					navRecord.ALDelete(DataError.ThrowError, runApplicationTrigger: true);
				}
				while (navRecord.ALNext() > 0);
			}
		}
		systemSession.EndTransaction(commit: true);
		systemSession.BeginTransaction();
		List<string> listOfCompanyNamesWithState = tenantDatabase.TenantCompanies.GetListOfCompanyNamesWithState(CompanyState.Deleting);
		num = 0;
		num2 = listOfCompanyNamesWithState.Count;
		foreach (string item in listOfCompanyNamesWithState)
		{
			num++;
			detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Format(CultureInfo.InvariantCulture, "Deleting data from company {0} of {1}.", num, num2);
			DeleteCompanySystemTask.DeleteCompanyData(tenantDatabase.Tenant, item, commitIndividualDeleteOperations: true);
		}
	}

	private HashSet<int> GetChangedTableIds(NavDiagnostics diagnostics)
	{
		Dictionary<int, MetadataHashTuple> tableIdsAndMetadataHashTuplesFromAppDatabase = NavSqlMetadata.GetTableIdsAndMetadataHashTuplesFromAppDatabase();
		Dictionary<int, MetadataHashTuple> tableIdsAndMetadataHashTuplesFromTenantDatabase = NavSqlMetadata.GetTableIdsAndMetadataHashTuplesFromTenantDatabase(tenantDatabase);
		HashSet<int> hashSet = new HashSet<int>();
		foreach (KeyValuePair<int, MetadataHashTuple> item in tableIdsAndMetadataHashTuplesFromAppDatabase)
		{
			NCLMetaTable metaTable;
			if (!tableIdsAndMetadataHashTuplesFromTenantDatabase.TryGetValue(item.Key, out var value))
			{
				if (NavGlobal.NCLMetadata.TryGetMetaTableById(item.Key, out metaTable, 0, requireCompiled: false) && !metaTable.IsExternalDataSource)
				{
					hashSet.Add(item.Key);
					diagnostics.SendTraceTag("00001Z6", Category.DatabaseSync, Verbosity.Normal, "Updating pending table changes -- Table {0} does not exists in tenant", DiagnosticParameter.SystemMetadata(item.Key));
				}
				continue;
			}
			tableIdsAndMetadataHashTuplesFromTenantDatabase.Remove(item.Key);
			if (!item.Value.Equals(value) && (!NavGlobal.NCLMetadata.TryGetMetaTableById(item.Key, out metaTable, 0, requireCompiled: false) || !metaTable.IsExternalDataSource))
			{
				hashSet.Add(item.Key);
				diagnostics.SendTraceTag("00001Z7", Category.DatabaseSync, Verbosity.Normal, "Updating pending table changes -- Table {0} has changed, applicationSchemaHash {1}, tenantSchemaHash {2}", DiagnosticParameter.SystemMetadata(item.Key), DiagnosticParameter.SystemMetadata(item.Value.SchemaHash), DiagnosticParameter.SystemMetadata(value.SchemaHash));
			}
		}
		foreach (int key in tableIdsAndMetadataHashTuplesFromTenantDatabase.Keys)
		{
			NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromTenant(tenantDatabase, key, out var table, databaseConvertedFromNav2013: false);
			if (!table.IsExternalDataSource)
			{
				hashSet.Add(key);
				diagnostics.SendTraceTag("00001Z8", Category.DatabaseSync, Verbosity.Normal, "Updating pending table changes -- Table {0} does not exists in application", DiagnosticParameter.SystemMetadata(key));
			}
		}
		if (hashSet.Count != 0)
		{
			return hashSet;
		}
		return null;
	}

	internal ReadOnlyCollection<NavSqlTableAndChange> GetTableChanges(bool sortingPropertiesChanged, bool databaseConvertedFromNav2013)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Getting table changes for tenant '{0}'", tenantDatabase.Tenant.Id);
		using (DiagnosticsResolver.CreateTimingScope("00000NW", Category.DatabaseSync, DiagnosticParameter.Oii(content)))
		{
			if (tenantDatabase.HasCapability(SupportedCapability.TablePartition))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001CN", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Setting up partitions if needed");
				tablePartitionsMetadata = NavSqlMetadataCommands.GetTablesPartitionMetadata(tenantDatabase);
				if (!tablePartitionsMetadata.Any((KeyValuePair<string, TablePartitionMetadata> t) => t.Value.PartitionSchemaName == "CompanyIdPartitionScheme") && tenantDatabase.IsSharedTenantSchema && ServerUserSettings.Instance.SqlPartitionsCount.Value > 1)
				{
					NavSqlDdlCommands.EnsurePartitionFunctionAndSchemeCreated(tenantDatabase, ServerUserSettings.Instance.SqlPartitionsCount.Value, ServerUserSettings.Instance.SqlPartitionBucketSize.Value);
				}
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000JO", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Getting app DB hashes");
			Dictionary<int, MetadataHashTuple> tableIdsAndMetadataHashTuplesFromAppDatabase = NavSqlMetadata.GetTableIdsAndMetadataHashTuplesFromAppDatabase();
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000JP", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Getting tenant DB hashes");
			Dictionary<int, MetadataHashTuple> tableIdsAndMetadataHashTuplesFromTenantDatabase = NavSqlMetadata.GetTableIdsAndMetadataHashTuplesFromTenantDatabase(tenantDatabase);
			List<NavSqlTableAndChange> list = new List<NavSqlTableAndChange>();
			foreach (KeyValuePair<int, MetadataHashTuple> item in tableIdsAndMetadataHashTuplesFromAppDatabase)
			{
				bool flag = tableIdsAndMetadataHashTuplesFromTenantDatabase.TryGetValue(item.Key, out var value);
				NCLMetaTable table = null;
				NCLMetaTable table2 = null;
				NavBLOB rawMetadata = null;
				NavBLOB rawMetadata2 = null;
				NavText newHash = null;
				int schemaHash = 0;
				if (!flag | sortingPropertiesChanged | databaseConvertedFromNav2013)
				{
					NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromAppDatabase(item.Key, out table2, out rawMetadata2, out newHash, out schemaHash);
					if (table2.IsExternalDataSource)
					{
						continue;
					}
				}
				if (!flag)
				{
					list.Add(new NavSqlTableAndChange(null, TableChangeType.Missing, table2, item.Value.MetadataHash, newHash, item.Value.SchemaHash, sortingPropertiesChanged, databaseConvertedFromNav2013, rawMetadata2, null, rawMetadata2));
					continue;
				}
				tableIdsAndMetadataHashTuplesFromTenantDatabase.Remove(item.Key);
				bool flag2 = false;
				bool flag3 = item.Value.Equals(value);
				if (flag3)
				{
					if (sortingPropertiesChanged)
					{
						flag2 = table2.NormalFields.Any((NCLMetaField field) => field.IsStringType);
					}
					if (!flag2 & databaseConvertedFromNav2013)
					{
						NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromTenant(tenantDatabase, item.Key, out table, databaseConvertedFromNav2013);
						flag2 = table2.Keys.Any((NCLMetaKey index) => !index.MaintainSqlIndex || !index.KeyIsActive);
					}
					bool flag4 = tenantDatabase.IsSharedTenantSchema && ServerUserSettings.Instance.SqlPartitionsCount.Value > 1;
					if (!flag2 && !flag4)
					{
						continue;
					}
				}
				if (table2 == null)
				{
					NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromAppDatabase(item.Key, out table2, out rawMetadata2, out newHash, out schemaHash);
					if (table2.IsExternalDataSource)
					{
						continue;
					}
				}
				TablePartitionMetadata value2 = null;
				bool flag5 = true;
				if (table2 != null && !tablePartitionsMetadata.TryGetValue(table2.SqlTableName, out value2))
				{
					flag5 = false;
					if (tenantDatabase.IsSharedTenantSchema && ServerUserSettings.Instance.SqlPartitionsCount.Value > 1)
					{
						value2 = TablePartitionMetadata.CompanyIdPartitionMetadata;
					}
				}
				if (table == null)
				{
					NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromTenant(tenantDatabase, item.Key, out table, databaseConvertedFromNav2013, out rawMetadata);
				}
				if (flag3)
				{
					bool flag6 = !flag5 && value2 != null && tenantDatabase.HasCompanyIdColumn(table2);
					if (!flag2 && !flag6)
					{
						continue;
					}
				}
				list.Add(new NavSqlTableAndChange(table, TableChangeType.Changed, table2, newHash, newHash, item.Value.SchemaHash, sortingPropertiesChanged, databaseConvertedFromNav2013, rawMetadata2, rawMetadata, rawMetadata2, isUpgradeChange: false, value2, flag5));
			}
			foreach (KeyValuePair<int, MetadataHashTuple> item2 in tableIdsAndMetadataHashTuplesFromTenantDatabase)
			{
				NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromTenant(tenantDatabase, item2.Key, out var table3, databaseConvertedFromNav2013, out var rawMetadata3);
				if (!table3.IsExternalDataSource)
				{
					list.Add(new NavSqlTableAndChange(table3, TableChangeType.Excessive, null, item2.Value.MetadataHash, null, item2.Value.SchemaHash, sortingPropertiesChanged, databaseConvertedFromNav2013, null, rawMetadata3));
				}
			}
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000NX", Category.DatabaseSync, Verbosity.Normal, "Found {0} table changes for tenant '{1}'", DiagnosticParameter.SystemMetadata(list.Count), DiagnosticParameter.Oii(tenantDatabase.Tenant.Id));
			return new ReadOnlyCollection<NavSqlTableAndChange>(list);
		}
	}

	private void SyncCloudReadyReplicationStatus(NavSqlConnectionScope tenantDatabaseScope)
	{
		lock (cloudReadyTableWhiteListObject)
		{
			TenantDatabaseTableMetadataRetriever tenantDatabaseTableMetadataRetriever = new TenantDatabaseTableMetadataRetriever(tenantDatabase);
			IEnumerable<(NCLMetaTable, NavAppTableContext, TableChangeType)> tablesContextsAndChangeTypes = from t in tenantDatabaseTableMetadataRetriever.GetSnapshotOfAllTenantTablesAndContexts(tenantDatabaseScope)
				where !t.Item1.IsExternalDataSource && !t.Item1.IsLinked
				select (t.Item1, t.Item2, None: TableChangeType.None);
			UpdateCloudReadyStatus(tenantDatabaseScope, tablesContextsAndChangeTypes, setChangeTracking: false);
		}
	}

	private void SyncCloudReadyReplicationStatus(NavSqlConnectionScope tenantDatabaseScope, IEnumerable<SyncChangeResult> tableChangesToSynchronize)
	{
		if (tableChangesToSynchronize == null || !tableChangesToSynchronize.Any())
		{
			return;
		}
		lock (cloudReadyTableWhiteListObject)
		{
			IEnumerable<(NCLMetaTable, NavAppTableContext, TableChangeType)> enumerable = tableChangesToSynchronize.Select((SyncChangeResult t) => ((t.TableAndChange.ChangeType == TableChangeType.Missing) ? t.TableAndChange.NewTable : t.TableAndChange.OldTable, TableContext: t.TableAndChange.TableContext, ChangeType: t.TableAndChange.ChangeType));
			bool setChangeTracking = enumerable.Any() && tenantDatabase.Tenant.IsIntelligentCloudReplicationEnabled;
			UpdateCloudReadyStatus(tenantDatabaseScope, enumerable, setChangeTracking);
		}
	}

	private void UpdateCloudReadyStatus(NavSqlConnectionScope conScope, IEnumerable<(NCLMetaTable table, NavAppTableContext context, TableChangeType changeType)> tablesContextsAndChangeTypes, bool setChangeTracking)
	{
		string content = FormattableString.Invariant($"Setting up cloud ready for intelligent cloud for tenant '{tenantDatabase.Tenant.Id}'");
		using (DiagnosticsResolver.CreateTimingScope("00000NW", Category.DatabaseSync, DiagnosticParameter.Oii(content)))
		{
			HashSet<(string CompanyName, string TableName)> intelligentCloudStatus = new HashSet<(string, string)>(from c in NavSqlCloudReadyReplicationStatus.GetCloudReadyReplicationStatusCollection(conScope)
				select (CompanyName: c.CompanyName, TableName: c.TableName));
			Dictionary<int, string> companyTokensAndNames = GetCompanyTokensAndNames();
			foreach (var tablesContextsAndChangeType in tablesContextsAndChangeTypes)
			{
				var (table, context, changeType) = tablesContextsAndChangeType;
				companyTokensAndNames.Where((KeyValuePair<int, string> c) => table.DataPerCompany == (c.Key != 0)).ForEach((KeyValuePair<int, string> c) =>
				{
					UpdateCloudReadyStatusForTable(conScope, table, context, changeType, c, intelligentCloudStatus, setChangeTracking);
				});
			}
		}
	}

	private void UpdateCloudReadyStatusForTable(NavSqlConnectionScope conScope, NCLMetaTable table, NavAppTableContext context, TableChangeType changeType, KeyValuePair<int, string> company, HashSet<(string CompanyName, string TableName)> intelligentCloudStatus, bool setChangeTracking)
	{
		string text = NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder(), table, tenantDatabase, company.Key, context).ToString();
		bool flag = intelligentCloudStatus.Contains((company.Value, text));
		if (flag == table.ReplicateData && (!flag || changeType != TableChangeType.Excessive))
		{
			return;
		}
		CloudReadyReplicationStatus cloudReadyReplicationStatus = new CloudReadyReplicationStatus
		{
			TableName = text,
			TableId = table.TableId,
			CompanyName = (company.Value ?? string.Empty),
			CompanyId = company.Key
		};
		if (table.ReplicateData && changeType != TableChangeType.Excessive)
		{
			NavSqlCloudReadyReplicationStatus.Insert(conScope, cloudReadyReplicationStatus);
			if (setChangeTracking)
			{
				NavSqlDdlCommands.EnableChangeTrackingForTable(cloudReadyReplicationStatus, conScope);
			}
		}
		else
		{
			NavSqlCloudReadyReplicationStatus.Delete(conScope, cloudReadyReplicationStatus.CompanyName, cloudReadyReplicationStatus.TableName);
			if (setChangeTracking)
			{
				NavSqlDdlCommands.DisableChangeTrackingForTable(cloudReadyReplicationStatus, conScope);
			}
		}
	}

	private Dictionary<int, string> GetCompanyTokensAndNames()
	{
		Dictionary<int, string> dictionary = new Dictionary<int, string>();
		if (tenantDatabase.TenantProperties.TenantLocalId.HasValue)
		{
			dictionary.Add(0, string.Empty);
			int[] companyTokens = tenantDatabase.GetCompanyTokens();
			foreach (int num in companyTokens)
			{
				string companyNameFromToken = tenantDatabase.GetCompanyNameFromToken(num);
				dictionary.Add(num, companyNameFromToken);
			}
		}
		return dictionary;
	}

	internal ReadOnlyCollection<NavSqlTableAndChange> GetTableChangesForUninstallAllAppsAction()
	{
		Dictionary<int, NavText> baseTableIdsAndMetadataHashesFromTenantDatabase = NavSqlMetadata.GetBaseTableIdsAndMetadataHashesFromTenantDatabase(tenantDatabase);
		Dictionary<int, NavText> tableIdsAndMetadataHashesFromTenantDatabase = NavSqlMetadata.GetTableIdsAndMetadataHashesFromTenantDatabase(tenantDatabase);
		List<NavSqlTableAndChange> list = new List<NavSqlTableAndChange>();
		foreach (KeyValuePair<int, NavText> item in baseTableIdsAndMetadataHashesFromTenantDatabase)
		{
			int key = item.Key;
			NavText value = item.Value;
			tableIdsAndMetadataHashesFromTenantDatabase.TryGetValue(item.Key, out var value2);
			tableIdsAndMetadataHashesFromTenantDatabase.Remove(key);
			if (!string.Equals(value, value2, StringComparison.Ordinal))
			{
				NavSqlObjectMetadataSnapshotHandler.GetBaseTableMetadataFromTenant(tenantDatabase, key, out var table, databaseConvertedFromNav2013: false, out var rawMetadata);
				if (!table.IsExternalDataSource)
				{
					NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromTenant(tenantDatabase, key, out var table2, databaseConvertedFromNav2013: false, out var rawMetadata2);
					list.Add(new NavSqlTableAndChange(table2, TableChangeType.Changed, table, value, value, 0, changedSortingProperties: false, databaseConvertedFromNav2013: false, rawMetadata, rawMetadata2, rawMetadata));
				}
			}
		}
		foreach (KeyValuePair<int, NavText> item2 in tableIdsAndMetadataHashesFromTenantDatabase)
		{
			int key2 = item2.Key;
			NavText value3 = item2.Value;
			NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromTenant(tenantDatabase, key2, out var table3, databaseConvertedFromNav2013: false, out var rawMetadata3);
			if (!table3.IsExternalDataSource)
			{
				list.Add(new NavSqlTableAndChange(table3, TableChangeType.Excessive, null, value3, null, 0, changedSortingProperties: false, databaseConvertedFromNav2013: false, null, rawMetadata3));
			}
		}
		return new ReadOnlyCollection<NavSqlTableAndChange>(list);
	}

	private static void AlterCollationOnSpecialTenantTables(NavSqlConnectionScope conScope, NavDatabase tenantDatabase)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000001071);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(10);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("ALTER TABLE ");
		NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, metaTableById, tenantDatabase, 0, NavAppTableContext.Main);
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " ALTER COLUMN [{0}] ", fieldByNo.SqlColumnName);
		NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder, fieldByNo.FieldNclType, fieldByNo.SqlDataType, fieldByNo.FieldDefinedLength, NavGlobal.AppDatabase.SqlSortingProperties.Collation);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.ExecuteNonQuery(stringBuilder.ToString());
	}

	private static void AlterObjectMetadataSnapshotTable(NavSqlConnectionScope tenantDatabaseScope, NavDatabase tenantDatabase, NavSqlTenantPropertiesSync properties)
	{
		if (properties.AlterObjectMetadataSnapshot)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000NB", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Altering snapshot table -- adding [Table Blob Version] field");
			AlterObjectMetadataSnapshotToContainTableBlobVersionField(tenantDatabaseScope, tenantDatabase);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000K2", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Altering snapshot table -- base additions");
			AlterObjectMetadataSnapshotToContainTwoSnapshotsIfNecessary(tenantDatabaseScope, tenantDatabase);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000NC", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Altering snapshot table -- lengthening hash field");
			AlterObjectMetadataSnapshotToHaveLongerHashField(tenantDatabaseScope, tenantDatabase);
		}
		if (properties.AddSchemaHashToObjectMetadata)
		{
			AddSchemHashToObjectMetadataSnapshotTable(tenantDatabaseScope);
		}
	}

	private static void AddSchemHashToObjectMetadataSnapshotTable(NavSqlConnectionScope tenantDatabaseScope)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag(string.Empty, Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Altering snapshot table --adding schema hash");
		AlterObjectMetadataSnapshotToContainSchemaHashField(tenantDatabaseScope);
		AddSchemaHashToObjectMetadataSnapshot(tenantDatabaseScope);
	}

	private static void AddSchemaHashToObjectMetadataSnapshot(NavSqlConnectionScope connectionScope)
	{
		Dictionary<int, MetadataHashTuple> tableIdsAndMetadataHashTuplesFromAppDatabase = NavSqlMetadata.GetTableIdsAndMetadataHashTuplesFromAppDatabase();
		Dictionary<int, MetadataHashTuple> tableIdsAndMetadataHashTuplesFromTenantDatabase = NavSqlMetadata.GetTableIdsAndMetadataHashTuplesFromTenantDatabase(connectionScope.Connection.Database);
		foreach (KeyValuePair<int, MetadataHashTuple> item in tableIdsAndMetadataHashTuplesFromAppDatabase)
		{
			int key = item.Key;
			if (tableIdsAndMetadataHashTuplesFromTenantDatabase.TryGetValue(key, out var value) && string.Equals(item.Value.MetadataHash, value.MetadataHash, StringComparison.Ordinal))
			{
				UpdateSchemaHashInObjectMetadataSnapshot(connectionScope, key, item.Value.SchemaHash);
			}
		}
	}

	private static void UpdateSchemaHashInObjectMetadataSnapshot(NavSqlConnectionScope conScope, int tableId, int schemaHash)
	{
		CompanyId? companyIdForSystemTables = conScope.Connection.Database.GetCompanyIdForSystemTables(2000000184);
		string value = string.Empty;
		if (companyIdForSystemTables.HasValue)
		{
			value = "\"$companyId\" = " + companyIdForSystemTables.Value.ToString() + " AND ";
		}
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		StringBuilder stringBuilder = new StringBuilder("UPDATE ");
		stringBuilder.Append("[dbo].[Object Metadata Snapshot]");
		stringBuilder.Append(" SET ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, "Schema Hash");
		stringBuilder.Append(" = ").Append(schemaHash);
		stringBuilder.Append(" WHERE ").Append(value);
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, "Object Type");
		stringBuilder.Append(" = ").Append(1);
		stringBuilder.Append(" AND ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, "Object ID");
		stringBuilder.Append(" = ").Append(tableId);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		navSqlCommand.ExecuteNonQuery();
	}

	private static void AlterObjectMetadataSnapshotToContainSchemaHashField(NavSqlConnectionScope conScope)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000001071);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(40);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'{0}' AND OBJECT_ID = OBJECT_ID(N'{1}')) BEGIN ALTER TABLE ", fieldByNo.SqlColumnName, metaTableById.SqlTableName);
		NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, metaTableById, conScope.Connection.Database, 0, NavAppTableContext.Main);
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " ADD [{0}] ", fieldByNo.SqlColumnName);
		NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder, fieldByNo.FieldNclType, fieldByNo.SqlDataType);
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " DEFAULT (0) NOT NULL");
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " END");
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.ExecuteNonQuery(stringBuilder.ToString());
	}

	private static void AlterObjectMetadataSnapshotToContainTwoSnapshotsIfNecessary(NavSqlConnectionScope conScope, NavDatabase tenantDatabase)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000001071);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(33);
		NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(36);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'{0}' AND OBJECT_ID = OBJECT_ID(N'{1}')) BEGIN ALTER TABLE ", fieldByNo.SqlColumnName, metaTableById.SqlTableName);
		NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, metaTableById, tenantDatabase, 0, NavAppTableContext.Main);
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " ADD [{0}] ", fieldByNo.SqlColumnName);
		NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder, fieldByNo.FieldNclType, fieldByNo.SqlDataType);
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, ", [{0}] ", fieldByNo2.SqlColumnName);
		NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder, fieldByNo2.FieldNclType, fieldByNo2.SqlDataType, fieldByNo2.FieldDefinedLength);
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " END");
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.ExecuteNonQuery(stringBuilder.ToString());
	}

	private static void AlterObjectMetadataSnapshotToContainTableBlobVersionField(NavSqlConnectionScope conScope, NavDatabase tenantDatabase)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000001071);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(39);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'{0}' AND OBJECT_ID = OBJECT_ID(N'{1}')) BEGIN ALTER TABLE ", fieldByNo.SqlColumnName, metaTableById.SqlTableName);
		NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, metaTableById, tenantDatabase, 0, NavAppTableContext.Main);
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " ADD [{0}] ", fieldByNo.SqlColumnName);
		NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder, fieldByNo.FieldNclType, fieldByNo.SqlDataType);
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " DEFAULT (1) NOT NULL");
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " END");
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.ExecuteNonQuery(stringBuilder.ToString());
	}

	private static void AlterObjectMetadataSnapshotToHaveLongerHashField(NavSqlConnectionScope conScope, NavDatabase tenantDatabase)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000001071);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(30);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "ALTER TABLE ");
		NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, metaTableById, tenantDatabase, 0, NavAppTableContext.Main);
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " ALTER COLUMN [{0}] ", fieldByNo.SqlColumnName);
		NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder, fieldByNo.FieldNclType, fieldByNo.SqlDataType, fieldByNo.FieldDefinedLength);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.ExecuteNonQuery(stringBuilder.ToString());
	}

	private void SynchronizeNewDatabase(bool isSharedSchema)
	{
		if (isSharedSchema)
		{
			tenantDatabase.TenantDatabaseProperties.StorePerCompanySchemaSetting(newPerCompanySchema: false);
		}
		tenantDatabase.SqlSortingProperties = NavGlobal.AppDatabase.SqlSortingProperties;
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000001071, -1, requireCompiled: false);
		NavSqlDdlCommands.CreateTableAndIndexes(tenantDatabase, metaTableById, 0, NavAppTableContext.Main, tenantDatabase.DefaultPartitionMetadata(metaTableById));
		CreateTablesFromMetadata(synchronizingEmptyDatabase: true);
	}

	private void CreateTablesFromMetadata(bool synchronizingEmptyDatabase)
	{
		Dictionary<int, MetadataHashTuple> tableIdsAndMetadataHashTuplesFromAppDatabase = NavSqlMetadata.GetTableIdsAndMetadataHashTuplesFromAppDatabase();
		NCLMetaTable table = null;
		try
		{
			foreach (KeyValuePair<int, MetadataHashTuple> item in tableIdsAndMetadataHashTuplesFromAppDatabase.Where((KeyValuePair<int, MetadataHashTuple> pair) => pair.Key >= 2000000001).Union(tableIdsAndMetadataHashTuplesFromAppDatabase.Where((KeyValuePair<int, MetadataHashTuple> pair) => pair.Key < 2000000001)))
			{
				NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromAppDatabase(item.Key, out table, out var rawMetadata, out var _, out var _);
				if (table.IsExternalDataSource)
				{
					continue;
				}
				bool num;
				if (!synchronizingEmptyDatabase)
				{
					num = table.DataPerCompany;
				}
				else
				{
					if (!tenantDatabase.TenantDatabaseProperties.PerCompanySchema)
					{
						goto IL_00b4;
					}
					num = !table.DataPerCompany;
				}
				if (num)
				{
					goto IL_00b4;
				}
				goto IL_00d2;
				IL_00d2:
				NavSqlObjectMetadataSnapshotHandler.SaveNewTableMetadataToTenantSingleTable(tenantDatabase, table, item.Value, rawMetadata);
				continue;
				IL_00b4:
				NavSqlDdlCommands.CreateTableAndIndexes(tenantDatabase, table, 0, NavAppTableContext.Main, tenantDatabase.DefaultPartitionMetadata(table));
				goto IL_00d2;
			}
		}
		catch (NavSqlException ex) when (((Func<bool>)delegate
		{
			// Could not convert BlockContainer to single expression
			SqlException obj = ex.InnerException as SqlException;
			return obj != null && obj.Number == 2714;
		}).Invoke())
		{
			throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.TablesAlreadyExistsInNewTenantDatabase, tenantDatabase.DatabaseName, table?.TableName));
		}
	}

	internal void CreateSequencesForTenantSystemAndNotPerCompanyTables()
	{
		IEnumerable<NCLMetaTable> enumerable = from t in NavGlobal.NCLMetadata.GetSnapshotOfAllNonVirtualMetaTables()
			where (t.IsNonVirtualSystemTable || !t.DataPerCompany) && tenantDatabase.HasSequencedAutoIncrementField(t, NavAppTableContext.Main)
			select t;
		NavSqlDdlCommands navSqlDdlCommands = new NavSqlDdlCommands(tenantDatabase);
		foreach (NCLMetaTable item in enumerable)
		{
			CompanyId companyId = (item.IsNonVirtualSystemTable ? tenantDatabase.GetCompanyIdForSystemTables(item.TableId).Value : tenantDatabase.GetCompanyId(item, 0).Value);
			string name = NavSqlStatementHelper.SequenceNameForAutoIncrementField(item, item.AutoIncrementField, companyId);
			navSqlDdlCommands.CreateSequenceIfNotExists(name, item.AutoIncrementField.SqlDataType, 1L);
		}
		navSqlDdlCommands.ExecuteNonQuery();
	}

	private void UpdateFromNav2013(NavSqlConnectionScope conScope)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000001071);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(6);
		NCLMetaField fieldByNo3 = metaTableById.GetFieldByNo(9);
		NCLMetaField fieldByNo4 = metaTableById.GetFieldByNo(10);
		NCLMetaField fieldByNo5 = metaTableById.GetFieldByNo(11);
		NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		StringBuilder stringBuilder = new StringBuilder("SELECT ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, fieldByNo2.SqlColumnName);
		stringBuilder.Append(',');
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, fieldByNo3.SqlColumnName);
		stringBuilder.Append(" FROM ");
		NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder, metaTableById, tenantDatabase, 0, NavAppTableContext.Main);
		stringBuilder.Append(" WHERE ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, fieldByNo.SqlColumnName);
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " = {0}", 1);
		NavSqlCommand navSqlCommand2 = NavSqlCommand.Create(conScope.Connection);
		StringBuilder stringBuilder2 = new StringBuilder("UPDATE ");
		NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(stringBuilder2, metaTableById, tenantDatabase, 0, NavAppTableContext.Main);
		stringBuilder2.Append(" SET ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder2, fieldByNo4.SqlColumnName);
		SqlParameter sqlParameter = navSqlCommand2.Command.Parameters.Add(NavSqlStatementHelper.CreateSqlParameter("@0", fieldByNo4.FieldNclType, fieldByNo4.SqlDataType, fieldByNo4.FieldDefinedLength, string.Empty, isWildcardValue: false, isFilterValue: false));
		stringBuilder2.AppendFormat(CultureInfo.InvariantCulture, "={0},", sqlParameter.ParameterName);
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder2, fieldByNo5.SqlColumnName);
		sqlParameter = navSqlCommand2.Command.Parameters.Add(NavSqlStatementHelper.CreateSqlParameter("@1", fieldByNo5.FieldNclType, fieldByNo5.SqlDataType, fieldByNo5.FieldDefinedLength, false, isWildcardValue: false, isFilterValue: false));
		stringBuilder2.AppendFormat(CultureInfo.InvariantCulture, "={0}", sqlParameter.ParameterName);
		stringBuilder2.Append(" WHERE ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder2, fieldByNo.SqlColumnName);
		sqlParameter = navSqlCommand2.Command.Parameters.Add(NavSqlStatementHelper.CreateSqlParameter("@2", fieldByNo.FieldNclType, fieldByNo.SqlDataType, fieldByNo.FieldDefinedLength, 1, isWildcardValue: false, isFilterValue: false));
		stringBuilder2.AppendFormat(CultureInfo.InvariantCulture, "={0}", sqlParameter.ParameterName);
		stringBuilder2.Append(" AND ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder2, fieldByNo2.SqlColumnName);
		sqlParameter = navSqlCommand2.Command.Parameters.Add(NavSqlStatementHelper.CreateSqlParameter("@3", fieldByNo.FieldNclType, fieldByNo2.SqlDataType, fieldByNo2.FieldDefinedLength, 0, isWildcardValue: false, isFilterValue: false));
		stringBuilder2.AppendFormat(CultureInfo.InvariantCulture, "={0} ", sqlParameter.ParameterName);
		navSqlCommand2.SetCommandText(stringBuilder2.ToString());
		navSqlCommand2.PrepareStatement();
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		navSqlCommand.PrepareStatement();
		using (navSqlCommand.ExecuteReader())
		{
			while (navSqlCommand.ReadNextRow())
			{
				NavBLOB navBLOB = new NavBLOB();
				XmlDocument document = XmlHelper.SafeXmlDocument();
				try
				{
					navSqlCommand.GetBlobDataFromReader(fieldByNo3.FieldIsCompressed, 1, navBLOB);
					document.Load(XmlHelper.SafeXmlReader(navBLOB.GetNewStream()));
				}
				catch (XmlException)
				{
					try
					{
						XmlMetadataCleaner.CleanForInvalidCharacters(navBLOB.GetNewStream(), ref document);
					}
					catch (XmlException)
					{
						string message = Encoding.Default.GetString(navBLOB.GetBytes());
						LogWriterHelper.LogEvent(EventLogEntryType.Error, message);
						throw;
					}
				}
				MetaTable metaTableFromMetadata = NavSqlObjectMetadataSnapshotHandler.GetMetaTableFromMetadata(document.DocumentElement, 1);
				navSqlCommand2.Command.Parameters[0].Value = metaTableFromMetadata.Name;
				navSqlCommand2.Command.Parameters[1].Value = (metaTableFromMetadata.IsDataPerCompany ? 1 : 0);
				navSqlCommand2.Command.Parameters[3].Value = metaTableFromMetadata.Id;
				navSqlCommand2.ExecuteNonQuery();
			}
		}
	}

	private NavSqlTableAndChange ResolveDesiredSqlPartition(NavSqlTableAndChange tableAndChange, int companyToken)
	{
		if (tableAndChange.OldTable == null)
		{
			return tableAndChange;
		}
		if (companyToken == 0 && tableAndChange.OldTable.DataPerCompany)
		{
			return tableAndChange;
		}
		string key = NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder(), tableAndChange.OldTable, tenantDatabase, companyToken, tableAndChange.TableContext).ToString();
		bool flag = tablePartitionsMetadata.TryGetValue(key, out var value);
		if (!flag && tenantDatabase.IsSharedTenantSchema && ServerUserSettings.Instance.SqlPartitionsCount.Value > 1)
		{
			value = TablePartitionMetadata.CompanyIdPartitionMetadata;
		}
		if (value != null)
		{
			tableAndChange = tableAndChange.CloneWithNewPartition(value, flag);
		}
		return tableAndChange;
	}

	private void SyncAllTables(IEnumerable<NavSqlTableAndChange> tableChanges, RebuildAction rebuildAction, int[] companyTokens, SyncMode syncMode, int syncStepNo, int totalSteps, bool perTable = false)
	{
		if (!tableChanges.Any() || (tenantDatabase.TenantDatabaseProperties.PerCompanySchema && companyTokens.Length == 0))
		{
			return;
		}
		if (!tenantDatabase.TenantDatabaseProperties.PerCompanySchema)
		{
			companyTokens = new int[1] { -2147483648 };
		}
		int num = tableChanges.Count() * companyTokens.Length;
		int num2 = 0;
		try
		{
			int[] array = companyTokens;
			foreach (int num3 in array)
			{
				string content = (tenantDatabase.TenantDatabaseProperties.PerCompanySchema ? string.Format(CultureInfo.InvariantCulture, "Sync operation {0} on company {1} ({2})", rebuildAction, num3, tenantDatabase.GetCompanyNameFromToken(num3)) : string.Format(CultureInfo.InvariantCulture, "Sync operation {0}", rebuildAction));
				using (DiagnosticsResolver.GetMostSpecificInstance().CreateTimingScope("00000RC", Category.DatabaseSync, DiagnosticParameter.Oii(content)))
				{
					foreach (NavSqlTableAndChange tableChange in tableChanges)
					{
						SyncMode syncMode2 = ((syncMode == SyncMode.CheckOnly || !ServerUserSettings.Instance.ForceSyncFromUpgradeCodeunit.Value || tableChange.DataUpgradeSetup != DataUpgradeSetup.ForceSync) ? syncMode : SyncMode.ForceSync);
						NavSqlTableAndChange navSqlTableAndChange = tableChange;
						detailedDatabaseSyncState.ProcessedOperationCount++;
						num2++;
						if (!perTable)
						{
							detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Format(CultureInfo.CurrentCulture, Lang.SyncDetailedState, syncStepNo, totalSteps, num2, num);
						}
						if (navSqlTableAndChange.NewTable != null && navSqlTableAndChange.TablePartitionMetadata == null)
						{
							navSqlTableAndChange = ResolveDesiredSqlPartition(navSqlTableAndChange, num3);
						}
						try
						{
							switch (rebuildAction)
							{
							case RebuildAction.PrepareOld:
								NavSqlDdlCommands.PrepareOldTable(tenantDatabase, navSqlTableAndChange, num3, syncMode2, NavCurrentThread.AppSyncContext?.AppSyncMode);
								break;
							case RebuildAction.PrepareNew:
								NavSqlDdlCommands.PrepareNewTable(tenantDatabase, navSqlTableAndChange, num3, syncMode2);
								break;
							case RebuildAction.RemoveExistingUpgradeTables:
								NavSqlDdlCommands.RemoveExistingUpgradeTable(tenantDatabase, navSqlTableAndChange, num3, syncMode2);
								break;
							case RebuildAction.CleanupOld:
								NavSqlDdlCommands.CleanupOldTable(tenantDatabase, navSqlTableAndChange, num3, syncMode2, NavCurrentThread.AppSyncContext?.AppSyncMode);
								break;
							case RebuildAction.FinalizeNew:
								NavSqlDdlCommands.FinalizeTable(tenantDatabase, navSqlTableAndChange, num3, syncMode2);
								break;
							}
							tableChange.DataTransferFromCompanyToken = navSqlTableAndChange.DataTransferFromCompanyToken;
						}
						catch (NavCSideException ex) when (ex.ErrorCode == 22926090)
						{
							if (navSqlTableAndChange.OldTable != null)
							{
								string tableName = NavSqlStatementHelper.AppendTableNameCompanyQualified(new StringBuilder(), navSqlTableAndChange.OldTable, tenantDatabase, num3, navSqlTableAndChange.TableContext).ToString();
								TableLocksLogger.AnalyzeAndLog(tenantDatabase, tableName);
							}
							string arg = ((navSqlTableAndChange.NewTable == null) ? navSqlTableAndChange.OldTable.TableName : navSqlTableAndChange.NewTable.TableName);
							throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.TableInUseCannotUpdateSQLDefinition, arg), ex);
						}
					}
				}
			}
		}
		finally
		{
			if (!perTable)
			{
				detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Empty;
			}
		}
	}

	private static IEnumerable<Guid> GetAppIdsAffectedByTableChanges(IEnumerable<NavSqlTableAndChange> tableChanges)
	{
		List<Guid> list = new List<Guid>();
		foreach (NavSqlTableAndChange tableChange in tableChanges)
		{
			IEnumerable<NCLMetaField> source = new List<NCLMetaField>();
			if (tableChange.Excessive)
			{
				source = tableChange.OldTable.AllFields;
			}
			else if (tableChange.FieldsAndChanges != null)
			{
				source = from fieldAndChange in tableChange.FieldsAndChanges
					where fieldAndChange.Excessive
					select fieldAndChange.OldField;
			}
			if (!source.Any())
			{
				continue;
			}
			IEnumerable<Guid> enumerable = (from f in source
				where f.SourceAppId.HasValue
				select f.SourceAppId.Value).Distinct();
			foreach (Guid item in enumerable)
			{
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	private void SynchronizeTableChanges(NavSession session, IEnumerable<NavSqlTableAndChange> tableChanges, int[] companyTokens, SyncMode syncMode, bool isUpgradeTableSynchronization, bool perTable = false)
	{
		IEnumerable<NavSqlTableAndChange> tableChanges2 = tableChanges.Where((NavSqlTableAndChange item) => item.OldTable != null && !item.OldTable.DataPerCompany);
		IEnumerable<NavSqlTableAndChange> tableChanges3 = tableChanges.Where((NavSqlTableAndChange item) => item.OldTable != null && item.OldTable.DataPerCompany);
		IEnumerable<NavSqlTableAndChange> tableChanges4 = tableChanges.Where((NavSqlTableAndChange item) => item.NewTable != null && !item.NewTable.DataPerCompany);
		IEnumerable<NavSqlTableAndChange> tableChanges5 = tableChanges.Where((NavSqlTableAndChange item) => item.NewTable != null && item.NewTable.DataPerCompany);
		int[] companyTokens2 = new int[1];
		if (!isUpgradeTableSynchronization)
		{
			if (session.AppInstallationContext != null)
			{
				IEnumerable<Guid> appIdsAffectedByTableChanges = GetAppIdsAffectedByTableChanges(tableChanges);
				foreach (Guid item in appIdsAffectedByTableChanges)
				{
					NavSqlDdlCommands.PurgeAppArchiveRecordsAndTablesForApp(tenantDatabase, item);
				}
			}
			SyncAllTables(tableChanges2, RebuildAction.PrepareOld, companyTokens2, syncMode, 1, 8, perTable);
			SyncAllTables(tableChanges3, RebuildAction.PrepareOld, companyTokens, syncMode, 2, 10, perTable);
		}
		else
		{
			SyncAllTables(tableChanges2, RebuildAction.RemoveExistingUpgradeTables, companyTokens2, syncMode, 2, 8, perTable);
			SyncAllTables(tableChanges3, RebuildAction.RemoveExistingUpgradeTables, companyTokens, syncMode, 1, 8, perTable);
		}
		SyncAllTables(tableChanges4, RebuildAction.PrepareNew, companyTokens2, syncMode, 3, 8, perTable);
		SyncAllTables(tableChanges5, RebuildAction.PrepareNew, companyTokens, syncMode, 4, 8, perTable);
		SyncAllTables(tableChanges3, RebuildAction.CleanupOld, companyTokens, syncMode, 5, 8, perTable);
		SyncAllTables(tableChanges5, RebuildAction.FinalizeNew, companyTokens, syncMode, 6, 8, perTable);
		SyncAllTables(tableChanges2, RebuildAction.CleanupOld, companyTokens2, syncMode, 7, 8, perTable);
		SyncAllTables(tableChanges4, RebuildAction.FinalizeNew, companyTokens2, syncMode, 8, 8, perTable);
	}

	private static NavTenantDatabaseSyncStatus CreateDatabaseSyncState(IEnumerable<NavSqlTableAndChange> changesToSynchronize, IEnumerable<NavSqlTableAndChange> upgradeTableSynchronization, int[] companyTokens, SyncMode syncMode)
	{
		int numberOfCompanies = companyTokens.Length;
		int num = (from tableAndChange in changesToSynchronize
			where tableAndChange.OldTable != null
			select (!tableAndChange.OldTable.DataPerCompany) ? 1 : numberOfCompanies).Sum() + (from tableAndChange in changesToSynchronize
			where tableAndChange.NewTable != null
			select (!tableAndChange.NewTable.DataPerCompany) ? 1 : numberOfCompanies).Sum() + ((syncMode == SyncMode.Sync) ? (upgradeTableSynchronization.Select((NavSqlTableAndChange tableAndChange) => (!tableAndChange.NewTable.DataPerCompany) ? 1 : numberOfCompanies).Sum() * 2) : 0);
		num *= 2;
		return new NavTenantDatabaseSyncStatus(0, num);
	}

	public void PerformSync(NavSession session, IEnumerable<NavSqlTableAndChange> tableChangesToSynchronize, GetTableSyncSetup getTableSyncSetup, int[] companyTokens, SyncMode syncMode, bool perTable = false)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000JQ", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Performing sync");
		if (!tableChangesToSynchronize.Any())
		{
			return;
		}
		if (syncMode != SyncMode.CheckOnly && tableChangesToSynchronize.Any((NavSqlTableAndChange tableAndChange) => tableAndChange.Missing || tableAndChange.Excessive || tableAndChange.TableRenamed))
		{
			NavSqlDdlTableInformationCommands.DropTableInformationStatsFunction(tenantDatabase);
		}
		if (getTableSyncSetup != null)
		{
			using (DiagnosticsResolver.CreateTimingScope("00000JR", Category.DatabaseSync, DiagnosticParameter.SystemMetadata("Preparing table sync setup")))
			{
				PrepareTableSyncSetup(tableChangesToSynchronize, getTableSyncSetup, syncMode);
			}
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000JS", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Clearing global cache");
		tenantDatabase.DatabaseState.ClearGlobalCache();
		IEnumerable<NavSqlTableAndChange> enumerable = tableChangesToSynchronize.Where((NavSqlTableAndChange tableAndChange) => tableAndChange.Missing || tableAndChange.Excessive || tableAndChange.ChangesAffectSqlSchema);
		IEnumerable<NavSqlTableAndChange> enumerable2 = from tableAndChange in enumerable
			where tableAndChange.DataUpgradeSetup == DataUpgradeSetup.OldAndNewPopulated || tableAndChange.DataUpgradeSetup == DataUpgradeSetup.OldPopulatedNewEmpty
			select new NavSqlTableAndChange(tableAndChange.OldTable, TableChangeType.Changed, tableAndChange.DataUpgradeTable, NavText.Empty, NavText.Empty, 0, tableAndChange.ChangedSortingProperties, tableAndChange.DatabaseConvertedFromNav2013, null, null, null, isUpgradeChange: true);
		detailedDatabaseSyncState = CreateDatabaseSyncState(enumerable, enumerable2, companyTokens, syncMode);
		if (perTable)
		{
			using (DiagnosticsResolver.CreateTimingScope("00000JT", Category.DatabaseSync, DiagnosticParameter.SystemMetadata("Synchronizing per table changes")))
			{
				IEnumerable<NavSqlTableAndChange> tablesAndChanges = tableChangesToSynchronize.Except(enumerable);
				IEnumerable<NavSqlTableAndChange> syncOnlyTables = enumerable.Except(enumerable2);
				int tablesToProcess = enumerable.Count() + enumerable2.Count();
				int tableProcessingIndex = 0;
				SynchronizeTableChangesPerTable(session, enumerable, syncOnlyTables, tablesToProcess, ref tableProcessingIndex, companyTokens, syncMode, isUpgradeTableSynchronization: false);
				if (syncMode == SyncMode.Sync && enumerable2.Any())
				{
					SynchronizeTableChangesPerTable(session, enumerable2, syncOnlyTables, tablesToProcess, ref tableProcessingIndex, companyTokens, SyncMode.ForceSync, isUpgradeTableSynchronization: true);
				}
				if (syncMode != SyncMode.CheckOnly)
				{
					NavSqlObjectMetadataSnapshotHandler.SaveTableMetadataToTenantDatabase(tenantDatabase, tablesAndChanges);
				}
				return;
			}
		}
		using (DiagnosticsResolver.CreateTimingScope("00000JU", Category.DatabaseSync, DiagnosticParameter.SystemMetadata("Synchronizing all table changes")))
		{
			SynchronizeTableChanges(session, enumerable, companyTokens, syncMode, isUpgradeTableSynchronization: false);
			if (syncMode == SyncMode.Sync && enumerable2.Any())
			{
				SynchronizeTableChanges(session, enumerable2, companyTokens, SyncMode.ForceSync, isUpgradeTableSynchronization: true);
			}
		}
	}

	private void SynchronizeTableChangesPerTable(NavSession session, IEnumerable<NavSqlTableAndChange> tableChanges, IEnumerable<NavSqlTableAndChange> syncOnlyTables, int tablesToProcess, ref int tableProcessingIndex, int[] companyTokens, SyncMode syncMode, bool isUpgradeTableSynchronization)
	{
		foreach (NavSqlTableAndChange tableChange in tableChanges)
		{
			using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(session.Database, updateOrlockingActivity: true))
			{
				try
				{
					tableProcessingIndex++;
					detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Format(CultureInfo.CurrentCulture, Lang.SyncDetailedStatePerTable, tableProcessingIndex, tablesToProcess);
					IEnumerable<NavSqlTableAndChange> enumerable = new List<NavSqlTableAndChange> { tableChange };
					SynchronizeTableChanges(session, enumerable, companyTokens, syncMode, isUpgradeTableSynchronization, perTable: true);
					if (syncMode != SyncMode.CheckOnly && (isUpgradeTableSynchronization || syncOnlyTables.Contains(tableChange)))
					{
						NavSqlObjectMetadataSnapshotHandler.SaveTableMetadataToTenantDatabase(tenantDatabase, enumerable);
					}
				}
				catch (Exception)
				{
					navSqlConnectionScope.Connection.EndTransaction(NavSqlEndTransactionType.Rollback);
					throw;
				}
			}
			detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Empty;
		}
	}

	private static void PrepareTableSyncSetup(IEnumerable<NavSqlTableAndChange> tableChangesToSynchronize, GetTableSyncSetup getTableSyncSetup, SyncMode syncMode)
	{
		IEnumerable<NavSqlTableAndChange> enumerable = tableChangesToSynchronize.Where((NavSqlTableAndChange t) => t.ChangesAffectSqlSchema);
		if (syncMode == SyncMode.ForceSync)
		{
			foreach (NavSqlTableAndChange item in enumerable)
			{
				item.NavTableSyncSetup = new NavTableSyncSetup(item.OldTable.TableId, 0, DataUpgradeSetup.ForceSync);
			}
			return;
		}
		Dictionary<int, NavSqlTableAndChange> dictionary = enumerable.ToDictionary((NavSqlTableAndChange tableChange) => tableChange.OldTable.TableId);
		IDictionary<int, NavTableSyncSetup> dictionary2 = getTableSyncSetup(dictionary.Keys);
		foreach (int key in dictionary2.Keys)
		{
			dictionary[key].NavTableSyncSetup = dictionary2[key];
		}
		if (enumerable.Any((NavSqlTableAndChange change) => change.OldTable.TableId != 2000000079))
		{
			NavSqlMissingTableSyncSetupValidator.ValidateNoSyncSetupErrors(enumerable);
		}
	}

	private IDictionary<int, NavTableSyncSetup> GetTableSyncSetup(IEnumerable<int> tableIds)
	{
		if (!tableIds.Any())
		{
			return new Dictionary<int, NavTableSyncSetup>();
		}
		DataModificationHandler dataModificationHandler = null;
		if (!tenantDatabase.TenantDatabaseProperties.PerCompanySchema && !tenantDatabase.TenantProperties.TenantLocalId.HasValue)
		{
			dataModificationHandler = new DataModificationHandler((NavRecord record, DataModificationType dataModificationType) =>
			{
				if (!SystemTables.ApplicationDatabaseTables.Contains(record.ObjectId.ObjectNumber) && !SystemTables.VirtualTables.Contains(record.ObjectId.ObjectNumber))
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000010K", Category.DatabaseSync, Verbosity.Error, PrivacyClassification.SystemMetadata, "Trying to do data changes ({0}) for table {1} in TableSyncSetup function.", dataModificationType, record.ObjectId.ObjectNumber);
					throw new NavSqlDatabaseSyncException("Data changes are not allowed in TableSynchSetup functions.");
				}
			});
			DataModificationListener.AddListener(dataModificationHandler);
		}
		try
		{
			tenantDatabase.UpgradeManager.RunTableSyncSetup(NavCurrentThread.Session, tableIds);
		}
		finally
		{
			if (dataModificationHandler != null)
			{
				DataModificationListener.RemoveListener(dataModificationHandler);
			}
		}
		return TableSynchSetupDataProvider.GetInstanceForSession(NavCurrentThread.Session).RawData;
	}

	internal NavSqlDatabaseLock AcquireCrossServerSyncLock(NavSqlConnectionScope conScope, bool lockOnlyCurrentTenant = false, bool updateTenantStateIfLockCannotBeAcquired = false)
	{
		int timeout = (NavGlobal.AppDatabase.SqlDatabaseProperties.LockTimeoutEnabled ? (NavGlobal.AppDatabase.SqlDatabaseProperties.LockTimeout * 1000) : (-1));
		lock (crossServerSyncLockObject)
		{
			NavSqlDatabaseLock navSqlDatabaseLock = (lockOnlyCurrentTenant ? NavSqlDatabaseLock.LockTenant(conScope, tenantDatabase.Tenant.Id, timeout) : NavSqlDatabaseLock.LockDatabase(conScope, timeout));
			if (!navSqlDatabaseLock.IsLockAcquired())
			{
				if (updateTenantStateIfLockCannotBeAcquired)
				{
					tenantDatabase.Tenant.State = NavTenantState.OperationalSyncInProgress;
				}
				throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.SynchronizationActionAlreadyRunning, NavGlobal.AppDatabase.SqlDatabaseProperties.LockTimeout, ProductNames.ProductNameClassicClient, ProductNames.ProductNameServer));
			}
			crossServerSyncLock = navSqlDatabaseLock;
			return crossServerSyncLock;
		}
	}

	private bool IsCrossServerLockAquired()
	{
		lock (crossServerSyncLockObject)
		{
			return crossServerSyncLock != null && crossServerSyncLock.IsLockAcquired();
		}
	}

	public void PerformSynchronizationAction(Action action, bool lockOnlyCurrentTenant = false)
	{
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(tenantDatabase, TransactionType.Browse);
		using (AcquireCrossServerSyncLock(conScope, lockOnlyCurrentTenant))
		{
			DatabaseSyncState databaseSyncState = this.databaseSyncState;
			NavTenantDatabaseSyncStatus navTenantDatabaseSyncStatus = detailedDatabaseSyncState;
			detailedDatabaseSyncState = new NavTenantDatabaseSyncStatus(0, 0);
			DatabaseSyncState = DatabaseSyncState.BeingSynchronized;
			try
			{
				action();
			}
			finally
			{
				detailedDatabaseSyncState = navTenantDatabaseSyncStatus;
				DatabaseSyncState = databaseSyncState;
			}
		}
	}

	public void OverwriteTenantId(bool ensureDatabaseExclusiveState = true)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(tenantDatabase, TransactionType.UpdateNoLocks);
		using (AcquireCrossServerSyncLock(navSqlConnectionScope))
		{
			if (tenantDatabase.IsNewTenantDatabase || tenantDatabase.TenantDatabaseProperties.VersionNo > (int)NavSqlTenantPropertiesSync.CurrentVersion)
			{
				throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.UnSupportedTenantDatabaseVersion, tenantDatabase.DatabaseName, tenantDatabase.TenantDatabaseProperties.VersionNo, ProductNames.ProductNameServer));
			}
			try
			{
				string text = tenantDatabase.TenantProperties.OverwriteTenantId(navSqlConnectionScope, tenantDatabase.TenantProperties.TenantId);
				if (text != null && text != tenantDatabase.TenantProperties.TenantId)
				{
					new DatabaseUninstalledAppRepository(tenantDatabase, navSqlConnectionScope).ChangeTenantId(text, tenantDatabase.TenantProperties.TenantId);
				}
				if (ensureDatabaseExclusiveState)
				{
					EnsureDatabaseInExclusiveStateForTenantIdOverwrite(navSqlConnectionScope);
				}
				NavTenant navTenant = NavEnvironment.Instance.Tenants.AllTenants().FirstOrDefault((NavTenant tenant) => tenant.TenantSettings.DatabaseName == tenantDatabase.Tenant.TenantSettings.DatabaseName && tenant.TenantSettings.DatabaseServer == tenantDatabase.Tenant.TenantSettings.DatabaseServer && tenant.Id != tenantDatabase.TenantProperties.TenantId);
				if (navTenant != null)
				{
					throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.CannotRenameTenantIdLooksLikeItsAlreadyMounted, navTenant.TenantSettings.Id));
				}
				if (tenantDatabase.Tenant.TenantSettings.DatabaseName == NavGlobal.AppDatabase.Tenant.TenantSettings.DatabaseName && tenantDatabase.Tenant.TenantSettings.DatabaseServer == NavGlobal.AppDatabase.Tenant.TenantSettings.DatabaseServer)
				{
					throw new NavSqlDatabaseSyncException(Lang.CannotRenameTenantIdLooksLikeItsAlreadyMountedAsAppDatabase);
				}
			}
			catch (Exception ex)
			{
				navSqlConnectionScope.TransactionEnd(NavSqlEndTransactionType.Rollback);
				detailedDatabaseSyncState.DatabaseSyncStateDetailed = ex.Message;
				DatabaseSyncState = DatabaseSyncState.Failed;
				throw;
			}
		}
	}

	internal void EnsureDatabaseInSync(SyncMode syncMode, bool perTable = false, bool sharedSchema = false)
	{
		NavEnvironment.Instance.ThrowIfInAppsDisabledMode("Cannot use the schema sync engine when extensions are disabled globally.");
		if (IsCurrentMetadataInSync || databaseSyncState == DatabaseSyncState.BeingSynchronized)
		{
			return;
		}
		bool sortingPropertiesDifferent;
		try
		{
			sortingPropertiesDifferent = ChangeDatabaseCollationIfNeeded(syncMode);
		}
		catch (Exception ex)
		{
			detailedDatabaseSyncState.DatabaseSyncStateDetailed = ex.Message;
			tenantDatabase.Tenant.Diagnostics.SendExceptionTag("000018U", Category.DatabaseSync, ex);
			if (syncMode == SyncMode.CheckOnly && ex is NavSqlDatabaseSyncException)
			{
				DatabaseSyncState = DatabaseSyncState.NotSynchronized;
			}
			else
			{
				DatabaseSyncState = DatabaseSyncState.Failed;
			}
			throw;
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000JX", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Acquiring DB sync lock");
		bool lockOnlyCurrentTenant = syncMode == SyncMode.CheckOnly;
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(tenantDatabase, TransactionType.UpdateNoLocks);
		using (AcquireCrossServerSyncLock(conScope, lockOnlyCurrentTenant, updateTenantStateIfLockCannotBeAcquired: true))
		{
			if (!IsCurrentMetadataInSync && databaseSyncState != DatabaseSyncState.BeingSynchronized)
			{
				int baseTableMetadataChangeBumperToken;
				using (NavSqlConnectionScope.CreateForceNewConnection(tenantDatabase, updateOrlockingActivity: true))
				{
					baseTableMetadataChangeBumperToken = NavGlobal.NCLMetadata.BaseTableMetadataChangeBumperToken;
					detailedDatabaseSyncState = new NavTenantDatabaseSyncStatus(0, 0);
					DatabaseSyncState = DatabaseSyncState.BeingSynchronized;
				}
				try
				{
					if (perTable)
					{
						((ITenantSessionHandler)tenantDatabase.Tenant).RunTenantActionInSystemSessionWithoutTransaction((Action<NavTenant, NavSession>)((NavTenant tenant, NavSession systemSession) =>
						{
							using (DiagnosticsResolver.GetMostSpecificInstance().CreateTimingScope("00000US", Category.DatabaseSync, DiagnosticParameter.SystemMetadata("Schema sync")))
							{
								SynchronizeTenantDatabase(systemSession, syncMode, perTable, sortingPropertiesDifferent, sharedSchema);
							}
						}), true, false, (LanguageSetting)null);
					}
					else
					{
						((ITenantSessionHandler)tenantDatabase.Tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant tenant, NavSession systemSession) =>
						{
							using (DiagnosticsResolver.GetMostSpecificInstance().CreateTimingScope("00000JY", Category.DatabaseSync, DiagnosticParameter.SystemMetadata("Schema sync")))
							{
								SynchronizeTenantDatabase(systemSession, syncMode, perTable, sortingPropertiesDifferent, sharedSchema);
							}
						}), true, false, false, (LanguageSetting)null);
					}
					if (tableSyncMetadataBumperToken == -1)
					{
						tenantDatabase.Tenant.OnFirstDatabaseAccess();
						if (!string.Equals(tenantDatabase.Tenant.Id, "nav-systemapplication", StringComparison.OrdinalIgnoreCase) && (pendingTableChanges == null || (!pendingTableChanges.Contains(2000000053) && !pendingTableChanges.Contains(2000000004))))
						{
							tenantDatabase.Tenant.RemoveOrphanedAccessControlRecords();
						}
					}
				}
				catch (Exception ex2)
				{
					detailedDatabaseSyncState.DatabaseSyncStateDetailed = ex2.Message;
					tenantDatabase.Tenant.Diagnostics.SendExceptionTag("000010R", Category.DatabaseSync, ex2);
					if (syncMode == SyncMode.CheckOnly && ex2 is NavSqlDatabaseSyncException)
					{
						DatabaseSyncState = DatabaseSyncState.NotSynchronized;
					}
					else
					{
						DatabaseSyncState = DatabaseSyncState.Failed;
					}
					throw;
				}
				if (syncMode != SyncMode.CheckOnly)
				{
					using (new MonitorLock(checkTableInSyncSynchObject, "checkTableInSyncSynchObject", tableSyncTimeout))
					{
						pendingTableChanges = null;
						checkTableInSyncMetadataBumperToken = baseTableMetadataChangeBumperToken;
					}
					detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Empty;
					DatabaseSyncState = DatabaseSyncState.Synchronized;
					tableSyncMetadataBumperToken = baseTableMetadataChangeBumperToken;
					applicationVersionNumber = NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion;
					UpdateTenantDatabaseVersions(syncMode);
					return;
				}
				using (new MonitorLock(checkTableInSyncSynchObject, "checkTableInSyncSynchObject", tableSyncTimeout))
				{
					GetPendingTableChanges("EnsureDatabaseInSync", forceUpdate: true);
					if (AnyPendingTableChanges == PendingTableChangesState.NoChanges)
					{
						tableSyncMetadataBumperToken = baseTableMetadataChangeBumperToken;
						applicationVersionNumber = NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion;
					}
				}
				databaseSyncState = ((tableSyncMetadataBumperToken != -1 && !(applicationVersionNumber == unknownApplicationVersionNumber) && AnyPendingTableChanges != PendingTableChangesState.PendingChanges) ? DatabaseSyncState.Synchronized : DatabaseSyncState.NotSynchronized);
			}
			if (syncMode != SyncMode.CheckOnly)
			{
				UpdateTenantDatabaseVersions(syncMode);
			}
		}
	}

	private void UpdateTenantDatabaseVersions(SyncMode syncMode)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000RV", Category.TenantDatabaseManagement, Verbosity.Normal, PrivacyClassification.SystemMetadata, "TenantDatabaseProperties updated with ApplicationVersion: " + NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion);
		NavGlobal.AppDatabase.SqlDatabaseProperties.RefreshDatabaseProperties();
		tenantDatabase.TenantDatabaseProperties.Refresh();
		tenantDatabase.TenantDatabaseProperties.StoreApplicationVersionSetting(NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion);
		if (syncMode == SyncMode.ForceSync)
		{
			tenantDatabase.TenantDatabaseVersion.TruncateApplicationVersions();
		}
		if (tenantDatabase.TenantDatabaseVersion.CountSupportedApplicationVersions() == 0 || !tenantDatabase.TenantDatabaseVersion.CheckIfSupportsApplicationVersion(NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily))
		{
			tenantDatabase.TenantDatabaseVersion.InsertNewApplicationVersion(NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily);
		}
	}

	private bool ChangeDatabaseCollationIfNeeded(SyncMode syncMode)
	{
		string collation = NavGlobal.AppDatabase.SqlSortingProperties.Collation;
		using (NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(tenantDatabase, TransactionType.UpdateNoLocks))
		{
			using (NavMetadataAccessContext.GetAccessToRemoved(NavCurrentThread.Session, Guid.NewGuid().ToString("B")))
			{
				if (tenantDatabase.Collation == collation)
				{
					return false;
				}
				if (syncMode == SyncMode.CheckOnly)
				{
					throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.TenantNotReadyForUseWrongDatabaseCollation, tenantDatabase.DatabaseName, tenantDatabase.Tenant.Id));
				}
				if (new DatabaseAppSyncTrackingRepository(tenantDatabase, conScope).GetAll().Any())
				{
					throw new NavSqlDatabaseSyncException(Lang.CollationChangeNotSupportedWithExtensions.FormatWithResolvedCulture(tenantDatabase.DatabaseName, collation, tenantDatabase.Collation));
				}
				DatabaseSyncState = DatabaseSyncState.BeingSynchronized;
				if (!tenantDatabase.IsNewTenantDatabase)
				{
					int[] companyTokens = tenantDatabase.GetCompanyTokens();
					int[] array = new int[1];
					NavSqlMetadata.LockTenantDatabaseForChangesToMetadata(conScope);
					Dictionary<int, NavText> baseTableIdsAndMetadataHashesFromTenantDatabase = NavSqlMetadata.GetBaseTableIdsAndMetadataHashesFromTenantDatabase(tenantDatabase);
					foreach (KeyValuePair<int, NavText> item in baseTableIdsAndMetadataHashesFromTenantDatabase)
					{
						NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromTenant(tenantDatabase, item.Key, out var table, databaseConvertedFromNav2013: false);
						IEnumerable<int> enumerable = (table.DataPerCompany ? companyTokens : array);
						detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Format(CultureInfo.CurrentCulture, Lang.SyncCollationChangeRemovingSiftIndex, table.TableName);
						foreach (int item2 in enumerable)
						{
							NavSqlDdlCommands.DropSiftIndexes(tenantDatabase, table, item2, NavAppTableContext.Main);
						}
					}
				}
			}
		}
		detailedDatabaseSyncState.DatabaseSyncStateDetailed = Lang.SyncChangingDatabaseCollation;
		AlterDatabaseCollation(collation);
		detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Empty;
		DatabaseSyncState = DatabaseSyncState.Synchronized;
		return true;
	}

	public void AlterDatabaseCollation(string newCollation)
	{
		try
		{
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateExclusiveConnection(tenantDatabase, TransactionType.Update);
			navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
			NavSqlCommand.Create(navSqlConnectionScope.Connection).ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "ALTER DATABASE [{0}] COLLATE {1}", tenantDatabase.DatabaseName, newCollation));
		}
		catch (NavBaseException ex)
		{
			throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.CouldNotSetDatabaseCollation, tenantDatabase.DatabaseName, tenantDatabase.Tenant.Id, ex.Message));
		}
	}

	public void ConvertTenantDatabase(bool perTable)
	{
		tenantDatabase.ValidateDatabaseState();
		if (DatabaseSyncState == DatabaseSyncState.BeingSynchronized || tenantDatabase.IsSharedTenantSchema)
		{
			return;
		}
		tenantDatabase.DatabaseSync.DatabaseSyncState = DatabaseSyncState.BeingSynchronized;
		tenantDatabase.Tenant.RefreshState();
		try
		{
			detailedDatabaseSyncState = new NavTenantDatabaseSyncStatus(0, 0);
			DatabaseSyncState = DatabaseSyncState.BeingSynchronized;
			if (perTable)
			{
				((ITenantSessionHandler)tenantDatabase.Tenant).RunTenantActionInSystemSessionWithoutTransaction((Action<NavTenant, NavSession>)((NavTenant tenant, NavSession systemSession) =>
				{
					using (DiagnosticsResolver.GetMostSpecificInstance().CreateTimingScope("00001LH", Category.DatabaseSync, DiagnosticParameter.SystemMetadata("Schema conversion")))
					{
						ConvertTenantDatabaseInternal(perTable);
					}
				}), true, false, (LanguageSetting)null);
			}
			else
			{
				((ITenantSessionHandler)tenantDatabase.Tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant tenant, NavSession systemSession) =>
				{
					using (DiagnosticsResolver.GetMostSpecificInstance().CreateTimingScope("00001LI", Category.DatabaseSync, DiagnosticParameter.SystemMetadata("Schema conversion")))
					{
						ConvertTenantDatabaseInternal(perTable);
					}
				}), true, false, false, (LanguageSetting)null);
			}
			if (tenantDatabase.GetCompanyTokens().Length == 0 && tenantDatabase.IsSharedTenantSchema)
			{
				CreateTablesFromMetadata(synchronizingEmptyDatabase: false);
			}
		}
		catch (Exception ex)
		{
			detailedDatabaseSyncState.DatabaseSyncStateDetailed = ex.Message;
			tenantDatabase.Tenant.Diagnostics.SendExceptionTag("00001LC", Category.DatabaseSync, ex);
			DatabaseSyncState = DatabaseSyncState.Failed;
			throw;
		}
		detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Empty;
		DatabaseSyncState = DatabaseSyncState.Synchronized;
	}

	private NavSqlConnectionScope GetConnectionScope(SyncMode syncMode, bool perTable)
	{
		if (!perTable || syncMode == SyncMode.CheckOnly)
		{
			return NavSqlConnectionScope.Create(tenantDatabase, TransactionType.Browse);
		}
		return NavSqlConnectionScope.CreateForceNewConnection(tenantDatabase, TransactionType.Update);
	}

	private void ConvertTenantDatabaseInternal(bool perTable)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001LD", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Start tenant conversion, perTable=" + perTable);
		tenantDatabase.TenantDatabaseProperties.Refresh();
		tenantDatabase.TenantProperties.Refresh();
		if (tenantDatabase.TenantCompanies.TenantExceededMaximumAllowedCompanies())
		{
			throw new NavSqlDatabaseSyncException("The database cannot be converted to shared schema model because it contains too many companies.");
		}
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, TransactionType.UpdateNoLocks))
		{
			using (AcquireCrossServerSyncLock(navSqlConnectionScope, lockOnlyCurrentTenant: true))
			{
				try
				{
					using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.UpdateNoLocks);
					NavSqlMetadata.LockAppDatabaseForChangesToMetadata(conScope);
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001LE", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "App DB locked");
					NavSqlMetadata.LockTenantDatabaseForChangesToMetadata(navSqlConnectionScope);
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001LF", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Tenant DB locked");
					NavSqlSharedDatabaseUpgradeSync navSqlSharedDatabaseUpgradeSync = new NavSqlSharedDatabaseUpgradeSync(tenantDatabase, detailedDatabaseSyncState, perTable, navSqlConnectionScope);
					navSqlSharedDatabaseUpgradeSync.RunUpgrade();
					SessionEventTableHandler.EnsureSessionEventTriggersExists(navSqlConnectionScope);
				}
				catch (Exception ex)
				{
					navSqlConnectionScope?.Connection.CheckEndTransaction(NavSqlEndTransactionType.Rollback);
					tenantDatabase.TenantDatabaseProperties.Refresh();
					tenantDatabase.TenantProperties.Refresh();
					if (ex is NavSqlDatabaseSyncException ex2)
					{
						throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.TenantDatabaseCouldNotBeSynchronized, ex2.Message));
					}
					throw;
				}
				finally
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001LG", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Leaving tenant conversion");
				}
			}
		}
		tenantDatabase.DatabaseServer.SqlConnectionPool.CloseAllAvailableConnections();
	}

	public bool IsEqualToAppDatabaseFamily()
	{
		return NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily == tenantDatabase.TenantDatabaseVersion.ApplicationFamily;
	}

	public bool IsSupportedPreviousVersionTenant()
	{
		if (!IsEqualToAppDatabaseFamily())
		{
			return false;
		}
		Version version = Version.Parse(NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion);
		if (Version.TryParse(tenantDatabase.TenantDatabaseProperties.ApplicationVersion, out var result) && Version.TryParse(tenantDatabase.TenantProperties.TenantDataVersion, out var result2) && result2 == version && result2 < result)
		{
			return tenantDatabase.TenantDatabaseVersion.CheckIfSupportsApplicationVersion(tenantDatabase.TenantProperties.TenantDataVersion);
		}
		return false;
	}

	private void SynchronizeTenantDatabase(NavSession session, SyncMode syncMode, bool perTable, bool sortingPropertiesDifferent, bool sharedSchema)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(tenantDatabase, TransactionType.UpdateNoLocks);
		using (NavMetadataAccessContext.GetAccessToRemoved(session, Guid.NewGuid().ToString("B")))
		{
			RefreshTenantProperties();
			try
			{
				if (syncMode == SyncMode.CheckOnly && !tenantDatabase.IsCurrentVersion)
				{
					string message = ((tenantDatabase.TenantDatabaseProperties.VersionNo >= 1080) ? string.Format(CultureInfo.InvariantCulture, "Tenant {0} in database {1} (application version {2}) cannot be used. The tenant can be brought to the operational state by synchronizing the database.", tenantDatabase.Tenant.Id, tenantDatabase.DatabaseName, tenantDatabase.TenantProperties.TenantDataVersion) : string.Format(CultureInfo.InvariantCulture, "Tenant {0} in database {1} is not operational because the application version is unknown. The tenant can be brought to the operational state by synchronizing the database.", tenantDatabase.Tenant.Id, tenantDatabase.DatabaseName));
					throw new NavSqlDatabaseSyncException(message);
				}
				if (syncMode == SyncMode.CheckOnly && IsSupportedPreviousVersionTenant())
				{
					return;
				}
				bool isNewTenantDatabase = tenantDatabase.IsNewTenantDatabase;
				NavSqlTenantPropertiesSync navSqlTenantPropertiesSync = NavSqlTenantPropertiesSync.Sync(tenantDatabase);
				if (!IsEqualToAppDatabaseFamily())
				{
					throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.InvariantCulture, "Tenant database has incompatible Application Family ({0}), current Application Family is {1}.", tenantDatabase.TenantDatabaseVersion.ApplicationFamily, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily));
				}
				if (navSqlTenantPropertiesSync.FillTenantCompanyTable)
				{
					using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(tenantDatabase, updateOrlockingActivity: true);
					NavSqlTenantCompaniesUpgrade.FillTableWithExistingCompanies(conScope);
				}
				ReadOnlyCollection<NavSqlTableAndChange> tableChanges;
				using (NavSqlConnectionScope conScope2 = NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, TransactionType.UpdateNoLocks))
				{
					NavSqlMetadata.LockAppDatabaseForChangesToMetadata(conScope2);
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000JZ", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "App DB locked");
					if (syncMode != SyncMode.CheckOnly && isNewTenantDatabase)
					{
						bool flag = false;
						if (tenantDatabase.Tenant.Id != "nav-systemapplication")
						{
							flag = tenantDatabase.TenantProperties.RegisterNewTenant();
						}
						SynchronizeNewDatabase(tenantDatabase.IsSharedTenantSchema | sharedSchema);
						if (flag && tenantDatabase.IsSharedTenantSchema)
						{
							CreateSequencesForTenantSystemAndNotPerCompanyTables();
						}
						if (sortingPropertiesDifferent)
						{
							tenantDatabase.TenantDatabaseProperties.StoreDatabaseCollationSetting(NavGlobal.AppDatabase.SqlSortingProperties.Collation);
						}
						SyncCloudReadyReplicationStatus(navSqlConnectionScope);
						return;
					}
					if (!perTable)
					{
						NavSqlMetadata.LockTenantDatabaseForChangesToMetadata(navSqlConnectionScope);
					}
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000K0", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Tenant DB locked");
					if (navSqlTenantPropertiesSync.AlterObjectMetadataSnapshot || navSqlTenantPropertiesSync.AddSchemaHashToObjectMetadata)
					{
						ApplyChangesToMetadataTables(syncMode, perTable, navSqlTenantPropertiesSync);
					}
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000K1", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Getting table changes");
					using (GetConnectionScope(syncMode, perTable))
					{
						tableChanges = GetTableChanges(sortingPropertiesDifferent, navSqlTenantPropertiesSync.UpgradeFromNav2013);
					}
				}
				if (navSqlTenantPropertiesSync.UpgradeFromNav2013)
				{
					using (GetConnectionScope(syncMode, perTable))
					{
						UpdateFromNav2013(navSqlConnectionScope);
					}
				}
				if (sortingPropertiesDifferent)
				{
					using (GetConnectionScope(syncMode, perTable))
					{
						AlterCollationOnSpecialTenantTables(navSqlConnectionScope, tenantDatabase);
					}
				}
				PerformSync(session, tableChanges, GetTableSyncSetup, tenantDatabase.GetCompanyTokens(), syncMode, perTable);
				if (!perTable && syncMode != SyncMode.CheckOnly)
				{
					NavSqlObjectMetadataSnapshotHandler.SaveTableMetadataToTenantDatabase(tenantDatabase, tableChanges);
				}
				if (sortingPropertiesDifferent)
				{
					tenantDatabase.TenantDatabaseProperties.StoreDatabaseCollationSetting(NavGlobal.AppDatabase.SqlSortingProperties.Collation);
					tenantDatabase.SqlSortingProperties = NavGlobal.AppDatabase.SqlSortingProperties;
				}
				if (navSqlTenantPropertiesSync.UpgradeAddCompanyId)
				{
					NavRecord navRecord = new NavRecord(session, 2000000006);
					NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(8000);
					StringBuilder stringBuilder = new StringBuilder("UPDATE ");
					stringBuilder.Append(navRecord.MetaTable.SqlTableName);
					stringBuilder.Append(" SET ");
					stringBuilder.Append(fieldByNo.SqlColumnName);
					stringBuilder.Append(" = NEWID() WHERE ");
					stringBuilder.Append(fieldByNo.SqlColumnName);
					stringBuilder.Append(" = CAST(0x0 AS UNIQUEIDENTIFIER)");
					NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
					navSqlCommand.ExecuteNonQuery(stringBuilder.ToString());
				}
				if (syncMode != SyncMode.CheckOnly)
				{
					SyncCloudReadyReplicationStatus(navSqlConnectionScope);
				}
			}
			catch (Exception ex)
			{
				navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Rollback);
				RefreshTenantProperties();
				if (ex is NavSqlDatabaseSyncException ex2 && syncMode != SyncMode.CheckOnly)
				{
					throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.TenantDatabaseCouldNotBeSynchronized, ex2.Message));
				}
				throw;
			}
			finally
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000KX", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Leaving schema sync");
			}
		}
	}

	internal void ApplyChangesToMetadataTables(SyncMode syncMode, bool perTable, NavSqlTenantPropertiesSync sqlTenantPropertiesSync)
	{
		using NavSqlConnectionScope tenantDatabaseScope = GetConnectionScope(syncMode, perTable);
		AlterObjectMetadataSnapshotTable(tenantDatabaseScope, tenantDatabase, sqlTenantPropertiesSync);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000K3", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Populating snapshot table with missing base records");
		NavSqlMetadata.PopulateBaseTableMetadataInTenantDatabaseForAllRecordsWhereMissing(tenantDatabase);
	}

	private void RefreshTenantProperties()
	{
		tenantDatabase.TenantDatabaseProperties.Refresh();
		tenantDatabase.TenantDatabaseVersion.Refresh();
		tenantDatabase.TenantProperties.Refresh();
	}

	internal void ValidateTableLayoutChanges(int tableId, string baseMetadata, GetTableSyncSetup getTableSyncSetup)
	{
		PerformSynchronizationAction(() =>
		{
			NCLMetaTable newTable = null;
			TableChangeType changeType = TableChangeType.Changed;
			if (string.IsNullOrEmpty(baseMetadata))
			{
				changeType = TableChangeType.Excessive;
			}
			else
			{
				try
				{
					XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
					xmlDocument.Load(XmlHelper.SafeXmlReader(new StringReader(baseMetadata)));
					int versionNo = 1;
					MetaTable metaTableFromMetadata = NavSqlObjectMetadataSnapshotHandler.GetMetaTableFromMetadata(xmlDocument.DocumentElement, versionNo);
					NCLMetaApplicationObjectLoader loader = NCLMetaApplicationObjectLoader.CreateFromNavGlobal();
					newTable = NCLMetaTable.CreateEmptyNCLMetaTable(loader, tableId, NavAppGroup.BaseGroup);
					bool databaseConvertedFromNav = tenantDatabase.TenantDatabaseProperties.VersionNo < 1000;
					newTable.AssignFromMetaTable(metaTableFromMetadata, databaseConvertedFromNav);
				}
				catch (XmlException innerException)
				{
					throw new NavNCLInvalidMetadataException(Lang.CSIDESentInvalidTableMetadataForValidation, innerException);
				}
			}
			if (!tenantDatabase.IsNewTenantDatabase && NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromTenant(tenantDatabase, tableId, out var oldTable, databaseConvertedFromNav2013: false))
			{
				int[] companyTokens = tenantDatabase.GetCompanyTokens();
				((ITenantSessionHandler)tenantDatabase.Tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant tenant, NavSession systemSession) =>
				{
					PerformSync(systemSession, new NavSqlTableAndChange[1]
					{
						new NavSqlTableAndChange(oldTable, changeType, newTable, NavText.Empty, NavText.Empty, 0, changedSortingProperties: false, databaseConvertedFromNav2013: false)
					}, getTableSyncSetup, companyTokens, SyncMode.CheckOnly);
				}), true, false, false, (LanguageSetting)null);
			}
		});
	}

	internal void SynchronizeTables(int[] tableIds, SyncMode syncMode)
	{
		if (!tableIds.Any())
		{
			return;
		}
		if (tenantDatabase.HasCapability(SupportedCapability.TablePartition))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001CO", Category.DatabaseSync, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Query partitions");
			tablePartitionsMetadata = NavSqlMetadataCommands.GetTablesPartitionMetadata(tenantDatabase);
		}
		PerformSynchronizationAction(() =>
		{
			((ITenantSessionHandler)tenantDatabase.Tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant tenant, NavSession systemSession) =>
			{
				using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(tenantDatabase, TransactionType.UpdateNoLocks))
				{
					using (NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, TransactionType.UpdateNoLocks))
					{
						NavSqlTenantPropertiesSync navSqlTenantPropertiesSync = NavSqlTenantPropertiesSync.Sync(tenantDatabase);
						if (navSqlTenantPropertiesSync.AlterObjectMetadataSnapshot || navSqlTenantPropertiesSync.AddSchemaHashToObjectMetadata)
						{
							AlterObjectMetadataSnapshotTable(navSqlConnectionScope, tenantDatabase, navSqlTenantPropertiesSync);
						}
						if (navSqlTenantPropertiesSync.FillTenantCompanyTable)
						{
							NavSqlTenantCompaniesUpgrade.FillTableWithExistingCompanies(navSqlConnectionScope);
						}
						try
						{
							List<NavSqlTableAndChange> list = new List<NavSqlTableAndChange>();
							int[] array = tableIds;
							foreach (int tableId in array)
							{
								NCLMetaTable table = null;
								NCLMetaTable table2 = null;
								NavBLOB rawMetadata = null;
								NavBLOB rawMetadata2 = null;
								NavText newHash = null;
								int schemaHash = 0;
								int schemaHash2 = 0;
								try
								{
									NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromAppDatabase(tableId, out table2, out rawMetadata2, out newHash, out schemaHash2);
								}
								catch (NavMetadataNotFoundException)
								{
								}
								NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromTenant(tenantDatabase, tableId, out table, databaseConvertedFromNav2013: false, out rawMetadata);
								if (table2 != null && table2.IsExternalDataSource)
								{
									table2 = null;
								}
								if (table != null && table.IsExternalDataSource)
								{
									table = null;
								}
								if (table2 != null || table != null)
								{
									TableChangeType change = ((table2 != null && table != null) ? TableChangeType.Changed : ((table2 != null) ? TableChangeType.Missing : TableChangeType.Excessive));
									list.Add(new NavSqlTableAndChange(table, change, table2, newHash, newHash, schemaHash, changedSortingProperties: false, databaseConvertedFromNav2013: false, rawMetadata2, rawMetadata, rawMetadata2));
								}
							}
							int[] companyTokens = tenantDatabase.GetCompanyTokens();
							PerformSync(systemSession, list, GetTableSyncSetup, companyTokens, syncMode);
							if (syncMode != SyncMode.CheckOnly)
							{
								NavSqlObjectMetadataSnapshotHandler.SaveTableMetadataToTenantDatabase(tenantDatabase, list);
							}
						}
						catch
						{
							navSqlConnectionScope.TransactionEnd(NavSqlEndTransactionType.Rollback);
							throw;
						}
					}
				}
				if (NavGlobal.AppDatabase.ChangeListener != null)
				{
					NavGlobal.AppDatabase.ChangeListener.ForceHandleTrackingEvent();
				}
				GetPendingTableChanges("SynchronizeTables", forceUpdate: true);
			}), true, false, false, (LanguageSetting)null);
		});
	}

	internal void ValidateTableLayoutChanges(int tableId, string baseMetadata)
	{
		ValidateTableLayoutChanges(tableId, baseMetadata, GetTableSyncSetup);
	}

	public void CheckTableInSync(int tableThatMustBeInSync)
	{
		if (IsCrossServerLockAquired() || Monitor.IsEntered(checkTableInSyncSynchObject))
		{
			return;
		}
		using (new MonitorLock(checkTableInSyncSynchObject, "checkTableInSyncSynchObject", tableSyncTimeout))
		{
			GetPendingTableChanges("CheckTableInSync (" + tableThatMustBeInSync + ")", pendingTableChanges != null && pendingTableChanges.Contains(tableThatMustBeInSync));
			if (pendingTableChanges != null && pendingTableChanges.Contains(tableThatMustBeInSync))
			{
				string original = string.Format(CultureInfo.InvariantCulture, "Metadata is out of sync -- Table:{0}", tableThatMustBeInSync);
				original = TraceStringHelper.AppendStackTrace(original);
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000KG", Category.DatabaseSync, Verbosity.Warning, PrivacyClassification.SystemMetadata, original);
				throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.TableNeedsSynchronizationError, tableThatMustBeInSync));
			}
		}
	}

	public void EnsurePendingTableChangesAreCurrent()
	{
		using (new MonitorLock(checkTableInSyncSynchObject, "checkTableInSyncSynchObject", tableSyncTimeout))
		{
			GetPendingTableChanges("EnsurePendingTableChangesAreCurrent");
		}
	}

	public void ForceRefreshOfPendingTableChanges()
	{
		using (new MonitorLock(checkTableInSyncSynchObject, "checkTableInSyncSynchObject", tableSyncTimeout))
		{
			GetPendingTableChanges("ForceRefreshOfPendingTableChanges", forceUpdate: true);
		}
	}

	private void GetPendingTableChanges(string reason, bool forceUpdate = false)
	{
		int num = checkTableInSyncMetadataBumperToken;
		int baseTableMetadataChangeBumperToken = NavGlobal.NCLMetadata.BaseTableMetadataChangeBumperToken;
		bool flag = num != baseTableMetadataChangeBumperToken;
		if (flag | forceUpdate)
		{
			NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
			mostSpecificInstance.SendTraceTag("00000KH", Category.DatabaseSync, Verbosity.Normal, "Updating pending table changes -- OldBase:{0}; NewBase:{1}; Force:{2}; Reason:{3}; Tenant:{4}", DiagnosticParameter.SystemMetadata(num), DiagnosticParameter.SystemMetadata(baseTableMetadataChangeBumperToken), DiagnosticParameter.SystemMetadata(forceUpdate), DiagnosticParameter.SystemMetadata(reason), DiagnosticParameter.Oii(tenantDatabase.Tenant.Id));
			KeyValuePair<string, List<string>> supportedFamilyAndVersions = NavSqlTenantDatabaseVersion.GetSupportedFamilyAndVersions(tenantDatabase);
			NavGlobal.AppDatabase.SqlDatabaseProperties.RefreshDatabaseProperties();
			bool flag2 = TenantDatabaseConfiguration.IsApplicationFamilyCompatible(supportedFamilyAndVersions.Key, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily) && TenantDatabaseConfiguration.IsApplicationVersionSynchronizable(supportedFamilyAndVersions.Value, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion);
			DiagnosticParameter firstParameter = DiagnosticParameter.SystemMetadata(flag2);
			DiagnosticParameter[] array = new DiagnosticParameter[4]
			{
				DiagnosticParameter.SystemMetadata(supportedFamilyAndVersions.Key),
				DiagnosticParameter.SystemMetadata(NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationFamily),
				null,
				null
			};
			IEnumerable<string> value = supportedFamilyAndVersions.Value;
			array[2] = DiagnosticParameter.SystemMetadata(string.Join(", ", value ?? Enumerable.Empty<string>()));
			array[3] = DiagnosticParameter.SystemMetadata(NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion);
			mostSpecificInstance.SendTraceTag("00001Z9", Category.DatabaseSync, Verbosity.Normal, "Updating pending table changes -- CanSynchronize:{0} TenantApplicationFamily:{1}; ServerApplicationFamily:{2}; SupportedVersions:{3}; ServerApplicationVersion:{4}", firstParameter, array);
			if (flag2)
			{
				pendingTableChanges = GetChangedTableIds(mostSpecificInstance);
			}
			else
			{
				pendingTableChanges = null;
			}
			IEnumerable<string> values = pendingTableChanges?.Select((int c) => c.ToString(CultureInfo.InvariantCulture)) ?? new List<string>();
			mostSpecificInstance.SendTraceTag("00000KI", Category.DatabaseSync, Verbosity.Normal, "Updated pending table changes -- Tables:{0}; Tenant:{1}", DiagnosticParameter.SystemMetadata(string.Join(", ", values)), DiagnosticParameter.Oii(tenantDatabase.Tenant.Id));
			checkTableInSyncMetadataBumperToken = NavGlobal.NCLMetadata.BaseTableMetadataChangeBumperToken;
			tenantDatabase.Tenant.RefreshState();
		}
		if (num == -1)
		{
			tenantDatabase.Tenant.OnFirstDatabaseAccess();
		}
	}

	public int StartSyncProcessIfNotAlreadyStarted(SyncMode syncMode, bool perTable = false, bool sharedSchema = false)
	{
		return StartProcessIfNotAlreadyStarted(() =>
		{
			EnsureDatabaseInSync(syncMode, perTable, sharedSchema);
		});
	}

	public int StartConvertProcessIfNotAlreadyStarted(bool perTable = false)
	{
		return StartProcessIfNotAlreadyStarted(() =>
		{
			ConvertTenantDatabase(perTable);
		});
	}

	private int StartProcessIfNotAlreadyStarted(Action actionToPerform)
	{
		lock (startNewSyncTaskSyncObject)
		{
			if (isDisposing || IsDisposed)
			{
				throw new ObjectDisposedException(typeof(NavSqlDatabaseSync).Name);
			}
			if (syncTask != null && (syncTask.Status == TaskStatus.Faulted || syncTask.Status == TaskStatus.RanToCompletion))
			{
				try
				{
					syncTask.WaitThrowingOriginalException();
				}
				finally
				{
					syncTask = null;
				}
			}
			if (syncTask == null)
			{
				syncTask = NavTaskFactory.RunTask(() =>
				{
					try
					{
						actionToPerform();
					}
					catch (NavBaseException)
					{
					}
				});
				return (syncTask != null) ? syncTask.Id : 0;
			}
		}
		throw new NavSqlDatabaseSyncException(Lang.TenantOperationIsAlreadyRunning);
	}

	private void PerformNavAppSync(NavSession session, IEnumerable<NavSqlTableAndChange> tableChangesToSynchronize, int[] companyTokens)
	{
		NavAppInstallationContext appInstallationContext = session.AppInstallationContext;
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000Q0", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Performing sync -- Changes:{0}", tableChangesToSynchronize.Count());
		if (tableChangesToSynchronize.Any())
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000Q1", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Clearing global cache");
			tenantDatabase.DatabaseState.ClearGlobalCache();
			IEnumerable<NavSqlTableAndChange> enumerable = tableChangesToSynchronize.Where((NavSqlTableAndChange tableAndChange) => tableAndChange.Missing || tableAndChange.Excessive || tableAndChange.ChangesAffectSqlSchema);
			detailedDatabaseSyncState = CreateDatabaseSyncState(enumerable, Enumerable.Empty<NavSqlTableAndChange>(), companyTokens, appInstallationContext.SyncMode);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000Q2", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Synchronizing table changes");
			SynchronizeTableChanges(session, enumerable, companyTokens, appInstallationContext.SyncMode, isUpgradeTableSynchronization: false);
		}
	}

	private void PerformAppSync(NavSession session, IEnumerable<SyncChangeResult> tableChangesToSynchronize, int[] companyTokens)
	{
		AppSyncContext appSyncContext = session.AppSyncContext;
		if (tableChangesToSynchronize.Any())
		{
			IEnumerable<NavSqlTableAndChange> enumerable = from t in tableChangesToSynchronize
				select t.TableAndChange into tableAndChange
				where tableAndChange != null && (tableAndChange.Missing || tableAndChange.Excessive || tableAndChange.ChangesAffectSqlSchema)
				select tableAndChange;
			detailedDatabaseSyncState = CreateDatabaseSyncState(enumerable, Enumerable.Empty<NavSqlTableAndChange>(), companyTokens, SyncMode.ForceSync);
			using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("0000156", DiagnosticParameter.SystemMetadata("Performing SQL schema sync"), MonitorType.SqlWriteCount, Category.Extensions))
			{
				SynchronizeTableChanges(session, enumerable, companyTokens, appSyncContext.SyncEngineMode, isUpgradeTableSynchronization: false);
			}
		}
	}

	public SyncNavAppResponse SynchronizeTenantDatabaseForAppSync(AppSyncContext appSyncContext)
	{
		SyncNavAppResponse result = new SyncNavAppResponse(SyncNavAppStatus.UnknownAppError);
		if (appSyncContext == null)
		{
			throw new ArgumentNullException("appSyncContext");
		}
		NavEnvironment.Instance.ThrowIfInAppsDisabledMode("Cannot use the schema sync engine when extensions are disabled globally.");
		if (databaseSyncState != DatabaseSyncState.BeingSynchronized)
		{
			using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(tenantDatabase, TransactionType.Browse);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006Y7", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Acquiring cross server DB lock");
			using (AcquireCrossServerSyncLock(conScope))
			{
				if (databaseSyncState != DatabaseSyncState.BeingSynchronized)
				{
					int versionNo = tenantDatabase.TenantDatabaseProperties.VersionNo;
					int currentVersion = (int)NavSqlTenantPropertiesSync.CurrentVersion;
					if (versionNo != currentVersion)
					{
						throw new NavAppSyncException(Lang.NavAppTenantVersionNotCurrent);
					}
					detailedDatabaseSyncState = new NavTenantDatabaseSyncStatus(0, 0);
					DatabaseSyncState = DatabaseSyncState.BeingSynchronized;
					try
					{
						((ITenantSessionHandler)tenantDatabase.Tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant tenant, NavSession systemSession) =>
						{
							DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000018F", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "System session transition -- {0}", NavAppTraceHelper.CreateActivityIdString(appSyncContext.ActivityId));
							systemSession.SetAppSyncContext(appSyncContext);
							DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006Y8", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "SynchronizeTenantDatabaseForAppSyncInternal");
							SynchronizeTenantDatabaseForAppSyncInternal(systemSession);
						}), true, true, false, (LanguageSetting)null);
						result = new SyncNavAppResponse(SyncNavAppStatus.AppSyncSuccessful);
					}
					catch (NavAppSyncValidationException ex)
					{
						result = new SyncNavAppResponse(SyncNavAppStatus.SyncValidationFailed, ex.Message);
					}
					catch (Exception ex2)
					{
						result = new SyncNavAppResponse(SyncNavAppStatus.UnknownAppError, ex2.Message);
					}
					finally
					{
						detailedDatabaseSyncState.DatabaseSyncStateDetailed = string.Empty;
						DatabaseSyncState = DatabaseSyncState.Synchronized;
					}
				}
			}
		}
		return result;
	}

	private void SynchronizeTenantDatabaseForAppSyncInternal(NavSession session)
	{
		AppSyncContext syncContext = session.AppSyncContext;
		RefreshTenantProperties();
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(tenantDatabase, TransactionType.UpdateNoLocks);
		string text = string.Format(CultureInfo.InvariantCulture, "AppId:{0}; {1}{2}{3}OldVersion:{4}; Mode:{5}; Tenant:{6}; TenantDatabase:{7}", syncContext.AppId, (syncContext.AppForAdd != null) ? ("Name:" + syncContext.AppForAdd.Name + "; ") : "", (syncContext.AppForAdd != null) ? ("Publisher:" + syncContext.AppForAdd.Publisher + "; ") : "", (syncContext.AppForAdd != null) ? ("Version:" + syncContext.Version?.ToString() + "; ") : "", syncContext.OldVersion, syncContext.AppSyncMode, tenantDatabase.Tenant.Id, tenantDatabase.DatabaseName);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000157", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Initiating App Sync -- " + text);
		try
		{
			IAppSyncSnapshotRepository appSyncSnapshotRepository = new DatabaseAppSyncSnapshotRepository(tenantDatabase, navSqlConnectionScope);
			IAppSyncTrackingRepository appSyncTrackingRepository = new DatabaseAppSyncTrackingRepository(tenantDatabase, navSqlConnectionScope);
			IEnumerable<SyncChangeResult> enumerable;
			using (NavSqlConnectionScope appDatabaseScope = NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, TransactionType.UpdateNoLocks))
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000158", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Acquiring App DB lock");
				HandleAcquireAppDatabaseLockWithRetries(appDatabaseScope);
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000159", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Acquiring Tenant DB lock");
				NavSqlMetadata.LockTenantDatabaseForChangesToMetadata(navSqlConnectionScope);
				AppSyncTrackingRecord forApp = appSyncTrackingRepository.GetForApp(syncContext.AppId);
				List<AppSyncSnapshotRecord> snapshots = appSyncSnapshotRepository.GetAllForApp(syncContext.AppId).ToList();
				V2AbnormalTableFixer v2AbnormalTableFixer = new V2AbnormalTableFixer(tenantDatabase, navSqlConnectionScope, appSyncSnapshotRepository);
				V2FaultyFlowFieldFixer v2FaultyFlowFieldFixer = new V2FaultyFlowFieldFixer(tenantDatabase, navSqlConnectionScope, appSyncSnapshotRepository);
				V2UserDefinedTimestampFixer v2UserDefinedTimestampFixer = new V2UserDefinedTimestampFixer(appSyncSnapshotRepository);
				List<Action<IList<AppSyncSnapshotRecord>>> list = new List<Action<IList<AppSyncSnapshotRecord>>> { v2AbnormalTableFixer.RemoveNonNormalSnapshotTables, v2FaultyFlowFieldFixer.RemoveFlowFieldsFromSchemaAndSnapshots, v2UserDefinedTimestampFixer.RemoveUserDefinedTimestampFromSnapshot };
				if (syncContext.AppSyncMode != NavAppSyncMode.Clean)
				{
					list.ForEach((Action<IList<AppSyncSnapshotRecord>> action) =>
					{
						action(snapshots);
					});
				}
				IEnumerable<MetaTable> structuralTablesForApp = NavGlobal.SystemTenant.SyncMetadataResolver.GetStructuralTablesForApp(syncContext.AppForAdd, syncContext.TargetGroup);
				IEnumerable<SyncTableBundle> source = SyncTableBundler.CreateTableBundles(syncContext.AppSyncMode, syncContext.AppForAdd, structuralTablesForApp, snapshots, SyncBundleFilters.AllFilters);
				switch (syncContext.AppSyncMode)
				{
				case NavAppSyncMode.Add:
					if (forApp?.Version >= syncContext.Version)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006K5", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Sync concurrency scenario -- internals reached but already synced. Exiting operation.");
						return;
					}
					break;
				case NavAppSyncMode.Clean:
				{
					IUninstalledAppRepository uninstalledAppRepository = new DatabaseUninstalledAppRepository(tenantDatabase, navSqlConnectionScope);
					uninstalledAppRepository.DeleteAllForApp(syncContext.AppId, deleteForAllTenants: true);
					break;
				}
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006Y9", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Getting table changes");
				enumerable = source.Select((SyncTableBundle b) => SyncChangeRetriever.GetChanges(syncContext, b)).ToList();
				List<string> list2 = new List<string>();
				foreach (SyncChangeResult item in enumerable)
				{
					if (item.ValidationErrors.Any())
					{
						string message = string.Join(Environment.NewLine, item.ValidationErrors);
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000183", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, message);
						list2.AddRange(item.ValidationErrors);
					}
				}
				if (list2.Any())
				{
					throw new NavAppSyncValidationException(string.Join(Environment.NewLine, list2.Select((string error) => error.ToString())));
				}
			}
			if (enumerable.Any())
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006YA", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Modifying SQL schema");
				PerformAppSync(session, enumerable, tenantDatabase.GetCompanyTokens());
			}
			SyncRepositoryUpdater syncRepositoryUpdater = new SyncRepositoryUpdater(appSyncSnapshotRepository, appSyncTrackingRepository);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006YB", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Updating snapshot repository");
			syncRepositoryUpdater.UpdateSnapshotRepository(syncContext, enumerable);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006YC", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Updating tracking repository");
			syncRepositoryUpdater.UpdateTrackingRepository(syncContext);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006YD", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Updating SyncCloudReadyReplicationStatus Table");
			SyncCloudReadyReplicationStatus(navSqlConnectionScope, enumerable);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000015E", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "App sync execution completed");
		}
		catch (Exception ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001X1", Category.Extensions, Verbosity.Error, PrivacyClassification.OrganizationIdentifiableInformation, "App Sync Exception -- {0}", ex);
			navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Rollback);
			if (ex is NavSqlDatabaseSyncException ex2)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000015F", Category.Extensions, ex2);
				throw new NavAppSyncException(string.Format(CultureInfo.CurrentCulture, Lang.TenantDatabaseCouldNotBeSynchronized, ex2.Message));
			}
			throw;
		}
		finally
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000015H", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Clearing App Sync Context");
			session.ClearAppSyncContext();
		}
	}

	private static void HandleAcquireAppDatabaseLockWithRetries(NavSqlConnectionScope appDatabaseScope)
	{
		int num = 4;
		do
		{
			try
			{
				NavSqlMetadata.LockAppDatabaseForChangesToMetadata(appDatabaseScope);
				num = 0;
			}
			catch (NavCSideException ex) when (ex.ErrorCode == 22926090)
			{
				if (--num <= 0)
				{
					throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001JS", Category.Extensions, ex, Verbosity.Error, DiagnosticParameter.SystemMetadata("Extension operation acquire App DB lock :: Failure"));
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001JT", Category.Extensions, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Extension operation acquire App DB lock :: Retry");
			}
		}
		while (num > 0);
	}

	private static void WireUpExtenions(NavDatabase tenantDB, NavSqlConnectionScope tenantScope, NavSqlConnectionScope appDBScope)
	{
		List<INavAppMetadataReference> appsInstalledForTenantInTenantDatabase = NavAppInstallationManagement.GetAppsInstalledForTenantInTenantDatabase(tenantDB);
		NavAppInstallationManagement.PurgeAndInsertNewTenantAppInstallationRecordsInternal(NavGlobal.AppDatabase, appDBScope, appsInstalledForTenantInTenantDatabase.Select((INavAppMetadataReference a) => a.PackageId).ToList(), tenantDB.Tenant.Id);
		NavEnvironment.Instance.NavAppGroupUpdater.UpdateGroupForChangedTenant(tenantDB.Tenant);
	}

	private static void PurgeAndInsertUninstalledRecordsForTenant(NavDatabase tenantDB, NavSqlConnectionScope tenantDBScope, string sourceTenantId)
	{
		DatabaseUninstalledAppRepository databaseUninstalledAppRepository = new DatabaseUninstalledAppRepository(tenantDB, tenantDBScope);
		IEnumerable<UninstalledAppRecord> allForTenant = databaseUninstalledAppRepository.GetAllForTenant(sourceTenantId);
		foreach (UninstalledAppRecord item in allForTenant)
		{
			databaseUninstalledAppRepository.InsertOrUpdate(item);
		}
	}

	private static void PurgeUninstalledRecordsForTenant(NavDatabase tenantDB, NavSqlConnectionScope tenantDBScope)
	{
		DatabaseUninstalledAppRepository databaseUninstalledAppRepository = new DatabaseUninstalledAppRepository(tenantDB, tenantDBScope);
		databaseUninstalledAppRepository.DeleteAllForTenant();
	}

	private void EnsureDatabaseInExclusiveStateForTenantIdOverwrite(NavSqlConnectionScope tenantDatabaseScope)
	{
		string arg;
		string arg2;
		string arg3;
		NavSqlCommand navSqlCommand;
		using (navSqlCommand = NavSqlCommand.Create(tenantDatabaseScope.Connection))
		{
			using (navSqlCommand.ExecuteReader("\r\n                        SELECT TOP 1 hostname, hostprocess, program_name\r\n                        FROM sys.sysprocesses\r\n                        WHERE spid = @@SPID"))
			{
				navSqlCommand.ReadNextRow();
				arg = navSqlCommand.Reader.GetString(0).Trim();
				arg2 = navSqlCommand.Reader.GetString(1).Trim();
				arg3 = navSqlCommand.Reader.GetString(2).Trim();
				navSqlCommand.CloseReader(forceClose: false);
			}
		}
		using (navSqlCommand = NavSqlCommand.Create(tenantDatabaseScope.Connection))
		{
			int num = navSqlCommand.ExecuteScalar<int>(string.Format(CultureInfo.InvariantCulture, "\r\n                                SELECT COUNT(0)\r\n                                FROM sys.sysprocesses A\r\n                                WHERE A.dbid = DB_ID()\r\n                                    AND A.program_name LIKE '{0}%'\r\n                                    AND (A.hostprocess <> {1} OR A.hostname <> '{2}')", arg3, arg2, arg));
			if (num > 0)
			{
				throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.CannotRenameTenantIdWhileDatabaseIsInUse, tenantDatabase.DatabaseName, ProductNames.ProductName));
			}
		}
	}
}
