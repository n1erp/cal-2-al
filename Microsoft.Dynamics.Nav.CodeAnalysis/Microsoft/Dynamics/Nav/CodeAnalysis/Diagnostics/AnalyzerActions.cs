using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class AnalyzerActions
{
	private ImmutableArray<CompilationStartAnalyzerAction> compilationStartActions = ImmutableArray<CompilationStartAnalyzerAction>.Empty;

	private ImmutableArray<CompilationAnalyzerAction> compilationEndActions = ImmutableArray<CompilationAnalyzerAction>.Empty;

	private ImmutableArray<CompilationAnalyzerAction> compilationActions = ImmutableArray<CompilationAnalyzerAction>.Empty;

	private ImmutableArray<SyntaxTreeAnalyzerAction> syntaxTreeActions = ImmutableArray<SyntaxTreeAnalyzerAction>.Empty;

	private ImmutableArray<SemanticModelAnalyzerAction> semanticModelActions = ImmutableArray<SemanticModelAnalyzerAction>.Empty;

	private ImmutableArray<SymbolAnalyzerAction> symbolActions = ImmutableArray<SymbolAnalyzerAction>.Empty;

	private ImmutableArray<AnalyzerAction> codeBlockStartActions = ImmutableArray<AnalyzerAction>.Empty;

	private ImmutableArray<CodeBlockAnalyzerAction> codeBlockEndActions = ImmutableArray<CodeBlockAnalyzerAction>.Empty;

	private ImmutableArray<CodeBlockAnalyzerAction> codeBlockActions = ImmutableArray<CodeBlockAnalyzerAction>.Empty;

	private ImmutableArray<OperationBlockStartAnalyzerAction> operationBlockStartActions = ImmutableArray<OperationBlockStartAnalyzerAction>.Empty;

	private ImmutableArray<OperationBlockAnalyzerAction> operationBlockEndActions = ImmutableArray<OperationBlockAnalyzerAction>.Empty;

	private ImmutableArray<OperationBlockAnalyzerAction> operationBlockActions = ImmutableArray<OperationBlockAnalyzerAction>.Empty;

	private ImmutableArray<OperationAnalyzerAction> operationActions = ImmutableArray<OperationAnalyzerAction>.Empty;

	private ImmutableArray<AnalyzerAction> syntaxNodeActions = ImmutableArray<AnalyzerAction>.Empty;

	internal static readonly AnalyzerActions Empty = new AnalyzerActions();

	public int CompilationStartActionsCount => compilationStartActions.Length;

	public int CompilationEndActionsCount => compilationEndActions.Length;

	public int CompilationActionsCount => compilationActions.Length;

	public int SyntaxTreeActionsCount => syntaxTreeActions.Length;

	public int SemanticModelActionsCount => semanticModelActions.Length;

	public int SymbolActionsCount => symbolActions.Length;

	public int SyntaxNodeActionsCount => syntaxNodeActions.Length;

	public int CodeBlockStartActionsCount => codeBlockStartActions.Length;

	public int CodeBlockEndActionsCount => codeBlockEndActions.Length;

	public int CodeBlockActionsCount => codeBlockActions.Length;

	public int OperationActionsCount => operationActions.Length;

	public int OperationBlockStartActionsCount => operationBlockStartActions.Length;

	public int OperationBlockEndActionsCount => operationBlockEndActions.Length;

	public int OperationBlockActionsCount => operationBlockActions.Length;

	internal ImmutableArray<OperationBlockAnalyzerAction> OperationBlockActions => operationBlockActions;

	internal ImmutableArray<OperationBlockAnalyzerAction> OperationBlockEndActions => operationBlockEndActions;

	internal ImmutableArray<OperationBlockStartAnalyzerAction> OperationBlockStartActions => operationBlockStartActions;

	internal ImmutableArray<OperationAnalyzerAction> OperationActions => operationActions;

	internal ImmutableArray<CompilationStartAnalyzerAction> CompilationStartActions => compilationStartActions;

	internal ImmutableArray<CompilationAnalyzerAction> CompilationEndActions => compilationEndActions;

	internal ImmutableArray<CompilationAnalyzerAction> CompilationActions => compilationActions;

	internal ImmutableArray<SyntaxTreeAnalyzerAction> SyntaxTreeActions => syntaxTreeActions;

	internal ImmutableArray<SemanticModelAnalyzerAction> SemanticModelActions => semanticModelActions;

	internal ImmutableArray<SymbolAnalyzerAction> SymbolActions => symbolActions;

	internal ImmutableArray<CodeBlockAnalyzerAction> CodeBlockEndActions => codeBlockEndActions;

	internal ImmutableArray<CodeBlockAnalyzerAction> CodeBlockActions => codeBlockActions;

	internal AnalyzerActions()
	{
	}

	internal ImmutableArray<CodeBlockStartAnalyzerAction> GetCodeBlockStartActions()
	{
		return codeBlockStartActions.OfType<CodeBlockStartAnalyzerAction>().ToImmutableArray();
	}

	internal ImmutableArray<SyntaxNodeAnalyzerAction> GetSyntaxNodeActions()
	{
		return syntaxNodeActions.OfType<SyntaxNodeAnalyzerAction>().ToImmutableArray();
	}

	internal void AddCompilationStartAction(CompilationStartAnalyzerAction action)
	{
		compilationStartActions = compilationStartActions.Add(action);
	}

	internal void AddCompilationEndAction(CompilationAnalyzerAction action)
	{
		compilationEndActions = compilationEndActions.Add(action);
	}

	internal void AddCompilationAction(CompilationAnalyzerAction action)
	{
		compilationActions = compilationActions.Add(action);
	}

	internal void AddSyntaxTreeAction(SyntaxTreeAnalyzerAction action)
	{
		syntaxTreeActions = syntaxTreeActions.Add(action);
	}

	internal void AddSemanticModelAction(SemanticModelAnalyzerAction action)
	{
		semanticModelActions = semanticModelActions.Add(action);
	}

	internal void AddSymbolAction(SymbolAnalyzerAction action)
	{
		symbolActions = symbolActions.Add(action);
	}

	internal void AddCodeBlockStartAction(CodeBlockStartAnalyzerAction action)
	{
		codeBlockStartActions = codeBlockStartActions.Add(action);
	}

	internal void AddCodeBlockEndAction(CodeBlockAnalyzerAction action)
	{
		codeBlockEndActions = codeBlockEndActions.Add(action);
	}

	internal void AddCodeBlockAction(CodeBlockAnalyzerAction action)
	{
		codeBlockActions = codeBlockActions.Add(action);
	}

	internal void AddOperationBlockStartAction(OperationBlockStartAnalyzerAction action)
	{
		operationBlockStartActions = operationBlockStartActions.Add(action);
	}

	internal void AddOperationBlockAction(OperationBlockAnalyzerAction action)
	{
		operationBlockActions = operationBlockActions.Add(action);
	}

	internal void AddOperationBlockEndAction(OperationBlockAnalyzerAction action)
	{
		operationBlockEndActions = operationBlockEndActions.Add(action);
	}

	internal void AddOperationAction(OperationAnalyzerAction action)
	{
		operationActions = operationActions.Add(action);
	}

	internal void AddSyntaxNodeAction(SyntaxNodeAnalyzerAction action)
	{
		syntaxNodeActions = syntaxNodeActions.Add(action);
	}

	public AnalyzerActions Append(AnalyzerActions otherActions)
	{
		if (otherActions == null)
		{
			throw new ArgumentNullException("otherActions");
		}
		return new AnalyzerActions
		{
			compilationStartActions = compilationStartActions.AddRange(otherActions.compilationStartActions),
			compilationEndActions = compilationEndActions.AddRange(otherActions.compilationEndActions),
			compilationActions = compilationActions.AddRange(otherActions.compilationActions),
			syntaxTreeActions = syntaxTreeActions.AddRange(otherActions.syntaxTreeActions),
			semanticModelActions = semanticModelActions.AddRange(otherActions.semanticModelActions),
			symbolActions = symbolActions.AddRange(otherActions.symbolActions),
			codeBlockStartActions = codeBlockStartActions.AddRange(otherActions.codeBlockStartActions),
			codeBlockEndActions = codeBlockEndActions.AddRange(otherActions.codeBlockEndActions),
			codeBlockActions = codeBlockActions.AddRange(otherActions.codeBlockActions),
			syntaxNodeActions = syntaxNodeActions.AddRange(otherActions.syntaxNodeActions),
			operationActions = operationActions.AddRange(otherActions.operationActions),
			operationBlockStartActions = operationBlockStartActions.AddRange(otherActions.operationBlockStartActions),
			operationBlockEndActions = operationBlockEndActions.AddRange(otherActions.operationBlockEndActions),
			operationBlockActions = operationBlockActions.AddRange(otherActions.operationBlockActions)
		};
	}
}
