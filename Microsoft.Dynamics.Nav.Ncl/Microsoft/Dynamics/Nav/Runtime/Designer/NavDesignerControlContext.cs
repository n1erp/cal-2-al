using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

public class NavDesignerControlContext
{
	public ControlBaseDefinition Control { get; }

	public ControlContainerType ControlContainerType { get; }

	public ControlGroupBaseDefinition ParentGroup { get; }

	public ControlBaseDefinition PreviousControl { get; }

	public ControlBaseDefinition NextControl { get; }

	public string PageIdentifier { get; set; }

	public NavDesignerControlContext(ControlBaseDefinition control, ControlContainerType controlContainerType, ControlGroupBaseDefinition parentGroup, ControlBaseDefinition previousControl, ControlBaseDefinition nextControl, string pageIdentifier)
	{
		Control = control;
		ControlContainerType = controlContainerType;
		ParentGroup = parentGroup;
		PreviousControl = previousControl;
		NextControl = nextControl;
		PageIdentifier = pageIdentifier;
	}

	public string GetControlALIdentifier()
	{
		return NavDesignerUtil.GetControlALIdentifier(Control, PageIdentifier);
	}

	public string GetControlALIdentifier(NamedElementDefinition control)
	{
		return NavDesignerUtil.GetControlALIdentifier(control, PageIdentifier);
	}
}
