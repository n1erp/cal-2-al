using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppPreCheckHelper
{
	public NavAppRuntimeMetadata Target { get; set; }

	public List<NavAppRuntimeMetadata> Installed { get; set; }

	public List<NavAppRuntimeMetadata> Published { get; set; }

	public List<AppSyncTrackingRecord> Synced { get; set; }

	public List<NavAppRuntimeMetadata> NeedToChange { get; set; }

	public ChangeNavAppStateResult Result { get; set; }

	public string Error { get; set; }
}
