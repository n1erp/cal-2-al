using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class SyncMetadataResolver
{
	private readonly INavAppMetadataRetriever metadataRetriever;

	private readonly INCLObjectXmlMetadataLoader applicationObjectXmlMetadataLoader;

	private readonly object lockObj = new object();

	private readonly HashSet<Tuple<int, int>> initializedApps = new HashSet<Tuple<int, int>>();

	private readonly Dictionary<Tuple<int, int>, Dictionary<int, MetaTable>> tableCache = new Dictionary<Tuple<int, int>, Dictionary<int, MetaTable>>();

	public SyncMetadataResolver(INavAppMetadataRetriever metadataRetriever, INCLObjectXmlMetadataLoader applicationXmlMetadataLoader)
	{
		this.metadataRetriever = metadataRetriever;
		applicationObjectXmlMetadataLoader = applicationXmlMetadataLoader;
	}

	public IEnumerable<MetaTable> GetStructuralTablesForApp(NavAppRuntimeMetadata targetApp, NavAppGroup appGroup)
	{
		if (appGroup == NavAppGroup.BaseGroup)
		{
			return Enumerable.Empty<MetaTable>();
		}
		Tuple<int, int> tuple = CreateAppLookupKey(targetApp, appGroup);
		lock (lockObj)
		{
			EnsureInitializationHasRunForApp(targetApp, appGroup, tuple);
			Dictionary<int, MetaTable> dictionary = tableCache[tuple];
			return dictionary.Values;
		}
	}

	public void ClearCache()
	{
		lock (lockObj)
		{
			initializedApps.Clear();
			tableCache.Clear();
		}
	}

	private void EnsureInitializationHasRunForApp(NavAppRuntimeMetadata targetApp, NavAppGroup appGroup, Tuple<int, int> lookupKey)
	{
		if (!initializedApps.Contains(lookupKey))
		{
			Dictionary<int, MetaTable> dictionary = new Dictionary<int, MetaTable>();
			InitializeNewAppTables(targetApp, appGroup, dictionary);
			InitializeExtendedTables(targetApp, appGroup, dictionary);
			tableCache.Add(lookupKey, dictionary);
			initializedApps.Add(lookupKey);
		}
	}

	private void InitializeNewAppTables(NavAppRuntimeMetadata appMetadata, NavAppGroup appGroup, Dictionary<int, MetaTable> tablesForThisApp)
	{
		List<NavAppObjectMetadataSummary> list = appMetadata.ObjectMetadataSummaries.Where((NavAppObjectMetadataSummary s) =>
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Invalid comparison between Unknown and I4
			return s.ObjectType == ObjectType.Table && (int)s.Format == 0;
		}).ToList();
		foreach (NavAppObjectMetadataSummary item in list)
		{
			MetaTable value = applicationObjectXmlMetadataLoader.GetMetaTable(item.ObjectId, appGroup).Value;
			if (!value.IsExternalDataSource)
			{
				tablesForThisApp.Add(item.ObjectId, value);
			}
			RemoveNonStructuralElementsFromTable(value);
		}
	}

	private void InitializeExtendedTables(NavAppRuntimeMetadata appMetadata, NavAppGroup appGroup, Dictionary<int, MetaTable> tablesForThisApp)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected Obj, but got Unknown
		//IL_00b9: Expected Obj, but got Unknown
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Expected Obj, but got Unknown
		List<NavAppObjectMetadataSummary> list = appMetadata.ObjectMetadataSummaries.Where((NavAppObjectMetadataSummary s) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			return (int)s.Format == 2 && s.ObjectType == ObjectType.TableExtension;
		}).ToList();
		foreach (NavAppObjectMetadataSummary item in list)
		{
			ApplicationObjectId subjectId = new ApplicationObjectId(item.TargetObjectType, item.TargetObjectId);
			MetaTable value = applicationObjectXmlMetadataLoader.GetMetaTable(subjectId.ObjectNumber, appGroup).Value;
			if (value.IsExternalDataSource)
			{
				continue;
			}
			ApplicationObjectId applicationObjectId = new ApplicationObjectId(item.ObjectType, item.ObjectId);
			NavAppObjectMetadataRuntimeDeltas metadataObject = metadataRetriever.GetObjectExtensionDeltas(applicationObjectId, appMetadata).MetadataObject;
			NavAppObjectMetadataRuntimeDeltas val = new NavAppObjectMetadataRuntimeDeltas();
			((NavAppObjectMetadataDeltaCreator<Delta>)val).AppMetadata = appMetadata;
			NavAppObjectMetadataRuntimeDeltas val2 = val;
			foreach (Delta allDelta in ((NavAppObjectMetadataDeltaCreator<Delta>)(object)metadataObject).AllDeltas)
			{
				if (allDelta is FieldAddDelta || allDelta is KeyAddDelta)
				{
					((NavAppObjectMetadataDeltaCreator<Delta>)(object)val2).Add(allDelta);
				}
			}
			if (!((NavAppObjectMetadataDeltaCreator<Delta>)(object)val2).AllDeltas.Any())
			{
				continue;
			}
			List<int> pkFieldIds = new List<int>();
			int count = value.Keys.Count;
			if (count > 0)
			{
				for (int num = count - 1; num > 0; num--)
				{
					value.Keys.RemoveAt(num);
				}
				value.Keys[0].SumIndexFields.Clear();
				pkFieldIds.AddRange(value.Keys[0].FieldRelations.Select((FieldMetadataRelation f) => f.Id));
			}
			value.Fields.RemoveAll((MetaField f) => !pkFieldIds.Contains(f.Id));
			MetadataRuntimeDeltaApplicator sharedInstanceWithAllAdapters = MetadataRuntimeDeltaApplicator.GetSharedInstanceWithAllAdapters(ObjectType.TableExtension);
			DeltaApplicationContext val3 = new DeltaApplicationContext();
			val3.SubjectId = subjectId;
			val3.Subject = value;
			val3.Deltas = new NavAppObjectMetadataRuntimeDeltas[1] { val2 };
			val3.SourceExtensionId = applicationObjectId;
			sharedInstanceWithAllAdapters.ApplyDeltas(val3, Array.Empty<IDeltaValidationFilter>());
			RemoveNonStructuralElementsFromTable(value);
			tablesForThisApp.Add(subjectId.ObjectNumber, value);
		}
	}

	private static void RemoveNonStructuralElementsFromTable(MetaTable table)
	{
		NCLMetaTable nCLMetaTable = NCLMetaTable.CreateEmptyNCLMetaTable(null, table.Id, NavAppGroup.BaseGroup);
		nCLMetaTable.AssignFromMetaTable(table, databaseConvertedFromNav2013: false);
		table.Fields.RemoveAll((MetaField f) => f.FieldClass != FieldClass.Normal || f.Timestamp || !f.Enabled);
		foreach (NCLMetaKey disabledKey in nCLMetaTable.Keys.Where((NCLMetaKey k) => !k.KeyIsActive))
		{
			table.Keys.RemoveAll((MetaKey k) => k.KeyName == disabledKey.KeyName);
		}
		foreach (NCLMetaKey keyToIgnore in from k in nCLMetaTable.Keys
			where k.MaintainSumIndex
			where k.SumFieldsList != null && k.SumFieldsList.Any((INavFieldMetadata f) => f.FieldClass != FieldClass.Normal)
			select k)
		{
			table.Keys.RemoveAll((MetaKey k) => k.KeyName == keyToIgnore.KeyName);
		}
	}

	private static Tuple<int, int> CreateAppLookupKey(NavAppRuntimeMetadata targetApp, NavAppGroup appGroup)
	{
		return Tuple.Create(targetApp.RuntimeAppId, appGroup.GroupId);
	}
}
