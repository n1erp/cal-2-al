namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavUpgradeEventExecutor
{
	void InvokeUpgradeEvent(NavUpgradeEvent upgradeEvent);
}
