using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Reporting.WinForms;

namespace Microsoft.Dynamics.Nav.Types;

public static class ReportAppDomain
{
	internal sealed class ReportAppDomainStatistics : MarshalByRefObject
	{
		public override object InitializeLifetimeService()
		{
			return null;
		}

		public int GetTotalLocalReportRenderings()
		{
			return reportRenderingCount;
		}

		public int GetOpenLocalReportInstanceCount()
		{
			return openLocalReportProxies;
		}
	}

	internal sealed class InternalReportAppDomain
	{
		public AppDomain AppDomain { get; }

		public ReportAppDomainStatistics Statistics { get; }

		public InternalReportAppDomain(AppDomain appDomain)
		{
			AppDomain = appDomain;
			Type typeFromHandle = typeof(ReportAppDomainStatistics);
			Statistics = (ReportAppDomainStatistics)appDomain.CreateInstanceAndUnwrap(typeFromHandle.Assembly.FullName, typeFromHandle.FullName);
			if (!AppDomain.MonitoringIsEnabled)
			{
				AppDomain.MonitoringIsEnabled = true;
			}
		}
	}

	private static int openLocalReportProxies;

	private static int reportRenderingCount;

	internal static int AppDomainIndex;

	private static readonly object reportingAppDomainLock = new object();

	private static InternalReportAppDomain reportingAppDomain;

	public static bool LegacyCasEnabled => AppDomain.CurrentDomain.ApplicationTrust == null;

	internal static bool ForceToCurrentAppDomain { get; set; }

	public static int IncrementOpenLocalReport()
	{
		Interlocked.Increment(ref reportRenderingCount);
		return Interlocked.Increment(ref openLocalReportProxies);
	}

	public static int DecrementOpenLocalReport()
	{
		return Interlocked.Decrement(ref openLocalReportProxies);
	}

	private static InternalReportAppDomain ReportingAppDomain()
	{
		lock (reportingAppDomainLock)
		{
			if (reportingAppDomain != null && reportingAppDomain.Statistics.GetTotalLocalReportRenderings() > ServerUserSettings.Instance.ReportAppDomainRecycleCount.Value && reportingAppDomain.Statistics.GetOpenLocalReportInstanceCount() == 0)
			{
				using (NavDiagnostics.GlobalInstance.CreateTimingScope("00007OB", Category.Runtime, DiagnosticParameter.SystemMetadata("Reporting - Time spent cleaning up Reporting AppDomain")))
				{
					try
					{
						AppDomain appDomain = reportingAppDomain.AppDomain;
						reportingAppDomain = null;
						AppDomain.Unload(appDomain);
					}
					catch (AppDomainUnloadedException exception)
					{
						NavDiagnostics.GlobalInstance.SendExceptionTag("00007OC", Category.Runtime, exception);
					}
				}
			}
			if (reportingAppDomain == null)
			{
				int num = AppDomainIndex++;
				string directoryName = Path.GetDirectoryName(new Uri(typeof(LocalReport).Assembly.CodeBase).LocalPath);
				AppDomainSetup appDomainSetup = new AppDomainSetup
				{
					ApplicationName = "ReportingAppDomain",
					ApplicationBase = directoryName,
					DynamicBase = directoryName,
					LoaderOptimization = LoaderOptimization.MultiDomainHost,
					CachePath = AppDomain.CurrentDomain.SetupInformation.CachePath,
					PrivateBinPath = directoryName,
					DisallowCodeDownload = true
				};
				appDomainSetup.SetCompatibilitySwitches(new string[1] { "NetFx40_LegacySecurityPolicy" });
				reportingAppDomain = new InternalReportAppDomain(AppDomain.CreateDomain("ReportingAppDomain" + num, null, appDomainSetup));
				NavDiagnostics.GetMostSpecificInstance.SendTraceTag("0000C67", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Creating new reporting AppDomain: " + reportingAppDomain.AppDomain.FriendlyName);
			}
			return reportingAppDomain;
		}
	}

	internal static LocalReportHandle.LocalReportProxy CreateLocalReportProxy(bool enableAppDomainIsolation = false, bool enableAppDomainMonitoring = true)
	{
		if (ServerUserSettings.Instance.PreviousReportAppDomainIsolation.Value)
		{
			if (((ForceToCurrentAppDomain || LegacyCasEnabled) | enableAppDomainIsolation) || ServerUserSettings.Instance.LegacyReportAppDomainIsolation.Value)
			{
				NavDiagnostics.GetMostSpecificInstance.SendTraceTag("00009AD", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, $"LocalReportProxy condition CAS={LegacyCasEnabled}, Isolation={enableAppDomainIsolation}, LegacyIsolation={ServerUserSettings.Instance.LegacyReportAppDomainIsolation}");
				return new LocalReportHandle.LocalReportProxy(enableAppDomainMonitoring);
			}
		}
		else if (ForceToCurrentAppDomain || LegacyCasEnabled || !enableAppDomainIsolation || ServerUserSettings.Instance.LegacyReportAppDomainIsolation.Value)
		{
			NavDiagnostics.GetMostSpecificInstance.SendTraceTag("00009AD", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, $"LocalReportProxy condition CAS={LegacyCasEnabled}, Isolation={enableAppDomainIsolation}, LegacyIsolation={ServerUserSettings.Instance.LegacyReportAppDomainIsolation}");
			return new LocalReportHandle.LocalReportProxy(enableAppDomainMonitoring);
		}
		return CreateLocalReportProxyInAppDomain(ReportingAppDomain());
	}

	private static LocalReportHandle.LocalReportProxy CreateLocalReportProxyInAppDomain(InternalReportAppDomain appDomain)
	{
		NavDiagnostics.GetMostSpecificInstance.SendTraceTag("00009AE", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Create report app domain " + appDomain.AppDomain.FriendlyName);
		Type typeFromHandle = typeof(LocalReportHandle.LocalReportProxy);
		LocalReportHandle.LocalReportProxy localReportProxy = (LocalReportHandle.LocalReportProxy)appDomain.AppDomain.CreateInstanceAndUnwrap(typeFromHandle.Assembly.FullName, typeFromHandle.FullName, ignoreCase: false, BindingFlags.Default, null, new object[1] { true }, null, null);
		localReportProxy.IsInRemoteAppDomain = true;
		localReportProxy.Name = appDomain.AppDomain.FriendlyName;
		return localReportProxy;
	}
}
