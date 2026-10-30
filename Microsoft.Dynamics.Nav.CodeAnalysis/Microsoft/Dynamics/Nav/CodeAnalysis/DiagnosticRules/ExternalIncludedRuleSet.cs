using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DiagnosticRules;

internal class ExternalIncludedRuleSet
{
	[JsonProperty(NullValueHandling = NullValueHandling.Ignore, Required = Required.Always)]
	public IncludeAction Action { get; set; }

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore, Required = Required.Always)]
	public string Path { get; set; }
}
