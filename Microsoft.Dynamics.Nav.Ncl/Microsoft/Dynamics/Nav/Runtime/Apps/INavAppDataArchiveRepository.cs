using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public interface INavAppDataArchiveRepository
{
	void InsertRecord(NavAppDataArchiveRecord record);

	void DeleteRecord(NavAppDataArchiveRecord record);

	NavAppDataArchiveRecord GetRecord(Guid appId, int tableId, string companyName);

	IEnumerable<NavAppDataArchiveRecord> GetRecordsForApp(Guid appId);

	IEnumerable<Tuple<Guid, Version>> GetAppsForTenant();

	IEnumerable<NavAppDataArchiveRecord> GetRecordsForAppAndCompany(Guid appId, string companyName);
}
