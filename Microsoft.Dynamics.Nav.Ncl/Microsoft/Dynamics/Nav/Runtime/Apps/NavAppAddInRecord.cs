using System;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppAddInRecord
{
	public AppId AppId { get; set; }

	public string AddInName { get; set; }

	public string PublicKeyToken { get; set; }

	public Version Version { get; set; }

	public AddInCategory Category { get; set; }

	public string Description { get; set; }

	public NavRecordId DatabaseRecordId { get; set; }
}
