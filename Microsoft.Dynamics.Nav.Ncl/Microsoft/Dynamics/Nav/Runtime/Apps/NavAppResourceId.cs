using Microsoft.Dynamics.Nav.Apps.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppResourceId
{
	public PackageId PackageId { get; }

	public NavAppResourceType ResourceType { get; }

	public NavAppResourceName ResourceName { get; }

	public NavAppResourceId(PackageId packageId, NavAppResourceType resourceType, NavAppResourceName resourceName)
	{
		PackageId = packageId;
		ResourceType = resourceType;
		ResourceName = resourceName;
	}
}
