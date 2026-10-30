using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlBatchedCommand
{
	private readonly NavDatabase database;

	private readonly StringBuilder commandText;

	private readonly List<SqlParameter> sqlParameters;

	protected StringBuilder CommandText => commandText;

	protected int ParameterCount => sqlParameters.Count;

	protected NavDatabase Database => database;

	public NavSqlBatchedCommand(NavDatabase database)
	{
		this.database = database;
		commandText = new StringBuilder();
		sqlParameters = new List<SqlParameter>();
	}

	protected void StartNewCommand()
	{
		if (commandText.Length > 0)
		{
			commandText.Append("; ");
		}
	}

	protected void StartNewExclusiveCommand()
	{
		ExecuteNonQuery();
		StartNewCommand();
	}

	public virtual void ExecuteNonQuery()
	{
		if (commandText.Length <= 0)
		{
			return;
		}
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(Database, updateOrlockingActivity: true))
		{
			using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
			navSqlCommand.SetCommandText(commandText.ToString());
			if (sqlParameters.Count > 0)
			{
				navSqlCommand.Command.Parameters.AddRange(sqlParameters.ToArray());
			}
			navSqlCommand.ExecuteNonQuery();
		}
		commandText.Length = 0;
		sqlParameters.Clear();
	}

	protected void AddParameter(SqlParameter parameter)
	{
		sqlParameters.Add(parameter);
	}
}
