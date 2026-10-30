using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavDatabaseChangeListener : IDisposable
{
	private const string TrackingInfoSelect = "SELECT CT.SYS_CHANGE_VERSION, 0[Change Type], CT.SYS_CHANGE_OPERATION, CT.[Object Type], CT.[Object ID], '00000000-0000-0000-0000-000000000000'[App Package ID], ''[Tenant ID] FROM CHANGETABLE(CHANGES [{0}].[Object Metadata], @maxChangeTrackingNumberObjectMetadata) CT\r\nUNION ALL\r\nSELECT CT.SYS_CHANGE_VERSION, 1[Change Type], CT.SYS_CHANGE_OPERATION, 0[Object Type], 0[Object ID], '00000000-0000-0000-0000-000000000000'[App Package ID], ''[Tenant ID] FROM CHANGETABLE(CHANGES [Debugger Breakpoint], @maxChangeTrackingNumberDebuggerBreakpoint) CT\r\nUNION ALL\r\nSELECT CT.SYS_CHANGE_VERSION, 2[Change Type], CT.SYS_CHANGE_OPERATION, 0[Object Type], 0[Object ID], '00000000-0000-0000-0000-000000000000'[App Package ID], ''[Tenant ID] FROM CHANGETABLE(CHANGES [{0}].[Add-in], @maxChangeTrackingNumberAddIn) CT WHERE CT.SYS_CHANGE_OPERATION = 'U'\r\nUNION ALL\r\nSELECT CT.SYS_CHANGE_VERSION, 3[Change Type], CT.SYS_CHANGE_OPERATION, CT.[Object Type], CT.[Object ID], CT.[App Package ID], ''[Tenant ID] FROM CHANGETABLE(CHANGES [{0}].[NAV App Object Metadata], @maxChangeTrackingNumberNavAppObjectMetadata) CT\r\nUNION ALL\r\nSELECT CT.SYS_CHANGE_VERSION, 4[Change Type], CT.SYS_CHANGE_OPERATION, 0[Object Type], 0[Object ID], CT.[App Package ID], CT.[Tenant ID] FROM CHANGETABLE(CHANGES [{0}].[NAV App Tenant App], @maxChangeTrackingNumberNavAppTenantApp) CT\r\nUNION ALL\r\nSELECT CT.SYS_CHANGE_VERSION, 5[Change Type], CT.SYS_CHANGE_OPERATION, 0[Object Type], 0[Object ID], CT.[Package ID], ''[Tenant ID] FROM CHANGETABLE(CHANGES [{0}].[NAV App], @maxChangeTrackingNumberNavApp) CT\r\nUNION ALL\r\nSELECT CT.SYS_CHANGE_VERSION, 6[Change Type], CT.SYS_CHANGE_OPERATION, 0[Object Type], 0[Object ID], CT.[App Package ID], CT.[Tenant ID] FROM CHANGETABLE(CHANGES [{0}].[NAV App Publish Reference], @maxChangeTrackingNumberNavAppPublishReference) CT";

	private const string EnableTableChangeTrackingStatement = "IF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id=Object_ID('[{0}].[{1}]')) ALTER TABLE [{0}].[{1}] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON)";

	private const string EnableDatabaseChangeTrackingStatement = "IF NOT EXISTS (SELECT * FROM sys.change_tracking_databases t JOIN sys.databases d ON d.database_id = t.database_id WHERE d.name = '{0}') ALTER DATABASE [{0}] SET CHANGE_TRACKING = ON (CHANGE_RETENTION = 10 DAYS, AUTO_CLEANUP = ON)";

	private const string EnableTablesChangeTrackingStatement = "IF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id=Object_ID('[{0}].[Object Metadata]')) ALTER TABLE [{0}].[Object Metadata] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);\r\nIF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id=Object_ID('[{0}].[Debugger Breakpoint]')) ALTER TABLE [{0}].[Debugger Breakpoint] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);\r\nIF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id=Object_ID('[{0}].[Add-in]')) ALTER TABLE [{0}].[Add-in] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);\r\nIF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id=Object_ID('[{0}].[NAV App Object Metadata]')) ALTER TABLE [{0}].[NAV App Object Metadata] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);\r\nIF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id=Object_ID('[{0}].[NAV App Tenant App]')) ALTER TABLE [{0}].[NAV App Tenant App] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);\r\nIF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id=Object_ID('[{0}].[NAV App]')) ALTER TABLE [{0}].[NAV App] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);\r\nIF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id=Object_ID('[{0}].[NAV App Publish Reference]')) ALTER TABLE [{0}].[NAV App Publish Reference] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);\r\nSELECT CONVERT(BIGINT, CHANGE_TRACKING_CURRENT_VERSION())";

	private readonly NavDatabase database;

	private bool isDisposed;

	private NonReEntrantTimerWrapper pollingTimer;

	private long maxChangeTrackingNumberObjectMetadata;

	private long maxChangeTrackingNumberDebuggerBreakpoint;

	private long maxChangeTrackingNumberAddIn;

	private long maxChangeTrackingNumberNavAppObjectMetadata;

	private long maxChangeTrackingNumberNavAppTenantApp;

	private long maxChangeTrackingNumberNavApp;

	private long maxChangeTrackingNumberNavAppPublishReference;

	private List<NavTrackingObject> trackingChanges;

	private readonly object notificationSyncObject = new object();

	private IReference<NavSqlConnection> lastUsedSqlConnection;

	public long MaxChangeTrackingNumberObjectMetadata => maxChangeTrackingNumberObjectMetadata;

	public int PollingInterval { get; } = 2000;

	internal void HandleTrackingEvent()
	{
		if (!isDisposed)
		{
			CollectTrackingObjects();
			if (trackingChanges != null)
			{
				database.HandleTrackingEvent(trackingChanges);
				trackingChanges = null;
			}
		}
	}

	internal void ForceHandleTrackingEvent()
	{
		if (!Monitor.TryEnter(notificationSyncObject, (int)ServerUserSettings.Instance.MetadataLockTimeout.Value.TotalMilliseconds))
		{
			database.Tenant?.Diagnostics.SendTraceTag("000093D", Category.Runtime, Verbosity.Warning, PrivacyClassification.SystemMetadata, "ForceHandleTrackingEvent failed to acquire a lock");
			throw new NavLockTimeoutException("ForceHandleTrackingEvent failed to acquire a lock");
		}
		try
		{
			HandleTrackingEvent();
		}
		finally
		{
			Monitor.Exit(notificationSyncObject);
		}
	}

	internal void TimerHandleTrackingEvent()
	{
		try
		{
			HandleTrackingEvent();
		}
		catch (NavCSideException ex)
		{
			switch (ex.ErrorCode)
			{
			case 22926089:
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001IG", Category.DatabaseSync, Verbosity.Warning, PrivacyClassification.SystemMetadata, "SQL Change Tracking failed to process metadata changes due to SQL deadlock exception. The operation will be retried.");
				break;
			default:
				throw;
			case 22924076:
			case 22926090:
				break;
			}
		}
		catch (NavSqlException ex2)
		{
			if (!NavSqlConnection.CheckIfNavSqlExceptionIsTimeoutException(ex2) && !NavSqlReturnCode.IsTransientError(ex2.ErrorNumber))
			{
				throw;
			}
		}
		catch (Exception exception)
		{
			LogWriterHelper.LogExceptionEvent(exception, EventLogEntryType.Error);
			throw;
		}
	}

	private void TimerHandler(object param)
	{
		if (Monitor.TryEnter(notificationSyncObject))
		{
			try
			{
				TimerHandleTrackingEvent();
			}
			finally
			{
				Monitor.Exit(notificationSyncObject);
			}
		}
	}

	internal void CollectTrackingObjects()
	{
		if (database == null)
		{
			throw new InvalidOperationException("this.database is null");
		}
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(database, TransactionType.Update, ref lastUsedSqlConnection);
		if (navSqlConnectionScope == null)
		{
			throw new InvalidOperationException("conScope is null");
		}
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("ChangeListenerDetails", database), () => string.Format(CultureInfo.InvariantCulture, "SELECT CT.SYS_CHANGE_VERSION, 0[Change Type], CT.SYS_CHANGE_OPERATION, CT.[Object Type], CT.[Object ID], '00000000-0000-0000-0000-000000000000'[App Package ID], ''[Tenant ID] FROM CHANGETABLE(CHANGES [{0}].[Object Metadata], @maxChangeTrackingNumberObjectMetadata) CT\r\nUNION ALL\r\nSELECT CT.SYS_CHANGE_VERSION, 1[Change Type], CT.SYS_CHANGE_OPERATION, 0[Object Type], 0[Object ID], '00000000-0000-0000-0000-000000000000'[App Package ID], ''[Tenant ID] FROM CHANGETABLE(CHANGES [Debugger Breakpoint], @maxChangeTrackingNumberDebuggerBreakpoint) CT\r\nUNION ALL\r\nSELECT CT.SYS_CHANGE_VERSION, 2[Change Type], CT.SYS_CHANGE_OPERATION, 0[Object Type], 0[Object ID], '00000000-0000-0000-0000-000000000000'[App Package ID], ''[Tenant ID] FROM CHANGETABLE(CHANGES [{0}].[Add-in], @maxChangeTrackingNumberAddIn) CT WHERE CT.SYS_CHANGE_OPERATION = 'U'\r\nUNION ALL\r\nSELECT CT.SYS_CHANGE_VERSION, 3[Change Type], CT.SYS_CHANGE_OPERATION, CT.[Object Type], CT.[Object ID], CT.[App Package ID], ''[Tenant ID] FROM CHANGETABLE(CHANGES [{0}].[NAV App Object Metadata], @maxChangeTrackingNumberNavAppObjectMetadata) CT\r\nUNION ALL\r\nSELECT CT.SYS_CHANGE_VERSION, 4[Change Type], CT.SYS_CHANGE_OPERATION, 0[Object Type], 0[Object ID], CT.[App Package ID], CT.[Tenant ID] FROM CHANGETABLE(CHANGES [{0}].[NAV App Tenant App], @maxChangeTrackingNumberNavAppTenantApp) CT\r\nUNION ALL\r\nSELECT CT.SYS_CHANGE_VERSION, 5[Change Type], CT.SYS_CHANGE_OPERATION, 0[Object Type], 0[Object ID], CT.[Package ID], ''[Tenant ID] FROM CHANGETABLE(CHANGES [{0}].[NAV App], @maxChangeTrackingNumberNavApp) CT\r\nUNION ALL\r\nSELECT CT.SYS_CHANGE_VERSION, 6[Change Type], CT.SYS_CHANGE_OPERATION, 0[Object Type], 0[Object ID], CT.[App Package ID], CT.[Tenant ID] FROM CHANGETABLE(CHANGES [{0}].[NAV App Publish Reference], @maxChangeTrackingNumberNavAppPublishReference) CT", "dbo"), (NavSqlCommand sqlDetailsCmd) =>
		{
			sqlDetailsCmd.Command.Parameters.Add("@maxChangeTrackingNumberObjectMetadata", SqlDbType.BigInt);
			sqlDetailsCmd.Command.Parameters.Add("@maxChangeTrackingNumberDebuggerBreakpoint", SqlDbType.BigInt);
			sqlDetailsCmd.Command.Parameters.Add("@maxChangeTrackingNumberAddIn", SqlDbType.BigInt);
			sqlDetailsCmd.Command.Parameters.Add("@maxChangeTrackingNumberNavAppObjectMetadata", SqlDbType.BigInt);
			sqlDetailsCmd.Command.Parameters.Add("@maxChangeTrackingNumberNavAppTenantApp", SqlDbType.BigInt);
			sqlDetailsCmd.Command.Parameters.Add("@maxChangeTrackingNumberNavApp", SqlDbType.BigInt);
			sqlDetailsCmd.Command.Parameters.Add("@maxChangeTrackingNumberNavAppPublishReference", SqlDbType.BigInt);
		}, (NavSqlCommand sqlDetailsCmd) =>
		{
			sqlDetailsCmd.Command.Parameters[0].Value = maxChangeTrackingNumberObjectMetadata;
			sqlDetailsCmd.Command.Parameters[1].Value = maxChangeTrackingNumberDebuggerBreakpoint;
			sqlDetailsCmd.Command.Parameters[2].Value = maxChangeTrackingNumberAddIn;
			sqlDetailsCmd.Command.Parameters[3].Value = maxChangeTrackingNumberNavAppObjectMetadata;
			sqlDetailsCmd.Command.Parameters[4].Value = maxChangeTrackingNumberNavAppTenantApp;
			sqlDetailsCmd.Command.Parameters[5].Value = maxChangeTrackingNumberNavApp;
			sqlDetailsCmd.Command.Parameters[6].Value = maxChangeTrackingNumberNavAppPublishReference;
		}, (NavSqlCommand sqlDetailsCmd) =>
		{
			using (sqlDetailsCmd.ExecuteReader())
			{
				while (sqlDetailsCmd.ReadNextRow())
				{
					if (trackingChanges == null)
					{
						trackingChanges = new List<NavTrackingObject>(10);
					}
					if (sqlDetailsCmd.Reader == null)
					{
						throw new InvalidOperationException("sqlDetailsCmd.Reader is null");
					}
					long @int = sqlDetailsCmd.Reader.GetInt64(0);
					ObjectTrackingChangeType int2 = (ObjectTrackingChangeType)sqlDetailsCmd.Reader.GetInt32(1);
					string text = sqlDetailsCmd.Reader.GetString(2);
					ApplicationObjectId applicationObjectId = new ApplicationObjectId((ObjectType)sqlDetailsCmd.Reader.GetInt32(3), sqlDetailsCmd.Reader.GetInt32(4));
					Guid guid = sqlDetailsCmd.Reader.GetGuid(5);
					string tenantId = sqlDetailsCmd.Reader.GetString(6);
					switch (int2)
					{
					case ObjectTrackingChangeType.Metadata:
						if (@int > maxChangeTrackingNumberObjectMetadata)
						{
							maxChangeTrackingNumberObjectMetadata = @int;
						}
						break;
					case ObjectTrackingChangeType.Breakpoints:
						if (@int > maxChangeTrackingNumberDebuggerBreakpoint)
						{
							maxChangeTrackingNumberDebuggerBreakpoint = @int;
						}
						break;
					case ObjectTrackingChangeType.ClientAddIn:
						if (@int > maxChangeTrackingNumberAddIn)
						{
							maxChangeTrackingNumberAddIn = @int;
						}
						break;
					case ObjectTrackingChangeType.NavAppMetadata:
						if (@int > maxChangeTrackingNumberNavAppObjectMetadata)
						{
							maxChangeTrackingNumberNavAppObjectMetadata = @int;
						}
						break;
					case ObjectTrackingChangeType.NavAppTenant:
						if (@int > maxChangeTrackingNumberNavAppTenantApp)
						{
							maxChangeTrackingNumberNavAppTenantApp = @int;
						}
						break;
					case ObjectTrackingChangeType.NavApp:
						if (@int > maxChangeTrackingNumberNavApp)
						{
							maxChangeTrackingNumberNavApp = @int;
						}
						break;
					case ObjectTrackingChangeType.NavAppPublishReference:
						if (@int > maxChangeTrackingNumberNavAppPublishReference)
						{
							maxChangeTrackingNumberNavAppPublishReference = @int;
						}
						break;
					}
					trackingChanges.Add(new NavTrackingObject(maxChangeTrackingNumberObjectMetadata, applicationObjectId, int2, PackageId.op_Implicit(guid), tenantId, text switch
					{
						"U" => ObjectTrackingChangeOperation.Update, 
						"I" => ObjectTrackingChangeOperation.Insert, 
						_ => ObjectTrackingChangeOperation.Delete, 
					}));
				}
				sqlDetailsCmd.CloseReader(forceClose: false);
			}
			return 0;
		});
	}

	public NavDatabaseChangeListener(NavDatabase database)
	{
		if (database == null)
		{
			throw new ArgumentNullException("database");
		}
		this.database = database;
	}

	public void Initialize()
	{
		int pollingInterval = PollingInterval;
		if (pollingTimer != null)
		{
			pollingTimer.Dispose();
			pollingTimer = null;
		}
		long num = 0L;
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(database, updateOrlockingActivity: true))
		{
			try
			{
				navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
				using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection))
				{
					navSqlCommand.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "IF NOT EXISTS (SELECT * FROM sys.change_tracking_databases t JOIN sys.databases d ON d.database_id = t.database_id WHERE d.name = '{0}') ALTER DATABASE [{0}] SET CHANGE_TRACKING = ON (CHANGE_RETENTION = 10 DAYS, AUTO_CLEANUP = ON)", database.DatabaseName));
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000XE", Category.DatabaseSync, Verbosity.Normal, "SQL Change Tracking enabled on database '{0}'", DiagnosticParameter.SystemMetadata(NavGlobal.AppDatabase.DatabaseName));
				using (NavSqlCommand navSqlCommand2 = NavSqlCommand.Create(navSqlConnectionScope.Connection))
				{
					num = (maxChangeTrackingNumberObjectMetadata = (maxChangeTrackingNumberDebuggerBreakpoint = (maxChangeTrackingNumberAddIn = (maxChangeTrackingNumberNavAppObjectMetadata = (maxChangeTrackingNumberNavAppTenantApp = (maxChangeTrackingNumberNavApp = (maxChangeTrackingNumberNavAppPublishReference = navSqlCommand2.ExecuteScalar<long>(string.Format(CultureInfo.InvariantCulture, "IF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id=Object_ID('[{0}].[Object Metadata]')) ALTER TABLE [{0}].[Object Metadata] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);\r\nIF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id=Object_ID('[{0}].[Debugger Breakpoint]')) ALTER TABLE [{0}].[Debugger Breakpoint] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);\r\nIF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id=Object_ID('[{0}].[Add-in]')) ALTER TABLE [{0}].[Add-in] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);\r\nIF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id=Object_ID('[{0}].[NAV App Object Metadata]')) ALTER TABLE [{0}].[NAV App Object Metadata] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);\r\nIF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id=Object_ID('[{0}].[NAV App Tenant App]')) ALTER TABLE [{0}].[NAV App Tenant App] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);\r\nIF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id=Object_ID('[{0}].[NAV App]')) ALTER TABLE [{0}].[NAV App] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);\r\nIF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id=Object_ID('[{0}].[NAV App Publish Reference]')) ALTER TABLE [{0}].[NAV App Publish Reference] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON);\r\nSELECT CONVERT(BIGINT, CHANGE_TRACKING_CURRENT_VERSION())", "dbo")))))))));
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000XF", Category.DatabaseSync, Verbosity.Normal, PrivacyClassification.SystemMetadata, "SQL Change Tracking enabled on tables 'Object Metadata', 'Debugger Breakpoint', 'Add-In', 'Nav App Object Metadata', 'Nav App Tenant App', 'NAV App', 'NAV App Publish Reference'");
			}
			catch (Exception exception)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000XG", Category.DatabaseSync, exception);
				throw;
			}
		}
		if (NavEnvironment.Instance.InBackgroundChangeListenerDisabledMode)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000VA", Category.Runtime, Verbosity.Normal, "DB change listener initialized -- no background timer - initial value {0}", DiagnosticParameter.SystemMetadata(num));
			return;
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000VB", Category.Runtime, Verbosity.Normal, "DB change listener initialized - initial value {0}", DiagnosticParameter.SystemMetadata(num));
		pollingTimer = new NonReEntrantTimerWrapper("NavDatabaseChangeListener", TimerHandler, null, pollingInterval, startImmediately: true);
		LogWriterHelper.LogEvent(EventLogEntryType.Information, string.Format(CultureInfo.CurrentCulture, Lang.ChangeListenerStarted, database.ServerName, database.DatabaseName));
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		lock (notificationSyncObject)
		{
			if (!isDisposed)
			{
				pollingTimer?.Dispose();
				isDisposed = true;
			}
		}
	}

	public static void EnableSqlChangeTrackingOnTable(NavSqlConnectionScope conScope, NCLMetaTable table)
	{
		using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection))
		{
			navSqlCommand.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "IF NOT EXISTS (SELECT * FROM sys.change_tracking_tables WHERE object_id=Object_ID('[{0}].[{1}]')) ALTER TABLE [{0}].[{1}] ENABLE CHANGE_TRACKING WITH (TRACK_COLUMNS_UPDATED = ON)", "dbo", table.SqlTableName));
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000XH", Category.DatabaseSync, Verbosity.Normal, "SQL Change Tracking enabled on table '{0}'", DiagnosticParameter.SystemMetadata(table.SqlTableName));
	}

	public static void EnableSqlChangeTrackingOnDatabase()
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, updateOrlockingActivity: true);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
		navSqlCommand.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "IF NOT EXISTS (SELECT * FROM sys.change_tracking_databases t JOIN sys.databases d ON d.database_id = t.database_id WHERE d.name = '{0}') ALTER DATABASE [{0}] SET CHANGE_TRACKING = ON (CHANGE_RETENTION = 10 DAYS, AUTO_CLEANUP = ON)", NavGlobal.AppDatabase.DatabaseName));
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000XI", Category.DatabaseSync, Verbosity.Normal, "SQL Change Tracking enabled on database '{0}'", DiagnosticParameter.SystemMetadata(NavGlobal.AppDatabase.DatabaseName));
	}
}
