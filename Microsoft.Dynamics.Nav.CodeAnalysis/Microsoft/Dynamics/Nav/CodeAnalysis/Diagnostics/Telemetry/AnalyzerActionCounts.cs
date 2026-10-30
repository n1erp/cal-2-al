namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Telemetry;

internal class AnalyzerActionCounts
{
	internal static AnalyzerActionCounts Empty = new AnalyzerActionCounts(AnalyzerActions.Empty);

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

	private AnalyzerActionCounts(AnalyzerActions analyzerActions)
	{
		CompilationStartActionsCount = analyzerActions.CompilationStartActionsCount;
		CompilationEndActionsCount = analyzerActions.CompilationEndActionsCount;
		CompilationActionsCount = analyzerActions.CompilationActionsCount;
		SyntaxTreeActionsCount = analyzerActions.SyntaxTreeActionsCount;
		SemanticModelActionsCount = analyzerActions.SemanticModelActionsCount;
		SymbolActionsCount = analyzerActions.SymbolActionsCount;
		SyntaxNodeActionsCount = analyzerActions.SyntaxNodeActionsCount;
		CodeBlockStartActionsCount = analyzerActions.CodeBlockStartActionsCount;
		CodeBlockEndActionsCount = analyzerActions.CodeBlockEndActionsCount;
		CodeBlockActionsCount = analyzerActions.CodeBlockActionsCount;
		OperationActionsCount = analyzerActions.OperationActionsCount;
		OperationBlockStartActionsCount = analyzerActions.OperationBlockStartActionsCount;
		OperationBlockEndActionsCount = analyzerActions.OperationBlockEndActionsCount;
		OperationBlockActionsCount = analyzerActions.OperationBlockActionsCount;
		HasAnyExecutableCodeActions = CodeBlockActionsCount > 0 || CodeBlockStartActionsCount > 0 || SyntaxNodeActionsCount > 0 || OperationActionsCount > 0 || OperationBlockActionsCount > 0 || OperationBlockStartActionsCount > 0;
	}

	internal static AnalyzerActionCounts Create(AnalyzerActions analyzerActions)
	{
		if (analyzerActions == null)
		{
			return Empty;
		}
		return new AnalyzerActionCounts(analyzerActions);
	}
}
