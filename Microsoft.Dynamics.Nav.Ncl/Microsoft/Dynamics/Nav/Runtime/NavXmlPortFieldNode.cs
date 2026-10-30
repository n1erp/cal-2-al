using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlPortFieldNode : NavXmlPortNode
{
	private NavXmlPortTableNode sourceFieldTableNode;

	private string sourceFieldName = string.Empty;

	private bool calcField;

	private NavTrigger onBeforePassField;

	private NavTrigger onAfterAssignField;

	public NavTrigger OnBeforePassField
	{
		get
		{
			return onBeforePassField;
		}
		set
		{
			onBeforePassField = value;
		}
	}

	public NavTrigger OnAfterAssignField
	{
		get
		{
			return onAfterAssignField;
		}
		set
		{
			onAfterAssignField = value;
		}
	}

	internal NavXmlPortTableNode SourceFieldTableNode
	{
		get
		{
			return sourceFieldTableNode;
		}
		set
		{
			sourceFieldTableNode = value;
		}
	}

	internal string SourceFieldName
	{
		get
		{
			return sourceFieldName;
		}
		set
		{
			sourceFieldName = value;
		}
	}

	internal bool CalcField
	{
		get
		{
			return calcField;
		}
		set
		{
			calcField = value;
		}
	}

	internal NavControlStatement FireOnBeforePassField()
	{
		NavControlStatement result = NavControlStatement.None;
		if (onBeforePassField != null)
		{
			try
			{
				OnBeforePassField();
			}
			catch (NavControlException ex)
			{
				result = ex.ControlStatement;
			}
		}
		return result;
	}

	internal NavControlStatement FireOnAfterAssignField()
	{
		NavControlStatement result = NavControlStatement.None;
		if (onAfterAssignField != null)
		{
			try
			{
				OnAfterAssignField();
			}
			catch (NavControlException ex) when (ex.ControlStatement != NavControlStatement.Quit)
			{
				result = ex.ControlStatement;
			}
		}
		return result;
	}
}
