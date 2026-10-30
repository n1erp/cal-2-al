namespace Microsoft.Dynamics.Nav.Runtime;

public class NavEventMethodScope<TParent> : NavMethodScope<TParent> where TParent : NavApplicationObjectBase
{
	public override bool IsEvent => true;

	public NavEventMethodScope(TParent applicationObject)
		: base(applicationObject, true)
	{
	}
}
