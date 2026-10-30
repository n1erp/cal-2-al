using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class DatabaseNavAppAddInRetriever : INavAppAddInRetriever
{
	public IEnumerable<NavAppAddInRecord> GetTenantAddInRecords(NavSession session, AddInCategory category)
	{
		List<NavAppAddInRecord> list = new List<NavAppAddInRecord>();
		using (new MaximizedPermissionScope(session, new int[1] { 2000000169 }))
		{
			NavRecord navRecord = new NavRecord(session, 2000000169, SecurityFiltering.Ignored);
			navRecord.ALInit();
			navRecord.ALSetFilter(5, "%1", NavInteger.Create((int)category));
			navRecord.ALSetFilter(7, "*");
			if (navRecord.ALFindSet(DataError.TrapError))
			{
				do
				{
					NavAppAddInRecord navAppAddInRecord = new NavAppAddInRecord();
					navAppAddInRecord.AppId = AppId.op_Implicit(navRecord.GetFieldValue(1).ToGuid());
					navAppAddInRecord.AddInName = navRecord.GetFieldValue(2).ToString();
					navAppAddInRecord.PublicKeyToken = navRecord.GetFieldValue(3).ToString();
					navAppAddInRecord.Version = Version.Parse(navRecord.GetFieldValue(4).ToString());
					navAppAddInRecord.Category = category;
					navAppAddInRecord.Description = navRecord.GetFieldValue(6).ToString();
					navAppAddInRecord.DatabaseRecordId = navRecord.ALRecordId;
					list.Add(navAppAddInRecord);
				}
				while (navRecord.ALNext(1) != 0);
			}
		}
		return list;
	}

	public NavAppAddInRawAssembly GetTenantAddInAssemblyContent(NavSession session, NavAppAddInRecord addInRecord)
	{
		using (new MaximizedPermissionScope(session, new int[1] { 2000000169 }))
		{
			NavRecord navRecord = new NavRecord(session, 2000000169, SecurityFiltering.Ignored);
			navRecord.ALInit();
			if (navRecord.ALGet(DataError.TrapError, addInRecord.DatabaseRecordId))
			{
				using Stream stream = GetRecordResourceStream(navRecord);
				using ZipArchive zipArchive = new ZipArchive(stream, ZipArchiveMode.Update);
				ZipArchiveEntry zipArchiveEntry = zipArchive.Entries.FirstOrDefault((ZipArchiveEntry e) => e.Name.StartsWith(addInRecord.AddInName, StringComparison.OrdinalIgnoreCase));
				if (zipArchiveEntry != null)
				{
					byte[] array = new byte[zipArchiveEntry.Length];
					using Stream stream2 = zipArchiveEntry.Open();
					stream2.Read(array, 0, array.Length);
					return new NavAppAddInRawAssembly(zipArchiveEntry.Name, array);
				}
			}
		}
		return null;
	}

	private static Stream GetRecordResourceStream(NavRecord record)
	{
		Stream result = null;
		if (!record.GetFieldValue(7).IsZeroOrEmpty && record.CalcFields(DataError.TrapError, new int[1] { 7 }))
		{
			result = ((NavBLOB)record.GetFieldValue(7)).GetStream();
		}
		return result;
	}
}
