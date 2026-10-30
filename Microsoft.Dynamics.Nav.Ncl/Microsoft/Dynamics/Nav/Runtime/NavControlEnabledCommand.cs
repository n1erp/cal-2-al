namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavControlEnabledCommand : NavControlCommand
{
	private bool enabled;

	public bool Enabled => enabled;

	public NavControlEnabledCommand(int controlId, bool enabled)
		: base(controlId)
	{
		this.enabled = enabled;
	}
}
