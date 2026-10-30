using System;
using System.Collections.Immutable;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class AnalyzerCompilationStartAnalysisContext : CompilationStartAnalysisContext
{
	private readonly DiagnosticAnalyzer analyzer;

	private readonly HostCompilationStartAnalysisScope scope;

	public AnalyzerCompilationStartAnalysisContext(DiagnosticAnalyzer analyzer, HostCompilationStartAnalysisScope scope, Compilation compilation, AnalyzerOptions options, CancellationToken cancellationToken)
		: base(compilation, options, cancellationToken)
	{
		this.analyzer = analyzer;
		this.scope = scope;
	}

	public override void RegisterCompilationEndAction(Action<CompilationAnalysisContext> action)
	{
		scope.RegisterCompilationEndAction(analyzer, action);
	}

	public override void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action)
	{
		scope.RegisterSyntaxTreeAction(analyzer, action);
	}

	public override void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action)
	{
		scope.RegisterSemanticModelAction(analyzer, action);
	}

	public override void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds)
	{
		scope.RegisterSymbolAction(analyzer, action, symbolKinds);
	}

	public override void RegisterCodeBlockStartAction(Action<CodeBlockStartAnalysisContext> action)
	{
		scope.RegisterCodeBlockStartAction(analyzer, action);
	}

	public override void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action)
	{
		scope.RegisterCodeBlockAction(analyzer, action);
	}

	public override void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<SyntaxKind> syntaxKinds)
	{
		scope.RegisterSyntaxNodeAction(analyzer, action, syntaxKinds);
	}
}
