using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class AzureLicenseUsagePayloadBuilder
{
	public const string ServerVersion = "ServerVersion";

	public const string TenantId = "TenantId";

	public const string IsAzureVM = "IsAzureVM";

	public const string AzureVMSize = "AzureVMSize";

	public const string AzureRegion = "AzureRegion";

	public const string SqlServerEngine = "SqlServerEngine";

	public const string SqlServerVersion = "SqlServerVersion";

	public const string UsingSharedSchema = "UsingSharedSchema";

	public const string NumberOfServers = "NumberOfServers";

	public const string IsMicrosoftLicense = "IsMicrosoftLicense";

	public const string LicenseModel = "LicenseModel";

	public const string LicenseFunctionality = "LicenseFunctionality";

	public const string LicenseNumberOfFullUsers = "LicenseNumberOfFullUsers";

	public const string LicenseNumberOfLimitedUsers = "LicenseNumberOfLimitedUsers";

	internal const int MicrosoftAccountNumber = 3048953;

	internal static string[] MicrosoftLicensesSerialNumber = new string[2]
	{
		3048953.ToString(CultureInfo.InvariantCulture) + "   ",
		"US-ZA-024-SXS50-1   "
	};

	private NavTenant tenantToBuild;

	private AzurePlatformInformation azurePlatformInformationToBuild;

	public void Add(NavTenant tenant)
	{
		tenantToBuild = tenant;
	}

	public void Add(AzurePlatformInformation azurePlatformInformation)
	{
		azurePlatformInformationToBuild = azurePlatformInformation;
	}

	public Dictionary<string, string> Build()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		AddServerInformation(dictionary);
		AddTenantInfo(dictionary, tenantToBuild);
		AddAzureInformation(dictionary);
		return dictionary;
	}

	private static void AddToPayload(Dictionary<string, string> payload, string key, string value)
	{
		payload[key] = value;
	}

	private static void AddTenantInfo(Dictionary<string, string> payload, NavTenant tenant)
	{
		if (!tenant.Database.TenantProperties.AppInsightsIdentifiers.TryGetValue(tenant.Database.SecurityAndLicense.License.LicenseUniqueIdentifier, out var value))
		{
			throw new InvalidOperationException("The tenant doesn't have a Guid specified for this license");
		}
		AddToPayload(payload, "TenantId", value.ToString("D"));
		NavSqlServerProperties sqlServerProperties = tenant.Database.SqlServerProperties;
		AddToPayload(payload, "SqlServerEngine", sqlServerProperties.EngineEdition.ToString());
		AddToPayload(payload, "SqlServerVersion", FormattableString.Invariant($"{sqlServerProperties.MajorProductVersion}.{sqlServerProperties.MinorProductVersion}.{sqlServerProperties.BuildProductVersion}"));
		AddToPayload(payload, "UsingSharedSchema", (!tenant.Database.TenantDatabaseProperties.PerCompanySchema).ToString(CultureInfo.InvariantCulture));
		LicenseModel licenseModel = tenant.Database.SecurityAndLicense.License.LicenseModel;
		AddToPayload(payload, "LicenseModel", licenseModel.ToString());
		switch (licenseModel)
		{
		case Microsoft.Dynamics.Nav.Runtime.LicenseModel.ConcurrentConnections:
			AddToPayload(payload, "LicenseNumberOfFullUsers", tenant.Database.SecurityAndLicense.License.ConcurrentConnectionsNumberOfFullUsers.ToString(CultureInfo.InvariantCulture));
			AddToPayload(payload, "LicenseNumberOfLimitedUsers", tenant.Database.SecurityAndLicense.License.ConcurrentConnectionsNumberOfLimitedUsers.ToString(CultureInfo.InvariantCulture));
			break;
		case Microsoft.Dynamics.Nav.Runtime.LicenseModel.NamedUsers:
			AddToPayload(payload, "LicenseNumberOfFullUsers", tenant.Database.SecurityAndLicense.License.NamedUsersNumberOfFullUsers.ToString(CultureInfo.InvariantCulture));
			AddToPayload(payload, "LicenseNumberOfLimitedUsers", tenant.Database.SecurityAndLicense.License.NamedUsersNumberOfLimitedUsers.ToString(CultureInfo.InvariantCulture));
			break;
		default:
			tenant.Diagnostics.SendTraceTag("00001L0", Category.Tenant, Verbosity.Warning, PrivacyClassification.SystemMetadata, "Unknown tenant license model, ignoring number of users in telemetry.");
			break;
		}
		if (IsLicenseInternal(tenant.Database.SecurityAndLicense.License))
		{
			AddToPayload(payload, "IsMicrosoftLicense", true.ToString(CultureInfo.InvariantCulture));
		}
	}

	private static void AddServerInformation(Dictionary<string, string> payload)
	{
		if (Heartbeat.Instance != null)
		{
			AddToPayload(payload, "NumberOfServers", Heartbeat.Instance.AliveServerInstances.Count.ToString(CultureInfo.InvariantCulture));
		}
		AddToPayload(payload, "ServerVersion", VersionCompatibility.AssemblyFileVersion.ToString());
	}

	private void AddAzureInformation(Dictionary<string, string> payload)
	{
		if (azurePlatformInformationToBuild == null)
		{
			AddToPayload(payload, "IsAzureVM", false.ToString(CultureInfo.InvariantCulture));
			return;
		}
		AddToPayload(payload, "IsAzureVM", true.ToString(CultureInfo.InvariantCulture));
		AddToPayload(payload, "AzureVMSize", azurePlatformInformationToBuild.VMSize);
		AddToPayload(payload, "AzureRegion", azurePlatformInformationToBuild.Location);
	}

	internal static bool IsLicenseInternal(INavLicense license)
	{
		if (license.AccountNumber == 3048953)
		{
			return true;
		}
		if (license.AccountNumber == 0)
		{
			string[] microsoftLicensesSerialNumber = MicrosoftLicensesSerialNumber;
			foreach (string text in microsoftLicensesSerialNumber)
			{
				if (text.Equals(license.SerialNumber, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}
}
