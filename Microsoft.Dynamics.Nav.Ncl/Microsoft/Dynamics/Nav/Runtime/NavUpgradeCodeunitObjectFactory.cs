namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavUpgradeCodeunitObjectFactory : INavUpgradeCodeunitObjectFactory
{
	public NavUpgradeCodeunit CreateCodeunitObject(NavSession navSession, NCLMetaCodeunit codeunit)
	{
		NavCodeunit navCodeunit = codeunit.CreateObjectInstance();
		return navCodeunit as NavUpgradeCodeunit;
	}
}
