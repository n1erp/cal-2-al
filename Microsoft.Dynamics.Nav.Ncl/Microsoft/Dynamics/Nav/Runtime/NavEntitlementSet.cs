using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavEntitlementSet
{
	private struct RangeEntitlement
	{
		public int Start { get; }

		public int End { get; }

		public PermissionMask Entitlement { get; }

		public RangeEntitlement(int rangeStart, int rangeEnd, PermissionMask entitlement)
		{
			Start = rangeStart;
			End = rangeEnd;
			Entitlement = entitlement;
		}
	}

	private static readonly int ObjectTypeCount = Enum.GetNames(typeof(ObjectType)).Length;

	private static readonly ApplicationObjectId[] ZeroObjects = CreateZeroObjects();

	private readonly NavCode setId;

	private readonly ConcurrentDictionary<ApplicationObjectId, PermissionMask> objectEntitlements = new ConcurrentDictionary<ApplicationObjectId, PermissionMask>();

	private readonly List<RangeEntitlement>[] rangeEntitlements = new List<RangeEntitlement>[ObjectTypeCount];

	public NavEntitlementSet(NavCode entitlementSetId)
	{
		setId = entitlementSetId;
		LoadEntitlements();
	}

	public PermissionMask GetEntitlement(ApplicationObjectId objectId)
	{
		return objectEntitlements.GetOrAdd(objectId, CalcObjectEntitlement);
	}

	private static ApplicationObjectId[] CreateZeroObjects()
	{
		ApplicationObjectId[] array = new ApplicationObjectId[ObjectTypeCount];
		for (int i = 0; i < ObjectTypeCount; i++)
		{
			array[i] = new ApplicationObjectId((ObjectType)i, 0);
		}
		return array;
	}

	private static PermissionMask GetEffectivePermissions(int fieldIndex, NavOption permissionOption)
	{
		int num = 0;
		switch (permissionOption.Value)
		{
		case 0:
			return PermissionMask.None;
		case 2:
			num = 5;
			break;
		}
		return (PermissionMask)(1 << fieldIndex + num);
	}

	private void LoadEntitlements()
	{
		ILookup<bool, NavEntitlement> lookup = ReadEntitlementsFromDatabase().ToLookup((NavEntitlement e) => e.IsRangeEntitlement);
		IEnumerable<NavEntitlement> rangeEntitlementList = lookup[true];
		IEnumerable<NavEntitlement> objectEntitlementList = lookup[false];
		LoadRangeEntitlements(rangeEntitlementList);
		LoadObjectEntitlements(objectEntitlementList);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000A2", Category.Security, Verbosity.Verbose, "Successfully loaded entitlement set {0} into cache.", DiagnosticParameter.SystemMetadata(setId.Value));
	}

	private IEnumerable<NavEntitlement> ReadEntitlementsFromDatabase()
	{
		NavSession session = NavCurrentThread.Session;
		using (new MaximizedPermissionScope(session, new int[1] { 2000000191 }))
		{
			List<NavEntitlement> list = new List<NavEntitlement>();
			NavRecord navRecord = new NavRecord(session, 2000000191);
			navRecord.ALSetRange(1, setId);
			if (navRecord.ALFindSet(DataError.TrapError))
			{
				do
				{
					ObjectType objectType = (ObjectType)navRecord.GetFieldValue(3).ToInt32();
					int objectIdRangeStart = navRecord.GetFieldValue(4).ToInt32();
					int objectIdRangeEnd = navRecord.GetFieldValue(5).ToInt32();
					PermissionMask permissionMask = PermissionMask.None;
					for (int i = 6; i <= 10; i++)
					{
						permissionMask |= GetEffectivePermissions(i - 6, navRecord.GetFieldValue(i) as NavOption);
					}
					list.Add(new NavEntitlement(objectType, objectIdRangeStart, objectIdRangeEnd, permissionMask));
				}
				while (navRecord.NextRecord());
			}
			return list;
		}
	}

	private void LoadObjectEntitlements(IEnumerable<NavEntitlement> objectEntitlementList)
	{
		foreach (NavEntitlement objectEntitlement in objectEntitlementList)
		{
			if (objectEntitlement.IsZeroEntitlement)
			{
				objectEntitlements.TryAdd(ZeroObjects[(int)objectEntitlement.ObjectType], objectEntitlement.Entitlement);
				continue;
			}
			ApplicationObjectId applicationObjectId = new ApplicationObjectId(objectEntitlement.ObjectType, objectEntitlement.ObjectIdRangeStart);
			objectEntitlements.TryAdd(applicationObjectId, objectEntitlement.Entitlement | CalcRangeEntitlement(applicationObjectId));
		}
	}

	private void LoadRangeEntitlements(IEnumerable<NavEntitlement> rangeEntitlementList)
	{
		(from range in rangeEntitlementList
			group range by range.ObjectType).ForEach((IGrouping<ObjectType, NavEntitlement> group) =>
		{
			List<RangeEntitlement> entitlements = new List<RangeEntitlement>();
			(from range in @group
				orderby range.ObjectIdRangeStart, range.ObjectIdRangeEnd
				select range).ForEach((NavEntitlement range) =>
			{
				entitlements.Add(new RangeEntitlement(range.ObjectIdRangeStart, range.ObjectIdRangeEnd, range.Entitlement));
			});
			rangeEntitlements[(int)group.Key] = entitlements;
		});
	}

	private PermissionMask CalcObjectEntitlement(ApplicationObjectId objectId)
	{
		PermissionMask permissionMask = CalcRangeEntitlement(objectId);
		ApplicationObjectId key = ZeroObjects[(int)objectId.ObjectType];
		if (permissionMask == PermissionMask.None && objectEntitlements.ContainsKey(key))
		{
			permissionMask = objectEntitlements[key];
		}
		return permissionMask;
	}

	private PermissionMask CalcRangeEntitlement(ApplicationObjectId objectId)
	{
		List<RangeEntitlement> list = rangeEntitlements[(int)objectId.ObjectType];
		if (list == null)
		{
			return PermissionMask.None;
		}
		PermissionMask permissionMask = PermissionMask.None;
		foreach (RangeEntitlement item in list)
		{
			if (objectId.ObjectNumber < item.Start)
			{
				break;
			}
			if (objectId.ObjectNumber <= item.End)
			{
				permissionMask |= item.Entitlement;
			}
		}
		return permissionMask;
	}
}
