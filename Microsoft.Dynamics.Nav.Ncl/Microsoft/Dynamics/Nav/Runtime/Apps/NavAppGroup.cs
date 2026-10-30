using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppGroup
{
	private class SummaryTargetTypeIdComparer : IEqualityComparer<NavAppObjectMetadataSummary>
	{
		public bool Equals(NavAppObjectMetadataSummary x, NavAppObjectMetadataSummary y)
		{
			if (x.TargetObjectType == y.TargetObjectType)
			{
				return x.TargetObjectId == y.TargetObjectId;
			}
			return false;
		}

		public int GetHashCode(NavAppObjectMetadataSummary obj)
		{
			return obj.ObjectType.GetHashCode() ^ obj.ObjectId.GetHashCode();
		}
	}

	private static readonly SummaryTargetTypeIdComparer summaryTargetTypeIdComparer = new SummaryTargetTypeIdComparer();

	private readonly OrderedAppCombination appCombination;

	private readonly ConcurrentDictionary<ApplicationObjectId, NavAppRuntimeMetadata> ownerLookup;

	private readonly Dictionary<Tuple<ApplicationObjectId, NavAppObjectFormat>, SpecificNavAppObjectMetadataSummaryPair> byIdLookups = new Dictionary<Tuple<ApplicationObjectId, NavAppObjectFormat>, SpecificNavAppObjectMetadataSummaryPair>();

	public const int BaseGroupId = 0;

	public const int None = -2;

	public static readonly NavAppGroup BaseGroup = new NavAppGroup(0, new OrderedAppCombination(Enumerable.Empty<NavAppRuntimeMetadata>()), Enumerable.Empty<NavAppGroupObjectMetadataSummary>());

	public static readonly NavAppGroup ExtensionGroup = new NavAppGroup(-2, new OrderedAppCombination(Enumerable.Empty<NavAppRuntimeMetadata>()), Enumerable.Empty<NavAppGroupObjectMetadataSummary>());

	public int GroupId { get; }

	public string StableCombinationId => appCombination.StableCombinationId;

	public IReadOnlyList<NavAppRuntimeMetadata> OrderedAppMetadata { get; }

	public IReadOnlyList<NavAppGroupObjectMetadataSummary> GroupObjectMetadataSummaries { get; }

	public NavAppGroup(int groupId, OrderedAppCombination appCombo, IEnumerable<NavAppGroupObjectMetadataSummary> groupSummaries)
	{
		GroupId = groupId;
		OrderedAppMetadata = appCombo.OrderedAppMetadata;
		GroupObjectMetadataSummaries = (from s in groupSummaries
			orderby s.ObjectType, s.ObjectId
			select (s.MetadataAppGroup != null) ? s : NavAppGroupObjectMetadataSummary.CreateForKnownGroup(this, s.FullObjectOwner, s.ObjectType, s.ObjectId, s.ObjectName)).ToList();
		appCombination = new OrderedAppCombination(OrderedAppMetadata);
		ownerLookup = new ConcurrentDictionary<ApplicationObjectId, NavAppRuntimeMetadata>();
	}

	public NavAppRuntimeMetadata GetObjectOwner(ObjectType objectType, int objectId)
	{
		ApplicationObjectId key = new ApplicationObjectId(objectType, objectId);
		return ownerLookup.GetOrAdd(key, (Func<ApplicationObjectId, NavAppRuntimeMetadata>)((ApplicationObjectId o) =>
		{
			foreach (NavAppRuntimeMetadata orderedAppMetadatum in OrderedAppMetadata)
			{
				if (orderedAppMetadatum.ContainsObject(o))
				{
					return orderedAppMetadatum;
				}
			}
			return (NavAppRuntimeMetadata)null;
		}));
	}

	public bool ContainsAppExtensionsForObject(ObjectType objectType, int objectId)
	{
		ApplicationObjectId applicationObjectId = new ApplicationObjectId(objectType, objectId);
		foreach (NavAppRuntimeMetadata orderedAppMetadatum in OrderedAppMetadata)
		{
			if (orderedAppMetadatum.ContainsExtensionsForObject(applicationObjectId))
			{
				return true;
			}
		}
		return false;
	}

	public IReadOnlyList<NavAppObjectMetadataSummary> GetDistinctNewObjectsAddedByApps()
	{
		return (from p in GetDistinctAppObjectsByTargetAndFormat((NavAppObjectFormat)0)
			select p.Summary).ToList();
	}

	public IReadOnlyList<SpecificNavAppObjectMetadataSummaryPair> GetExtensionObjects()
	{
		List<SpecificNavAppObjectMetadataSummaryPair> list = new List<SpecificNavAppObjectMetadataSummaryPair>();
		foreach (NavAppRuntimeMetadata appMetadata in OrderedAppMetadata)
		{
			list.AddRange(from s in appMetadata.ObjectMetadataSummaries.Where((NavAppObjectMetadataSummary s) =>
				{
					//IL_0001: Unknown result type (might be due to invalid IL or missing references)
					//IL_0007: Invalid comparison between Unknown and I4
					return (int)s.Format == 2;
				})
				select new SpecificNavAppObjectMetadataSummaryPair(appMetadata, s));
		}
		return list;
	}

	public int GetAppDependencyOrderForObject(ObjectType objectType, int objectId)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		ApplicationObjectId applicationObjectId = new ApplicationObjectId(objectType, objectId);
		for (int i = 0; i < OrderedAppMetadata.Count; i++)
		{
			NavAppRuntimeMetadata val = OrderedAppMetadata[i];
			NavAppObjectMetadataSummary objectSummary = val.GetObjectSummary(applicationObjectId);
			if (objectSummary != null && (int)objectSummary.Format == 0)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetAppDependencyOrderForExtensionObject(int runtimeAppId)
	{
		for (int i = 0; i < OrderedAppMetadata.Count; i++)
		{
			NavAppRuntimeMetadata val = OrderedAppMetadata[i];
			if (val.RuntimeAppId == runtimeAppId)
			{
				return i;
			}
		}
		return -1;
	}

	public SpecificNavAppObjectMetadataSummaryPair GetObjectSummary(ApplicationObjectId objectId)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		NavAppObjectFormat format = (NavAppObjectFormat)(objectId.ObjectType.IsExtensionType() ? 2 : 0);
		lock (byIdLookups)
		{
			Tuple<ApplicationObjectId, NavAppObjectFormat> key = Tuple.Create<ApplicationObjectId, NavAppObjectFormat>(objectId, format);
			if (!byIdLookups.TryGetValue(key, out var value))
			{
				foreach (NavAppRuntimeMetadata orderedAppMetadatum in OrderedAppMetadata)
				{
					foreach (NavAppObjectMetadataSummary item in orderedAppMetadatum.ObjectMetadataSummaries.Where((NavAppObjectMetadataSummary s) =>
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						//IL_0007: Unknown result type (might be due to invalid IL or missing references)
						return s.Format == format;
					}))
					{
						if (item.ObjectType == objectId.ObjectType && item.ObjectId == objectId.ObjectNumber)
						{
							value = new SpecificNavAppObjectMetadataSummaryPair(orderedAppMetadatum, item);
							byIdLookups.Add(key, value);
							return value;
						}
					}
				}
				byIdLookups.Add(key, null);
			}
			return value;
		}
	}

	private IReadOnlyList<SpecificNavAppObjectMetadataSummaryPair> GetDistinctAppObjectsByTargetAndFormat(NavAppObjectFormat format)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<NavAppObjectMetadataSummary, SpecificNavAppObjectMetadataSummaryPair> dictionary = new Dictionary<NavAppObjectMetadataSummary, SpecificNavAppObjectMetadataSummaryPair>(summaryTargetTypeIdComparer);
		foreach (NavAppRuntimeMetadata orderedAppMetadatum in OrderedAppMetadata)
		{
			foreach (NavAppObjectMetadataSummary item in orderedAppMetadatum.ObjectMetadataSummaries.Where((NavAppObjectMetadataSummary s) =>
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				return format == s.Format;
			}))
			{
				if (!dictionary.ContainsKey(item))
				{
					dictionary.Add(item, new SpecificNavAppObjectMetadataSummaryPair(orderedAppMetadatum, item));
				}
			}
		}
		return dictionary.Values.ToList();
	}
}
