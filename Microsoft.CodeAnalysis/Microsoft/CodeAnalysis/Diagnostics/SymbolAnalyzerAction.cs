using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class SymbolAnalyzerAction : AnalyzerAction
{
	private readonly Action<SymbolAnalysisContext> _action;

	private readonly ImmutableArray<SymbolKind> _kinds;

	public Action<SymbolAnalysisContext> Action => _action;

	public ImmutableArray<SymbolKind> Kinds => _kinds;

	public SymbolAnalyzerAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> kinds, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		_action = action;
		_kinds = kinds;
	}
}
