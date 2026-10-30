using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal abstract class HostAnalysisScope
{
	private ImmutableArray<CompilationAnalyzerAction> _compilationActions = ImmutableArray<CompilationAnalyzerAction>.Empty;

	private ImmutableArray<CompilationAnalyzerAction> _compilationEndActions = ImmutableArray<CompilationAnalyzerAction>.Empty;

	private ImmutableArray<SemanticModelAnalyzerAction> _semanticModelActions = ImmutableArray<SemanticModelAnalyzerAction>.Empty;

	private ImmutableArray<SyntaxTreeAnalyzerAction> _syntaxTreeActions = ImmutableArray<SyntaxTreeAnalyzerAction>.Empty;

	private ImmutableArray<SymbolAnalyzerAction> _symbolActions = ImmutableArray<SymbolAnalyzerAction>.Empty;

	private ImmutableArray<AnalyzerAction> _codeBlockStartActions = ImmutableArray<AnalyzerAction>.Empty;

	private ImmutableArray<CodeBlockAnalyzerAction> _codeBlockEndActions = ImmutableArray<CodeBlockAnalyzerAction>.Empty;

	private ImmutableArray<CodeBlockAnalyzerAction> _codeBlockActions = ImmutableArray<CodeBlockAnalyzerAction>.Empty;

	private ImmutableArray<AnalyzerAction> _syntaxNodeActions = ImmutableArray<AnalyzerAction>.Empty;

	private ImmutableArray<OperationBlockStartAnalyzerAction> _operationBlockStartActions = ImmutableArray<OperationBlockStartAnalyzerAction>.Empty;

	private ImmutableArray<OperationBlockAnalyzerAction> _operationBlockEndActions = ImmutableArray<OperationBlockAnalyzerAction>.Empty;

	private ImmutableArray<OperationBlockAnalyzerAction> _operationBlockActions = ImmutableArray<OperationBlockAnalyzerAction>.Empty;

	private ImmutableArray<OperationAnalyzerAction> _operationActions = ImmutableArray<OperationAnalyzerAction>.Empty;

	private readonly Dictionary<DiagnosticAnalyzer, AnalyzerActions> _analyzerActions = new Dictionary<DiagnosticAnalyzer, AnalyzerActions>();

	public ImmutableArray<CompilationAnalyzerAction> CompilationActions => _compilationActions;

	public virtual ImmutableArray<CompilationAnalyzerAction> CompilationEndActions => _compilationEndActions;

	public virtual ImmutableArray<SemanticModelAnalyzerAction> SemanticModelActions => _semanticModelActions;

	public virtual ImmutableArray<SyntaxTreeAnalyzerAction> SyntaxTreeActions => _syntaxTreeActions;

	public virtual ImmutableArray<SymbolAnalyzerAction> SymbolActions => _symbolActions;

	public virtual ImmutableArray<CodeBlockAnalyzerAction> CodeBlockEndActions => _codeBlockEndActions;

	public virtual ImmutableArray<CodeBlockAnalyzerAction> CodeBlockActions => _codeBlockActions;

	public virtual bool HasCodeBlockEndActions => _codeBlockEndActions.Any();

	public virtual bool HasCodeBlockActions => _codeBlockActions.Any();

	public virtual ImmutableArray<OperationAnalyzerAction> OperationActions => _operationActions;

	public virtual bool HasCodeBlockStartActions<TLanguageKindEnum>() where TLanguageKindEnum : struct
	{
		return _codeBlockStartActions.OfType<CodeBlockStartAnalyzerAction<TLanguageKindEnum>>().Any();
	}

	public virtual ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>> GetCodeBlockStartActions<TLanguageKindEnum>() where TLanguageKindEnum : struct
	{
		return _codeBlockStartActions.OfType<CodeBlockStartAnalyzerAction<TLanguageKindEnum>>().AsImmutable();
	}

	public virtual ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> GetSyntaxNodeActions<TLanguageKindEnum>() where TLanguageKindEnum : struct
	{
		return _syntaxNodeActions.OfType<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>().AsImmutable();
	}

	public virtual AnalyzerActions GetAnalyzerActions(DiagnosticAnalyzer analyzer)
	{
		_analyzerActions.TryGetValue(analyzer, out var value);
		return value;
	}

	public void RegisterCompilationAction(DiagnosticAnalyzer analyzer, Action<CompilationAnalysisContext> action)
	{
		CompilationAnalyzerAction compilationAnalyzerAction = new CompilationAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddCompilationAction(compilationAnalyzerAction);
		_compilationActions = _compilationActions.Add(compilationAnalyzerAction);
	}

	public void RegisterCompilationEndAction(DiagnosticAnalyzer analyzer, Action<CompilationAnalysisContext> action)
	{
		CompilationAnalyzerAction compilationAnalyzerAction = new CompilationAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddCompilationEndAction(compilationAnalyzerAction);
		_compilationEndActions = _compilationEndActions.Add(compilationAnalyzerAction);
	}

	public void RegisterSemanticModelAction(DiagnosticAnalyzer analyzer, Action<SemanticModelAnalysisContext> action)
	{
		SemanticModelAnalyzerAction semanticModelAnalyzerAction = new SemanticModelAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddSemanticModelAction(semanticModelAnalyzerAction);
		_semanticModelActions = _semanticModelActions.Add(semanticModelAnalyzerAction);
	}

	public void RegisterSyntaxTreeAction(DiagnosticAnalyzer analyzer, Action<SyntaxTreeAnalysisContext> action)
	{
		SyntaxTreeAnalyzerAction syntaxTreeAnalyzerAction = new SyntaxTreeAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddSyntaxTreeAction(syntaxTreeAnalyzerAction);
		_syntaxTreeActions = _syntaxTreeActions.Add(syntaxTreeAnalyzerAction);
	}

	public void RegisterSymbolAction(DiagnosticAnalyzer analyzer, Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds)
	{
		SymbolAnalyzerAction symbolAnalyzerAction = new SymbolAnalyzerAction(action, symbolKinds, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddSymbolAction(symbolAnalyzerAction);
		_symbolActions = _symbolActions.Add(symbolAnalyzerAction);
		if (!symbolKinds.Contains(SymbolKind.Parameter))
		{
			return;
		}
		RegisterSymbolAction(analyzer, (SymbolAnalysisContext context) =>
		{
			foreach (IParameterSymbol item in context.Symbol.Kind switch
			{
				SymbolKind.Method => ((IMethodSymbol)context.Symbol).Parameters, 
				SymbolKind.Property => ((IPropertySymbol)context.Symbol).Parameters, 
				SymbolKind.NamedType => ((INamedTypeSymbol)context.Symbol).DelegateInvokeMethod?.Parameters ?? ImmutableArray.Create<IParameterSymbol>(), 
				_ => throw new ArgumentException($"{context.Symbol.Kind} is not supported.", "context"), 
			})
			{
				if (!item.IsImplicitlyDeclared)
				{
					action(new SymbolAnalysisContext(item, context.Compilation, context.Options, ((SymbolAnalysisContext)context).ReportDiagnostic, context.IsSupportedDiagnostic, context.CancellationToken));
				}
			}
		}, ImmutableArray.Create(SymbolKind.Method, SymbolKind.Property, SymbolKind.NamedType));
	}

	public void RegisterCodeBlockStartAction<TLanguageKindEnum>(DiagnosticAnalyzer analyzer, Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) where TLanguageKindEnum : struct
	{
		CodeBlockStartAnalyzerAction<TLanguageKindEnum> codeBlockStartAnalyzerAction = new CodeBlockStartAnalyzerAction<TLanguageKindEnum>(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddCodeBlockStartAction(codeBlockStartAnalyzerAction);
		_codeBlockStartActions = _codeBlockStartActions.Add(codeBlockStartAnalyzerAction);
	}

	public void RegisterCodeBlockEndAction(DiagnosticAnalyzer analyzer, Action<CodeBlockAnalysisContext> action)
	{
		CodeBlockAnalyzerAction codeBlockAnalyzerAction = new CodeBlockAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddCodeBlockEndAction(codeBlockAnalyzerAction);
		_codeBlockEndActions = _codeBlockEndActions.Add(codeBlockAnalyzerAction);
	}

	public void RegisterCodeBlockAction(DiagnosticAnalyzer analyzer, Action<CodeBlockAnalysisContext> action)
	{
		CodeBlockAnalyzerAction codeBlockAnalyzerAction = new CodeBlockAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddCodeBlockAction(codeBlockAnalyzerAction);
		_codeBlockActions = _codeBlockActions.Add(codeBlockAnalyzerAction);
	}

	public void RegisterSyntaxNodeAction<TLanguageKindEnum>(DiagnosticAnalyzer analyzer, Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds) where TLanguageKindEnum : struct
	{
		SyntaxNodeAnalyzerAction<TLanguageKindEnum> syntaxNodeAnalyzerAction = new SyntaxNodeAnalyzerAction<TLanguageKindEnum>(action, syntaxKinds, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddSyntaxNodeAction(syntaxNodeAnalyzerAction);
		_syntaxNodeActions = _syntaxNodeActions.Add(syntaxNodeAnalyzerAction);
	}

	public void RegisterOperationBlockStartAction(DiagnosticAnalyzer analyzer, Action<OperationBlockStartAnalysisContext> action)
	{
		OperationBlockStartAnalyzerAction operationBlockStartAnalyzerAction = new OperationBlockStartAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddOperationBlockStartAction(operationBlockStartAnalyzerAction);
		_operationBlockStartActions = _operationBlockStartActions.Add(operationBlockStartAnalyzerAction);
	}

	public void RegisterOperationBlockEndAction(DiagnosticAnalyzer analyzer, Action<OperationBlockAnalysisContext> action)
	{
		OperationBlockAnalyzerAction operationBlockAnalyzerAction = new OperationBlockAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddOperationBlockEndAction(operationBlockAnalyzerAction);
		_operationBlockEndActions = _operationBlockEndActions.Add(operationBlockAnalyzerAction);
	}

	public void RegisterOperationBlockAction(DiagnosticAnalyzer analyzer, Action<OperationBlockAnalysisContext> action)
	{
		OperationBlockAnalyzerAction operationBlockAnalyzerAction = new OperationBlockAnalyzerAction(action, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddOperationBlockAction(operationBlockAnalyzerAction);
		_operationBlockActions = _operationBlockActions.Add(operationBlockAnalyzerAction);
	}

	public void RegisterOperationAction(DiagnosticAnalyzer analyzer, Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		OperationAnalyzerAction operationAnalyzerAction = new OperationAnalyzerAction(action, operationKinds, analyzer);
		GetOrCreateAnalyzerActions(analyzer).AddOperationAction(operationAnalyzerAction);
		_operationActions = _operationActions.Add(operationAnalyzerAction);
	}

	protected AnalyzerActions GetOrCreateAnalyzerActions(DiagnosticAnalyzer analyzer)
	{
		if (!_analyzerActions.TryGetValue(analyzer, out var value))
		{
			value = new AnalyzerActions();
			_analyzerActions[analyzer] = value;
		}
		return value;
	}
}
