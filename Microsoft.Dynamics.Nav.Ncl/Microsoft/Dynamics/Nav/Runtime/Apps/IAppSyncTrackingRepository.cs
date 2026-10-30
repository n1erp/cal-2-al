using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public interface IAppSyncTrackingRepository
{
	IEnumerable<AppSyncTrackingRecord> GetAll(bool hasBaseline = true);

	AppSyncTrackingRecord GetForApp(AppId appId);

	void InsertOrUpdate(AppSyncTrackingRecord record, bool forceUpdateBaseline);

	void DeleteForApp(AppId appId);
}
