namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavUpgradeCodeunitObjectFactory
{
	NavUpgradeCodeunit CreateCodeunitObject(NavSession session, NCLMetaCodeunit codeunit);
}
