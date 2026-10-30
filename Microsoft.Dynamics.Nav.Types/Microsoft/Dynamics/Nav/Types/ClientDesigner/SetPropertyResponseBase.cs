using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

public class SetPropertyResponseBase : IClientDesignerResponseSetProperty, IClientDesignerActionResponse, IClientDesignerResponse
{
	public string TargetName { get; set; }

	public UIElementDefinition TargetElementDefinition { get; set; }
}
