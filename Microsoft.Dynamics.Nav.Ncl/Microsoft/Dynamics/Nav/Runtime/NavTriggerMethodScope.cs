namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTriggerMethodScope<TParent> : NavMethodScope<TParent> where TParent : NavApplicationObjectBase
{
	public NavTriggerMethodScope(TParent applicationObject)
		: base(applicationObject)
	{
	}
}
