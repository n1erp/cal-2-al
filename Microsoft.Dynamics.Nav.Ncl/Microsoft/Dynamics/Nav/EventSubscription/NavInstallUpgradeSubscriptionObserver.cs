using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.EventSubscription;

public class NavInstallUpgradeSubscriptionObserver : INavEventSubscriptionObserver
{
	public bool AreInstallUpgradeEventsUsed { get; private set; }

	public void NotifyAboutAdd(NavEventSubscription subscription)
	{
		if (!AreInstallUpgradeEventsUsed && subscription.OriginalApplicationObjectId.Equals(new ApplicationObjectId(ObjectType.CodeUnit, NavSystemCodeunitUpgradeTriggers.Id)) && NavSystemCodeunitUpgradeTriggers.IsInstallUpgradeMethodName(subscription.OriginalMethodALName))
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001LR", Category.EventSubscription, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Found install/upgrade event consumer (event publisher definitions are now required) -- CodeunitId:{0}; Method:{1}", subscription.SubscriberId, subscription.SubscriberMethodALName);
			AreInstallUpgradeEventsUsed = true;
		}
	}

	public void NotifyAboutClear()
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001LS", Category.EventSubscription, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Clearing install/upgrade event consumer state (event publisher definitions are optional until a subscriber is seen)");
		AreInstallUpgradeEventsUsed = false;
	}
}
