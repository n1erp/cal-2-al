using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSqlSequenceProvider : INavSequenceProvider
{
	private readonly NavDatabase database;

	public NavSqlSequenceProvider(NavDatabase database)
	{
		this.database = database;
	}

	public long GetNextValue(string sequenceName)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.Browse);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		object value = navSqlCommand.ExecuteScalar("SELECT NEXT VALUE FOR [" + sequenceName + "]");
		return (long)Convert.ChangeType(value, typeof(long), CultureInfo.InvariantCulture);
	}

	public void UpdateSequenceCurrentValue(string sequenceName, long startValue)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, TransactionType.Browse);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand.SetCommandText("declare @next_range_size bigint;\r\n                    set @next_range_size = (select @value - CAST(current_value as bigint) from sys.sequences where name = N'" + sequenceName + "')\r\n\r\n                    if @next_range_size > 0\r\n                    begin\r\n                        declare @first_value sql_variant;\r\n                        exec sp_sequence_get_range @sequence_name = N'" + sequenceName + "', @range_size = @next_range_size, @range_first_value = @first_value OUTPUT\r\n                    end ");
		navSqlCommand.Command.Parameters.AddWithValue("value", startValue);
		navSqlCommand.ExecuteNonQuery();
	}
}
