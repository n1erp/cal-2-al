using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class PermissionCache
{
	private static readonly int ObjectTypeCount = Enum.GetValues(typeof(ObjectType)).Cast<int>().Max() + 1;

	private readonly Dictionary<int, PermissionMask>[] lookupByType = new Dictionary<int, PermissionMask>[ObjectTypeCount];

	private Dictionary<int, FilterFieldDictionary> securityFilters = new Dictionary<int, FilterFieldDictionary>();

	public void AddOrSet(ApplicationObjectId objectId, PermissionMask value)
	{
		GetDictionaryForType(objectId.ObjectType)[objectId.ObjectNumber] = value;
	}

	internal void AddOrSetSecurityFilter(int tableId, NavTableFilter value)
	{
		securityFilters[tableId] = value.FieldFilters;
	}

	public bool TryGetValue(ApplicationObjectId objectId, out PermissionMask result)
	{
		return GetDictionaryForType(objectId.ObjectType).TryGetValue(objectId.ObjectNumber, out result);
	}

	public bool Contains(ApplicationObjectId objectId)
	{
		return GetDictionaryForType(objectId.ObjectType).ContainsKey(objectId.ObjectNumber);
	}

	internal FilterFieldDictionary GetSecurityFiltersForTable(int tableId)
	{
		securityFilters.TryGetValue(tableId, out var value);
		return value ?? FilterFieldDictionary.Empty;
	}

	public void Clear()
	{
		foreach (Dictionary<int, PermissionMask> item in lookupByType.Where((Dictionary<int, PermissionMask> d) => d != null))
		{
			item.Clear();
		}
		securityFilters.Clear();
	}

	private Dictionary<int, PermissionMask> GetDictionaryForType(ObjectType objectType)
	{
		Dictionary<int, PermissionMask> dictionary = lookupByType[(int)objectType];
		if (dictionary == null)
		{
			dictionary = new Dictionary<int, PermissionMask>();
			lookupByType[(int)objectType] = dictionary;
		}
		return dictionary;
	}
}
