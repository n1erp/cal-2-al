using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class LoadPackageDataOnlyNavAppDataArchiveManager : NavAppDataArchiveTenantUpgradeManager
{
	public LoadPackageDataOnlyNavAppDataArchiveManager(NavSession session, NavAppInstallationContext installContext)
		: base(session, installContext)
	{
	}

	public override NavRecordRef GetArchiveRecordRef(int tableId)
	{
		return null;
	}

	public override string GetArchiveVersion()
	{
		return string.Empty;
	}

	public override bool RestoreArchiveData(DataError errorLevel, int tableId, bool runApplicationTrigger)
	{
		return false;
	}

	public override void DeleteArchiveData(int tableId)
	{
	}
}
