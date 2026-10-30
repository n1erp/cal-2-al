using System;

namespace Microsoft.Dynamics.Nav.EventSubscription;

[Flags]
public enum NavEventSubscriptionSettings
{
	None = 0,
	IsolatedMode = 1
}
