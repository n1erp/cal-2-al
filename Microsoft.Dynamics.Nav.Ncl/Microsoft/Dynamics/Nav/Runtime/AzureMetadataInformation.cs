using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.Runtime;

public class AzureMetadataInformation
{
	[JsonProperty("compute")]
	public AzurePlatformInformation PlatformInformation { get; set; }
}
