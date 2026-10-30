namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

public interface IClientDesignerResponseMoveControl : IClientDesignerActionResponse, IClientDesignerResponse
{
	string ParentTargetName { get; set; }

	string RelativeTargetName { get; set; }
}
