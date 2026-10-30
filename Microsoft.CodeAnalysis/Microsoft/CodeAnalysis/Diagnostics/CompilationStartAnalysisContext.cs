using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Diagnostics;

public abstract class CompilationStartAnalysisContext
{
	private readonly Compilation _compilation;

	private readonly AnalyzerOptions _options;

	private readonly CancellationToken _cancellationToken;

	public Compilation Compilation => _compilation;

	public AnalyzerOptions Options => _options;

	public CancellationToken CancellationToken => _cancellationToken;

	protected CompilationStartAnalysisContext(Compilation compilation, AnalyzerOptions options, CancellationToken cancellationToken)
	{
		_compilation = compilation;
		_options = options;
		_cancellationToken = cancellationToken;
	}

	public abstract void RegisterCompilationEndAction(Action<CompilationAnalysisContext> action);

	public abstract void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action);

	public void RegisterSymbolAction(Action<SymbolAnalysisContext> action, params SymbolKind[] symbolKinds)
	{
		RegisterSymbolAction(action, symbolKinds.AsImmutableOrEmpty());
	}

	public abstract void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds);

	public abstract void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action) where TLanguageKindEnum : struct;

	public abstract void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action);

	public virtual void RegisterOperationBlockStartAction(Action<OperationBlockStartAnalysisContext> action)
	{
		throw new NotImplementedException();
	}

	public virtual void RegisterOperationBlockAction(Action<OperationBlockAnalysisContext> action)
	{
		throw new NotImplementedException();
	}

	public abstract void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action);

	public void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, params TLanguageKindEnum[] syntaxKinds) where TLanguageKindEnum : struct
	{
		RegisterSyntaxNodeAction(action, syntaxKinds.AsImmutableOrEmpty());
	}

	public abstract void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds) where TLanguageKindEnum : struct;

	public void RegisterOperationAction(Action<OperationAnalysisContext> action, params OperationKind[] operationKinds)
	{
		RegisterOperationAction(action, operationKinds.AsImmutableOrEmpty());
	}

	public virtual void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		throw new NotImplementedException();
	}

	public bool TryGetValue<TValue>(SourceText text, SourceTextValueProvider<TValue> valueProvider, out TValue value)
	{
		return TryGetValue(text, valueProvider.CoreValueProvider, out value);
	}

	public bool TryGetValue<TValue>(SyntaxTree tree, SyntaxTreeValueProvider<TValue> valueProvider, out TValue value)
	{
		return TryGetValue(tree, valueProvider.CoreValueProvider, out value);
	}

	private bool TryGetValue<TKey, TValue>(TKey key, AnalysisValueProvider<TKey, TValue> valueProvider, out TValue value) where TKey : class
	{
		DiagnosticAnalysisContextHelpers.VerifyArguments(key, valueProvider);
		return TryGetValueCore(key, valueProvider, out value);
	}

	internal virtual bool TryGetValueCore<TKey, TValue>(TKey key, AnalysisValueProvider<TKey, TValue> valueProvider, out TValue value) where TKey : class
	{
		throw new NotImplementedException();
	}
}
