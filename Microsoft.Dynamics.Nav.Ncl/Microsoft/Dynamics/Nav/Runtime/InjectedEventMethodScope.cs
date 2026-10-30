namespace Microsoft.Dynamics.Nav.Runtime;

internal class InjectedEventMethodScope : NavMethodScope
{
	public override NavApplicationObjectBase ApplicationObject => Parent;

	public override bool IsStackFrame => false;

	public NavApplicationObjectBase Parent { get; }

	protected override uint RawScopeId { get; set; }

	public InjectedEventMethodScope(NavApplicationObjectBase applicationObject)
		: base(applicationObject, eventSource: true)
	{
		Parent = applicationObject;
	}
}
