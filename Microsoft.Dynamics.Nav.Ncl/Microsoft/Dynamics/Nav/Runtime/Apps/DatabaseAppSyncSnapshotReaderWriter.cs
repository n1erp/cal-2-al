using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public static class DatabaseAppSyncSnapshotReaderWriter
{
	public static List<AppSyncSnapshotRecord> ReadSyncedAppSnapshotRecordsFromBinary(BinaryReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		List<AppSyncSnapshotRecord> list = new List<AppSyncSnapshotRecord>();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			AppId appId = AppId.op_Implicit(Guid.Parse(reader.ReadString()));
			int targetTableId = reader.ReadInt32();
			bool isTableExtension = reader.ReadBoolean();
			XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
			xmlDocument.LoadXml(reader.ReadString());
			MetaTable tableSnapshot = DatabaseAppSyncSnapshotRepository.DeserializeMetaTable(xmlDocument.DocumentElement);
			string targetTableName = reader.ReadString();
			int originalObjectId = reader.ReadInt32();
			string originalObjectName = reader.ReadString();
			list.Add(new AppSyncSnapshotRecord
			{
				AppId = appId,
				TargetTableId = targetTableId,
				IsTableExtension = isTableExtension,
				TableSnapshot = tableSnapshot,
				TargetTableName = targetTableName,
				OriginalObjectId = originalObjectId,
				OriginalObjectName = originalObjectName
			});
		}
		return list;
	}

	public static void WriteSyncedAppSnapshotRecordsToStream(BinaryWriter writer, List<AppSyncSnapshotRecord> snapshotRecords)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (snapshotRecords == null)
		{
			throw new ArgumentNullException("snapshotRecords");
		}
		writer.Write(snapshotRecords.Count);
		foreach (AppSyncSnapshotRecord snapshotRecord in snapshotRecords)
		{
			writer.Write(((WrappedValue<Guid>)(object)snapshotRecord.AppId).Value.ToString(string.Empty, CultureInfo.InvariantCulture));
			writer.Write(snapshotRecord.TargetTableId);
			writer.Write(snapshotRecord.IsTableExtension);
			writer.Write(DatabaseAppSyncSnapshotRepository.SerializeMetaTable(snapshotRecord.TableSnapshot).OuterXml);
			writer.Write(snapshotRecord.TargetTableName);
			writer.Write(snapshotRecord.OriginalObjectId);
			writer.Write(snapshotRecord.OriginalObjectName);
		}
	}
}
