namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavAppInstallEventExecutor
{
	void InvokeOnInstallAppPerDatabase();

	void InvokeOnInstallAppPerCompany();
}
