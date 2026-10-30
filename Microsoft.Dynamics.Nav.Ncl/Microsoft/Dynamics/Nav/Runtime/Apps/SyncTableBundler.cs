using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class SyncTableBundler
{
	public static IEnumerable<SyncTableBundle> CreateTableBundles(NavAppSyncMode mode, NavAppRuntimeMetadata targetApp, IEnumerable<MetaTable> targetAppTables, IEnumerable<AppSyncSnapshotRecord> snapshotsForApp, IEnumerable<ISyncBundleFilter> filters)
	{
		switch (mode)
		{
		case NavAppSyncMode.Add:
		case NavAppSyncMode.Development:
			return CreateTableBundles(targetApp, targetAppTables, snapshotsForApp, filters, allowIdFallback: false);
		case NavAppSyncMode.ForceSync:
			return CreateTableBundles(targetApp, targetAppTables, snapshotsForApp, filters, allowIdFallback: true);
		case NavAppSyncMode.Clean:
			return CreateTableBundlesForClean(snapshotsForApp);
		default:
			throw new InvalidOperationException("Unsupported sync mode: " + mode);
		}
	}

	private static IEnumerable<SyncTableBundle> CreateTableBundles(NavAppRuntimeMetadata targetApp, IEnumerable<MetaTable> targetAppTables, IEnumerable<AppSyncSnapshotRecord> snapshotsForApp, IEnumerable<ISyncBundleFilter> filters, bool allowIdFallback)
	{
		List<SyncTableBundle> list = new List<SyncTableBundle>();
		List<MetaTable> source = targetAppTables.ToList();
		List<AppSyncSnapshotRecord> source2 = snapshotsForApp.ToList();
		List<ISyncBundleFilter> filters2 = filters.ToList();
		Dictionary<string, MetaTable> dictionary = source.ToDictionary((MetaTable t) => t.Name);
		Dictionary<int, MetaTable> dictionary2 = source.ToDictionary((MetaTable t) => t.Id);
		Dictionary<string, AppSyncSnapshotRecord> dictionary3 = source2.ToDictionary((AppSyncSnapshotRecord t) => t.TargetTableName);
		Dictionary<int, AppSyncSnapshotRecord> dictionary4 = source2.ToDictionary((AppSyncSnapshotRecord t) => t.TargetTableId);
		List<string> list2 = dictionary3.Keys.ToList();
		foreach (string item in list2)
		{
			AppSyncSnapshotRecord appSyncSnapshotRecord = dictionary3[item];
			MetaTable value = (dictionary.TryGetValue(appSyncSnapshotRecord.TargetTableName, out value) ? value : null);
			if (value != null)
			{
				dictionary.Remove(value.Name);
				dictionary2.Remove(value.Id);
			}
			if ((value == null) & allowIdFallback)
			{
				value = (dictionary2.TryGetValue(appSyncSnapshotRecord.TargetTableId, out value) ? value : null);
				if (value != null)
				{
					dictionary.Remove(value.Name);
					dictionary2.Remove(value.Id);
				}
			}
			dictionary3.Remove(appSyncSnapshotRecord.TargetTableName);
			dictionary4.Remove(appSyncSnapshotRecord.TargetTableId);
			ProcessRawMatchComponents(targetApp, value, appSyncSnapshotRecord, filters2, list);
		}
		foreach (MetaTable value2 in dictionary.Values)
		{
			ProcessRawMatchComponents(targetApp, value2, null, filters2, list);
		}
		return list;
	}

	private static void ProcessRawMatchComponents(NavAppRuntimeMetadata targetApp, MetaTable targetTable, AppSyncSnapshotRecord snapshotRecord, List<ISyncBundleFilter> filters, List<SyncTableBundle> tableBundles)
	{
		NavAppObjectMetadataSummary val = targetApp.ObjectMetadataSummaries.FirstOrDefault((NavAppObjectMetadataSummary s) => s.TargetObjectType == ObjectType.Table && s.TargetObjectId == targetTable?.Id);
		ObjectType objectType = ((val != null) ? val.ObjectType : ((!snapshotRecord.IsTableExtension) ? ObjectType.Table : ObjectType.TableExtension));
		int objectId = ((val != null) ? val.ObjectId : snapshotRecord.OriginalObjectId);
		string objectName = ((val != null) ? val.ObjectName : null) ?? targetTable?.Name ?? snapshotRecord.OriginalObjectName;
		SyncTableBundle bundle = new SyncTableBundle(objectType, objectId, objectName, snapshotRecord?.TableSnapshot, targetTable);
		if (!filters.Any((ISyncBundleFilter rule) => rule.ShouldExclude(targetApp, bundle)))
		{
			tableBundles.Add(bundle);
		}
	}

	private static IEnumerable<SyncTableBundle> CreateTableBundlesForClean(IEnumerable<AppSyncSnapshotRecord> snapshotsForApp)
	{
		List<SyncTableBundle> list = new List<SyncTableBundle>();
		foreach (AppSyncSnapshotRecord item2 in snapshotsForApp)
		{
			ObjectType objectType = ((!item2.IsTableExtension) ? ObjectType.Table : ObjectType.TableExtension);
			SyncTableBundle item = new SyncTableBundle(objectType, item2.OriginalObjectId, item2.OriginalObjectName, item2.TableSnapshot, null);
			list.Add(item);
		}
		return list;
	}
}
