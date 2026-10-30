using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics.Telemetry;

public static class AnalyzerTelemetry
{
	[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
	public class AnalyzerActionCounts
	{
		private static readonly AnalyzerActionCounts empty = new AnalyzerActionCounts(AnalyzerActions.Empty);

		public int CompilationStartActionsCount { get; private set; }

		public int CompilationEndActionsCount { get; private set; }

		public int CompilationActionsCount { get; private set; }

		public int SyntaxTreeActionsCount { get; private set; }

		public int SemanticModelActionsCount { get; private set; }

		public int SymbolActionsCount { get; private set; }

		public int SyntaxNodeActionsCount { get; private set; }

		public int CodeBlockStartActionsCount { get; private set; }

		public int CodeBlockEndActionsCount { get; private set; }

		public int CodeBlockActionsCount { get; private set; }

		private static AnalyzerActionCounts Empty => empty;

		internal static AnalyzerActionCounts Create(AnalyzerActions analyzerActions)
		{
			if (analyzerActions == null)
			{
				return Empty;
			}
			return new AnalyzerActionCounts(analyzerActions);
		}

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
		}
	}

	public static TimeSpan GetAnalyzerExecutionTime(this CompilationWithAnalyzers compilationWithAnalyzers, DiagnosticAnalyzer analyzer)
	{
		return compilationWithAnalyzers.GetAnalyzerExecutionTime(analyzer);
	}
}
