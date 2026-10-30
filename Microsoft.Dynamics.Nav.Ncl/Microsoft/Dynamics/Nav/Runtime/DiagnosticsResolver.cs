using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class DiagnosticsResolver
{
	public static NavDiagnostics GetMostSpecificInstance(NavTenant navTenant = null)
	{
		return GetMostSpecificInstance(NavCurrentThread.Session, navTenant);
	}

	public static NavDiagnostics GetMostSpecificInstance(NavSession session, NavTenant navTenant = null)
	{
		return session?.Diagnostics ?? session?.Tenant?.Diagnostics ?? navTenant?.Diagnostics ?? NavDiagnostics.GlobalInstance;
	}

	public static ITraceScope CreateTraceScope(string tag, DiagnosticParameter name, MonitorType monitorTypes, Category category, bool createNewActivity = true)
	{
		return GetMostSpecificInstance().CreateTraceScope(tag, name, monitorTypes, category, createNewActivity);
	}

	public static ITraceScope CreateTimingScope(string tag, Category category, DiagnosticParameter name)
	{
		return GetMostSpecificInstance().CreateTimingScope(tag, category, name);
	}
}
