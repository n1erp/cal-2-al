using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class AzureLicenseUsageService
{
	private static readonly TimeSpan FlushLoggerTimeout = TimeSpan.FromHours(1.0);

	private static Lazy<AzureLicenseUsageService> instance = new Lazy<AzureLicenseUsageService>(() => new AzureLicenseUsageService());

	private readonly Queue<Tuple<string, WeakReference<NavTenant>>> telemetryEmissionQueue = new Queue<Tuple<string, WeakReference<NavTenant>>>();

	private readonly HashSet<string> queuedTenantIds = new HashSet<string>();

	private readonly IAzurePlatformInformationProvider azurePlatformInformationProvider;

	private ILogger logger;

	private AzurePlatformInformation azurePlatformInformation;

	private AzureLicenseUsagePayloadBuilder azureLicensePayloadBuilder;

	private bool notRunningOnAzureVm;

	private bool stop = true;

	private Thread processQueueThread;

	private ManualResetEventSlim manualResetEvent = new ManualResetEventSlim(initialState: true);

	private object syncLock = new object();

	private readonly bool enableTimer;

	private readonly TimeSpan timerInterval;

	private NonReEntrantTimerWrapper logAllTenantsTimer;

	private readonly Func<IEnumerable<NavTenant>> getAllTenantsProvider;

	public static AzureLicenseUsageService Instance => instance.Value;

	public bool IsRunning
	{
		get
		{
			if (processQueueThread != null)
			{
				return processQueueThread.IsAlive;
			}
			return false;
		}
	}

	public AzureLicenseUsageService()
	{
		azurePlatformInformationProvider = new AzurePlatformInformationProvider(NavDiagnostics.GlobalInstance);
		enableTimer = true;
		timerInterval = NavTenant.AzureTelemetryEmissionPeriod;
		getAllTenantsProvider = () => NavEnvironment.Instance.Tenants.AllTenants(includeSystemTenant: true);
	}

	internal AzureLicenseUsageService(ILogger logger, AzureLicenseUsagePayloadBuilder azureLicensePayloadBuilder, IAzurePlatformInformationProvider azurePlatformInformationProvider, bool enableTimer, TimeSpan timerInterval, Func<IEnumerable<NavTenant>> getAllTenantsProvider)
	{
		this.logger = logger;
		this.azureLicensePayloadBuilder = azureLicensePayloadBuilder;
		this.azurePlatformInformationProvider = azurePlatformInformationProvider;
		this.enableTimer = enableTimer;
		this.timerInterval = timerInterval;
		this.getAllTenantsProvider = getAllTenantsProvider;
	}

	public void Start()
	{
		stop = false;
		lock (syncLock)
		{
			if (logAllTenantsTimer == null && enableTimer)
			{
				logAllTenantsTimer = new NonReEntrantTimerWrapper("AzureLicenseUsageService", EnqueueAllActiveTenants, null, (int)timerInterval.TotalMilliseconds);
			}
		}
	}

	public void Stop()
	{
		stop = true;
		manualResetEvent.Set();
		lock (syncLock)
		{
			NonReEntrantTimerWrapper nonReEntrantTimerWrapper = logAllTenantsTimer;
			if (nonReEntrantTimerWrapper != null)
			{
				logAllTenantsTimer = null;
				nonReEntrantTimerWrapper.Dispose();
			}
		}
	}

	public bool EnqueueTenant(NavTenant tenant)
	{
		if (tenant == null)
		{
			throw new ArgumentNullException("tenant");
		}
		if (ServerUserSettings.Instance.DisableAzureLicenseLogging.Value)
		{
			return false;
		}
		if (tenant.TenantType != NavTenantType.Normal || tenant.Database.DatabaseType != NavDatabaseType.Tenant)
		{
			return false;
		}
		if (tenant.Database.SecurityAndLicense.License.IsDemoLicense || (!ServerUserSettings.Instance.AzureLicenseLoggingLogMicrosoftAccount.Value && AzureLicenseUsagePayloadBuilder.IsLicenseInternal(tenant.Database.SecurityAndLicense.License)))
		{
			return false;
		}
		lock (syncLock)
		{
			if (queuedTenantIds.Contains(tenant.Id))
			{
				return false;
			}
			telemetryEmissionQueue.Enqueue(new Tuple<string, WeakReference<NavTenant>>(tenant.Id, new WeakReference<NavTenant>(tenant)));
			queuedTenantIds.Add(tenant.Id);
			if (!stop && !IsRunning)
			{
				processQueueThread = new Thread(ProcessTenantAzureLicenseQueue);
				processQueueThread.Name = "ProcessTenantAzureLicenseQueue";
				processQueueThread.Start();
			}
		}
		manualResetEvent.Set();
		return true;
	}

	private void EnqueueAllActiveTenants(object state)
	{
		foreach (NavTenant item in getAllTenantsProvider())
		{
			item.LogTenantAzureUsage();
		}
	}

	internal void ProcessTenantAzureLicenseQueue()
	{
		try
		{
			do
			{
				Tuple<string, WeakReference<NavTenant>> tuple = null;
				lock (syncLock)
				{
					if (telemetryEmissionQueue.Count == 0)
					{
						manualResetEvent.Reset();
					}
					else
					{
						tuple = telemetryEmissionQueue.Dequeue();
						queuedTenantIds.Remove(tuple.Item1);
					}
				}
				if (tuple != null)
				{
					if (azurePlatformInformation == null && !notRunningOnAzureVm && !azurePlatformInformationProvider.TryReadAzurePlatformInfo(out azurePlatformInformation))
					{
						notRunningOnAzureVm = true;
					}
					if (tuple.Item2.TryGetTarget(out var target) && !target.IsDisposed && target.State == NavTenantState.Operational)
					{
						try
						{
							ProcessTenantAzureLicense(target);
						}
						catch (ObjectDisposedException)
						{
						}
						catch (NavSqlException exception)
						{
							DiagnosticsResolver.GetMostSpecificInstance(target).SendExceptionTag("00001L1", Category.License, exception, Verbosity.Warning, DiagnosticParameter.SystemMetadata("SQL exception while emitting telemetry for a tenant, ignoring tenant and continue."));
						}
					}
				}
				try
				{
					if (!manualResetEvent.Wait(FlushLoggerTimeout))
					{
						logger?.Flush();
					}
				}
				catch (Exception)
				{
					logger?.Flush();
					return;
				}
			}
			while (!stop);
			logger?.Flush();
		}
		catch (Exception exception2)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001L2", Category.License, exception2, Verbosity.Error, DiagnosticParameter.SystemMetadata("Unhandled exception in Azure license usage service, stopping it."));
			Stop();
		}
	}

	private void ProcessTenantAzureLicense(NavTenant tenant)
	{
		if (azureLicensePayloadBuilder == null)
		{
			azureLicensePayloadBuilder = new AzureLicenseUsagePayloadBuilder();
			azureLicensePayloadBuilder.Add(azurePlatformInformation);
		}
		azureLicensePayloadBuilder.Add(tenant);
		if (logger == null)
		{
			logger = new AppInsightAzureLicenseUsageLogger();
		}
		logger.Log(azureLicensePayloadBuilder.Build());
	}
}
