using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public class NavEventSubscriptionSettingsEntry
{
	public ApplicationObjectId PublisherObject { get; }

	public string PublisherMethod { get; }

	public NavEventSubscriptionSettings Settings { get; }

	public NavEventSubscriptionSettingsEntry(ApplicationObjectId publisherObject, string publisherMethod, NavEventSubscriptionSettings settings)
	{
		if (publisherMethod == null)
		{
			throw new ArgumentNullException("publisherMethod");
		}
		PublisherObject = publisherObject;
		PublisherMethod = publisherMethod;
		Settings = settings;
	}
}
