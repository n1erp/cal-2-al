using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerIDManager
{
	private readonly HashSet<int> idSet = new HashSet<int>();

	private readonly int lowerRange;

	private readonly int upperRange;

	public NavDesignerIDManager(int lowerRange, int upperRange)
	{
		this.lowerRange = lowerRange;
		this.upperRange = upperRange;
	}

	public void AddIds(IEnumerable<int> ids)
	{
		idSet.UnionWith(ids);
	}

	public int ReserveNewId()
	{
		for (int i = lowerRange; i < upperRange; i++)
		{
			if (!idSet.Contains(i))
			{
				idSet.Add(i);
				return i;
			}
		}
		return 0;
	}
}
