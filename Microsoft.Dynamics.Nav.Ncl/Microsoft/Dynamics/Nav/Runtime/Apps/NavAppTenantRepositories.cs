using System;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppTenantRepositories
{
	public IAppSyncTrackingRepository TrackingRepository { get; }

	public IAppSyncSnapshotRepository SnapshotRepository { get; }

	public IUninstalledAppRepository UninstallRepository { get; }

	public NavAppTenantRepositories(NavDatabase tenantDatabase, NavSqlConnectionScope conScope)
	{
		if (tenantDatabase == null)
		{
			throw new ArgumentNullException("tenantDatabase");
		}
		if (conScope == null)
		{
			throw new ArgumentNullException("conScope");
		}
		TrackingRepository = new DatabaseAppSyncTrackingRepository(tenantDatabase, conScope);
		SnapshotRepository = new DatabaseAppSyncSnapshotRepository(tenantDatabase, conScope);
		UninstallRepository = new DatabaseUninstalledAppRepository(tenantDatabase, conScope);
	}
}
