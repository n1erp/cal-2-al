using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class AnalyzerActions
{
	private ImmutableArray<CompilationStartAnalyzerAction> _compilationStartActions = ImmutableArray<CompilationStartAnalyzerAction>.Empty;

	private ImmutableArray<CompilationAnalyzerAction> _compilationEndActions = ImmutableArray<CompilationAnalyzerAction>.Empty;

	private ImmutableArray<CompilationAnalyzerAction> _compilationActions = ImmutableArray<CompilationAnalyzerAction>.Empty;

	private ImmutableArray<SyntaxTreeAnalyzerAction> _syntaxTreeActions = ImmutableArray<SyntaxTreeAnalyzerAction>.Empty;

	private ImmutableArray<SemanticModelAnalyzerAction> _semanticModelActions = ImmutableArray<SemanticModelAnalyzerAction>.Empty;

	private ImmutableArray<SymbolAnalyzerAction> _symbolActions = ImmutableArray<SymbolAnalyzerAction>.Empty;

	private ImmutableArray<AnalyzerAction> _codeBlockStartActions = ImmutableArray<AnalyzerAction>.Empty;

	private ImmutableArray<CodeBlockAnalyzerAction> _codeBlockEndActions = ImmutableArray<CodeBlockAnalyzerAction>.Empty;

	private ImmutableArray<CodeBlockAnalyzerAction> _codeBlockActions = ImmutableArray<CodeBlockAnalyzerAction>.Empty;

	private ImmutableArray<OperationBlockStartAnalyzerAction> _operationBlockStartActions = ImmutableArray<OperationBlockStartAnalyzerAction>.Empty;

	private ImmutableArray<OperationBlockAnalyzerAction> _operationBlockEndActions = ImmutableArray<OperationBlockAnalyzerAction>.Empty;

	private ImmutableArray<OperationBlockAnalyzerAction> _operationBlockActions = ImmutableArray<OperationBlockAnalyzerAction>.Empty;

	private ImmutableArray<AnalyzerAction> _syntaxNodeActions = ImmutableArray<AnalyzerAction>.Empty;

	private ImmutableArray<OperationAnalyzerAction> _operationActions = ImmutableArray<OperationAnalyzerAction>.Empty;

	private bool _concurrent;

	public int CompilationStartActionsCount => _compilationStartActions.Length;

	public int CompilationEndActionsCount => _compilationEndActions.Length;

	public int CompilationActionsCount => _compilationActions.Length;

	public int SyntaxTreeActionsCount => _syntaxTreeActions.Length;

	public int SemanticModelActionsCount => _semanticModelActions.Length;

	public int SymbolActionsCount => _symbolActions.Length;

	public int SyntaxNodeActionsCount => _syntaxNodeActions.Length;

	public int OperationActionsCount => _operationActions.Length;

	public int OperationBlockStartActionsCount => _operationBlockStartActions.Length;

	public int OperationBlockEndActionsCount => _operationBlockEndActions.Length;

	public int OperationBlockActionsCount => _operationBlockActions.Length;

	public int CodeBlockStartActionsCount => _codeBlockStartActions.Length;

	public int CodeBlockEndActionsCount => _codeBlockEndActions.Length;

	public int CodeBlockActionsCount => _codeBlockActions.Length;

	public bool Concurrent => _concurrent;

	internal ImmutableArray<CompilationStartAnalyzerAction> CompilationStartActions => _compilationStartActions;

	internal ImmutableArray<CompilationAnalyzerAction> CompilationEndActions => _compilationEndActions;

	internal ImmutableArray<CompilationAnalyzerAction> CompilationActions => _compilationActions;

	internal ImmutableArray<SyntaxTreeAnalyzerAction> SyntaxTreeActions => _syntaxTreeActions;

	internal ImmutableArray<SemanticModelAnalyzerAction> SemanticModelActions => _semanticModelActions;

	internal ImmutableArray<SymbolAnalyzerAction> SymbolActions => _symbolActions;

	internal ImmutableArray<CodeBlockAnalyzerAction> CodeBlockEndActions => _codeBlockEndActions;

	internal ImmutableArray<CodeBlockAnalyzerAction> CodeBlockActions => _codeBlockActions;

	internal ImmutableArray<OperationBlockAnalyzerAction> OperationBlockActions => _operationBlockActions;

	internal ImmutableArray<OperationBlockAnalyzerAction> OperationBlockEndActions => _operationBlockEndActions;

	internal ImmutableArray<OperationBlockStartAnalyzerAction> OperationBlockStartActions => _operationBlockStartActions;

	internal ImmutableArray<OperationAnalyzerAction> OperationActions => _operationActions;

	internal AnalyzerActions()
	{
	}

	internal ImmutableArray<CodeBlockStartAnalyzerAction<TLanguageKindEnum>> GetCodeBlockStartActions<TLanguageKindEnum>() where TLanguageKindEnum : struct
	{
		return _codeBlockStartActions.OfType<CodeBlockStartAnalyzerAction<TLanguageKindEnum>>().ToImmutableArray();
	}

	internal ImmutableArray<SyntaxNodeAnalyzerAction<TLanguageKindEnum>> GetSyntaxNodeActions<TLanguageKindEnum>() where TLanguageKindEnum : struct
	{
		return _syntaxNodeActions.OfType<SyntaxNodeAnalyzerAction<TLanguageKindEnum>>().ToImmutableArray();
	}

	internal void AddCompilationStartAction(CompilationStartAnalyzerAction action)
	{
		_compilationStartActions = _compilationStartActions.Add(action);
	}

	internal void AddCompilationEndAction(CompilationAnalyzerAction action)
	{
		_compilationEndActions = _compilationEndActions.Add(action);
	}

	internal void AddCompilationAction(CompilationAnalyzerAction action)
	{
		_compilationActions = _compilationActions.Add(action);
	}

	internal void AddSyntaxTreeAction(SyntaxTreeAnalyzerAction action)
	{
		_syntaxTreeActions = _syntaxTreeActions.Add(action);
	}

	internal void AddSemanticModelAction(SemanticModelAnalyzerAction action)
	{
		_semanticModelActions = _semanticModelActions.Add(action);
	}

	internal void AddSymbolAction(SymbolAnalyzerAction action)
	{
		_symbolActions = _symbolActions.Add(action);
	}

	internal void AddCodeBlockStartAction<TLanguageKindEnum>(CodeBlockStartAnalyzerAction<TLanguageKindEnum> action) where TLanguageKindEnum : struct
	{
		_codeBlockStartActions = _codeBlockStartActions.Add(action);
	}

	internal void AddCodeBlockEndAction(CodeBlockAnalyzerAction action)
	{
		_codeBlockEndActions = _codeBlockEndActions.Add(action);
	}

	internal void AddCodeBlockAction(CodeBlockAnalyzerAction action)
	{
		_codeBlockActions = _codeBlockActions.Add(action);
	}

	internal void AddSyntaxNodeAction<TLanguageKindEnum>(SyntaxNodeAnalyzerAction<TLanguageKindEnum> action) where TLanguageKindEnum : struct
	{
		_syntaxNodeActions = _syntaxNodeActions.Add(action);
	}

	internal void AddOperationBlockStartAction(OperationBlockStartAnalyzerAction action)
	{
		_operationBlockStartActions = _operationBlockStartActions.Add(action);
	}

	internal void AddOperationBlockAction(OperationBlockAnalyzerAction action)
	{
		_operationBlockActions = _operationBlockActions.Add(action);
	}

	internal void AddOperationBlockEndAction(OperationBlockAnalyzerAction action)
	{
		_operationBlockEndActions = _operationBlockEndActions.Add(action);
	}

	internal void AddOperationAction(OperationAnalyzerAction action)
	{
		_operationActions = _operationActions.Add(action);
	}

	internal void EnableConcurrentExecution()
	{
		_concurrent = true;
	}

	public AnalyzerActions Append(AnalyzerActions otherActions)
	{
		if (otherActions == null)
		{
			throw new ArgumentNullException("otherActions");
		}
		AnalyzerActions analyzerActions = new AnalyzerActions
		{
			_compilationStartActions = _compilationStartActions.AddRange(otherActions._compilationStartActions),
			_compilationEndActions = _compilationEndActions.AddRange(otherActions._compilationEndActions),
			_compilationActions = _compilationActions.AddRange(otherActions._compilationActions),
			_syntaxTreeActions = _syntaxTreeActions.AddRange(otherActions._syntaxTreeActions),
			_semanticModelActions = _semanticModelActions.AddRange(otherActions._semanticModelActions),
			_symbolActions = _symbolActions.AddRange(otherActions._symbolActions),
			_codeBlockStartActions = _codeBlockStartActions.AddRange(otherActions._codeBlockStartActions),
			_codeBlockEndActions = _codeBlockEndActions.AddRange(otherActions._codeBlockEndActions),
			_codeBlockActions = _codeBlockActions.AddRange(otherActions._codeBlockActions),
			_syntaxNodeActions = _syntaxNodeActions.AddRange(otherActions._syntaxNodeActions),
			_operationActions = _operationActions.AddRange(otherActions._operationActions),
			_operationBlockStartActions = _operationBlockStartActions.AddRange(otherActions._operationBlockStartActions),
			_operationBlockEndActions = _operationBlockEndActions.AddRange(otherActions._operationBlockEndActions),
			_operationBlockActions = _operationBlockActions.AddRange(otherActions._operationBlockActions)
		};
		analyzerActions._concurrent = analyzerActions._concurrent || otherActions.Concurrent;
		return analyzerActions;
	}
}
