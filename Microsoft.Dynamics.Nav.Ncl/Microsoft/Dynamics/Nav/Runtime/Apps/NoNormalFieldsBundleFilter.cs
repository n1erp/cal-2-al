using System;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NoNormalFieldsBundleFilter : ISyncBundleFilter
{
	public bool ShouldExclude(NavAppRuntimeMetadata appBeingSynced, SyncTableBundle bundle)
	{
		bool result = false;
		if (bundle.ObjectType == ObjectType.TableExtension && bundle.TargetNCL != null)
		{
			NavAppTableContext tableContext = NavAppTableContext.Companion(NavAppOrigin.CreateForApp(((WrappedValue<Guid>)(object)appBeingSynced.AppId).Value));
			result = bundle.OriginalSnapshotNCL == null && !bundle.TargetNCL.NormalFieldsForContext(tableContext).Any();
		}
		return result;
	}
}
