using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavSqlCloudReadyReplicationStatus
{
	internal enum FieldNo
	{
		Timestamp,
		TableName,
		CompanyName,
		TableId,
		SyncedVersion,
		Blocked,
		SharedDbCompanyId
	}

	internal const string TableName = "[dbo].[Intelligent Cloud Status]";

	internal static readonly IReadOnlyDictionary<FieldNo, NavSqlFieldDefinition> Fields = new Dictionary<FieldNo, NavSqlFieldDefinition>
	{
		{
			FieldNo.TableName,
			new NavSqlFieldDefinition("Table Name", NavNclType.NavText, 250)
		},
		{
			FieldNo.CompanyName,
			new NavSqlFieldDefinition("Company Name", NavNclType.NavText, 30)
		},
		{
			FieldNo.TableId,
			new NavSqlFieldDefinition("Table Id", NavNclType.NavInteger, 0)
		},
		{
			FieldNo.SyncedVersion,
			new NavSqlFieldDefinition("Synced Version", NavNclType.NavBigInteger, 0)
		},
		{
			FieldNo.Blocked,
			new NavSqlFieldDefinition("Blocked", NavNclType.NavBoolean, 0)
		},
		{
			FieldNo.SharedDbCompanyId,
			new NavSqlFieldDefinition("$companyId", NavNclType.NavInteger, 0)
		}
	};

	internal static void Insert(NavSqlConnectionScope conScope, CloudReadyReplicationStatus record)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000029R", Category.CloudReady, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Inserting a record in table:{0} to track TableId:{1}; Name:{2} for cloud ready", "[dbo].[Intelligent Cloud Status]", record.TableId, record.TableName);
		if (conScope == null || GetCloudReadyReplicationStatus(conScope, record.TableName, record.CompanyName) != null)
		{
			return;
		}
		IEnumerable<KeyValuePair<FieldNo, NavSqlFieldDefinition>> enumerable;
		if (!conScope.Connection.Database.IsSharedTenantSchema)
		{
			enumerable = Fields.Take(Fields.Count - 1);
		}
		else
		{
			IEnumerable<KeyValuePair<FieldNo, NavSqlFieldDefinition>> fields = Fields;
			enumerable = fields;
		}
		IEnumerable<KeyValuePair<FieldNo, NavSqlFieldDefinition>> fieldsList = enumerable;
		conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("NavSqlCloudReadyReplicationStatus_Insert", conScope.Connection.Database), () =>
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("INSERT INTO ").Append("[dbo].[Intelligent Cloud Status]").Append('(');
			foreach (KeyValuePair<FieldNo, NavSqlFieldDefinition> item in fieldsList)
			{
				NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, item.Value.Name);
				stringBuilder.Append(',');
			}
			stringBuilder.Length--;
			stringBuilder.Append(") VALUES (");
			int num = fieldsList.Count();
			for (int i = 0; i < num; i++)
			{
				string value = NavSqlStatementHelper.ParameterName(i);
				stringBuilder.Append(value);
				stringBuilder.Append(',');
			}
			stringBuilder.Length--;
			stringBuilder.Append(')');
			return stringBuilder.ToString();
		}, (NavSqlCommand cmd) =>
		{
			int num = 0;
			foreach (KeyValuePair<FieldNo, NavSqlFieldDefinition> item2 in fieldsList)
			{
				object fieldValueFromRecord = GetFieldValueFromRecord(record, item2.Key);
				string parameterName = NavSqlStatementHelper.ParameterName(num++);
				SqlParameter value = NavSqlStatementHelper.CreateSqlParameter(parameterName, item2.Value.Type, SqlDataType.Default, item2.Value.Length, fieldValueFromRecord, isWildcardValue: false, isFilterValue: false);
				cmd.Command.Parameters.Add(value);
			}
		}, (NavSqlCommand cmd) =>
		{
			int num = 0;
			foreach (KeyValuePair<FieldNo, NavSqlFieldDefinition> item3 in fieldsList)
			{
				object fieldValueFromRecord = GetFieldValueFromRecord(record, item3.Key);
				cmd.Command.Parameters[num++].Value = fieldValueFromRecord;
			}
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
	}

	internal static void Modify(NavSqlConnectionScope conScope, List<FieldNo> fieldsToModify, CloudReadyReplicationStatus record)
	{
		if (conScope == null || fieldsToModify == null || fieldsToModify.Count == 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("UPDATE ").Append("[dbo].[Intelligent Cloud Status]");
		stringBuilder.Append(" SET ");
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		int parameterIndex = 0;
		foreach (FieldNo item in fieldsToModify)
		{
			object fieldValueFromRecord = GetFieldValueFromRecord(record, item);
			NavSqlFieldDefinition navSqlFieldDefinition = Fields[item];
			string text = NavSqlStatementHelper.ParameterName(parameterIndex++);
			SqlParameter value = NavSqlStatementHelper.CreateSqlParameter(text, navSqlFieldDefinition.Type, SqlDataType.Default, navSqlFieldDefinition.Length, fieldValueFromRecord, isWildcardValue: false, isFilterValue: false);
			navSqlCommand.Command.Parameters.Add(value);
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, navSqlFieldDefinition.Name);
			stringBuilder.Append(" = ").Append(text);
			stringBuilder.Append(',');
		}
		stringBuilder.Length--;
		AppendWhereClause(stringBuilder, parameterIndex, FieldNo.TableName, isFirstCondition: true);
		AddParameter(navSqlCommand, FieldNo.TableName, record.TableName, ref parameterIndex);
		AppendWhereClause(stringBuilder, parameterIndex, FieldNo.CompanyName, isFirstCondition: false);
		AddParameter(navSqlCommand, FieldNo.CompanyName, record.CompanyName, ref parameterIndex);
		if (conScope.Connection.Database.IsSharedTenantSchema)
		{
			AppendWhereClause(stringBuilder, parameterIndex, FieldNo.SharedDbCompanyId, isFirstCondition: false);
			AddParameter(navSqlCommand, FieldNo.SharedDbCompanyId, record.CompanyId, ref parameterIndex);
		}
		navSqlCommand.ExecuteNonQuery(stringBuilder.ToString());
	}

	internal static int ModifyAll(NavSqlConnectionScope conScope, string companyName, FieldNo fieldToModify, object value)
	{
		if (conScope != null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("UPDATE ").Append("[dbo].[Intelligent Cloud Status]");
			stringBuilder.Append(" SET ");
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
			int parameterIndex = 0;
			NavSqlFieldDefinition navSqlFieldDefinition = Fields[fieldToModify];
			string text = NavSqlStatementHelper.ParameterName(parameterIndex++);
			SqlParameter value2 = NavSqlStatementHelper.CreateSqlParameter(text, navSqlFieldDefinition.Type, SqlDataType.Default, navSqlFieldDefinition.Length, value, isWildcardValue: false, isFilterValue: false);
			navSqlCommand.Command.Parameters.Add(value2);
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, navSqlFieldDefinition.Name);
			stringBuilder.Append(" = ").Append(text);
			AppendWhereClause(stringBuilder, parameterIndex, FieldNo.CompanyName, isFirstCondition: true);
			AddParameter(navSqlCommand, FieldNo.CompanyName, companyName, ref parameterIndex);
			if (conScope.Connection.Database.IsSharedTenantSchema && conScope.Connection.Database.TryGetCompanyNameToken(companyName, out var token))
			{
				AppendWhereClause(stringBuilder, parameterIndex, FieldNo.SharedDbCompanyId, isFirstCondition: false);
				AddParameter(navSqlCommand, FieldNo.SharedDbCompanyId, token, ref parameterIndex);
			}
			return navSqlCommand.ExecuteNonQuery(stringBuilder.ToString());
		}
		return 0;
	}

	internal static bool Delete(NavSqlConnectionScope conScope, string companyName, string tableName)
	{
		if (conScope != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00007KU", Category.CloudReady, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Deleting Cloud Ready Replication Status record {0}.", tableName);
			bool isShared = conScope.Connection.Database.IsSharedTenantSchema;
			int? companyNameToken = null;
			if (isShared && conScope.Connection.Database.TryGetCompanyNameToken(companyName, out var token))
			{
				companyNameToken = token;
			}
			return conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("NavSqlCloudReadyReplicationStatus_Delete", conScope.Connection.Database), () =>
			{
				int parameterIndex = 0;
				StringBuilder stringBuilder = new StringBuilder("DELETE FROM ");
				stringBuilder.Append("[dbo].[Intelligent Cloud Status]");
				AppendWhereClause(stringBuilder, parameterIndex++, FieldNo.TableName, isFirstCondition: true);
				AppendWhereClause(stringBuilder, parameterIndex++, FieldNo.CompanyName, isFirstCondition: false);
				if (isShared && companyNameToken.HasValue)
				{
					AppendWhereClause(stringBuilder, parameterIndex, FieldNo.SharedDbCompanyId, isFirstCondition: false);
				}
				stringBuilder.Append(" SELECT @@ROWCOUNT");
				return stringBuilder.ToString();
			}, (NavSqlCommand cmd) =>
			{
				int parameterIndex = 0;
				AddParameter(cmd, FieldNo.TableName, tableName, ref parameterIndex);
				AddParameter(cmd, FieldNo.CompanyName, companyName, ref parameterIndex);
				if (isShared && companyNameToken.HasValue)
				{
					AddParameter(cmd, FieldNo.SharedDbCompanyId, companyNameToken, ref parameterIndex);
				}
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = tableName;
				cmd.Command.Parameters[1].Value = companyName;
				if (isShared && companyNameToken.HasValue)
				{
					cmd.Command.Parameters[2].Value = companyNameToken;
				}
			}, (NavSqlCommand cmd) => cmd.ExecuteScalar<int>() == 1);
		}
		return false;
	}

	internal static CloudReadyReplicationStatus GetCloudReadyReplicationStatus(NavSqlConnectionScope conScope, string tableName, string companyName)
	{
		if (conScope != null)
		{
			bool isShared = conScope.Connection.Database.IsSharedTenantSchema;
			int? companyNameToken = null;
			if (isShared && conScope.Connection.Database.TryGetCompanyNameToken(companyName, out var token))
			{
				companyNameToken = token;
			}
			return conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("NavSqlCloudReadyReplicationStatus_Get", conScope.Connection.Database), () =>
			{
				StringBuilder stringBuilder = new StringBuilder("SELECT TOP 1 ");
				AppendSelectFieldList(stringBuilder, conScope.Connection.Database.IsSharedTenantSchema);
				stringBuilder.Append(" FROM ");
				stringBuilder.Append("[dbo].[Intelligent Cloud Status]");
				stringBuilder.Append(" WITH(NOLOCK) ");
				int parameterIndex = 0;
				AppendWhereClause(stringBuilder, parameterIndex++, FieldNo.TableName, isFirstCondition: true);
				AppendWhereClause(stringBuilder, parameterIndex++, FieldNo.CompanyName, isFirstCondition: false);
				if (isShared && companyNameToken.HasValue)
				{
					AppendWhereClause(stringBuilder, parameterIndex, FieldNo.SharedDbCompanyId, isFirstCondition: false);
				}
				return stringBuilder.ToString();
			}, (NavSqlCommand cmd) =>
			{
				int parameterIndex = 0;
				AddParameter(cmd, FieldNo.TableName, tableName, ref parameterIndex);
				AddParameter(cmd, FieldNo.CompanyName, companyName, ref parameterIndex);
				if (isShared && companyNameToken.HasValue)
				{
					AddParameter(cmd, FieldNo.SharedDbCompanyId, companyNameToken, ref parameterIndex);
				}
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = tableName;
				cmd.Command.Parameters[1].Value = companyName;
				if (isShared && companyNameToken.HasValue)
				{
					cmd.Command.Parameters[2].Value = companyNameToken;
				}
			}, (NavSqlCommand cmd) =>
			{
				using (cmd.ExecuteReader())
				{
					if (cmd.ReadNextRow())
					{
						return ReadCloudStatusObject(isShared, cmd);
					}
				}
				return (CloudReadyReplicationStatus)null;
			});
		}
		return null;
	}

	internal static IEnumerable<CloudReadyReplicationStatus> GetCloudReadyReplicationStatusCollection(NavSqlConnectionScope conScope)
	{
		return conScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("NavSqlCloudReadyReplicationStatus_GetAll", conScope.Connection.Database), () =>
		{
			StringBuilder stringBuilder = new StringBuilder("SELECT ");
			AppendSelectFieldList(stringBuilder, conScope.Connection.Database.IsSharedTenantSchema);
			stringBuilder.Append(" FROM ");
			stringBuilder.Append("[dbo].[Intelligent Cloud Status]");
			stringBuilder.Append(" WITH(NOLOCK) ");
			return stringBuilder.ToString();
		}, (NavSqlCommand cmd) =>
		{
			List<CloudReadyReplicationStatus> list = new List<CloudReadyReplicationStatus>();
			using (cmd.ExecuteReader())
			{
				while (cmd.ReadNextRow())
				{
					CloudReadyReplicationStatus item = ReadCloudStatusObject(conScope.Connection.Database.IsSharedTenantSchema, cmd);
					list.Add(item);
				}
				return list;
			}
		});
	}

	internal static int UpdateBlockedStatus(NavSqlConnectionScope conScope, string tableName, bool value)
	{
		if (conScope != null)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00007AU", Category.CloudReady, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Updating blocked status Cloud Ready Replication Status record {0}.", tableName);
			return conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("NavSqlCloudReadyReplicationStatus_UpdateBlocked", conScope.Connection.Database), () => string.Format(CultureInfo.InvariantCulture, "UPDATE {0} SET [Blocked] = @0 WHERE [Table Name] = @1 SELECT @@ROWCOUNT", "[dbo].[Intelligent Cloud Status]"), (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add(new SqlParameter("@0", SqlDbType.TinyInt));
				cmd.Command.Parameters.Add(new SqlParameter("@1", SqlDbType.NVarChar, 250));
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = value;
				cmd.Command.Parameters[1].Value = tableName;
			}, (NavSqlCommand cmd) => cmd.ExecuteScalar<int>());
		}
		return 0;
	}

	private static CloudReadyReplicationStatus ReadCloudStatusObject(bool isSharedSchema, NavSqlCommand cmd)
	{
		return new CloudReadyReplicationStatus
		{
			TableName = cmd.Reader.GetString(1),
			CompanyName = cmd.Reader.GetString(2),
			TableId = cmd.Reader.GetInt32(3),
			SyncedVersion = cmd.Reader.GetInt64(4),
			Blocked = Convert.ToBoolean(cmd.Reader.GetByte(5)),
			CompanyId = (isSharedSchema ? cmd.Reader.GetInt32(6) : 0)
		};
	}

	private static object GetFieldValueFromRecord(CloudReadyReplicationStatus record, FieldNo fieldNo)
	{
		return fieldNo switch
		{
			FieldNo.TableName => (object)record.TableName, 
			FieldNo.SyncedVersion => record.SyncedVersion, 
			FieldNo.CompanyName => record.CompanyName ?? string.Empty, 
			FieldNo.Blocked => record.Blocked, 
			FieldNo.TableId => record.TableId, 
			FieldNo.SharedDbCompanyId => record.CompanyId, 
			_ => throw new ArgumentException("Illegal fieldNo specified"), 
		};
	}

	private static void AddParameter(NavSqlCommand command, FieldNo fieldNo, object value, ref int parameterIndex)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		NavSqlFieldDefinition navSqlFieldDefinition = Fields[fieldNo];
		SqlParameter value2 = NavSqlStatementHelper.CreateSqlParameter(NavSqlStatementHelper.ParameterName(parameterIndex++), navSqlFieldDefinition.Type, SqlDataType.Default, navSqlFieldDefinition.Length, value, isWildcardValue: false, isFilterValue: false);
		command.Command.Parameters.Add(value2);
	}

	internal static void AppendWhereClause(StringBuilder commandText, int parameterIndex, FieldNo fieldNo, bool isFirstCondition)
	{
		if (isFirstCondition)
		{
			commandText.Append(" WHERE ");
		}
		else
		{
			commandText.Append(" AND ");
		}
		NavSqlStatementHelper.AppendQuotedIdentifier(commandText, Fields[fieldNo].Name);
		commandText.AppendFormat(CultureInfo.InvariantCulture, " = {0} ", NavSqlStatementHelper.ParameterName(parameterIndex));
	}

	private static void AppendSelectFieldList(StringBuilder commandText, bool sharedSchema)
	{
		commandText.Append("timestamp, ");
		foreach (KeyValuePair<FieldNo, NavSqlFieldDefinition> field in Fields)
		{
			if (field.Key == FieldNo.SharedDbCompanyId && !sharedSchema)
			{
				commandText.Append("-1");
			}
			else
			{
				NavSqlStatementHelper.AppendQuotedIdentifier(commandText, field.Value.Name);
			}
			commandText.Append(" , ");
		}
		commandText.Remove(commandText.Length - 3, 2);
	}
}
