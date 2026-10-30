using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class SyntaxNodeAnalyzerAction<TLanguageKindEnum> : AnalyzerAction where TLanguageKindEnum : struct
{
	private readonly Action<SyntaxNodeAnalysisContext> _action;

	private readonly ImmutableArray<TLanguageKindEnum> _kinds;

	public Action<SyntaxNodeAnalysisContext> Action => _action;

	public ImmutableArray<TLanguageKindEnum> Kinds => _kinds;

	public SyntaxNodeAnalyzerAction(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> kinds, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		_action = action;
		_kinds = kinds;
	}
}
