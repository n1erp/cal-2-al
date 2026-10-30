using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DiagnosticRules;

internal class ExternalRule
{
	[JsonProperty(NullValueHandling = NullValueHandling.Ignore, Required = Required.Always)]
	public string Id { get; set; }

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore, Required = Required.Always)]
	public RuleAction Action { get; set; }
}
