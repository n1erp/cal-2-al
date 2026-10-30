using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Types;

public class NavTableFilterGroups
{
	public int TableNo { get; private set; }

	public IEnumerable<NavFilterGroup> FilterGroups { get; private set; }

	public NavTableFilterGroups(int tableNo, params NavFilterGroup[] groups)
	{
		TableNo = tableNo;
		FilterGroups = groups.ToArray();
	}
}
