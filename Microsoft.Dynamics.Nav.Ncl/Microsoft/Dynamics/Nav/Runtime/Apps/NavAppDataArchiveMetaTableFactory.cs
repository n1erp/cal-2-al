using System;
using System.Linq;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public static class NavAppDataArchiveMetaTableFactory
{
	public static NCLMetaTable CreateArchiveMetaTableFromArchiveRecord(NavAppDataArchiveRecord record)
	{
		INCLMetaApplicationObjectLoader loader = CreateNCLLoader();
		MetaTable metaTableFromStream = NavSqlObjectMetadataSnapshotHandler.GetMetaTableFromStream(record.MetadataSnapshot.GetNewStream(), record.TableBlobVersion);
		NCLMetaTable nCLMetaTable = NCLMetaTable.CreateEmptyNCLMetaTable(loader, record.TableId, NavAppGroup.BaseGroup);
		nCLMetaTable.AssignFromMetaTable(metaTableFromStream, databaseConvertedFromNav2013: false);
		MetaTable metaTableFromStream2 = NavSqlObjectMetadataSnapshotHandler.GetMetaTableFromStream(record.MetadataSnapshot.GetNewStream(), record.TableBlobVersion);
		ConvertOriginalMetaTableToAppArchiveTableVersion(nCLMetaTable, metaTableFromStream2, record.AppId, record.ArchiveTableName);
		NCLMetaTable nCLMetaTable2 = NCLMetaTable.CreateEmptyNCLMetaTable(loader, record.TableId, NavAppGroup.BaseGroup);
		nCLMetaTable2.AssignFromMetaTable(metaTableFromStream2, databaseConvertedFromNav2013: false, isAppArchiveTable: true);
		return nCLMetaTable2;
	}

	public static NCLMetaTable CreateOriginalMetaTableFromArchiveRecord(NavAppDataArchiveRecord record)
	{
		MetaTable metaTableFromStream = NavSqlObjectMetadataSnapshotHandler.GetMetaTableFromStream(record.MetadataSnapshot.GetNewStream(), record.TableBlobVersion);
		NCLMetaTable nCLMetaTable = NCLMetaTable.CreateEmptyNCLMetaTable(CreateNCLLoader(), record.TableId, NavAppGroup.BaseGroup);
		nCLMetaTable.AssignFromMetaTable(metaTableFromStream, databaseConvertedFromNav2013: false);
		return nCLMetaTable;
	}

	private static INCLMetaApplicationObjectLoader CreateNCLLoader()
	{
		return NCLMetaApplicationObjectLoader.CreateFromNavGlobal();
	}

	private static void ConvertOriginalMetaTableToAppArchiveTableVersion(NCLMetaTable origTable, MetaTable tableBeingModified, Guid appId, string archiveTableName)
	{
		NCLMetaKey primaryKey = origTable.PrimaryKey;
		Func<MetaField, bool> isInPrimaryKey = (MetaField f) => primaryKey.KeyFieldsList.Any((NCLMetaField kf) => kf.FieldNo == f.Id);
		Func<MetaField, bool> isForThisApp = (MetaField f) => f.SourceAppId != null && Guid.Parse(f.SourceAppId) == appId;
		tableBeingModified.Fields.RemoveAll((MetaField f) => f.FieldClass != FieldClass.Normal || (!isInPrimaryKey(f) && !isForThisApp(f)));
		tableBeingModified.SourceAppId = null;
		tableBeingModified.Fields.ForEach((MetaField f) =>
		{
			f.SourceAppId = null;
		});
		foreach (MetaField field in tableBeingModified.Fields)
		{
			field.AutoIncrement = false;
		}
		tableBeingModified.Fields.RemoveAll((MetaField f) => f.Timestamp);
		tableBeingModified.Name = archiveTableName;
		tableBeingModified.IsDataPerCompany = false;
		int count = tableBeingModified.Keys.Count;
		if (count > 0)
		{
			for (int num = count - 1; num > 0; num--)
			{
				tableBeingModified.Keys.RemoveAt(num);
			}
			tableBeingModified.Keys[0].SumIndexFields.Clear();
		}
	}
}
