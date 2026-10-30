using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public interface IAppSyncSnapshotRepository
{
	IEnumerable<AppSyncSnapshotRecord> GetAllForApp(AppId appId);

	void InsertOrUpdate(AppSyncSnapshotRecord record);

	void Delete(AppId appId, int tableId);

	void DeleteAllForApp(AppId appId);
}
