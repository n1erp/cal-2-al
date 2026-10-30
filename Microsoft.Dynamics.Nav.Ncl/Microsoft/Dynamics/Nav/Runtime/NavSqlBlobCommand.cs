using System;
using System.Text;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlBlobCommand : NavSqlCommand
{
	private class BlobCommandCacheKey : NavSqlCollationSensitiveCommandCacheKey
	{
		private readonly int companyIndex;

		public NCLMetaTable Table { get; private set; }

		public FieldList BlobFields { get; private set; }

		public string Hints { get; private set; }

		public BlobCommandCacheKey(NCLMetaTable table, NavDatabase database, int companyIndex, FieldList blobFields, string hints)
			: base(CachedCommandType.Blob, database, (table.TableId % 20000 << 8) | companyIndex)
		{
			Table = table;
			this.companyIndex = companyIndex;
			BlobFields = blobFields;
			Hints = hints;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is BlobCommandCacheKey blobCommandCacheKey))
			{
				return false;
			}
			if (!base.Equals(blobCommandCacheKey) || !Table.Equals(blobCommandCacheKey.Table) || companyIndex != blobCommandCacheKey.companyIndex || !string.Equals(Hints, blobCommandCacheKey.Hints))
			{
				return false;
			}
			if (BlobFields == null != (blobCommandCacheKey.BlobFields == null))
			{
				return false;
			}
			if (BlobFields != null)
			{
				if (BlobFields.Count != blobCommandCacheKey.BlobFields.Count)
				{
					return false;
				}
				for (int i = 0; i < BlobFields.Count; i++)
				{
					if (!BlobFields[i].Equals(blobCommandCacheKey.BlobFields[i]))
					{
						return false;
					}
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

	private const string SelectStr = "SELECT ";

	private const string FromStr = " FROM ";

	private BlobCommandCacheKey cacheKey;

	public override bool Stale => cacheKey.Table.Stale;

	private NavSqlBlobCommand(NavSqlConnection parentConnection, BlobCommandCacheKey cacheKey)
		: base(parentConnection)
	{
		this.cacheKey = cacheKey;
	}

	private static NavSqlBlobCommand Create(NavSqlConnection parentConnection, BlobCommandCacheKey cacheKey)
	{
		return new NavSqlBlobCommand(parentConnection, cacheKey);
	}

	private int ExecuteGetBlobCommand(MutableRecordBuffer record, NavDatabase database, int companyIndex, CompanyId? companyId)
	{
		FilterExpression filter = NavSqlWhereClauseBuilder.CreateRecordClause(FilterExpressionType.Equal, record, cacheKey.Table.PrimaryKey.SortingFieldsWithPrimaryKeyFields, useOriginalValues: false);
		if (State == NavSqlCommandState.Created)
		{
			StringBuilder stringBuilder = new StringBuilder("SELECT ");
			string defaultMainTableAlias = NavGlobal.SystemTenant.NavSqlTableAliasMap.GetDefaultMainTableAlias(cacheKey.Table.TableId);
			NavSqlStatementHelper.AppendFieldList(stringBuilder, cacheKey.BlobFields.AsNclMetaFields(), defaultMainTableAlias);
			NavSqlStatementHelper.AppendFromClauseWithCompanionTableJoinsIncluded(stringBuilder, cacheKey.Table, database, companyIndex, cacheKey.Hints, cacheKey.Table.DistinctCompanionTableOrigins, qualifyTableWithDatabase: true, defaultMainTableAlias);
			NavSqlWhereClauseBuilder.AppendFilterClause(stringBuilder, filter, new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId, shouldAliasQualifyFields: true, defaultMainTableAlias);
			SetCommandText(stringBuilder.ToString());
		}
		if (State != NavSqlCommandState.ExecutedWithPendingResults)
		{
			NavSqlWhereClauseBuilder.AddOrSetFilterParameters(filter, Command.Parameters, new NavSqlParameterHandler(), cacheKey.SqlSortingProperties, companyId);
		}
		PrepareStatement();
		if (State == NavSqlCommandState.Prepared)
		{
			ExecuteReader();
			if (!ReadNextRow())
			{
				return 85132272;
			}
		}
		int num = 0;
		int num2 = 0;
		foreach (NCLMetaField blobField in cacheKey.BlobFields)
		{
			NavBLOB navBLOB = (NavBLOB)record[blobField.FieldIndex];
			num = GetBlobDataFromReader(blobField.FieldIsCompressed, num2, navBLOB);
			record[blobField.FieldIndex] = navBLOB;
			if (num != 0)
			{
				break;
			}
			num2++;
		}
		CloseReader(forceClose: false);
		return num;
	}

	internal static FieldDictionary<NavValue> GetBlob(NCLMetaTable table, NavDatabase database, int companyIndex, FieldList blobFields, IRecordBuffer record, DataLockState lockState)
	{
		bool locking = lockState == DataLockState.LockExclusive;
		Tuple<INavFieldMetadata, NavValue>[] array = new Tuple<INavFieldMetadata, NavValue>[blobFields.Count];
		MutableRecordBuffer mutableRecordBuffer = (record as MutableRecordBuffer) ?? new MutableRecordBuffer((ReadOnlyRecordBuffer)record);
		int blob = GetBlob(table, database, companyIndex, blobFields, mutableRecordBuffer, locking);
		if (blob == 22926086)
		{
			throw new NavCSideException(blob);
		}
		for (int i = 0; i < blobFields.Count; i++)
		{
			NCLMetaField nCLMetaField = (NCLMetaField)blobFields[i];
			switch (blob)
			{
			case 0:
				array[i] = new Tuple<INavFieldMetadata, NavValue>(nCLMetaField, mutableRecordBuffer[nCLMetaField.FieldIndex]);
				break;
			case 85132272:
				array[i] = new Tuple<INavFieldMetadata, NavValue>(nCLMetaField, NavBLOB.Default());
				break;
			default:
				throw new NavCSideException(blob);
			}
		}
		return new FieldDictionary<NavValue>(array);
	}

	internal static int GetBlob(NCLMetaTable table, NavDatabase database, int companyIndex, FieldList blobMetaFields, MutableRecordBuffer record, bool locking)
	{
		database.CheckTableInSync(table);
		NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(database, updateOrlockingActivity: true);
		try
		{
			string hints = NavSqlStatementHelper.GenerateSelectStatementHints((!locking) ? DataLockState.Shared : DataLockState.LockExclusive, noExpand: false, conScope.Connection);
			BlobCommandCacheKey commandCacheKey = new BlobCommandCacheKey(table, database, NavSqlCommandCacheKey.GetCompanyIndexForCacheKey(database, companyIndex), blobMetaFields, hints);
			return conScope.Connection.ExecuteCommandAndCacheOnSuccess(commandCacheKey, () => Create(conScope.Connection, commandCacheKey), (NavSqlCommand command) => ((NavSqlBlobCommand)command).ExecuteGetBlobCommand(record, database, companyIndex, database.GetCompanyId(commandCacheKey.Table, companyIndex)));
		}
		finally
		{
			if (conScope != null)
			{
				((IDisposable)conScope).Dispose();
			}
		}
	}
}
