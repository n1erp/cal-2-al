using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

public interface IClientDesignerResponseAddAction : IClientDesignerAddResponse, IClientDesignerActionResponse, IClientDesignerResponse
{
	ActionBaseDefinition ActionDefinition { get; set; }
}
