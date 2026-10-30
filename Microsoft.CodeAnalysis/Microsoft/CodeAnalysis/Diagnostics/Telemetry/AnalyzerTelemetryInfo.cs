using System;

namespace Microsoft.CodeAnalysis.Diagnostics.Telemetry;

public sealed class AnalyzerTelemetryInfo
{
	public int CompilationStartActionsCount { get; set; }

	public int CompilationEndActionsCount { get; set; }

	public int CompilationActionsCount { get; set; }

	public int SyntaxTreeActionsCount { get; set; }

	public int SemanticModelActionsCount { get; set; }

	public int SymbolActionsCount { get; set; }

	public int SyntaxNodeActionsCount { get; set; }

	public int CodeBlockStartActionsCount { get; set; }

	public int CodeBlockEndActionsCount { get; set; }

	public int CodeBlockActionsCount { get; set; }

	public int OperationActionsCount { get; set; }

	public int OperationBlockStartActionsCount { get; set; }

	public int OperationBlockEndActionsCount { get; set; }

	public int OperationBlockActionsCount { get; set; }

	public TimeSpan ExecutionTime { get; set; } = TimeSpan.Zero;

	public bool Concurrent { get; set; }

	internal AnalyzerTelemetryInfo(AnalyzerActionCounts actionCounts, TimeSpan executionTime)
	{
		CompilationStartActionsCount = actionCounts.CompilationStartActionsCount;
		CompilationEndActionsCount = actionCounts.CompilationEndActionsCount;
		CompilationActionsCount = actionCounts.CompilationActionsCount;
		SyntaxTreeActionsCount = actionCounts.SyntaxTreeActionsCount;
		SemanticModelActionsCount = actionCounts.SemanticModelActionsCount;
		SymbolActionsCount = actionCounts.SymbolActionsCount;
		SyntaxNodeActionsCount = actionCounts.SyntaxNodeActionsCount;
		CodeBlockStartActionsCount = actionCounts.CodeBlockStartActionsCount;
		CodeBlockEndActionsCount = actionCounts.CodeBlockEndActionsCount;
		CodeBlockActionsCount = actionCounts.CodeBlockActionsCount;
		OperationActionsCount = actionCounts.OperationActionsCount;
		OperationBlockStartActionsCount = actionCounts.OperationBlockStartActionsCount;
		OperationBlockEndActionsCount = actionCounts.OperationBlockEndActionsCount;
		OperationBlockActionsCount = actionCounts.OperationBlockActionsCount;
		ExecutionTime = executionTime;
		Concurrent = actionCounts.Concurrent;
	}

	public AnalyzerTelemetryInfo()
	{
	}
}
