using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class SyntaxNodeAnalyzerAction : AnalyzerAction
{
	public Action<SyntaxNodeAnalysisContext> Action { get; }

	public ImmutableArray<SyntaxKind> Kinds { get; }

	public SyntaxNodeAnalyzerAction(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<SyntaxKind> kinds, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		Action = action;
		Kinds = kinds;
	}
}
