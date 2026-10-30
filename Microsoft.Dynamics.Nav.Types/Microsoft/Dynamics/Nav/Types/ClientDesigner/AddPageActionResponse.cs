using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

public class AddPageActionResponse : IClientDesignerResponseAddAction, IClientDesignerAddResponse, IClientDesignerActionResponse, IClientDesignerResponse
{
	public string TargetName { get; set; }

	public string ParentTargetName { get; set; }

	public string RelativeTargetName { get; set; }

	public ActionBaseDefinition ActionDefinition { get; set; }

	public ActionContainerType ContainerType { get; set; }
}
