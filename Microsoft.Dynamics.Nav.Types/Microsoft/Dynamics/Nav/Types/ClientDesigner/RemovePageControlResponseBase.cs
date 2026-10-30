namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

public class RemovePageControlResponseBase : IClientDesignerResponseRemoveControl, IClientDesignerActionResponse, IClientDesignerResponse
{
	public string TargetName { get; set; }
}
