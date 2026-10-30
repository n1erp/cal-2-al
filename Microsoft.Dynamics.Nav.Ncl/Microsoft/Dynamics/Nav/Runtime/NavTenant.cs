using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService.Model.Authentication;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Constants;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTenant : ITreeObject, IDisposable, ITenantSessionHandler
{
	internal static readonly TimeSpan AzureTelemetryEmissionPeriod = TimeSpan.FromHours(24.0);

	private const string AbsoluteCacheKeyPrefix = "Absolute-";

	private static readonly Lazy<NCLMetaQueryDefinition> rolesInAccessControlQuery = new Lazy<NCLMetaQueryDefinition>(() => NCLMetaQueryDefinition.Create(0, (NCLMetaQueryDefinition queryBeingCreated) => NCLMetaQueryDataItemCollection.Create(queryBeingCreated, NavGlobal.NCLMetadata.GetMetaTableById(2000000053), DataLockState.None, "AccessControl", (NCLMetaQueryDataItem dataItem) => new FieldList(NCLMetaQueryColumn.Create(dataItem, dataItem.MetaTable.GetFieldByNo(8), filterOnly: false), NCLMetaQueryColumn.Create(dataItem, dataItem.MetaTable.GetFieldByNo(9), filterOnly: false), NCLMetaQueryColumn.Create(dataItem, dataItem.MetaTable.GetFieldByNo(2), filterOnly: false)), null), null));

	private static readonly Lazy<NCLMetaQueryDefinition> rolesInPermissionSetQuery = new Lazy<NCLMetaQueryDefinition>(() => NCLMetaQueryDefinition.Create(0, (NCLMetaQueryDefinition queryBeingCreated) => NCLMetaQueryDataItemCollection.Create(queryBeingCreated, NavGlobal.NCLMetadata.GetMetaTableById(2000000004), DataLockState.None, "PermissionSet", (NCLMetaQueryDataItem dataItem) => new FieldList(NCLMetaQueryColumn.Create(dataItem, dataItem.MetaTable.GetFieldByNo(1), filterOnly: false)), null), null));

	private readonly NavTenantSettings tenantSettings;

	private NavAppGroup databaseDefinedAppGroup;

	private readonly object extensionInstallUninstallLockObject = new object();

	private bool onFirstDatabaseAccessMethodHasBeenRun;

	private MemoryCache tenantCache;

	private OfficeEntitlementStatusCache officeEtitlementCache;

	private string id;

	private bool? isIntelligentCloudReplicationEnabled;

	private readonly object isIntelligentCloudReplicationEnabledLockObj = new object();

	private Lazy<NavDatabase> database;

	private readonly NavDiagnostics diagnostics;

	private IAuthenticationHandler authenticationHandler;

	private ISecureAuthenticationHandler secureAuthenticationHandler;

	private readonly NavUserAuthenticationCache authenticationCache;

	private readonly NavLicenseValidator licenseValidator;

	private long lastSessionEventPurgeTime;

	private long lastAzureTelemetryReportTime = long.MinValue;

	private readonly object dataInitializationObj = new object();

	private readonly Queue<Action<NavTenant>> tenantActions = new Queue<Action<NavTenant>>();

	private NavApplicationServer nas;

	private FederationMetadataProvider federationMetadataProvider;

	private readonly object stateSyncRoot = new object();

	internal AzureLicenseUsageService azureLicenseUsageService = AzureLicenseUsageService.Instance;

	private readonly ManualResetGate disposingGate = new ManualResetGate();

	private bool disposed;

	private readonly ManualResetEvent waitForTenantToBeInitialized = new ManualResetEvent(initialState: true);

	internal ConcurrentDictionary<Guid, object> ActiveManagementTasks { get; } = new ConcurrentDictionary<Guid, object>();

	internal ConcurrentDictionary<string, int> ConcurrentDeviceUsers { get; } = new ConcurrentDictionary<string, int>();

	public TreeHandler Tree { get; private set; }

	public bool IsIntelligentCloudReplicationEnabled
	{
		get
		{
			if (!isIntelligentCloudReplicationEnabled.HasValue)
			{
				lock (isIntelligentCloudReplicationEnabledLockObj)
				{
					if (!isIntelligentCloudReplicationEnabled.HasValue)
					{
						isIntelligentCloudReplicationEnabled = !database.Value.IsSharedTenantSchema && new NavSqlIntelligentCloud(database.Value).ReadIsIntelligentCloudEnabledFromDatabase() == true;
					}
				}
			}
			return isIntelligentCloudReplicationEnabled.Value;
		}
		set
		{
			isIntelligentCloudReplicationEnabled = value;
		}
	}

	public NavUserAuthenticationCache AuthenticationCache => authenticationCache;

	public NavDiagnostics Diagnostics => diagnostics;

	public IAuthenticationHandler AuthenticationHandler
	{
		get
		{
			if (authenticationHandler == null)
			{
				authenticationHandler = NavAuthenticationHandlerFactory.CreateHandler(diagnostics);
			}
			return authenticationHandler;
		}
	}

	public ISecureAuthenticationHandler SecureAuthenticationHandler
	{
		get
		{
			if (secureAuthenticationHandler == null)
			{
				secureAuthenticationHandler = NavAuthenticationHandlerFactory.CreateSecureHandler(diagnostics, AuthenticationHandler);
			}
			return secureAuthenticationHandler;
		}
	}

	public CloudReadyReplicationStatusTable CloudReadyReplicationStatusCache { get; internal set; }

	public NavLicenseValidator LicenseValidator => licenseValidator;

	public virtual NavTenantType TenantType => NavTenantType.Normal;

	public virtual string Id => id ?? (id = tenantSettings.Id);

	public bool IsInExclusiveAccessMode { get; private set; }

	internal bool IsSystemTenant => Id.Equals("nav-systemapplication", StringComparison.OrdinalIgnoreCase);

	internal virtual TenantLicenseState LicenseState
	{
		get
		{
			using (new MaximizedPermissionScope(NavCurrentThread.Session, new int[1] { 2000000189 }))
			{
				using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000189, SecurityFiltering.Ignored);
				if (!navRecord.ALFindLast(DataError.TrapError))
				{
					return TenantLicenseState.Evaluation;
				}
				return (TenantLicenseState)navRecord.GetFieldValue(3).ToInt32();
			}
		}
	}

	public NavAppGroup DatabaseDefinedNavAppGroup => databaseDefinedAppGroup;

	public NavAppGroup NavAppGroup => databaseDefinedAppGroup;

	public object ExtensionInstallUninstallLockObject => extensionInstallUninstallLockObject;

	public bool IsTenantDismounting => !disposingGate.IsOpen;

	public bool FailedToMount
	{
		get
		{
			if (State != NavTenantState.Dismounted && State != NavTenantState.Failed)
			{
				return IsDisposed;
			}
			return true;
		}
	}

	public NavDatabase Database
	{
		get
		{
			if (disposed)
			{
				throw new ObjectDisposedException("NavTenant");
			}
			if (IsDisposed)
			{
				throw new ObjectDisposedException("NavTenant");
			}
			if (database == null)
			{
				throw new NavNCLArgumentException("NavDatabase");
			}
			NavDatabase value = database.Value;
			if (value != null && value.IsDisposed)
			{
				throw new ObjectDisposedException("NavDatabase");
			}
			return database.Value;
		}
	}

	public bool IsDatabaseInitialized => database.IsValueCreated;

	protected NavDatabaseServer DatabaseServer { get; }

	public NavTenantSettings TenantSettings => tenantSettings;

	public NavSystemTenant SystemTenant => NavEnvironment.Instance.Tenants.SystemTenant;

	public IEnumerable<NavSession> ActiveSessions => Tree.ChildrenOfTypeWhere((NavSession session) => session.IsOpen);

	public IEnumerable<NavSession> AllSessions => Tree.Children.OfType<NavSession>();

	public FederationMetadataProvider WSFederationMetadataProvider
	{
		get
		{
			if (federationMetadataProvider == null)
			{
				federationMetadataProvider = new FederationMetadataProvider(tenantSettings.GetWSFederationMetadataLocation(), Diagnostics);
			}
			return federationMetadataProvider;
		}
	}

	public bool IsDisposed
	{
		get
		{
			if (Tree != null && !Tree.IsDisposed)
			{
				return disposed;
			}
			return true;
		}
	}

	private NavTenantState TenantStateBasedOnPendingChanges => Database.DatabaseSync.AnyPendingTableChanges switch
	{
		PendingTableChangesState.PendingChanges => NavTenantState.OperationalWithSyncPending, 
		PendingTableChangesState.NoChanges => NavTenantState.Operational, 
		_ => NavTenantState.Mounted, 
	};

	public NavTenantState State { get; set; }

	public NavTenantDatabaseSyncStatus DetailedState { get; set; }

	internal OfficeEntitlementStatusCache OfficeEntitlementCache
	{
		get
		{
			if (officeEtitlementCache == null)
			{
				officeEtitlementCache = new OfficeEntitlementStatusCache();
			}
			return officeEtitlementCache;
		}
	}

	public string FixedTenantWebServiceEndpointBaseUri
	{
		get
		{
			Uri value = ServerUserSettings.Instance.FixedTenantWebServiceEndpointBaseUri.Value;
			if (value != null)
			{
				if (!string.IsNullOrWhiteSpace(TenantSettings.UrlName) && !TenantSettings.UrlName.Equals("production", StringComparison.OrdinalIgnoreCase) && !TenantSettings.UrlName.Equals("sandbox", StringComparison.OrdinalIgnoreCase))
				{
					return value.OriginalString.Replace("/v1.0", "/v2.0");
				}
				return value.OriginalString;
			}
			return null;
		}
	}

	protected NavTenant()
	{
		Tree = TreeHandler.CreateTreeRoot(this);
		lastSessionEventPurgeTime = DateTime.UtcNow.Ticks;
		State = NavTenantState.Mounted;
		DetailedState = null;
	}

	public NavTenant(NavTenantSettings tenantSettings, NavDatabaseServer databaseServer, NavTenantState initialState = NavTenantState.Mounted, bool isInExclusiveAccessMode = false)
	{
		if (tenantSettings == null)
		{
			throw new ArgumentNullException("tenantSettings");
		}
		if (!TenantId.IsValid(tenantSettings.Id))
		{
			throw new NavTenantInvalidIdException(Lang.TenantIdInvalid);
		}
		if (!TenantDatabaseId.IsValid(tenantSettings.TenantDatabaseId))
		{
			throw new NavTenantInvalidIdException(Lang.TenantDatabaseIdInvalid);
		}
		if (FeatureSwitches.Instance.EnableWaitOnInitializeTenant)
		{
			waitForTenantToBeInitialized = new ManualResetEvent(initialState != NavTenantState.Mounting);
		}
		Tree = TreeHandler.CreateTreeRoot(this);
		id = ConvertTenantIdToExpectedCase(tenantSettings.Id);
		diagnostics = NavDiagnostics.CreateSectionDiagnostics(NavDiagnostics.GlobalInstance, id, MonitorType.None, singleThreadMonitorAccess: false, tenantSettings.ConcatenatedAlternateId, tenantSettings.AadTenantId, null);
		diagnostics.SendTraceTag("000003X", Category.Tenant, Verbosity.Normal, "NavTenant Created, ID={0}", DiagnosticParameter.Oii(tenantSettings.Id));
		IsInExclusiveAccessMode = isInExclusiveAccessMode;
		licenseValidator = new NavLicenseValidator();
		DatabaseServer = databaseServer;
		database = new Lazy<NavDatabase>(CreateDatabase, LazyThreadSafetyMode.ExecutionAndPublication);
		this.tenantSettings = new NavTenantSettings(tenantSettings);
		this.tenantSettings.Id = id;
		if (this.tenantSettings.DefaultTimeZone == null)
		{
			this.tenantSettings.DefaultTimeZone = ServerUserSettings.Instance.ServicesDefaultTimeZone.Value;
		}
		this.tenantSettings.MakeImmutable();
		databaseDefinedAppGroup = NavAppGroup.BaseGroup;
		lastSessionEventPurgeTime = DateTime.UtcNow.Ticks;
		State = initialState;
		DetailedState = null;
		authenticationCache = new NavUserAuthenticationCache(this);
		CloudReadyReplicationStatusCache = new CloudReadyReplicationStatusTable(this);
	}

	public void MoveTenantToExclusiveAccessMode()
	{
		lock (stateSyncRoot)
		{
			if (IsInExclusiveAccessMode)
			{
				throw new NavTenantInvalidStateException(string.Format(CultureInfo.InvariantCulture, "Tenant '{0}' is already in exclusive access mode.", Id));
			}
			IsInExclusiveAccessMode = true;
		}
	}

	public void RestoreStateFromExclusiveAccessMode()
	{
		IsInExclusiveAccessMode = false;
	}

	public NavTenantRuntimeSettings GetRuntimeTenantSettings(bool useTenantDatabaseToCalculateState = true, bool refreshState = true)
	{
		if (refreshState)
		{
			RefreshState(useTenantDatabaseToCalculateState);
		}
		NavTenantRuntimeSettings navTenantRuntimeSettings = new NavTenantRuntimeSettings(tenantSettings)
		{
			State = State,
			DetailedState = DetailedState,
			IsInExclusiveAccessMode = IsInExclusiveAccessMode
		};
		if (database.IsValueCreated)
		{
			NavDatabase value = database.Value;
			if (value != null && value.Health.IsHealthy)
			{
				switch (Database.TenantProperties.TenantType)
				{
				case NavTenantType.SoftDeleted:
					navTenantRuntimeSettings.DeletionState = "Soft deleted";
					break;
				case NavTenantType.Deleting:
					navTenantRuntimeSettings.DeletionState = "Deleting";
					break;
				case NavTenantType.Deleted:
					navTenantRuntimeSettings.DeletionState = "Deleted";
					break;
				default:
					navTenantRuntimeSettings.DeletionState = "Not deleted";
					break;
				}
				navTenantRuntimeSettings.TenantDataVersion = ((database.Value.TenantProperties.TenantDataVersion == NavSqlTenantProperties.UninitializedVersion.ToString()) ? "Uninitialized" : database.Value.TenantProperties.TenantDataVersion);
			}
		}
		return navTenantRuntimeSettings;
	}

	public void UpdateDatabaseDefinedAppGroups(NavAppGroup newGroup)
	{
		databaseDefinedAppGroup = newGroup;
		string content = string.Join(" | ", newGroup.OrderedAppMetadata.Select((NavAppRuntimeMetadata m) => m.TechnicalDisplayName));
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000007J", Category.Extensions, Verbosity.Normal, "Assigning extension group -- Tenant:{0}; AllGroup:{1}; AllStableCombinationId:{2}; AllExtensions:{3}", DiagnosticParameter.Oii(Id), DiagnosticParameter.SystemMetadata(newGroup.GroupId), DiagnosticParameter.SystemMetadata(newGroup.StableCombinationId), DiagnosticParameter.SystemMetadata(content));
	}

	NavSession ITenantSessionHandler.CreateSystemSession()
	{
		return new NavSession(this, TenantSettings.DefaultTimeZone, NavUserAuthentication.CreateSystemAuthentication(this), ExecutionMode.Standard, null, null, isSystemSession: true, -1, null, ConnectionType.Background);
	}

	NavSession ITenantSessionHandler.CreateTaskSchedulerSession(Guid userGuid, string userName, TimeZoneInfo timeZone, string navAppId)
	{
		return new NavSession(this, timeZone, NavUserAuthentication.CreateTaskSchedulerAuthentication(this, userGuid, userName, navAppId), ExecutionMode.Standard, null, null, isSystemSession: false, -1, navAppId, ConnectionType.Background);
	}

	public NavSession GetSessionById(int sessionId)
	{
		return Tree.ChildOfTypeFirstOrDefault((NavSession s) => s.Id == sessionId);
	}

	void ITenantSessionHandler.RunTenantActionInSystemSession(Action<NavTenant, NavSession> action, bool throwExceptions, bool useCurrentCulture, bool allowAppsDisabledMode, LanguageSetting language)
	{
		try
		{
			NavSession session = NavCurrentThread.Session;
			try
			{
				using NavSession navSession = ((ITenantSessionHandler)this).CreateSystemSession();
				navSession.AccessLock.BeginExclusiveExecution();
				navSession.Open(useUserPersonalization: false, null, allowAppsDisabledMode);
				if ((session != null) & useCurrentCulture)
				{
					navSession.SetSessionLanguage(language?.LCID ?? session.LCID, NavEnvironment.DefaultLanguage, session.WindowsCulture.LCID);
				}
				navSession.BeginTransaction();
				try
				{
					action(this, navSession);
				}
				catch (Exception)
				{
					navSession.EndTransaction(commit: false);
					throw;
				}
				navSession.EndTransaction(commit: true);
			}
			finally
			{
				NavCurrentThread.Session = session;
			}
		}
		catch (NavBaseException exception) when (!throwExceptions)
		{
			DiagnosticsResolver.GetMostSpecificInstance(this).SendExceptionTag("00001CE", Category.Runtime, exception, Verbosity.Warning);
		}
	}

	void ITenantSessionHandler.RunTenantActionInSystemSessionWithExclusiveAccess(Action<NavTenant, NavSession, ExclusiveTenantActionScope> action)
	{
		NavSession session = NavCurrentThread.Session;
		try
		{
			using NavSession navSession = ((ITenantSessionHandler)this).CreateSystemSession();
			navSession.AccessLock.BeginExclusiveExecution();
			navSession.Open(useUserPersonalization: false);
			using ExclusiveTenantActionScope arg = navSession.CreateExclusiveTenantActionScope();
			navSession.BeginTransaction();
			try
			{
				action(this, navSession, arg);
			}
			catch (Exception)
			{
				navSession.EndTransaction(commit: false);
				throw;
			}
			navSession.EndTransaction(commit: true);
		}
		finally
		{
			NavCurrentThread.Session = session;
		}
	}

	void ITenantSessionHandler.RunTenantActionInSystemSessionWithoutTransaction(Action<NavTenant, NavSession> action, bool throwExceptions, bool useCurrentCulture, LanguageSetting language)
	{
		try
		{
			NavSession session = NavCurrentThread.Session;
			try
			{
				using NavSession navSession = ((ITenantSessionHandler)this).CreateSystemSession();
				navSession.AccessLock.BeginExclusiveExecution();
				navSession.Open(useUserPersonalization: false);
				if ((session != null) & useCurrentCulture)
				{
					navSession.SetSessionLanguage(language?.LCID ?? session.LCID, NavEnvironment.DefaultLanguage, session.WindowsCulture.LCID);
				}
				action(this, navSession);
			}
			finally
			{
				NavCurrentThread.Session = session;
			}
		}
		catch (NavBaseException exception) when (!throwExceptions)
		{
			LogWriterHelper.LogExceptionEvent(exception, EventLogEntryType.Warning);
		}
	}

	public virtual string GetEncryptionKeyFileName()
	{
		return Database.TenantProperties.GetEncryptionKeyFileName();
	}

	public virtual void SetEncryptionKeyFileName(string newValue)
	{
		Database.TenantProperties.SetEncryptionKeyFileName(newValue);
	}

	public virtual string GetEncryptionKeyHash()
	{
		return Database.TenantProperties.GetEncryptionKeyHash();
	}

	public virtual void SetEncryptionKeyHash(string newValue)
	{
		Database.TenantProperties.SetEncryptionKeyHash(newValue);
	}

	internal void AddTenantAction(Action<NavTenant> action)
	{
		lock (tenantActions)
		{
			tenantActions.Enqueue(action);
		}
	}

	internal void ProcessTenantActions()
	{
		while (tenantActions.Count != 0)
		{
			Action<NavTenant> action = null;
			lock (tenantActions)
			{
				if (tenantActions.Count == 0)
				{
					break;
				}
				action = tenantActions.Dequeue();
			}
			action(this);
		}
	}

	public void OnFirstDatabaseAccess()
	{
		diagnostics.SendTraceTag("000007X", Category.Tenant, Verbosity.Normal, "OnFirstDatabaseAccess, tenantID={0}, runStatus={1}", DiagnosticParameter.Oii(Id), DiagnosticParameter.SystemMetadata(onFirstDatabaseAccessMethodHasBeenRun));
		lock (dataInitializationObj)
		{
			if (onFirstDatabaseAccessMethodHasBeenRun || State == NavTenantState.Mounting)
			{
				return;
			}
			Database.OnFirstDatabaseAccess();
			if (NavEnvironment.Instance.StandaloneInstance)
			{
				onFirstDatabaseAccessMethodHasBeenRun = true;
				return;
			}
			if (Database.DatabaseType == NavDatabaseType.Application)
			{
				NavGlobal.AppDatabase.InitializeChangeListener();
			}
			else if (!Database.IsNewTenantDatabase)
			{
				SessionEventTableHandler.EnsureSessionEventTriggersExists(Database);
			}
			onFirstDatabaseAccessMethodHasBeenRun = true;
		}
	}

	internal void OnTenantAdded()
	{
		waitForTenantToBeInitialized.Set();
		StartNasIfNotRunning();
	}

	internal void ReleaseInitializationLock()
	{
		waitForTenantToBeInitialized.Set();
	}

	internal void OnNasPermanentlyFailed()
	{
		nas = null;
	}

	internal void StartNasIfNotRunning()
	{
		if (!NavEnvironment.Instance.StandaloneInstance && TenantSettings.NasServicesEnabled && nas == null)
		{
			if (ServerUserSettings.Instance.NASServicesStartupCodeunit.Value > 0)
			{
				nas = new NavApplicationServer(this);
				nas.StartAsync(default).LogOrFailOnException();
			}
			else
			{
				string message = string.Format(CultureInfo.CurrentCulture, Lang.CannotStartNasInMutiTenantEnvironment, tenantSettings.Id, ProductNames.ProductNameServer);
				LogWriterHelper.LogEvent(EventLogEntryType.Warning, message);
			}
		}
	}

	internal bool GenerateTenantAppInsightsGuidIfNeeded()
	{
		if (TenantType != NavTenantType.Normal || Database.DatabaseType != NavDatabaseType.Tenant)
		{
			return false;
		}
		if (!Database.IsCurrentVersion)
		{
			return false;
		}
		if (!Database.TenantProperties.AppInsightsIdentifiers.ContainsKey(Database.SecurityAndLicense.License.LicenseUniqueIdentifier))
		{
			Database.TenantProperties.AddAppInsightsIdentifier(Database.SecurityAndLicense.License.LicenseUniqueIdentifier, Guid.NewGuid());
			return true;
		}
		return false;
	}

	internal void RemoveOrphanedAccessControlRecords()
	{
		List<Tuple<NavValue, NavValue, NavValue>> list = (from r in NavSqlQueryCommand.QueryWithSingleScope(rolesInAccessControlQuery.Value, Database, 0, 0, 0, FiltersAndMarks.Empty, null, includeCurrent: false, FindType.All, null, SecurityFiltering.Ignored, addExplicitJoinHints: false)
			select new Tuple<NavValue, NavValue, NavValue>(r[0], r[1], r[2])).ToList();
		if (list.Count == 0)
		{
			return;
		}
		IEnumerable<Tuple<NavValue, NavValue, NavValue>> second = from r in NavSqlQueryCommand.QueryWithSingleScope(rolesInPermissionSetQuery.Value, NavGlobal.AppDatabase, 0, 0, 0, FiltersAndMarks.Empty, null, includeCurrent: false, FindType.All, null, SecurityFiltering.Ignored, addExplicitJoinHints: false)
			select new Tuple<NavValue, NavValue, NavValue>(NavInteger.Create(0), new NavGuid(((WrappedValue<Guid>)(object)AppId.None).Value), r[0]);
		HashSet<Tuple<NavValue, NavValue, NavValue>> rolesToRemove = new HashSet<Tuple<NavValue, NavValue, NavValue>>(list.Where((Tuple<NavValue, NavValue, NavValue> k) => k.Item1.ToInt32() == 0 && k.Item2.ToGuid() == ((WrappedValue<Guid>)(object)AppId.None).Value).Except(second));
		if (rolesToRemove.Count == 0)
		{
			return;
		}
		((ITenantSessionHandler)this).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant tenant, NavSession systemSession) =>
		{
			NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000053);
			NCLMetaField fieldByNo = metaTableById.GetFieldByNo(8);
			NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(9);
			NCLMetaField fieldByNo3 = metaTableById.GetFieldByNo(2);
			using NavRecord navRecord = metaTableById.CreateObjectInstance(systemSession);
			navRecord.SetFilter(fieldByNo, "%1", NavOption.Create(fieldByNo.FieldOptionMetadata, 0));
			navRecord.SetFilter(fieldByNo2, "%1", new NavGuid(((WrappedValue<Guid>)(object)AppId.None).Value));
			if (navRecord.ALFind(DataError.TrapError, "-"))
			{
				do
				{
					NavValue fieldValue = navRecord.GetFieldValue(fieldByNo);
					NavValue fieldValue2 = navRecord.GetFieldValue(fieldByNo2);
					NavValue fieldValue3 = navRecord.GetFieldValue(fieldByNo3);
					Tuple<NavValue, NavValue, NavValue> item = new Tuple<NavValue, NavValue, NavValue>(fieldValue, fieldValue2, fieldValue3);
					if (rolesToRemove.Contains(item))
					{
						navRecord.ALDelete(DataError.TrapError);
					}
				}
				while (navRecord.ALNext() != 0);
			}
		}), true, false, false, (LanguageSetting)null);
	}

	internal void OnNewSession()
	{
		ProcessTenantActions();
	}

	public bool CanCreateSession(bool allowNonoperationalStateConnection = false, bool enableLogging = true, bool allowedDuringUpgradeState = true)
	{
		if (TenantSettings == null)
		{
			return true;
		}
		if (State == NavTenantState.Mounting)
		{
			if (FeatureSwitches.Instance.EnableWaitOnInitializeTenant)
			{
				waitForTenantToBeInitialized.WaitOne(allowNonoperationalStateConnection ? TimeSpan.FromMinutes(10.0) : TimeSpan.FromSeconds(30.0));
			}
			ReleaseInitializationLock();
		}
		if (FailedToMount || IsTenantDismounting || State == NavTenantState.Mounting)
		{
			if (enableLogging)
			{
				Diagnostics.SendTraceTag("00001CI", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Can create session not allowed, FailedToMount={0}, IsTenantDismounting={1}, State={2}", FailedToMount, IsTenantDismounting, State);
			}
			return false;
		}
		if (IsInExclusiveAccessMode)
		{
			if (enableLogging && !ExclusiveTenantActionElevatedScope.HasElevatedAccess)
			{
				Diagnostics.SendTraceTag("00001CJ", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Can create session not allowed, does not have \"HasElevatedAccess\"");
			}
			return ExclusiveTenantActionElevatedScope.HasElevatedAccess;
		}
		if (!allowNonoperationalStateConnection)
		{
			if (State == NavTenantState.Mounted)
			{
				try
				{
					Database.ValidateDatabaseState();
				}
				catch (NavBaseException)
				{
				}
				finally
				{
					RefreshState();
				}
			}
			if (!IsConsideredOperational() && !HasUpgradeStatus() && State != NavTenantState.Mounted)
			{
				if (enableLogging)
				{
					Diagnostics.SendTraceTag("00001CK", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Can create session not allowed, state={0}, hasUpgradeStatus={1}", State, HasUpgradeStatus());
				}
				return false;
			}
			if (State != NavTenantState.Mounted && Database.DatabaseType == NavDatabaseType.Tenant && Database.TenantProperties.TenantType == NavTenantType.Deleted)
			{
				if (enableLogging)
				{
					Diagnostics.SendTraceTag("00001JU", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Can create session not allowed, Tenant type={0}", Database.TenantProperties.TenantType);
				}
				return false;
			}
		}
		if (!allowedDuringUpgradeState && HasUpgradeStatus())
		{
			if (enableLogging)
			{
				Diagnostics.SendTraceTag("00001MP", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Can create session not allowed, state={0}, hasUpgradeStatus={1}", State, HasUpgradeStatus());
			}
			return false;
		}
		return true;
	}

	private bool IsConsideredOperational()
	{
		if (State == NavTenantState.Operational)
		{
			return true;
		}
		RefreshState(useTenantDatabaseToCalculateState: true, checkOnOperationalWithSyncPending: true);
		if (State == NavTenantState.Operational)
		{
			return true;
		}
		if (State == NavTenantState.OperationalWithSyncPending || State == NavTenantState.OperationalDataUpgradePending)
		{
			return ServerUserSettings.Instance.AllowSessionWhileSyncAndDataUpgrade.Value;
		}
		return false;
	}

	private bool HasUpgradeStatus()
	{
		if (State != NavTenantState.OperationalDataUpgradePending && State != NavTenantState.OperationalDataUpgradeInProgress)
		{
			return State == NavTenantState.OperationalWithDataUpgradeFailure;
		}
		return true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposingGate.TryEnter())
		{
			return;
		}
		try
		{
			waitForTenantToBeInitialized.Close();
			diagnostics.SendTraceTag("000003S", Category.Runtime, Verbosity.Normal, "NavTenant Dispose Called, ID={0}", DiagnosticParameter.Oii(Id));
			using (SessionShutdownExecutionListener.SetupSessionShutdownExecutionListener(this))
			{
				StopAllSessions();
			}
			if (!NavEnvironment.Instance.InServerShutdown && NavEnvironment.Instance.CacheSynchronizationController != null && database.IsValueCreated)
			{
				NavEnvironment.Instance.CacheSynchronizationController.OnTenantDataChanged(this);
			}
		}
		finally
		{
			try
			{
				if (Tree != null)
				{
					Tree.DisposeAllChildren();
				}
			}
			finally
			{
				if (Tree != null)
				{
					Tree.Dispose();
				}
				disposed = true;
				database = null;
			}
			if (tenantCache != null && tenantCache != MemoryCache.Default)
			{
				tenantCache.Dispose();
			}
			tenantCache = null;
			diagnostics.SendTraceTag("000003T", Category.Runtime, Verbosity.Normal, "NavTenant Disposed, ID={0}", DiagnosticParameter.Oii(Id));
		}
	}

	public void StopAllSessionsExcept(NavSession sessionThatShouldNotBeStopped)
	{
		Task task = Task.WhenAll(from session in AllSessions
			where session != sessionThatShouldNotBeStopped
			let task = session.DisposeAsync(ShutdownScope.SingleTenant, !session.AdminSession, !session.AdminSession, alwaysAsync: true, externalStopRequest: true)
			select task.LogOrFailOnExceptionReturnOriginalTask(failFast: false));
		task.WaitThrowingOriginalException();
	}

	public void StopAllSessions()
	{
		StopAllSessionsExcept(null);
	}

	internal void CheckForDataUpgradePendingStatus(string tenantAppVersion)
	{
		if (!string.IsNullOrEmpty(tenantAppVersion) && (State == NavTenantState.Mounted || State == NavTenantState.Operational))
		{
			string applicationVersion = NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion;
			if (!Version.TryParse(tenantAppVersion, out var result))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Application version [{0}] is unsupported and can not be updated", tenantAppVersion));
			}
			if (!Version.TryParse(applicationVersion, out var result2))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Application version [{0}] is unsupported and can not be updated", applicationVersion));
			}
			if (result2 > result)
			{
				diagnostics.SendTraceTag("0000959", Category.Tenant, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Tenant state changed from {0} to {1}", State, NavTenantState.OperationalDataUpgradePending);
				State = NavTenantState.OperationalDataUpgradePending;
			}
		}
	}

	internal void CheckForExtensionDataUpgradePendingStatus()
	{
		Dictionary<AppId, NavAppRuntimeMetadata> dictionary = (from m in NavEnvironment.Instance.TenantNavAppMetadataRetriever.GetForTenant(Id)
			where m.ExtensionType == NavExtensionType.ModernDev
			select m).ToDictionary((NavAppRuntimeMetadata m) => m.AppId);
		List<INavAppMetadataReference> appsInstalledForTenantInTenantDatabase = NavAppInstallationManagement.GetAppsInstalledForTenantInTenantDatabase(Database);
		foreach (INavAppMetadataReference item in appsInstalledForTenantInTenantDatabase)
		{
			if (dictionary.ContainsKey(((INavAppReference)item).AppId) && dictionary[((INavAppReference)item).AppId].Version != ((INavAppReference)item).Version)
			{
				diagnostics.SendTraceTag("000095A", Category.Tenant, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Tenant state changed from {0} to {1}", State, NavTenantState.OperationalDataUpgradePending);
				State = NavTenantState.OperationalDataUpgradePending;
				break;
			}
		}
	}

	internal void RefreshState(bool useTenantDatabaseToCalculateState = true, bool checkOnOperationalWithSyncPending = false)
	{
		if (disposed)
		{
			DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00000Y8", Category.Sql, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Tenant instance is disposed contrary to expectation");
			throw new ObjectDisposedException("NavTenant");
		}
		if (!database.IsValueCreated || database.Value?.DatabaseSync == null || State == NavTenantState.Mounting)
		{
			return;
		}
		if ((useTenantDatabaseToCalculateState & checkOnOperationalWithSyncPending) && State == NavTenantState.OperationalWithSyncPending)
		{
			Database.DatabaseSync.ForceRefreshOfPendingTableChanges();
			return;
		}
		NavTenantState state = State;
		CalculateStateOnMetadata(useTenantDatabaseToCalculateState);
		if (useTenantDatabaseToCalculateState && State != NavTenantState.Failed && State != NavTenantState.OperationalWithSyncFailure)
		{
			CalculateStateOnData();
		}
		LogStateChange(state, useTenantDatabaseToCalculateState);
	}

	private void CalculateStateOnMetadata(bool useTenantDatabaseToCalculateState = true)
	{
		DetailedState = database.Value.DatabaseSync.DatabaseSyncStateDetailed;
		switch (database.Value.DatabaseSync.DatabaseSyncState)
		{
		case DatabaseSyncState.NotSynchronized:
			State = TenantStateBasedOnPendingChanges;
			if (useTenantDatabaseToCalculateState)
			{
				KeyValuePair<string, List<string>> supportedFamilyAndVersions = GetSupportedFamilyAndVersions();
				bool flag = supportedFamilyAndVersions.Value == null || !supportedFamilyAndVersions.Value.Contains(NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion);
				if ((database.Value.IsSharedTenantSchema & flag) && Database.DatabaseSync.IsSupportedPreviousVersionTenant())
				{
					State = NavTenantState.Operational;
				}
				else if ((State == NavTenantState.Operational || State == NavTenantState.Mounted) & flag)
				{
					State = NavTenantState.OperationalWithSyncPending;
				}
			}
			break;
		case DatabaseSyncState.Synchronized:
			State = TenantStateBasedOnPendingChanges;
			break;
		case DatabaseSyncState.BeingSynchronized:
			State = NavTenantState.OperationalSyncInProgress;
			break;
		case DatabaseSyncState.Failed:
			if (State != NavTenantState.Failed)
			{
				State = NavTenantState.OperationalWithSyncFailure;
			}
			break;
		}
		if (useTenantDatabaseToCalculateState)
		{
			CheckForExtensionSyncPendingStatus();
		}
	}

	internal static bool IsSqlConnectionException(NavBaseException navException)
	{
		if (navException is NavSqlException)
		{
			return true;
		}
		if (navException is NavCSideException ex)
		{
			return ex.ErrorCode == 22924076;
		}
		return false;
	}

	private void CheckForExtensionSyncPendingStatus()
	{
		if (State != NavTenantState.Operational && State != NavTenantState.Mounted)
		{
			return;
		}
		IEnumerable<AppSyncTrackingRecord> source;
		using (NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(Database))
		{
			DatabaseAppSyncTrackingRepository databaseAppSyncTrackingRepository = new DatabaseAppSyncTrackingRepository(Database, conScope);
			source = databaseAppSyncTrackingRepository.GetAll(hasBaseline: false).ToList();
		}
		List<Tuple<NavAppRuntimeMetadata, string>> list = new List<Tuple<NavAppRuntimeMetadata, string>>();
		foreach (NavAppRuntimeMetadata installedApp in NavAppGroup.OrderedAppMetadata)
		{
			AppSyncTrackingRecord appSyncTrackingRecord = source.FirstOrDefault((AppSyncTrackingRecord s) => (WrappedComparable<Guid>)(object)s.AppId == (WrappedComparable<Guid>)(object)installedApp.AppId);
			if (appSyncTrackingRecord == null || appSyncTrackingRecord.Version < installedApp.Version)
			{
				string item = appSyncTrackingRecord?.Version.ToString() ?? string.Empty;
				list.Add(Tuple.Create<NavAppRuntimeMetadata, string>(installedApp, item));
			}
		}
		if (list.Any())
		{
			diagnostics.SendTraceTag("000095B", Category.Tenant, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Tenant state changed from {0} to {1}", State, NavTenantState.OperationalWithSyncPending);
			State = NavTenantState.OperationalWithSyncPending;
			string detailedSyncStatus = string.Join(Environment.NewLine, list.Select((Tuple<NavAppRuntimeMetadata, string> ns) => Lang.NavAppSyncCheck_SyncedVersionTooOld.FormatWithResolvedCulture(ns.Item1.TechnicalDisplayName, ns.Item2)));
			DetailedState = new NavTenantDatabaseSyncStatus(0, 0, detailedSyncStatus);
		}
	}

	private void CalculateStateOnData()
	{
		string tenantDataVersion = GetTenantDataVersion();
		if (!string.IsNullOrEmpty(Database.TenantProperties.TenantDataVersion) && !DataIsCompatibleWithApplication())
		{
			State = NavTenantState.Failed;
			string detailedSyncStatus = string.Format(CultureInfo.InvariantCulture, "Tenant {1} cannot be used because it is assigned a data version ({0}) that is greater than the Application Version ({2}) supported on the {3} instance.", Database.TenantProperties.TenantDataVersion, Id, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion, ProductNames.ProductNameServer);
			DetailedState = new NavTenantDatabaseSyncStatus(0, 0, detailedSyncStatus);
		}
		if (State == NavTenantState.Failed)
		{
			return;
		}
		CheckForDataUpgradePendingStatus(tenantDataVersion);
		if ((State == NavTenantState.Operational || State == NavTenantState.Mounted || State == NavTenantState.OperationalDataUpgradePending) && !Database.TenantProperties.IsNewTenant)
		{
			CheckForExtensionDataUpgradePendingStatus();
		}
		if (State == NavTenantState.Operational)
		{
			switch (Database.DatabaseSync.DatabaseTenantDataOperationState)
			{
			case DatabaseTenantDataOperationState.CopyInProgress:
				State = NavTenantState.OperationalTenantDataCopyInProgress;
				break;
			case DatabaseTenantDataOperationState.CopyFailed:
				State = NavTenantState.OperationalTenantDataCopyFailed;
				break;
			case DatabaseTenantDataOperationState.MoveInProgress:
				State = NavTenantState.OperationalTenantDataMoveInProgress;
				break;
			case DatabaseTenantDataOperationState.RemoveInProgress:
				State = NavTenantState.OperationalTenantRemoveInProgress;
				break;
			case DatabaseTenantDataOperationState.RemoveFailed:
				State = NavTenantState.OperationalTenantRemoveFailed;
				break;
			}
		}
		CheckTenantStateDependingOnRunningDataUpgrade();
	}

	internal bool DataIsCompatibleWithApplication()
	{
		if (Version.TryParse(Database.TenantProperties.TenantDataVersion, out var result) && Version.TryParse(NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion, out var result2))
		{
			return result <= result2;
		}
		return false;
	}

	private void LogStateChange(NavTenantState prevState, bool useTenantDatabaseToCalculateState)
	{
		if (prevState != State)
		{
			diagnostics.SendTraceTag("00001M3", Category.Tenant, Verbosity.Normal, "The state of tenant: {0} refreshed from {1} to {2} with the database's detailed sync state being: {3}. UseTenantDatabase={4}", DiagnosticParameter.Oii(Id), DiagnosticParameter.SystemMetadata(prevState), DiagnosticParameter.SystemMetadata(State), DiagnosticParameter.SystemMetadata(DetailedState), DiagnosticParameter.SystemMetadata(useTenantDatabaseToCalculateState));
		}
	}

	private KeyValuePair<string, List<string>> GetSupportedFamilyAndVersions()
	{
		try
		{
			return NavSqlTenantDatabaseVersion.GetSupportedFamilyAndVersions(Database);
		}
		catch (NavSqlException ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance(this).SendExceptionTag("00001IC", Category.SqlConnection, ex, Verbosity.Error, DiagnosticParameter.SystemMetadata("Failed to get the supported versions for the tenant database"));
			State = NavTenantState.Failed;
			DetailedState = new NavTenantDatabaseSyncStatus(0, 0, ex.Message);
		}
		return default;
	}

	private string GetTenantDataVersion()
	{
		try
		{
			return database.Value.TenantProperties.TenantDataVersion;
		}
		catch (NavCSideException ex)
		{
			string detailedSyncStatus = string.Format(CultureInfo.CurrentCulture, Lang.CannotRefreshDataVersionFromTenant, ex.Message);
			DiagnosticsResolver.GetMostSpecificInstance(this).SendExceptionTag("00001ID", Category.SqlConnection, ex, Verbosity.Error, DiagnosticParameter.SystemMetadata("Failed to get the tenant data version for the tenant."));
			State = NavTenantState.Failed;
			DetailedState = new NavTenantDatabaseSyncStatus(0, 0, detailedSyncStatus);
		}
		return string.Empty;
	}

	private void CheckTenantStateDependingOnRunningDataUpgrade()
	{
		if (State != NavTenantState.Operational && State != NavTenantState.OperationalDataUpgradePending)
		{
			return;
		}
		NavDataUpgradeProcessExecutionState upgradeInformation = Database.UpgradeManager.GetUpgradeInformation();
		if (upgradeInformation.State == NavWorkflowState.InProgress)
		{
			State = NavTenantState.OperationalDataUpgradeInProgress;
			DetailedState = new NavTenantDatabaseSyncStatus(upgradeInformation.ExecutedFunctions, upgradeInformation.TotalFunctionCount, string.Format(CultureInfo.InvariantCulture, "Completed {0} out of {1} upgrade functions.", upgradeInformation.ExecutedFunctions, upgradeInformation.TotalFunctionCount));
		}
		else if (upgradeInformation.ExecutionErrors != null && upgradeInformation.ExecutionErrors.Any((NavDataUpgradeExecutionDetails e) => !e.PostUpgrade))
		{
			switch (upgradeInformation.UpgradeExecutionMode)
			{
			case NavDataUpgradeExecutionMode.UsingSingleTransaction:
				State = NavTenantState.OperationalDataUpgradePending;
				break;
			case NavDataUpgradeExecutionMode.UsingParallelOrSerialTransaction:
				State = NavTenantState.OperationalWithDataUpgradeFailure;
				break;
			default:
				State = NavTenantState.OperationalWithDataUpgradeFailure;
				break;
			}
			DetailedState = new NavTenantDatabaseSyncStatus(upgradeInformation.ExecutedFunctions, upgradeInformation.TotalFunctionCount, upgradeInformation.ExecutionErrors.First().Error);
		}
	}

	protected virtual NavDatabase CreateDatabase()
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000WI", Category.Tenant, Verbosity.Normal, "Lazily initializing a new NavDatabase instance for tenant '{0}'. DatabaseServer: {1}. DatabaseName: {2}.", DiagnosticParameter.Oii(Id), DiagnosticParameter.SystemMetadata(DatabaseServer?.ServerName), DiagnosticParameter.Oii(TenantSettings.DatabaseName));
		return new NavDatabase(this, DatabaseServer, TenantSettings.DatabaseName, NavDatabaseType.Tenant);
	}

	public static string ConvertTenantIdToExpectedCase(string tenantId)
	{
		return tenantId?.ToLowerInvariant();
	}

	public void RunActionWhenTenantIsMounted(Action<NavTenant> action)
	{
		if (State == NavTenantState.Mounting)
		{
			AddTenantAction(action);
		}
		else
		{
			action(this);
		}
	}

	public void PurgeOldTenantSessionEvents()
	{
		long num;
		long num2;
		do
		{
			num = Interlocked.Read(ref lastSessionEventPurgeTime);
			if (DateTime.UtcNow.Ticks < num + ServerUserSettings.Instance.SessionEventTablePurgeLookupPeriod.Value.Ticks)
			{
				return;
			}
			long ticks = DateTime.UtcNow.Ticks;
			num2 = Interlocked.CompareExchange(ref lastSessionEventPurgeTime, ticks, num);
		}
		while (num != num2);
		NavTaskFactory.StartNewTask(() =>
		{
			SessionEventTableHandler.PurgeSessionEventTable(Database);
		});
	}

	public void LogTenantAzureUsage()
	{
		try
		{
			if (State != NavTenantState.Operational)
			{
				return;
			}
			long num;
			long num2;
			do
			{
				num = Interlocked.Read(ref lastAzureTelemetryReportTime);
				if (DateTime.UtcNow.Ticks < num + AzureTelemetryEmissionPeriod.Ticks)
				{
					return;
				}
				long ticks = DateTime.UtcNow.Ticks;
				num2 = Interlocked.CompareExchange(ref lastAzureTelemetryReportTime, ticks, num);
			}
			while (num != num2);
			GenerateTenantAppInsightsGuidIfNeeded();
			azureLicenseUsageService.EnqueueTenant(this);
		}
		catch (Exception exception)
		{
			Diagnostics.SendExceptionTag("00001L9", Category.License, exception, Verbosity.Warning, DiagnosticParameter.SystemMetadata("Error while enqueuing tenant for telemetry logging. Ignoring."));
		}
	}

	internal bool AddToCache(string cacheKey, object cacheObject, DateTime absoluteExpiryTime, TimeSpan slidingExpiryTime)
	{
		if (absoluteExpiryTime <= DateTime.UtcNow)
		{
			Diagnostics.SendTraceTag("00000ZG", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Cannot add cache item with invalid expiration. CacheKey: {0}, Absolute: {1}, Sliding: {2}", cacheKey, absoluteExpiryTime, slidingExpiryTime);
			return false;
		}
		if (tenantCache == null)
		{
			tenantCache = CreateCache();
		}
		string text = Id + cacheKey;
		string text2 = "Absolute-" + text;
		if (!tenantCache.Contains(text) && tenantCache.Contains(text2))
		{
			tenantCache.Remove(text2);
		}
		if (!tenantCache.Add(text2, new object(), absoluteExpiryTime))
		{
			Diagnostics.SendTraceTag("00000ZH", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Failed to add absolute cache object.");
		}
		CacheEntryChangeMonitor item = tenantCache.CreateCacheEntryChangeMonitor(new string[1] { text2 });
		if (!tenantCache.Add(text, cacheObject, new CacheItemPolicy
		{
			SlidingExpiration = slidingExpiryTime,
			ChangeMonitors = { (ChangeMonitor)item }
		}))
		{
			Diagnostics.SendTraceTag("00000ZI", Category.Security, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Failed to add sliding cache object.");
			return false;
		}
		return true;
	}

	internal T GetFromCache<T>(string cacheKey) where T : class
	{
		if (tenantCache == null)
		{
			return null;
		}
		string text = Id + cacheKey;
		string key = "Absolute-" + text;
		if (tenantCache.Contains(key) && tenantCache.Contains(text))
		{
			return tenantCache.Get(text) as T;
		}
		return null;
	}

	internal void RemoveFromCache(string cacheKey)
	{
		if (tenantCache != null)
		{
			string text = Id + cacheKey;
			string key = "Absolute-" + text;
			tenantCache.Remove(key);
			tenantCache.Remove(text);
		}
	}

	private MemoryCache CreateCache()
	{
		return MemoryCache.Default;
	}

	internal IEnumerable<string> GetAlternateIdsForApplication(string appId)
	{
		ReadOnlyCollection<string> alternateId = TenantSettings.AlternateId;
		if (string.IsNullOrEmpty(appId))
		{
			appId = ServerUserSettings.Instance.DefaultApplicationId.Value;
		}
		string preferredDomainFragment = HttpDomainHelper.PreferredDomainFragment(appId);
		if (!string.IsNullOrEmpty(preferredDomainFragment))
		{
			IEnumerable<string> enumerable = alternateId.Where((string t) => -1 != t.IndexOf(preferredDomainFragment, StringComparison.OrdinalIgnoreCase));
			if (enumerable.Any())
			{
				return enumerable;
			}
		}
		return alternateId;
	}
}
