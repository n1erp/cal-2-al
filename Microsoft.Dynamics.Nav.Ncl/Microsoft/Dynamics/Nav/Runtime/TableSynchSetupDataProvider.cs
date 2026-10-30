using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class TableSynchSetupDataProvider : EagerVirtualDataProvider, ITreeObject, IDisposable
{
	private readonly Dictionary<int, NavTableSyncSetup> tableMapping;

	private readonly HashSet<int> upgradeTableIds;

	private readonly TreeHandler tree;

	public override int TableId => 2000000135;

	public TreeHandler Tree => tree;

	public IDictionary<int, NavTableSyncSetup> RawData => new Dictionary<int, NavTableSyncSetup>(tableMapping);

	public static TableSynchSetupDataProvider GetInstanceForSession(NavSession session)
	{
		return session.Tree.Children.OfType<TableSynchSetupDataProvider>().FirstOrDefault() ?? new TableSynchSetupDataProvider(session);
	}

	private TableSynchSetupDataProvider(NavSession session)
	{
		tree = TreeHandler.CreateTreeHandler(session, this);
		tableMapping = new Dictionary<int, NavTableSyncSetup>();
		upgradeTableIds = new HashSet<int>();
	}

	public void SetTableIds(IEnumerable<int> tableIds)
	{
		foreach (int tableId in tableIds)
		{
			tableMapping.Add(tableId, null);
		}
	}

	protected override IEnumerable<ReadOnlyRecordBuffer> GetAllItems(out bool isOrderedByPrimaryKeyAscending)
	{
		isOrderedByPrimaryKeyAscending = true;
		return new SortedSet<int>(tableMapping.Keys).Select((int key) => RecordFromKey(key));
	}

	private ReadOnlyRecordBuffer RecordFromKey(int key)
	{
		if (tableMapping[key] != null)
		{
			return new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavInteger.Create(key), NavInteger.Create(tableMapping[key].UpgradeTableId), NavOption.Create(MetaTable.AllFields[3].FieldOptionMetadata, (int)tableMapping[key].SyncSetup));
		}
		return new ReadOnlyRecordBuffer(MetaTable, VirtualDataProvider.VirtualTimeStamp, NavInteger.Create(key), NavInteger.Default, NavOption.Create(MetaTable.AllFields[3].FieldOptionMetadata, 0));
	}

	public override ModifyResult Modify(int companyToken, MutableRecordBuffer recordBuffer, bool recordHasBeenReadWithLockInsideThisTransaction, out ReadOnlyRecordBuffer outputRecordBuffer)
	{
		int num = recordBuffer.ReadOnlyBuffer[1].ToInt32();
		int num2 = recordBuffer[1].ToInt32();
		if (num != num2)
		{
			throw new NotSupportedException();
		}
		if (tableMapping[num] != null)
		{
			throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.MultipleTableSchemaSyncSetups, num));
		}
		int num3 = recordBuffer[3].ToInt32();
		if (!Enum.IsDefined(typeof(DataUpgradeSetup), num3))
		{
			throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.TableSyncSetupValueNotSupported, num3, num));
		}
		int upgradeTableId = recordBuffer[2].ToInt32();
		ReserveUpgradeTableId(upgradeTableId);
		tableMapping[num] = new NavTableSyncSetup(num, upgradeTableId, (DataUpgradeSetup)num3);
		outputRecordBuffer = RecordFromKey(num);
		return ModifyResult.Success;
	}

	private void ReserveUpgradeTableId(int upgradeTableId)
	{
		if (upgradeTableId != 0)
		{
			if (upgradeTableIds.Contains(upgradeTableId))
			{
				throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.MultipleUpgradeTableSyncSetups, upgradeTableId));
			}
			upgradeTableIds.Add(upgradeTableId);
		}
	}

	public void Dispose()
	{
		tree.Dispose();
	}
}
