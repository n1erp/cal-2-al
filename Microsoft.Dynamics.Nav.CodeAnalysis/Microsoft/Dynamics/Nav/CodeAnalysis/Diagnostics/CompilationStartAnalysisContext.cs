using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

public abstract class CompilationStartAnalysisContext
{
	public Compilation Compilation { get; }

	public AnalyzerOptions Options { get; }

	public CancellationToken CancellationToken { get; }

	protected CompilationStartAnalysisContext(Compilation compilation, AnalyzerOptions options, CancellationToken cancellationToken)
	{
		Compilation = compilation;
		Options = options;
		CancellationToken = cancellationToken;
	}

	public abstract void RegisterCompilationEndAction(Action<CompilationAnalysisContext> action);

	public abstract void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action);

	public void RegisterSymbolAction(Action<SymbolAnalysisContext> action, params SymbolKind[] symbolKinds)
	{
		RegisterSymbolAction(action, symbolKinds.AsImmutableOrNull());
	}

	public abstract void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds);

	[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design for now")]
	public abstract void RegisterCodeBlockStartAction(Action<CodeBlockStartAnalysisContext> action);

	public abstract void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action);

	public abstract void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action);

	public void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, params SyntaxKind[] syntaxKinds)
	{
		RegisterSyntaxNodeAction(action, syntaxKinds.AsImmutableOrNull());
	}

	public abstract void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<SyntaxKind> syntaxKinds);
}
