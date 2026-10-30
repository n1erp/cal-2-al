using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class NavSqlTenantCompanies
{
	internal enum FieldNo
	{
		CompanyId,
		TenantLocalId,
		CompanyName,
		State
	}

	public const int MaxNumberCompanies = 1000;

	public const int NumberOfReservedCompanies = 9;

	internal const string TableName = "[dbo].[$ndo$tenantcompany]";

	internal const string PrimaryKeyName = "PK$ndo$tenantcompany";

	internal static readonly IReadOnlyDictionary<FieldNo, NavSqlFieldDefinition> Fields = new Dictionary<FieldNo, NavSqlFieldDefinition>
	{
		{
			FieldNo.CompanyId,
			new NavSqlFieldDefinition("$companyid", NavNclType.NavInteger, 0)
		},
		{
			FieldNo.TenantLocalId,
			new NavSqlFieldDefinition("$tenantlocalid", NavNclType.NavInteger, 0)
		},
		{
			FieldNo.CompanyName,
			new NavSqlFieldDefinition("companyname", NavNclType.NavText, 30)
		},
		{
			FieldNo.State,
			new NavSqlFieldDefinition("companystate", NavNclType.NavInteger, 0)
		}
	};

	internal static readonly IReadOnlyList<FieldNo> BaseSchemaFieldNumbers = new List<FieldNo>
	{
		FieldNo.CompanyId,
		FieldNo.TenantLocalId,
		FieldNo.CompanyName
	};

	private readonly NavDatabase tenantDatabase;

	private readonly int tenantLocalId;

	private const string GetDeletedCompanyIdStatementName = "GetDeletedCompanyIdStatement";

	private const string GetDeletedCompanyIdStatement = "\r\n           WITH digits AS(\r\n               SELECT 0 AS N \r\n               UNION ALL SELECT 1 \r\n               UNION ALL SELECT 2 \r\n               UNION ALL SELECT 3 \r\n               UNION ALL SELECT 4 \r\n               UNION ALL SELECT 5 \r\n               UNION ALL SELECT 6 \r\n               UNION ALL SELECT 7 \r\n               UNION ALL SELECT 8 \r\n               UNION ALL SELECT 9 \r\n           )\r\n           SELECT TOP 1 x \r\n           FROM \r\n           (SELECT a.N + b.N * 10 + c.N * 100 + 1 + @0 as x \r\n              FROM(SELECT N FROM digits) a, \r\n                  (SELECT N FROM digits) b, \r\n                  (SELECT N FROM digits) c) t \r\n           LEFT JOIN {0} ON x = [{1}]\r\n           WHERE x < @1 AND [{1}] IS NULL\r\n           ORDER BY x";

	private const string ReadMaxCompanyIdStatement = "SELECT MAX([{0}]) FROM {1} WHERE [{2}] = @0";

	internal NavSqlTenantCompanies(NavDatabase tenantDatabase)
	{
		if (tenantDatabase == null)
		{
			throw new ArgumentNullException("tenantDatabase");
		}
		if (tenantDatabase.DatabaseType != NavDatabaseType.Tenant)
		{
			throw new ArgumentException("Given database is not a tenant database");
		}
		this.tenantDatabase = tenantDatabase;
		if (!this.tenantDatabase.TenantProperties.TenantLocalId.HasValue)
		{
			throw new InvalidOperationException("Tenant numeric id must be set");
		}
		tenantLocalId = this.tenantDatabase.TenantProperties.TenantLocalId.Value;
	}

	internal CompanyId GetCompanyIdForSharedCompany()
	{
		return GetCompanyId(string.Empty);
	}

	internal CompanyId GetCompanyId(string companyName)
	{
		if (companyName == null)
		{
			throw new ArgumentNullException("companyName");
		}
		return new CompanyId(GetFieldValue<int>(companyName, FieldNo.CompanyId));
	}

	internal CompanyId InsertSharedCompany()
	{
		return Insert(string.Empty);
	}

	internal CompanyId Insert(string companyName)
	{
		if (companyName == null)
		{
			throw new ArgumentNullException("companyName");
		}
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(tenantDatabase, TransactionType.Update);
		int num = GenerateNewCompanyId(companyName, conScope);
		Insert(num, companyName, conScope);
		return new CompanyId(num);
	}

	internal void DeleteSharedCompany()
	{
		Delete(string.Empty);
	}

	internal void Delete(string companyName)
	{
		if (companyName == null)
		{
			throw new ArgumentNullException("companyName");
		}
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, TransactionType.Update))
		{
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("DELETE FROM ").Append("[dbo].[$ndo$tenantcompany]");
			stringBuilder.Append(" WHERE ");
			AppendTenantIdCompanyNameToWhereClause(stringBuilder);
			navSqlCommand.SetCommandText(stringBuilder.ToString());
			AddTenantIdCompanyNameParameters(navSqlCommand, tenantLocalId, companyName);
			navSqlCommand.ExecuteScalar();
		}
		tenantDatabase.ResetCompanyIdCache();
	}

	internal void SetState(string companyName, CompanyState companyState)
	{
		ModifyField(companyName, FieldNo.State, companyState);
	}

	internal CompanyState GetState(string companyName)
	{
		if (companyName == null)
		{
			throw new ArgumentNullException("companyName");
		}
		return GetFieldValue<CompanyState>(companyName, FieldNo.State);
	}

	internal void Rename(string oldCompanyName, string newCompanyName)
	{
		if (oldCompanyName == null)
		{
			throw new ArgumentNullException("oldCompanyName");
		}
		if (newCompanyName == null)
		{
			throw new ArgumentNullException("newCompanyName");
		}
		ModifyField(oldCompanyName, FieldNo.CompanyName, newCompanyName);
	}

	public bool CompanyExists(string companyName)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, TransactionType.UpdateNoLocks);
		return navSqlConnectionScope.Connection.ExecuteCommandAndCacheOnSuccess(new NavSqlStringBasedCommandCacheKey("TenantCompanyCompanyExists", tenantDatabase), () =>
		{
			StringBuilder stringBuilder = new StringBuilder("SELECT COUNT(0)");
			stringBuilder.Append(" FROM ");
			stringBuilder.Append("[dbo].[$ndo$tenantcompany]");
			stringBuilder.Append(" WHERE ");
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, Fields[FieldNo.TenantLocalId].Name);
			stringBuilder.Append(" = @0 AND (");
			if (!tenantDatabase.IsSharedTenantSchema)
			{
				NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, Fields[FieldNo.CompanyName].Name);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " COLLATE {0} = @1 OR ", tenantDatabase.SqlServerProperties.ServerCollation);
			}
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, Fields[FieldNo.CompanyName].Name);
			stringBuilder.Append(" = @1)");
			return stringBuilder.ToString();
		}, (NavSqlCommand commandToExecute) =>
		{
			if (commandToExecute.Command.Parameters.Count == 0)
			{
				AddTenantIdCompanyNameParameters(commandToExecute, tenantLocalId, companyName);
			}
			else
			{
				commandToExecute.Command.Parameters[0].Value = tenantLocalId;
				commandToExecute.Command.Parameters[1].Value = companyName;
			}
		}, (NavSqlCommand commandToExecute) =>
		{
			int num = commandToExecute.ExecuteScalar<int>();
			return num > 0;
		});
	}

	internal static List<CompanyId> ReadAllNormalCompanyIdsFromAllTenants(NavDatabase database)
	{
		return ReadAllCompanyIdsFromAllTenants(database, (StringBuilder commandText) =>
		{
			commandText.Append(FormattableString.Invariant($"WHERE {Fields[FieldNo.CompanyName].Name} <> ''"));
		});
	}

	internal static List<CompanyId> ReadAllSharedCompanyIdsFromAllTenants(NavDatabase database)
	{
		return ReadAllCompanyIdsFromAllTenants(database, (StringBuilder commandText) =>
		{
			commandText.Append(FormattableString.Invariant($"WHERE {Fields[FieldNo.CompanyName].Name} = ''"));
		});
	}

	private static List<CompanyId> ReadAllCompanyIdsFromAllTenants(NavDatabase database, Action<StringBuilder> whereClauseBuilderAction)
	{
		StringBuilder stringBuilder = new StringBuilder(FormattableString.Invariant(FormattableStringFactory.Create("SELECT [{0}] FROM {1} ", Fields[FieldNo.CompanyId].Name, "[dbo].[$ndo$tenantcompany]")));
		whereClauseBuilderAction?.Invoke(stringBuilder);
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.Update);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		List<CompanyId> list = new List<CompanyId>();
		using (navSqlCommand.ExecuteReader())
		{
			while (navSqlCommand.ReadNextRow())
			{
				list.Add(new CompanyId(navSqlCommand.Reader.GetInt32(0)));
			}
		}
		return list;
	}

	internal int CountAll()
	{
		StringBuilder stringBuilder = new StringBuilder(FormattableString.Invariant(FormattableStringFactory.Create("SELECT COUNT(1) FROM {0}", "[dbo].[$ndo$tenantcompany]")));
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, TransactionType.Update);
		NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		return navSqlCommand.ExecuteScalar<int>();
	}

	internal int GetTenantMaximumCompanyId()
	{
		StringBuilder stringBuilder = new StringBuilder(FormattableString.Invariant(FormattableStringFactory.Create("SELECT MAX([{0}]) FROM {1} WHERE [{2}] = @0", Fields[FieldNo.CompanyId].Name, "[dbo].[$ndo$tenantcompany]", Fields[FieldNo.TenantLocalId].Name)));
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, TransactionType.Browse);
		NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand.Command.Parameters.Add("@0", SqlDbType.Int);
		navSqlCommand.Command.Parameters[0].Value = tenantLocalId;
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		return navSqlCommand.ExecuteScalar<int>();
	}

	internal bool TenantExceededMaximumAllowedCompanies()
	{
		return GetTenantMaximumCompanyId() >= (tenantLocalId + 1) * 1000;
	}

	private int GenerateNewCompanyId(string companyName, NavSqlConnectionScope conScope)
	{
		int num = tenantLocalId * 1000;
		if (string.IsNullOrEmpty(companyName))
		{
			return num;
		}
		int? num2 = ReadMaxCompanyId(conScope);
		if (!num2.HasValue || num2 <= num + 9)
		{
			num2 = num + 9;
		}
		num2++;
		if (num2 >= (tenantLocalId + 1) * 1000)
		{
			int? num3 = GenerateDeletedCompanyId(conScope);
			if (num3.HasValue)
			{
				return num3.Value;
			}
			if (tenantDatabase.IsSharedTenantSchema)
			{
				throw new NavCSideException(Lang.NavNCLCreateTenantCompanyException);
			}
		}
		return num2.Value;
	}

	private int? GenerateDeletedCompanyId(NavSqlConnectionScope conScope)
	{
		int minRange = tenantLocalId * 1000 + 9;
		int maxRange = (tenantLocalId + 1) * 1000;
		object obj = conScope.Connection.ExecuteCommandWithParameters(new NavSqlCombinedStringBasedCommandCacheKey(tenantDatabase, "GetDeletedCompanyIdStatement"), () => string.Format(CultureInfo.InvariantCulture, "\r\n           WITH digits AS(\r\n               SELECT 0 AS N \r\n               UNION ALL SELECT 1 \r\n               UNION ALL SELECT 2 \r\n               UNION ALL SELECT 3 \r\n               UNION ALL SELECT 4 \r\n               UNION ALL SELECT 5 \r\n               UNION ALL SELECT 6 \r\n               UNION ALL SELECT 7 \r\n               UNION ALL SELECT 8 \r\n               UNION ALL SELECT 9 \r\n           )\r\n           SELECT TOP 1 x \r\n           FROM \r\n           (SELECT a.N + b.N * 10 + c.N * 100 + 1 + @0 as x \r\n              FROM(SELECT N FROM digits) a, \r\n                  (SELECT N FROM digits) b, \r\n                  (SELECT N FROM digits) c) t \r\n           LEFT JOIN {0} ON x = [{1}]\r\n           WHERE x < @1 AND [{1}] IS NULL\r\n           ORDER BY x", "[dbo].[$ndo$tenantcompany]", Fields[FieldNo.CompanyId].Name), (NavSqlCommand command) =>
		{
			command.Command.Parameters.AddRange(new SqlParameter[2]
			{
				new SqlParameter("@0", SqlDbType.Int),
				new SqlParameter("@1", SqlDbType.Int)
			});
		}, (NavSqlCommand command) =>
		{
			command.Command.Parameters[0].Value = minRange;
			command.Command.Parameters[1].Value = maxRange;
		}, (NavSqlCommand command) => command.ExecuteScalar());
		if (obj == null || Convert.IsDBNull(obj))
		{
			return null;
		}
		return (int)obj;
	}

	private int? ReadMaxCompanyId(NavSqlConnectionScope conScope)
	{
		return conScope.Connection.ExecuteCommandAndCacheOnSuccess(new NavSqlStringBasedCommandCacheKey("TENANTCOMPANYMAXID", tenantDatabase), () => string.Format(CultureInfo.InvariantCulture, "SELECT MAX([{0}]) FROM {1} WHERE [{2}] = @0", Fields[FieldNo.CompanyId].Name, "[dbo].[$ndo$tenantcompany]", Fields[FieldNo.TenantLocalId].Name), (NavSqlCommand commandToExecute) =>
		{
			if (commandToExecute.Command.Parameters.Count == 0)
			{
				commandToExecute.Command.Parameters.Add("@0", SqlDbType.Int);
			}
			commandToExecute.Command.Parameters[0].Value = tenantLocalId;
		}, (NavSqlCommand commandToExecute) =>
		{
			object obj = commandToExecute.ExecuteScalar();
			return (obj == null || Convert.IsDBNull(obj)) ? ((int?)null) : new int?((int)obj);
		});
	}

	private void Insert(int companyId, string companyName, NavSqlConnectionScope conScope)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("INSERT INTO ").Append("[dbo].[$ndo$tenantcompany]");
		stringBuilder.Append('(');
		foreach (FieldNo baseSchemaFieldNumber in BaseSchemaFieldNumbers)
		{
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, Fields[baseSchemaFieldNumber].Name);
			stringBuilder.Append(',');
		}
		stringBuilder.Length--;
		stringBuilder.Append(") VALUES (");
		int num = 0;
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		foreach (FieldNo baseSchemaFieldNumber2 in BaseSchemaFieldNumbers)
		{
			object value = baseSchemaFieldNumber2 switch
			{
				FieldNo.CompanyId => (object)companyId, 
				FieldNo.TenantLocalId => tenantLocalId, 
				FieldNo.CompanyName => companyName, 
				_ => throw new NotImplementedException(), 
			};
			string text = NavSqlStatementHelper.ParameterName(num++);
			SqlParameter value2 = NavSqlStatementHelper.CreateSqlParameter(text, Fields[baseSchemaFieldNumber2].Type, SqlDataType.Default, Fields[baseSchemaFieldNumber2].Length, value, isWildcardValue: false, isFilterValue: false);
			navSqlCommand.Command.Parameters.Add(value2);
			stringBuilder.Append(text);
			stringBuilder.Append(',');
		}
		stringBuilder.Length--;
		stringBuilder.Append(')');
		navSqlCommand.ExecuteNonQuery(stringBuilder.ToString());
	}

	private T GetFieldValue<T>(string companyName, FieldNo fieldNo)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, TransactionType.UpdateNoLocks);
		return navSqlConnectionScope.Connection.ExecuteCommandAndCacheOnSuccess(new NavSqlStringBasedCommandCacheKey("GetTenantCompanyFieldValue" + fieldNo, tenantDatabase), () =>
		{
			StringBuilder stringBuilder = new StringBuilder("SELECT TOP 1 ");
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, Fields[fieldNo].Name);
			stringBuilder.Append(" FROM ");
			stringBuilder.Append("[dbo].[$ndo$tenantcompany]");
			stringBuilder.Append(" WHERE ");
			AppendTenantIdCompanyNameToWhereClause(stringBuilder);
			return stringBuilder.ToString();
		}, (NavSqlCommand commandToExecute) =>
		{
			if (commandToExecute.Command.Parameters.Count == 0)
			{
				AddTenantIdCompanyNameParameters(commandToExecute, tenantLocalId, companyName);
			}
			else
			{
				commandToExecute.Command.Parameters[0].Value = tenantLocalId;
				commandToExecute.Command.Parameters[1].Value = companyName;
			}
		}, (NavSqlCommand commandToExecute) =>
		{
			object obj = commandToExecute.ExecuteScalar();
			if (obj == null || Convert.IsDBNull(obj))
			{
				throw new NavCompanyNotFoundException(string.Format(CultureInfo.InvariantCulture, "Company with name {0} could not be found in tenant {1} in {2} table.", companyName, tenantLocalId, "[dbo].[$ndo$tenantcompany]"));
			}
			return (T)obj;
		});
	}

	internal List<string> GetListOfCompanyNamesWithState(CompanyState state)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, TransactionType.UpdateNoLocks);
		return navSqlConnectionScope.Connection.ExecuteCommandAndCacheOnSuccess(new NavSqlStringBasedCommandCacheKey("GetListOfCompanyNamesWithState", tenantDatabase), () =>
		{
			StringBuilder stringBuilder = new StringBuilder("SELECT ");
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, Fields[FieldNo.CompanyName].Name);
			stringBuilder.Append(" FROM ");
			stringBuilder.Append("[dbo].[$ndo$tenantcompany]");
			stringBuilder.Append(FormattableString.Invariant($"WHERE {Fields[FieldNo.CompanyName].Name} <> '' AND "));
			AppendTenantIdStateToWhereClause(stringBuilder);
			return stringBuilder.ToString();
		}, (NavSqlCommand commandToExecute) =>
		{
			if (commandToExecute.Command.Parameters.Count == 0)
			{
				AddTenantIdStateParameters(commandToExecute, tenantLocalId, state);
			}
			else
			{
				commandToExecute.Command.Parameters[0].Value = tenantLocalId;
				commandToExecute.Command.Parameters[1].Value = state;
			}
		}, (NavSqlCommand commandToExecute) =>
		{
			List<string> list = new List<string>();
			using (commandToExecute.ExecuteReader())
			{
				while (commandToExecute.ReadNextRow())
				{
					list.Add(commandToExecute.Reader.GetString(0));
				}
				return list;
			}
		});
	}

	private void ModifyField(string companyName, FieldNo fieldNo, object value)
	{
		if (companyName == null)
		{
			throw new ArgumentNullException("companyName");
		}
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenantDatabase, TransactionType.Update);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("UPDATE ").Append("[dbo].[$ndo$tenantcompany]");
		stringBuilder.Append("SET ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, Fields[fieldNo].Name);
		stringBuilder.Append(" = @2");
		stringBuilder.Append(" WHERE ");
		AppendTenantIdCompanyNameToWhereClause(stringBuilder);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		AddTenantIdCompanyNameParameters(navSqlCommand, tenantLocalId, companyName);
		SqlParameter value2 = NavSqlStatementHelper.CreateSqlParameter("@2", Fields[fieldNo].Type, SqlDataType.Default, Fields[FieldNo.State].Length, value, isWildcardValue: false, isFilterValue: false);
		navSqlCommand.Command.Parameters.Add(value2);
		navSqlCommand.ExecuteScalar();
	}

	private static void AppendTenantIdCompanyNameToWhereClause(StringBuilder commandText)
	{
		NavSqlStatementHelper.AppendQuotedIdentifier(commandText, Fields[FieldNo.TenantLocalId].Name);
		commandText.Append(" = @0 AND ");
		NavSqlStatementHelper.AppendQuotedIdentifier(commandText, Fields[FieldNo.CompanyName].Name);
		commandText.Append(" = @1");
	}

	private static void AddTenantIdCompanyNameParameters(NavSqlCommand command, int tenantLocalId, string companyName)
	{
		SqlParameter value = NavSqlStatementHelper.CreateSqlParameter("@0", NavNclType.NavInteger, SqlDataType.Integer, Fields[FieldNo.TenantLocalId].Length, tenantLocalId, isWildcardValue: false, isFilterValue: false);
		command.Command.Parameters.Add(value);
		SqlParameter value2 = NavSqlStatementHelper.CreateSqlParameter("@1", NavNclType.NavText, SqlDataType.VarChar, Fields[FieldNo.CompanyName].Length, companyName, isWildcardValue: false, isFilterValue: false);
		command.Command.Parameters.Add(value2);
	}

	private static void AppendTenantIdStateToWhereClause(StringBuilder commandText)
	{
		NavSqlStatementHelper.AppendQuotedIdentifier(commandText, Fields[FieldNo.TenantLocalId].Name);
		commandText.Append(" = @0 AND ");
		NavSqlStatementHelper.AppendQuotedIdentifier(commandText, Fields[FieldNo.State].Name);
		commandText.Append(" = @1");
	}

	private static void AddTenantIdStateParameters(NavSqlCommand command, int tenantLocalId, CompanyState state)
	{
		SqlParameter value = NavSqlStatementHelper.CreateSqlParameter("@0", NavNclType.NavInteger, SqlDataType.Integer, Fields[FieldNo.TenantLocalId].Length, tenantLocalId, isWildcardValue: false, isFilterValue: false);
		command.Command.Parameters.Add(value);
		SqlParameter value2 = NavSqlStatementHelper.CreateSqlParameter("@1", NavNclType.NavInteger, SqlDataType.Integer, Fields[FieldNo.State].Length, state, isWildcardValue: false, isFilterValue: false);
		command.Command.Parameters.Add(value2);
	}
}
