using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Apps.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public static class DatabaseAppSyncTrackingReaderWriter
{
	public static List<AppSyncTrackingRecord> ReadSyncedAppTrackingRecordsFromBinary(BinaryReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		List<AppSyncTrackingRecord> list = new List<AppSyncTrackingRecord>();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			AppId appId = AppId.op_Implicit(Guid.Parse(reader.ReadString()));
			Version version = Version.Parse(reader.ReadString());
			string name = reader.ReadString();
			string publisher = reader.ReadString();
			Version baselineVersion = Version.Parse(reader.ReadString());
			list.Add(new AppSyncTrackingRecord
			{
				AppId = appId,
				Version = version,
				Name = name,
				Publisher = publisher,
				BaselineVersion = baselineVersion
			});
		}
		return list;
	}

	public static void WriteSyncedAppTrackingRecordsToStream(BinaryWriter writer, List<AppSyncTrackingRecord> trackingRecords)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (trackingRecords == null)
		{
			throw new ArgumentNullException("trackingRecords");
		}
		writer.Write(trackingRecords.Count);
		foreach (AppSyncTrackingRecord trackingRecord in trackingRecords)
		{
			writer.Write(((WrappedValue<Guid>)(object)trackingRecord.AppId).Value.ToString(string.Empty, CultureInfo.InvariantCulture));
			writer.Write(trackingRecord.Version.ToString());
			writer.Write(trackingRecord.Name);
			writer.Write(trackingRecord.Publisher);
			writer.Write(trackingRecord.BaselineVersion.ToString());
		}
	}
}
