namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

public interface IClientDesignerAddResponse : IClientDesignerActionResponse, IClientDesignerResponse
{
	string ParentTargetName { get; set; }

	string RelativeTargetName { get; set; }
}
