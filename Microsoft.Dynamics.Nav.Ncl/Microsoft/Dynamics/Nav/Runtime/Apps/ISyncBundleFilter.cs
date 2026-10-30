using Microsoft.Dynamics.Nav.Apps.Runtime;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal interface ISyncBundleFilter
{
	bool ShouldExclude(NavAppRuntimeMetadata appBeingSynced, SyncTableBundle bundle);
}
