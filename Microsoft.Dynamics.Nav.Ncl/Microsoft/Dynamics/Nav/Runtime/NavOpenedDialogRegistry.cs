using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class NavOpenedDialogRegistry
{
	private readonly Stack<NavOpenDialogTracking> registeredOpenDialogTracking = new Stack<NavOpenDialogTracking>();

	internal Stack<NavOpenDialogTracking> RegisteredOpenDialogTracking => registeredOpenDialogTracking;

	internal void RegisterOpenDialog(NavDialog dialog)
	{
		if (registeredOpenDialogTracking.Count != 0 && !registeredOpenDialogTracking.Any((NavOpenDialogTracking tempTracking) => tempTracking.Contains(dialog)))
		{
			registeredOpenDialogTracking.Peek()?.RegisterOpenDialog(dialog);
		}
	}

	internal void UnregisterOpenDialog(NavDialog dialog)
	{
		if (registeredOpenDialogTracking.Count == 0)
		{
			return;
		}
		foreach (NavOpenDialogTracking item in registeredOpenDialogTracking)
		{
			if (item.Contains(dialog))
			{
				item.UnregisterOpenDialog(dialog);
			}
		}
	}

	internal void PopUntilOpenedDialogTracking(NavOpenDialogTracking tracking)
	{
		if (RegisteredOpenDialogTracking.Count == 0)
		{
			return;
		}
		if (RegisteredOpenDialogTracking.Count == 1)
		{
			RegisteredOpenDialogTracking.Pop();
			return;
		}
		int num = -1;
		int num2 = 0;
		foreach (NavOpenDialogTracking item2 in RegisteredOpenDialogTracking)
		{
			num2++;
			if (tracking == item2)
			{
				num = num2;
				break;
			}
		}
		if (num == -1)
		{
			return;
		}
		IList<NavOpenDialogTracking> list = new List<NavOpenDialogTracking>(num2);
		for (int i = 0; i < num; i++)
		{
			NavOpenDialogTracking item = RegisteredOpenDialogTracking.Pop();
			if (i < num - 1)
			{
				list.Add(item);
			}
		}
		foreach (NavOpenDialogTracking item3 in list)
		{
			item3.Dispose();
		}
	}
}
