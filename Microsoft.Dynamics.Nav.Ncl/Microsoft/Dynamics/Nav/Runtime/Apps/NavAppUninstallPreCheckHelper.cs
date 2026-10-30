using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.Runtime;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppUninstallPreCheckHelper : NavAppPreCheckHelper
{
	public List<NavAppRuntimeMetadata> DependentsToUninstall { get; set; }
}
