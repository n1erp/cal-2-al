using System;
using System.Linq;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class TableChangeMonitor : IDisposable
{
	private int setupVersion;

	private readonly int[] tableIds;

	private readonly NavDatabase database;

	private readonly NavDatabase appDatabase;

	private readonly bool watchMetadataChanges;

	protected NavDatabase Database => database;

	public int SetupVersion => setupVersion;

	protected TableChangeMonitor(NavDatabase database, int[] tableIds, bool watchMetadataChanges)
	{
		this.tableIds = tableIds;
		this.database = database;
		this.watchMetadataChanges = watchMetadataChanges;
		this.database.CacheSynchronization.TableDataChanged += TableDataChanged;
		SystemTableTriggers.TableChangeSubscribe(tableIds, ResetSetup);
		if (this.watchMetadataChanges)
		{
			appDatabase = ((this.database.DatabaseType == NavDatabaseType.Application) ? this.database : NavGlobal.AppDatabase);
			appDatabase.MetadataChanged += MetadataChanged;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (watchMetadataChanges)
			{
				appDatabase.MetadataChanged -= MetadataChanged;
			}
			SystemTableTriggers.TableChangeUnsubscribe(ResetSetup);
			Database.CacheSynchronization.TableDataChanged -= TableDataChanged;
		}
	}

	internal void TableDataChanged(object sender, TableDataChangedEventArgs changedTables)
	{
		if (changedTables.Tables.SelectMany((TableSet set) => set.TableIds).Any((int tableId) => tableIds.Contains(tableId)))
		{
			ResetSetup(database.Tenant.Id, changedTables.Tables.SelectMany((TableSet t) => t.TableIds).ToArray());
		}
	}

	internal void MetadataChanged(object sender, NavTrackingEventArgs e)
	{
		ResetSetup(NavGlobal.SystemTenant.Id, new int[1] { 2000000071 });
	}

	public void IncrementSetupVersion()
	{
		Interlocked.Increment(ref setupVersion);
	}

	protected abstract void ResetSetup(string tenantId, int[] changedTables);
}
