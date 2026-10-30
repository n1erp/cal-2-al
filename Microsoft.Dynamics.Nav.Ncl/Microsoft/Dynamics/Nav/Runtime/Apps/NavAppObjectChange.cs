using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppObjectChange
{
	public ApplicationObjectId ObjectId { get; private set; }

	public NavAppGroup AppGroup { get; private set; }

	public NavAppObjectChange(ApplicationObjectId objectId, NavAppGroup appGroup)
	{
		ObjectId = objectId;
		AppGroup = appGroup;
	}
}
