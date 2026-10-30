using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ALObjectScopeDiagnosticEntry
{
	public string ClientType { get; set; }

	public string CompanyName { get; set; }

	public string ALCallStack { get; set; }

	public int ALObjectId { get; set; }

	public string ALObjectName { get; set; }

	public ObjectType ALObjectType { get; set; }

	public string ALMethodScope { get; set; }

	public string ExtensionId { get; set; }

	public string ExtensionName { get; set; }

	public Version ExtensionVersion { get; set; }
}
