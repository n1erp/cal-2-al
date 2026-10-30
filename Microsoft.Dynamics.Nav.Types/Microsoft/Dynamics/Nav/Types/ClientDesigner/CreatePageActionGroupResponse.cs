using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

public class CreatePageActionGroupResponse : IClientDesignerResponseAddAction, IClientDesignerAddResponse, IClientDesignerActionResponse, IClientDesignerResponse
{
	public ActionBaseDefinition ActionDefinition { get; set; }

	public string ParentTargetName { get; set; }

	public string RelativeTargetName { get; set; }

	public string TargetName { get; set; }

	public ActionContainerType ContainerType { get; set; }
}
