using System;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class UninstalledAppRecord : INavAppReference
{
	public string TenantId { get; set; }

	public AppId AppId { get; set; }

	public Version Version { get; set; }

	public string Name { get; set; }

	public string Publisher { get; set; }

	public DateTime TimeOfUninstall { get; set; }

	public bool HasData { get; set; }
}
