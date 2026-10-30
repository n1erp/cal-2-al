using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavSqlSystemScheduledTask
{
	internal enum FieldNo
	{
		TaskId,
		ActionId,
		Parameters,
		TenantId,
		NotBefore
	}

	internal const string TableName = "[dbo].[$ndo$systemscheduledtask]";

	internal const string PrimaryKeyName = "PK$ndo$systemscheduledtask";

	internal static readonly IReadOnlyDictionary<FieldNo, NavSqlFieldDefinition> Fields = new Dictionary<FieldNo, NavSqlFieldDefinition>
	{
		{
			FieldNo.TaskId,
			new NavSqlFieldDefinition("taskid", NavNclType.NavGuid, 0)
		},
		{
			FieldNo.ActionId,
			new NavSqlFieldDefinition("actionid", NavNclType.NavInteger, 0)
		},
		{
			FieldNo.Parameters,
			new NavSqlFieldDefinition("parameters", NavNclType.NavText, int.MaxValue)
		},
		{
			FieldNo.TenantId,
			new NavSqlFieldDefinition("tenantid", NavNclType.NavText, 128)
		},
		{
			FieldNo.NotBefore,
			new NavSqlFieldDefinition("notbefore", NavNclType.NavDateTime, 0)
		}
	};

	internal static void Insert(NavSqlConnectionScope conScope, NavSystemScheduledTask record)
	{
		conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("NavSqlSystemScheduledTask_Insert", conScope.Connection.Database), () =>
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("INSERT INTO ").Append("[dbo].[$ndo$systemscheduledtask]");
			stringBuilder.Append('(');
			foreach (KeyValuePair<FieldNo, NavSqlFieldDefinition> field in Fields)
			{
				NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, field.Value.Name);
				stringBuilder.Append(',');
			}
			stringBuilder.Length--;
			stringBuilder.Append(") VALUES (");
			int count = Fields.Count;
			for (int i = 0; i < count; i++)
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
			foreach (KeyValuePair<FieldNo, NavSqlFieldDefinition> field2 in Fields)
			{
				object fieldValueFromRecord = GetFieldValueFromRecord(record, field2.Key);
				string parameterName = NavSqlStatementHelper.ParameterName(num++);
				SqlParameter value = NavSqlStatementHelper.CreateSqlParameter(parameterName, field2.Value.Type, SqlDataType.Default, field2.Value.Length, fieldValueFromRecord, isWildcardValue: false, isFilterValue: false);
				cmd.Command.Parameters.Add(value);
			}
		}, (NavSqlCommand cmd) =>
		{
			int num = 0;
			foreach (KeyValuePair<FieldNo, NavSqlFieldDefinition> field3 in Fields)
			{
				object fieldValueFromRecord = GetFieldValueFromRecord(record, field3.Key);
				cmd.Command.Parameters[num++].Value = fieldValueFromRecord;
			}
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
	}

	internal static void Modify(NavSqlConnectionScope conScope, List<FieldNo> fieldsToModify, NavSystemScheduledTask systemScheduledTask)
	{
		if (fieldsToModify == null || fieldsToModify.Count == 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("UPDATE ").Append("[dbo].[$ndo$systemscheduledtask]");
		stringBuilder.Append(" SET ");
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		int parameterIndex = 0;
		foreach (FieldNo item in fieldsToModify)
		{
			object fieldValueFromRecord = GetFieldValueFromRecord(systemScheduledTask, item);
			NavSqlFieldDefinition navSqlFieldDefinition = Fields[item];
			string text = NavSqlStatementHelper.ParameterName(parameterIndex++);
			SqlParameter value = NavSqlStatementHelper.CreateSqlParameter(text, navSqlFieldDefinition.Type, SqlDataType.Default, navSqlFieldDefinition.Length, fieldValueFromRecord, isWildcardValue: false, isFilterValue: false);
			navSqlCommand.Command.Parameters.Add(value);
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, navSqlFieldDefinition.Name);
			stringBuilder.Append(" = ").Append(text);
			stringBuilder.Append(',');
		}
		stringBuilder.Length--;
		AppendWhereClause(stringBuilder, parameterIndex, FieldNo.TaskId);
		AddParameter(navSqlCommand, FieldNo.TaskId, systemScheduledTask.TaskId, ref parameterIndex);
		navSqlCommand.ExecuteNonQuery(stringBuilder.ToString());
	}

	internal static bool Delete(NavSqlConnectionScope conScope, Guid taskId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001GZ", Category.TaskScheduling, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "Deleting system scheduled task record {0}.", taskId);
		return conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("NavSqlSystemScheduledTask_Delete", conScope.Connection.Database), () =>
		{
			int parameterIndex = 0;
			StringBuilder stringBuilder = new StringBuilder("DELETE FROM ");
			stringBuilder.Append("[dbo].[$ndo$systemscheduledtask]");
			AppendWhereClause(stringBuilder, parameterIndex, FieldNo.TaskId);
			stringBuilder.Append(" SELECT @@ROWCOUNT");
			return stringBuilder.ToString();
		}, (NavSqlCommand cmd) =>
		{
			int parameterIndex = 0;
			AddParameter(cmd, FieldNo.TaskId, taskId, ref parameterIndex);
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = taskId;
		}, (NavSqlCommand cmd) => cmd.ExecuteScalar<int>() == 1);
	}

	internal static NavSystemScheduledTask GetSystemScheduledTask(NavSqlConnectionScope conScope, Guid taskId)
	{
		return conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("NavSqlSystemScheduledTask_Get", conScope.Connection.Database), () =>
		{
			StringBuilder stringBuilder = new StringBuilder("SELECT TOP 1 * FROM ");
			stringBuilder.Append("[dbo].[$ndo$systemscheduledtask]");
			int parameterIndex = 0;
			AppendWhereClause(stringBuilder, parameterIndex, FieldNo.TaskId);
			return stringBuilder.ToString();
		}, (NavSqlCommand cmd) =>
		{
			int parameterIndex = 0;
			AddParameter(cmd, FieldNo.TaskId, taskId, ref parameterIndex);
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = taskId;
		}, (NavSqlCommand cmd) =>
		{
			using (cmd.ExecuteReader())
			{
				if (cmd.ReadNextRow())
				{
					return new NavSystemScheduledTask(cmd.Reader.GetGuid(0), (SystemTaskActionId)cmd.Reader.GetInt32(1), cmd.Reader.GetString(2), new TenantId(cmd.Reader.GetString(3)), DateTime.SpecifyKind(cmd.Reader.GetDateTime(4), DateTimeKind.Utc));
				}
			}
			return (NavSystemScheduledTask)null;
		});
	}

	internal static IEnumerable<NavSystemScheduledTask> GetSystemScheduledTasksInTenant(NavSqlConnectionScope conScope, string tenantId)
	{
		return conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("NavSqlSystemScheduledTask_GetAllInTenant", conScope.Connection.Database), () =>
		{
			StringBuilder stringBuilder = new StringBuilder("SELECT * FROM ");
			stringBuilder.Append("[dbo].[$ndo$systemscheduledtask]");
			int parameterIndex = 0;
			AppendWhereClause(stringBuilder, parameterIndex, FieldNo.TenantId);
			return stringBuilder.ToString();
		}, (NavSqlCommand cmd) =>
		{
			int parameterIndex = 0;
			AddParameter(cmd, FieldNo.TenantId, tenantId, ref parameterIndex);
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = tenantId;
		}, (NavSqlCommand cmd) =>
		{
			List<NavSystemScheduledTask> list = new List<NavSystemScheduledTask>();
			using (cmd.ExecuteReader())
			{
				while (cmd.ReadNextRow())
				{
					list.Add(new NavSystemScheduledTask(cmd.Reader.GetGuid(0), (SystemTaskActionId)cmd.Reader.GetInt32(1), cmd.Reader.GetString(2), new TenantId(cmd.Reader.GetString(3)), DateTime.SpecifyKind(cmd.Reader.GetDateTime(4), DateTimeKind.Utc)));
				}
				return list;
			}
		});
	}

	private static object GetFieldValueFromRecord(NavSystemScheduledTask record, FieldNo fieldNo)
	{
		return fieldNo switch
		{
			FieldNo.TaskId => (object)record.TaskId, 
			FieldNo.ActionId => record.SystemTaskActionId, 
			FieldNo.Parameters => record.Parameters, 
			FieldNo.TenantId => record.TenantId, 
			FieldNo.NotBefore => record.NotBefore, 
			_ => throw new NotImplementedException(), 
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

	private static void AppendWhereClause(StringBuilder commandText, int parameterIndex, FieldNo fieldNo)
	{
		commandText.Append(" WHERE ");
		NavSqlStatementHelper.AppendQuotedIdentifier(commandText, Fields[fieldNo].Name);
		commandText.AppendFormat(CultureInfo.InvariantCulture, " = {0} ", NavSqlStatementHelper.ParameterName(parameterIndex));
	}
}
