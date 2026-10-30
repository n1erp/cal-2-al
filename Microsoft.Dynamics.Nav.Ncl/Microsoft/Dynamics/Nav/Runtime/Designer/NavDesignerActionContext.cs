using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

public class NavDesignerActionContext
{
	public ActionBaseDefinition Action { get; }

	public ActionContainerType ActionContainerType { get; }

	public ActionGroupBaseDefinition ParentGroup { get; }

	public ActionBaseDefinition PreviousAction { get; }

	public ActionBaseDefinition NextAction { get; }

	public string PageIdentifier { get; set; }

	public NavDesignerActionContext(ActionBaseDefinition action, ActionContainerType actionContainerType, ActionGroupBaseDefinition parentGroup, ActionBaseDefinition previousAction, ActionBaseDefinition nextAction, string pageIdentifier)
	{
		Action = action;
		ActionContainerType = actionContainerType;
		ParentGroup = parentGroup;
		PreviousAction = previousAction;
		NextAction = nextAction;
		PageIdentifier = pageIdentifier;
	}

	public string GetActionALIdentifier()
	{
		if (ActionContainerType == ActionContainerType.ViewActions)
		{
			return NavDesignerManagementTasks.GetALViewName(Action.Name);
		}
		return NavDesignerUtil.GetActionALIdentifier(Action, PageIdentifier);
	}

	public string GetActionALIdentifier(NamedElementDefinition action)
	{
		return NavDesignerUtil.GetActionALIdentifier(action, PageIdentifier);
	}
}
