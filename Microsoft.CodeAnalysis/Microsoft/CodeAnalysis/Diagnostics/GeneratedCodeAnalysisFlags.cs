using System;

namespace Microsoft.CodeAnalysis.Diagnostics;

[Flags]
public enum GeneratedCodeAnalysisFlags
{
	None = 0,
	Analyze = 1,
	ReportDiagnostics = 2
}
