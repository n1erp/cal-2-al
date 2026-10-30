using System;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TenantStateReporter : IDisposable
{
	internal const string MetricName = "TenantStateMetric";

	private readonly NavMetric3Dimensions TenantStateMetric = new NavMetric3Dimensions("TenantStateMetric", "TenantId", "State", "AadTenantId");

	private readonly TimerCallback tenantStateReporterAction;

	private NonReEntrantTimerWrapper tenantStateReporterTimer;

	public static readonly int Period = (int)Math.Min(2147483647.0, ServerUserSettings.Instance.TenantStateReportingPeriod.Value.TotalMilliseconds);

	public TenantStateReporter(TimerCallback tenantStateReporterAction = null, Func<TimerCallback, NonReEntrantTimerWrapper> timerCreator = null)
	{
		this.tenantStateReporterAction = tenantStateReporterAction ?? new TimerCallback(LogTenantsState);
		tenantStateReporterTimer = ((timerCreator != null) ? timerCreator(this.tenantStateReporterAction) : new NonReEntrantTimerWrapper("TenantStateReporter", this.tenantStateReporterAction, this, Period));
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			InterlockedAction.Dispose(ref tenantStateReporterTimer);
		}
	}

	internal void Stop()
	{
		Dispose();
	}

	private void LogTenantsState(object state)
	{
		foreach (NavTenant item in from t in NavEnvironment.Instance.Tenants.AllTenants(includeSystemTenant: false, includeFailedTenants: true)
			where t.State != NavTenantState.Operational && t.State != NavTenantState.Mounted
			select t)
		{
			TenantStateMetric.LogValue(1L, item.Id, EnumHelper<NavTenantState>.EnumToString(item.State), item.TenantSettings.AadTenantId);
		}
	}
}
