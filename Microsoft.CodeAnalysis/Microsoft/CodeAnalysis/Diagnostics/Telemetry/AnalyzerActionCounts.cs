namespace Microsoft.CodeAnalysis.Diagnostics.Telemetry;

internal class AnalyzerActionCounts
{
	internal static readonly AnalyzerActionCounts Empty = new AnalyzerActionCounts(null);

	public int CompilationStartActionsCount { get; }

	public int CompilationEndActionsCount { get; }

	public int CompilationActionsCount { get; }

	public int SyntaxTreeActionsCount { get; }

	public int SemanticModelActionsCount { get; }

	public int SymbolActionsCount { get; }

	public int SyntaxNodeActionsCount { get; }

	public int CodeBlockStartActionsCount { get; }

	public int CodeBlockEndActionsCount { get; }

	public int CodeBlockActionsCount { get; }

	public int OperationActionsCount { get; }

	public int OperationBlockStartActionsCount { get; }

	public int OperationBlockEndActionsCount { get; }

	public int OperationBlockActionsCount { get; }

	public bool HasAnyExecutableCodeActions { get; }

	public bool Concurrent { get; }

	internal AnalyzerActionCounts(AnalyzerActions analyzerActions)
		: this(analyzerActions?.CompilationStartActionsCount ?? 0, analyzerActions?.CompilationEndActionsCount ?? 0, analyzerActions?.CompilationActionsCount ?? 0, analyzerActions?.SyntaxTreeActionsCount ?? 0, analyzerActions?.SemanticModelActionsCount ?? 0, analyzerActions?.SymbolActionsCount ?? 0, analyzerActions?.SyntaxNodeActionsCount ?? 0, analyzerActions?.CodeBlockStartActionsCount ?? 0, analyzerActions?.CodeBlockEndActionsCount ?? 0, analyzerActions?.CodeBlockActionsCount ?? 0, analyzerActions?.OperationActionsCount ?? 0, analyzerActions?.OperationBlockStartActionsCount ?? 0, analyzerActions?.OperationBlockEndActionsCount ?? 0, analyzerActions?.OperationBlockActionsCount ?? 0, analyzerActions?.Concurrent ?? false)
	{
	}

	internal AnalyzerActionCounts(int compilationStartActionsCount, int compilationEndActionsCount, int compilationActionsCount, int syntaxTreeActionsCount, int semanticModelActionsCount, int symbolActionsCount, int syntaxNodeActionsCount, int codeBlockStartActionsCount, int codeBlockEndActionsCount, int codeBlockActionsCount, int operationActionsCount, int operationBlockStartActionsCount, int operationBlockEndActionsCount, int operationBlockActionsCount, bool concurrent)
	{
		CompilationStartActionsCount = compilationStartActionsCount;
		CompilationEndActionsCount = compilationEndActionsCount;
		CompilationActionsCount = compilationActionsCount;
		SyntaxTreeActionsCount = syntaxTreeActionsCount;
		SemanticModelActionsCount = semanticModelActionsCount;
		SymbolActionsCount = symbolActionsCount;
		SyntaxNodeActionsCount = syntaxNodeActionsCount;
		CodeBlockStartActionsCount = codeBlockStartActionsCount;
		CodeBlockEndActionsCount = codeBlockEndActionsCount;
		CodeBlockActionsCount = codeBlockActionsCount;
		OperationActionsCount = operationActionsCount;
		OperationBlockStartActionsCount = operationBlockStartActionsCount;
		OperationBlockEndActionsCount = operationBlockEndActionsCount;
		OperationBlockActionsCount = operationBlockActionsCount;
		Concurrent = concurrent;
		HasAnyExecutableCodeActions = CodeBlockActionsCount > 0 || CodeBlockStartActionsCount > 0 || SyntaxNodeActionsCount > 0 || OperationActionsCount > 0 || OperationBlockActionsCount > 0 || OperationBlockStartActionsCount > 0;
	}
}
