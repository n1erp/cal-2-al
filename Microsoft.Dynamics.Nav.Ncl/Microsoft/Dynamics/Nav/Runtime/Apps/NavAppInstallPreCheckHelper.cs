using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppInstallPreCheckHelper : NavAppPreCheckHelper
{
	public List<INavAppReference> HasData { get; set; }

	public List<NavAppRuntimeMetadata> DependenciesToInstall { get; set; }
}
