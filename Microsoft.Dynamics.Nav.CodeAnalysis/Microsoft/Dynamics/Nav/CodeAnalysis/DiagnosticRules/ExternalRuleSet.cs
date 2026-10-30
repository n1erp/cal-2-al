using Newtonsoft.Json;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DiagnosticRules;

internal class ExternalRuleSet
{
	[JsonProperty(NullValueHandling = NullValueHandling.Ignore, Required = Required.Always)]
	internal string Name { get; set; }

	public string Description { get; set; }

	public GeneralAction? GeneralAction { get; set; }

	public ExternalIncludedRuleSet[] IncludedRuleSets { get; set; }

	public ExternalRule[] Rules { get; set; }
}
