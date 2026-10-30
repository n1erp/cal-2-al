using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public class NavEventSubscriptionDistinctEqualityComparer : IEqualityComparer<NavEventSubscription>
{
	public bool Equals(NavEventSubscription x, NavEventSubscription y)
	{
		if (x == y)
		{
			return true;
		}
		if (x == null || y == null)
		{
			return false;
		}
		if (x.SubscriberId == y.SubscriberId)
		{
			return x.SubscriberMethodALName == y.SubscriberMethodALName;
		}
		return false;
	}

	public int GetHashCode(NavEventSubscription obj)
	{
		if (obj != null)
		{
			return (obj.SubscriberMethodALName ?? string.Empty).GetHashCode() ^ obj.SubscriberId;
		}
		return 0;
	}
}
