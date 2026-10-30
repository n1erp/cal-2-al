using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using System.Runtime;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Diagnostics.Telemetry;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Data;
using Microsoft.Dynamics.Nav.Runtime.Debugger;
using Microsoft.Dynamics.Nav.Runtime.XmlMetadata;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.XmlMetadata;
using Microsoft.Dynamics.Nav.Xrm;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavEnvironment : IDisposable
{
	private const int BlobCacheVersion = 3;

	private const string ManagementObjectSearcherMemoryQuery = "SELECT Capacity FROM Win32_PhysicalMemory";

	private static ulong TotalPhysicalMemory;

	private static NavEnvironment instance;

	private static readonly object lockObject = new object();

	private static readonly WindowsIdentity serviceAccount = WindowsIdentity.GetCurrent();

	private static ManualResetGate compactLohGate = new ManualResetGate();

	private static readonly Guid instanceId = Guid.NewGuid();

	private CancellationTokenSource collectAndCompactHeapCancelationToken;

	private readonly List<Assembly> appAssembly = new List<Assembly>();

	private string webServiceName = string.Empty;

	private NetProtocolType netType;

	private TempPathHelper tempPathHelper;

	private static string serviceInstanceName = string.Empty;

	private static bool digitalSignatureCheckEnabled = true;

	private readonly object heartBeatSync = new object();

	private ICacheSynchronizationController cacheSynchronizationController;

	private readonly object cacheSynchronizationControllerLock = new object();

	private volatile TenantStateReporter tenantStateReporter;

	private readonly object taskSchedulerLock = new object();

	private ITaskScheduler taskScheduler;

	private NavPerformanceCounterSetter performanceCounterSetter;

	private readonly List<TableChangeMonitor> tableChangeMonitors = new List<TableChangeMonitor>();

	private readonly DebugRuntimeManager debugRuntimeManager = new DebugRuntimeManager();

	private Lazy<INavFilesystemContentCache> navAppBlobCache;

	private int serviceInstanceId = -1;

	private readonly bool standaloneInstance;

	public const string ApplicationNamespace = "Microsoft.Dynamics.Nav.BusinessApplication";

	public const int EnforceRequestedLanguage = -1;

	public const int LanguageNotSet = 0;

	public const int FormattingCultureNotSet = 0;

	public const string DefaultLanguageAbbreviation = "ENU";

	internal const string ServerInstanceNamePrefix = "MicrosoftDynamicsNavServer$";

	private NavEnvironmentState state = NavEnvironmentState.Ready;

	private bool trackingTenantAssociations;

	private readonly Lazy<NavThreadScheduler> threadScheduler = new Lazy<NavThreadScheduler>(() => new NavThreadScheduler());

	private readonly Lazy<NavChildSessionTaskScheduler> childSessionScheduler = new Lazy<NavChildSessionTaskScheduler>(() => new NavChildSessionTaskScheduler());

	internal ICollection<Action> OnAfterApplicationMounted = new List<Action>();

	private static string serverInstanceName;

	public bool IsDisposed { get; private set; }

	public NavDatabaseServerCollection DatabaseServers { get; private set; }

	public NavTenantCollection Tenants { get; private set; }

	public NavTenantDatabaseCollection TenantDatabases { get; private set; }

	public static Guid InstanceId => instanceId;

	internal LRUCache LRUCache { get; private set; }

	internal INavAppGroupResolver NavAppGroupResolver { get; private set; }

	internal NavAppGroupUpdater NavAppGroupUpdater { get; private set; }

	public INavAppLanguageProvider NavAppLanguageProvider { get; private set; }

	public INavAppMetadataRetriever NavAppMetadataRetriever { get; private set; }

	public INavFilesystemContentCache NavAppBlobCache => navAppBlobCache.Value;

	public ITenantNavAppMetadataRetriever TenantNavAppMetadataRetriever { get; private set; }

	public static IServiceTopology Topology { get; set; } = new StandardServiceTopology();

	internal NavAppContentHashComparer NavAppContentHashComparer { get; private set; }

	internal INavV1PageConfiguration NavV1PageConfigurationRepository { get; private set; }

	internal IProfilePageMetadataRetriever NavProfilePageMetadataCache { get; set; }

	internal IProfilePageMetadataRetrieverV1 NavProfilePageMetadataCacheV1 { get; private set; }

	internal Compilation CompilationWithSystemSymbols { get; set; }

	internal bool HasAliveServerInstancesChangedSubscribers => AliveServerInstancesChanged != null;

	public bool DevelopmentMode { get; }

	internal static string ApplicationAssemblyPath
	{
		get
		{
			if (AppDomain.CurrentDomain.RelativeSearchPath != null)
			{
				return AppDomain.CurrentDomain.RelativeSearchPath;
			}
			return AppDomain.CurrentDomain.BaseDirectory;
		}
	}

	public static int DefaultLanguage => NavResourceManager.DefaultLanguageId;

	public static IEnumerable<int> SupportedLanguages => NavResourceManager.SupportedLanguageIds;

	public static NavEnvironment Instance
	{
		get
		{
			if (instance == null)
			{
				lock (lockObject)
				{
					if (instance == null)
					{
						ServerUserSettings.CreateInstance(string.Empty);
						instance = new NavEnvironment(NavEnvironmentFlags.None);
					}
				}
			}
			return instance;
		}
	}

	internal DeadlockAnalyzer DeadlockAnalyzer { get; set; }

	internal static bool IsDevelopmentModeInstance
	{
		get
		{
			if (instance != null)
			{
				return instance.DevelopmentMode;
			}
			return false;
		}
	}

	internal static bool IsInstantiated => instance != null;

	internal ITaskScheduler TaskScheduler
	{
		get
		{
			if (taskScheduler == null)
			{
				lock (taskSchedulerLock)
				{
					if (taskScheduler == null)
					{
						taskScheduler = new NavTaskScheduler();
					}
				}
			}
			return taskScheduler;
		}
	}

	public bool StandaloneInstance => standaloneInstance;

	public NavEnvironmentState State => state;

	public NavThreadScheduler ThreadScheduler => threadScheduler.Value;

	public NavChildSessionTaskScheduler ChildSessionTaskScheduler => childSessionScheduler.Value;

	public bool InServerShutdown => state == NavEnvironmentState.ShuttingDown;

	public static bool IsServiceTier => true;

	public static byte[] ServiceFabricServerLicense { get; set; }

	public static SecurityIdentifier ServiceAccount => serviceAccount.User;

	public static string ServiceAccountName => serviceAccount.Name;

	public bool InAppsDisabledMode { get; }

	public bool InEventsDisabledMode { get; }

	public bool InBackgroundChangeListenerDisabledMode { get; }

	internal bool CompileBusinessApplicationAtStartup
	{
		get
		{
			if (!StandaloneInstance && !DevelopmentMode && (ServerUserSettings.Instance.CompileBusinessApplicationAtStartup.Value || ServerUserSettings.Instance.EnableDebugging.Value))
			{
				return !ServerUserSettings.Instance.EnableStaticAssemblies.Value;
			}
			return false;
		}
	}

	internal bool AllowCachingOfMetadataXml => CompileBusinessApplicationAtStartup;

	internal List<Assembly> ApplicationAssemblies
	{
		get
		{
			if (appAssembly.Count == 0)
			{
				List<string> knownApplicationAssemblies = GetKnownApplicationAssemblies();
				foreach (string item in knownApplicationAssemblies)
				{
					appAssembly.Add(LoadAssemblyWithCheck(item));
				}
			}
			return appAssembly;
		}
	}

	internal ICacheSynchronizationController CacheSynchronizationController => cacheSynchronizationController;

	public bool IsService { get; set; }

	public string WebServiceName
	{
		get
		{
			return webServiceName;
		}
		set
		{
			webServiceName = value;
		}
	}

	public NetProtocolType NetworkProtocol
	{
		get
		{
			return netType;
		}
		set
		{
			netType = value;
		}
	}

	internal static INavLicense NavLicense
	{
		get
		{
			NavSession session = NavCurrentThread.Session;
			if (session == null || session.Tenant == null)
			{
				return NavGlobal.SystemTenant.Database.SecurityAndLicense.License;
			}
			NavDatabaseSecurityAndLicense securityAndLicense = session.Tenant.Database.SecurityAndLicense;
			if (securityAndLicense != null && securityAndLicense.License != null)
			{
				return securityAndLicense.License;
			}
			return NavGlobal.SystemTenant.Database.SecurityAndLicense.License;
		}
	}

	public static string LicenseInformation => NavLicense.Description;

	public NavPerformanceCounterSetter PerformanceCounterSetter
	{
		get
		{
			return performanceCounterSetter;
		}
		set
		{
			performanceCounterSetter = value;
		}
	}

	public static ITenantConfigurationSynchronizer TenantConfigurationSynchronizer
	{
		get
		{
			if (IsDiscoveryServiceEnabled)
			{
				return StandaloneTenantConfigurationSynchronizer.Instance;
			}
			return Heartbeat.Instance;
		}
	}

	internal DebugRuntimeManager DebugRuntimeManager => debugRuntimeManager;

	public TempPathHelper TemporaryPathHelper
	{
		get
		{
			if (tempPathHelper == null)
			{
				lock (this)
				{
					if (tempPathHelper == null)
					{
						string text = string.Empty;
						if (Instance.StandaloneInstance)
						{
							text = "_" + Process.GetCurrentProcess().Id + "_" + AppDomain.CurrentDomain.Id;
						}
						tempPathHelper = new TempPathHelper(FullServiceInstanceName + text);
					}
				}
			}
			return tempPathHelper;
		}
	}

	public static string ServiceInstanceName
	{
		get
		{
			return serviceInstanceName;
		}
		set
		{
			serviceInstanceName = value;
		}
	}

	public static string FullServiceInstanceName
	{
		get
		{
			string text = serviceInstanceName;
			if (text.StartsWith("MicrosoftDynamicsNavServer$", StringComparison.OrdinalIgnoreCase))
			{
				return text;
			}
			return "MicrosoftDynamicsNavServer$" + text;
		}
	}

	public static string NavServerName { get; set; }

	public static int NavClientServicesPort { get; set; }

	public static int ManagementServicesPort { get; set; }

	public static string ServerInstanceName
	{
		get
		{
			return serverInstanceName ?? string.Empty;
		}
		set
		{
			serverInstanceName = value;
			if (NavDiagnostics.TraceWriter != null)
			{
				NavDiagnostics.TraceWriter.InstanceName = serverInstanceName;
			}
		}
	}

	public static string ServerComputerName => DnsHelper.MachineName;

	public static string CurrentTenantId
	{
		get
		{
			if (NavCurrentThread.Session == null || NavCurrentThread.Session.Tenant == null)
			{
				return string.Empty;
			}
			return NavCurrentThread.Session.Tenant.Id;
		}
	}

	internal static bool IsDiscoveryServiceEnabled => !string.IsNullOrWhiteSpace(ServerUserSettings.Instance.DiscoveryServiceUri.Value);

	public static bool DisableCompileBusinessApplication { get; set; }

	internal event EventHandler<AliveServerInstancesChangedEventArgs> AliveServerInstancesChanged;

	private static List<string> GetKnownApplicationAssemblies()
	{
		List<string> list = new List<string>();
		int num = 0;
		bool flag = true;
		while (flag)
		{
			string text = num switch
			{
				0 => "\\NavApplication.dll", 
				1 => "\\NavSecondaryApplication.dll", 
				_ => "\\NavApplication" + num.ToString(CultureInfo.InvariantCulture) + ".dll", 
			};
			if (File.Exists(ApplicationAssemblyPath + text))
			{
				list.Add(ApplicationAssemblyPath + text);
			}
			else
			{
				flag = false;
			}
			num++;
		}
		return list;
	}

	internal void OnAliveServerInstancesChanged(object sender, AliveServerInstancesChangedEventArgs e)
	{
		AliveServerInstancesChanged?.Invoke(sender, e);
	}

	private static void CheckDigitalSignature(string assemblyPath)
	{
		if (!digitalSignatureCheckEnabled || (DigitalSignatureChecker.VerifyDigitalSignature(assemblyPath) && DigitalSignatureChecker.DigitalSignatureBelongsToMicrosoft(assemblyPath)))
		{
			return;
		}
		throw new InvalidDigitalSignatureException();
	}

	[Conditional("DEBUG")]
	[Conditional("LAB_EXECUTION")]
	private static void DisableDigitalSignatureCheck()
	{
		digitalSignatureCheckEnabled = false;
	}

	internal static Assembly LoadAssemblyWithCheck(string assemblyPath)
	{
		if (!File.Exists(assemblyPath))
		{
			throw new FileNotFoundException();
		}
		CheckDigitalSignature(assemblyPath);
		return Assembly.LoadFrom(assemblyPath);
	}

	private void Dispose(bool disposing)
	{
		try
		{
			if (!disposing || IsDisposed)
			{
				return;
			}
			try
			{
				if (collectAndCompactHeapCancelationToken != null)
				{
					collectAndCompactHeapCancelationToken.Cancel();
					collectAndCompactHeapCancelationToken.Dispose();
				}
				EndCacheSynchronization();
				StopTaskScheduling();
				taskScheduler?.Dispose();
				StopHeartbeat();
				StopHealthMonitor();
				StopTenantStateReporter();
				if (Tenants.IsSystemTenantCreatedNoWait)
				{
					NavGlobal.SystemTenant.NCLCodeLoader?.DeleteUnneededSources();
				}
				Tenants.Dispose();
				DatabaseServers.Dispose();
				if (performanceCounterSetter != null)
				{
					performanceCounterSetter.Dispose();
				}
				foreach (TableChangeMonitor tableChangeMonitor in tableChangeMonitors)
				{
					tableChangeMonitor.Dispose();
				}
				if (childSessionScheduler.IsValueCreated)
				{
					childSessionScheduler.Value.Dispose();
				}
				if (threadScheduler.IsValueCreated)
				{
					threadScheduler.Value.Dispose();
				}
				tableChangeMonitors.Clear();
				XrmServiceProvider.UnloadServices();
			}
			finally
			{
				if (tempPathHelper != null)
				{
					tempPathHelper.Dispose();
				}
			}
		}
		finally
		{
			IsDisposed = true;
		}
	}

	internal static void InstantiateDevelopmentNavEnvironment()
	{
		if (instance != null)
		{
			throw new InvalidOperationException("NavEnvironment already instantiated");
		}
		ServerUserSettings.CreateDefaultInstance();
		lock (lockObject)
		{
			instance = new NavEnvironment(NavEnvironmentFlags.DevelopmentMode);
		}
	}

	internal static void InstantiateCustomNavEnvironment(NavEnvironmentFlags flags)
	{
		if (instance != null)
		{
			throw new InvalidOperationException("NavEnvironment already instantiated");
		}
		ServerUserSettings.CreateDefaultInstance();
		lock (lockObject)
		{
			instance = new NavEnvironment(flags);
		}
	}

	internal static void InstantiateStandaloneNavEnvironment(bool enableNavApps = true, bool developmentMode = false, bool removeAllAppsMode = false)
	{
		NavEnvironmentFlags navEnvironmentFlags = NavEnvironmentFlags.Standalone;
		if (!enableNavApps)
		{
			navEnvironmentFlags |= NavEnvironmentFlags.DisableNavApps;
		}
		if (developmentMode)
		{
			navEnvironmentFlags |= NavEnvironmentFlags.DevelopmentMode;
		}
		if (removeAllAppsMode)
		{
			navEnvironmentFlags |= NavEnvironmentFlags.RemoveAllAppsMode;
		}
		if (instance != null)
		{
			throw new InvalidOperationException("NavEnvironment already instantiated");
		}
		ServerUserSettings.CreateDefaultInstance();
		lock (lockObject)
		{
			instance = new NavEnvironment(navEnvironmentFlags);
		}
	}

	internal void SetupTaskScheduler(ITaskScheduler scheduler)
	{
		lock (taskSchedulerLock)
		{
			taskScheduler?.Dispose();
			taskScheduler = scheduler;
		}
	}

	public void ThrowIfInAppsDisabledMode(string message)
	{
		if (InAppsDisabledMode)
		{
			string message2 = TraceStringHelper.AppendStackTrace(message);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000009Z", Category.Runtime, Verbosity.Error, PrivacyClassification.SystemMetadata, message2);
			throw new NavNotSupportedException(message);
		}
	}

	internal static void Terminate(Exception unhandledException, bool withGrace)
	{
		if (unhandledException == null)
		{
			throw new ArgumentNullException("unhandledException");
		}
		if (NavServiceControl.Controller != null && (withGrace || unhandledException is NavBaseException || unhandledException is NavServerSessionBaseException))
		{
			NavServiceControl.Controller.Terminate(unhandledException);
		}
		else
		{
			TerminateWithFailFast(unhandledException);
		}
	}

	internal static void TerminateWithFailFast(Exception unhandledException)
	{
		LogWriterHelper.LogExceptionEvent(unhandledException);
		DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001MR", Category.Runtime, unhandledException, Verbosity.Critical);
		EventLogWriter.StopAndWait();
		Environment.FailFast(unhandledException.ToString());
	}

	internal static void WriteWatsonLog(Exception e)
	{
		if (NavServiceControl.Controller != null)
		{
			NavServiceControl.Controller.WriteWatsonLog(e);
		}
		else
		{
			Terminate(e, withGrace: false);
		}
	}

	public int GetServiceInstanceId()
	{
		if (IsDiscoveryServiceEnabled)
		{
			if (serviceInstanceId == -1)
			{
				throw new InvalidOperationException("Environment is not initialized yet. Make sure that discovery controller is started.");
			}
			return serviceInstanceId;
		}
		StartHeartbeat();
		return Heartbeat.Instance?.ServiceInstanceId ?? 0;
	}

	public void SetServiceInstanceId(int newServiceInstanceId)
	{
		if (!IsDiscoveryServiceEnabled)
		{
			throw new NotSupportedException("Impossible to set service instance id when discovery service is not used");
		}
		if (serviceInstanceId != -1)
		{
			throw new InvalidOperationException("Service instance id is already set");
		}
		serviceInstanceId = newServiceInstanceId;
	}

	private NavEnvironment(NavEnvironmentFlags flags)
	{
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Expected Obj, but got Unknown
		ConfigurationManager.AppSettings.Set("wcf:disableOperationContextAsyncFlow", false.ToString(CultureInfo.InvariantCulture));
		standaloneInstance = flags.HasFlag(NavEnvironmentFlags.Standalone);
		DevelopmentMode = flags.HasFlag(NavEnvironmentFlags.DevelopmentMode);
		InAppsDisabledMode = flags.HasFlag(NavEnvironmentFlags.DisableNavApps);
		InEventsDisabledMode = flags.HasFlag(NavEnvironmentFlags.DisableEvents);
		InBackgroundChangeListenerDisabledMode = flags.HasFlag(NavEnvironmentFlags.DisableBackgroundChangeListener);
		NavDiagnostics.TraceWriter = new EtwTelemetryLog(ServerInstanceName);
		NavDiagnostics.GetMostSpecificInstanceGetter = () => DiagnosticsResolver.GetMostSpecificInstance();
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000SS", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "NavEnvironment created");
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000VC", Category.Runtime, Verbosity.Normal, "NavEnvironmentFlags -- {0}", DiagnosticParameter.SystemMetadata(flags));
		if (!Topology.CanTrackSessionsInMemory)
		{
			ServerUserSettings.Instance.TrackSessionsInMemory.Value = false;
		}
		ServerUserSettings.LogServerStartupSettings(ServerUserSettings.Instance);
		NavResourceManager.DefaultLanguageIdFunc = () => ServerUserSettings.Instance.DefaultLanguage.LCID;
		NavResourceManager.SupportedLanguageIdsFunc = () => ServerUserSettings.Instance.SupportedLanguages.SupportedLanguageIds;
		if (FeatureSwitches.Instance.RawValues.Any())
		{
			string message = string.Join(", ", FeatureSwitches.Instance.RawValues.Select((KeyValuePair<string, bool> raw) => raw.Key + ": " + raw.Value));
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000A3", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, message);
		}
		ServicePointManager.SecurityProtocol = ServerUserSettings.Instance.SecurityProtocol.Value;
		ServicePointManager.CheckCertificateRevocationList = ServerUserSettings.Instance.CheckCertificateRevocationList.Value;
		ServicePointManager.DefaultConnectionLimit = ServerUserSettings.Instance.ServicePointManagerDefaultConnectionLimit.Value;
		ServicePointManager.MaxServicePointIdleTime = ServerUserSettings.Instance.ServicePointManagerMaxServicePointIdleTime.Value;
		navAppBlobCache = new Lazy<INavFilesystemContentCache>(() =>
		{
			NavFilesystemContentCache navFilesystemContentCache = new NavFilesystemContentCache(TemporaryPathHelper.AppMetadataPath, 3.ToString(CultureInfo.InvariantCulture));
			navFilesystemContentCache.Initialize();
			return navFilesystemContentCache;
		}, LazyThreadSafetyMode.ExecutionAndPublication);
		NavAppMetadataRetriever = new CachingDatabaseNavAppMetadataRetriever(() => navAppBlobCache.Value);
		DatabaseTenantNavAppRetriever databaseTenantNavAppRetriever = new DatabaseTenantNavAppRetriever();
		TenantNavAppMetadataRetriever = (ITenantNavAppMetadataRetriever)new TenantNavAppMetadataRetriever((ITenantNavAppRetriever)(object)databaseTenantNavAppRetriever, (IServerLogger)(object)new LogWriterHelperServerLogger(), (Func<PackageId, NavAppRuntimeMetadata>)NavAppMetadataRetriever.GetMetadata);
		NavAppContentHashComparer = new NavAppContentHashComparer(NavAppMetadataRetriever.GetPackageBlob, (NavAppHashAlgorithm)1);
		if (InAppsDisabledMode)
		{
			NavAppGroupResolver = new BaseOnlyNavAppGroupResolver();
		}
		else
		{
			NavAppGroupResolver = new NavAppGroupResolver(TenantNavAppMetadataRetriever, NavAppMetadataRetriever.GetMetadata);
		}
		NavAppPackageLanguageStoreFactory navAppPackageLanguageStoreFactory = new NavAppPackageLanguageStoreFactory();
		NavAppLanguageProvider = new NavAppLanguageProvider(navAppPackageLanguageStoreFactory.GetLanguageStoreForPackage, NavAppGroupResolver.GetExistingGroupById);
		NavAppGroupUpdater = new NavAppGroupUpdater(NavAppGroupResolver);
		LRUCache = new LRUCache(ServerUserSettings.Instance.DataCacheSize.CacheSize);
		ServerUserSettings.SettingsChanged += LRUCache.ServerUserSettingsChanged;
		DatabaseServers = new NavDatabaseServerCollection();
		Tenants = new NavTenantCollection(OnApplicationMounted, NavAppGroupUpdater);
		TenantDatabases = new NavTenantDatabaseCollection();
		NavV1PageConfigurationRepository = new DatabaseNavV1PageConfigurationRepository();
		NavProfilePageMetadataCacheV1 = new CachingDatabaseProfilePageMetadataRetrieverV1(NavV1PageConfigurationRepository);
		NavProfilePageMetadataCache = new CachingDatabaseProfilePageMetadataRetriever(new DatabaseNavPageCustomizationRepository());
		performanceCounterSetter = new NavPerformanceCounterSetter(new NavPerformanceCounterManager(ServiceInstanceName));
		ExternalDataServiceManager.InitializeExternalDataServiceProviders("Xrm", (Action<int, string>)XrmServiceProvider.RegisterXrmService);
		if (!standaloneInstance)
		{
			System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (object sender, UnobservedTaskExceptionEventArgs e) =>
			{
				WriteWatsonLog(e.Exception);
				e.SetObserved();
			};
		}
	}

	~NavEnvironment()
	{
		Dispose(disposing: false);
	}

	internal void OnApplicationMounted()
	{
		if (ServerUserSettings.Instance.EnableV2MissingHashFixerOnStartup.Value)
		{
			ContentHashFixer.EnsureHashesAreCalculatedForPublishedArtifacts(NavGlobal.AppDatabase);
		}
		NavGlobal.MetadataProvider.TrackObjects();
		NavGlobal.AppDatabase.InitializeChangeListener();
		IEnumerable<NavAppRuntimeMetadata> allMetadata = NavAppMetadataRetriever.GetAllMetadata();
		NavGlobal.NavAppClrTypeRetriever.PurgeUnreferencedArtifacts(allMetadata);
		NavGlobal.NCLCodeLoader.CompileBusinessEventAssemblies();
		NavGlobal.SystemTenant.EventSubscriptionMetadata.InitializeBaseEvents();
		Task task = NavGlobal.NCLCodeLoader.CompileBusinessAssembliesAsync();
		RegisterTenantAssociationChangeHandler();
		NavGlobal.MetadataProvider.UpdateMetadataVersion();
		NavGlobal.AppDatabase.UpdateApplicationInformation();
		SetupDeadlockAnalyzer();
		if (!IsService)
		{
			return;
		}
		StartHeartbeat();
		StartHealthMonitor();
		StartTaskScheduling();
		StartTenantStateReporter();
		if (!standaloneInstance)
		{
			EnableCacheSynchronization(!ServerUserSettings.Instance.DisableDataCache.Value);
		}
		RegisterSessionEventTableHandler();
		string message = string.Format(CultureInfo.InvariantCulture, "The NAV application was mounted from database '{0}' on database server '{1}'.", NavGlobal.SystemTenant.Database.DatabaseName, NavGlobal.SystemTenant.Database.DatabaseServer.ServerName);
		NavServerEventSource.Log.WriteApplicationMountedEvent(ServiceInstanceName, message);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000194", Category.Runtime, Verbosity.Normal, "NavEnvironment initialized on Platform version: {0} Application version: {1} Application build: {2}", DiagnosticParameter.SystemMetadata(NavSession.PlatformVersion), DiagnosticParameter.SystemMetadata(NavGlobal.AppDatabase.ApplicationVersion), DiagnosticParameter.SystemMetadata(NavGlobal.AppDatabase.ApplicationBuild));
		if (NavGlobal.NCLMetadata.SymbolAggregator != null)
		{
			NavGlobal.NCLMetadata.SymbolAggregator.StartReadingSymbolReferences();
		}
		foreach (Action item in OnAfterApplicationMounted)
		{
			item();
		}
		task.ContinueWith((Task task2) =>
		{
			SetupGcCollector();
			ServerUserSettings.SettingsChanged += (object sender, SettingsChangedEventArgs args) =>
			{
				if (string.Compare("CompactLargeObjectHeapInterval", args.SettingName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					SetupGcCollector();
				}
			};
		});
	}

	private void SetupGcCollector()
	{
		if (collectAndCompactHeapCancelationToken != null)
		{
			collectAndCompactHeapCancelationToken.Cancel();
			collectAndCompactHeapCancelationToken = null;
		}
		if (ServerUserSettings.Instance.CompactLargeObjectHeapInterval.TotalSeconds > 0 && Environment.ProcessorCount > 1)
		{
			collectAndCompactHeapCancelationToken = new CancellationTokenSource();
			Thread thread = new Thread(CollectAndCompactHeap);
			thread.IsBackground = true;
			thread.Name = "CollectAndCompactHeap";
			thread.Priority = ThreadPriority.Lowest;
			thread.Start(collectAndCompactHeapCancelationToken.Token);
		}
	}

	private static void CompactLargeObjectHeap()
	{
		try
		{
			using (DiagnosticsResolver.GetMostSpecificInstance().CreateTimingScope("00001KE", Category.Runtime, DiagnosticParameter.SystemMetadata("Compact large object heap(LOH)")))
			{
				ulong totalPhysicalMemory = GetTotalPhysicalMemory();
				long privateMemorySize = Process.GetCurrentProcess().PrivateMemorySize64;
				int num = (int)((double)privateMemorySize / (double)totalPhysicalMemory * 100.0);
				bool flag = num >= ServerUserSettings.Instance.LargeObjectCompactionThresholdPercent.Value;
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000BEL", Category.Runtime, Verbosity.Normal, "Total available memory: {0} GB. NST memory usage: {1} GB. Running GC: {2}.", DiagnosticParameter.SystemMetadata(BytesToGb(totalPhysicalMemory)), DiagnosticParameter.SystemMetadata(BytesToGb((ulong)privateMemorySize)), DiagnosticParameter.Oii(flag));
				if (flag)
				{
					GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
					GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
					ulong totalPhysicalMemory2 = GetTotalPhysicalMemory();
					long privateMemorySize2 = Process.GetCurrentProcess().PrivateMemorySize64;
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000BEM", Category.Runtime, Verbosity.Normal, "Total available memory: {0} GB. NST memory usage after GC: {1} GB.", DiagnosticParameter.SystemMetadata(BytesToGb(totalPhysicalMemory2)), DiagnosticParameter.SystemMetadata(BytesToGb((ulong)privateMemorySize2)));
				}
			}
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("0000BF7", Category.Runtime, exception, Verbosity.Critical, DiagnosticParameter.SystemMetadata("Failed to perform LOH compaction."));
			LogWriterHelper.LogExceptionEvent(exception);
		}
		finally
		{
			compactLohGate.Reset();
		}
	}

	private static double BytesToGb(ulong totalMemory)
	{
		return (double)totalMemory / 1024.0 / 1024.0 / 1024.0;
	}

	private static ulong GetTotalPhysicalMemory()
	{
		if (TotalPhysicalMemory != 0L)
		{
			return TotalPhysicalMemory;
		}
		ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory");
		foreach (ManagementBaseObject item in managementObjectSearcher.Get())
		{
			TotalPhysicalMemory += Convert.ToUInt64(item.Properties["Capacity"].Value, CultureInfo.InvariantCulture);
		}
		return TotalPhysicalMemory;
	}

	private void CollectAndCompactHeap(object arg)
	{
		CancellationToken cancellationToken = (CancellationToken)arg;
		while (!cancellationToken.IsCancellationRequested)
		{
			if (compactLohGate.TryEnter())
			{
				threadScheduler.Value.RunActionOnceOnIdle(CompactLargeObjectHeap);
			}
			Thread.Sleep(ServerUserSettings.Instance.CompactLargeObjectHeapInterval.TotalSeconds * 1000);
		}
	}

	private void SetupDeadlockAnalyzer()
	{
		if (!ServerUserSettings.Instance.EnableDeadlockMonitoring.Value)
		{
			NavDatabase.DatabaseFirstAccess += DeadlockAnalyzer.CleanupEventSessionForDatabase;
			return;
		}
		NavDatabase.DatabaseFirstAccess += (NavDatabase database) =>
		{
			if (database.SqlServerProperties.MajorProductVersion < 11)
			{
				database.Tenant.Diagnostics.SendTraceTag("00000GK", Category.Sql, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Deadlock monitoring feature is enabled, but it is not supported for SQL Server versions older than 2012.");
			}
			else if (!database.HasViewStatePermission || !database.HasAlterAnyEventSessionPermission)
			{
				database.Tenant.Diagnostics.SendTraceTag("00000DE", Category.Sql, Verbosity.Warning, "Deadlock monitoring feature is enabled but current SQL server user does not have 'VIEW SERVER STATE' or 'ALTER ANY EVENT SESSION' permissions for the database: {0}. Please grant both of those permisions in order to use this feature.", DiagnosticParameter.Oii(database.DatabaseName));
			}
			else if (DeadlockAnalyzer.SetupEventSessionForDatabase(database))
			{
				database.DatabaseServer.SqlConnectionPool.EnableTransactionHistory();
			}
		};
		DeadlockAnalyzer = new DeadlockAnalyzer(new TraceDeadlockLogTarget());
		NavSqlConnection.OnSqlException += DeadlockAnalyzer.OnDeadlockException;
	}

	private void RegisterTenantAssociationChangeHandler()
	{
		if (trackingTenantAssociations)
		{
			return;
		}
		NavGlobal.AppDatabase.NavAppTenantAssociationChanged += (object sender, NavTrackingEventArgs ea) =>
		{
			List<string> list = ea.TrackingObjects.Select((NavTrackingObject trackingObj) => trackingObj.TenantId).Distinct().ToList();
			foreach (string item in list)
			{
				if (Tenants.TryGetTenantById(item, out var tenant) && !tenant.IsDisposed && !tenant.Database.IsDisposed)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000OA", Category.Extensions, Verbosity.Normal, "Updating tenant extension group (change listener) -- Tenant:{0}", DiagnosticParameter.Oii(item));
					try
					{
						NavAppGroupUpdater.UpdateGroupForChangedTenant(tenant);
					}
					catch (Exception exception)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000SU", Category.Sql, exception);
					}
				}
			}
			NavGlobal.MetadataProvider.UpdateMetadataVersion();
		};
		trackingTenantAssociations = true;
	}

	private void RegisterSessionEventTableHandler()
	{
		AliveServerInstancesChanged += (object sender, AliveServerInstancesChangedEventArgs e) =>
		{
			if (Instance.Tenants == null)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000792", Category.HealthCheck, Verbosity.Warning, PrivacyClassification.SystemMetadata, "NavTenantCollection is null. Skipping SessionEventTable registration for this Heartbeat.");
			}
			else
			{
				if (!Instance.Tenants.IsSingleTenancyMode)
				{
					Tenants.Configuration.DeleteRemovedTenantConfigurationsIfAcknowledged();
					if (e.AnotherServerRemoved)
					{
						Tenants.Configuration.RecoverTenantsFromExclusiveAccessState();
					}
				}
				if (e.AnotherServerHasCrashed)
				{
					Tenants.AddTenantAction((NavTenant tenant) =>
					{
						NavTaskFactory.StartNewTask(() =>
						{
							SessionEventTableHandler.ForceCloseDeadSessions(tenant.Database);
						});
					}, Instance.Tenants.SystemTenant);
				}
			}
		};
	}

	public void EndCacheSynchronization()
	{
		if (cacheSynchronizationController == null)
		{
			return;
		}
		try
		{
			cacheSynchronizationController.Dispose();
		}
		catch
		{
		}
		finally
		{
			cacheSynchronizationController = null;
		}
	}

	public static void DisposeIfInitialized()
	{
		lock (lockObject)
		{
			if (instance != null)
			{
				instance.Dispose();
			}
		}
	}

	internal static void DisposeAndClearInstance()
	{
		lock (lockObject)
		{
			try
			{
				if (instance != null)
				{
					instance.Dispose();
				}
			}
			finally
			{
				instance = null;
			}
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public static int FindSupportedLanguage(int languageId, int defaultLanguageId)
	{
		if (WindowsLanguageDataProvider.IsAvailableLanguage(languageId))
		{
			return languageId;
		}
		CultureInfo cultureInfo = new CultureInfo(languageId);
		if (cultureInfo.Parent != null && cultureInfo.Parent != CultureInfo.InvariantCulture && cultureInfo.TextInfo.LCID != cultureInfo.Parent.TextInfo.LCID)
		{
			return FindSupportedLanguage(cultureInfo.Parent.TextInfo.LCID, defaultLanguageId);
		}
		if (defaultLanguageId != 0)
		{
			return FindSupportedLanguage(defaultLanguageId, 0);
		}
		return DefaultLanguage;
	}

	private void StartHeartbeat()
	{
		if (Heartbeat.Instance != null || standaloneInstance || IsDiscoveryServiceEnabled)
		{
			return;
		}
		lock (heartBeatSync)
		{
			if (Heartbeat.Instance == null)
			{
				Heartbeat.Initialize();
			}
		}
	}

	private void StartHealthMonitor()
	{
		if (!standaloneInstance && !IsDiscoveryServiceEnabled)
		{
			HealthMonitor.Initialize();
		}
	}

	public void StopHeartbeat()
	{
		lock (heartBeatSync)
		{
			Heartbeat.Destroy();
		}
	}

	public void StopHealthMonitor()
	{
		lock (heartBeatSync)
		{
			HealthMonitor.Instance?.Dispose();
		}
	}

	public void StartTenantStateReporter()
	{
		if (tenantStateReporter == null && !standaloneInstance)
		{
			tenantStateReporter = new TenantStateReporter();
		}
	}

	public void StopTenantStateReporter()
	{
		tenantStateReporter?.Stop();
	}

	public void StartTaskScheduling()
	{
		lock (taskSchedulerLock)
		{
			TaskScheduler.Start();
		}
	}

	public void StopTaskScheduling()
	{
		lock (taskSchedulerLock)
		{
			if (taskScheduler != null)
			{
				taskScheduler.Stop();
			}
		}
	}

	public void RegisterHeartbeatForStandaloneInstance()
	{
		if (!standaloneInstance)
		{
			throw new NotSupportedException("Should not call this unless standalone instance");
		}
		string serviceName = Guid.NewGuid().ToString();
		using Heartbeat heartbeat = new Heartbeat(Heartbeat.Period, DnsHelper.MachineName, serviceName, serviceName, 0, 0);
		heartbeat.WriteHeartbeatToServiceInstanceTable();
	}

	public void AddTableChangeMonitor(TableChangeMonitor tableChangeMonitor)
	{
		if (!IsDisposed)
		{
			tableChangeMonitors.Add(tableChangeMonitor);
		}
	}

	public void EnableCacheSynchronization(bool enabled, ICacheSynchronizationController cacheSyncController = null)
	{
		lock (cacheSynchronizationControllerLock)
		{
			if (cacheSyncController != null || !enabled)
			{
				EndCacheSynchronization();
			}
			if (enabled && cacheSynchronizationController == null)
			{
				cacheSynchronizationController = cacheSyncController ?? new CacheSynchronizationController();
				NavDiagnostics.GlobalInstance.SendTraceTag("000087K", Category.CacheSynchronization, Verbosity.Normal, PrivacyClassification.SystemMetadata, "EnableCacheSynchronization with these arguments. DisableDataCache:{0} - Topology.EnableServiceBusCacheSync:{1} - ServerUserSettings.EnableServiceBusCacheSync:{2}", ServerUserSettings.Instance.DisableDataCache.Value, Topology.EnableServiceBusCacheSync, ServerUserSettings.Instance.EnableServiceBusCacheSync.Value);
			}
		}
	}

	public async Task ServerTerminating()
	{
		ChangeState(NavEnvironmentState.ShuttingDown);
		using (SessionShutdownExecutionListener.SetupSessionShutdownExecutionListener())
		{
			await Task.WhenAll(from tenant in Tenants.AllTenants()
				from session in tenant.ActiveSessions
				let task = session.DisposeAsync(ShutdownScope.Server, !session.AdminSession, abortChannel: false, alwaysAsync: true, externalStopRequest: true)
				select task.LogOrFailOnExceptionReturnOriginalTask(failFast: false, ignoreNavBaseExceptionUnlessAttributed: true));
		}
	}

	public void ChangeState(NavEnvironmentState newState)
	{
		if (state != newState)
		{
			state = newState;
			switch (state)
			{
			case NavEnvironmentState.Initializing:
				NavServerEventSource.Log.WriteNavServerInitializingEvent(ServiceInstanceName, "The service is initializing its configuration.");
				break;
			case NavEnvironmentState.Ready:
				NavServerEventSource.Log.WriteNavServerReadyEvent(ServiceInstanceName, "The service has completed configuration and is ready.");
				break;
			case NavEnvironmentState.ShuttingDown:
				NavServerEventSource.Log.WriteNavServerShuttingDownEvent(ServiceInstanceName, "The service is shutting down.");
				break;
			}
		}
	}
}
