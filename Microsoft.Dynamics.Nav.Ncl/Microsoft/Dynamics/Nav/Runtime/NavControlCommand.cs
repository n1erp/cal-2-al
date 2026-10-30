namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavControlCommand : NavFormCommand
{
	private int controlId;

	public int ControlId => controlId;

	protected NavControlCommand(int controlId)
	{
		this.controlId = controlId;
	}
}
