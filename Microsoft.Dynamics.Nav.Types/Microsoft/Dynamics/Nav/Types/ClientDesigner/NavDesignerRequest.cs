using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.ClientDesigner;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
[KnownType(typeof(AddPageFieldRequest))]
[KnownType(typeof(AddPageActionRequest))]
[KnownType(typeof(AddNavigationNodeRequest))]
[KnownType(typeof(CreatePageViewRequest))]
[KnownType(typeof(CopyPageViewRequest))]
[KnownType(typeof(UpdatePageViewRequest))]
[KnownType(typeof(MovePageFieldRequest))]
[KnownType(typeof(MoveViewPageFieldRequest))]
[KnownType(typeof(MovePageActionRequest))]
[KnownType(typeof(MovePageViewRequest))]
[KnownType(typeof(MoveNavigationNodeRequest))]
[KnownType(typeof(RemovePageFieldRequest))]
[KnownType(typeof(RemovePageActionRequest))]
[KnownType(typeof(RemovePageViewRequest))]
[KnownType(typeof(RemoveNavigationNodeRequest))]
[KnownType(typeof(SetFieldPropertyRequest))]
[KnownType(typeof(SetViewFieldPropertyRequest))]
[KnownType(typeof(SetActionPropertyRequest))]
[KnownType(typeof(PromotePageActionRequest))]
[KnownType(typeof(DemotePageActionRequest))]
[KnownType(typeof(DemoteAndMovePageActionRequest))]
[KnownType(typeof(CreatePageActionGroupRequest))]
[KnownType(typeof(SetViewPropertyRequest))]
[KnownType(typeof(SetPagePropertyRequest))]
[KnownType(typeof(SetNavigationNodePropertyRequest))]
[KnownType(typeof(StartDesignerRequest))]
[KnownType(typeof(StopDesignerRequest))]
[KnownType(typeof(RestoreDefaultsRequest))]
[KnownType(typeof(RestoreControlDefaultsRequest))]
[KnownType(typeof(RestoreActionDefaultsRequest))]
[KnownType(typeof(RestoreViewDefaultsRequest))]
public abstract class NavDesignerRequest : IExtensibleDataObject
{
	private ExtensionDataObject unknownDataBag;

	public virtual ExtensionDataObject ExtensionData
	{
		get
		{
			return unknownDataBag;
		}
		set
		{
			unknownDataBag = value;
		}
	}
}
