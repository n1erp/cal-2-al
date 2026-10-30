using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Authentication;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavDatabase : TreeObject, IReferenceTarget<NavDatabase>
{
	private const string CmdHasPermission = "SELECT HAS_PERMS_BY_NAME(null, null, 'VIEW SERVER STATE'), HAS_PERMS_BY_NAME(null, null, 'ALTER ANY EVENT SESSION')";

	private const string CmdHasPermissionAzure = "SELECT HAS_PERMS_BY_NAME(null, 'DATABASE', 'VIEW DATABASE STATE'), 1";

	private const string NoCompanyName = "NO COMPANY";

	public const int CompanyTokenForTablesThatAreNotPerCompany = 0;

	public const int CompanyAgnosticCompanyToken = int.MinValue;

	private readonly object syncLock = new object();

	private readonly object appInformationSyncLock = new object();

	private bool firstAccessCalled;

	private ConcurrentDictionary<SupportedCapability, bool> supportedCapabilitesCache = new ConcurrentDictionary<SupportedCapability, bool>();

	private readonly IReference<NavDatabase> selfReference;

	private NavTenant tenant;

	private readonly NavDatabaseServer databaseServer;

	private readonly string databaseName;

	private readonly string normalizedName;

	private readonly NavDatabaseType databaseType;

	private SqlSortingProperties sqlSortingProperties;

	private CollationAwareStringComparer collationAwareStringComparer;

	private readonly DataCacheDatabaseState databaseState;

	private readonly NavUserCache navUsers;

	private readonly NavDatabaseHealth databaseHealth;

	private NavDatabaseChangeListener listener;

	private readonly NavSqlDatabaseProperties sqlDatabaseProperties;

	private readonly TableConnectionSettingsStorage tableConnectionSettingsStorage;

	private NavDatabaseSecurityAndLicense securityAndLicense;

	private NavSqlTenantProperties tenantProperties;

	private NavSqlTenantDatabaseProperties tenantDatabaseProperties;

	private NavSqlTenantDatabaseVersion tenantDatabaseVersion;

	private NavSqlTenantCompanies tenantCompanies;

	private readonly NavSqlDatabaseSync databaseSync;

	private NavDataUpgradeManager upgradeManager;

	private readonly ExternalSqlTableConnectionSettings externalSqlTableConnectionSettings;

	private readonly List<string> companyNames;

	private readonly Lazy<NCLMetaQueryDefinition> allCompaniesQuerySharedLock = new Lazy<NCLMetaQueryDefinition>(() => CreateCompanyTableQuery(DataLockState.Shared), isThreadSafe: true);

	private readonly Lazy<NCLMetaQueryDefinition> allCompaniesQuery = new Lazy<NCLMetaQueryDefinition>(() => CreateCompanyTableQuery(DataLockState.None), isThreadSafe: true);

	private readonly Lazy<NCLMetaQueryDefinition> allCompanyIdentifierQuery = new Lazy<NCLMetaQueryDefinition>(() => CreateCompanyTableIdentifierQuery(DataLockState.None), isThreadSafe: true);

	private IEqualityComparer<string> companyNameComparer;

	private NavText applicationVersion;

	private NavDatabaseSharedSchemaExtension sharedSchemaExtension;

	private NavText applicationBuild;

	internal NavSqlServerProperties sqlServerProperties;

	internal NavText ApplicationVersion
	{
		get
		{
			if (applicationVersion != null)
			{
				return applicationVersion;
			}
			lock (appInformationSyncLock)
			{
				if (applicationVersion == null)
				{
					UpdateApplicationInformation();
				}
			}
			return applicationVersion ?? NavText.Empty;
		}
		private set
		{
			if (databaseType != NavDatabaseType.Application)
			{
				throw new NotSupportedException("This should only be used on the application database");
			}
			applicationVersion = value;
		}
	}

	internal NavDatabaseSharedSchemaExtension SharedSchemaExtension
	{
		get
		{
			if (sharedSchemaExtension == null)
			{
				lock (syncLock)
				{
					if (sharedSchemaExtension == null)
					{
						sharedSchemaExtension = new NavDatabaseSharedSchemaExtension(this);
					}
				}
			}
			return sharedSchemaExtension;
		}
	}

	internal NavText ApplicationBuild
	{
		get
		{
			if (applicationBuild != null)
			{
				return applicationBuild;
			}
			lock (appInformationSyncLock)
			{
				if (applicationBuild == null)
				{
					UpdateApplicationInformation();
				}
			}
			return applicationBuild ?? NavText.Empty;
		}
		private set
		{
			if (databaseType != NavDatabaseType.Application)
			{
				throw new NotSupportedException("This should only be used on the application database");
			}
			applicationBuild = value;
		}
	}

	public NavSqlServerProperties SqlServerProperties
	{
		get
		{
			if (sqlServerProperties == null)
			{
				sqlServerProperties = NavSqlServerProperties.QueryNavSqlServerProperties(this);
			}
			return sqlServerProperties;
		}
	}

	public bool IsAzureDatabase => SqlServerProperties.EngineEdition == SqlServerEngineEdition.Azure;

	public INavSequenceProvider SequenceProvider { get; private set; }

	public NavSqlConnectionInfo ConnectionInfo
	{
		get
		{
			(SqlConnectionStringBuilder, SqlCredential) tuple = DatabaseServer.SqlConnectionPool.CreateConnectionInfo(DatabaseName);
			if (databaseType == NavDatabaseType.External)
			{
				tuple.Item1.ConnectTimeout = externalSqlTableConnectionSettings.ConnectionTimeout;
			}
			return new NavSqlConnectionInfo(tuple.Item1.ConnectionString, tuple.Item2);
		}
	}

	public NavDatabaseServer DatabaseServer => databaseServer;

	public string ServerName => DatabaseServer.ServerName;

	public string DatabaseName => databaseName;

	public string NormalizedDatabaseName => normalizedName;

	internal DataCacheDatabaseState DatabaseState => databaseState;

	public NavDatabaseChangeListener ChangeListener => listener;

	public string Collation => databaseType switch
	{
		NavDatabaseType.Application => SqlDatabaseProperties.Collation, 
		NavDatabaseType.Tenant => TenantDatabaseProperties.Collation, 
		NavDatabaseType.External => NavGlobal.AppDatabase.Collation, 
		_ => throw new InvalidOperationException("Unknown databasetype"), 
	};

	public NavDatabaseHealth Health => databaseHealth;

	public bool HasViewStatePermission { get; private set; }

	public bool HasAlterAnyEventSessionPermission { get; private set; }

	public string Key => string.Format(CultureInfo.CurrentCulture, "Server={0}, Name={1}, Type={2}", databaseServer?.ServerName, databaseName, databaseType);

	internal CacheSynchronization CacheSynchronization { get; private set; }

	internal PermissionSetupMonitor PermissionSetupMonitor { get; private set; }

	internal ProfileSetupMonitor ProfileSetupMonitor { get; private set; }

	internal EntitlementSetupMonitor EntitlementSetupMonitor { get; private set; }

	internal UserTableChangeMonitor UserTableChangeMonitor { get; private set; }

	internal WebServiceSetupMonitor WebServiceSetupMonitor { get; private set; }

	internal CompanyTableChangeMonitor CompanyTableChangeMonitor { get; private set; }

	public NavTenant Tenant
	{
		get
		{
			return tenant;
		}
		internal set
		{
			tenant = value;
		}
	}

	public NavSystemTenant SystemTenant => NavEnvironment.Instance.Tenants.SystemTenant;

	public NavSqlDatabaseProperties SqlDatabaseProperties => sqlDatabaseProperties;

	internal TableConnectionSettingsStorage TableConnectionSettingsStorage => tableConnectionSettingsStorage;

	public bool SqlDatabasePropertiesRead
	{
		get
		{
			if (sqlDatabaseProperties != null)
			{
				return sqlDatabaseProperties.DatabasePropertiesReady;
			}
			return false;
		}
	}

	public NavSqlTenantProperties TenantProperties
	{
		get
		{
			if (tenantProperties == null)
			{
				tenantProperties = new NavSqlTenantProperties(this);
			}
			return tenantProperties;
		}
	}

	public NavSqlTenantDatabaseProperties TenantDatabaseProperties
	{
		get
		{
			if (tenantDatabaseProperties == null)
			{
				tenantDatabaseProperties = new NavSqlTenantDatabaseProperties(this);
			}
			return tenantDatabaseProperties;
		}
	}

	public NavSqlTenantDatabaseVersion TenantDatabaseVersion
	{
		get
		{
			if (tenantDatabaseVersion == null)
			{
				tenantDatabaseVersion = new NavSqlTenantDatabaseVersion(this);
			}
			return tenantDatabaseVersion;
		}
	}

	internal NavSqlTenantCompanies TenantCompanies
	{
		get
		{
			if (tenantCompanies == null)
			{
				tenantCompanies = new NavSqlTenantCompanies(this);
			}
			return tenantCompanies;
		}
	}

	public bool IsNewTenantDatabase => !TenantProperties.TableExists;

	public bool IsCurrentVersion => TenantDatabaseProperties.VersionNo == (int)NavSqlTenantPropertiesSync.CurrentVersion;

	public bool IsSharedTenantSchema
	{
		get
		{
			if (databaseType != NavDatabaseType.Tenant)
			{
				return false;
			}
			return !TenantDatabaseProperties.PerCompanySchema;
		}
	}

	public NavUserCache NavUsers => navUsers;

	public NavDatabaseSecurityAndLicense SecurityAndLicense => securityAndLicense;

	public NavDatabaseType DatabaseType => databaseType;

	public SqlSortingProperties SqlSortingProperties
	{
		get
		{
			if (sqlSortingProperties == null)
			{
				sqlSortingProperties = NavSqlMetadataCommands.GetSqlSortingPropertiesForCollation(Collation);
			}
			return sqlSortingProperties;
		}
		set
		{
			sqlSortingProperties = value;
		}
	}

	internal CollationAwareStringComparer CollationAwareStringComparer => collationAwareStringComparer ?? (collationAwareStringComparer = new CollationAwareStringComparer(SqlSortingProperties));

	internal DatabaseSyncState DatabaseSyncState => databaseSync.DatabaseSyncState;

	internal NavSqlDatabaseSync DatabaseSync => databaseSync;

	internal NavDataUpgradeManager UpgradeManager
	{
		get
		{
			if (upgradeManager == null)
			{
				upgradeManager = new NavDataUpgradeManager(SystemTenant.UpgradeMetadata, Tenant);
			}
			return upgradeManager;
		}
	}

	internal IEqualityComparer<string> CompanyNameComparer
	{
		get
		{
			IEqualityComparer<string> equalityComparer = companyNameComparer;
			if (equalityComparer == null)
			{
				IEqualityComparer<string> equalityComparer3;
				IEqualityComparer<string> equalityComparer2;
				if (SqlSortingProperties.IsCaseInsensitive || SqlSortingProperties.IsAccentInsensitive)
				{
					equalityComparer2 = new CollationAwareStringComparer(SqlSortingProperties);
					equalityComparer3 = equalityComparer2;
				}
				else
				{
					equalityComparer2 = EqualityComparer<string>.Default;
					equalityComparer3 = equalityComparer2;
				}
				equalityComparer2 = equalityComparer3;
				companyNameComparer = equalityComparer3;
				equalityComparer = equalityComparer2;
			}
			return equalityComparer;
		}
	}

	public bool ContainsDeadlockMonitorView { get; set; }

	public int CurrentSynchronizationTaskId => databaseSync.SynchronizationTaskId;

	public event EventHandler<NavTrackingEventArgs> MetadataChanged;

	public event EventHandler<NavTrackingEventArgs> BreakpointsChanged;

	public event EventHandler<NavTrackingEventArgs> ClientAddInChanged;

	public event EventHandler<NavTrackingEventArgs> NavAppObjectMetadataChanged;

	public event EventHandler<NavTrackingEventArgs> NavAppTenantAssociationChanged;

	public event EventHandler<NavTrackingEventArgs> NavAppChanged;

	public event EventHandler<NavTrackingEventArgs> NavAppPublishReferenceChanged;

	public static event Action<NavDatabase> DatabaseFirstAccess;

	internal NavDatabase(NavTenant parent, NavDatabaseServer databaseServer, string databaseName, NavDatabaseType databaseType, ExternalSqlTableConnectionSettings externalSqlTableConnectionSettings = null)
		: base(parent)
	{
		if (databaseServer == null)
		{
			throw new ArgumentNullException("databaseServer");
		}
		if (string.IsNullOrEmpty(databaseName))
		{
			throw new ArgumentNullException("databaseName");
		}
		if (databaseType == NavDatabaseType.External && externalSqlTableConnectionSettings == null)
		{
			throw new ArgumentNullException("externalSqlTableConnectionSettings");
		}
		selfReference = new Reference<NavDatabase>(this);
		tenant = parent;
		this.databaseServer = databaseServer;
		this.databaseName = databaseName;
		normalizedName = NormalizeDatabaseName(databaseName);
		this.databaseType = databaseType;
		databaseHealth = new NavDatabaseHealth(this);
		this.externalSqlTableConnectionSettings = externalSqlTableConnectionSettings;
		databaseState = new DataCacheDatabaseState(this);
		navUsers = new NavUserCache(this);
		securityAndLicense = new NavDatabaseSecurityAndLicense(this);
		CacheSynchronization = new CacheSynchronization(this);
		PermissionSetupMonitor = new PermissionSetupMonitor(this);
		ProfileSetupMonitor = new ProfileSetupMonitor(this);
		UserTableChangeMonitor = new UserTableChangeMonitor(this);
		WebServiceSetupMonitor = new WebServiceSetupMonitor(this);
		CompanyTableChangeMonitor = new CompanyTableChangeMonitor(this);
		SequenceProvider = new NavSqlSequenceProvider(this);
		companyNames = new List<string>(5) { string.Empty };
		this.ResetCompanyIdCache();
		switch (databaseType)
		{
		case NavDatabaseType.Application:
			EntitlementSetupMonitor = new EntitlementSetupMonitor(this);
			sqlDatabaseProperties = new NavSqlDatabaseProperties(this);
			tableConnectionSettingsStorage = new TableConnectionSettingsStorage(this);
			break;
		case NavDatabaseType.Tenant:
			databaseSync = new NavSqlDatabaseSync(this);
			break;
		default:
			throw new ArgumentException("Unknown databasetype");
		case NavDatabaseType.External:
			break;
		}
	}

	public override string ToString()
	{
		return ToString(useKey: false);
	}

	public string ToString(bool useKey)
	{
		if (!useKey)
		{
			return DatabaseName;
		}
		return Key;
	}

	internal static string NormalizeDatabaseName(string databaseName)
	{
		return databaseName.ToUpperInvariant();
	}

	internal static NavSqlDatabaseLock AcquireSyncTenantDatabaseCrossProcessLock(NavSqlConnectionScope tenantDatabaseLockScope)
	{
		int timeout = NavGlobal.AppDatabase.SqlDatabaseProperties.LockTimeout * 1000;
		return NavSqlDatabaseLock.LockResource(tenantDatabaseLockScope, "SyncTenantDatabase", timeout);
	}

	internal static NavSqlDatabaseLock AcquireNavAppTenantCrossProcessLock(NavSqlConnectionScope tenantDatabaseLockScope, string tenantId)
	{
		int timeout = NavGlobal.AppDatabase.SqlDatabaseProperties.LockTimeout * 1000;
		return NavSqlDatabaseLock.LockResource(tenantDatabaseLockScope, "NAVAPP_" + tenantId, timeout);
	}

	public IReference<NavDatabase> GetReference()
	{
		return selfReference;
	}

	public bool HasCapability(SupportedCapability capability)
	{
		if (supportedCapabilitesCache.TryGetValue(capability, out var value))
		{
			return value;
		}
		value = CheckIfCapabilityIsSupported(capability);
		supportedCapabilitesCache.TryAdd(capability, value);
		return value;
	}

	internal void VerifyConnection(int attemptCount)
	{
		Exception ex = null;
		int reconnectDelayTime = DatabaseServer.SqlConnectionPool.ReconnectDelayTime;
		for (int i = 0; i < attemptCount; i++)
		{
			if (i != 0)
			{
				LogWriterHelper.LogEvent(EventLogEntryType.Information, string.Format(CultureInfo.InvariantCulture, "Will retry in {0} seconds to create a connection to database '{1}' on database server '{2}'.", TimeSpan.FromMilliseconds(reconnectDelayTime).TotalSeconds, DatabaseName, DatabaseServer.ServerName));
				Thread.Sleep(reconnectDelayTime);
			}
			try
			{
				sqlDatabaseProperties.CheckDatabaseVersion();
				return;
			}
			catch (NavCSideException ex2)
			{
				ex = ex2;
				LogWriterHelper.LogExceptionEvent(ex2, EventLogEntryType.Error);
				if (ex2.ErrorCode != 22924076 || NavEnvironment.Instance.InServerShutdown)
				{
					throw;
				}
			}
			catch (NavSqlException exception)
			{
				LogWriterHelper.LogExceptionEvent(exception, EventLogEntryType.Error);
				throw;
			}
		}
		if (ex != null)
		{
			throw ex;
		}
	}

	internal void InitializeChangeListener()
	{
		if (NavEnvironment.Instance.StandaloneInstance)
		{
			return;
		}
		lock (syncLock)
		{
			if (listener == null)
			{
				listener = new NavDatabaseChangeListener(this);
				listener.Initialize();
			}
		}
	}

	public int SynchronizeTenantDatabase(SyncMode syncMode, bool async, bool perTable = false, bool sharedSchema = false)
	{
		if (DatabaseType != NavDatabaseType.Tenant)
		{
			return 0;
		}
		using (DiagnosticsResolver.CreateTimingScope("00000RA", Category.DatabaseSync, DiagnosticParameter.SystemMetadata("Synchronizing tenant database")))
		{
			NavGlobal.AppDatabase.InitializeChangeListener();
			NavGlobal.AppDatabase.ChangeListener.ForceHandleTrackingEvent();
			if (async)
			{
				return databaseSync.StartSyncProcessIfNotAlreadyStarted(syncMode, perTable, sharedSchema);
			}
			try
			{
				EnsureDatabaseInSync(syncMode, perTable, sharedSchema);
			}
			catch (NavSqlDatabaseSyncException exception) when (syncMode == SyncMode.CheckOnly)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000925", Category.DatabaseSync, exception, Verbosity.Warning);
			}
			return 0;
		}
	}

	public int ConvertTenantDatabase(bool async, bool perTable = false)
	{
		if (DatabaseType != NavDatabaseType.Tenant)
		{
			return 0;
		}
		using (DiagnosticsResolver.CreateTimingScope("00001LB", Category.DatabaseSync, DiagnosticParameter.SystemMetadata("Converting tenant database")))
		{
			if (async)
			{
				return databaseSync.StartConvertProcessIfNotAlreadyStarted(perTable);
			}
			databaseSync.ConvertTenantDatabase(perTable);
		}
		return 0;
	}

	public int CopyTenantData(NavTenant sourceTenant, bool async = true)
	{
		if (DatabaseType != NavDatabaseType.Tenant)
		{
			throw new InvalidOperationException("Operation can only be performed on a tenant database.");
		}
		if (async)
		{
			return databaseSync.StartTenantDataCopyProcessIfNotAlreadyStarted(sourceTenant);
		}
		((ITenantSessionHandler)Tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant navTenant, NavSession systemSession) =>
		{
			databaseSync.ImportTenantTemplateData(systemSession, sourceTenant);
		}), true, false, false, (LanguageSetting)null);
		return 0;
	}

	public int RemoveTenant(bool async = true)
	{
		if (DatabaseType != NavDatabaseType.Tenant)
		{
			throw new InvalidOperationException("Operation can only be performed on a tenant database.");
		}
		if (!IsSharedTenantSchema)
		{
			throw new NavTenantRemoveException("You can only use the Remove-NAVTenant cmdlet to remove a tenant when the tenant database uses shared schema. If the tenant database does not use shared schema, you can dismount the tenant, and then delete the database manually, for example, by using SQL Server Management Studio.");
		}
		if (async)
		{
			return databaseSync.StartTenantRemoveProcessIfNotAlreadyStarted();
		}
		((ITenantSessionHandler)tenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant navTenant, NavSession systemSession) =>
		{
			databaseSync.RemoveTenant(systemSession);
		}), true, false, false, (LanguageSetting)null);
		return 0;
	}

	public int MoveTenantData(TenantDatabaseId destinationTenantDatabaseId, bool async = true, bool immediateSourceTenantRemoval = false)
	{
		if (DatabaseType != NavDatabaseType.Tenant)
		{
			throw new InvalidOperationException("Operation can only be performed on a tenant database.");
		}
		NavTenantType tenantType = TenantProperties.TenantType;
		if (tenantType == NavTenantType.SoftDeleted || tenantType == NavTenantType.Deleting || tenantType == NavTenantType.Deleted)
		{
			throw new NavTenantDataMoveException("The Move-NAVTenant cmdlet cannot move the tenant because the tenant has been deleted.");
		}
		if (async)
		{
			return databaseSync.StartTenantDataMoveProcessIfNotAlreadyStarted(destinationTenantDatabaseId.Value, immediateSourceTenantRemoval);
		}
		databaseSync.MoveTenantData(destinationTenantDatabaseId.Value, immediateSourceTenantRemoval);
		return 0;
	}

	internal bool GetIsDatabaseInSync()
	{
		try
		{
			ValidateDatabaseState();
		}
		catch (NavSqlDatabaseSyncException)
		{
		}
		return DatabaseSyncState == DatabaseSyncState.Synchronized;
	}

	internal void EnsureDatabaseInSync(SyncMode syncMode, bool perTable = false)
	{
		EnsureDatabaseInSync(syncMode, perTable, sharedSchema: false);
	}

	internal void ValidateDatabaseState()
	{
		EnsureDatabaseInSync(SyncMode.CheckOnly, perTable: false, sharedSchema: false);
	}

	private void EnsureDatabaseInSync(SyncMode syncMode, bool perTable, bool sharedSchema)
	{
		if (DatabaseType == NavDatabaseType.Tenant)
		{
			NavGlobal.AppDatabase.InitializeChangeListener();
			databaseSync.EnsureDatabaseInSync(syncMode, perTable, sharedSchema);
			tenant.ProcessTenantActions();
		}
	}

	public void EnsurePendingTableChangesAreCurrent()
	{
		DatabaseSync.EnsurePendingTableChangesAreCurrent();
	}

	public SyncNavAppResponse SynchronizeTenantDatabaseForApp(AppSyncContext appSyncContext)
	{
		if (DatabaseType != NavDatabaseType.Tenant)
		{
			return new SyncNavAppResponse(SyncNavAppStatus.MustBeTenantDatabase, Lang.NavAppSyncCannotSyncApplicationDatabase);
		}
		return databaseSync.SynchronizeTenantDatabaseForAppSync(appSyncContext);
	}

	public void OverwriteTenantId(bool ensureDatabaseExclusiveState = true)
	{
		if (DatabaseType == NavDatabaseType.Tenant)
		{
			databaseSync.OverwriteTenantId(ensureDatabaseExclusiveState);
		}
	}

	public void CheckTablesInSync(IEnumerable<NCLMetaTable> tablesThatMustBeInSync)
	{
		if (databaseType != NavDatabaseType.Tenant)
		{
			return;
		}
		foreach (NCLMetaTable item in tablesThatMustBeInSync)
		{
			CheckTableInSync(item);
		}
	}

	public void CheckTableInSync(NCLMetaTable tableThatMustBeInSync)
	{
		if (databaseType == NavDatabaseType.Tenant)
		{
			databaseSync.CheckTableInSync(tableThatMustBeInSync.TableId);
		}
	}

	private bool CheckIfCapabilityIsSupported(SupportedCapability capability)
	{
		switch (capability)
		{
		case SupportedCapability.CreateOnlineIndex:
			if (IsAzureDatabase)
			{
				return true;
			}
			if (SqlServerProperties.EngineEdition != SqlServerEngineEdition.Enterprise)
			{
				return false;
			}
			if (SqlServerProperties.MajorProductVersion < 10)
			{
				return false;
			}
			return true;
		case SupportedCapability.TablePartition:
			if (IsAzureDatabase)
			{
				return true;
			}
			if (SqlServerProperties.EngineEdition == SqlServerEngineEdition.Enterprise)
			{
				return SqlServerProperties.MajorProductVersion >= 9;
			}
			if (SqlServerProperties.EngineEdition == SqlServerEngineEdition.Standard)
			{
				if ((SqlServerProperties.MajorProductVersion != 13 || SqlServerProperties.MinorProductVersion != 0 || SqlServerProperties.BuildProductVersion < 4001) && (SqlServerProperties.MajorProductVersion != 13 || SqlServerProperties.MinorProductVersion <= 0))
				{
					return SqlServerProperties.MajorProductVersion >= 14;
				}
				return true;
			}
			return false;
		default:
			throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, "{0} isn't a valid SupportedCapability enum value ", capability));
		}
	}

	protected override void Dispose(bool disposing)
	{
		lock (syncLock)
		{
			try
			{
				if (disposing && !IsDisposed)
				{
					Tree.DisposeAllChildren();
					if (listener != null)
					{
						listener.Dispose();
					}
					if (PermissionSetupMonitor != null)
					{
						PermissionSetupMonitor.Dispose();
						PermissionSetupMonitor = null;
					}
					if (ProfileSetupMonitor != null)
					{
						ProfileSetupMonitor.Dispose();
						ProfileSetupMonitor = null;
					}
					if (UserTableChangeMonitor != null)
					{
						UserTableChangeMonitor.Dispose();
						UserTableChangeMonitor = null;
					}
					if (WebServiceSetupMonitor != null)
					{
						WebServiceSetupMonitor.Dispose();
						WebServiceSetupMonitor = null;
					}
					if (CompanyTableChangeMonitor != null)
					{
						CompanyTableChangeMonitor.Dispose();
						CompanyTableChangeMonitor = null;
					}
					if (securityAndLicense != null)
					{
						securityAndLicense.Dispose();
					}
					NavEnvironment.Instance.LRUCache.RemoveAllFromOrigin(this);
				}
			}
			finally
			{
				selfReference?.ClearTarget();
				base.Dispose(disposing);
			}
		}
	}

	internal void HandleTrackingEvent(List<NavTrackingObject> trackingObjects)
	{
		NavSession session = NavCurrentThread.Session;
		try
		{
			NavCurrentThread.Session = null;
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00006XV", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Handling tracking event -- Metadata:{0}; Breakpoint:{1}; AddIn:{2}; NavApp:{3}; NavAppObjectMetadata:{4}; TenantAssociation:{5}; PublishReference:{6}", trackingObjects.Count((NavTrackingObject to) => to.ChangeType == ObjectTrackingChangeType.Metadata), trackingObjects.Count((NavTrackingObject to) => to.ChangeType == ObjectTrackingChangeType.Breakpoints), trackingObjects.Count((NavTrackingObject to) => to.ChangeType == ObjectTrackingChangeType.ClientAddIn), trackingObjects.Count((NavTrackingObject to) => to.ChangeType == ObjectTrackingChangeType.NavApp), trackingObjects.Count((NavTrackingObject to) => to.ChangeType == ObjectTrackingChangeType.NavAppMetadata), trackingObjects.Count((NavTrackingObject to) => to.ChangeType == ObjectTrackingChangeType.NavAppTenant), trackingObjects.Count((NavTrackingObject to) => to.ChangeType == ObjectTrackingChangeType.NavAppPublishReference));
			RaiseTypeSpecificTrackingEvent(MetadataChanged, trackingObjects, ObjectTrackingChangeType.Metadata);
			RaiseSystemCodeunitChange(trackingObjects);
			RaiseTypeSpecificTrackingEvent(BreakpointsChanged, trackingObjects, ObjectTrackingChangeType.Breakpoints);
			RaiseTypeSpecificTrackingEvent(ClientAddInChanged, trackingObjects, ObjectTrackingChangeType.ClientAddIn);
			RaiseTypeSpecificTrackingEvent(NavAppObjectMetadataChanged, trackingObjects, ObjectTrackingChangeType.NavAppMetadata);
			RaiseTypeSpecificTrackingEvent(NavAppTenantAssociationChanged, trackingObjects, ObjectTrackingChangeType.NavAppTenant);
			RaiseTypeSpecificTrackingEvent(NavAppChanged, trackingObjects, ObjectTrackingChangeType.NavApp);
			RaiseTypeSpecificTrackingEvent(NavAppPublishReferenceChanged, trackingObjects, ObjectTrackingChangeType.NavAppPublishReference);
		}
		finally
		{
			NavCurrentThread.Session = session;
		}
	}

	private void RaiseSystemCodeunitChange(List<NavTrackingObject> trackingObjects)
	{
		if (trackingObjects.Any((NavTrackingObject to) => to.ChangeType == ObjectTrackingChangeType.Metadata && to.ObjectId >= 2000000001 && to.ObjectType == ObjectType.CodeUnit))
		{
			CleanupApplicationInformation();
		}
	}

	private void RaiseTypeSpecificTrackingEvent(EventHandler<NavTrackingEventArgs> eventHandler, List<NavTrackingObject> trackingObjects, ObjectTrackingChangeType changeType)
	{
		if (eventHandler == null)
		{
			return;
		}
		IReadOnlyList<NavTrackingObject> readOnlyList = trackingObjects.FindAll((NavTrackingObject to) => to.ChangeType == changeType);
		if (readOnlyList.Count > 0)
		{
			readOnlyList.ForEach((NavTrackingObject item) =>
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000QU", Category.Runtime, Verbosity.Normal, "Tracking object -- MaxChangeTrackingNumberObjectMetadata:{0}; ChangeType:{1}; Operation:{2}; ObjType:{3}; ObjId:{4}; PackageId:{5}; Tenant:{6}", DiagnosticParameter.SystemMetadata(item.MaxChangeTrackingNumberObjectMetadata), DiagnosticParameter.SystemMetadata(item.ChangeType), DiagnosticParameter.SystemMetadata(item.ChangeOperation), DiagnosticParameter.SystemMetadata(item.ApplicationObjectId.ObjectType), DiagnosticParameter.SystemMetadata(item.ApplicationObjectId.ObjectNumber), DiagnosticParameter.SystemMetadata(item.AppPackageId), DiagnosticParameter.Oii(item.TenantId));
			});
			eventHandler(this, new NavTrackingEventArgs(readOnlyList));
		}
	}

	internal int GetCompanyNameToken(string companyName)
	{
		if (DatabaseType != NavDatabaseType.Tenant && !string.IsNullOrEmpty(companyName))
		{
			throw new InvalidOperationException("Only tenant databases have companies.");
		}
		int token;
		lock (companyNames)
		{
			if (!TryGetCompanyNameToken(companyName, out token))
			{
				token = (string.IsNullOrEmpty(companyName) ? companyNames.IndexOf(companyName) : companyNames.IndexOf(companyName, CompanyNameComparer));
				if (token < 0)
				{
					token = companyNames.Count;
					string collationAwareCompanyName = GetCollationAwareCompanyName(companyName);
					companyNames.Add(collationAwareCompanyName);
				}
			}
		}
		return token;
	}

	internal bool TryGetCompanyNameToken(string companyName, out int token)
	{
		if (string.IsNullOrEmpty(companyName))
		{
			return (token = companyNames.IndexOf(companyName)) >= 0;
		}
		return (token = companyNames.IndexOf(companyName, CompanyNameComparer)) >= 0;
	}

	internal string GetCompanyNameFromToken(int token)
	{
		if (!TryGetCompanyNameFromToken(token, out var companyName))
		{
			throw new ArgumentException("Invalid company token");
		}
		return companyName;
	}

	internal bool TryGetCompanyNameFromToken(int token, out string companyName)
	{
		companyName = null;
		if (token >= companyNames.Count)
		{
			return false;
		}
		if (token == int.MinValue)
		{
			companyName = "NO COMPANY";
			return true;
		}
		companyName = companyNames[token];
		return true;
	}

	public IEnumerable<string> GetCompanyNames()
	{
		return (from x in CompanyDataQuery()
			select x[0].ToString()).ToArray();
	}

	public IEnumerable<Guid> CompanyIds()
	{
		return from x in CompanyIdentifiers()
			select x.ODataId;
	}

	public IEnumerable<CompanyExternalIdentifier> CompanyIdentifiers()
	{
		return from record in CompanyIdentifierQuery()
			select new CompanyExternalIdentifier
			{
				Name = record[0].ToString(),
				ODataId = Guid.Parse(record[1].ToString())
			};
	}

	internal void DeleteCompanyData(int companyToken, bool commitIndividualDeleteOperations)
	{
		if (!IsSharedTenantSchema)
		{
			DatabaseSync.DeleteAllTablesInCompany(companyToken, commitIndividualDeleteOperations);
			return;
		}
		DatabaseSync.DeleteAllCompanyData(companyToken, commitIndividualDeleteOperations);
		DatabaseSync.DeleteAllSequencesInCompany(companyToken);
	}

	private IEnumerable<ReadOnlyRecordBuffer> CompanyDataQuery()
	{
		return NavSqlQueryCommand.QueryWithSingleScope(allCompaniesQuery.Value, this, 0, 0, 0, null, null, includeCurrent: false, FindType.All, null, SecurityFiltering.Ignored, addExplicitJoinHints: false);
	}

	private IEnumerable<ReadOnlyRecordBuffer> CompanyIdentifierQuery()
	{
		return NavSqlQueryCommand.QueryWithSingleScope(allCompanyIdentifierQuery.Value, this, 0, 0, 0, null, null, includeCurrent: false, FindType.All, null, SecurityFiltering.Ignored, addExplicitJoinHints: false);
	}

	public int[] GetCompanyTokens()
	{
		NavAppInstallationContext appInstallationContext = NavCurrentThread.AppInstallationContext;
		if (appInstallationContext != null)
		{
			Func<int[]> command = InternalGetCompanyTokens;
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] array = new object[4] { appInstallationContext.IsInstalling, null, null, null };
			NavAppRuntimeMetadata appBeingTargeted = appInstallationContext.AppBeingTargeted;
			array[1] = ((appBeingTargeted != null) ? appBeingTargeted.Name : null);
			NavAppRuntimeMetadata appBeingTargeted2 = appInstallationContext.AppBeingTargeted;
			array[2] = ((appBeingTargeted2 != null) ? appBeingTargeted2.Publisher : null);
			NavAppRuntimeMetadata appBeingTargeted3 = appInstallationContext.AppBeingTargeted;
			array[3] = ((appBeingTargeted3 != null) ? appBeingTargeted3.Version : null);
			return ExecuteWithDbLockTimeoutRetryLoop(command, string.Format(invariantCulture, "Extensions:: IsInstalling:{0}; Name:{1}; Publisher:{2}; Version:{3}", array), "GetCompanyTokens");
		}
		return InternalGetCompanyTokens();
	}

	private int[] InternalGetCompanyTokens()
	{
		using (NavSqlConnectionScope.Create(this, TransactionType.Update))
		{
			return (from record in NavSqlQueryCommand.QueryWithSingleScope(allCompaniesQuerySharedLock.Value, this, 0, 0, 0, null, null, includeCurrent: false, FindType.All, null, SecurityFiltering.Ignored, addExplicitJoinHints: false).ToList()
				select GetCompanyNameToken(((NavText)record[0]).Value)).ToArray();
		}
	}

	private T ExecuteWithDbLockTimeoutRetryLoop<T>(Func<T> command, string additionalInfo = "", [CallerMemberName] string operationName = "", int retryCount = 4)
	{
		T result = default;
		bool flag = false;
		do
		{
			try
			{
				result = command();
				flag = true;
				retryCount = 0;
			}
			catch (NavCSideException ex)
			{
				retryCount = ((ex.ErrorCode == 22926090) ? (retryCount - 1) : 0);
				if (retryCount == 0)
				{
					throw;
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000HY", Category.Sql, Verbosity.Warning, "{0}::Retry -- Database Name:{1}; Tenant:{2}; Additional Info {3};", DiagnosticParameter.SystemMetadata(operationName), DiagnosticParameter.Oii(DatabaseName), DiagnosticParameter.Oii(tenant?.Id), DiagnosticParameter.SystemMetadata(additionalInfo));
			}
		}
		while (retryCount > 0);
		if (!flag)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Database retry operation {0} failed unexpectedly", operationName));
		}
		return result;
	}

	internal void OnFirstDatabaseAccess()
	{
		if (!firstAccessCalled)
		{
			CheckPermissions();
			DatabaseFirstAccess?.Invoke(this);
			firstAccessCalled = true;
		}
	}

	private void CheckPermissions()
	{
		string statement = (IsAzureDatabase ? "SELECT HAS_PERMS_BY_NAME(null, 'DATABASE', 'VIEW DATABASE STATE'), 1" : "SELECT HAS_PERMS_BY_NAME(null, null, 'VIEW SERVER STATE'), HAS_PERMS_BY_NAME(null, null, 'ALTER ANY EVENT SESSION')");
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(this);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		using (navSqlCommand.ExecuteReader(statement))
		{
			if (navSqlCommand.ReadNextRow())
			{
				object value = navSqlCommand.Reader.GetValue(0);
				HasViewStatePermission = value != null && (int)value == 1;
				value = navSqlCommand.Reader.GetValue(1);
				HasAlterAnyEventSessionPermission = value != null && (int)value == 1;
			}
			navSqlCommand.CloseReader(forceClose: false);
		}
	}

	public void UpdateApplicationInformation()
	{
		if (NavEnvironment.Instance.StandaloneInstance)
		{
			return;
		}
		if (databaseType != NavDatabaseType.Application)
		{
			throw new NotSupportedException("This should only be used on the application database");
		}
		lock (appInformationSyncLock)
		{
			ITenantSessionHandler tenantSessionHandler = Tenant;
			tenantSessionHandler.RunTenantActionInSystemSession((NavTenant systemTenant, NavSession session) =>
			{
				try
				{
					ApplicationVersion = session.SystemCodeunitFactory.VersionTriggers.InvokeApplicationVersion();
					ApplicationBuild = session.SystemCodeunitFactory.VersionTriggers.InvokeApplicationBuild();
					Tenant.Diagnostics.SendTraceTag("0000177", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, FormattableString.Invariant($"ApplicationVersion: {NavGlobal.AppDatabase.ApplicationVersion}, ApplicationBuild: {NavGlobal.AppDatabase.ApplicationBuild}"));
				}
				catch (Exception ex)
				{
					Tenant.Diagnostics.SendTraceTag("00001CF", Category.Runtime, Verbosity.Normal, "Update of application information has failed. Exception message: {0}, Stack trace: {1}", DiagnosticParameter.SystemMetadata(ex.Message), DiagnosticParameter.SystemMetadata(ex.StackTrace));
					throw;
				}
			});
		}
	}

	internal NavSqlTenantDatabaseProperties GetTenantDatabasePropertiesIfLoaded()
	{
		if (tenantDatabaseProperties == null)
		{
			return null;
		}
		return tenantDatabaseProperties;
	}

	private void CleanupApplicationInformation()
	{
		applicationBuild = null;
		applicationVersion = null;
	}

	private string GetCollationAwareCompanyName(string companyName)
	{
		if (!SqlSortingProperties.IsCaseInsensitive && !SqlSortingProperties.IsAccentInsensitive)
		{
			return companyName;
		}
		using (NavSqlConnectionScope.Create(this, TransactionType.Update))
		{
			return (from record in NavSqlQueryCommand.QueryWithSingleScope(allCompaniesQuerySharedLock.Value, this, 0, 0, 0, null, null, includeCurrent: false, FindType.All, null, SecurityFiltering.Ignored, addExplicitJoinHints: false)
				select ((NavText)record[0]).Value).FirstOrDefault((string tempCompanyName) => CompanyNameComparer.Equals(companyName, tempCompanyName)) ?? companyName;
		}
	}

	private static NCLMetaQueryDefinition CreateCompanyTableQuery(DataLockState dataLockState)
	{
		return NCLMetaQueryDefinition.Create(0, (NCLMetaQueryDefinition queryBeingCreated) => NCLMetaQueryDataItemCollection.Create(queryBeingCreated, 2000000006, dataLockState, "Company", (NCLMetaQueryDataItem dataItemBeingCreated) => new FieldList(NCLMetaQueryColumn.Create(dataItemBeingCreated, 1, "Name", 1, filterOnly: false))), null);
	}

	private static NCLMetaQueryDefinition CreateCompanyTableIdentifierQuery(DataLockState dataLockState)
	{
		return NCLMetaQueryDefinition.Create(0, (NCLMetaQueryDefinition queryBeingCreated) => NCLMetaQueryDataItemCollection.Create(queryBeingCreated, 2000000006, dataLockState, "Company", (NCLMetaQueryDataItem dataItemBeingCreated) => new FieldList(NCLMetaQueryColumn.Create(dataItemBeingCreated, 1, "Name", 1, filterOnly: false), NCLMetaQueryColumn.Create(dataItemBeingCreated, 8000, "Id", 2, filterOnly: false))), null);
	}
}
