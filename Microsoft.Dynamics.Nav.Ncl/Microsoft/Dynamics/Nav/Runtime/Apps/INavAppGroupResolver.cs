using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public interface INavAppGroupResolver
{
	NavAppGroup GetExistingGroupById(int groupId);

	IEnumerable<NavAppGroup> GetExistingGroupsMatchingIds(IEnumerable<int> groupIds);

	NavAppGroup GetOrCreateGroupForTenant(string tenantId);

	NavAppGroup GetOrCreateGroupForAppCombination(IEnumerable<PackageId> appPackageIds);

	IEnumerable<NavAppGroup> GetExistingGroupsContainingApp(PackageId appPackageId);
}
