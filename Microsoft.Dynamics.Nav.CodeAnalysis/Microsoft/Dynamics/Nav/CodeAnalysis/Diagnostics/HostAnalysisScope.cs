using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal abstract class HostAnalysisScope
{
	private ImmutableArray<CompilationAnalyzerAction> compilationActions = ImmutableArray<CompilationAnalyzerAction>.Empty;

	private ImmutableArray<CompilationAnalyzerAction> compilationEndActions = ImmutableArray<CompilationAnalyzerAction>.Empty;

	private ImmutableArray<SemanticModelAnalyzerAction> semanticModelActions = ImmutableArray<SemanticModelAnalyzerAction>.Empty;

	private ImmutableArray<SyntaxTreeAnalyzerAction> syntaxTreeActions = ImmutableArray<SyntaxTreeAnalyzerAction>.Empty;

	private ImmutableArray<SymbolAnalyzerAction> symbolActions = ImmutableArray<SymbolAnalyzerAction>.Empty;

	private ImmutableArray<AnalyzerAction> codeBlockStartActions = ImmutableArray<AnalyzerAction>.Empty;

	private ImmutableArray<CodeBlockAnalyzerAction> codeBlockEndActions = ImmutableArray<CodeBlockAnalyzerAction>.Empty;

	private ImmutableArray<CodeBlockAnalyzerAction> codeBlockActions = ImmutableArray<CodeBlockAnalyzerAction>.Empty;

	private ImmutableArray<AnalyzerAction> syntaxNodeActions = ImmutableArray<AnalyzerAction>.Empty;

	private ImmutableArray<OperationBlockStartAnalyzerAction> operationBlockStartActions = ImmutableArray<OperationBlockStartAnalyzerAction>.Empty;

	private ImmutableArray<OperationBlockAnalyzerAction> operationBlockEndActions = ImmutableArray<OperationBlockAnalyzerAction>.Empty;

	private ImmutableArray<OperationBlockAnalyzerAction> operationBlockActions = ImmutableArray<OperationBlockAnalyzerAction>.Empty;

	private ImmutableArray<OperationAnalyzerAction> operationActions = ImmutableArray<OperationAnalyzerAction>.Empty;

	private readonly Dictionary<DiagnosticAnalyzer, AnalyzerActions> analyzerActions = new Dictionary<DiagnosticAnalyzer, AnalyzerActions>();

	public ImmutableArray<CompilationAnalyzerAction> CompilationActions => compilationActions;

	public virtual ImmutableArray<CompilationAnalyzerAction> CompilationEndActions => compilationEndActions;

	public virtual ImmutableArray<SemanticModelAnalyzerAction> SemanticModelActions => semanticModelActions;

	public virtual ImmutableArray<SyntaxTreeAnalyzerAction> SyntaxTreeActions => syntaxTreeActions;

	public virtual ImmutableArray<SymbolAnalyzerAction> SymbolActions => symbolActions;

	public virtual ImmutableArray<CodeBlockAnalyzerAction> CodeBlockEndActions => codeBlockEndActions;

	public virtual ImmutableArray<CodeBlockAnalyzerAction> CodeBlockActions => codeBlockActions;

	public virtual bool HasCodeBlockEndActions => codeBlockEndActions.Any();

	public virtual bool HasCodeBlockActions => codeBlockActions.Any();

	public virtual bool HasCodeBlockStartActions()
	{
		return codeBlockStartActions.OfType<CodeBlockStartAnalyzerAction>().Any();
	}

	public virtual ImmutableArray<CodeBlockStartAnalyzerAction> GetCodeBlockStartActions()
	{
		return codeBlockStartActions.OfType<CodeBlockStartAnalyzerAction>().ToImmutableArray();
	}

	public virtual ImmutableArray<SyntaxNodeAnalyzerAction> GetSyntaxNodeActions()
	{
		return syntaxNodeActions.OfType<SyntaxNodeAnalyzerAction>().ToImmutableArray();
	}

	public virtual AnalyzerActions GetAnalyzerActions(DiagnosticAnalyzer analyzer)
	{
		analyzerActions.TryGetValue(analyzer, out var value);
		return value;
	}

	public void RegisterCompilationAction(DiagnosticAnalyzer analyzer, Action<CompilationAnalysisContext> action)
	{
		CompilationAnalyzerAction compilationAnalyzerAction = new CompilationAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddCompilationAction(compilationAnalyzerAction);
		compilationActions = compilationActions.Add(compilationAnalyzerAction);
	}

	public void RegisterCompilationEndAction(DiagnosticAnalyzer analyzer, Action<CompilationAnalysisContext> action)
	{
		CompilationAnalyzerAction compilationAnalyzerAction = new CompilationAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddCompilationEndAction(compilationAnalyzerAction);
		compilationEndActions = compilationEndActions.Add(compilationAnalyzerAction);
	}

	public void RegisterSemanticModelAction(DiagnosticAnalyzer analyzer, Action<SemanticModelAnalysisContext> action)
	{
		SemanticModelAnalyzerAction semanticModelAnalyzerAction = new SemanticModelAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddSemanticModelAction(semanticModelAnalyzerAction);
		semanticModelActions = semanticModelActions.Add(semanticModelAnalyzerAction);
	}

	public void RegisterSyntaxTreeAction(DiagnosticAnalyzer analyzer, Action<SyntaxTreeAnalysisContext> action)
	{
		SyntaxTreeAnalyzerAction syntaxTreeAnalyzerAction = new SyntaxTreeAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddSyntaxTreeAction(syntaxTreeAnalyzerAction);
		syntaxTreeActions = syntaxTreeActions.Add(syntaxTreeAnalyzerAction);
	}

	public void RegisterSymbolAction(DiagnosticAnalyzer analyzer, Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds)
	{
		SymbolAnalyzerAction symbolAnalyzerAction = new SymbolAnalyzerAction(action, symbolKinds, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddSymbolAction(symbolAnalyzerAction);
		symbolActions = symbolActions.Add(symbolAnalyzerAction);
	}

	public void RegisterCodeBlockStartAction(DiagnosticAnalyzer analyzer, Action<CodeBlockStartAnalysisContext> action)
	{
		CodeBlockStartAnalyzerAction codeBlockStartAnalyzerAction = new CodeBlockStartAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddCodeBlockStartAction(codeBlockStartAnalyzerAction);
		codeBlockStartActions = codeBlockStartActions.Add(codeBlockStartAnalyzerAction);
	}

	public void RegisterCodeBlockEndAction(DiagnosticAnalyzer analyzer, Action<CodeBlockAnalysisContext> action)
	{
		CodeBlockAnalyzerAction codeBlockAnalyzerAction = new CodeBlockAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddCodeBlockEndAction(codeBlockAnalyzerAction);
		codeBlockEndActions = codeBlockEndActions.Add(codeBlockAnalyzerAction);
	}

	public void RegisterCodeBlockAction(DiagnosticAnalyzer analyzer, Action<CodeBlockAnalysisContext> action)
	{
		CodeBlockAnalyzerAction codeBlockAnalyzerAction = new CodeBlockAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddCodeBlockAction(codeBlockAnalyzerAction);
		codeBlockActions = codeBlockActions.Add(codeBlockAnalyzerAction);
	}

	public void RegisterSyntaxNodeAction(DiagnosticAnalyzer analyzer, Action<SyntaxNodeAnalysisContext> action, ImmutableArray<SyntaxKind> syntaxKinds)
	{
		SyntaxNodeAnalyzerAction syntaxNodeAnalyzerAction = new SyntaxNodeAnalyzerAction(action, syntaxKinds, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddSyntaxNodeAction(syntaxNodeAnalyzerAction);
		syntaxNodeActions = syntaxNodeActions.Add(syntaxNodeAnalyzerAction);
	}

	public void RegisterOperationBlockStartAction(DiagnosticAnalyzer analyzer, Action<OperationBlockStartAnalysisContext> action)
	{
		OperationBlockStartAnalyzerAction operationBlockStartAnalyzerAction = new OperationBlockStartAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddOperationBlockStartAction(operationBlockStartAnalyzerAction);
		operationBlockStartActions = operationBlockStartActions.Add(operationBlockStartAnalyzerAction);
	}

	public void RegisterOperationBlockEndAction(DiagnosticAnalyzer analyzer, Action<OperationBlockAnalysisContext> action)
	{
		OperationBlockAnalyzerAction operationBlockAnalyzerAction = new OperationBlockAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddOperationBlockEndAction(operationBlockAnalyzerAction);
		operationBlockEndActions = operationBlockEndActions.Add(operationBlockAnalyzerAction);
	}

	public void RegisterOperationBlockAction(DiagnosticAnalyzer analyzer, Action<OperationBlockAnalysisContext> action)
	{
		OperationBlockAnalyzerAction operationBlockAnalyzerAction = new OperationBlockAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddOperationBlockAction(operationBlockAnalyzerAction);
		operationBlockActions = operationBlockActions.Add(operationBlockAnalyzerAction);
	}

	public void RegisterOperationAction(DiagnosticAnalyzer analyzer, Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		OperationAnalyzerAction operationAnalyzerAction = new OperationAnalyzerAction(action, operationKinds, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddOperationAction(operationAnalyzerAction);
		operationActions = operationActions.Add(operationAnalyzerAction);
	}

	protected AnalyzerActions GetOrCreateAnalyzerActions(DiagnosticAnalyzer analyzer)
	{
		if (!analyzerActions.TryGetValue(analyzer, out var value))
		{
			value = new AnalyzerActions();
			analyzerActions[analyzer] = value;
		}
		return value;
	}
}
