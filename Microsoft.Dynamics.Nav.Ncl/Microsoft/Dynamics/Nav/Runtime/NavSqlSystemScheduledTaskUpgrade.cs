using System.Collections.Generic;
using System.Text;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavSqlSystemScheduledTaskUpgrade
{
	public static NavSqlCommand GetCreateTableCommand(NavSqlConnectionScope conScope)
	{
		StringBuilder stringBuilder = new StringBuilder("CREATE TABLE ");
		stringBuilder.Append("[dbo].[$ndo$systemscheduledtask]");
		stringBuilder.Append(" (");
		foreach (KeyValuePair<NavSqlSystemScheduledTask.FieldNo, NavSqlFieldDefinition> field in NavSqlSystemScheduledTask.Fields)
		{
			NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, field.Value.Name);
			stringBuilder.Append(' ');
			if (field.Value.Length == int.MaxValue)
			{
				NavSqlStatementHelper.AppendSqlColumnTypeAndMaxLength(stringBuilder, field.Value.Type);
			}
			else
			{
				NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder, field.Value.Type, SqlDataType.Default, field.Value.Length);
			}
			stringBuilder.Append(" NOT NULL,");
		}
		stringBuilder.Append(" CONSTRAINT ");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, "PK$ndo$systemscheduledtask");
		stringBuilder.Append(" PRIMARY KEY (");
		NavSqlStatementHelper.AppendQuotedIdentifier(stringBuilder, NavSqlSystemScheduledTask.Fields[NavSqlSystemScheduledTask.FieldNo.TaskId].Name);
		stringBuilder.AppendLine("));");
		NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.SetCommandText(stringBuilder.ToString());
		return navSqlCommand;
	}
}
