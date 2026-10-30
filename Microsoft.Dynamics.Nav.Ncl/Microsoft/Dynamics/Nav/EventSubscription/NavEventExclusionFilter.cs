using System;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public class NavEventExclusionFilter
{
	private readonly Func<NavEventSubscription, NavApplicationObjectBase, bool> shouldExcludeFunc;

	public ApplicationObjectId PublisherObject { get; }

	public string PublisherMethod { get; }

	public NavEventExclusionFilter(ApplicationObjectId publisherObject, string publisherMethod, Func<NavEventSubscription, NavApplicationObjectBase, bool> shouldExcludeFunc)
	{
		if (publisherMethod == null)
		{
			throw new ArgumentNullException("publisherMethod");
		}
		if (shouldExcludeFunc == null)
		{
			throw new ArgumentNullException("shouldExcludeFunc");
		}
		PublisherObject = publisherObject;
		PublisherMethod = publisherMethod;
		this.shouldExcludeFunc = shouldExcludeFunc;
	}

	public bool ShouldExcludeCall(NavEventSubscription subscriber, NavApplicationObjectBase subscriberInstance)
	{
		return shouldExcludeFunc(subscriber, subscriberInstance);
	}
}
