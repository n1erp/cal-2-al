using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

public abstract class AnalysisContext
{
	public abstract void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action);

	public abstract void RegisterCompilationAction(Action<CompilationAnalysisContext> action);

	public abstract void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action);

	public void RegisterSymbolAction(Action<SymbolAnalysisContext> action, params SymbolKind[] symbolKinds)
	{
		RegisterSymbolAction(action, symbolKinds.AsImmutableOrEmpty());
	}

	public abstract void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds);

	public abstract void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) where TLanguageKindEnum : struct;

	public abstract void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action);

	public abstract void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action);

	public void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct
	{
		RegisterSyntaxNodeAction(action, syntaxKinds.AsImmutableOrEmpty());
	}

	public abstract void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds) where TLanguageKindEnum : struct;

	public virtual void RegisterOperationBlockStartAction(Action<OperationBlockStartAnalysisContext> action)
	{
		throw new NotImplementedException();
	}

	public virtual void RegisterOperationBlockAction(Action<OperationBlockAnalysisContext> action)
	{
		throw new NotImplementedException();
	}

	public void RegisterOperationAction(Action<OperationAnalysisContext> action, params OperationKind[] operationKinds)
	{
		RegisterOperationAction(action, operationKinds.AsImmutableOrEmpty());
	}

	public virtual void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		throw new NotImplementedException();
	}

	public virtual void EnableConcurrentExecution()
	{
		throw new NotImplementedException();
	}

	public virtual void ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags analysisMode)
	{
		throw new NotImplementedException();
	}

	public bool TryGetValue<TValue>(SourceText text, SourceTextValueProvider<TValue> valueProvider, out TValue value)
	{
		return TryGetValue(text, valueProvider.CoreValueProvider, out value);
	}

	private bool TryGetValue<TKey, TValue>(TKey key, AnalysisValueProvider<TKey, TValue> valueProvider, out TValue value) where TKey : class
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(key, valueProvider);
		return valueProvider.TryGetValue(key, out value);
	}
}
