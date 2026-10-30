using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;

internal class DatabaseNavAppTenantOperationRepository : INavAppTenantOperationRepository
{
	private class SensitiveColumnScope : IDisposable
	{
		private readonly int oldParameterNumber;

		private readonly SensitiveColumn column;

		public SensitiveColumnScope(SensitiveColumn column, int parameterNumber = 0)
		{
			this.column = column;
			oldParameterNumber = this.column.ParameterNumber;
			this.column.ParameterNumber = parameterNumber;
		}

		public void Dispose()
		{
			column.ParameterNumber = oldParameterNumber;
		}
	}

	private class SensitiveColumn
	{
		private readonly bool enabled;

		public static SensitiveColumn Empty { get; } = new SensitiveColumn(null);

		public int ParameterNumber { get; set; }

		public string QuotedColumnName { get; }

		public SqlDbType ParameterType { get; }

		public object Value { get; }

		public string AndClauseAddition => IfEnabled(FormattableString.Invariant($" AND {QuotedColumnName} = @{ParameterNumber}"));

		public string WholeWhereClauseWithParam => IfEnabled(FormattableString.Invariant($" WHERE {QuotedColumnName} = @{ParameterNumber}"));

		public string ColumnListAddition => IfEnabled(FormattableString.Invariant($", {QuotedColumnName}"));

		public string ParameterListAddition => IfEnabled(FormattableString.Invariant($", @{ParameterNumber}"));

		public SensitiveColumn(string quotedColumnName, object value = null, int parameterNumber = 0, SqlDbType parameterType = SqlDbType.Int)
		{
			QuotedColumnName = quotedColumnName;
			Value = value;
			ParameterNumber = parameterNumber;
			ParameterType = parameterType;
			enabled = !string.IsNullOrWhiteSpace(QuotedColumnName);
		}

		public void AddParameter(NavSqlCommand cmd)
		{
			if (enabled)
			{
				cmd.Command.Parameters.Add(FormattableString.Invariant($"@{ParameterNumber}"), ParameterType);
			}
		}

		public void AddValue(NavSqlCommand cmd)
		{
			if (enabled)
			{
				cmd.Command.Parameters[ParameterNumber].Value = Value;
			}
		}

		private string IfEnabled(string value)
		{
			if (!enabled)
			{
				return string.Empty;
			}
			return value;
		}
	}

	public const string TableName = "[dbo].[NAV App Tenant Operation]";

	private readonly NavTenant tenant;

	private readonly Lazy<SensitiveColumn> sharedDBColumn;

	public string TenantId => tenant.Id;

	private NavDatabase Database => tenant.Database;

	public DatabaseNavAppTenantOperationRepository(NavTenant tenant)
	{
		this.tenant = tenant ?? throw new ArgumentNullException("tenant");
		sharedDBColumn = new Lazy<SensitiveColumn>(() => (!Database.IsSharedTenantSchema) ? SensitiveColumn.Empty : new SensitiveColumn("\"$companyId\"", Database.GetCompanyIdForSystemTables(2000000200)));
	}

	public void DeleteOperationForId(Guid operationId)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(Database, TransactionType.Update);
		using (new SensitiveColumnScope(sharedDBColumn.Value, 1))
		{
			navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("NavAppTenantOperation-DeleteForId", Database), () => string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE [Operation ID] = @0{1}", "[dbo].[NAV App Tenant Operation]", sharedDBColumn.Value.AndClauseAddition), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
				sharedDBColumn.Value.AddParameter(cmd);
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = operationId;
				sharedDBColumn.Value.AddValue(cmd);
			}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		}
	}

	public IEnumerable<NavAppTenantOperationRecord> GetAll()
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(Database, TransactionType.Browse);
		using (new SensitiveColumnScope(sharedDBColumn.Value))
		{
			return navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("NavAppTenantOperation-GetAll", Database), () => string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'{0}') AND OBJECTPROPERTY(id, N'IsTable') = 1) SELECT * FROM {0}{1}", "[dbo].[NAV App Tenant Operation]", sharedDBColumn.Value.WholeWhereClauseWithParam), (NavSqlCommand cmd) =>
			{
				sharedDBColumn.Value.AddParameter(cmd);
			}, (NavSqlCommand cmd) =>
			{
				sharedDBColumn.Value.AddValue(cmd);
			}, (NavSqlCommand cmd) =>
			{
				List<NavAppTenantOperationRecord> list = new List<NavAppTenantOperationRecord>();
				using (cmd.ExecuteReader())
				{
					while (cmd.ReadNextRow())
					{
						list.Add(cmd.DeserializeOperationRecord());
					}
					return list;
				}
			});
		}
	}

	public NavAppTenantOperationRecord GetOperationForId(Guid operationId)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(Database, TransactionType.Browse);
		using (new SensitiveColumnScope(sharedDBColumn.Value, 1))
		{
			return navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("NavAppTenantOperation-GetForId", Database), () => string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'{0}') AND OBJECTPROPERTY(id, N'IsTable') = 1) SELECT * FROM {0} WHERE [Operation ID] = @0{1}", "[dbo].[NAV App Tenant Operation]", sharedDBColumn.Value.AndClauseAddition), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
				sharedDBColumn.Value.AddParameter(cmd);
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = operationId;
				sharedDBColumn.Value.AddValue(cmd);
			}, (NavSqlCommand cmd) =>
			{
				using (cmd.ExecuteReader())
				{
					return cmd.ReadNextRow() ? cmd.DeserializeOperationRecord() : null;
				}
			});
		}
	}

	public NavAppTenantOperationRecord GetOperationForResult(NavAppOperationResult result)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(Database, TransactionType.Browse);
		using (new SensitiveColumnScope(sharedDBColumn.Value, 2))
		{
			return navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("NavAppTenantOperation-GetForResult", Database), () => string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'{0}') AND OBJECTPROPERTY(id, N'IsTable') = 1) SELECT * FROM {0} WHERE [Operation Type] = @0 AND [Metadata Key] = @1{1}", "[dbo].[NAV App Tenant Operation]", sharedDBColumn.Value.AndClauseAddition), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.Int));
				cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.NVarChar, 250));
				sharedDBColumn.Value.AddParameter(cmd);
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = result.OperationType;
				cmd.Command.Parameters[1].Value = result.Key;
				sharedDBColumn.Value.AddValue(cmd);
			}, (NavSqlCommand cmd) =>
			{
				using (cmd.ExecuteReader())
				{
					return cmd.ReadNextRow() ? cmd.DeserializeOperationRecord() : null;
				}
			});
		}
	}

	public void UpdateOperationStatus(Guid operationId, NavAppTenantOperationFields.NavAppOperationStatus status, string statusDetails)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(Database, TransactionType.Update);
		using (new SensitiveColumnScope(sharedDBColumn.Value, 3))
		{
			navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("NavAppTenantOperation-UpdateStatus", Database), () => string.Format(CultureInfo.InvariantCulture, "UPDATE {0} SET [Status] = @1, [Details] = @2 WHERE [Operation ID] = @0{1}", "[dbo].[NAV App Tenant Operation]", sharedDBColumn.Value.AndClauseAddition), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
				cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.Int));
				cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Image, int.MaxValue));
				sharedDBColumn.Value.AddParameter(cmd);
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = operationId;
				cmd.Command.Parameters[1].Value = status;
				cmd.Command.Parameters[2].Value = statusDetails.SerializeBlobString();
				sharedDBColumn.Value.AddValue(cmd);
			}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		}
	}

	public Guid UpdateOrInsert(NavAppOperationRequestReference reference)
	{
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(Database, TransactionType.Update))
		{
			using (new SensitiveColumnScope(sharedDBColumn.Value, 9))
			{
				navSqlConnectionScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("NavAppTenantOperation-InsertOrUpdate", Database), () => string.Format(CultureInfo.InvariantCulture, "UPDATE {0} SET [Started On] = @1, [Status] = @3, [Details] = @4, [Metadata] = @6 WHERE [Operation Type] = @2 AND [Metadata Key] = @7{1} IF @@ROWCOUNT = 0 INSERT INTO {0} ([Operation ID], [Started On], [Operation Type], [Status], [Details], [Metadata Version], [Metadata], [Metadata Key], [Description]{2}) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8{3});", "[dbo].[NAV App Tenant Operation]", sharedDBColumn.Value.AndClauseAddition, sharedDBColumn.Value.ColumnListAddition, sharedDBColumn.Value.ParameterListAddition), (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.UniqueIdentifier));
					cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.DateTime));
					cmd.Command.Parameters.Add(new SqlParameter("@2", SqlDbType.Int));
					cmd.Command.Parameters.Add(new SqlParameter("@3", SqlDbType.Int));
					cmd.Command.Parameters.Add(new SqlParameter("@4", SqlDbType.Image, int.MaxValue));
					cmd.Command.Parameters.Add(new SqlParameter("@5", SqlDbType.Int));
					cmd.Command.Parameters.Add(new SqlParameter("@6", SqlDbType.Image, int.MaxValue));
					cmd.Command.Parameters.Add(new SqlParameter("@7", SqlDbType.NVarChar, 250));
					cmd.Command.Parameters.Add(new SqlParameter("@8", SqlDbType.NVarChar, 250));
					sharedDBColumn.Value.AddParameter(cmd);
				}, (NavSqlCommand cmd) =>
				{
					cmd.Command.Parameters[0].Value = Guid.NewGuid();
					cmd.Command.Parameters[1].Value = NavDateTime.Now.ToDateTime();
					cmd.Command.Parameters[2].Value = reference.OperationType;
					cmd.Command.Parameters[3].Value = reference.Status;
					cmd.Command.Parameters[4].Value = reference.Details.SerializeBlobString();
					cmd.Command.Parameters[5].Value = NavAppOperationMetadataVersion.Initial;
					cmd.Command.Parameters[6].Value = reference.SerializeMetadata();
					cmd.Command.Parameters[7].Value = reference.Metadata.Key;
					cmd.Command.Parameters[8].Value = reference.Description;
					sharedDBColumn.Value.AddValue(cmd);
				}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
			}
		}
		using NavSqlConnectionScope navSqlConnectionScope2 = NavSqlConnectionScope.CreateForceNewConnection(Database, TransactionType.Browse);
		using (new SensitiveColumnScope(sharedDBColumn.Value, 2))
		{
			return navSqlConnectionScope2.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("NavAppTenantOperation-GetIdForTypeAndKey", Database), () => string.Format(CultureInfo.InvariantCulture, "SELECT [Operation ID] from {0} WHERE [Operation Type] = @0 AND [Metadata Key] = @1{1}", "[dbo].[NAV App Tenant Operation]", sharedDBColumn.Value.AndClauseAddition), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.Int));
				cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.NVarChar, 250));
				sharedDBColumn.Value.AddParameter(cmd);
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = reference.OperationType;
				cmd.Command.Parameters[1].Value = reference.Metadata.Key;
				sharedDBColumn.Value.AddValue(cmd);
			}, (NavSqlCommand cmd) => cmd.ExecuteScalar<Guid?>()) ?? throw new NavAppExternalDeployException("Unexpected null in return value");
		}
	}
}
