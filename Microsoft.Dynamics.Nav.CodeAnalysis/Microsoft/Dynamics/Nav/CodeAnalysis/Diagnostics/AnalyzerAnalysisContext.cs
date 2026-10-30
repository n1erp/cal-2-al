using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal sealed class AnalyzerAnalysisContext : AnalysisContext
{
	private readonly DiagnosticAnalyzer analyzer;

	private readonly HostSessionStartAnalysisScope scope;

	public AnalyzerAnalysisContext(DiagnosticAnalyzer analyzer, HostSessionStartAnalysisScope scope)
	{
		this.analyzer = analyzer;
		this.scope = scope;
	}

	public override void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		scope.RegisterCompilationStartAction(analyzer, action);
	}

	public override void RegisterCompilationAction(Action<CompilationAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		scope.RegisterCompilationAction(analyzer, action);
	}

	public override void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		scope.RegisterSyntaxTreeAction(analyzer, action);
	}

	public override void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		scope.RegisterSemanticModelAction(analyzer, action);
	}

	public override void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action, symbolKinds);
		scope.RegisterSymbolAction(analyzer, action, symbolKinds);
	}

	public override void RegisterCodeBlockStartAction(Action<CodeBlockStartAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		scope.RegisterCodeBlockStartAction(analyzer, action);
	}

	public override void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		scope.RegisterCodeBlockAction(analyzer, action);
	}

	public override void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<SyntaxKind> syntaxKinds)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action, syntaxKinds);
		scope.RegisterSyntaxNodeAction(analyzer, action, syntaxKinds);
	}

	public override void RegisterOperationBlockStartAction(Action<OperationBlockStartAnalysisContext> action)
	{
		RegisterOperationBlockStartActionInternal(action);
	}

	internal override void RegisterOperationBlockStartActionInternal(Action<OperationBlockStartAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		scope.RegisterOperationBlockStartAction(analyzer, action);
	}

	public override void RegisterOperationBlockAction(Action<OperationBlockAnalysisContext> action)
	{
		RegisterOperationBlockActionInternal(action);
	}

	internal override void RegisterOperationBlockActionInternal(Action<OperationBlockAnalysisContext> action)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action);
		scope.RegisterOperationBlockAction(analyzer, action);
	}

	public override void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		RegisterOperationActionImmutableArrayInternal(action, operationKinds);
	}

	internal override void RegisterOperationActionImmutableArrayInternal(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(action, operationKinds);
		scope.RegisterOperationAction(analyzer, action, operationKinds);
	}
}
