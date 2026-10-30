using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public interface INavAppAddInRetriever
{
	IEnumerable<NavAppAddInRecord> GetTenantAddInRecords(NavSession session, AddInCategory category);

	NavAppAddInRawAssembly GetTenantAddInAssemblyContent(NavSession session, NavAppAddInRecord addInRecord);
}
