using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavSqlTenantCompaniesUpgrade
{
	internal static bool CreateTableBaseSchema(NavSqlConnectionScope conScope)
	{
		return CreateTable(conScope, baseFieldsOnly: true);
	}

	internal static bool CreateTable(NavSqlConnectionScope conScope)
	{
		return CreateTable(conScope, baseFieldsOnly: false);
	}

	private static bool CreateTable(NavSqlConnectionScope conScope, bool baseFieldsOnly)
	{
		if (IsTableCreated(conScope))
		{
			return false;
		}
		using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection))
		{
			navSqlCommand.SetCommandText(CreateTableSqlCommandText(baseFieldsOnly));
			navSqlCommand.ExecuteNonQuery();
		}
		return true;
	}

	internal static void FillTableWithExistingCompanies(NavSqlConnectionScope conScope)
	{
		NavDatabase database = conScope.Connection.Database;
		if (database.TenantProperties.TenantLocalId.HasValue)
		{
			database.TenantCompanies.InsertSharedCompany();
			int[] companyTokens = database.GetCompanyTokens();
			foreach (int token in companyTokens)
			{
				database.TenantCompanies.Insert(database.GetCompanyNameFromToken(token));
			}
		}
	}

	internal static NavSqlCommand GetAddCompanyStateSqlCommand(NavSqlConnectionScope conScope)
	{
		NavSqlFieldDefinition navSqlFieldDefinition = NavSqlTenantCompanies.Fields[NavSqlTenantCompanies.FieldNo.State];
		string text = NavSqlStatementHelper.AppendSqlColumnTypeAndLength(new StringBuilder(), navSqlFieldDefinition.Type, SqlDataType.Default, navSqlFieldDefinition.Length).ToString();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(FormattableString.Invariant(FormattableStringFactory.Create("ALTER TABLE {0}\r\n                ADD [{1}] {2} NOT NULL ", "[dbo].[$ndo$tenantcompany]", navSqlFieldDefinition.Name, text)));
		AppendDefaultValueForCompanyState(stringBuilder);
		NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		return navSqlCommand;
	}

	private static string CreateTableSqlCommandText(bool baseFieldsOnly)
	{
		IEnumerable<NavSqlTenantCompanies.FieldNo> enumerable;
		if (!baseFieldsOnly)
		{
			enumerable = NavSqlTenantCompanies.Fields.Select((KeyValuePair<NavSqlTenantCompanies.FieldNo, NavSqlFieldDefinition> keyValue) => keyValue.Key);
		}
		else
		{
			IEnumerable<NavSqlTenantCompanies.FieldNo> baseSchemaFieldNumbers = NavSqlTenantCompanies.BaseSchemaFieldNumbers;
			enumerable = baseSchemaFieldNumbers;
		}
		IEnumerable<NavSqlTenantCompanies.FieldNo> enumerable2 = enumerable;
		StringBuilder stringBuilder = new StringBuilder("CREATE TABLE ");
		stringBuilder.Append("[dbo].[$ndo$tenantcompany]");
		stringBuilder.Append(" (");
		foreach (NavSqlTenantCompanies.FieldNo item in enumerable2)
		{
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, NavSqlTenantCompanies.Fields[item].Name);
			stringBuilder.Append(' ');
			NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder, NavSqlTenantCompanies.Fields[item].Type, SqlDataType.Default, NavSqlTenantCompanies.Fields[item].Length);
			stringBuilder.Append(" NOT NULL");
			if (item == NavSqlTenantCompanies.FieldNo.State)
			{
				AppendDefaultValueForCompanyState(stringBuilder);
			}
			stringBuilder.Append(",");
		}
		stringBuilder.Append(" CONSTRAINT ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, "PK$ndo$tenantcompany");
		stringBuilder.Append(" PRIMARY KEY (");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, NavSqlTenantCompanies.Fields[NavSqlTenantCompanies.FieldNo.CompanyId].Name);
		stringBuilder.AppendLine("));");
		stringBuilder.AppendLine(string.Format(CultureInfo.InvariantCulture, "CREATE UNIQUE INDEX IX$TENANTLOCALID_COMPANYNAME ON {0} ([{1}],[{2}]);", "[dbo].[$ndo$tenantcompany]", NavSqlTenantCompanies.Fields[NavSqlTenantCompanies.FieldNo.TenantLocalId].Name, NavSqlTenantCompanies.Fields[NavSqlTenantCompanies.FieldNo.CompanyName].Name));
		return stringBuilder.ToString();
	}

	private static void AppendDefaultValueForCompanyState(StringBuilder commandText)
	{
		commandText.AppendFormat(CultureInfo.InvariantCulture, " DEFAULT {0}", 0);
	}

	private static bool IsTableCreated(NavSqlConnectionScope conScope)
	{
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(FormattableString.Invariant(FormattableStringFactory.Create("IF OBJECT_ID('{0}', 'U') is not null SELECT 1 ELSE SELECT 0", "[dbo].[$ndo$tenantcompany]")));
		return navSqlCommand.ExecuteScalar<int>() == 1;
	}
}
