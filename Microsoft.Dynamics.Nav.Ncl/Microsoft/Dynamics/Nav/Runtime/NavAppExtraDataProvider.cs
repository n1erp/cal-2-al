using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavAppExtraDataProvider : EagerVirtualDataProvider
{
	public static readonly NavAppExtraDataProvider Instance = new NavAppExtraDataProvider();

	public override int TableId => 2000000157;

	private NavAppExtraDataProvider()
	{
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		isOrderedByPrimaryKeyAscending = false;
		List<NavAppRuntimeMetadata> list = NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadata().ToList();
		List<NavAppRuntimeMetadata> source = NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadataVisibleToTenant(NavCurrentThread.Session.Tenant.Id).ToList();
		ImmutableHashSet<PackageId> immutableHashSet = source.Select((NavAppRuntimeMetadata m) => m.PackageId).ToImmutableHashSet();
		ImmutableHashSet<PackageId> immutableHashSet2 = (from m in source
			where m.Scope == NavAppScope.Tenant
			select m.PackageId).ToImmutableHashSet();
		IReadOnlyList<NavAppRuntimeMetadata> orderedAppMetadata = NavCurrentThread.Session.Tenant.NavAppGroup.OrderedAppMetadata;
		ImmutableHashSet<PackageId> immutableHashSet3 = orderedAppMetadata.Select((NavAppRuntimeMetadata m) => m.PackageId).ToImmutableHashSet();
		List<ReadOnlyRecordBuffer> list2 = new List<ReadOnlyRecordBuffer>(list.Count);
		foreach (NavAppRuntimeMetadata item2 in list)
		{
			bool value = immutableHashSet.Contains(item2.PackageId);
			bool flag = immutableHashSet3.Contains(item2.PackageId);
			bool flag2 = immutableHashSet2.Contains(item2.PackageId);
			ReadOnlyRecordBuffer item = new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, new NavGuid(((WrappedValue<Guid>)(object)item2.PackageId).Value), NavBoolean.Create(value), NavBoolean.Create(flag2 | flag));
			list2.Add(item);
		}
		return list2;
	}
}
