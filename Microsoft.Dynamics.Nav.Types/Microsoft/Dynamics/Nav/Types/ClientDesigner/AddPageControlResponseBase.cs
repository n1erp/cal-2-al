namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

public class AddPageControlResponseBase : IClientDesignerAddResponse, IClientDesignerActionResponse, IClientDesignerResponse
{
	public string TargetName { get; set; }

	public string ParentTargetName { get; set; }

	public string RelativeTargetName { get; set; }
}
