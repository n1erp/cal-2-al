using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public class NavEventSubscriberReflectionWrapper
{
	private readonly List<NavEventSubscriberMethodInfo> eventSubscriberMethodInfo;

	public IEnumerable<NavEventSubscriberMethodInfo> EventSubscriberMethodInfo => eventSubscriberMethodInfo;

	public NavEventSubscriberReflectionWrapper(Type clrType)
	{
		if (clrType == null)
		{
			throw new ArgumentNullException("clrType");
		}
		eventSubscriberMethodInfo = (from mi in clrType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where mi.IsDefined(typeof(NavEventSubscriberAttribute))
			select new NavEventSubscriberMethodInfo(mi)).ToList();
	}
}
