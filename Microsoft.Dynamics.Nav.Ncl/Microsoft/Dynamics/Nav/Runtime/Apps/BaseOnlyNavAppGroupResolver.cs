using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class BaseOnlyNavAppGroupResolver : INavAppGroupResolver
{
	public NavAppGroup GetOrCreateGroupForTenant(string tenantId)
	{
		return NavAppGroup.BaseGroup;
	}

	public NavAppGroup GetOrCreateGroupForAppCombination(IEnumerable<PackageId> appPackageIds)
	{
		return NavAppGroup.BaseGroup;
	}

	public IEnumerable<NavAppGroup> GetExistingGroupsContainingApp(PackageId appPackageId)
	{
		return Enumerable.Empty<NavAppGroup>();
	}

	public NavAppGroup GetExistingGroupById(int groupId)
	{
		if (groupId == 0)
		{
			return NavAppGroup.BaseGroup;
		}
		return null;
	}

	public IEnumerable<NavAppGroup> GetExistingGroupsMatchingIds(IEnumerable<int> groupIds)
	{
		if (!groupIds.Contains(0))
		{
			return Enumerable.Empty<NavAppGroup>();
		}
		return new NavAppGroup[1] { NavAppGroup.BaseGroup };
	}
}
