using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CacheSynchronizationStorage
{
	private class OptimizedBinaryReader : BinaryReader
	{
		public OptimizedBinaryReader(Stream input)
			: base(input)
		{
		}

		internal new int Read7BitEncodedInt()
		{
			return base.Read7BitEncodedInt();
		}
	}

	private class OptimizedBinaryWriter : BinaryWriter
	{
		public OptimizedBinaryWriter(Stream output)
			: base(output)
		{
		}

		internal new void Write7BitEncodedInt(int value)
		{
			base.Write7BitEncodedInt(value);
		}
	}

	private const string SelectCacheSync = "SELECT [Server Instance ID], CONVERT(BIGINT, [timestamp]), [modifiedtables] FROM [{0}].[dbo].[$ndo$cachesync] WITH (READCOMMITTED) WHERE [timestamp] >= @timestamp";

	private const string InsertCacheSync = "INSERT INTO [{0}].[dbo].[$ndo$cachesync] ([Server Instance ID]) VALUES (@serverInstanceId); SELECT CAST(SCOPE_IDENTITY() AS INT)";

	private const string DeleteOlderThanCacheSync = "DELETE FROM [{0}].[dbo].[$ndo$cachesync] WHERE [timestamp] < @timestamp";

	private const string GetLatestWrite = "SELECT ISNULL(MAX(CONVERT(BIGINT, [timestamp])), 0) FROM [{0}].[dbo].[$ndo$cachesync] WITH (READCOMMITTED)";

	private const string CreateTableStatement = "IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'$ndo$cachesync') AND OBJECTPROPERTY(id, N'IsTable') = 1) CREATE TABLE [{0}].[dbo].[$ndo$cachesync]( id\t\t\t\t\t      int              IDENTITY(1,1),[Server Instance ID ]    int              NOT NULL,modifiedtables           varbinary(max)   NULL,timestamp                timestamp        NOT NULL,CONSTRAINT [PK$ndo$cachesync] PRIMARY KEY CLUSTERED (id))";

	private readonly Func<int> getServerInstanceId;

	internal static long LastWriteToCacheSyncTimeStamp
	{
		get
		{
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Browse);
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			navSqlCommand.ExecuteScalar<long>(string.Format(CultureInfo.InvariantCulture, "SELECT ISNULL(MAX(CONVERT(BIGINT, [timestamp])), 0) FROM [{0}].[dbo].[$ndo$cachesync] WITH (READCOMMITTED)", NavGlobal.AppDatabase.DatabaseName));
			return (long)navSqlCommand.ExecuteScalar();
		}
	}

	public CacheSynchronizationStorage(Func<int> getServerInstanceId)
	{
		this.getServerInstanceId = getServerInstanceId;
	}

	internal static void CreateTable()
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'$ndo$cachesync') AND OBJECTPROPERTY(id, N'IsTable') = 1) CREATE TABLE [{0}].[dbo].[$ndo$cachesync]( id\t\t\t\t\t      int              IDENTITY(1,1),[Server Instance ID ]    int              NOT NULL,modifiedtables           varbinary(max)   NULL,timestamp                timestamp        NOT NULL,CONSTRAINT [PK$ndo$cachesync] PRIMARY KEY CLUSTERED (id))", NavGlobal.AppDatabase.DatabaseName));
	}

	internal IEnumerable<CacheSynchronizationChangeSet> Read(long timestamp)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
		NavSqlCommand navSqlCommand = navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("SelectCacheSync", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Server Instance ID], CONVERT(BIGINT, [timestamp]), [modifiedtables] FROM [{0}].[dbo].[$ndo$cachesync] WITH (READCOMMITTED) WHERE [timestamp] >= @timestamp", NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add("@timestamp", SqlDbType.BigInt);
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = timestamp;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteReader();
			return cmd;
		});
		List<CacheSynchronizationChangeSet> list = new List<CacheSynchronizationChangeSet>();
		while (navSqlCommand.ReadNextRow())
		{
			list.Add(ReadRow(navSqlCommand));
		}
		navSqlCommand.CloseReader(forceClose: false);
		return new ReadOnlyCollection<CacheSynchronizationChangeSet>(list);
	}

	private CacheSynchronizationChangeSet ReadRow(NavSqlCommand readerCommand)
	{
		CacheSynchronizationChangeSet cacheSynchronizationChangeSet = new CacheSynchronizationChangeSet();
		int @int = readerCommand.Reader.GetInt32(0);
		cacheSynchronizationChangeSet.Timestamp = readerCommand.Reader.GetInt64(1);
		if (@int == getServerInstanceId())
		{
			cacheSynchronizationChangeSet.TenantTableSets = Enumerable.Empty<TenantTableSet>();
			return cacheSynchronizationChangeSet;
		}
		using (Stream input = new BlobReaderStream(readerCommand.Reader, 2))
		{
			using OptimizedBinaryReader optimizedBinaryReader = new OptimizedBinaryReader(input);
			int num = 0;
			int num2 = optimizedBinaryReader.Read7BitEncodedInt();
			NavTenant[] array = new NavTenant[num2];
			for (int i = 0; i < num2; i++)
			{
				string tenantId = optimizedBinaryReader.ReadString();
				if (NavEnvironment.Instance.Tenants.TryGetTenantById(tenantId, out var tenant, includeSystemTenant: true))
				{
					array[i] = tenant;
					num++;
				}
			}
			if (num == 0)
			{
				cacheSynchronizationChangeSet.TenantTableSets = Enumerable.Empty<TenantTableSet>();
			}
			else
			{
				List<TenantTableSet> list = (List<TenantTableSet>)(cacheSynchronizationChangeSet.TenantTableSets = new List<TenantTableSet>(num));
				for (int j = 0; j < num2; j++)
				{
					TenantTableSet tenantTableSet = ReadTenantTableSet(optimizedBinaryReader);
					if (array[j] != null)
					{
						tenantTableSet.Tenant = array[j];
						list.Add(tenantTableSet);
						if (list.Count == num)
						{
							break;
						}
					}
				}
			}
		}
		return cacheSynchronizationChangeSet;
	}

	private static TenantTableSet ReadTenantTableSet(OptimizedBinaryReader reader)
	{
		TenantTableSet tenantTableSet = new TenantTableSet();
		int num = reader.Read7BitEncodedInt();
		List<TableSet> list = (List<TableSet>)(tenantTableSet.CompanyTableSets = new List<TableSet>(num));
		for (int i = 0; i < num; i++)
		{
			TableSet tableSet = new TableSet();
			tableSet.CompanyName = reader.ReadString();
			int num2 = reader.Read7BitEncodedInt();
			SortedSet<int> sortedSet = (tableSet.TableIds = new SortedSet<int>());
			int num3 = 0;
			for (int j = 0; j < num2; j++)
			{
				sortedSet.Add(num3 += reader.Read7BitEncodedInt());
			}
			list.Add(tableSet);
		}
		return tenantTableSet;
	}

	internal void Write(IList<NavTenant> tenants)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
		List<Tuple<NavTenant, List<TableSet>>> changes = tenants.Select((NavTenant tenant) => Tuple.Create(tenant, tenant.Database.CacheSynchronization.GetAndClearModifiedTables())).ToList();
		NavDiagnostics mostSpecificInstance = DiagnosticsResolver.GetMostSpecificInstance();
		NavTenant item;
		List<TableSet> item2;
		try
		{
			Write(navSqlConnectionScope, (OptimizedBinaryWriter writer) =>
			{
				writer.Write7BitEncodedInt(tenants.Count);
				foreach (Tuple<NavTenant, List<TableSet>> item3 in changes)
				{
					writer.Write(item3.Item1.Id);
				}
				foreach (Tuple<NavTenant, List<TableSet>> item4 in changes)
				{
					Serialize(item4.Item2, writer);
				}
			});
			navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
		}
		catch (Exception exception)
		{
			foreach (Tuple<NavTenant, List<TableSet>> item5 in changes)
			{
				item5.Deconstruct(out item, out item2);
				NavTenant navTenant = item;
				List<TableSet> tables = item2;
				mostSpecificInstance.SendExceptionTag("00001UB", Category.CacheSynchronization, exception, Verbosity.Warning, DiagnosticParameter.Oii(string.Format(CultureInfo.InvariantCulture, "Errors writing to synchronize cache for tenant {0}", navTenant.Id)));
				navTenant.Database.CacheSynchronization.RegisterTableDataChanged(tables);
			}
			navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Rollback);
			throw;
		}
		if (!mostSpecificInstance.EmittingEnabledForVerbosity(Verbosity.Verbose))
		{
			return;
		}
		foreach (Tuple<NavTenant, List<TableSet>> item6 in changes)
		{
			item6.Deconstruct(out item, out item2);
			NavTenant navTenant2 = item;
			List<TableSet> source = item2;
			HashSet<int> values = new HashSet<int>(source.SelectMany((TableSet ts) => ts.TableIds));
			mostSpecificInstance.SendTraceTag("00001UC", Category.CacheSynchronization, Verbosity.Verbose, "Writing synchronization data for tenant {0} for tables {1}", DiagnosticParameter.Oii(navTenant2.Id), DiagnosticParameter.SystemMetadata(string.Join(",", values)));
		}
	}

	private void Write(NavSqlConnectionScope conScope, Action<OptimizedBinaryWriter> serializationAction)
	{
		int num = conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("InsertCacheSync", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, "INSERT INTO [{0}].[dbo].[$ndo$cachesync] ([Server Instance ID]) VALUES (@serverInstanceId); SELECT CAST(SCOPE_IDENTITY() AS INT)", NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add("@serverInstanceId", SqlDbType.Int);
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = getServerInstanceId();
		}, (NavSqlCommand cmd) => (int)cmd.ExecuteScalar());
		using BlobWriterStream output = new BlobWriterStream(conScope.Connection, "dbo", "$ndo$cachesync", "modifiedtables", "id", SqlDbType.Int, num, isTenantTable: false, null);
		using OptimizedBinaryWriter obj = new OptimizedBinaryWriter(output);
		serializationAction(obj);
	}

	private static void Serialize(IList<TableSet> companyTableSets, OptimizedBinaryWriter writer)
	{
		writer.Write7BitEncodedInt(companyTableSets.Count);
		foreach (TableSet companyTableSet in companyTableSets)
		{
			Serialize(companyTableSet, writer);
		}
	}

	private static void Serialize(TableSet tableSet, OptimizedBinaryWriter writer)
	{
		writer.Write(tableSet.CompanyName);
		writer.Write7BitEncodedInt(tableSet.TableIds.Count());
		int num = 0;
		foreach (int tableId in tableSet.TableIds)
		{
			writer.Write7BitEncodedInt(tableId - num);
			num = tableId;
		}
	}

	public void DeleteOlderThan(long timestamp)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001UD", Category.CacheSynchronization, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "DeleteOlderThan {0}", timestamp);
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase, TransactionType.Update);
		navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("DeleteOlderThanCacheSync", NavGlobal.AppDatabase), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM [{0}].[dbo].[$ndo$cachesync] WHERE [timestamp] < @timestamp", NavGlobal.AppDatabase.DatabaseName), (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add("@timestamp", SqlDbType.BigInt);
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = timestamp;
		}, (NavSqlCommand cmd) =>
		{
			cmd.ExecuteNonQuery();
			return 0;
		});
	}
}
