using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Runtime.Apps;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class AppGroupAwareLookup<T>
{
	private readonly Func<NavAppGroup, T> performLookup;

	private readonly Func<T, bool> isRefreshNeeded;

	private readonly Dictionary<int, T> referencesByGroup = new Dictionary<int, T>();

	public static AppGroupAwareLookup<T> CreateWithDefaultRefreshLogic(Func<NavAppGroup, T> performLookup)
	{
		return new AppGroupAwareLookup<T>(performLookup, (T oldRef) => false);
	}

	public static AppGroupAwareLookup<T> CreateWithCustomRefreshLogic(Func<NavAppGroup, T> performLookup, Func<T, bool> isRefreshNeeded)
	{
		return new AppGroupAwareLookup<T>(performLookup, isRefreshNeeded);
	}

	private AppGroupAwareLookup(Func<NavAppGroup, T> performLookup, Func<T, bool> isRefreshNeeded)
	{
		this.performLookup = performLookup;
		this.isRefreshNeeded = isRefreshNeeded;
	}

	public T GetReferenceUsingCurrentAppGroup()
	{
		NavAppGroup appGroup = NavCurrentThread.ResolveAppGroup();
		return GetReferenceUsingSpecificAppGroup(appGroup);
	}

	public T GetReferenceUsingSpecificAppGroup(NavAppGroup appGroup)
	{
		lock (referencesByGroup)
		{
			referencesByGroup.TryGetValue(appGroup.GroupId, out var value);
			if (value == null || isRefreshNeeded(value))
			{
				value = performLookup(appGroup);
				referencesByGroup[appGroup.GroupId] = value;
			}
			return value;
		}
	}

	public IEnumerable<Tuple<int, T>> GetAllKnownReferences()
	{
		lock (referencesByGroup)
		{
			return referencesByGroup.Select((KeyValuePair<int, T> kvp) => Tuple.Create(kvp.Key, kvp.Value)).ToList();
		}
	}

	public void SetKnownReference(int groupId, T reference)
	{
		lock (referencesByGroup)
		{
			referencesByGroup[groupId] = reference;
		}
	}
}
