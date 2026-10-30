using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlPortTextNode : NavXmlPortNode
{
	private NavTrigger onBeforePassVariable;

	private NavTrigger onAfterAssignVariable;

	public NavTrigger OnBeforePassVariable
	{
		get
		{
			return onBeforePassVariable;
		}
		set
		{
			onBeforePassVariable = value;
		}
	}

	public NavTrigger OnAfterAssignVariable
	{
		get
		{
			return onAfterAssignVariable;
		}
		set
		{
			onAfterAssignVariable = value;
		}
	}

	internal NavControlStatement FireOnBeforePassVariable()
	{
		if (onBeforePassVariable != null)
		{
			try
			{
				OnBeforePassVariable();
			}
			catch (NavControlException ex)
			{
				return ex.ControlStatement;
			}
		}
		return NavControlStatement.None;
	}

	internal NavControlStatement FireOnAfterAssignVariable()
	{
		if (onAfterAssignVariable != null)
		{
			try
			{
				OnAfterAssignVariable();
			}
			catch (NavControlException ex) when (ex.ControlStatement != NavControlStatement.Quit)
			{
				return ex.ControlStatement;
			}
		}
		return NavControlStatement.None;
	}
}
