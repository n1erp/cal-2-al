using System;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class CodeBlockStartAnalyzerAction<TLanguageKindEnum> : AnalyzerAction where TLanguageKindEnum : struct
{
	private readonly Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> _action;

	public Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> Action => _action;

	public CodeBlockStartAnalyzerAction(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action, DiagnosticAnalyzer analyzer)
		: base(analyzer)
	{
		_action = action;
	}
}
