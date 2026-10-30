using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Runtime;

public class AzurePlatformInformation
{
	[JsonProperty("location")]
	public string Location { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("offer")]
	public string Offer { get; set; }

	[JsonProperty("osType")]
	public string OSType { get; set; }

	[JsonProperty("platformFaultDomain")]
	public string PlatformFaultDomain { get; set; }

	[JsonProperty("platformUpdateDomain")]
	public string PlatformUpdateDomain { get; set; }

	[JsonProperty("publisher")]
	public string Publisher { get; set; }

	[JsonProperty("sku")]
	public string Sku { get; set; }

	[JsonProperty("version")]
	public string Version { get; set; }

	[JsonProperty("vmId")]
	public string VMId { get; set; }

	[JsonProperty("vmSize")]
	public string VMSize { get; set; }
}
