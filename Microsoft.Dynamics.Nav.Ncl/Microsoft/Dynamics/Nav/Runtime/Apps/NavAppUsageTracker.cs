using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppUsageTracker
{
	private readonly NavDiagnostics diagnostics;

	private readonly string navTenantId;

	private readonly string navUserId;

	private readonly string aadTenantId;

	private readonly string aadUserObjectId;

	private readonly TimeSpan reportingInterval;

	private readonly Stack<string> eventSuppressions = new Stack<string>();

	private readonly object lockObj = new object();

	private readonly Dictionary<int, DateTime?> lastAppUsageEventFiredTime;

	public NavAppUsageTracker(NavDiagnostics diagnostics, string navTenantId, Guid navUserId, string aadTenantId, string aadUserObjectId, TimeSpan reportingInterval)
	{
		this.diagnostics = diagnostics;
		this.navTenantId = navTenantId;
		this.navUserId = navUserId.ToString();
		if (aadTenantId != null)
		{
			if (Guid.TryParse(aadTenantId, out var result))
			{
				this.aadTenantId = result.ToString();
			}
			else
			{
				this.aadTenantId = aadTenantId;
			}
		}
		this.aadUserObjectId = aadUserObjectId;
		this.reportingInterval = reportingInterval;
		lastAppUsageEventFiredTime = new Dictionary<int, DateTime?>();
	}

	public void PushEventSuppression(string reason)
	{
		lock (lockObj)
		{
			eventSuppressions.Push(reason);
		}
	}

	[Conditional("DEBUG_NAVAPPUSAGETRACKER")]
	private void SendPushEventSuppressionTrace(string reason)
	{
		diagnostics.SendTraceTag("00000AZ", Category.Extensions, Verbosity.Verbose, "Pushing extension usage suppression entry -- NAVTenantId:{0}; NAVUserId:{1}; AADTenantId:{2}; AADUserObjectId:{3}; Reason:{4}, Depth:{5}", DiagnosticParameter.Oii(navTenantId), DiagnosticParameter.Eupi(navUserId), DiagnosticParameter.Oii(aadTenantId), DiagnosticParameter.Eupi(aadUserObjectId), DiagnosticParameter.SystemMetadata(reason), DiagnosticParameter.SystemMetadata(eventSuppressions.Count));
	}

	public void PopEventSuppression()
	{
		lock (lockObj)
		{
			if (eventSuppressions.Count > 0)
			{
				eventSuppressions.Pop();
			}
		}
	}

	[Conditional("DEBUG_NAVAPPUSAGETRACKER")]
	private void SendPopEventSuppressionTrace()
	{
		diagnostics.SendTraceTag("00000B0", Category.Extensions, Verbosity.Verbose, "Popping extension usage suppression entry -- NAVTenantId:{0}; NAVUserId:{1}; AADTenantId:{2}; AADUserObjectId:{3}; Reason:{4}, Depth:{5}", DiagnosticParameter.Oii(navTenantId), DiagnosticParameter.Eupi(navUserId), DiagnosticParameter.Oii(aadTenantId), DiagnosticParameter.Eupi(aadUserObjectId), DiagnosticParameter.SystemMetadata(eventSuppressions.Peek()), DiagnosticParameter.SystemMetadata(eventSuppressions.Count - 1));
	}

	public void SignalUsage(NavAppRuntimeMetadata appMetadata)
	{
		if (appMetadata == null)
		{
			return;
		}
		lock (lockObj)
		{
			try
			{
				if (eventSuppressions.Count > 0)
				{
					return;
				}
				DateTime? dateTime = (lastAppUsageEventFiredTime.TryGetValue(appMetadata.RuntimeAppId, out dateTime) ? dateTime : ((DateTime?)null));
				DateTime utcNow = DateTimeNowProvider.Current.UtcNow;
				if (dateTime.HasValue)
				{
					DateTime value = utcNow;
					DateTime? dateTime2 = dateTime;
					if (!(value - dateTime2 >= reportingInterval))
					{
						return;
					}
				}
				lastAppUsageEventFiredTime[appMetadata.RuntimeAppId] = utcNow;
				diagnostics.SendTraceTag("00000B1", Category.Extensions, Verbosity.Normal, "Extension usage detected -- NAVTenantId:{0}; NAVUserId:{1}; AADTenantId:{2}; AADUserObjectId:{3}; PackageId:{4}; AppId:{5}; Name:{6}; Publisher:{7}; Version:{8}", DiagnosticParameter.Oii(navTenantId), DiagnosticParameter.Eupi(navUserId), DiagnosticParameter.Oii(aadTenantId), DiagnosticParameter.Eupi(aadUserObjectId), DiagnosticParameter.SystemMetadata(appMetadata.PackageId), DiagnosticParameter.SystemMetadata(appMetadata.AppId), DiagnosticParameter.SystemMetadata(appMetadata.Name), DiagnosticParameter.SystemMetadata(appMetadata.Publisher), DiagnosticParameter.SystemMetadata(appMetadata.Version));
				diagnostics.SendTraceTag("00000BK", Category.Extensions, Verbosity.Verbose, "Extension usage event details -- NAVTenantId:{0}; NAVUserId:{1}; AADTenantId:{2}; AADUserObjectId:{3}; PackageId:{4}; AppId:{5}; Name:{6}; Publisher:{7}; Version:{8}; {9}StackTrace:{9}{10}", DiagnosticParameter.Oii(navTenantId), DiagnosticParameter.Eupi(navUserId), DiagnosticParameter.Oii(aadTenantId), DiagnosticParameter.Eupi(aadUserObjectId), DiagnosticParameter.SystemMetadata(appMetadata.PackageId), DiagnosticParameter.SystemMetadata(appMetadata.AppId), DiagnosticParameter.SystemMetadata(appMetadata.Name), DiagnosticParameter.SystemMetadata(appMetadata.Publisher), DiagnosticParameter.SystemMetadata(appMetadata.Version), DiagnosticParameter.SystemMetadata(Environment.NewLine), DiagnosticParameter.SystemMetadata(Environment.StackTrace));
			}
			catch (Exception exception)
			{
				diagnostics.SendExceptionTag("00000BL", Category.Extensions, exception);
			}
		}
	}
}
