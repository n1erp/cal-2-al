using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

public interface IClientDesignerResponseSetProperty : IClientDesignerActionResponse, IClientDesignerResponse
{
	UIElementDefinition TargetElementDefinition { get; set; }
}
