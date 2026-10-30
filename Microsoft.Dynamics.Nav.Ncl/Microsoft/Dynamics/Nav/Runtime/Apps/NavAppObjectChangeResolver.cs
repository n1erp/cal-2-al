using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppObjectChangeResolver
{
	private readonly INavAppGroupResolver groupResolver;

	public NavAppObjectChangeResolver(INavAppGroupResolver groupResolver)
	{
		this.groupResolver = groupResolver;
	}

	public IEnumerable<NavAppObjectChange> ResolveChangesFromTrackingObjects(IEnumerable<NavTrackingObject> trackingObjects)
	{
		List<NavAppObjectChange> list = new List<NavAppObjectChange>();
		foreach (NavTrackingObject trackingObject in trackingObjects)
		{
			IEnumerable<NavAppGroup> existingGroupsContainingApp = groupResolver.GetExistingGroupsContainingApp(trackingObject.AppPackageId);
			foreach (NavAppGroup item in existingGroupsContainingApp)
			{
				list.Add(new NavAppObjectChange(trackingObject.ApplicationObjectId, item));
			}
		}
		return list;
	}
}
