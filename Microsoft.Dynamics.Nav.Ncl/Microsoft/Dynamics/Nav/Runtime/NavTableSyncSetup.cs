using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTableSyncSetup
{
	private NCLMetaTable upgradeTableMetadata;

	public int OldTableId { get; private set; }

	public int UpgradeTableId { get; private set; }

	public DataUpgradeSetup SyncSetup { get; private set; }

	public NCLMetaTable UpgradeTableMetadata
	{
		get
		{
			if (upgradeTableMetadata == null)
			{
				upgradeTableMetadata = GetUpgradeMetaTableById(UpgradeTableId);
			}
			return upgradeTableMetadata;
		}
	}

	public NavTableSyncSetup(int oldTableId, int upgradeTableId, DataUpgradeSetup syncSetup)
	{
		OldTableId = oldTableId;
		UpgradeTableId = upgradeTableId;
		SyncSetup = syncSetup;
	}

	private static NCLMetaTable GetUpgradeMetaTableById(int upgradeTableId)
	{
		try
		{
			return NavGlobal.NCLMetadata.GetMetaTableById(upgradeTableId);
		}
		catch (NavMetadataNotFoundException innerException)
		{
			throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.UpgradeTableIdDoesNotExists, upgradeTableId), innerException);
		}
	}
}
