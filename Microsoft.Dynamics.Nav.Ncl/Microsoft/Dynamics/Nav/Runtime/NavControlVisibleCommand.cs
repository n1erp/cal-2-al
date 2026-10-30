namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavControlVisibleCommand : NavControlCommand
{
	private bool visible;

	public bool Visible => visible;

	public NavControlVisibleCommand(int controlId, bool visible)
		: base(controlId)
	{
		this.visible = visible;
	}
}
