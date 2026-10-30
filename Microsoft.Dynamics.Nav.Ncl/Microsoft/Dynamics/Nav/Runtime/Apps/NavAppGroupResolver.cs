using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppGroupResolver : INavAppGroupResolver
{
	private class ObjectInfo
	{
		private Lazy<OrderedAppCombination> orderedAppCombo;

		public List<NavAppRuntimeMetadata> Apps { get; private set; }

		public OrderedAppCombination AppCombination => orderedAppCombo.Value;

		public string ObjectName { get; private set; }

		public NavAppRuntimeMetadata FullObjectOwner { get; private set; }

		public ObjectInfo()
		{
			Apps = new List<NavAppRuntimeMetadata>();
			orderedAppCombo = new Lazy<OrderedAppCombination>(() => new OrderedAppCombination(Apps));
		}

		public void AddAppInfoInExpectedOrder(NavAppRuntimeMetadata app, NavAppRuntimeMetadata fullObjectOwner, string objectName)
		{
			Apps.Add(app);
			if (fullObjectOwner != null)
			{
				FullObjectOwner = fullObjectOwner;
				ObjectName = objectName;
			}
			else
			{
				ObjectName = ObjectName ?? objectName;
			}
		}
	}

	private readonly ITenantNavAppMetadataRetriever tenantAppMetadataRetriever;

	private readonly Func<PackageId, NavAppRuntimeMetadata> getMetadata;

	private readonly ConcurrentDictionary<string, NavAppGroup> groupPackageSignatureMap = new ConcurrentDictionary<string, NavAppGroup>();

	private readonly ConcurrentDictionary<int, NavAppGroup> groupByGroupIdMap = new ConcurrentDictionary<int, NavAppGroup>();

	private readonly ConcurrentDictionary<PackageId, List<NavAppGroup>> groupsForPackageId = new ConcurrentDictionary<PackageId, List<NavAppGroup>>();

	private int nextGroupId;

	public NavAppGroupResolver(ITenantNavAppMetadataRetriever tenantAppMetadataRetriever, Func<PackageId, NavAppRuntimeMetadata> getMetadata)
	{
		this.tenantAppMetadataRetriever = tenantAppMetadataRetriever;
		this.getMetadata = getMetadata;
	}

	public NavAppGroup GetOrCreateGroupForTenant(string tenantId)
	{
		List<NavAppRuntimeMetadata> list = tenantAppMetadataRetriever.GetForTenant(tenantId).ToList();
		NavAppGroup result = NavAppGroup.BaseGroup;
		if (list.Any())
		{
			result = OrderAppsThenGetOrCreateGroupForCombination(list);
		}
		return result;
	}

	public NavAppGroup GetOrCreateGroupForAppCombination(IEnumerable<PackageId> appPackageIds)
	{
		if (!appPackageIds.Any())
		{
			return NavAppGroup.BaseGroup;
		}
		List<NavAppRuntimeMetadata> list = new List<NavAppRuntimeMetadata>();
		foreach (PackageId appPackageId in appPackageIds)
		{
			NavAppRuntimeMetadata val = getMetadata(appPackageId);
			if (val == null)
			{
				string message = string.Format(CultureInfo.InvariantCulture, "The package ID '{0}' does not correspond to an installed app.", ((WrappedValue<Guid>)(object)appPackageId).Value);
				throw new NavAppException(message);
			}
			list.Add(val);
		}
		NavAppGroup result = NavAppGroup.BaseGroup;
		if (list.Any())
		{
			result = OrderAppsThenGetOrCreateGroupForCombination(list);
		}
		return result;
	}

	public IEnumerable<NavAppGroup> GetExistingGroupsContainingApp(PackageId appPackageId)
	{
		if (!groupsForPackageId.TryGetValue(appPackageId, out var value))
		{
			return new List<NavAppGroup>();
		}
		return value;
	}

	public NavAppGroup GetExistingGroupById(int groupId)
	{
		if (groupId == 0)
		{
			return NavAppGroup.BaseGroup;
		}
		if (groupByGroupIdMap.TryGetValue(groupId, out var value))
		{
			return value;
		}
		return null;
	}

	public IEnumerable<NavAppGroup> GetExistingGroupsMatchingIds(IEnumerable<int> groupIds)
	{
		return from g in groupIds.Select(GetExistingGroupById)
			where g != null
			select g;
	}

	private NavAppGroup OrderAppsThenGetOrCreateGroupForCombination(List<NavAppRuntimeMetadata> unorderedApps)
	{
		List<NavAppRuntimeMetadata> orderedAppMetadata = NavAppOrderResolver.DetermineProperOrderForApps(unorderedApps).ToList();
		OrderedAppCombination targetGroupCombo = new OrderedAppCombination(orderedAppMetadata);
		return GetOrCreateGroupForOrderedAppCombination(targetGroupCombo);
	}

	private NavAppGroup GetOrCreateGroupForOrderedAppCombination(OrderedAppCombination targetGroupCombo)
	{
		if (!targetGroupCombo.OrderedAppMetadata.Any())
		{
			return NavAppGroup.BaseGroup;
		}
		NavAppGroup orAdd = groupPackageSignatureMap.GetOrAdd(targetGroupCombo.CurrentProcessCombinationId, (string key) =>
		{
			int num = Interlocked.Increment(ref nextGroupId);
			List<ApplicationObjectId> list = new List<ApplicationObjectId>();
			Dictionary<ApplicationObjectId, ObjectInfo> dictionary = BuildLookupOfAppsDirectlyAffectingEachObject(targetGroupCombo);
			List<NavAppGroupObjectMetadataSummary> list2 = new List<NavAppGroupObjectMetadataSummary>();
			foreach (KeyValuePair<ApplicationObjectId, ObjectInfo> item in dictionary)
			{
				ApplicationObjectId key2 = item.Key;
				ObjectInfo value = item.Value;
				OrderedAppCombination orderedAppCombination = NavAppGroupObjectDependencyResolver.SelectAppsPlusDependencies(targetGroupCombo, value.AppCombination);
				if (orderedAppCombination.CurrentProcessCombinationId == targetGroupCombo.CurrentProcessCombinationId)
				{
					list.Add(key2);
					list2.Add(NavAppGroupObjectMetadataSummary.CreateForFutureGroup(value.FullObjectOwner, key2.ObjectType, key2.ObjectNumber, value.ObjectName));
				}
				else
				{
					NavAppGroup orCreateGroupForOrderedAppCombination = GetOrCreateGroupForOrderedAppCombination(orderedAppCombination);
					list2.Add(NavAppGroupObjectMetadataSummary.CreateForKnownGroup(orCreateGroupForOrderedAppCombination, value.FullObjectOwner, key2.ObjectType, key2.ObjectNumber, value.ObjectName));
				}
			}
			NavAppGroup navAppGroup = new NavAppGroup(num, targetGroupCombo, list2);
			string text = string.Join(" | ", navAppGroup.OrderedAppMetadata.Select((NavAppRuntimeMetadata m) => m.TechnicalDisplayName));
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000078", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Creating extension group -- Group:{0}; Extensions:{1}", num, text);
			return navAppGroup;
		});
		foreach (PackageId item2 in targetGroupCombo.OrderedAppMetadata.Select((NavAppRuntimeMetadata a) => a.PackageId))
		{
			List<NavAppGroup> orAdd2 = groupsForPackageId.GetOrAdd(item2, (PackageId key) => new List<NavAppGroup>());
			lock (orAdd2)
			{
				if (!orAdd2.Contains(orAdd))
				{
					orAdd2.Add(orAdd);
				}
			}
		}
		groupByGroupIdMap.TryAdd(orAdd.GroupId, orAdd);
		return orAdd;
	}

	private static Dictionary<ApplicationObjectId, ObjectInfo> BuildLookupOfAppsDirectlyAffectingEachObject(OrderedAppCombination groupBeingFormedAppCombination)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<ApplicationObjectId, ObjectInfo> dictionary = new Dictionary<ApplicationObjectId, ObjectInfo>();
		foreach (NavAppRuntimeMetadata orderedAppMetadatum in groupBeingFormedAppCombination.OrderedAppMetadata)
		{
			foreach (NavAppObjectMetadataSummary objectMetadataSummary in orderedAppMetadatum.ObjectMetadataSummaries)
			{
				ApplicationObjectId key = new ApplicationObjectId(objectMetadataSummary.TargetObjectType, objectMetadataSummary.TargetObjectId);
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = new ObjectInfo();
					dictionary.Add(key, value);
				}
				NavAppRuntimeMetadata fullObjectOwner = (((int)objectMetadataSummary.Format == 0) ? orderedAppMetadatum : null);
				value.AddAppInfoInExpectedOrder(orderedAppMetadatum, fullObjectOwner, objectMetadataSummary.ObjectName);
			}
		}
		return dictionary;
	}
}
