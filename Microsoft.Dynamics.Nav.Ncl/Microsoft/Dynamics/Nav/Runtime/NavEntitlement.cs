using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal struct NavEntitlement(ObjectType objectType, int objectIdRangeStart, int objectIdRangeEnd, PermissionMask entitlement)
{
	public ObjectType ObjectType { get; } = objectType;

	public int ObjectIdRangeStart { get; } = objectIdRangeStart;

	public int ObjectIdRangeEnd { get; } = objectIdRangeEnd;

	public PermissionMask Entitlement { get; } = entitlement;

	public bool IsRangeEntitlement => ObjectIdRangeStart != ObjectIdRangeEnd;

	public bool IsZeroEntitlement => ObjectIdRangeStart == 0;
}
