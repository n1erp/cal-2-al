using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class SyncTableAndChangeCreator
{
	private static readonly Dictionary<SyncFieldChangeType, FieldChangeTypes> fieldChangeMap = new Dictionary<SyncFieldChangeType, FieldChangeTypes>
	{
		{
			SyncFieldChangeType.Add,
			FieldChangeTypes.Missing
		},
		{
			SyncFieldChangeType.AutoIncrement,
			FieldChangeTypes.AutoIncrement
		},
		{
			SyncFieldChangeType.Compression,
			FieldChangeTypes.Compression
		},
		{
			SyncFieldChangeType.DataType,
			FieldChangeTypes.DataType
		},
		{
			SyncFieldChangeType.Length,
			FieldChangeTypes.Length
		},
		{
			SyncFieldChangeType.Name,
			FieldChangeTypes.Name
		},
		{
			SyncFieldChangeType.NoChange,
			FieldChangeTypes.None
		},
		{
			SyncFieldChangeType.Remove,
			FieldChangeTypes.Excessive
		},
		{
			SyncFieldChangeType.SqlDataType,
			FieldChangeTypes.SqlDataType
		}
	};

	private static readonly Dictionary<SyncKeyChangeType, IndexChangeTypes> keyChangeMap = new Dictionary<SyncKeyChangeType, IndexChangeTypes>
	{
		{
			SyncKeyChangeType.AddSqlIndex,
			IndexChangeTypes.Missing
		},
		{
			SyncKeyChangeType.AddSiftIndex,
			IndexChangeTypes.SiftMissing
		},
		{
			SyncKeyChangeType.RemoveSqlIndex,
			IndexChangeTypes.Excessive
		},
		{
			SyncKeyChangeType.RemoveSiftIndex,
			IndexChangeTypes.SiftExcessive
		},
		{
			SyncKeyChangeType.Unique,
			IndexChangeTypes.Unique
		},
		{
			SyncKeyChangeType.IndexFields,
			IndexChangeTypes.IndexFields
		},
		{
			SyncKeyChangeType.SumFields,
			IndexChangeTypes.SumFields
		},
		{
			SyncKeyChangeType.Clustered,
			IndexChangeTypes.ClusterIndex
		},
		{
			SyncKeyChangeType.PrimaryKey,
			IndexChangeTypes.PrimaryKey
		}
	};

	public static NavSqlTableAndChange CreateFromLogicalChanges(AppSyncContext syncContext, SyncTableBundle bundle, SyncTableChanges tableChanges, MetaTable updatedSnapshot)
	{
		NavAppTableContext tableContext = ((bundle.ObjectType == ObjectType.TableExtension) ? NavAppTableContext.Companion(NavAppOrigin.CreateForApp(((WrappedValue<Guid>)(object)syncContext.AppId).Value)) : NavAppTableContext.Main);
		NCLMetaTable nCLMetaTable = ((updatedSnapshot != null) ? NCLMetaTable.CreateEmptyNCLMetaTable(null, bundle.TargetTableId, NavAppGroup.BaseGroup) : null);
		nCLMetaTable?.AssignFromMetaTable(updatedSnapshot, databaseConvertedFromNav2013: false);
		NavSqlTableAndChange navSqlTableAndChange;
		if (tableChanges.ChangeType == SyncTableChangeType.Add)
		{
			navSqlTableAndChange = NavSqlTableAndChange.CreateForAppSync(null, nCLMetaTable, tableContext, TableChangeType.Missing, Enumerable.Empty<NavSqlFieldAndChanges>(), Enumerable.Empty<NavSqlIndexAndChanges>());
		}
		else if (tableChanges.ChangeType == SyncTableChangeType.Remove)
		{
			navSqlTableAndChange = NavSqlTableAndChange.CreateForAppSync(bundle.OriginalSnapshotNCL, null, tableContext, TableChangeType.Excessive, Enumerable.Empty<NavSqlFieldAndChanges>(), Enumerable.Empty<NavSqlIndexAndChanges>());
		}
		else
		{
			List<NavSqlFieldAndChanges> list = TransformFieldChanges(bundle.OriginalSnapshotNCL, nCLMetaTable, tableChanges.FieldChanges);
			List<NavSqlIndexAndChanges> list2 = TransformKeyChanges(bundle.OriginalSnapshotNCL, nCLMetaTable, tableChanges.KeyChanges);
			navSqlTableAndChange = ((!list.Any() && !list2.Any() && tableChanges.ChangeType != SyncTableChangeType.Change) ? NavSqlTableAndChange.CreateForAppSync(bundle.OriginalSnapshotNCL, nCLMetaTable, tableContext, TableChangeType.None, Enumerable.Empty<NavSqlFieldAndChanges>(), Enumerable.Empty<NavSqlIndexAndChanges>()) : NavSqlTableAndChange.CreateForAppSync(bundle.OriginalSnapshotNCL, nCLMetaTable, tableContext, TableChangeType.Changed, list, list2));
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000018D", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Extension table change summary" + Environment.NewLine + SyncTableAndChangeStringizer.Stringize(navSqlTableAndChange));
		return navSqlTableAndChange;
	}

	private static List<NavSqlFieldAndChanges> TransformFieldChanges(NCLMetaTable originalSnapshot, NCLMetaTable updatedSnapshot, IEnumerable<SyncFieldChange> changes)
	{
		List<NavSqlFieldAndChanges> list = new List<NavSqlFieldAndChanges>();
		IEnumerable<IGrouping<string, SyncFieldChange>> enumerable = changes.GroupBy((SyncFieldChange c) =>
		{
			string text = c.Target?.FieldName;
			if (text == null)
			{
				NCLMetaField snapshot = c.Snapshot;
				if (snapshot == null)
				{
					return (string)null;
				}
				text = snapshot.FieldName;
			}
			return text;
		});
		foreach (IGrouping<string, SyncFieldChange> item in enumerable)
		{
			FieldChangeTypes fieldChangeTypes = FieldChangeTypes.None;
			SyncFieldChange syncFieldChange = item.First();
			NCLMetaField oldField;
			if (syncFieldChange.Snapshot == null)
			{
				oldField = null;
			}
			else
			{
				oldField = (originalSnapshot.TryGetFieldByName(syncFieldChange.Snapshot.FieldName, out var result) ? result : null);
			}
			NCLMetaField newField;
			if (syncFieldChange.Target == null)
			{
				newField = null;
			}
			else
			{
				newField = (updatedSnapshot.TryGetFieldByName(syncFieldChange.Target.FieldName, out var result2) ? result2 : null);
			}
			foreach (SyncFieldChange item2 in item)
			{
				fieldChangeTypes |= fieldChangeMap[item2.Change];
			}
			list.Add(new NavSqlFieldAndChanges(oldField, fieldChangeTypes, newField));
		}
		return list;
	}

	private static List<NavSqlIndexAndChanges> TransformKeyChanges(NCLMetaTable originalSnapshot, NCLMetaTable updatedSnapshot, IEnumerable<SyncKeyChange> changes)
	{
		List<NavSqlIndexAndChanges> list = new List<NavSqlIndexAndChanges>();
		IEnumerable<IGrouping<string, SyncKeyChange>> enumerable = from c in changes
			group c by c.TargetNCL?.KeyName ?? c.SnapshotNCL?.KeyName ?? c.TargetNCL?.GetKeyName() ?? c.SnapshotNCL.GetKeyName();
		foreach (IGrouping<string, SyncKeyChange> item2 in enumerable)
		{
			IndexChangeTypes indexChangeTypes = IndexChangeTypes.None;
			SyncKeyChange syncKeyChange = item2.First();
			NCLMetaKey nCLMetaKey = FindKey(originalSnapshot, syncKeyChange?.SnapshotNCL);
			NCLMetaKey newKey = FindKey(updatedSnapshot, syncKeyChange?.TargetNCL);
			foreach (SyncKeyChange item3 in item2)
			{
				indexChangeTypes |= keyChangeMap[item3.Change];
			}
			if (indexChangeTypes != IndexChangeTypes.None)
			{
				NavSqlIndexAndChanges item = new NavSqlIndexAndChanges(nCLMetaKey, nCLMetaKey, indexChangeTypes, newKey);
				list.Add(item);
			}
		}
		return list;
	}

	private static NCLMetaKey FindKey(NCLMetaTable table, NCLMetaKey keyFromChange)
	{
		if (table == null || keyFromChange == null)
		{
			return null;
		}
		return table.Keys.FirstOrDefault((NCLMetaKey k) => k.KeyName == keyFromChange.KeyName) ?? table.Keys.FirstOrDefault((NCLMetaKey k) => k.GetKeyName() == keyFromChange.GetKeyName());
	}
}
