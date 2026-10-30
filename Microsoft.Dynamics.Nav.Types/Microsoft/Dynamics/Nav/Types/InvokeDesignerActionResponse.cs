using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.ClientDesigner;

namespace Microsoft.Dynamics.Nav.Types;

[KnownType(typeof(AddPageFieldResponse))]
[KnownType(typeof(AddPageActionResponse))]
[KnownType(typeof(AddNavigationNodeResponse))]
[KnownType(typeof(SavePageViewResponse))]
[KnownType(typeof(MovePageFieldResponse))]
[KnownType(typeof(MoveViewPageFieldResponse))]
[KnownType(typeof(MovePageActionResponse))]
[KnownType(typeof(MovePageViewResponse))]
[KnownType(typeof(MoveNavigationNodeResponse))]
[KnownType(typeof(RemovePageFieldResponse))]
[KnownType(typeof(RemovePageActionResponse))]
[KnownType(typeof(RemovePageViewResponse))]
[KnownType(typeof(RemoveNavigationNodeResponse))]
[KnownType(typeof(SetFieldPropertyResponse))]
[KnownType(typeof(SetViewFieldPropertyResponse))]
[KnownType(typeof(SetViewPropertyResponse))]
[KnownType(typeof(SetActionPropertyResponse))]
[KnownType(typeof(PromotePageActionResponse))]
[KnownType(typeof(DemotePageActionResponse))]
[KnownType(typeof(CreatePageActionGroupResponse))]
[KnownType(typeof(SetPagePropertyResponse))]
[KnownType(typeof(SetNavigationNodePropertyResponse))]
[KnownType(typeof(StartDesignerResponse))]
[KnownType(typeof(StopDesignerResponse))]
[KnownType(typeof(RestoreDefaultsResponse))]
[KnownType(typeof(RestoreControlDefaultsResponse))]
[KnownType(typeof(RestoreViewControlDefaultsResponse))]
[KnownType(typeof(RestoreActionDefaultsResponse))]
[KnownType(typeof(RestoreViewDefaultsResponse))]
public class InvokeDesignerActionResponse : ServerResponse
{
	public IClientDesignerResponse Info { get; set; }

	public NavDesignerActionStatus Status { get; set; }
}
