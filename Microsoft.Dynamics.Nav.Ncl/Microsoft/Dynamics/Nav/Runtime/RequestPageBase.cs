using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class RequestPageBase : NavForm
{
	private NavApplicationObjectBase parent;

	protected override uint[] IndirectPermissionList => parent.GetIndirectPermissionList();

	protected RequestPageBase(NavApplicationObjectBase parent, ITreeSharedObjectContainer treeParent, MasterPage masterPage)
		: base(treeParent, masterPage)
	{
		this.parent = parent;
	}
}
