using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public interface INavAppRoutesUpdater
{
	bool IsRouteConfigured(string apiRoute);

	void ConfigureRoutes(NavTenant tenant);

	void ConfigureRoutes(NavAppGroup group);

	void ConfigureRoutes(IEnumerable<ApplicationObjectId> changedObjectIds);

	void InvalidateCache(NavAppGroup group);
}
