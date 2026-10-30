using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavApplicationServer
{
	private static readonly TimeSpan DebugStartupDelayPeriod = TimeSpan.FromMinutes(1.0);

	private readonly NavApplicationServerConfiguration configuration;

	private readonly NavTenant tenant;

	private readonly IClientCallback callback;

	private readonly Action runCodeunit;

	public NavApplicationServer(NavTenant navTenant)
		: this(navTenant, new NavApplicationServerConfiguration(ServerUserSettings.Instance.NASServicesStartupCodeunit.Value, ServerUserSettings.Instance.NASServicesStartupMethod.Value, ServerUserSettings.Instance.NASServicesStartupArgument.Value, ServerUserSettings.Instance.NASServicesEnableDebugging.Value, TimeSpan.FromMinutes(5.0)))
	{
	}

	public NavApplicationServer(NavTenant tenant, NavApplicationServerConfiguration configuration, IClientCallback callback = null, Action runCodeunit = null)
	{
		if (tenant == null)
		{
			throw new ArgumentNullException("tenant");
		}
		if (configuration == null)
		{
			throw new ArgumentNullException("configuration");
		}
		this.configuration = configuration;
		this.tenant = tenant;
		this.callback = callback;
		this.runCodeunit = runCodeunit ?? new Action(RunCodeunitMethod);
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		List<DateTime> failureTimestamps = new List<DateTime>();
		while (true)
		{
			try
			{
				await NavTaskFactory.StartNewTask(() => RunOnceAsync(cancellationToken), TaskCreationOptions.None).Unwrap();
				NavServerEventSource.Log.WriteNasCompletedEvent(NavEnvironment.ServiceInstanceName, tenant.Id, string.Format(CultureInfo.CurrentCulture, Lang.NasCompleted, ProductNames.NasName, tenant.Id));
				break;
			}
			catch (NavDatabasePasswordException e)
			{
				LogNasPermanentlyFailedEvent(e);
				break;
			}
			catch (NavBaseException ex)
			{
				if (ex is NavNCLMetadataException || ex is NavNCLMissingMethodException || ex is NavObjectNotCompiledException)
				{
					LogNasPermanentlyFailedEvent(ex);
					break;
				}
				int timeoutBeforeRestart = GetTimeoutBeforeRestart(ex, ref failureTimestamps);
				if (ex is NavSqlException ex2 && NavSqlReturnCode.IsTransientError(ex2.ErrorNumber))
				{
					tenant.Diagnostics.SendExceptionTag("00000BR", Category.Nas, ex, Verbosity.Warning);
				}
				else
				{
					NavServerEventSource.Log.WriteNasFailedEvent(NavEnvironment.ServiceInstanceName, tenant.Id, string.Format(CultureInfo.CurrentCulture, Lang.NasSessionFailedWillRetry, ProductNames.NasName, tenant.Id, timeoutBeforeRestart / 1000, ExceptionHandler.FormatException(ex)));
				}
				Thread.Sleep(timeoutBeforeRestart);
			}
			catch (NavServerSessionBaseException)
			{
				if (!NavEnvironment.Instance.InServerShutdown)
				{
					NavServerEventSource.Log.WriteNasPermanentlyFailedEvent(NavEnvironment.ServiceInstanceName, tenant.Id, string.Format(CultureInfo.CurrentCulture, Lang.NasSessionStopped, ProductNames.NasName, tenant.Id));
				}
				break;
			}
			catch (ObjectDisposedException e2)
			{
				LogNasPermanentlyFailedEvent(e2);
				break;
			}
			catch (Exception e3)
			{
				NavEnvironment.WriteWatsonLog(e3);
				LogNasPermanentlyFailedEvent(e3);
				throw;
			}
		}
	}

	private int GetTimeoutBeforeRestart(NavBaseException e, ref List<DateTime> failureTimestamps)
	{
		DateTime utcNow = DateTime.UtcNow;
		DateTime cutoffTime = utcNow - configuration.RetryPeriod;
		failureTimestamps = failureTimestamps.Where((DateTime f) => f >= cutoffTime).ToList();
		failureTimestamps.Add(utcNow);
		int num = 0;
		if (failureTimestamps.Count > 6)
		{
			num = configuration.RetryDelayAfter6Failures;
		}
		else if (failureTimestamps.Count > 3)
		{
			num = configuration.RetryDelayAfter3Failures;
		}
		if (e is NavCSideException { ErrorCode: 22924076 })
		{
			num = Math.Max(num, tenant.Database.DatabaseServer.SqlConnectionPool.ReconnectDelayTime);
		}
		return num;
	}

	private void LogNasPermanentlyFailedEvent(Exception e)
	{
		string text = (tenant.TenantSettings.RunNasWithAdminRights ? "NAS Administrator" : WindowsIdentity.GetCurrent().Name);
		NavServerEventSource.Log.WriteNasPermanentlyFailedEvent(NavEnvironment.ServiceInstanceName, tenant.Id, string.Format(CultureInfo.CurrentCulture, Lang.NasSessionFailedWillNotRetry, ProductNames.NasName, tenant.Id, text, ExceptionHandler.FormatException(e)));
		tenant.OnNasPermanentlyFailed();
	}

	private async Task RunOnceAsync(CancellationToken cancellationToken)
	{
		NavServerEventSource.Log.WriteNasStartingEvent(NavEnvironment.ServiceInstanceName, tenant.Id, string.Format(CultureInfo.CurrentCulture, Lang.NasStarting, ProductNames.NasName, tenant.TenantSettings.DefaultCompany, configuration.StartupCodeunit, string.IsNullOrEmpty(configuration.StartupMethod) ? "OnRun" : configuration.StartupMethod, configuration.Argument, tenant.Id));
		NavSession session = null;
		try
		{
			NavUserAuthentication navUserAuthentication = NavUserAuthentication.CreateNasUserAuthentication(tenant);
			session = new NavSession(tenant, tenant.TenantSettings.DefaultTimeZone, navUserAuthentication, navUserAuthentication.AppId);
			session.AccessLock.BeginExclusiveExecution();
			session.ClientConnectionType = ConnectionType.Nas;
			session.ClientCallbackOverride = callback ?? new HeadlessClientCallback(ProductNames.NasName, session);
			session.Open();
			session.SetCulture(ServerUserSettings.Instance.DefaultLanguage.Value);
			if (configuration.DebugMode)
			{
				LogWriterHelper.LogEvent(EventLogEntryType.Information, string.Format(CultureInfo.CurrentCulture, Lang.NasStartingInDebugMode, ProductNames.NasName, tenant.Id));
				NavCurrentThread.Session = null;
				session.AccessLock.EndExclusiveExecution();
				await Task.WhenAny(Task.Delay(DebugStartupDelayPeriod), session.AccessLock.WaitForStoppingAsync());
				session.AccessLock.BeginExclusiveExecution();
				NavCurrentThread.Session = session;
			}
			string servicesDefaultCompanyName = session.GetServicesDefaultCompanyName();
			if (!string.IsNullOrEmpty(servicesDefaultCompanyName))
			{
				session.OpenCompany(servicesDefaultCompanyName);
			}
			session.TransactionScope(runCodeunit);
			session.AccessLock.EndExclusiveExecution();
			NavCurrentThread.Session = null;
			if (!IsListeningToEvents(session))
			{
				session.Dispose();
				return;
			}
			Task task = session.AccessLock.WaitForDisposedAsync();
			if (!cancellationToken.CanBeCanceled)
			{
				await task;
			}
			else
			{
				cancellationToken.ThrowIfCancellationRequested();
				TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
				cancellationToken.Register(taskCompletionSource.SetCanceled);
				await Task.WhenAny(task, taskCompletionSource.Task);
				cancellationToken.ThrowIfCancellationRequested();
			}
			if (session.AccessLock.ExternalStopRequest || session.LastException == null)
			{
				session.AccessLock.BeginExclusiveExecution();
			}
			session.LastException.Rethrow();
		}
		catch (Exception ex)
		{
			if (session != null)
			{
				session.SessionComment = ex.Message;
				session.DisposeAsync().LogOrFailOnException();
			}
			NavCurrentThread.Session = null;
			throw;
		}
	}

	private static bool IsListeningToEvents(ITreeObject treeObject)
	{
		if (!(treeObject is NavDotNet { HasEventSink: not false }))
		{
			return treeObject.Tree.Children.Any(IsListeningToEvents);
		}
		return true;
	}

	private void RunCodeunitMethod()
	{
		if (string.IsNullOrEmpty(configuration.StartupMethod))
		{
			NavCodeunit.InvokeRun(configuration.StartupCodeunit, string.IsNullOrEmpty(configuration.Argument) ? null : new object[1] { configuration.Argument });
		}
		else
		{
			NavCodeunit.Invoke(configuration.StartupCodeunit, configuration.StartupMethod, string.IsNullOrEmpty(configuration.Argument) ? null : new object[1] { configuration.Argument });
		}
	}
}
