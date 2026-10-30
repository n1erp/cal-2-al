using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Types;

internal class AppConfigurationSetting
{
	public string Domain { get; set; }

	public string ApplicationConfigurationId { get; set; }

	public IDictionary<string, string> ServerSettings { get; set; }
}
