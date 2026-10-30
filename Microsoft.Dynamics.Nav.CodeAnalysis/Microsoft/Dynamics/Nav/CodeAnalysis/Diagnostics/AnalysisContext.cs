using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

public abstract class AnalysisContext
{
	public abstract void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action);

	public abstract void RegisterCompilationAction(Action<CompilationAnalysisContext> action);

	public abstract void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action);

	public void RegisterSymbolAction(Action<SymbolAnalysisContext> action, params SymbolKind[] symbolKinds)
	{
		RegisterSymbolAction(action, symbolKinds.AsImmutableOrNull());
	}

	public abstract void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds);

	[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
	public abstract void RegisterCodeBlockStartAction(Action<CodeBlockStartAnalysisContext> action);

	public abstract void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action);

	public abstract void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action);

	public void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, params SyntaxKind[] syntaxKinds)
	{
		RegisterSyntaxNodeAction(action, syntaxKinds.AsImmutableOrNull());
	}

	public abstract void RegisterSyntaxNodeAction(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<SyntaxKind> syntaxKinds);

	public void RegisterOperationAction(Action<OperationAnalysisContext> action, params OperationKind[] operationKinds)
	{
		RegisterOperationAction(action, operationKinds.AsImmutableOrEmpty());
	}

	internal void RegisterOperationActionParamsArrayInternal(Action<OperationAnalysisContext> action, params OperationKind[] operationKinds)
	{
		RegisterOperationActionImmutableArrayInternal(action, operationKinds.AsImmutableOrEmpty());
	}

	public virtual void RegisterOperationAction(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		throw new NotImplementedException();
	}

	internal virtual void RegisterOperationActionImmutableArrayInternal(Action<OperationAnalysisContext> action, ImmutableArray<OperationKind> operationKinds)
	{
		throw new NotImplementedException();
	}

	public virtual void RegisterOperationBlockStartAction(Action<OperationBlockStartAnalysisContext> action)
	{
		throw new NotImplementedException();
	}

	internal virtual void RegisterOperationBlockStartActionInternal(Action<OperationBlockStartAnalysisContext> action)
	{
		throw new NotImplementedException();
	}

	public virtual void RegisterOperationBlockAction(Action<OperationBlockAnalysisContext> action)
	{
		throw new NotImplementedException();
	}

	internal virtual void RegisterOperationBlockActionInternal(Action<OperationBlockAnalysisContext> action)
	{
		throw new NotImplementedException();
	}

	internal void RegisterSymbolAction(Action analyzeModule, SymbolKind module)
	{
		throw new NotImplementedException();
	}
}
