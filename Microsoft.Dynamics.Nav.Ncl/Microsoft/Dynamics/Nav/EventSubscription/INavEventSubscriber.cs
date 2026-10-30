using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public interface INavEventSubscriber
{
	bool OriginatesFromBase { get; }

	NavEventSubscriberReflectionWrapper SubscriberReflectionWrapper { get; }

	Type ApplicationObjectClrType { get; }

	ApplicationObjectId ApplicationObjectId { get; }

	bool IsEventManualBinding { get; }
}
