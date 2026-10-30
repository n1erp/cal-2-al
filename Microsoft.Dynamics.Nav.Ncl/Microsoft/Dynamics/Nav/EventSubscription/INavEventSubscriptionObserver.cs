namespace Microsoft.Dynamics.Nav.EventSubscription;

public interface INavEventSubscriptionObserver
{
	void NotifyAboutAdd(NavEventSubscription subscription);

	void NotifyAboutClear();
}
