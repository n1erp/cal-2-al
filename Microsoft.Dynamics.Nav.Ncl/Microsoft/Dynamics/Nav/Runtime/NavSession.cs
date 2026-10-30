using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.LicensingService.Model;
using Microsoft.Dynamics.Nav.LicensingService.Model.V10;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Debugger;
using Microsoft.Dynamics.Nav.Runtime.Designer;
using Microsoft.Dynamics.Nav.Runtime.TaskScheduling;
using Microsoft.Dynamics.Nav.Runtime.TokenCache;
using Microsoft.Dynamics.Nav.Runtime.XmlMetadata;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.ClientDesigner;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSession : TreeObject, ISessionProvider, IReferenceTarget<NavSession>
{
	public enum LanguagePermission
	{
		Global,
		Form,
		Report,
		DataPort,
		Design,
		XmlPort
	}

	internal enum CallbackState
	{
		None,
		Allow,
		Deny,
		ConnectionClosed,
		SessionStopping,
		Unsupported
	}

	public enum CompanyDeletionType
	{
		Complete,
		Incremental,
		IncrementalNoSystemTask
	}

	private NavDialog rootDialog;

	private static NavMetric6Dimensions OpenCompanyDurationMetric;

	internal const int MaxIdentifierCharCount = 30;

	private const int ActiveSessionValidationInterval = 2;

	public const int UnknownSessionId = -1;

	private IServiceConnection serviceConnection;

	private readonly IReference<NavSession> selfReference;

	private int localPermissionSetupVersion;

	private readonly NavTenant tenant;

	private readonly NavSystemTenant systemTenant;

	private bool skipInsertSessionEventRecord;

	private Random random;

	private LocalReportCache localReportCache;

	private readonly int id = -1;

	private bool hasBeenOpened;

	private bool permissionEngineActivated;

	private bool accessControlDatabaseCalled;

	private static int lastSessionId;

	private ConnectionType clientConnectionType;

	private WebConnectionType webConnectionType;

	private string currentLCIDAbbreviatedName = "ENU";

	private bool defaultLCID = true;

	private CultureInfo currentCulture;

	private CultureInfo windowsCulture;

	private ClientSettings cultureSettings = new ClientSettings
	{
		LCID = NavEnvironment.DefaultLanguage
	};

	private NavCompany company;

	private readonly Stack<int> globalLanguageStack = new Stack<int>();

	private readonly Stack<long> dynamicCaptionStack = new Stack<long>();

	private readonly List<int> allowedReportLanguages = new List<int>();

	private Decimal18 defaultRoundPrecision;

	private bool defaultRoundPrecisionCached;

	private NavSqlConnectionScope currentSqlConnectionScope;

	private readonly Dictionary<string, bool> applicationAreaCache = new Dictionary<string, bool>();

	private const string ApplicationAreaAll = "#All";

	private const string InsertApplicationAreaAll = "#All,";

	private string[] applicationAreas = new string[1] { "#All" };

	private string applicationAreaString = string.Empty;

	private bool allowClientCacheFlush;

	private readonly SessionAccessLock accessLock;

	private int lastLanguage;

	private CultureInfo lastCultureInfo;

	private NavTestExecution testCodeUnitExecution;

	private Exception lastException;

	private IEnumerable<CallStackElement> lastErrorCallstack;

	private MethodInfoDictionary methodParameterInfoDictionary;

	private NavDiagnostics diagnostics;

	private byte[] privateLicense;

	private INavLicense license;

	private HashSet<string> cachedAllowedCompanies;

	private string sessionComment = string.Empty;

	private NavUserPermissions permissions;

	private static volatile Dictionary<ConnectionType, ClientType> connectionTypeToClientType;

	private NavDate workDate = NavDate.Undefined;

	private Dictionary<int, FormatSettings> formatSettingsDictionary;

	private int verifyExecutePermissionCacheBumperToken;

	private readonly HashSet<ApplicationObjectId> verifyExecutePermissionCache = new HashSet<ApplicationObjectId>();

	private FormatSettings lastFormatSettings;

	private int lastFormatSettingsLcid = -1;

	private readonly HashSet<ApplicationObjectId> alreadyWarned = new HashSet<ApplicationObjectId>();

	private readonly DataAccessSource dataAccessSource;

	private Queue<ServerCallbackRequest> requestsToClient;

	private readonly ExecutionMode executionMode;

	private Stopwatch serviceOperationStopwatch;

	private readonly bool isSystemSession;

	private readonly List<ConnectionWarning> warnings = new List<ConnectionWarning>();

	private readonly List<NavCodeunit> eventBindings = new List<NavCodeunit>(128);

	private DateTime earliestNextActiveSessionExistValidation;

	private readonly Guid uniqueSessionId = Guid.NewGuid();

	private bool callbackAllowed = true;

	private readonly NavAppUsageTracker navAppUsageTracker;

	private string appId;

	private HashSet<int> stalePages;

	private PageBackgroundTask pageBackgroundTask;

	private static Version platformVersion;

	private static FormatSettings invariantFormatSettings;

	private ExclusiveTenantActionScope exclusiveTenantActionScope;

	public Func<int, object[], object> AlternativeSystemCodeunitInvoke { get; set; }

	public Guid ApplicationExtension { get; set; }

	internal IUserPageMetadataRetriever UserPageMetadataCache { get; private set; }

	internal IUserPageMetadataRetrieverV1 UserPageMetadataCacheV1 { get; private set; }

	public DateTime EarliestAllowedFromRecordCache { get; set; }

	private bool DisableUpdateUserPersonalization
	{
		get
		{
			if (ClientConnectionType != ConnectionType.UnknownClient)
			{
				return ClientConnectionType == ConnectionType.WebServiceClient;
			}
			return true;
		}
	}

	internal Exception LastException
	{
		get
		{
			return lastException;
		}
		set
		{
			Exception ex = lastException;
			lastException = value;
			while (value != null)
			{
				if (value == ex)
				{
					return;
				}
				value = value.InnerException;
			}
			lastErrorCallstack = BuildCurrentStack();
		}
	}

	public NavDiagnostics Diagnostics => diagnostics;

	internal INavLicense License
	{
		get
		{
			if (license != null)
			{
				return license;
			}
			if (privateLicense != null)
			{
				license = NavLicense.Create(privateLicense, LicenseExpiryHandler);
				return license;
			}
			return Database.SecurityAndLicense.License;
		}
	}

	internal bool DisposeInitiated { get; private set; }

	internal NavSqlDebuggingStatistics SqlDebuggingStatistics { get; } = new NavSqlDebuggingStatistics();

	internal NavClientPaths NavClientPaths { get; } = new NavClientPaths();

	public NavSystemCodeunitFactory SystemCodeunitFactory => Company.SystemCodeunitFactory;

	internal string AppId
	{
		get
		{
			return appId;
		}
		private set
		{
			if (string.IsNullOrEmpty(value))
			{
				value = Authenticator.AuthorizedInAppId;
			}
			else if (string.Compare(value, Authenticator.AuthorizedInAppId, StringComparison.OrdinalIgnoreCase) != 0)
			{
				DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("000010I", Category.Security, Verbosity.Warning, PrivacyClassification.SystemMetadata, "The specified application ID '{0}' is different from the application the user was authorized: '{1}'. StackTrace: {2}", value, Authenticator.AuthorizedInAppId, Environment.StackTrace);
			}
			if (string.IsNullOrEmpty(value))
			{
				value = ServerUserSettings.Instance.DefaultApplicationId.Value;
			}
			else if (!ApplicationIdTypes.KnownApplicationIdTypes.Contains(value))
			{
				DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00000LN", Category.Security, Verbosity.Warning, PrivacyClassification.SystemMetadata, "The specified application ID {0} is not valid. StackTrace: {1}", value, Environment.StackTrace);
				value = ServerUserSettings.Instance.DefaultApplicationId.Value;
			}
			appId = value.ToUpperInvariant();
		}
	}

	internal DataAccessSource DataAccessSource => dataAccessSource;

	public IServiceConnection ServiceConnection
	{
		get
		{
			return serviceConnection;
		}
		set
		{
			if (serviceConnection != null)
			{
				throw new NotSupportedException();
			}
			serviceConnection = value;
		}
	}

	public LimitedUserPermissionChecker LimitedUserPermissionChecker { get; private set; }

	internal SqlProfilerCommentProvider SqlProfilerCommentProvider { get; private set; }

	public Action<StreamTransfer> UploadAction { get; set; }

	public LocalReportCache LocalReportCache
	{
		get
		{
			LocalReportCache localReportCache = this.localReportCache;
			if (localReportCache != null)
			{
				return localReportCache;
			}
			bool reportDomainIsolation = !ServerUserSettings.Instance.PreviousReportAppDomainIsolation.Value && ServerUserSettings.Instance.ReportAppDomainIsolation.Value;
			return this.localReportCache = new LocalReportCache(this, NavGlobal.MetadataProvider, (MetaReport metaReport) => new LocalReportHandle(metaReport.RDLCRuntimeData, metaReport.EnableExternalImages, metaReport.EnableHyperlinks, metaReport.EnableExternalAssemblies, reportDomainIsolation, !FeatureSwitches.Instance.DisableAppDomainMonitoring));
		}
	}

	internal Random Random
	{
		get
		{
			return random ?? (random = new Random());
		}
		set
		{
			random = value;
		}
	}

	public int Id => id;

	private static int NextSessionId => Interlocked.Increment(ref lastSessionId);

	public ConnectionType ClientConnectionType
	{
		get
		{
			return clientConnectionType;
		}
		set
		{
			clientConnectionType = value;
			webConnectionType = WebConnectionType.None;
		}
	}

	public bool IsDesktopWebClientSession
	{
		get
		{
			if (ClientConnectionType != ConnectionType.WebClient)
			{
				return ClientConnectionType == ConnectionType.Desktop;
			}
			return true;
		}
	}

	public bool IsSystemSession => isSystemSession;

	public WebConnectionType WebConnectionType
	{
		get
		{
			return webConnectionType;
		}
		set
		{
			webConnectionType = value;
		}
	}

	internal ClientStopExecutionListener ClientStopExecutionListener { get; set; }

	public string ClientMachineName { get; set; }

	public string DebuggingContext { get; private set; }

	public bool IsOpen
	{
		get
		{
			if (hasBeenOpened)
			{
				return !IsDisposed;
			}
			return false;
		}
	}

	public bool IsCompanyOpen
	{
		get
		{
			if (IsOpen && company != null)
			{
				return !string.IsNullOrEmpty(company.CompanyName);
			}
			return false;
		}
	}

	public ClientSettings RegionalSettings
	{
		get
		{
			return cultureSettings;
		}
		set
		{
			if (value.ShortDatePattern == null || value.ShortTimeFormat == null || value.TimeAMDesignator == null || value.TimePMDesignator == null || value.TimeSeparator == null || value.DateSeparator == null || value.DecimalSeparator == null || value.GroupSeparator == null)
			{
				throw new NavNCLClientCultureInfoException();
			}
			CheckConnectionIsOpen();
			try
			{
				currentCulture = CultureInfo.GetCultureInfo(value.LCID);
				Thread.CurrentThread.CurrentUICulture = currentCulture;
				Lang.Culture = null;
			}
			catch (ArgumentException)
			{
				throw new NavNotSupportedLanguageException(Lang.InvalidLanguage);
			}
			currentLCIDAbbreviatedName = currentCulture.ThreeLetterWindowsLanguageName;
			defaultLCID = false;
			cultureSettings = value;
			if (formatSettingsDictionary != null)
			{
				formatSettingsDictionary.Clear();
				lastFormatSettingsLcid = -1;
			}
			try
			{
				windowsCulture = GetFormattingCulture(cultureSettings);
				Thread.CurrentThread.CurrentCulture = windowsCulture;
			}
			catch (ArgumentException)
			{
				throw new NavNotSupportedFormatLanguageException(Lang.InvalidFormatLanguage);
			}
		}
	}

	public CultureInfo Culture
	{
		get
		{
			if (IsLocalLanguage)
			{
				return CultureInfo.GetCultureInfo(LocalLanguage);
			}
			return currentCulture ?? (currentCulture = CultureInfo.GetCultureInfo(cultureSettings.LCID));
		}
	}

	public CultureInfo WindowsCulture
	{
		get
		{
			if (windowsCulture == null)
			{
				_ = cultureSettings;
			}
			if (windowsCulture == null)
			{
				DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00001UP", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Set WindowsCulture sessionId:{0}, WindowsLCID:{1}, session.Culture:{2}", Id, cultureSettings.WindowsLCID, Culture);
			}
			return windowsCulture ?? (windowsCulture = ((cultureSettings.WindowsLCID > 0) ? CultureInfo.GetCultureInfo(cultureSettings.WindowsLCID) : Culture));
		}
	}

	public string AbbreviatedLanguageName => currentLCIDAbbreviatedName;

	public UserSettings Personalization
	{
		get
		{
			CheckConnectionIsOpen();
			return GetPersonalization();
		}
	}

	public IDocumentStorageService DocumentStorageService { get; set; }

	public int LCID
	{
		get
		{
			return GlobalLanguage;
		}
		set
		{
			CheckConnectionIsOpen();
			if (GlobalLanguage != value)
			{
				GlobalLanguage = value;
			}
		}
	}

	public NavDate WorkDate
	{
		get
		{
			CheckConnectionIsOpen();
			if (!(workDate == NavDate.Undefined))
			{
				return workDate;
			}
			return ALSystemDate.ALToday;
		}
		set
		{
			CheckConnectionIsOpen();
			workDate = ((value == ALSystemDate.ALToday) ? NavDate.Undefined : value);
		}
	}

	public bool SkipRestoreSavedReportSettings { get; set; }

	public bool IsLocalLanguage => globalLanguageStack.Count > 0;

	public int LocalLanguageNoFallback
	{
		get
		{
			if (globalLanguageStack.Count > 0)
			{
				int num = globalLanguageStack.Peek();
				if (num != -1)
				{
					return num;
				}
				int[] array = globalLanguageStack.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i] != -1)
					{
						return array[i];
					}
				}
			}
			return -1;
		}
	}

	public int LocalLanguage
	{
		get
		{
			int localLanguageNoFallback = LocalLanguageNoFallback;
			if (localLanguageNoFallback == -1)
			{
				return GlobalLanguage;
			}
			return localLanguageNoFallback;
		}
		set
		{
			PopLocalLanguage();
			PushLocalLanguage(value);
		}
	}

	public int GlobalLanguage
	{
		get
		{
			return cultureSettings.LCID;
		}
		set
		{
			CheckConnectionIsOpen();
			try
			{
				currentCulture = CultureInfo.GetCultureInfo(value);
				Lang.Culture = null;
				Thread.CurrentThread.CurrentUICulture = currentCulture;
			}
			catch (ArgumentException)
			{
				throw new NavNotSupportedLanguageException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidLanguageCode, value));
			}
			if (formatSettingsDictionary != null)
			{
				formatSettingsDictionary.Remove(value);
				lastFormatSettingsLcid = -1;
			}
			cultureSettings.LCID = value;
			defaultLCID = false;
			currentLCIDAbbreviatedName = CultureInfo.GetCultureInfo(value).ThreeLetterWindowsLanguageName;
		}
	}

	public int WindowsLanguage => cultureSettings.WindowsLCID;

	internal TableConnectionManager TableConnectionManager { get; private set; }

	public FormatSettings FormatSettings
	{
		get
		{
			if (formatSettingsDictionary == null)
			{
				formatSettingsDictionary = new Dictionary<int, FormatSettings>();
			}
			int num = LocalLanguageNoFallback;
			if (num == -1)
			{
				num = cultureSettings.LCID;
				if (num <= 0)
				{
					num = NavEnvironment.DefaultLanguage;
				}
			}
			else if (!WindowsLanguageDataProvider.IsAvailableLanguage(num))
			{
				int primaryLanguageId = LanguageHelper.GetPrimaryLanguageId(num);
				num = (WindowsLanguageDataProvider.IsAvailableLanguage(primaryLanguageId) ? primaryLanguageId : NavEnvironment.DefaultLanguage);
			}
			if (num == lastFormatSettingsLcid && num != -1)
			{
				return lastFormatSettings;
			}
			if (!formatSettingsDictionary.TryGetValue(num, out var value))
			{
				value = FormatSettings.Create(num, RegionalSettings);
				formatSettingsDictionary.Add(num, value);
			}
			lastFormatSettings = value;
			lastFormatSettingsLcid = num;
			return value;
		}
	}

	public bool HasExclusiveAccess
	{
		get
		{
			if (exclusiveTenantActionScope != null)
			{
				return !exclusiveTenantActionScope.IsDisposed;
			}
			return false;
		}
	}

	internal static FormatSettings InvariantFormatSettings
	{
		get
		{
			if (invariantFormatSettings != null)
			{
				return invariantFormatSettings;
			}
			ClientSettings clientSettings = default;
			clientSettings.Refresh(CultureInfo.InvariantCulture, CultureInfo.InvariantCulture);
			invariantFormatSettings = FormatSettings.Create(CultureInfo.InvariantCulture.LCID, clientSettings);
			return invariantFormatSettings;
		}
	}

	internal string GetLastErrorText
	{
		get
		{
			if (!(lastException is NavBaseException { SuppressMessage: not false }))
			{
				return lastException?.Message ?? string.Empty;
			}
			return string.Empty;
		}
	}

	internal string GetLastErrorTextWithoutPrivateInformation
	{
		get
		{
			if (lastException is NavBaseException ex)
			{
				if (ex.SuppressMessage)
				{
					return string.Empty;
				}
				return ex.MessageWithoutPrivateInformation;
			}
			return string.Empty;
		}
	}

	public Guid UniqueSessionId => uniqueSessionId;

	internal string GetLastErrorCode
	{
		get
		{
			if (lastException != null)
			{
				if (lastException is NavErrorCodeException ex)
				{
					return FormatLastErrorCode(ex.ErrorCode);
				}
				if (lastException.InnerException != null)
				{
					return GetErrorCode(lastException) + ":" + GetErrorCode(lastException.InnerException);
				}
				return GetErrorCode(lastException);
			}
			return string.Empty;
		}
	}

	public NavTestExecution TestExecution => testCodeUnitExecution ?? (testCodeUnitExecution = new NavTestExecution(this));

	internal CallbackState SessionCallbackState { get; private set; }

	public bool CallbackAllowed
	{
		get
		{
			if (!callbackAllowed)
			{
				SessionCallbackState = CallbackState.Deny;
				return false;
			}
			if (serviceConnection == null)
			{
				SessionCallbackState = CallbackState.ConnectionClosed;
				return false;
			}
			if (accessLock.IsStoppingOrDisposed)
			{
				SessionCallbackState = CallbackState.SessionStopping;
				return false;
			}
			bool flag = ClientCallbackOrNull?.IsCallbackAllowed ?? false;
			SessionCallbackState = (flag ? CallbackState.Allow : CallbackState.Unsupported);
			return flag;
		}
		internal set
		{
			callbackAllowed = value;
			SessionCallbackState = (value ? CallbackState.Allow : CallbackState.Deny);
		}
	}

	public IClientCallback ClientCallbackOrNull
	{
		get
		{
			IClientCallback clientCallback = ClientCallbackOverride;
			if (clientCallback == null && serviceConnection != null)
			{
				clientCallback = serviceConnection.ClientCallback;
			}
			return clientCallback;
		}
	}

	public IClientCallback ClientCallback
	{
		get
		{
			IClientCallback clientCallbackOrNull = ClientCallbackOrNull;
			if (clientCallbackOrNull == null)
			{
				throw new NavNCLCallbackNotAllowedException();
			}
			return clientCallbackOrNull;
		}
	}

	public IClientCallback ClientCallbackOverride { get; set; }

	public Queue<ServerCallbackRequest> RequestsToClient => requestsToClient ?? (requestsToClient = new Queue<ServerCallbackRequest>());

	public bool ConfigurationModeEnabled { get; set; }

	public bool PersonalizationDisabled { get; set; }

	public NavProfileKey ProfileKey { get; set; }

	public ExecutionMode ExecutionMode => executionMode;

	public Microsoft.Dynamics.Nav.Types.ExecutionContext ExecutionContext
	{
		get
		{
			if (AppInstallationContext != null)
			{
				if (!AppInstallationContext.IsInstalling)
				{
					return Microsoft.Dynamics.Nav.Types.ExecutionContext.Uninstall;
				}
				return Microsoft.Dynamics.Nav.Types.ExecutionContext.Install;
			}
			if (AppUpgradeContext != null)
			{
				return Microsoft.Dynamics.Nav.Types.ExecutionContext.Upgrade;
			}
			NavDataUpgradeProcessExecutionState navDataUpgradeProcessExecutionState = Database?.UpgradeManager?.GetUpgradeInformation();
			if (navDataUpgradeProcessExecutionState != null && navDataUpgradeProcessExecutionState.ExecutionDetails.Any((NavDataUpgradeExecutionDetails d) => d.SessionId == id && d.State == NavDataUpgradeMethodExecutionState.InProgress))
			{
				return Microsoft.Dynamics.Nav.Types.ExecutionContext.Upgrade;
			}
			return Microsoft.Dynamics.Nav.Types.ExecutionContext.Normal;
		}
	}

	public NavMethodScope CurrentMethodScope { get; internal set; }

	public TimeZoneInfo ClientTimeZone { get; set; }

	public NavUserAuthentication Authenticator { get; }

	public NavUser User => Authenticator.User;

	public string UserDisplayName => Authenticator?.User?.DisplayName ?? string.Empty;

	NavSession ISessionProvider.Session => this;

	public NavSqlConnectionScope CurrentSqlConnectionScope
	{
		get
		{
			return currentSqlConnectionScope;
		}
		set
		{
			currentSqlConnectionScope = value;
		}
	}

	internal MethodInfoDictionary MethodParameterInfoDictionary => methodParameterInfoDictionary ?? (methodParameterInfoDictionary = new MethodInfoDictionary());

	public string SessionComment
	{
		get
		{
			return sessionComment;
		}
		set
		{
			sessionComment = value;
		}
	}

	public SessionAccessLock AccessLock => accessLock;

	public int CopyFromCompany { get; set; }

	public CompanyDeletionType CompanyDeletionMode { get; set; }

	public PageBackgroundTask PageBackgroundTask
	{
		get
		{
			return pageBackgroundTask;
		}
		set
		{
			pageBackgroundTask = value;
		}
	}

	public Stopwatch ServiceOperationStopwatch => serviceOperationStopwatch ?? (serviceOperationStopwatch = new Stopwatch());

	public IEnumerable<ConnectionWarning> Warnings => warnings;

	public bool ClientIs64BitProcess { get; set; }

	internal ISystemEncryptionProvider EncryptionProvider { get; set; }

	internal IList<NavCodeunit> EventBindings => eventBindings;

	internal bool IsDiagnosticsMode { get; set; }

	internal NavTokenCacheBase NavTokenCache { get; set; }

	internal NavTaskScheduleActionList UncommittedTaskScheduleActions { get; } = new NavTaskScheduleActionList();

	public bool IsIntelligentCloudDataAccessBlockingEnabled
	{
		get
		{
			if (Database.DatabaseType == NavDatabaseType.Tenant)
			{
				return !ExecutionContext.In(Microsoft.Dynamics.Nav.Types.ExecutionContext.Install, Microsoft.Dynamics.Nav.Types.ExecutionContext.Uninstall, Microsoft.Dynamics.Nav.Types.ExecutionContext.Upgrade);
			}
			return false;
		}
	}

	public CultureInfo RuntimeLanguage
	{
		get
		{
			int num = (IsOpen ? GlobalLanguage : NavEnvironment.DefaultLanguage);
			if (lastCultureInfo == null || num != lastLanguage)
			{
				lastCultureInfo = CultureInfo.GetCultureInfo(num);
				lastLanguage = num;
			}
			return lastCultureInfo;
		}
	}

	public bool PermissionSystemActivated
	{
		get
		{
			if (!accessControlDatabaseCalled)
			{
				if (Database.NavUsers.IsEmpty)
				{
					return false;
				}
				NCLMetaTable metaTableById = NCLMetadata.GetMetaTableById(2000000053);
				ExistsCacheRequest request = new ExistsCacheRequest(0, metaTableById, cacheable: false, FiltersAndMarks.Empty, SecurityFiltering.Ignored, forceNonLockingProviderRequest: false);
				DataAccess dataAccessForTable = DataAccessSource.GetDataAccessForTable(metaTableById);
				permissionEngineActivated = dataAccessForTable.Exists(request);
				accessControlDatabaseCalled = true;
			}
			return permissionEngineActivated;
		}
	}

	public bool AdminSession { get; set; }

	internal TenantLicenseState TenantLicenseState { get; private set; }

	internal static Version PlatformVersion
	{
		get
		{
			if (platformVersion == null)
			{
				platformVersion = VersionCompatibility.AssemblyFileVersion;
			}
			return platformVersion;
		}
	}

	public virtual NavTenant Tenant => tenant;

	internal string ExclusiveAccessTicket { get; set; }

	public NavSystemTenant SystemTenant => systemTenant;

	public NavDatabase Database => Tenant.Database;

	public NavDatabase AppDatabase => SystemTenant.Database;

	public NCLMetadata NCLMetadata => SystemTenant.NCLMetadata;

	public MetadataProvider MetadataProvider => SystemTenant.MetadataProvider;

	public NavCompany Company
	{
		get
		{
			return company ?? (company = new NavCompany(this, null, evaluationCompany: false));
		}
		internal set
		{
			company = value;
		}
	}

	public string CompanyName
	{
		get
		{
			if (company == null)
			{
				return string.Empty;
			}
			return company.CompanyName;
		}
	}

	public string ApplicationAreas
	{
		get
		{
			return applicationAreaString;
		}
		set
		{
			if (value == null)
			{
				value = string.Empty;
			}
			applicationAreaString = value;
			applicationAreas = ((value.Length != 0) ? ("#All," + value).Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries) : new string[1] { "#All" });
			applicationAreaCache.Clear();
			if (CallbackAllowed && allowClientCacheFlush && !TestExecution.InTest)
			{
				ClientCallback.ClearClientMetadataCache();
			}
		}
	}

	public NavUserPermissions Permissions => permissions;

	public NavAppDataArchiveTenantUpgradeManager NavAppUpgradeSessionManager { get; set; }

	internal NavAppUpgradeManager NavAppUpgradeManager { get; set; }

	internal DesignerLevels CurrentDesignerLevel { get; set; }

	internal NavDesignerExtension CurrentDesignerExtension { get; set; }

	internal Compilation DesignerCompilation { get; set; }

	public NavAppInstallationContext AppInstallationContext { get; private set; }

	public AppSyncContext AppSyncContext { get; private set; }

	public NavAppUpgradeContext AppUpgradeContext { get; private set; }

	public INavErrorReporter IsolatedEventErrorReporter { get; private set; }

	public NavMetadataAccessContext RemovedMetadataAccessContext { get; private set; }

	public NavRunTableContext RunTableContext { get; private set; }

	public NavAppGroup NavAppGroup => AppInstallationContext?.TargetGroup ?? AppUpgradeContext?.TargetGroup ?? AppSyncContext?.TargetGroup ?? tenant.NavAppGroup;

	internal bool IsEventSessionRecorderEnabled { get; set; }

	internal NavEventTelemetryRegistrar EventTelemetryRegistrar { get; private set; }

	internal HashSet<int> StalePages
	{
		get
		{
			if (stalePages == null)
			{
				stalePages = new HashSet<int>();
			}
			return stalePages;
		}
	}

	private static CultureInfo GetFormattingCulture(ClientSettings clientFormats)
	{
		return new CultureInfo(clientFormats.WindowsLCID)
		{
			DateTimeFormat = 
			{
				ShortDatePattern = clientFormats.ShortDatePattern,
				ShortTimePattern = clientFormats.ShortTimeFormat,
				AMDesignator = clientFormats.TimeAMDesignator,
				PMDesignator = clientFormats.TimePMDesignator,
				TimeSeparator = clientFormats.TimeSeparator,
				DateSeparator = clientFormats.DateSeparator
			},
			NumberFormat = 
			{
				NumberDecimalSeparator = clientFormats.DecimalSeparator,
				NumberGroupSeparator = clientFormats.GroupSeparator,
				CurrencyDecimalSeparator = clientFormats.CurrencyDecimalSeparator,
				CurrencyGroupSeparator = clientFormats.CurrencyGroupSeparator
			}
		};
	}

	private void SetAndValidateLicense(byte[] licenseToUse)
	{
		if (licenseToUse != null)
		{
			privateLicense = licenseToUse;
		}
		License.CheckIsValid();
		if (!License.IsProgramVersionValid)
		{
			string message = string.Format(CultureInfo.CurrentCulture, Lang.ProgramLicenseIncompatible, ProductNames.ProductNameShort);
			LogWriterHelper.LogEvent(EventLogEntryType.Warning, message);
			if (clientConnectionType == ConnectionType.ManagementClient)
			{
				ClientCallback.DialogMessage(message, NavWindowIdHelper.MessageDefaultId);
			}
		}
	}

	private UserSettings GetPersonalization()
	{
		UserSettings userSettings = new UserSettings
		{
			LCID = 0,
			CompanyName = string.Empty
		};
		using (new MaximizedPermissionScope(this, new int[1] { 2000000073 }))
		{
			using NavRecord navRecord = new NavRecord(this, 2000000073, SecurityFiltering.Ignored);
			if (navRecord.ALGet(DataError.TrapError, new NavGuid(User.Id)))
			{
				userSettings.LCID = navRecord.GetFieldValue(12).ToInt32();
				userSettings.LocalId = navRecord.GetFieldValue(27).ToInt32();
				if (userSettings.LocalId == 0)
				{
					userSettings.LocalId = userSettings.LCID;
				}
				userSettings.CompanyName = navRecord.GetFieldValue(15).ToString();
				string value = navRecord.GetFieldValue(30).ToString();
				if (!string.IsNullOrEmpty(value))
				{
					userSettings.TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(value);
				}
			}
			else
			{
				string defaultCompany = tenant.TenantSettings.DefaultCompany;
				if (GetAllowedCompanies().Contains(defaultCompany))
				{
					userSettings.CompanyName = defaultCompany;
					SavePersonalization(0, NavEnvironment.DefaultLanguage, overrideLocaleIdInUserProfile: true, defaultCompany);
				}
			}
		}
		return userSettings;
	}

	private static void NavBaseExceptionExceptionCreated(object sender, ExceptionCreatedEventArgs e)
	{
		if (NavCurrentThread.Session != null && !NavCurrentThread.Session.IsDiagnosticsMode)
		{
			NavCurrentThread.Session.LastException = e.CreatedException;
		}
	}

	internal IEnumerable<CallStackElement> BuildCurrentStack()
	{
		if (CurrentMethodScope != null)
		{
			List<CallStackElement> list = new List<CallStackElement>(CurrentMethodScope.StackDepth);
			foreach (NavMethodScope item in CurrentMethodScope.StackTrace)
			{
				if (item.ApplicationObject == null)
				{
					break;
				}
				list.Add(new CallStackElement(item));
			}
			return list;
		}
		return null;
	}

	public void VerifyExecutePermission(ApplicationObjectId objectId)
	{
		if (verifyExecutePermissionCacheBumperToken != Database.PermissionSetupMonitor.SetupVersion)
		{
			verifyExecutePermissionCache.Clear();
			verifyExecutePermissionCacheBumperToken = Database.PermissionSetupMonitor.SetupVersion;
		}
		else if (verifyExecutePermissionCache.Contains(objectId))
		{
			return;
		}
		GetPermissionSet(null, Database.GetCompanyNameToken(CompanyName), objectId).VerifyPermissions(null, PermissionMask.Execute, LimitedUserPermissionChecker);
		verifyExecutePermissionCache.Add(objectId);
	}

	public void VerifyExecutePermission(NavApplicationObjectBase applicationObject)
	{
		if (applicationObject.ObjectId.ObjectType == ObjectType.CodeUnit && applicationObject.ObjectId.ObjectNumber >= 2000000001)
		{
			return;
		}
		if (applicationObject.ExecutePermissionsValidated)
		{
			if (!TestExecution.EffectivePermissionsInUse)
			{
				return;
			}
			if (TestExecution.InTest)
			{
				NavForm obj = applicationObject as NavForm;
				if (obj != null && obj.IsRequestPage)
				{
					return;
				}
			}
		}
		if (!(applicationObject is NavRecord { IsTemporary: not false }))
		{
			VerifyExecutePermission(applicationObject.ObjectId);
			if (!NavTestExecution.UseEffectivePermissions())
			{
				applicationObject.ExecutePermissionsValidated = true;
			}
		}
	}

	public bool HasExecutePermission(ApplicationObjectId objectId)
	{
		return GetPermissionSet(null, Database.GetCompanyNameToken(CompanyName), objectId).HasPermissions(null, PermissionMask.Execute, LimitedUserPermissionChecker);
	}

	public bool HasExecutePermission(NavApplicationObjectBase applicationObject)
	{
		if (applicationObject.ExecutePermissionsValidatedEx.HasValue)
		{
			return applicationObject.ExecutePermissionsValidatedEx.Value;
		}
		if (applicationObject is NavRecord { IsTemporary: not false })
		{
			applicationObject.ExecutePermissionsValidatedEx = true;
			return true;
		}
		bool flag = HasExecutePermission(applicationObject.ObjectId);
		applicationObject.ExecutePermissionsValidatedEx = flag;
		return flag;
	}

	public IPermissionSet GetPermissionSet(NavApplicationObjectBase callingObject, IEnumerable<ApplicationObjectId> applicationObjects)
	{
		return GetPermissionSet(callingObject, Database.GetCompanyNameToken(CompanyName), applicationObjects);
	}

	public IPermissionSet GetPermissionSet(NavApplicationObjectBase callingObject, int companyNameToken, ApplicationObjectId applicationObject)
	{
		if (companyNameToken != 0)
		{
			return permissions.GetPermissionSet(companyNameToken, callingObject, applicationObject);
		}
		return permissions.GetPermissionSet(Database.GetCompanyNameToken(CompanyName), callingObject, applicationObject);
	}

	internal FilterFieldDictionary GetSecurityFilters(int companyNameToken, int tableId, SecurityFilterType securityFilterType, NavApplicationObjectBase callingObject, NavApplicationObjectBase securableObject)
	{
		if (!Database.SecurityAndLicense.IsPermissionSystemEnabled() || !Database.SecurityAndLicense.IsSecurityFiltersActive)
		{
			return FilterFieldDictionary.Empty;
		}
		if (companyNameToken != 0)
		{
			return permissions.GetSecurityFilters(companyNameToken, tableId, securityFilterType, callingObject, securableObject);
		}
		return permissions.GetSecurityFilters(Database.GetCompanyNameToken(CompanyName), tableId, securityFilterType, callingObject, securableObject);
	}

	public IPermissionSet GetPermissionSet(NavApplicationObjectBase callingObject, int companyNameToken, IEnumerable<ApplicationObjectId> applicationObjects)
	{
		if (companyNameToken != 0)
		{
			return permissions.GetPermissionSet(companyNameToken, callingObject, applicationObjects);
		}
		return permissions.GetPermissionSet(Database.GetCompanyNameToken(CompanyName), callingObject, applicationObjects);
	}

	public void MaximizePermissions(int[] tableObjects)
	{
		permissions.MaximizePermissions(tableObjects);
	}

	public void RemoveMaximizedPermissions(int[] tableObjects)
	{
		permissions.RemoveMaximizedPermissions(tableObjects);
	}

	public static void Sleep(int duration)
	{
		NavCurrentThread.Session.AccessLock.SleepButWakeOnStopping(duration);
	}

	internal static void LicenseExpiryHandler(ushort objType, int objNo, bool isLicenseFile, bool isWarning, NavDate expiryDate, NavDate today, string expireText)
	{
		if (NavCurrentThread.Session != null)
		{
			NavCurrentThread.Session.LicenseHandler((ObjectType)objType, objNo, isLicenseFile, isWarning, expiryDate, today, expireText);
		}
	}

	private void LicenseHandler(ObjectType objType, int objNo, bool isLicenseFile, bool isWarning, NavDate expiryDate, NavDate today, string expireText)
	{
		if (isWarning)
		{
			IClientCallback clientCallbackOrNull = NavCurrentThread.Session.ClientCallbackOrNull;
			if (clientCallbackOrNull == null)
			{
				return;
			}
			int num = expiryDate - today;
			string message;
			if (isLicenseFile)
			{
				message = string.Format(CultureInfo.CurrentCulture, Lang.LicenseExpiryWarning, num.ToString(CultureInfo.CurrentCulture));
			}
			else
			{
				ApplicationObjectId item = new ApplicationObjectId(objType, objNo);
				if (alreadyWarned.Contains(item))
				{
					return;
				}
				alreadyWarned.Add(item);
				message = (string.IsNullOrEmpty(expireText) ? string.Format(CultureInfo.CurrentCulture, Lang.ObjectLicenseExpiryWarning, num.ToString(CultureInfo.CurrentCulture), objType.ToString(), objNo.ToString(CultureInfo.CurrentCulture)) : string.Format(CultureInfo.CurrentCulture, Lang.ModuleLicenseExpiryWarning, num.ToString(CultureInfo.CurrentCulture), expireText));
			}
			clientCallbackOrNull.DialogMessage(message, NavWindowIdHelper.MessageDefaultId);
			return;
		}
		string message2;
		if (isLicenseFile)
		{
			message2 = Lang.LicenseExpiredError;
		}
		else
		{
			message2 = (string.IsNullOrEmpty(expireText) ? string.Format(CultureInfo.CurrentCulture, Lang.ObjectLicenseExpiredError, objType.ToString(), objNo.ToString(CultureInfo.CurrentCulture)) : string.Format(CultureInfo.CurrentCulture, Lang.ModuleLicenseExpiredError, expireText));
		}
		throw new NavLicenseViolationException(message2);
	}

	internal void ValidateSessionIsActive(bool forceValidation = false)
	{
		if (forceValidation || (!AdminSession && !skipInsertSessionEventRecord && !(earliestNextActiveSessionExistValidation >= DateTime.UtcNow)))
		{
			earliestNextActiveSessionExistValidation = DateTime.UtcNow.AddMinutes(2.0);
			if (!SessionEventTableHandler.IsSessionActive(this))
			{
				throw DiagnosticsResolver.GetMostSpecificInstance(this).SendExceptionTag("000001W", Category.Security, new NavServerSessionRemovedException(string.Format(CultureInfo.CurrentCulture, Lang.SessionIsNoLongerActive, Id)));
			}
		}
	}

	internal void CheckConnectionIsOpen()
	{
		if (!IsOpen)
		{
			throw new NavNCLConnectionNotOpenedException();
		}
	}

	internal INavService CreateNavTestService()
	{
		if (serviceConnection == null)
		{
			throw new NotSupportedException();
		}
		return serviceConnection.CreateNavTestService();
	}

	internal void SetServerFormRequestData(NavForm form, NavFormRuntimeParameters parameters, FormRunData data)
	{
		if (serviceConnection == null)
		{
			throw new NotSupportedException();
		}
		serviceConnection.SetServerFormRequestData(form, parameters, data);
	}

	internal FormCloseRequest MakeFormCloseRequest(NavForm form)
	{
		if (serviceConnection == null)
		{
			throw new NotSupportedException();
		}
		return serviceConnection.MakeFormCloseRequest(form);
	}

	protected override void Dispose(bool disposing)
	{
		DisposeInitiated = true;
		DisposeAsync().WaitThrowingOriginalException();
	}

	public async Task DisposeAsync(ShutdownScope shutdownScope = ShutdownScope.SingleSession, bool notifyClient = false, bool abortChannel = false, bool alwaysAsync = false, bool externalStopRequest = false)
	{
		DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00000KT", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Session DisposeAsync -- SessionId:{0}; AlwaysAsync:{1}", Id, alwaysAsync);
		DisposeEventSessionRecorder();
		if (ClientConnectionType != ConnectionType.Background && Authenticator != null && Authenticator.IsDeviceUser)
		{
			DecrementDeviceUserCount();
		}
		if (AccessLock == null)
		{
			return;
		}
		if (!AccessLock.SetStoppingOrDisposedIfPossible(externalStopRequest, out var areThreadsExecuting))
		{
			DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00000KU", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Session DisposeAsync awaiting accesslock -- SessionId:{0};", Id);
			await AccessLock.WaitForDisposedAsync();
			return;
		}
		try
		{
			if (exclusiveTenantActionScope != null && !exclusiveTenantActionScope.IsDisposed)
			{
				try
				{
					exclusiveTenantActionScope.Dispose();
				}
				catch (NavNCLExclusiveActionException exception)
				{
					DiagnosticsResolver.GetMostSpecificInstance(this).SendExceptionTag("000011Z", Category.Tenant, exception);
				}
			}
			if (ClientStopExecutionListener != null)
			{
				ClientStopExecutionListener.CheckForDeactivate(this);
				ClientStopExecutionListener = null;
			}
			if (serviceConnection != null)
			{
				serviceConnection.OnSessionTerminating(notifyClient, abortChannel);
			}
			switch (shutdownScope)
			{
			case ShutdownScope.SingleTenant:
				SessionComment = Lang.StopSessionByTenantDismountComment;
				break;
			case ShutdownScope.Server:
				SessionComment = Lang.StopSessionByServerShutdownComment;
				break;
			}
			if (areThreadsExecuting)
			{
				using (SessionShutdownExecutionListener.SetupSessionShutdownExecutionListener(this, shutdownScope))
				{
					using (DiagnosticsResolver.GetMostSpecificInstance(this).CreateTraceScope("000021R", DiagnosticParameter.SystemMetadata("Setup session shutdown execution listener"), MonitorType.None, Category.Runtime))
					{
						await AccessLock.WaitUntilExecutionStoppedAndSetDisposingAsync();
					}
				}
			}
			if (alwaysAsync)
			{
				await NavTaskFactory.RunTask((Action)InnerDispose);
			}
			else
			{
				InnerDispose();
			}
		}
		finally
		{
			AccessLock.SetDisposed();
		}
	}

	private void DecrementDeviceUserCount()
	{
		LicenseDetails userLicenseDetails = Authenticator.UserLicenseDetails;
		string text = ((userLicenseDetails != null) ? ((UserInfo)userLicenseDetails.User).ObjectId : null);
		int value = -1;
		if (text != null && Tenant.ConcurrentDeviceUsers.TryGetValue(text, out value))
		{
			bool flag = ((value != 1) ? Tenant.ConcurrentDeviceUsers.TryUpdate(text, value - 1, value) : Tenant.ConcurrentDeviceUsers.TryRemove(text, out var _));
			DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00009GV", Category.Runtime, Verbosity.Normal, "Session DisposeAsync removing device session entry from dictionary -- user:{0}; sessions:{1}; status={2}", DiagnosticParameter.Eupi(text), DiagnosticParameter.SystemMetadata(value), DiagnosticParameter.SystemMetadata(flag));
		}
	}

	private void InnerDispose()
	{
		NavSession session = NavCurrentThread.Session;
		NavCurrentThread.Session = this;
		string empty = string.Empty;
		using (DiagnosticsResolver.GetMostSpecificInstance(this).CreateTimingScope("000002C", Category.Runtime, DiagnosticParameter.SystemMetadata(("ServerSession dispose called ID=" + Id + Environment.NewLine + empty).Trim())))
		{
			try
			{
				Exception firstCaughtException = null;
				try
				{
					CloseCompany();
				}
				catch (NavBaseException ex)
				{
					firstCaughtException = ex;
				}
				ShutdownDebuggingEnvironment(ref firstCaughtException);
				int num = 0;
				while (currentSqlConnectionScope != null)
				{
					if (num >= 10)
					{
						DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("000007O", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, ("NavSession broken SQL connection scope dispose. SessionId:" + Id + Environment.NewLine + empty).Trim());
						break;
					}
					try
					{
						currentSqlConnectionScope.Dispose();
					}
					catch (Exception ex2)
					{
						if (firstCaughtException == null)
						{
							firstCaughtException = ex2;
						}
					}
					num++;
				}
				try
				{
					if (hasBeenOpened && !skipInsertSessionEventRecord)
					{
						SessionEventTableHandler.InsertSessionEventRecord(1, this);
					}
				}
				catch (Exception ex3)
				{
					if (firstCaughtException == null)
					{
						firstCaughtException = ex3;
					}
				}
				try
				{
					if (license != null)
					{
						license.Dispose();
						license = null;
					}
				}
				catch (Exception ex4)
				{
					if (firstCaughtException == null)
					{
						firstCaughtException = ex4;
					}
				}
				try
				{
					base.Dispose(disposing: true);
				}
				catch (Exception ex5)
				{
					if (firstCaughtException == null)
					{
						firstCaughtException = ex5;
					}
				}
				firstCaughtException?.Rethrow();
			}
			finally
			{
				NavCurrentThread.Session = session;
				selfReference.ClearTarget();
				if (hasBeenOpened && !isSystemSession)
				{
					NavEnvironment.Instance.PerformanceCounterSetter.DecrementNumberOfActiveSessions();
					NavServerEventSource.Log.WriteSessionClosedEvent(tenant.Id, Id, UserDisplayName, clientConnectionType.ToString(), NavEnvironment.ServerInstanceName);
					hasBeenOpened = false;
				}
				DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("000002D", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, ("ServerSession disposed ID=" + Id + Environment.NewLine + empty).Trim());
				diagnostics = null;
			}
		}
	}

	private void ShutdownDebuggingEnvironment(ref Exception firstCaughtException)
	{
		try
		{
			SingleInstanceDebugRuntime.Instance?.EnsureDetachedFromConnection(this);
			if (IsDebugging())
			{
				if (!string.IsNullOrEmpty(DebuggingContext))
				{
					NavEnvironment.Instance.DebugRuntimeManager.DetachSession(DebuggingContext, this);
				}
				ALDebugger.Instance?.Dispose();
			}
		}
		catch (Exception ex)
		{
			if (firstCaughtException == null)
			{
				firstCaughtException = ex;
			}
		}
	}

	internal void TestReportLanguagePermission(int lcid)
	{
		lock (allowedReportLanguages)
		{
			if (!allowedReportLanguages.Contains(lcid))
			{
				if (!License.HasLanguagePermission(lcid, LanguagePermission.Report))
				{
					throw new NavLicenseViolationException(string.Format(CultureInfo.CurrentCulture, Lang.LicenseLangPermError, lcid));
				}
				allowedReportLanguages.Add(lcid);
			}
		}
	}

	private static string GetErrorCode(Exception exception)
	{
		string text = exception.GetType().Name;
		if (exception is NavBaseException)
		{
			if (text.StartsWith("NavNcl", StringComparison.OrdinalIgnoreCase))
			{
				text = text.Substring(6);
			}
			else if (text.StartsWith("Nav", StringComparison.Ordinal))
			{
				text = text.Substring(3);
			}
		}
		if (text.EndsWith("Exception", StringComparison.Ordinal))
		{
			text = text.Substring(0, text.Length - 9);
		}
		return text;
	}

	private static string FormatLastErrorCode(int errorCode)
	{
		switch (errorCode)
		{
		case 67108864:
			return "DB:AllowableCode";
		case 22926086:
			return "NDBCS:BadBlobMagic";
		case 22928070:
			return "NDBCS:BadCSideCodeData";
		case 22928068:
			return "NDBCS:BadCSideData";
		case 22928072:
			return "NDBCS:BadSqlIntCodeData";
		case 18023770:
			return "DB:CannotChangeObjectType";
		case 18023779:
			return "DB:CannotChangeTransactionType";
		case 18023809:
			return "DB:ClientDeleteDenied";
		case 18023808:
			return "DB:ClientInsertDenied";
		case 18023810:
			return "DB:ClientModifyDenied";
		case 18023807:
			return "DB:ClientReadDenied";
		case 22924076:
			return "NDBCS:ConnectionLost";
		case 22926089:
			return "NDBCS:Deadlock";
		case 85132273:
			return "DB:RecordExists";
		case 16777216:
			return "DB:ErrorCode";
		case 19464333:
			return "Eval:RangeO";
		case 18023698:
			return "DB:ExecuteDenied";
		case 18023703:
			return "DB:ExecuteObjDenied";
		case 33554432:
			return "DB:FatalCode";
		case 18023644:
			return "DB:ValueIsFilterNotStr";
		case 23004244:
			return "NCLCSRTS:FieldTypeIncompatible";
		case 18023443:
			return "DB:TypeMisMatch";
		case 18023495:
			return "DB:NothingInsideFilter";
		case 18023675:
			return "DB:FlowFieldNotBoolean";
		case 18023676:
			return "DB:FlowFieldNotInteger";
		case 18023729:
			return "DB:IllegalCompanyName";
		case 22928069:
			return "NDBCS:IncompatibleSqlType";
		case 18023424:
			return "DB:BadSearchMethod";
		case 85132271:
			return "DB:KeyNotFound";
		case 23397458:
			return "AL:LockTableBadParm";
		case 22926090:
			return "NDBCS:LockTimeout";
		case 18022407:
			return "DB:BWTMissing";
		case 18023796:
			return "DB:ModifyOnIdentityField";
		case 101:
			return "DB:ModuleALRun";
		case 19:
			return "DB:ModuleDB";
		case 23:
			return "DB:ModuleDBDataError";
		case 41:
			return "DB:ModuleEval";
		case 95:
			return "DB:ModuleNCLCSRT";
		case 93:
			return "DB:ModuleNdbcs";
		case 65536:
			return "DB:MsgMsgCode";
		case 18023430:
			return "DB:FieldIsNotCFormula";
		case 18023434:
			return "DB:NotIntervalFilter";
		case 18023435:
			return "DB:NoFilter";
		case 18023772:
			return "DB:TransActionIsReadOnly";
		case 22923076:
			return "NDBCS:NoWaitNotSupported";
		case 0:
			return "DB:Ok";
		case 18023494:
			return "DB:FormelFieldNoFormel";
		case 85132286:
			return "DB:RecordChanged";
		case 85132272:
			return "DB:RecordNotFound";
		case 18023402:
			return "DB:PrimRecordNotFound";
		case 22926068:
			return "NDBCS:SqlObjectNotFound";
		case 18023674:
			return "DB:SumOnNoneNumeric";
		case 23004238:
			return "NCLCSRTS:TableEqualValue";
		case 23004237:
			return "NCLCSRTS:TableErrorStr";
		case 18023508:
			return "DB:TableInConsistent";
		case 18023438:
			return "DB:TableIsEmpty";
		case 23396552:
			return "AL:ToManyKeyFields";
		default:
		{
			int num = (errorCode >> 16) & 0xFF;
			int num2 = errorCode & 0x7FFF;
			return string.Format(CultureInfo.InvariantCulture, "{0}@{1}", num, num2);
		}
		}
	}

	internal NavDotNet GetLastErrorObject(ITreeObject parent)
	{
		return new NavDotNet(parent, lastException, runOnClient: false);
	}

	private static string GetCallstack(IEnumerable<CallStackElement> callstack, string stackElementDelimiter, Func<NavApplicationObjectBase, string> getApplicationObjectName)
	{
		if (callstack == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (CallStackElement item in callstack)
		{
			item.GetText(stringBuilder, getApplicationObjectName);
			stringBuilder.Append(stackElementDelimiter);
		}
		return stringBuilder.ToString();
	}

	internal string GetLastErrorCallstack()
	{
		return GetCallstack(lastErrorCallstack, "\\", (NavApplicationObjectBase appObject) => appObject.ObjectName);
	}

	internal string GetCurrentCallstack()
	{
		return GetCallstack(BuildCurrentStack(), Environment.NewLine, (NavApplicationObjectBase appObject) => appObject.ObjectName);
	}

	internal string GetCurrentCallstack(Func<NavApplicationObjectBase, string> objectNameGetter)
	{
		return GetCallstack(BuildCurrentStack(), Environment.NewLine, objectNameGetter);
	}

	public bool TrySetRootDialog(NavDialog dialog)
	{
		if (rootDialog == null)
		{
			rootDialog = dialog;
			return true;
		}
		return false;
	}

	public bool DialogAllowed(NavDialog dialog)
	{
		if (NavCurrentThread.Session.CallbackAllowed)
		{
			if (rootDialog != null)
			{
				return rootDialog == dialog;
			}
			return true;
		}
		return false;
	}

	public bool ReleaseRootDialog(NavDialog dialog)
	{
		if (rootDialog == dialog)
		{
			rootDialog = null;
			return true;
		}
		return false;
	}

	public void FillReturnRequests(ServerResponse response)
	{
		Queue<ServerCallbackRequest> queue = requestsToClient;
		if (queue == null)
		{
			return;
		}
		foreach (ServerCallbackRequest item in queue)
		{
			response.Requests.Enqueue(item);
		}
		queue.Clear();
	}

	public Microsoft.Dynamics.Nav.Types.ExecutionContext GetCurrentModuleExecutionContext()
	{
		Guid moduleId = ((WrappedValue<Guid>)(object)NavCurrentThread.Session?.CurrentMethodScope?.ApplicationObject?.GetOwningAppId())?.Value ?? Guid.Empty;
		return GetModuleExecutionContext(moduleId);
	}

	public Microsoft.Dynamics.Nav.Types.ExecutionContext GetModuleExecutionContext(Guid moduleId)
	{
		NavAppInstallationContext appInstallationContext = AppInstallationContext;
		if (appInstallationContext != null && ((WrappedValue<Guid>)(object)appInstallationContext.AppBeingTargeted.AppId).Value == moduleId)
		{
			if (!AppInstallationContext.IsInstalling)
			{
				return Microsoft.Dynamics.Nav.Types.ExecutionContext.Uninstall;
			}
			return Microsoft.Dynamics.Nav.Types.ExecutionContext.Install;
		}
		NavAppUpgradeContext appUpgradeContext = AppUpgradeContext;
		if (appUpgradeContext != null && ((WrappedValue<Guid>)(object)appUpgradeContext.AppBeingTargeted.AppId).Value == moduleId)
		{
			return Microsoft.Dynamics.Nav.Types.ExecutionContext.Upgrade;
		}
		NavDataUpgradeManager navDataUpgradeManager = NavCurrentThread.Session?.Database?.UpgradeManager;
		if (navDataUpgradeManager != null && navDataUpgradeManager.GetUpgradeInformation().State == NavWorkflowState.InProgress && (moduleId == Guid.Empty || navDataUpgradeManager.ExtensionUpgradeContexts.Any((NavAppJointUpgradeContext c) => ((WrappedValue<Guid>)(object)c.AppBeingTargeted.AppId).Value == moduleId)))
		{
			return Microsoft.Dynamics.Nav.Types.ExecutionContext.Upgrade;
		}
		return Microsoft.Dynamics.Nav.Types.ExecutionContext.Normal;
	}

	static NavSession()
	{
		OpenCompanyDurationMetric = new NavMetric6Dimensions("OpenCompanyDuration", "CompanyName", "TenantId", "Result", "PlatformVersion", "ApplicationVersion", "ApplicationBuild");
		NavBaseException.ExceptionCreated += NavBaseExceptionExceptionCreated;
	}

	protected NavSession(NavTenant tenant)
		: base(tenant)
	{
		if (!FeatureSwitches.Instance.DisableTenantStateCheckOnSessionCreate && !tenant.CanCreateSession())
		{
			throw tenant.Diagnostics.SendExceptionTag("00001CG", Category.Runtime, NavTenantNotAccessibleException.Create(tenant.Id));
		}
	}

	internal NavSession(NavTenant tenant, TimeZoneInfo clientTimeZone, Func<NavSession, DataAccessSource> dataAccessSourceCreator = null, string appId = null)
		: this(tenant, clientTimeZone, new NavUserAuthentication(tenant, NavClientCredentialType.Windows), ExecutionMode.Standard, dataAccessSourceCreator, null, isSystemSession: false, -1, appId)
	{
	}

	public NavSession(NavTenant tenant, TimeZoneInfo clientTimeZone, NavUserAuthentication navUserAuthentication, string appId)
		: this(tenant, clientTimeZone, navUserAuthentication, ExecutionMode.Standard, null, null, isSystemSession: false, -1, appId)
	{
	}

	internal NavSession(NavTenant tenant, TimeZoneInfo clientTimeZone, NavUserAuthentication navUserAuthentication, ExecutionMode executionMode = ExecutionMode.Standard, Func<NavSession, DataAccessSource> dataAccessSourceCreator = null, SqlProfilerCommentProvider sqlProfilerCommentProvider = null, bool isSystemSession = false, int newSessionId = -1, string appId = null, ConnectionType connectionType = ConnectionType.UnknownClient)
		: base(tenant)
	{
		if (clientTimeZone == null)
		{
			throw new ArgumentNullException("clientTimeZone");
		}
		if (navUserAuthentication == null)
		{
			throw new ArgumentNullException("navUserAuthentication");
		}
		if (!FeatureSwitches.Instance.DisableTenantStateCheckOnSessionCreate && !tenant.CanCreateSession(isSystemSession || !navUserAuthentication.IsNormalUserType || connectionType == ConnectionType.ManagementClient))
		{
			throw tenant.Diagnostics.SendExceptionTag("00001CH", Category.Runtime, NavTenantNotAccessibleException.Create(tenant.Id));
		}
		id = ((newSessionId == -1) ? NextSessionId : newSessionId);
		selfReference = new Reference<NavSession>(this);
		this.tenant = tenant;
		systemTenant = NavEnvironment.Instance.Tenants.SystemTenant;
		this.isSystemSession = isSystemSession;
		clientConnectionType = connectionType;
		Authenticator = navUserAuthentication;
		string text = User?.AuthenticationObjectId;
		diagnostics = NavDiagnostics.CreateScopedDiagnostics(this.tenant.Diagnostics, uniqueSessionId, MonitorType.None, singleThreadAccess: true, (this.tenant == null) ? string.Empty : this.tenant.TenantSettings.ConcatenatedAlternateId, text);
		AppId = appId;
		NavCurrentThread.ClientActivityId = Authenticator?.ClientActivityId ?? Guid.Empty;
		diagnostics.SendTraceTag("000002E", Category.Runtime, Verbosity.Normal, "NavSession Created, sessionId:{0}, aadTenantId:{1}, IsSystem:{2}, appId:{3}", DiagnosticParameter.SystemMetadata(Id), DiagnosticParameter.Oii(this.tenant?.TenantSettings?.AadTenantId ?? string.Empty), DiagnosticParameter.SystemMetadata(isSystemSession), DiagnosticParameter.SystemMetadata(AppId));
		if (!isSystemSession && tenant.TenantType == NavTenantType.Normal && User != null)
		{
			TimeSpan value = ServerUserSettings.Instance.ExtensionsUsageReportingInterval.Value;
			string navTenantId = this.tenant.Id;
			Guid navUserId = User.Id;
			string aadTenantId = this.tenant.TenantSettings.AadTenantId;
			navAppUsageTracker = new NavAppUsageTracker(diagnostics, navTenantId, navUserId, aadTenantId, text, value);
		}
		this.executionMode = executionMode;
		ClientTimeZone = clientTimeZone;
		dataAccessSource = ((dataAccessSourceCreator == null) ? new DataAccessSource(this, this.tenant.TenantSettings.AllowAppDatabaseWrite || ExecutionMode == ExecutionMode.Debug, !ServerUserSettings.Instance.DisableDataCache.Value) : dataAccessSourceCreator(this));
		CurrentMethodScope = NavMethodScope.CreateRootMethodScope(this);
		EarliestAllowedFromRecordCache = DateTime.MinValue;
		SqlProfilerCommentProvider = sqlProfilerCommentProvider ?? new SqlProfilerCommentProvider();
		SqlProfilerCommentProvider.Initialize(this);
		LimitedUserPermissionChecker = ((User != null && User.LicenseType == LicenseType.Limited) ? new LimitedUserPermissionChecker() : null);
		accessLock = new SessionAccessLock(this);
		TableConnectionManager = new TableConnectionManager(this);
		UserPageMetadataCache = new CachingDatabaseUserPageMetadataRetriever(new DatabaseNavPageCustomizationRepository());
		UserPageMetadataCacheV1 = new CachingDatabaseUserPageMetadataRetrieverV1(new DatabaseNavV1PageConfigurationRepository());
		CompanyDeletionMode = (ServerUserSettings.Instance.UseIncrementalCompanyDelete.Value ? CompanyDeletionType.Incremental : CompanyDeletionType.Complete);
	}

	public IReference<NavSession> GetReference()
	{
		return selfReference;
	}

	public void FlushDataCache()
	{
		DataAccessSource.TableVersionTokens.SelectLatestVersion();
		if (company != null)
		{
			company.SystemCodeunitFactory.UIHelperTriggers.ResetCaptionClassCache();
		}
	}

	public void FlushAllDataAccessCaches()
	{
		dataAccessSource.TenantDataAccess.ClearCache(clearGlobal: true);
		dataAccessSource.AppDataAccess.ClearCache(clearGlobal: true);
		Database.SecurityAndLicense.ClearPermissions(clearLicenseInfo: true);
		Database.NavUsers.RefreshList();
	}

	public void Open()
	{
		Open(useUserPersonalization: true);
	}

	public void Open(bool allowAppsDisabledMode = false)
	{
		Open(useUserPersonalization: true, null, allowAppsDisabledMode);
	}

	public void Open(bool useUserPersonalization, byte[] licenseToUse = null, bool allowAppsDisabledMode = false)
	{
		if (!allowAppsDisabledMode)
		{
			NavEnvironment.Instance.ThrowIfInAppsDisabledMode("Cannot open this session when extensions are disabled globally.");
		}
		NavCurrentThread.Session = this;
		if (!IsSystemSession)
		{
			NavGlobal.AppDatabase.SqlDatabaseProperties.CheckDatabaseVersion();
			if (Database.DatabaseType == NavDatabaseType.Tenant)
			{
				NavSqlTenantPropertiesSync.CheckTenantDatabaseVersion(Database);
			}
			useUserPersonalization = (licenseToUse == null) & useUserPersonalization;
			SetAndValidateLicense(licenseToUse);
			if (ClientConnectionType != ConnectionType.Background && Authenticator.IsDeviceUser)
			{
				Authenticator.EnsureDeviceUserCanSignIn();
			}
			if (!Authenticator.Authenticated)
			{
				throw new NavDatabasePasswordException(Lang.AuthenticationFailed)
				{
					NavTenantId = Tenant?.Id
				};
			}
			NavFile.GetUserDirectoryAndEnsureExists(DataError.ThrowError, tempPath: false);
		}
		permissions = new NavUserPermissions(Authenticator.User, this);
		if (ExecutionMode == ExecutionMode.Debug)
		{
			ALDebugger.CheckPermissionToDebug();
		}
		DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00000Y3", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "NavSession Opened, sessionId:{0}, IsSystem:{1}, appId:{2}", id.ToString(CultureInfo.InvariantCulture), isSystemSession.ToString(), AppId));
		company = new NavCompany(this, null, evaluationCompany: false);
		int num = 0;
		CultureInfo cultureInfo = null;
		if (useUserPersonalization && Database.DatabaseType == NavDatabaseType.Tenant)
		{
			UserSettings personalization = GetPersonalization();
			if (personalization.LCID > 0)
			{
				if (WindowsLanguageDataProvider.IsAvailableLanguage(personalization.LCID))
				{
					num = personalization.LCID;
				}
				else
				{
					num = NavEnvironment.DefaultLanguage;
					SavePersonalization(num, personalization.LocalId, overrideLocaleIdInUserProfile: false);
				}
			}
			if (num == 0)
			{
				num = NavEnvironment.FindSupportedLanguage(CultureInfo.CurrentCulture.LCID, NavEnvironment.DefaultLanguage);
				if (num == NavEnvironment.DefaultLanguage && CultureInfo.CurrentCulture.LCID != NavEnvironment.DefaultLanguage)
				{
					cultureInfo = CultureInfo.CurrentCulture;
				}
			}
		}
		else
		{
			num = NavEnvironment.DefaultLanguage;
		}
		CultureInfo cultureInfo2 = CultureInfo.GetCultureInfo(num);
		ClientSettings clientSettings = default;
		clientSettings.Refresh(cultureInfo ?? cultureInfo2, cultureInfo2);
		currentLCIDAbbreviatedName = cultureInfo2.ThreeLetterWindowsLanguageName;
		cultureSettings = clientSettings;
		CurrentMethodScope = NavMethodScope.CreateRootMethodScope(this);
		skipInsertSessionEventRecord = isSystemSession || Tenant.LicenseValidator.CheckIfSessionIsAnActiveWebServiceSession(this) || AdminSession || Database.DatabaseType != NavDatabaseType.Tenant;
		if (!isSystemSession)
		{
			Tenant.LicenseValidator.ValidateLicenseForNewSession(this);
			if (!skipInsertSessionEventRecord)
			{
				tenant.PurgeOldTenantSessionEvents();
				SessionEventTableHandler.InsertSessionEventRecord(0, this);
				earliestNextActiveSessionExistValidation = DateTime.UtcNow.AddMinutes(2.0);
			}
			if (NavEnvironment.Instance.CacheSynchronizationController != null)
			{
				NavEnvironment.Instance.CacheSynchronizationController.StartCacheSynchronization();
			}
			if (tenant.TenantType == NavTenantType.Normal)
			{
				tenant.LogTenantAzureUsage();
			}
			NavEnvironment.Instance.PerformanceCounterSetter.IncrementNumberOfActiveSessions();
			NavServerEventSource.Log.WriteSessionOpenedEvent(tenant.Id, Id, UserDisplayName, clientConnectionType.ToString(), NavEnvironment.ServerInstanceName);
			tenant.OnNewSession();
			this.TransactionScope(() =>
			{
				Authenticator.ObtainUserAccessToken(this);
			});
		}
		hasBeenOpened = true;
		string empty = string.Empty;
		DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("000007R", Category.Runtime, Verbosity.Normal, "NavSession Open. SessionId:{0}, AllowAppsDisabledMode:{1}, ClientType:{2}, NAVAppId:{3}, AADAppId:{4}, NAVTenantId:{5}, AADTenantId:{6}, NAVUserId:{7}, AADUserId:{8}, UserType:{9}, UserAuthenticationEmail:{10}, UserContactEmail:{11}, UserApplicationID:{12}, isSystemSession:{13}, formatCulture:{14}, userCulture:{15}, lcid:{16}, Thread.CurrentCulture:{17}, StackTrace:{18}", DiagnosticParameter.SystemMetadata(Id), DiagnosticParameter.SystemMetadata(allowAppsDisabledMode), DiagnosticParameter.SystemMetadata(clientConnectionType), DiagnosticParameter.SystemMetadata(AppId), DiagnosticParameter.SystemMetadata(ServerUserSettings.Instance?.AzureActiveDirectoryClientApplicationId?.Value), DiagnosticParameter.Oii(tenant?.TenantSettings?.Id), DiagnosticParameter.Oii(tenant?.TenantSettings?.AadTenantId), DiagnosticParameter.Eupi(User?.Id), DiagnosticParameter.Eupi(User?.AuthenticationObjectId), DiagnosticParameter.SystemMetadata(Authenticator?.UserTypeDescription), DiagnosticParameter.Euii(User?.AuthenticationEmail), DiagnosticParameter.Euii(User?.ContactEmail), DiagnosticParameter.Euii(User?.ApplicationId), DiagnosticParameter.SystemMetadata(isSystemSession), DiagnosticParameter.SystemMetadata(cultureInfo), DiagnosticParameter.SystemMetadata(cultureInfo2), DiagnosticParameter.SystemMetadata(num), DiagnosticParameter.SystemMetadata(Thread.CurrentThread.CurrentCulture), DiagnosticParameter.SystemMetadata(empty));
	}

	public void SetCulture(string name)
	{
		CultureInfo cultureInfo;
		try
		{
			cultureInfo = CultureInfo.GetCultureInfo(name);
		}
		catch (ArgumentException)
		{
			throw new NavNotSupportedLanguageException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidLanguageCode, name));
		}
		ClientSettings regionalSettings = RegionalSettings;
		regionalSettings.Refresh(cultureInfo, cultureInfo);
		RegionalSettings = regionalSettings;
	}

	public void SetSessionLanguage(int lcid, int defaultClientLcid, int localId)
	{
		int culture = NavEnvironment.FindSupportedLanguage(lcid, defaultClientLcid);
		ClientSettings regionalSettings = RegionalSettings;
		CultureInfo cultureInfo = CultureInfo.GetCultureInfo(culture);
		regionalSettings.Refresh(CultureInfo.GetCultureInfo(localId), cultureInfo);
		RegionalSettings = regionalSettings;
	}

	public bool TrySetSessionLanguage(int lcid, int defaultClientLcid, int localId)
	{
		int windowsLCID = RegionalSettings.WindowsLCID;
		int lCID = RegionalSettings.LCID;
		try
		{
			SetSessionLanguage(lcid, defaultClientLcid, localId);
		}
		catch (Exception)
		{
			SetSessionLanguage(lCID, NavEnvironment.DefaultLanguage, windowsLCID);
		}
		return lcid == RegionalSettings.LCID;
	}

	internal static ClientType TranslateToClientType(ConnectionType connectionType)
	{
		if (connectionTypeToClientType == null)
		{
			Dictionary<ConnectionType, ClientType> dictionary = new Dictionary<ConnectionType, ClientType>();
			Type type = connectionType.GetType();
			FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (FieldInfo fieldInfo in fields)
			{
				SessionEventClientTypeAttribute sessionEventClientTypeAttribute = (SessionEventClientTypeAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(SessionEventClientTypeAttribute));
				if (sessionEventClientTypeAttribute != null)
				{
					dictionary[(ConnectionType)Enum.Parse(type, fieldInfo.Name)] = sessionEventClientTypeAttribute.ClientType;
				}
			}
			connectionTypeToClientType = dictionary;
		}
		if (!connectionTypeToClientType.TryGetValue(connectionType, out var value))
		{
			return ClientType.Unknown;
		}
		return value;
	}

	private void EnsurePermissionSetupVersionCurrent()
	{
		if (localPermissionSetupVersion < Database.PermissionSetupMonitor.SetupVersion)
		{
			localPermissionSetupVersion = Database.PermissionSetupMonitor.SetupVersion;
			cachedAllowedCompanies = null;
		}
	}

	internal bool ValidateUserHasAccessToCompany(string companyName)
	{
		ValidateSessionIsActive();
		return GetAllowedCompanies().Contains(companyName);
	}

	public DataSetResponse GetAllowedCompaniesDataSet()
	{
		HashSet<string> allowedCompanies = GetAllowedCompanies();
		NavDataSet navDataSet = new NavDataSet();
		NavDataTable navDataTable = navDataSet.Tables.Add("Companies");
		navDataTable.Columns.Add(1.ToString(CultureInfo.CurrentCulture), typeof(string));
		foreach (string item in allowedCompanies)
		{
			navDataTable.Rows.Add(new object[1] { item });
		}
		return new DataSetResponse(navDataSet, null);
	}

	public HashSet<string> GetAllowedCompanies()
	{
		EnsurePermissionSetupVersionCurrent();
		if (cachedAllowedCompanies != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00001SI", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Returning cached version of allowed companies");
			return cachedAllowedCompanies;
		}
		HashSet<string> hashSet = new HashSet<string>(Database.CompanyNameComparer);
		HashSet<string> allCompanies = GetAllCompanies(useSecurityFilter: true);
		if (permissions.IsSuperForAllCompanies)
		{
			DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00001SJ", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "User is SuperForAllCompanies");
			cachedAllowedCompanies = allCompanies;
			return allCompanies;
		}
		using (new MaximizedPermissionScope(this, new int[1] { 2000000053 }))
		{
			using NavRecord navRecord = new NavRecord(this, 2000000053, SecurityFiltering.Ignored);
			string companyNameFilter = License.CompanyNameFilter;
			if (!string.IsNullOrEmpty(companyNameFilter))
			{
				DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00001SK", Category.Runtime, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Searching for companies with filter {0}", companyNameFilter);
				navRecord.ALSetFilter(3, companyNameFilter);
			}
			if (GetCompaniesForUser(hashSet, navRecord))
			{
				hashSet = allCompanies;
			}
		}
		if (hashSet != allCompanies && hashSet.Count > 0)
		{
			hashSet.IntersectWith(allCompanies);
		}
		cachedAllowedCompanies = hashSet;
		DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00001SN", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Found {0} companies.", hashSet.Count);
		return hashSet;
	}

	public string GetServicesDefaultCompanyName(bool isSettingRequired = false, bool onlyDefaultIfOneCompany = false)
	{
		if (!string.IsNullOrEmpty(Tenant.TenantSettings.DefaultCompany))
		{
			return Tenant.TenantSettings.DefaultCompany;
		}
		if (!string.IsNullOrEmpty(ServerUserSettings.Instance.ServicesDefaultCompany.Value))
		{
			return ServerUserSettings.Instance.ServicesDefaultCompany.Value;
		}
		HashSet<string> allowedCompanies = GetAllowedCompanies();
		if ((allowedCompanies.Count != 1) & isSettingRequired)
		{
			throw new NavCompanyNotFoundException("Cannot process the request because the default company cannot be found. You can specify a default company in the service configuration file, or specify one for each tenant, or you can add a query string in the form of \"company=[name]\". You can see the available companies by accessing the default OData web service, Company. For more information, see \"OData Web Services\" in Help.");
		}
		if ((allowedCompanies.Count != 1) & onlyDefaultIfOneCompany)
		{
			return string.Empty;
		}
		return allowedCompanies.FirstOrDefault();
	}

	private bool GetCompaniesForUser(HashSet<string> companies, NavRecord wacRecord)
	{
		NCLMetaField fieldByNo = wacRecord.MetaTable.GetFieldByNo(1);
		NCLMetaField fieldByNo2 = wacRecord.MetaTable.GetFieldByNo(3);
		List<NavValue> list = new List<NavValue>
		{
			new NavGuid(User.Id)
		};
		StringBuilder stringBuilder = new StringBuilder("%1");
		foreach (IdentityReference group in User.Groups)
		{
			if (Tenant.Database.NavUsers.TryGetWindowsGroupFromSid(group.Value, out var user))
			{
				list.Add(new NavGuid(user.Id));
				stringBuilder.Append("|%");
				stringBuilder.Append(list.Count);
			}
		}
		wacRecord.SetFilter(fieldByNo, stringBuilder.ToString(), list.ToArray());
		wacRecord.ALSetCurrentKey(fieldByNo2.FieldNo);
		if (!wacRecord.ALFindSet(DataError.TrapError))
		{
			wacRecord.SetFilter(fieldByNo, "");
			return wacRecord.ALIsEmpty;
		}
		do
		{
			string text = wacRecord.GetFieldValue(fieldByNo2).ToString();
			if (string.IsNullOrEmpty(text))
			{
				return true;
			}
			companies.Add(text);
		}
		while (wacRecord.NextRecord());
		return false;
	}

	internal HashSet<string> GetAllCompanies(bool useSecurityFilter)
	{
		HashSet<string> hashSet = new HashSet<string>(Database.CompanyNameComparer);
		using (new MaximizedPermissionScope(this, new int[1] { 2000000006 }))
		{
			using NavRecord navRecord = new NavRecord(this, 2000000006, useSecurityFilter ? SecurityFiltering.Filtered : SecurityFiltering.Ignored);
			string companyNameFilter = License.CompanyNameFilter;
			if (!string.IsNullOrEmpty(companyNameFilter))
			{
				navRecord.ALSetFilter(1, companyNameFilter);
			}
			if (navRecord.ALFind(DataError.TrapError, "-"))
			{
				NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(1);
				do
				{
					hashSet.Add(navRecord.GetFieldValue(fieldByNo).ToString());
				}
				while (navRecord.ALNext() != 0);
			}
		}
		return hashSet;
	}

	internal string GetCompanyNameFromId(NavGuid companyId, bool useSecurityFilter)
	{
		string result = null;
		using (new MaximizedPermissionScope(this, new int[1] { 2000000006 }))
		{
			using NavRecord navRecord = new NavRecord(this, 2000000006, useSecurityFilter ? SecurityFiltering.Filtered : SecurityFiltering.Ignored);
			string companyNameFilter = License.CompanyNameFilter;
			if (!string.IsNullOrEmpty(companyNameFilter))
			{
				navRecord.ALSetFilter(1, companyNameFilter);
			}
			navRecord.ALSetFilter(8000, companyId.ToString());
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				result = navRecord.GetFieldValue(1).ToString();
			}
		}
		return result;
	}

	internal string GetCompanyNameFromProfileId(NavText businessProfileId, bool useSecurityFilter)
	{
		string result = null;
		using (new MaximizedPermissionScope(this, new int[1] { 2000000006 }))
		{
			using NavRecord navRecord = new NavRecord(this, 2000000006, useSecurityFilter ? SecurityFiltering.Filtered : SecurityFiltering.Ignored);
			string companyNameFilter = License.CompanyNameFilter;
			if (!string.IsNullOrEmpty(companyNameFilter))
			{
				navRecord.ALSetFilter(1, companyNameFilter);
			}
			navRecord.ALSetFilter(8005, businessProfileId.ToString());
			if (navRecord.ALFindFirst(DataError.TrapError))
			{
				result = navRecord.GetFieldValue(1).ToString();
			}
		}
		return result;
	}

	internal long CalculateNumberOfNonFreeCompanies()
	{
		HashSet<string> allCompanies = GetAllCompanies(useSecurityFilter: false);
		long num = allCompanies.Count;
		if (!string.IsNullOrEmpty(License.FreeCompanyNameFilter))
		{
			foreach (string item in allCompanies)
			{
				if (License.IsFreeCompanyName(item))
				{
					num--;
				}
			}
		}
		return num;
	}

	internal bool GetCompanyIsEvaluationCompany(string companyName)
	{
		using (new MaximizedPermissionScope(this, new int[1] { 2000000006 }))
		{
			using NavRecord navRecord = new NavRecord(this, 2000000006, SecurityFiltering.Ignored);
			navRecord.SetFieldValue(1, new NavText(companyName));
			navRecord.ALFind("=");
			return navRecord.GetFieldValue(2).ToBoolean();
		}
	}

	public Lazy<SystemIndicator> OpenCompany(string companyName, bool updateUserPersonalization, bool openedForRenameOrDelete = false)
	{
		verifyExecutePermissionCache.Clear();
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		Stopwatch stopwatch = Stopwatch.StartNew();
		string dimension3Value = "Failed";
		if (GetAllowedCompanies().Contains("My Company") && AppId == "INV" && !companyName.Equals("My Company"))
		{
			throw mostSpecificInstance.SendExceptionTag("000018H", Category.Runtime, new NavCSideDataException(18023729, CultureInfo.CurrentCulture, Lang.InvoicingUserDoesNotHaveAccessToCompany, DiagnosticParameter.Oii(companyName), DiagnosticParameter.SystemMetadata("My Company")), Verbosity.Warning);
		}
		using (mostSpecificInstance.CreateTraceScope("00000SL", DiagnosticParameter.SystemMetadata("OpenCompany"), MonitorType.ExceptionCount, Category.Runtime))
		{
			try
			{
				mostSpecificInstance.SendTraceTag("00000KV", Category.Runtime, Verbosity.Normal, "Opening company -- CompanyName:{0}", DiagnosticParameter.Oii(companyName));
				if (AppId == "INV")
				{
					updateUserPersonalization = false;
				}
				if (string.IsNullOrEmpty(companyName) || companyName.Length > 30)
				{
					throw mostSpecificInstance.SendExceptionTag("00000SM", Category.Runtime, new NavNCLInvalidCompanyNameException());
				}
				bool flag = ValidateUserHasAccessToCompany(companyName);
				if (!flag && AppId == "INV")
				{
					DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00001SO", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Clearning cachedAllowedCompanies");
					cachedAllowedCompanies = null;
					flag = ValidateUserHasAccessToCompany(companyName);
				}
				if (!flag)
				{
					if (!License.CheckCompanyNameLicenseFilter(companyName))
					{
						throw mostSpecificInstance.SendExceptionTag("00000SN", Category.Runtime, new NavLicenseViolationException(string.Format(CultureInfo.CurrentCulture, Lang.CompanyNameLicenseFilterViolation, License.CompanyNameFilter)));
					}
					if (GetAllCompanies(useSecurityFilter: false).Contains(companyName))
					{
						throw mostSpecificInstance.SendExceptionTag("00000SO", Category.Runtime, new NavCSideDataException(18023729, string.Format(CultureInfo.CurrentCulture, Lang.UserDoesNotHaveAccessToCompany, companyName)));
					}
					throw mostSpecificInstance.SendExceptionTag("00000SP", Category.Runtime, new NavCSideDataException(85132272, string.Format(CultureInfo.CurrentCulture, Lang.CompanyNameDoesNotExist, companyName)));
				}
				if (!License.IsFreeCompanyName(companyName))
				{
					long num = CalculateNumberOfNonFreeCompanies();
					int maxCompanies = License.MaxCompanies;
					if (num > maxCompanies && !openedForRenameOrDelete)
					{
						throw mostSpecificInstance.SendExceptionTag("00000SQ", Category.Runtime, new NavLicenseViolationException(string.Format(CultureInfo.CurrentCulture, Lang.MaxCompaniesError, maxCompanies, License.FreeCompanyNameFilter)));
					}
				}
				TenantLicenseState = Tenant.LicenseState;
				bool companyIsEvaluationCompany = GetCompanyIsEvaluationCompany(companyName);
				if (!IsSystemSession)
				{
					CheckTenantLicenseState(companyIsEvaluationCompany);
				}
				if (company != null)
				{
					if (string.IsNullOrEmpty(company.CompanyName) && !Tenant.LicenseValidator.CheckIfLicenseNamedUserLimitsAreOk(this))
					{
						NavDialog.ALMessage(Guid.NewGuid(), string.Format(CultureInfo.CurrentCulture, Lang.LicenseViolation, License.NamedUsersNumberOfFullUsers, License.NamedUsersNumberOfLimitedUsers, License.NumberOfFullDevices, License.NumberOfLimitedDevices, License.NumberOfIsvDevices, License.NumberOfIsvFunctionalDevices));
					}
					CloseCompany();
				}
				company = new NavCompany(this, companyName, companyIsEvaluationCompany);
				if (!IsSystemSession && !DisableUpdateUserPersonalization && (updateUserPersonalization || NoCompanyFoundInUserPersonalization()))
				{
					SavePersonalization(0, 0, overrideLocaleIdInUserProfile: false);
				}
				if (!IsSystemSession && ServerUserSettings.Instance.EnableMembershipEntitlement.Value)
				{
					IEnumerable<NavCode> enumerable = User.MembershipEntitlements?.GetEntitlementSetIds(this);
					if (enumerable == null || !enumerable.Any())
					{
						DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00007NB", Category.Security, Verbosity.Error, PrivacyClassification.SystemMetadata, "User opened a company where no entitlementsets were found to be applicable. This will result in permission errors.");
					}
					else
					{
						DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00007NC", Category.Security, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Effective entitlements sets on current company: " + string.Join(",", enumerable.Select((NavCode e) => e.Value.ToString())));
					}
				}
				defaultRoundPrecisionCached = false;
				Diagnostics.SendTraceTagOnThreshold("00000MA", Category.Runtime, Verbosity.Warning, 1000, (Stopwatch watch) =>
				{
					SystemCodeunitFactory.CompanyTriggers.InvokeOnCompanyOpen();
				}, PrivacyClassification.SystemMetadata, null, "InvokeOpenCompany");
				allowClientCacheFlush = true;
				DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("0000089", Category.Operations, Verbosity.Normal, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "Platform: {0} Application version: {1} Application build: {2}", PlatformVersion, NavGlobal.AppDatabase.ApplicationVersion, NavGlobal.AppDatabase.ApplicationBuild));
				Lazy<SystemIndicator> result = new Lazy<SystemIndicator>(SystemCodeunitFactory.UIHelperTriggers.InvokeGetSystemIndicator, isThreadSafe: false);
				dimension3Value = "Success";
				return result;
			}
			catch (NavCompanyNotAccessibleException)
			{
				throw;
			}
			catch (Exception exception) when (mostSpecificInstance.SendExceptionTagExceptionFilter("00000SR", Category.Runtime, exception))
			{
				throw;
			}
			finally
			{
				OpenCompanyDurationMetric.LogValue(stopwatch.ElapsedMilliseconds, companyName, Tenant.Id, dimension3Value, PlatformVersion.ToString(), NavGlobal.AppDatabase.ApplicationVersion, NavGlobal.AppDatabase.ApplicationBuild);
			}
		}
	}

	private void CheckTenantLicenseState(bool isEvaluationCompany)
	{
		if (TenantLicenseState == TenantLicenseState.LockedOut && ApplicationIdTypes.SaasApplicationIdTypes.Contains(AppId))
		{
			DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00000GL", Category.Security, Verbosity.Warning, "User {0} was trying to open a company of a locked tenant.", DiagnosticParameter.Eupi(User.Id));
			throw new NavCompanyNotAccessibleException(string.Format(CultureInfo.CurrentCulture, Lang.TenantLockedOut))
			{
				DiagnosticsSuppress = true
			};
		}
		if (!isEvaluationCompany && (!(AppId != "FIN") || !(appId != "ACC")) && !Authenticator.SkipTenantLicenseStateCheck)
		{
			if (TenantLicenseState == TenantLicenseState.Suspended || TenantLicenseState == TenantLicenseState.Deleted)
			{
				DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00000GM", Category.Security, Verbosity.Warning, "User {0} was trying to open a production company of a tenant in {1} state.", DiagnosticParameter.Eupi(User.Id), DiagnosticParameter.SystemMetadata(TenantLicenseState));
				ResetDefaultCompanyToFirstEvalCompany();
				throw new NavCompanyNotAccessibleException(string.Format(CultureInfo.CurrentCulture, Lang.PaidLicenseExpired))
				{
					DiagnosticsSuppress = true
				};
			}
			if ((TenantLicenseState == TenantLicenseState.Paid || TenantLicenseState == TenantLicenseState.Warning) && !User.CanAccessProductionCompany)
			{
				DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00000GN", Category.Security, Verbosity.Warning, "User {0} was trying to open a production company. The tenant is in {1} state but a paid license is not assigned to the user.", DiagnosticParameter.Eupi(User.Id), DiagnosticParameter.SystemMetadata(TenantLicenseState));
				ResetDefaultCompanyToFirstEvalCompany();
				throw new NavCompanyNotAccessibleException(string.Format(CultureInfo.CurrentCulture, Lang.NoPaidLicenseAssigned))
				{
					DiagnosticsSuppress = true
				};
			}
		}
	}

	private void ResetDefaultCompanyToFirstEvalCompany()
	{
		if (!IsSystemSession)
		{
			string firstEvalCompanyName = GetFirstEvalCompanyName();
			if (firstEvalCompanyName != null)
			{
				SavePersonalization(0, 0, overrideLocaleIdInUserProfile: false, firstEvalCompanyName);
			}
		}
	}

	private string GetFirstEvalCompanyName()
	{
		return GetAllowedCompanies().FirstOrDefault(GetCompanyIsEvaluationCompany);
	}

	private bool NoCompanyFoundInUserPersonalization()
	{
		return string.IsNullOrEmpty(Personalization.CompanyName);
	}

	public Lazy<SystemIndicator> OpenCompany(string companyName)
	{
		return OpenCompany(companyName, updateUserPersonalization: false);
	}

	public void CloseCompany()
	{
		try
		{
			if (IsCompanyOpen)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000KW", Category.Runtime, Verbosity.Normal, "Closing company -- CompanyName:{0}", DiagnosticParameter.Oii(CompanyName));
				SystemCodeunitFactory.CompanyTriggers.InvokeOnCompanyClose();
			}
		}
		finally
		{
			if (TableConnectionManager != null && !FeatureSwitches.Instance.DisableTableConnectionPerCompany)
			{
				TableConnectionManager.Dispose();
				TableConnectionManager = new TableConnectionManager(this);
			}
			if (company != null)
			{
				company.Dispose();
			}
			allowClientCacheFlush = false;
			company = null;
		}
	}

	internal void ChangeCurrentCompany(string newCompanyName)
	{
		company.Dispose();
		company = new NavCompany(this, newCompanyName, GetCompanyIsEvaluationCompany(newCompanyName));
	}

	public void CloseCompanyShowMessageOnFailure()
	{
		try
		{
			CloseCompany();
		}
		catch (NavBaseException ex)
		{
			if (CallbackAllowed)
			{
				ClientCallback.DialogMessage(ex.Message, new Guid("{00000000-000C-000C-000E-00000000000D}"));
			}
		}
	}

	internal NavRecord GetGlobalRecordInstance(int tableId)
	{
		return NCLMetadata.GetMetaTableById(tableId).CreateObjectInstance(this, isTemporary: false, null, string.Empty, SecurityFiltering.Ignored);
	}

	public string GetObjectName(ApplicationObjectId objectId)
	{
		using (new MaximizedPermissionScope(this, new int[1] { 2000000038 }))
		{
			using NavRecord navRecord = new NavRecord(this, 2000000038, SecurityFiltering.Ignored);
			if (navRecord.ALGet(DataError.TrapError, NavInteger.Create((int)objectId.ObjectType), NavInteger.Create(objectId.ObjectNumber)))
			{
				return navRecord.GetFieldValue(navRecord.MetaTable.GetFieldByNo(4)).ToString();
			}
		}
		throw new NavNCLObjectNotFoundException(objectId);
	}

	public string GetObjectNameForErrorMessage(ApplicationObjectId objectId)
	{
		try
		{
			using (new MaximizedPermissionScope(this, new int[1] { 2000000001 }))
			{
				using NavRecord navRecord = new NavRecord(this, 2000000001, SecurityFiltering.Ignored);
				navRecord.SetFilter(navRecord.MetaTable.GetFieldByIndex(1), "%1", NavInteger.Create((int)objectId.ObjectType));
				navRecord.SetFilter(navRecord.MetaTable.GetFieldByIndex(3), "%1", NavInteger.Create(objectId.ObjectNumber));
				if (navRecord.ALFindFirst(DataError.TrapError))
				{
					return navRecord.GetFieldValue(navRecord.MetaTable.GetFieldByIndex(4)).ToString();
				}
			}
		}
		catch (Exception)
		{
		}
		return objectId.ObjectNumber.ToString(CultureInfo.InvariantCulture);
	}

	internal void EnsureTrialStarted()
	{
		if (!company.IsEvaluationCompany && AppId == "FIN" && !Authenticator.IsAuthenticatingAsApplication)
		{
			TenantLicenseStateHelper.EnsureTrialStarted(this);
		}
	}

	public void SavePersonalization(int languageId, int formattingCulture, bool overrideLocaleIdInUserProfile, string companyName = null, bool overrideTimeZoneInUserProfile = false)
	{
		using (DiagnosticsResolver.GetMostSpecificInstance(this).CreateTimingScope("000011T", Category.Runtime, DiagnosticParameter.SystemMetadata("Save Personalization")))
		{
			using (new MaximizedPermissionScope(this, new int[1] { 2000000073 }))
			{
				using NavRecord navRecord = new NavRecord(this, 2000000073);
				this.BeginTransaction();
				bool commit = false;
				try
				{
					bool flag = navRecord.ALGet(DataError.TrapError, new NavGuid(User.Id));
					if (flag && PersonalizationDisabled && (languageId == 0 || languageId == navRecord.GetFieldValue(12).ToInt32()))
					{
						return;
					}
					bool flag2 = languageId != 0 || !defaultLCID;
					int num = ((languageId == 0) ? cultureSettings.LCID : languageId);
					flag2 &= !flag || navRecord.GetFieldValue(12).ToInt32() != num;
					if (flag2)
					{
						navRecord.SetFieldValue(12, NavInteger.Create(num));
					}
					bool flag3 = formattingCulture != 0;
					int num2;
					if (formattingCulture != 0)
					{
						num2 = formattingCulture;
					}
					else
					{
						num2 = (flag ? cultureSettings.WindowsLCID : NavEnvironment.DefaultLanguage);
					}
					flag3 = flag3 || !flag || ((navRecord.GetFieldValue(27).ToInt32() != num2) & overrideLocaleIdInUserProfile);
					if (flag3)
					{
						navRecord.SetFieldValue(27, NavInteger.Create(num2));
					}
					string text;
					if (string.IsNullOrWhiteSpace(companyName))
					{
						text = (IsCompanyOpen ? CompanyName : null);
					}
					else
					{
						text = companyName;
					}
					bool flag4 = text != null && (!flag || navRecord.GetFieldValue(15).ToString() != text);
					if (flag4)
					{
						navRecord.SetFieldValue(15, new NavText(int.MaxValue, text));
					}
					bool flag5 = !flag || ((navRecord.GetFieldValue(30).ToString() != ClientTimeZone.Id) & overrideTimeZoneInUserProfile);
					if (flag5)
					{
						navRecord.SetFieldValue(30, new NavText(int.MaxValue, ClientTimeZone.Id));
					}
					if (flag)
					{
						if (flag2 | flag4 | flag3 | flag5)
						{
							try
							{
								navRecord.ALModify(DataError.TrapError);
								DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00000EN", Category.Runtime, Verbosity.Normal, "Setting user personalization - client type is {8} - storing language true/false {0} - value = {1}, storing formattingCulture true/false {2} - value = {3}, company language true/false {4} - value = {5}, storing timezone true/false {6} - value = {7}", DiagnosticParameter.SystemMetadata(flag2), DiagnosticParameter.SystemMetadata(num), DiagnosticParameter.SystemMetadata(flag3), DiagnosticParameter.SystemMetadata(num2), DiagnosticParameter.SystemMetadata(flag4), DiagnosticParameter.Oii(text), DiagnosticParameter.SystemMetadata(flag5), DiagnosticParameter.SystemMetadata(ClientTimeZone.Id), DiagnosticParameter.SystemMetadata(clientConnectionType));
							}
							catch (NavCSideException)
							{
							}
						}
					}
					else
					{
						navRecord.SetFieldValue(3, new NavGuid(User.Id));
						navRecord.ALInsert(DataError.TrapError);
						DiagnosticsResolver.GetMostSpecificInstance(this).SendTraceTag("00001QZ", Category.Runtime, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Creating user personalization - client type is {8} - storing language true/false {0} - value = {1}, storing formattingCulture true/false {2} - value = {3}, company language true/false {4} - value = {5}, storing timezone true/false {6} - value = {7}", flag2, num, flag3, num2, flag4, text, flag5, ClientTimeZone.Id, clientConnectionType);
					}
					commit = true;
				}
				finally
				{
					this.EndTransaction(commit);
				}
			}
		}
	}

	internal void BeginNonServiceCallExecution()
	{
		if (serviceConnection == null)
		{
			throw new NotSupportedException();
		}
		serviceConnection.BeginNonServiceCallExecution();
	}

	internal void EndNonServiceCallExecution()
	{
		if (serviceConnection == null)
		{
			throw new NotSupportedException();
		}
		serviceConnection.EndNonServiceCallExecution();
	}

	public override int GetHashCode()
	{
		return Id.GetHashCode();
	}

	public bool IsApplicationAreaEnabled(string applicationArea)
	{
		if (applicationAreas.Length == 1)
		{
			return true;
		}
		if (string.IsNullOrWhiteSpace(applicationArea))
		{
			return false;
		}
		if (!applicationAreaCache.TryGetValue(applicationArea, out var value))
		{
			string[] splittedApplicationAreas = applicationArea.Split(',');
			value = applicationAreas.Any((string area) => splittedApplicationAreas.Contains(area, StringComparer.CurrentCultureIgnoreCase));
			applicationAreaCache[applicationArea] = value;
		}
		return value;
	}

	private static long MakeCombinedInt64(int highNumber, int lowNumber)
	{
		return ((long)highNumber << 32) + lowNumber;
	}

	public bool PushDynamicCaptionStack(int tableNumber, int fieldNumber)
	{
		long mask = MakeCombinedInt64(tableNumber, fieldNumber);
		if (dynamicCaptionStack.Any((long number) => number == mask))
		{
			return false;
		}
		dynamicCaptionStack.Push(mask);
		return true;
	}

	public void PopDynamicCaptionStack()
	{
		dynamicCaptionStack.Pop();
	}

	public void PushLocalLanguage(int languageId)
	{
		globalLanguageStack.Push(languageId);
	}

	public int PopLocalLanguage()
	{
		if (globalLanguageStack.Count <= 0)
		{
			return -1;
		}
		return globalLanguageStack.Pop();
	}

	public IDisposable CreateGlobalLanguageScope()
	{
		return new GlobalLanguageScope(this);
	}

	public Decimal18 GetDefaultRoundPrecision()
	{
		if (!defaultRoundPrecisionCached)
		{
			defaultRoundPrecision = Company?.SystemCodeunitFactory.UIHelperTriggers.InvokeGetDefaultRoundingPrecision() ?? NavDecimalHelper.FallbackRoundPrecision();
			defaultRoundPrecisionCached = true;
		}
		return defaultRoundPrecision;
	}

	public bool IsDebugging()
	{
		if (ALDebugger.DebuggingSession == this)
		{
			return true;
		}
		return !string.IsNullOrEmpty(DebuggingContext);
	}

	public void ThrowSessionTerminatedExceptionIfStopping()
	{
		AccessLock.ThrowSessionTerminatedExceptionIfStopping();
	}

	public void ThrowSessionTerminatedExceptionIfStoppingOrDisposed()
	{
		AccessLock.ThrowIfNotActive();
	}

	public void AddWarning(ConnectionWarning warning)
	{
		warnings.Add(warning);
	}

	public void PushNavAppUsageEventSuppression(string reason)
	{
		navAppUsageTracker?.PushEventSuppression(reason);
	}

	public void PopNavAppUsageEventSuppression()
	{
		navAppUsageTracker?.PopEventSuppression();
	}

	public void SignalNavAppUsage(NavAppRuntimeMetadata appMetadata)
	{
		navAppUsageTracker?.SignalUsage(appMetadata);
	}

	public void SetAppInstallationContext(NavAppInstallationContext context)
	{
		if (context != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000JG", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Setting extension install context -- Group:{0}; Package:{1}; App:{2}; Name:{3}; Publisher:{4}; Version:{5}", context.TargetGroup.GroupId, ((WrappedValue<Guid>)(object)context.AppBeingTargeted.PackageId).Value, ((WrappedValue<Guid>)(object)context.AppBeingTargeted.AppId).Value, context.AppBeingTargeted.Name, context.AppBeingTargeted.Publisher, context.AppBeingTargeted.Version);
			AppInstallationContext = context;
		}
	}

	public void ClearAppInstallationContext()
	{
		if (AppInstallationContext != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000JH", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Clearing extension install context");
			AppInstallationContext = null;
		}
	}

	public void SetAppUpgradeContext(NavAppUpgradeContext context)
	{
		if (context != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000019D", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Setting extension upgrade context -- Group:{0}; Package:{1}; App:{2}; Name:{3}; Publisher:{4}; Version:{5}", context.TargetGroup.GroupId, ((WrappedValue<Guid>)(object)context.AppBeingTargeted.PackageId).Value, ((WrappedValue<Guid>)(object)context.AppBeingTargeted.AppId).Value, context.AppBeingTargeted.Name, context.AppBeingTargeted.Publisher, context.AppBeingTargeted.Version);
			AppUpgradeContext = context;
		}
	}

	public void ClearAppUpgradeContext()
	{
		if (AppUpgradeContext != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000019E", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Clearing extension upgrade context");
			AppUpgradeContext = null;
		}
	}

	internal void SetAccessToRemovedMetadataContext(NavMetadataAccessContext context)
	{
		if (context != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001FL", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Setting access to removed metadata context");
			RemovedMetadataAccessContext = context;
		}
	}

	internal void ClearAccessToRemovedMetadataContext()
	{
		if (RemovedMetadataAccessContext != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001FM", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Clearing access to removed metadata context");
			RemovedMetadataAccessContext = null;
		}
	}

	internal void SetRunTableContext(NavRunTableContext context)
	{
		if (context != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001FN", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Setting run table context");
			RunTableContext = context;
		}
	}

	internal void ClearRunTableContext()
	{
		if (RunTableContext != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001FO", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Clearing run table context");
			RunTableContext = null;
		}
	}

	public void SetAppSyncContext(AppSyncContext context)
	{
		if (context != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000019F", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Setting extension sync context");
			AppSyncContext = context;
		}
	}

	public void ClearAppSyncContext()
	{
		if (AppSyncContext != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000019G", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Clearing extension sync context");
			AppSyncContext = null;
		}
	}

	public void SetIsolatedEventErrorReporter(INavErrorReporter reporter)
	{
		if (reporter != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001EZ", Category.DataUpgrade, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Setting isolated error reporter");
			IsolatedEventErrorReporter = reporter;
		}
	}

	public void ClearIsolatedEventErrorReporter()
	{
		if (IsolatedEventErrorReporter != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001F0", Category.DataUpgrade, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Clearing isolated error reporter");
			IsolatedEventErrorReporter = null;
		}
	}

	public void ClearPermissionSystemActivatedCache()
	{
		accessControlDatabaseCalled = false;
	}

	public ExclusiveTenantActionScope CreateExclusiveTenantActionScope()
	{
		exclusiveTenantActionScope = new ExclusiveTenantActionScope(this, NavEnvironment.TenantConfigurationSynchronizer, NavEnvironment.Instance.Tenants.Configuration);
		return exclusiveTenantActionScope;
	}

	public void UpdateSessionConfiguration(string key, object value)
	{
		if (key != null && key == "ExclusiveAccessTicket")
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001IH", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "Updating Session Configuration -- SessionId:{0}; Key:{1}, Value:{2}", Id.ToString(CultureInfo.InvariantCulture), key, value));
			ExclusiveAccessTicket = value?.ToString();
			return;
		}
		throw new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.SessionPropertyNotConfigurable, key));
	}

	internal void StartDebuggingSession(string debuggingContext, bool waitForDebugAdapterConfiguration = true)
	{
		if (!string.IsNullOrEmpty(debuggingContext))
		{
			DebuggingContext = debuggingContext;
			NavEnvironment.Instance.DebugRuntimeManager.CreateRuntime(debuggingContext, this, waitForDebugAdapterConfiguration);
		}
	}

	internal void RemoveDebugRuntime(string debuggingContext)
	{
		if (!string.IsNullOrEmpty(debuggingContext))
		{
			NavEnvironment.Instance.DebugRuntimeManager.RemoveRuntime(debuggingContext);
			DebuggingContext = null;
		}
	}

	internal void ClearLastError()
	{
		lastException = null;
		lastErrorCallstack = null;
	}

	internal void CheckTryFunction(string writeMethodName)
	{
		if (ServerUserSettings.Instance.DisableWriteInsideTryFunctions.Value)
		{
			NavMethodScope currentMethodScope = CurrentMethodScope;
			if (currentMethodScope != null && currentMethodScope.IsInsideTryCall)
			{
				throw NavNclTryFunctionWriteException.Create(writeMethodName, currentMethodScope.TryFunctionName);
			}
		}
	}

	internal void RegisterSessionEventRecorder(NavEventTelemetryRegistrar eventTelemetryRegistrar)
	{
		IsEventSessionRecorderEnabled = true;
		EventTelemetryRegistrar = eventTelemetryRegistrar;
	}

	internal void DisposeEventSessionRecorder()
	{
		if (IsEventSessionRecorderEnabled || EventTelemetryRegistrar != null)
		{
			IsEventSessionRecorderEnabled = false;
			EventTelemetryRegistrar = null;
		}
	}
}
