using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal sealed class AnalyzerAnalysisContext : AnalysisContext
{
	private readonly DiagnosticAnalyzer _analyzer;

	private readonly HostSessionStartAnalysisScope _scope;

	public AnalyzerAnalysisContext(DiagnosticAnalyzer analyzer, HostSessionStartAnalysisScope scope)
	{
		_analyzer = analyzer;
		_scope = scope;
	}

	public override void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterCompilationStartAction(_analyzer, action);
	}

	public override void RegisterCompilationAction(Action<CompilationAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterCompilationAction(_analyzer, action);
	}

	public override void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterSyntaxTreeAction(_analyzer, action);
	}

	public override void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterSemanticModelAction(_analyzer, action);
	}

	public override void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action, symbolKinds);
		_scope.RegisterSymbolAction(_analyzer, action, symbolKinds);
	}

	public override void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterCodeBlockStartAction(_analyzer, action);
	}

	public override void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterCodeBlockAction(_analyzer, action);
	}

	public override void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action, syntaxKinds);
		_scope.RegisterSyntaxNodeAction(_analyzer, action, syntaxKinds);
	}

	public override void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action, operationKinds);
		_scope.RegisterOperationAction(_analyzer, action, operationKinds);
	}

	public override void RegisterOperationBlockStartAction(Action<OperationBlockStartAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterOperationBlockStartAction(_analyzer, action);
	}

	public override void RegisterOperationBlockAction(Action<OperationBlockAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		_scope.RegisterOperationBlockAction(_analyzer, action);
	}

	public override void EnableConcurrentExecution()
	{
		_scope.EnableConcurrentExecution(_analyzer);
	}

	public override void ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags mode)
	{
		_scope.ConfigureGeneratedCodeAnalysis(_analyzer, mode);
	}
}
