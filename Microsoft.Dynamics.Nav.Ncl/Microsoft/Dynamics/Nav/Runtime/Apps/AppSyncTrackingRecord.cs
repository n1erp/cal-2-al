using System;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class AppSyncTrackingRecord : INavAppReference
{
	public AppId AppId { get; set; }

	public Version Version { get; set; }

	public string Name { get; set; }

	public string Publisher { get; set; }

	public Version BaselineVersion { get; set; }
}
