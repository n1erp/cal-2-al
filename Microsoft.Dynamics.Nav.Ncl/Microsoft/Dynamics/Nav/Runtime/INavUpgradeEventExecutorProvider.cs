namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavUpgradeEventExecutorProvider
{
	INavUpgradeEventExecutor CreateUpgradeEventExecutor();

	INavAppInstallEventExecutor CreateInstallEventExecutor();
}
