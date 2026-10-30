using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Exceptions.Encryption;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavTenantCollection : IDisposable
{
	private class NavMountingTask
	{
		public NavTenantMountingParameters Parameters { get; set; }

		public NavTenant Tenant { get; set; }

		public int AttemptNumber { get; set; }
	}

	private const int MaxMountingAttemptNumber = 3;

	private const int TenantConfigurationSyncPeriodMs = 10000;

	private const int PollSleepMs = 1000;

	private const int DatabaseConnectionAttempts = 3;

	private const int TenantMountingThreadsPerProcessor = 5;

	private const int MaxDisposeTimeout = 5000;

	private readonly LockFreeDictionary<string, NavTenant> alternateTenantIds = new LockFreeDictionary<string, NavTenant>(StringComparer.OrdinalIgnoreCase);

	private readonly NavAppGroupUpdater navAppGroupUpdater;

	private readonly ConcurrentBag<NavMountingTask> pendingMountingTenants = new ConcurrentBag<NavMountingTask>();

	private readonly Action onApplicationMounted;

	private readonly int tenantDetectionMaxRetryCount;

	private readonly int tenantMaxPollCountPerCycle;

	private LockFreeDictionary<string, NavTenant> tenants = new LockFreeDictionary<string, NavTenant>(StringComparer.OrdinalIgnoreCase);

	private Task configurationTask = TaskHelper.CompletedTask;

	private string publicEncryptionKeyFromDatabase;

	private IDatabasePasswordPublicEncryptionKeyStorage publicKeyStorage;

	private volatile bool singleTenancyConfigured;

	private NavSystemTenant systemTenant;

	private TenantConfigurationManager tenantConfigurationManager;

	private NonReEntrantTimerWrapper mountPendingTenantsTimer;

	public NavSystemTenant SystemTenant
	{
		get
		{
			if (Volatile.Read(ref systemTenant) == null)
			{
				WaitForConfigurationCompleted();
			}
			return systemTenant;
		}
	}

	public bool IsSystemTenantCreatedNoWait => systemTenant != null;

	public bool IsSystemTenantCreated
	{
		get
		{
			if (systemTenant == null)
			{
				WaitForConfigurationCompleted();
			}
			return systemTenant != null;
		}
	}

	internal int Count => tenants.Count;

	public NavTenant DefaultTenant
	{
		get
		{
			WaitForConfigurationCompleted();
			NavTenant navTenant = tenants.Values.FirstOrDefault();
			if (navTenant == null)
			{
				throw NavTenantNotFoundException.CreateForNoTenantsMounted();
			}
			if (!IsSingleTenancyMode)
			{
				throw NavTenantNotFoundException.CreateForTenantRequiredButNotSpecified();
			}
			return navTenant;
		}
	}

	public TenantConfigurationManager Configuration => tenantConfigurationManager;

	public bool IsSingleTenancyMode
	{
		get
		{
			return singleTenancyConfigured;
		}
		private set
		{
			singleTenancyConfigured = value;
		}
	}

	public string PublicEncryptionKeyFromDatabase
	{
		get
		{
			if (publicEncryptionKeyFromDatabase == null)
			{
				publicEncryptionKeyFromDatabase = publicKeyStorage.GetPublicKey(SystemTenant.Database);
			}
			return publicEncryptionKeyFromDatabase;
		}
	}

	public event EventHandler<NavTenantChangedEventArgs> TenantDismounting;

	public event EventHandler<NavTenantChangedEventArgs> TenantDismounted;

	public NavTenantCollection(Action onApplicationMounted, NavAppGroupUpdater groupUpdater)
	{
		this.onApplicationMounted = onApplicationMounted;
		navAppGroupUpdater = groupUpdater;
		tenantDetectionMaxRetryCount = ServerUserSettings.Instance.TenantDetectionMaxRetryCount.Value;
		tenantMaxPollCountPerCycle = ServerUserSettings.Instance.TenantMaxPollCountPerCycle.Value;
	}

	public NavTenant TenantWhere(Func<NavTenant, bool> predicate, bool includeSystemTenant = false)
	{
		WaitForConfigurationCompleted();
		if (includeSystemTenant && predicate(systemTenant))
		{
			return systemTenant;
		}
		return tenants.Values.Where((NavTenant tenant) => !tenant.FailedToMount).FirstOrDefault(predicate);
	}

	public void OnTenantDismounting(string tenantId)
	{
		TenantDismounting?.Invoke(this, new NavTenantChangedEventArgs(tenantId));
	}

	public void OnTenantDismounted(string tenantId)
	{
		TenantDismounted?.Invoke(this, new NavTenantChangedEventArgs(tenantId));
	}

	public IEnumerable<NavTenant> TenantsOnTheSameSqlDatabase(string tenantDatabaseId)
	{
		TenantDatabaseConfiguration tenantDatabaseConfiguration = NavEnvironment.Instance.TenantDatabases.Configuration.GetTenantDatabaseConfiguration(tenantDatabaseId);
		if (tenantDatabaseConfiguration == null)
		{
			return new List<NavTenant>();
		}
		return tenants.Values.Where((NavTenant t) => !t.FailedToMount && t.Database.DatabaseName.Equals(tenantDatabaseConfiguration.DatabaseName) && t.Database.DatabaseServer.NormalizedServerName.Equals(NavDatabaseServer.NormalizeServerName(tenantDatabaseConfiguration.DatabaseServer)));
	}

	public IEnumerable<NavTenant> AllTenants(bool includeSystemTenant = false, bool includeFailedTenants = false)
	{
		IEnumerable<NavTenant> values = (includeFailedTenants ? tenants.Values : tenants.Values.Where((NavTenant tenant) => !tenant.FailedToMount));
		NavSystemTenant navSystemTenant = Volatile.Read(ref systemTenant);
		if (includeSystemTenant && navSystemTenant != null)
		{
			yield return navSystemTenant;
		}
		foreach (NavTenant item in values)
		{
			yield return item;
		}
	}

	public NavTenant GetTenantById(string tenantId, bool includeFailed = false, bool enablePolling = false)
	{
		WaitForConfigurationCompleted();
		if (TryGetTenantById(tenantId, out var tenant, includeSystemTenant: false, includeFailed))
		{
			return tenant;
		}
		if (enablePolling && !IsSingleTenancyMode && (tenant = PollForTenantChanges(tenantId, string.Empty)) != null)
		{
			return tenant;
		}
		if (!includeFailed && TryGetTenantById(tenantId, out tenant, includeSystemTenant: true))
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001EB", Category.Runtime, new NavTenantNotFoundException(string.Format(CultureInfo.CurrentCulture, Lang.TenantNotAccessibleInState, tenantId, tenant.State)));
		}
		throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001JP", Category.Runtime, NavTenantNotFoundException.CreateForMountedTenantNotFound(tenantId));
	}

	public NavTenant GetTenantByUri(Uri uri)
	{
		return ResolveTenantByIdOrHost(HttpUtility.ParseQueryString(uri.Query).Get("tenant") ?? string.Empty, uri.Host);
	}

	public NavTenant GetTenantByIdAndCheckClientHostName(string tenantId, string clientHostName, bool includeFailed = false, bool enablePolling = false)
	{
		NavTenant navTenant = ResolveTenantByIdAndCheckClientHostName(tenantId, clientHostName, includeFailed, enablePolling);
		return navTenant ?? DefaultTenant;
	}

	public Uri AddTenantInfo(string tenantId, Uri uri, bool doNotUpdateUriWithAlternateIdInfo)
	{
		return AddTenantInfo(tenantId, uri, Dns.GetHostAddresses, null, doNotUpdateUriWithAlternateIdInfo);
	}

	public bool TryGetTenantById(string tenantId, out NavTenant tenant, bool includeSystemTenant = false, bool includeFailed = false)
	{
		if (tenantId == null)
		{
			throw new ArgumentNullException("tenantId");
		}
		if (tenantId.Length == 0)
		{
			tenant = DefaultTenant;
			return true;
		}
		WaitForConfigurationCompleted();
		if (includeSystemTenant && IsSystemTenantId(tenantId))
		{
			tenant = systemTenant;
			return tenant != null;
		}
		if (tenants.TryGetValue(tenantId, out tenant) || alternateTenantIds.TryGetValue(tenantId, out tenant))
		{
			if ((!includeFailed && tenant.FailedToMount) || tenant.IsDisposed)
			{
				tenant = null;
				return false;
			}
			return true;
		}
		return false;
	}

	internal void AddTenantAction(Action<NavTenant> action, NavTenant excludeTenant)
	{
		foreach (NavTenant item in AllTenants())
		{
			if (excludeTenant != item)
			{
				item.AddTenantAction(action);
			}
		}
	}

	internal Uri AddTenantInfo(string tenantId, Uri uri, string applicationId, bool skipUpdateUriWithAlternateIdInfo)
	{
		return AddTenantInfo(tenantId, uri, Dns.GetHostAddresses, () => applicationId, skipUpdateUriWithAlternateIdInfo);
	}

	internal Uri AddTenantInfo(string tenantId, Uri uri, Func<string, IPAddress[]> getHostAddresses, Func<string> getApplicationId = null, bool skipUpdateUriWithAlternateIdInfo = false)
	{
		if (string.IsNullOrWhiteSpace(tenantId))
		{
			throw new ArgumentNullException("tenantId");
		}
		if (uri == null)
		{
			throw new ArgumentNullException("uri");
		}
		if (IsSingleTenancyMode)
		{
			return uri;
		}
		string appId = ((getApplicationId != null) ? getApplicationId() : NavCurrentThread.Session?.AppId);
		if (skipUpdateUriWithAlternateIdInfo || !TryUpdateUriWithTenantAlternateIdInfo(tenantId, getHostAddresses, ref uri, appId))
		{
			return AddTenantParameterIfMissing(tenantId, uri);
		}
		return uri;
	}

	private NavTenant PollForTenantChanges(string tenantId, string hostname)
	{
		if (tenantConfigurationManager == null)
		{
			return null;
		}
		if (Interlocked.Increment(ref tenantConfigurationManager.manualPollCounter) >= tenantMaxPollCountPerCycle)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000003U", Category.Tenant, Verbosity.Warning, "Exceeded maximum tenant poll requests for the current timeslot. Tenant id {0}, hostname {1}", DiagnosticParameter.Oii(tenantId), DiagnosticParameter.SystemMetadata(hostname));
			return null;
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000003V", Category.Tenant, Verbosity.Normal, "Polling for unknown tenant with id {0}, hostname {1}", DiagnosticParameter.Oii(tenantId), DiagnosticParameter.SystemMetadata(hostname));
		for (int i = 0; i < tenantDetectionMaxRetryCount; i++)
		{
			NavTenant result;
			if ((result = PollForTenantChange(tenantId, hostname)) != null)
			{
				return result;
			}
			if (i < tenantDetectionMaxRetryCount - 1)
			{
				Thread.Sleep(1000);
			}
		}
		return null;
	}

	private NavTenant PollForTenantChange(string tenantId, string hostname)
	{
		tenantConfigurationManager.CheckForConfigurationChanges();
		NavTenant value = null;
		if (string.IsNullOrEmpty(hostname))
		{
			TryGetTenantById(tenantId, out value);
		}
		else
		{
			alternateTenantIds?.TryGetValue(hostname, out value);
		}
		return value;
	}

	private NavTenant ResolveTenantByIdOrHost(string tenantId, string host)
	{
		NavTenant navTenant = (string.IsNullOrEmpty(tenantId) ? null : GetTenantById(tenantId));
		WaitForConfigurationCompleted();
		if (!string.IsNullOrEmpty(host) && alternateTenantIds.TryGetValue(host, out var value))
		{
			if (navTenant != null && navTenant != value)
			{
				throw new NavTenantInvalidIdException(string.Format(CultureInfo.CurrentCulture, Lang.TenantNamesAmbiguous, value.Id, tenantId));
			}
			return value;
		}
		return navTenant;
	}

	private bool TryUpdateUriWithTenantAlternateIdInfo(string tenantId, Func<string, IPAddress[]> getHostAddresses, ref Uri uri, string appId)
	{
		string dnsSafeHost = uri.DnsSafeHost;
		IPAddress[] serverAddresses;
		try
		{
			serverAddresses = getHostAddresses(dnsSafeHost);
		}
		catch (SocketException ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000007V", Category.Tenant, Verbosity.Warning, "Could not get host addresses for server host name: '{0}'. Tenant id {1}, message {2}.", DiagnosticParameter.SystemMetadata(dnsSafeHost), DiagnosticParameter.Oii(tenantId), DiagnosticParameter.SystemMetadata(ex.Message));
			return false;
		}
		IEnumerable<string> enumerable = FindAlternateTenantIdsForAppId(tenantId, appId);
		foreach (string item in enumerable)
		{
			if (Uri.CheckHostName(item) == UriHostNameType.Unknown)
			{
				continue;
			}
			try
			{
				if (getHostAddresses(item).Any((IPAddress ipAddress) => serverAddresses.Contains(ipAddress)))
				{
					uri = new Uri(uri.Scheme + Uri.SchemeDelimiter + item + uri.Authority.Substring(uri.Host.Length) + uri.PathAndQuery);
					uri = AddTenantParameterIfMissing(item, uri);
					return true;
				}
			}
			catch (SocketException ex2)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000007W", Category.Tenant, Verbosity.Warning, "Could not get host addresses for tenant alternate id: '{0}'. Tenant id {1}, message {2}.", DiagnosticParameter.Oii(item), DiagnosticParameter.Oii(tenantId), DiagnosticParameter.SystemMetadata(ex2.Message));
			}
		}
		return false;
	}

	private IEnumerable<string> FindAlternateTenantIdsForAppId(string tenantId, string appId)
	{
		NavTenant value = tenants.FirstOrDefault((KeyValuePair<string, NavTenant> t) => t.Value.Id.Equals(tenantId, StringComparison.OrdinalIgnoreCase)).Value;
		if (value == null)
		{
			throw new NavTenantNotFoundException();
		}
		return value.GetAlternateIdsForApplication(appId);
	}

	private static Uri AddTenantParameterIfMissing(string tenantId, Uri uri)
	{
		NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(uri.Query);
		if (nameValueCollection.Get("tenant") == null)
		{
			return new Uri(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}={3}", uri.OriginalString, string.IsNullOrEmpty(uri.Query) ? "?" : "&", "tenant", tenantId));
		}
		return uri;
	}

	private NavTenant ResolveTenantByIdAndCheckClientHostName(string tenantId, string clientHostName, bool includeFailed = false, bool enablePolling = false)
	{
		NavTenant navTenant = (string.IsNullOrEmpty(tenantId) ? null : GetTenantById(tenantId, includeFailed, enablePolling));
		if (navTenant == null)
		{
			return null;
		}
		if (!string.IsNullOrEmpty(clientHostName) && !DnsHelper.IsLocalhost(clientHostName))
		{
			WaitForConfigurationCompleted();
			if (alternateTenantIds.TryGetValue(clientHostName, out var value) && navTenant != value)
			{
				throw new NavTenantInvalidIdException(string.Format(CultureInfo.CurrentCulture, Lang.TenantNamesAmbiguous, value.Id, tenantId));
			}
		}
		return navTenant;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WaitForConfigurationCompleted()
	{
		if (NavEnvironment.Instance.State == NavEnvironmentState.Initializing)
		{
			try
			{
				configurationTask.Wait();
			}
			catch (Exception innerException)
			{
				throw new NavServerStateException(string.Format(CultureInfo.CurrentCulture, Lang.ServerNotReady, ProductNames.ProductNameServer), innerException);
			}
		}
	}

	public void ConfigureTenantsAsync(ServerUserSettings settings, string encryptionKey = null)
	{
		if (settings.Multitenant.Value)
		{
			if (!string.IsNullOrEmpty(settings.DatabaseName.Value))
			{
				try
				{
					AddSystemTenant(settings.ServerName, settings.DatabaseName.Value, settings.DatabaseUserName.Value, settings.ProtectedDatabasePassword, verifyConnection: true, encryptionKey, new DatabasePasswordPublicEncryptionKeyStorage());
				}
				catch (Exception ex)
				{
					NavServerEventSource.Log.WriteApplicationMountFailedEvent(NavEnvironment.ServiceInstanceName, ex.Message);
					throw;
				}
				configurationTask = NavTaskFactory.StartNewTask(() =>
				{
					PerformInitialMountingOfKnownTenants();
				}, TaskCreationOptions.LongRunning);
				configurationTask.TerminateOnException<NavBaseException>().LogOrFailOnException();
			}
		}
		else if (settings.DatabaseName != null && !string.IsNullOrEmpty(settings.DatabaseName.Value) && !string.IsNullOrEmpty(settings.ServerName))
		{
			EncryptionProvider encryptionProvider = EncryptionProvider.LocalKeyFile;
			AzureKeyVaultSettings azureKeyVaultSettings = null;
			if (settings.EncryptionProvider != null)
			{
				encryptionProvider = settings.EncryptionProvider.Value;
			}
			if (encryptionProvider == EncryptionProvider.AzureKeyVault)
			{
				azureKeyVaultSettings = new AzureKeyVaultSettings(settings.AzureKeyVaultClientId.Value, settings.AzureKeyVaultClientCertificateStoreLocation.Value, settings.AzureKeyVaultClientCertificateStoreName.Value, settings.AzureKeyVaultClientCertificateThumbprint.Value, settings.AzureKeyVaultKeyUri.Value);
			}
			AddSingleLegacyTenant(settings.ServerName, settings.DatabaseName.Value, settings.DatabaseUserName.Value, settings.ProtectedDatabasePassword, encryptionProvider, azureKeyVaultSettings);
		}
		NavEnvironment.Instance.ChangeState(NavEnvironmentState.Ready);
	}

	public void PerformInitialMountingOfKnownTenants(int syncPeriod = 10000, bool verifyServerInstanceKey = true)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTagOnThreshold("00000V4", Category.Runtime, Verbosity.Verbose, 100, (Stopwatch watch) =>
		{
			InitializeTenantCollection(syncPeriod);
			tenantConfigurationManager.PerformInitialMounting(verifyServerInstanceKey);
		}, PrivacyClassification.SystemMetadata, null, "Executing PerformInitialMountingOfKnownTenants");
	}

	private void MountPendingTenants(object state)
	{
		if (FeatureSwitches.Instance.DisableTenantParallelMounting)
		{
			return;
		}
		Parallel.For(0, pendingMountingTenants.Count, new ParallelOptions
		{
			MaxDegreeOfParallelism = Environment.ProcessorCount * 5
		}, (int index, ParallelLoopState parallelState) =>
		{
			NavMountingTask result = null;
			NavTenant navTenant = null;
			NavTenantMountingParameters navTenantMountingParameters = null;
			try
			{
				if (pendingMountingTenants.TryTake(out result))
				{
					result.AttemptNumber++;
					navTenant = result.Tenant;
					navTenantMountingParameters = result.Parameters;
					InitializeTenant(navTenant, navTenantMountingParameters.TenantSettings, navTenantMountingParameters.OverwriteTenantIdInDatabase, navTenantMountingParameters.VerifyDatabaseConnection, navTenantMountingParameters.EnsureDatabaseExclusiveStateWhileOverwritingTenantId);
				}
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex2)
			{
				if (result == null || navTenant == null || navTenantMountingParameters == null)
				{
					DiagnosticsResolver.GetMostSpecificInstance(navTenant).SendExceptionTag("00000XD", Category.Tenant, ex2);
				}
				else
				{
					if (result.AttemptNumber < 3)
					{
						pendingMountingTenants.Add(result);
					}
					else
					{
						SetTenantToFailedState(navTenant.TenantSettings);
					}
					string message = string.Join(Environment.NewLine, string.Format(CultureInfo.InvariantCulture, "Tenant '{0}' could not be mounted due to the following error: {1}", navTenant.Id, ex2.Message), ExceptionHandler.FormatException(ex2));
					NavServerEventSource.Log.WriteTenantMountFailedEvent(NavEnvironment.ServiceInstanceName, navTenant.Id, message);
					navTenant.Diagnostics.SendExceptionTag("00000V5", Category.Tenant, ex2);
				}
			}
		});
	}

	public void InitializeTenantCollection(int syncPeriod = 10000)
	{
		tenants = new LockFreeDictionary<string, NavTenant>(StringComparer.OrdinalIgnoreCase);
		InitializeTenantConfigurationManager(syncPeriod, NavEnvironment.TenantConfigurationSynchronizer);
		if (!FeatureSwitches.Instance.DisableTenantParallelMounting)
		{
			mountPendingTenantsTimer = new NonReEntrantTimerWrapper("mountPendingTenantsTimer", MountPendingTenants, null, 1000);
		}
	}

	internal void InitializeTenantConfigurationManager(int syncPeriod, ITenantConfigurationSynchronizer configurationSynchronizer)
	{
		tenantConfigurationManager = new TenantConfigurationManager(systemTenant.Database, syncPeriod, configurationSynchronizer, AddTenant, RemoveTenant, AddFailedTenant, ReplaceTenant, NavEnvironment.Instance.StandaloneInstance, () => NavEnvironment.Instance.GetServiceInstanceId());
	}

	public void AddSingleLegacyTenant(string databaseServerName, string databaseName, string databaseUserName, ProtectedDatabasePassword databasePassword, EncryptionProvider encryptionProvider = EncryptionProvider.LocalKeyFile, AzureKeyVaultSettings azureKeyVaultSettings = null)
	{
		if (string.IsNullOrEmpty(databaseServerName))
		{
			throw new ArgumentNullException("databaseServerName");
		}
		int count = tenants.Count;
		if (count > 0)
		{
			if ((count == 1 || DefaultTenant.Database == SystemTenant.Database) && SystemTenant.Database.DatabaseServer.NormalizedServerName == NavDatabaseServer.NormalizeServerName(databaseServerName) && SystemTenant.Database.NormalizedDatabaseName == NavDatabase.NormalizeDatabaseName(databaseName))
			{
				singleTenancyConfigured = true;
				return;
			}
			throw new InvalidOperationException("There should only be one tenant");
		}
		singleTenancyConfigured = true;
		NavEnvironment.Instance.DatabaseServers.AddServer(databaseServerName, databaseUserName, databasePassword);
		try
		{
			AddSystemTenant(databaseServerName, databaseName, databaseUserName, databasePassword, verifyConnection: true, null, new DatabasePasswordPublicEncryptionKeyStorage());
		}
		catch (Exception ex)
		{
			NavServerEventSource.Log.WriteApplicationMountFailedEvent(NavEnvironment.ServiceInstanceName, ex.Message);
			throw;
		}
		MountDefaultTenant(databaseServerName, databaseName, databaseUserName, databasePassword, encryptionProvider, azureKeyVaultSettings);
	}

	internal NavTenant MountDefaultTenant(string databaseServerName, string databaseName, string databaseUserName, ProtectedDatabasePassword databasePassword, EncryptionProvider encryptionProvider = EncryptionProvider.LocalKeyFile, AzureKeyVaultSettings azureKeyVaultSettings = null, bool verifyServerInstanceKey = true)
	{
		NavTenantSettings tenantSettings = new NavTenantSettings("default", databaseName, databaseServerName, databaseUserName, databasePassword, null, allowAppDatabaseWrite: true, ServerUserSettings.Instance.NASServicesStartupCodeunit.Value > 0, ServerUserSettings.Instance.ServicesDefaultCompany.Value, ServerUserSettings.Instance.ServicesDefaultTimeZone.Value, ServerUserSettings.Instance.NASServicesRunWithAdminRights.Value, encryptionProvider, azureKeyVaultSettings, ServerUserSettings.Instance.ExchangeAuthenticationMetadataLocation.Value, ServerUserSettings.Instance.AadTenantId, null, null, ServerUserSettings.Instance.TenantEnvironmentType.Value);
		return AddTenant(new NavTenantMountingParameters
		{
			TenantSettings = tenantSettings,
			VerifyServerInstanceKey = verifyServerInstanceKey
		});
	}

	internal void AddSystemTenant(string appDatabaseServerName, string appDatabaseName, string appDatabaseUserName, ProtectedDatabasePassword appDatabasePassword, bool verifyConnection = true, string newEncryptionKey = null, IDatabasePasswordPublicEncryptionKeyStorage keyStorage = null, bool allowOverwritePublicKey = false, bool verifyServerInstanceKey = true)
	{
		NavDatabaseServer orAddDatabaseServerByName = NavEnvironment.Instance.DatabaseServers.GetOrAddDatabaseServerByName(appDatabaseServerName, appDatabaseUserName, appDatabasePassword);
		if (string.IsNullOrEmpty(NavEnvironment.ServiceInstanceName) && NavEnvironment.Instance.StandaloneInstance)
		{
			NavEnvironment.ServiceInstanceName = appDatabaseServerName + "_" + appDatabaseName;
		}
		NavSystemTenant navSystemTenant = new NavSystemTenant(orAddDatabaseServerByName, appDatabaseName);
		try
		{
			if (verifyConnection)
			{
				navSystemTenant.Database.VerifyConnection(3);
			}
		}
		catch
		{
			navSystemTenant.Dispose();
			throw;
		}
		if (Interlocked.CompareExchange(ref systemTenant, navSystemTenant, null) != null)
		{
			throw new NavApplicationAlreadyMountedException("The application has already been mounted on the server instance.");
		}
		publicKeyStorage = keyStorage;
		try
		{
			SetPublicEncryptionKey(newEncryptionKey, allowOverwritePublicKey);
			if (appDatabasePassword != null)
			{
				CheckEncryptionKey(newEncryptionKey, verifyServerInstanceKey);
			}
			if (verifyConnection)
			{
				CheckCollation();
			}
		}
		catch
		{
			Volatile.Write(ref systemTenant, null);
			navSystemTenant.Dispose();
			throw;
		}
		if (onApplicationMounted != null && !NavEnvironment.Instance.StandaloneInstance)
		{
			onApplicationMounted();
		}
	}

	private void CheckCollation()
	{
		if (!systemTenant.Database.SqlSortingProperties.IsCollationCompatibleWithWindows)
		{
			throw new NavSqlException(string.Format(CultureInfo.CurrentCulture, Lang.OnlyWindowsCollationsSupported, systemTenant.Database.SqlSortingProperties.Collation, ProductNames.ProductNameClassicClient));
		}
	}

	internal void CheckEncryptionKey(string newEncryptionKey, bool verifyServerInstanceKey)
	{
		if (!string.IsNullOrEmpty(newEncryptionKey) || publicKeyStorage == null)
		{
			return;
		}
		if (PublicEncryptionKeyFromDatabase == null)
		{
			throw new NavEncryptionNotCreatedException("An encryption key must be imported using Import-NAVEncryptionKey before using Sql Authentication to access the database.");
		}
		if (verifyServerInstanceKey)
		{
			string publicKey;
			try
			{
				publicKey = ServerInstanceRsaEncryptionProvider.Instance.PublicKey;
			}
			catch (NavEncryptionNotCreatedException)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000007M", Category.Tenant, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Encryption key has not been set, verifyServerInstanceKey {0}, call stack {1}", verifyServerInstanceKey, new StackTrace(fNeedFileInfo: true).ToString());
				throw;
			}
			if (!publicEncryptionKeyFromDatabase.Equals(publicKey))
			{
				string message = string.Format(CultureInfo.InvariantCulture, "The NAV application could not be mounted for database '{0}' on database server '{1}' due to the following error: {2}", systemTenant.Database.DatabaseName, systemTenant.Database.DatabaseServer.ServerName, string.Format(CultureInfo.CurrentCulture, Lang.SystemEncryptionDecryptBadPasswordError, ProductNames.ProductNameServer));
				NavServerEventSource.Log.WriteApplicationMountFailedEvent(NavEnvironment.ServiceInstanceName, message);
				throw new NavApplicationMountFailedException(message);
			}
		}
	}

	private void SetPublicEncryptionKey(string newEncryptionKey, bool allowOverwriteNewPublicKey)
	{
		if (!string.IsNullOrEmpty(newEncryptionKey))
		{
			if (publicKeyStorage == null || systemTenant == null)
			{
				throw new InvalidOperationException("SystemTenant and PublicKeyStorage must be set prior to calling this method");
			}
			string publicKey = publicKeyStorage.GetPublicKey(systemTenant.Database);
			if (!string.IsNullOrEmpty(publicKey) && !allowOverwriteNewPublicKey && !publicKey.Equals(newEncryptionKey))
			{
				throw new NavEncryptionCreatedException("An encryption key has already been imported. Use the existing key or overwrite the current key using Import-NAVEncryptionKey -Force.");
			}
			if (string.IsNullOrEmpty(publicKey) | allowOverwriteNewPublicKey)
			{
				publicKeyStorage.PutPublicKey(systemTenant.Database, newEncryptionKey, allowOverwriteNewPublicKey);
				publicEncryptionKeyFromDatabase = newEncryptionKey;
			}
			else
			{
				publicEncryptionKeyFromDatabase = publicKey;
			}
		}
	}

	private void CheckAddTenant(string tenantId, IEnumerable<string> alternateIds)
	{
		if (IsSystemTenantId(tenantId))
		{
			throw new NavTenantInvalidIdException(string.Format(CultureInfo.CurrentCulture, Lang.TenantNameNotAllowed, tenantId));
		}
		if (tenants.ContainsKey(tenantId))
		{
			throw new NavTenantAlreadyExistsException(string.Format(CultureInfo.CurrentCulture, Lang.TenantAlreadyExists, tenantId));
		}
		if (alternateTenantIds.TryGetValue(tenantId, out var value))
		{
			throw new NavTenantAlreadyExistsException(string.Format(CultureInfo.CurrentCulture, Lang.TenantWithAlternateIdAlreadyExists, value.Id, tenantId, tenantId));
		}
		if (alternateIds == null)
		{
			return;
		}
		foreach (string alternateId in alternateIds)
		{
			if (IsSystemTenantId(alternateId))
			{
				throw new NavTenantInvalidIdException(string.Format(CultureInfo.CurrentCulture, Lang.TenantAlternateIdNotAllowed, alternateId));
			}
			if (tenants.TryGetValue(alternateId, out value) || alternateTenantIds.TryGetValue(alternateId, out value))
			{
				throw new NavTenantAlreadyExistsException(string.Format(CultureInfo.CurrentCulture, Lang.TenantWithAlternateIdAlreadyExists, value.Id, alternateId, tenantId));
			}
		}
	}

	private static bool IsSystemTenantId(string tenantId)
	{
		return string.Compare(tenantId, "nav-systemapplication", StringComparison.OrdinalIgnoreCase) == 0;
	}

	public NavTenant AddTenant(NavTenantMountingParameters mountingParameters)
	{
		NavTenantSettings tenantSettings = mountingParameters.TenantSettings;
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000007N", Category.Tenant, Verbosity.Verbose, "Adding tenant id={0}, overwrite={1}, verifydatabase={2}, verifyKey={3}, tenant database id={4}", DiagnosticParameter.Oii(tenantSettings.Id), DiagnosticParameter.SystemMetadata(mountingParameters.OverwriteTenantIdInDatabase), DiagnosticParameter.SystemMetadata(mountingParameters.VerifyDatabaseConnection), DiagnosticParameter.SystemMetadata(mountingParameters.VerifyServerInstanceKey), DiagnosticParameter.Oii(tenantSettings.TenantDatabaseId));
		CheckAddTenant(tenantSettings.Id, tenantSettings.AlternateId);
		bool flag = !FeatureSwitches.Instance.DisableTenantParallelMounting && mountingParameters.AllowAsync;
		NavTenant navTenant = CreateTenantInstance(mountingParameters, flag);
		if (!tenants.TryAdd(navTenant.Id, navTenant))
		{
			throw new NavTenantAlreadyExistsException(string.Format(CultureInfo.CurrentCulture, Lang.TenantAlreadyExists, tenantSettings.Id));
		}
		UpdateAlternateIds(tenantSettings, navTenant);
		if (!flag)
		{
			try
			{
				InitializeTenant(navTenant, tenantSettings, mountingParameters.OverwriteTenantIdInDatabase, mountingParameters.VerifyDatabaseConnection, mountingParameters.EnsureDatabaseExclusiveStateWhileOverwritingTenantId);
			}
			catch (Exception exception)
			{
				if (tenantConfigurationManager != null && tenantConfigurationManager.TenantConfigurationExists(navTenant.Id))
				{
					SetTenantToFailedState(navTenant.TenantSettings);
				}
				else
				{
					TryRemoveTenantInternal(navTenant.Id);
				}
				throw navTenant.Diagnostics.SendExceptionTag("00000Z7", Category.Tenant, exception);
			}
		}
		else
		{
			pendingMountingTenants.Add(new NavMountingTask
			{
				Parameters = mountingParameters,
				Tenant = navTenant
			});
		}
		if (navTenant.IsInExclusiveAccessMode)
		{
			navTenant.Diagnostics.SendTraceTag("00000XQ", Category.Runtime, Verbosity.Verbose, "Tenant {0} entered exclusive access mode on server (Server Instance ID = {1}). It was caused by other server request.", DiagnosticParameter.Oii(navTenant.Id), DiagnosticParameter.SystemMetadata(NavEnvironment.Instance.GetServiceInstanceId()));
		}
		return navTenant;
	}

	public void ReplaceTenant(NavTenantMountingParameters mountingParameters)
	{
		NavTenantSettings tenantSettings = mountingParameters.TenantSettings;
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000YK", Category.Tenant, Verbosity.Verbose, "Adding tenant id={0}, overwrite={1}, verifydatabase={2}, verifyKey={3}, tenant database id={4}", DiagnosticParameter.Oii(tenantSettings.Id), DiagnosticParameter.SystemMetadata(mountingParameters.OverwriteTenantIdInDatabase), DiagnosticParameter.SystemMetadata(mountingParameters.VerifyDatabaseConnection), DiagnosticParameter.SystemMetadata(mountingParameters.VerifyServerInstanceKey), DiagnosticParameter.Oii(tenantSettings.TenantDatabaseId));
		NavTenant navTenant = CreateTenantInstance(mountingParameters, mountTenantAsynchronously: false);
		InitializeTenant(navTenant, tenantSettings, mountingParameters.OverwriteTenantIdInDatabase, mountingParameters.VerifyDatabaseConnection, mountingParameters.EnsureDatabaseExclusiveStateWhileOverwritingTenantId);
		tenants.TryGetValue(navTenant.Id, out var value);
		value.State = NavTenantState.Mounting;
		ClearAlternateIds(value);
		tenants.AddOrUpdate(navTenant.Id, navTenant);
		UpdateAlternateIds(tenantSettings, navTenant);
		if (navTenant.IsInExclusiveAccessMode)
		{
			navTenant.Diagnostics.SendTraceTag("00000YL", Category.Runtime, Verbosity.Verbose, "Tenant {0} entered exclusive access mode on server (Server Instance ID = {1}). It was caused by other server request.", DiagnosticParameter.Oii(navTenant.Id), DiagnosticParameter.SystemMetadata(NavEnvironment.Instance.GetServiceInstanceId()));
		}
	}

	private void ClearAlternateIds(NavTenant tenant)
	{
		NavTenantSettings tenantSettings = tenant.TenantSettings;
		if (tenantSettings.AlternateId == null)
		{
			return;
		}
		foreach (string item in tenantSettings.AlternateId)
		{
			if (!string.IsNullOrWhiteSpace(item))
			{
				alternateTenantIds.Remove(item);
			}
		}
	}

	private NavTenant CreateTenantInstance(NavTenantMountingParameters mountingParameters, bool mountTenantAsynchronously)
	{
		NavTenantSettings tenantSettings = mountingParameters.TenantSettings;
		if (tenantSettings.ProtectedDatabasePassword != null)
		{
			CheckEncryptionKey(null, mountingParameters.VerifyServerInstanceKey);
		}
		NavDatabaseServer orAddDatabaseServerByName = NavEnvironment.Instance.DatabaseServers.GetOrAddDatabaseServerByName(tenantSettings.DatabaseServer, tenantSettings.DatabaseUserName, tenantSettings.ProtectedDatabasePassword);
		NavTenantState initialTenantState = GetInitialTenantState(mountTenantAsynchronously);
		NavTenant navTenant = new NavTenant(tenantSettings, orAddDatabaseServerByName, initialTenantState, mountingParameters.ExclusiveAccessMode);
		if (IsSingleTenancyMode)
		{
			MakeSureAppsInAppDatabaseMatchTenantDatabaseForSingleTenantMode(navTenant);
		}
		return navTenant;
	}

	private static NavTenantState GetInitialTenantState(bool mountTenantAsynchronously)
	{
		if (mountTenantAsynchronously)
		{
			return NavTenantState.Mounting;
		}
		return NavTenantState.Mounted;
	}

	private void UpdateAlternateIds(NavTenantSettings tenantSettings, NavTenant tenant)
	{
		if (tenantSettings.AlternateId == null)
		{
			return;
		}
		foreach (string item in tenantSettings.AlternateId)
		{
			if (!string.IsNullOrWhiteSpace(item))
			{
				alternateTenantIds.AddOrUpdate(item, tenant);
			}
		}
	}

	private void InitializeTenant(NavTenant tenant, NavTenantSettings tenantSettings, bool overwriteTenantIdInDatabase, bool verifyDatabaseConnection, bool ensureDatabaseExclusiveState)
	{
		try
		{
			InitializeAppsForTenant(tenant);
			if (overwriteTenantIdInDatabase)
			{
				tenant.Database.OverwriteTenantId(ensureDatabaseExclusiveState);
				tenant.Database.TenantProperties.Refresh();
			}
			else if (verifyDatabaseConnection && !tenant.Database.IsNewTenantDatabase && tenant.Database.TenantProperties.IsNewTenant)
			{
				throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.TenantIdMismatch, tenant.Database.DatabaseName, tenantSettings.Id));
			}
			if ((overwriteTenantIdInDatabase | verifyDatabaseConnection) && tenant.Database.IsSharedTenantSchema)
			{
				if (tenant.Database.TenantProperties.IsBufferTenant)
				{
					throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000SY", Category.Tenant, NavBufferTenantsCannotBeMountedException.Create(tenantSettings.Id, tenantSettings.DatabaseName));
				}
				if (!tenant.DataIsCompatibleWithApplication())
				{
					NavTenantDatabaseException ex = new NavTenantDatabaseException(string.Format(CultureInfo.InvariantCulture, "Tenant {1} cannot be used because it is assigned a data version ({0}) that is greater than the Application Version ({2}) supported on the {3} instance.", tenant.Database.TenantProperties.TenantDataVersion, tenant.Id, NavGlobal.AppDatabase.SqlDatabaseProperties.ApplicationVersion, ProductNames.ProductNameServer));
					DiagnosticsResolver.GetMostSpecificInstance(tenant).SendExceptionTag("00000VX", Category.Tenant, ex);
					throw ex;
				}
			}
			tenant.State = NavTenantState.Mounted;
			UpdateMountedTenantPerfCounter();
			tenant.RefreshState(useTenantDatabaseToCalculateState: false);
			tenant.OnTenantAdded();
			tenant.LogTenantAzureUsage();
			NavServerEventSource.Log.WriteTenantMountedEvent(NavEnvironment.ServiceInstanceName, tenant.Id, string.Format(CultureInfo.InvariantCulture, "Tenant '{0}' from database '{1}' on database server '{2}' has been mounted.", tenant.Id, tenant.TenantSettings.DatabaseName, tenant.TenantSettings.DatabaseServer));
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance(tenant).SendExceptionTagExceptionFilter("00000V6", Category.Tenant, exception);
			tenant.State = NavTenantState.Failed;
			tenant.ReleaseInitializationLock();
			throw;
		}
		try
		{
			tenant.ProcessTenantActions();
		}
		catch (Exception exception2)
		{
			DiagnosticsResolver.GetMostSpecificInstance(tenant).SendExceptionTag("00000V7", Category.Tenant, exception2);
		}
	}

	private void InitializeAppsForTenant(NavTenant tenant)
	{
		try
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000OB", Category.Extensions, Verbosity.Normal, "Updating tenant extension group (add tenant) -- Tenant:{0}", DiagnosticParameter.Oii(tenant.Id));
			navAppGroupUpdater.UpdateGroupForChangedTenant(tenant);
		}
		catch (Exception exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000009Y", Category.Extensions, exception, Verbosity.Error, DiagnosticParameter.Oii("Adding tenant aborted because of an extension failure -- Tenant: " + tenant.Id));
			throw;
		}
	}

	private static void MakeSureAppsInAppDatabaseMatchTenantDatabaseForSingleTenantMode(NavTenant tenant)
	{
		try
		{
			List<INavAppMetadataReference> appsInstalledForTenantInTenantDatabase = NavAppInstallationManagement.GetAppsInstalledForTenantInTenantDatabase(tenant.Database);
			List<Guid> first = (from m in NavAppInstallationManagement.GetAppsInstalledForTenantInTenantDatabase(tenant.Database)
				select ((WrappedValue<Guid>)(object)m.PackageId).Value into guid
				orderby guid
				select guid).ToList();
			List<Guid> second = (from packageId in NavAppInstallationManagement.GetAppsInstalledForTenant(NavGlobal.AppDatabase, tenant.Id)
				select ((WrappedValue<Guid>)(object)packageId).Value into guid
				orderby guid
				select guid).ToList();
			if (first.SequenceEqual(second))
			{
				return;
			}
			NavAppInstallationManagement.PurgeInstalledAndPublishedTenantAppsFromApplicationDatabase(NavGlobal.AppDatabase, tenant.Id);
			foreach (INavAppMetadataReference item in appsInstalledForTenantInTenantDatabase)
			{
				NavAppInstallationManagement.InstallAppToApplicationDatabaseWithRetries(NavGlobal.AppDatabase, tenant.Id, item);
			}
		}
		catch (NavCSideException)
		{
		}
		catch (NavSqlException)
		{
		}
	}

	internal void AddFailedTenant(NavTenantSettings tenantSettings)
	{
		NavTenant value = new NavTenant(tenantSettings, null, NavTenantState.Failed);
		if (!tenants.TryAdd(tenantSettings.Id, value))
		{
			CheckAddTenant(tenantSettings.Id, tenantSettings.AlternateId);
		}
		UpdateMountedTenantPerfCounter();
	}

	internal void SetTenantToFailedState(NavTenantSettings tenantSettings)
	{
		NavTenant value = new NavTenant(tenantSettings, null, NavTenantState.Failed);
		tenants.AddOrUpdate(tenantSettings.Id, value);
	}

	public void RemoveTenant(string tenantId)
	{
		if (!TryRemoveTenant(tenantId))
		{
			throw NavTenantNotFoundException.CreateForGeneralNotFound(tenantId);
		}
	}

	public NavTenantSettings RemoveTenantAndReturn(string tenantId)
	{
		tenants.TryGetValue(tenantId, out var value);
		NavTenantSettings result = value?.TenantSettings;
		RemoveTenant(tenantId);
		return result;
	}

	public bool TryRemoveTenant(string tenantId)
	{
		if (!TryRemoveTenantInternal(tenantId))
		{
			return false;
		}
		string message = string.Format(CultureInfo.InvariantCulture, "Tenant '{0}' was dismounted.", tenantId);
		NavServerEventSource.Log.WriteTenantDismountedEvent(NavEnvironment.ServiceInstanceName, tenantId, message);
		return true;
	}

	private bool TryRemoveTenantInternal(string tenantId)
	{
		NavTenant tenant = tenants.Remove(tenantId);
		if (tenant == null)
		{
			return false;
		}
		ClearAlternateIds(tenant);
		UpdateMountedTenantPerfCounter();
		Task task = NavTaskFactory.RunTask(() =>
		{
			tenant.Dispose();
		});
		Task.WaitAll(new Task[1] { task }, 5000);
		return true;
	}

	private void UpdateMountedTenantPerfCounter()
	{
		if (!tenants.IsDisposed)
		{
			NavEnvironment.Instance.PerformanceCounterSetter.UpdateNumberOfTenantsCounter(tenants.Count((KeyValuePair<string, NavTenant> t) => t.Value.State != NavTenantState.Mounting));
		}
	}

	public void Dispose()
	{
		if (mountPendingTenantsTimer != null)
		{
			mountPendingTenantsTimer.Dispose();
			mountPendingTenantsTimer = null;
		}
		IEnumerable<KeyValuePair<string, NavTenant>> enumerable = tenants.ClearAndDestroy();
		InterlockedAction.Dispose(ref tenantConfigurationManager);
		foreach (KeyValuePair<string, NavTenant> item in enumerable)
		{
			item.Value.Dispose();
		}
		Volatile.Read(ref systemTenant)?.Dispose();
	}
}
