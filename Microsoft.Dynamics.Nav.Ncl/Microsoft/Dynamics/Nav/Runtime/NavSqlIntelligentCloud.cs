using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlIntelligentCloud
{
	internal enum FieldNo
	{
		PrimaryKey,
		Enabled
	}

	private NavDatabase database;

	private const string TableName = "[dbo].[Intelligent Cloud]";

	private const string ReadTopOneCommandText = "IF OBJECT_ID('[dbo].[Intelligent Cloud]', 'U') IS NOT NULL SELECT TOP 1 [Enabled] FROM [dbo].[Intelligent Cloud] WITH(NOLOCK)";

	internal static readonly IReadOnlyDictionary<FieldNo, NavSqlFieldDefinition> Fields = new Dictionary<FieldNo, NavSqlFieldDefinition>
	{
		{
			FieldNo.PrimaryKey,
			new NavSqlFieldDefinition("Primary Key", NavNclType.NavText, 10)
		},
		{
			FieldNo.Enabled,
			new NavSqlFieldDefinition("Enabled", NavNclType.NavByte, 0)
		}
	};

	public NavSqlIntelligentCloud(NavDatabase database)
	{
		this.database = database;
	}

	public bool? ReadIsIntelligentCloudEnabledFromDatabase()
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(database, TransactionType.Browse);
		return navSqlConnectionScope.Connection.ExecuteCommandWithoutParameters(new NavSqlStringBasedCommandCacheKey("NavSqlIntelligentCloud_Get", navSqlConnectionScope.Connection.Database), () => "IF OBJECT_ID('[dbo].[Intelligent Cloud]', 'U') IS NOT NULL SELECT TOP 1 [Enabled] FROM [dbo].[Intelligent Cloud] WITH(NOLOCK)", (NavSqlCommand cmd) =>
		{
			using (cmd.ExecuteReader())
			{
				if (cmd.ReadNextRow())
				{
					return cmd.Reader.GetByte(0) != 0;
				}
			}
			return (bool?)null;
		});
	}
}
