using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public interface IUninstalledAppRepository
{
	void InsertOrUpdate(UninstalledAppRecord record);

	void DeleteAllForApp(AppId appId, bool deleteForAllTenants = false);

	void DeleteAllForTenant();

	IEnumerable<UninstalledAppRecord> GetAllForTenant(string tenantId);

	IEnumerable<UninstalledAppRecord> GetAll();

	void ChangeTenantId(string oldId, string newId);
}
