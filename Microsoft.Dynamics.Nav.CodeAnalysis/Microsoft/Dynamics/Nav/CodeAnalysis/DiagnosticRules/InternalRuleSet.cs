using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DiagnosticRules;

internal class InternalRuleSet
{
	internal string Name { get; set; }

	internal string Description { get; set; }

	internal ReportDiagnostic GeneralAction { get; set; }

	internal ImmutableArray<InternalIncludedRuleSet> IncludedRuleSets { get; set; }

	internal ImmutableDictionary<string, ReportDiagnostic> SpecificActions { get; set; }
}
