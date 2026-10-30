using System;
using System.Collections.Concurrent;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class EntitlementCache
{
	private readonly ConcurrentDictionary<NavCode, Lazy<NavEntitlementSet>> cachedEntitlementSets = new ConcurrentDictionary<NavCode, Lazy<NavEntitlementSet>>();

	public PermissionMask GetEntitlement(NavCode setId, ApplicationObjectId objectId)
	{
		return GetEntitlementSet(setId).GetEntitlement(objectId);
	}

	private NavEntitlementSet GetEntitlementSet(NavCode setId)
	{
		return cachedEntitlementSets.GetOrAdd(setId, (NavCode id) => new Lazy<NavEntitlementSet>(() => new NavEntitlementSet(id))).Value;
	}
}
