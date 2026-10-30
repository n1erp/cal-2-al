using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Apps.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public static class DatabaseUninstalledAppReaderWriter
{
	public static List<UninstalledAppRecord> ReadUninstalledAppRecordsFromBinary(BinaryReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		List<UninstalledAppRecord> list = new List<UninstalledAppRecord>();
		int num = reader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			string tenantId = reader.ReadString();
			AppId appId = AppId.op_Implicit(Guid.Parse(reader.ReadString()));
			Version version = Version.Parse(reader.ReadString());
			string name = reader.ReadString();
			string publisher = reader.ReadString();
			DateTime timeOfUninstall = DateTime.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
			bool hasData = reader.ReadBoolean();
			list.Add(new UninstalledAppRecord
			{
				TenantId = tenantId,
				AppId = appId,
				Version = version,
				Name = name,
				Publisher = publisher,
				TimeOfUninstall = timeOfUninstall,
				HasData = hasData
			});
		}
		return list;
	}

	public static void WriteUninstalledAppRecordsToStream(BinaryWriter writer, List<UninstalledAppRecord> uninstalledRecords)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		if (uninstalledRecords == null)
		{
			throw new ArgumentNullException("uninstalledRecords");
		}
		writer.Write(uninstalledRecords.Count);
		foreach (UninstalledAppRecord uninstalledRecord in uninstalledRecords)
		{
			writer.Write(uninstalledRecord.TenantId);
			writer.Write(((WrappedValue<Guid>)(object)uninstalledRecord.AppId).Value.ToString(string.Empty, CultureInfo.InvariantCulture));
			writer.Write(uninstalledRecord.Version.ToString());
			writer.Write(uninstalledRecord.Name);
			writer.Write(uninstalledRecord.Publisher);
			writer.Write(uninstalledRecord.TimeOfUninstall.ToString(CultureInfo.InvariantCulture));
			writer.Write(uninstalledRecord.HasData);
		}
	}
}
