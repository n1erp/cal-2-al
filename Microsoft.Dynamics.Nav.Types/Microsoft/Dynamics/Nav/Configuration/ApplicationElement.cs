using System.Configuration;

namespace Microsoft.Dynamics.Nav.Configuration;

public class ApplicationElement : ConfigurationElement
{
	private const string DatabaseServerConfigurationName = "databaseServer";

	private const string DatabaseConfigurationName = "database";

	[ConfigurationProperty("databaseServer", IsKey = false, IsRequired = true)]
	public string DatabaseServer
	{
		get
		{
			return base["databaseServer"] as string;
		}
		set
		{
			base["databaseServer"] = value;
		}
	}

	[ConfigurationProperty("database", IsKey = false, IsRequired = true)]
	public string Database
	{
		get
		{
			return base["database"] as string;
		}
		set
		{
			base["database"] = value;
		}
	}
}
