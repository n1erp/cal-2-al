using System.Data.SqlClient;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSqlServerProperties
{
	private const string CmdServerProperties = "SELECT CAST(ServerProperty('EngineEdition') as int), SERVERPROPERTY('Edition') as string, SERVERPROPERTY('productversion'), SERVERPROPERTY('ProductLevel') as string, SERVERPROPERTY('collation') as string";

	private const int SqlServer2016MajorVersion = 13;

	private const int SqlServer2016SP1MinorVersion = 4001;

	public int MajorProductVersion { get; private set; }

	public int MinorProductVersion { get; private set; }

	public int BuildProductVersion { get; private set; }

	internal SqlServerEdition Edition { get; private set; }

	internal SqlServerEngineEdition EngineEdition { get; private set; }

	internal string ProductLevel { get; private set; }

	internal string ServerCollation { get; private set; }

	private NavSqlServerProperties()
	{
	}

	internal NavSqlServerProperties(int majorProductVersion, int minorProductVersion, int buildProductVersion, SqlServerEdition edition, SqlServerEngineEdition engineEdition)
	{
		MajorProductVersion = majorProductVersion;
		MinorProductVersion = minorProductVersion;
		BuildProductVersion = buildProductVersion;
		Edition = edition;
		EngineEdition = engineEdition;
	}

	public static NavSqlServerProperties QueryNavSqlServerProperties(NavDatabase database)
	{
		NavSqlServerProperties navSqlServerProperties = new NavSqlServerProperties();
		navSqlServerProperties.QuerySqlServerParameters(database);
		return navSqlServerProperties;
	}

	private void SetProductVersion(string prodctVersion)
	{
		string[] array = prodctVersion.Split('.');
		if (int.TryParse(array.First(), out var result))
		{
			MajorProductVersion = result;
		}
		if (array.Length > 1 && int.TryParse(array[1], out var result2))
		{
			MinorProductVersion = result2;
		}
		if (array.Length > 2 && int.TryParse(array[2], out var result3))
		{
			BuildProductVersion = result3;
		}
	}

	private static SqlServerEdition ParseEdition(string value)
	{
		if (value.Contains("Enterprise"))
		{
			return SqlServerEdition.Enterprise;
		}
		if (value.Contains("Business"))
		{
			return SqlServerEdition.BusinessIntelligence;
		}
		if (value.Contains("Standard"))
		{
			return SqlServerEdition.Standard;
		}
		if (value.Contains("Web"))
		{
			return SqlServerEdition.Web;
		}
		if (value.Contains("Express with Advanced"))
		{
			return SqlServerEdition.ExpressWithAdvancedServices;
		}
		if (value.Contains("Express with Tools"))
		{
			return SqlServerEdition.ExpressWithTools;
		}
		if (value.Contains("Express"))
		{
			return SqlServerEdition.Express;
		}
		if (value.Contains("Developer"))
		{
			return SqlServerEdition.Developer;
		}
		if (value.Contains("Evaluation"))
		{
			return SqlServerEdition.Evaluation;
		}
		if (value.Contains("Datacenter"))
		{
			return SqlServerEdition.Datacenter;
		}
		return SqlServerEdition.None;
	}

	private void QuerySqlServerParameters(NavDatabase database)
	{
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		using SqlDataReader sqlDataReader = navSqlCommand.ExecuteReader("SELECT CAST(ServerProperty('EngineEdition') as int), SERVERPROPERTY('Edition') as string, SERVERPROPERTY('productversion'), SERVERPROPERTY('ProductLevel') as string, SERVERPROPERTY('collation') as string");
		sqlDataReader.Read();
		EngineEdition = (SqlServerEngineEdition)sqlDataReader.GetInt32(0);
		Edition = ParseEdition(sqlDataReader.GetString(1));
		SetProductVersion(sqlDataReader.GetString(2));
		ProductLevel = sqlDataReader.GetString(3);
		ServerCollation = sqlDataReader.GetString(4);
	}
}
