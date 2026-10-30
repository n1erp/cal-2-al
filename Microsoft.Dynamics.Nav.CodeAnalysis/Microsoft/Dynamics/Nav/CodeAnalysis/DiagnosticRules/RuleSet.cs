using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DiagnosticRules;

public class RuleSet
{
	public ReportDiagnostic GeneralDiagnosticOption { get; }

	public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; }

	public RuleSet(ReportDiagnostic generalOption, IDictionary<string, ReportDiagnostic> specificOptions)
	{
		GeneralDiagnosticOption = generalOption;
		SpecificDiagnosticOptions = specificOptions.ToImmutableDictionary();
	}
}
