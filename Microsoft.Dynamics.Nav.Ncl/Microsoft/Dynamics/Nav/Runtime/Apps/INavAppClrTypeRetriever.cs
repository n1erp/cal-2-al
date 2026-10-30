using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public interface INavAppClrTypeRetriever
{
	Type GetClrType(PackageId packageId, ApplicationObjectId objectId);

	void PurgeUnreferencedArtifacts(IEnumerable<NavAppRuntimeMetadata> allPublished);
}
