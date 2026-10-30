using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DiagnosticRules;

internal class InternalIncludedRuleSet
{
	internal InternalRuleSet RuleSet { get; set; }

	internal ReportDiagnostic Action { get; set; }
}
