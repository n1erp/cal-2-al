namespace Microsoft.Dynamics.Nav.Types;

public static class ManagementTables
{
	public static class AddIn
	{
		public static class Columns
		{
			public const string AddInName = "AddInName";

			public const string PublicKeyToken = "PublicKeyToken";

			public const string Version = "Version";

			public const string Category = "Category";

			public const string Description = "Description";

			public const string Resource = "Resource";
		}

		public const string Name = "AddIn";
	}

	public static class Application
	{
		public static class Columns
		{
			public const string DatabaseServer = "Database server";

			public const string DatabaseName = "Database name";

			public const string ApplicationVersion = "ApplicationVersion";

			public const string ApplicationFamily = "ApplicationFamily";
		}

		public const string Name = "Nav Application";
	}

	public static class WebServerInstance
	{
		public static class Columns
		{
			public const string WebServerInstance = "WebServerInstance";

			public const string Website = "Website";

			public const string Uri = "Uri";

			public const string Server = "Server";

			public const string ServerInstance = "ServerInstance";

			public const string ClientServicesPort = "ClientServicesPort";

			public const string ClientServicesCredentialType = "ClientServicesCredentialType";

			public const string Company = "Company";

			public const string Language = "Language";

			public const string RegionFormat = "RegionFormat";

			public const string DnsIdentity = "DnsIdentity";

			public const string ConfigurationFile = "Configuration File";
		}

		public const string Name = "Web Server Instance";
	}

	public static class Company
	{
		public static class Columns
		{
			public const string CompanyName = "CompanyName";

			public const string CompanyDisplayName = "CompanyDisplayName";

			public const string EvaluationCompany = "EvaluationCompany";

			public const string State = "State";
		}

		public const string Name = "Company";
	}

	public static class TenantLicenseState
	{
		public static class Columns
		{
			public const string StartDate = "StartDate";

			public const string EndDate = "EndDate";

			public const string State = "State";
		}

		public const string Name = "Tenant License State";
	}

	public static class WebService
	{
		public static class Columns
		{
			public const string ObjectId = "ObjectId";

			public const string ObjectType = "ObjectType";

			public const string ServiceName = "ServiceName";

			public const string Published = "Published";

			public const string AllowOverride = "AllowOverride";
		}

		public const string Name = "Web Service";
	}

	public static class NavDataExportInformation
	{
		public static class Columns
		{
			public const string FileName = "FileName";

			public const string ExportVersion = "ExportVersion";

			public const string DatabaseVersion = "DatabaseVersion";

			public const string Description = "Description";

			public const string IncludeApplication = "IncludeApplication";

			public const string IncludeApplicationData = "IncludeApplicationData";

			public const string TenantVersion = "TenantVersion";

			public const string TenantId = "TenantId";

			public const string IncludeGlobalData = "IncludeGlobalData";

			public const string CompanyName = "CompanyName";

			public const string ExportDateTime = "ExportDateTime";
		}

		public const string Name = "Nav Export Information";
	}
}
