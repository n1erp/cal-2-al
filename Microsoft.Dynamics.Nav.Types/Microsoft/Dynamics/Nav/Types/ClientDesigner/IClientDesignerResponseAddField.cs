using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

public interface IClientDesignerResponseAddField : IClientDesignerAddResponse, IClientDesignerActionResponse, IClientDesignerResponse
{
	MetaField MetaField { get; set; }

	ControlBaseDefinition TargetDefinition { get; set; }
}
