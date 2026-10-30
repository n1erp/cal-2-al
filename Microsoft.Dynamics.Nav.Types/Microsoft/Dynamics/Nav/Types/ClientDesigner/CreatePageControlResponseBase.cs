namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

public class CreatePageControlResponseBase : IClientDesignerAddResponse, IClientDesignerActionResponse, IClientDesignerResponse
{
	public string ParentTargetName { get; set; }

	public string RelativeTargetName { get; set; }

	public string TargetName { get; set; }
}
