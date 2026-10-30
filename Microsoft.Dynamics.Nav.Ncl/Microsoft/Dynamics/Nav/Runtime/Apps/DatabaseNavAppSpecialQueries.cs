using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class DatabaseNavAppSpecialQueries
{
	public static NavAppMetadata GetMetadataForDistinctAppInTenant(string name, string publisher)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		List<NavAppMetadata> list = (from m in GetMetadataFromInstalledAppTable()
			where name.Equals(m.Name, StringComparison.OrdinalIgnoreCase)
			where string.IsNullOrEmpty(publisher) || publisher.Equals(m.Publisher, StringComparison.OrdinalIgnoreCase)
			select m).Take(2).ToList();
		if (!list.Any())
		{
			throw NavAppNotFoundException.Create(name, NavCurrentThread.Session.Tenant.Id);
		}
		if (list.Count > 1)
		{
			throw NavAppMultipleAppsFoundException.Create();
		}
		return list.First();
	}

	private static IEnumerable<NavAppMetadata> GetMetadataFromInstalledAppTable()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected Obj, but got Unknown
		List<NavAppMetadata> list = new List<NavAppMetadata>();
		using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, 2000000153);
		bool flag = navRecord.ALFindFirst(DataError.TrapError);
		while (flag)
		{
			list.Add(new NavAppMetadata
			{
				PackageId = PackageId.op_Implicit(navRecord.GetFieldValue(2).ToGuid()),
				AppId = AppId.op_Implicit(navRecord.GetFieldValue(1).ToGuid()),
				Name = navRecord.GetFieldValue(3).ToString(),
				Publisher = navRecord.GetFieldValue(4).ToString(),
				VersionMajor = navRecord.GetFieldValue(5).ToInt32(),
				VersionMinor = navRecord.GetFieldValue(6).ToInt32(),
				VersionBuild = navRecord.GetFieldValue(7).ToInt32(),
				VersionRevision = navRecord.GetFieldValue(8).ToInt32(),
				CompatibilityMajor = navRecord.GetFieldValue(9).ToInt32(),
				CompatibilityMinor = navRecord.GetFieldValue(10).ToInt32(),
				CompatibilityBuild = navRecord.GetFieldValue(11).ToInt32(),
				CompatibilityRevision = navRecord.GetFieldValue(12).ToInt32()
			});
			flag = navRecord.NextRecord();
		}
		return list;
	}
}
