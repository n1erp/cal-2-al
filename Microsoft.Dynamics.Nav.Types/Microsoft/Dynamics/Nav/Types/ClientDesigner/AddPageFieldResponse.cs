using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

public class AddPageFieldResponse : IClientDesignerResponseAddField, IClientDesignerAddResponse, IClientDesignerActionResponse, IClientDesignerResponse
{
	public string TargetName { get; set; }

	public string ParentTargetName { get; set; }

	public string RelativeTargetName { get; set; }

	public ControlBaseDefinition TargetDefinition { get; set; }

	public MetaField MetaField { get; set; }
}
