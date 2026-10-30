using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis;

public abstract class SemanticModel
{
	public abstract string Language { get; }

	public Compilation Compilation => CompilationCore;

	protected abstract Compilation CompilationCore { get; }

	public SyntaxTree SyntaxTree => SyntaxTreeCore;

	protected abstract SyntaxTree SyntaxTreeCore { get; }

	public virtual bool IgnoresAccessibility => false;

	public abstract bool IsSpeculativeSemanticModel { get; }

	public abstract int OriginalPositionForSpeculation { get; }

	public SemanticModel ParentModel => ParentModelCore;

	protected abstract SemanticModel ParentModelCore { get; }

	internal abstract SemanticModel ContainingModelOrSelf { get; }

	internal SyntaxNode Root => RootCore;

	protected abstract SyntaxNode RootCore { get; }

	public IOperation GetOperation(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		try
		{
			return GetOperationCore(node, cancellationToken);
		}
		catch (Exception exception) when (FatalError.ReportWithoutCrashUnlessCanceled(exception))
		{
		}
		return null;
	}

	protected abstract IOperation GetOperationCore(SyntaxNode node, CancellationToken cancellationToken);

	internal SymbolInfo GetSymbolInfo(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetSymbolInfoCore(node, cancellationToken);
	}

	protected abstract SymbolInfo GetSymbolInfoCore(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken));

	internal SymbolInfo GetSpeculativeSymbolInfo(int position, SyntaxNode expression, SpeculativeBindingOption bindingOption)
	{
		return GetSpeculativeSymbolInfoCore(position, expression, bindingOption);
	}

	protected abstract SymbolInfo GetSpeculativeSymbolInfoCore(int position, SyntaxNode expression, SpeculativeBindingOption bindingOption);

	internal TypeInfo GetSpeculativeTypeInfo(int position, SyntaxNode expression, SpeculativeBindingOption bindingOption)
	{
		return GetSpeculativeTypeInfoCore(position, expression, bindingOption);
	}

	protected abstract TypeInfo GetSpeculativeTypeInfoCore(int position, SyntaxNode expression, SpeculativeBindingOption bindingOption);

	internal TypeInfo GetTypeInfo(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetTypeInfoCore(node, cancellationToken);
	}

	protected abstract TypeInfo GetTypeInfoCore(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken));

	internal IAliasSymbol GetAliasInfo(SyntaxNode nameSyntax, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetAliasInfoCore(nameSyntax, cancellationToken);
	}

	protected abstract IAliasSymbol GetAliasInfoCore(SyntaxNode nameSyntax, CancellationToken cancellationToken = default(CancellationToken));

	internal IAliasSymbol GetSpeculativeAliasInfo(int position, SyntaxNode nameSyntax, SpeculativeBindingOption bindingOption)
	{
		return GetSpeculativeAliasInfoCore(position, nameSyntax, bindingOption);
	}

	protected abstract IAliasSymbol GetSpeculativeAliasInfoCore(int position, SyntaxNode nameSyntax, SpeculativeBindingOption bindingOption);

	public abstract ImmutableArray<Diagnostic> GetSyntaxDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ImmutableArray<Diagnostic> GetDeclarationDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ImmutableArray<Diagnostic> GetMethodBodyDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken));

	public abstract ImmutableArray<Diagnostic> GetDiagnostics(TextSpan? span = null, CancellationToken cancellationToken = default(CancellationToken));

	internal ISymbol GetDeclaredSymbolForNode(SyntaxNode declaration, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDeclaredSymbolCore(declaration, cancellationToken);
	}

	protected abstract ISymbol GetDeclaredSymbolCore(SyntaxNode declaration, CancellationToken cancellationToken = default(CancellationToken));

	internal ImmutableArray<ISymbol> GetDeclaredSymbolsForNode(SyntaxNode declaration, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetDeclaredSymbolsCore(declaration, cancellationToken);
	}

	protected abstract ImmutableArray<ISymbol> GetDeclaredSymbolsCore(SyntaxNode declaration, CancellationToken cancellationToken = default(CancellationToken));

	public ImmutableArray<ISymbol> LookupSymbols(int position, INamespaceOrTypeSymbol container = null, string name = null, bool includeReducedExtensionMethods = false)
	{
		return LookupSymbolsCore(position, container, name, includeReducedExtensionMethods);
	}

	protected abstract ImmutableArray<ISymbol> LookupSymbolsCore(int position, INamespaceOrTypeSymbol container, string name, bool includeReducedExtensionMethods);

	public ImmutableArray<ISymbol> LookupBaseMembers(int position, string name = null)
	{
		return LookupBaseMembersCore(position, name);
	}

	protected abstract ImmutableArray<ISymbol> LookupBaseMembersCore(int position, string name);

	public ImmutableArray<ISymbol> LookupStaticMembers(int position, INamespaceOrTypeSymbol container = null, string name = null)
	{
		return LookupStaticMembersCore(position, container, name);
	}

	protected abstract ImmutableArray<ISymbol> LookupStaticMembersCore(int position, INamespaceOrTypeSymbol container, string name);

	public ImmutableArray<ISymbol> LookupNamespacesAndTypes(int position, INamespaceOrTypeSymbol container = null, string name = null)
	{
		return LookupNamespacesAndTypesCore(position, container, name);
	}

	protected abstract ImmutableArray<ISymbol> LookupNamespacesAndTypesCore(int position, INamespaceOrTypeSymbol container, string name);

	public ImmutableArray<ISymbol> LookupLabels(int position, string name = null)
	{
		return LookupLabelsCore(position, name);
	}

	protected abstract ImmutableArray<ISymbol> LookupLabelsCore(int position, string name);

	internal ControlFlowAnalysis AnalyzeControlFlow(SyntaxNode firstStatement, SyntaxNode lastStatement)
	{
		return AnalyzeControlFlowCore(firstStatement, lastStatement);
	}

	protected abstract ControlFlowAnalysis AnalyzeControlFlowCore(SyntaxNode firstStatement, SyntaxNode lastStatement);

	internal ControlFlowAnalysis AnalyzeControlFlow(SyntaxNode statement)
	{
		return AnalyzeControlFlowCore(statement);
	}

	protected abstract ControlFlowAnalysis AnalyzeControlFlowCore(SyntaxNode statement);

	internal DataFlowAnalysis AnalyzeDataFlow(SyntaxNode firstStatement, SyntaxNode lastStatement)
	{
		return AnalyzeDataFlowCore(firstStatement, lastStatement);
	}

	protected abstract DataFlowAnalysis AnalyzeDataFlowCore(SyntaxNode firstStatement, SyntaxNode lastStatement);

	internal DataFlowAnalysis AnalyzeDataFlow(SyntaxNode statementOrExpression)
	{
		return AnalyzeDataFlowCore(statementOrExpression);
	}

	protected abstract DataFlowAnalysis AnalyzeDataFlowCore(SyntaxNode statementOrExpression);

	public Optional<object> GetConstantValue(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetConstantValueCore(node, cancellationToken);
	}

	protected abstract Optional<object> GetConstantValueCore(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken));

	internal ImmutableArray<ISymbol> GetMemberGroup(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetMemberGroupCore(node, cancellationToken);
	}

	protected abstract ImmutableArray<ISymbol> GetMemberGroupCore(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken));

	public ISymbol GetEnclosingSymbol(int position, CancellationToken cancellationToken = default(CancellationToken))
	{
		return GetEnclosingSymbolCore(position, cancellationToken);
	}

	protected abstract ISymbol GetEnclosingSymbolCore(int position, CancellationToken cancellationToken = default(CancellationToken));

	public bool IsAccessible(int position, ISymbol symbol)
	{
		return IsAccessibleCore(position, symbol);
	}

	protected abstract bool IsAccessibleCore(int position, ISymbol symbol);

	public bool IsEventUsableAsField(int position, IEventSymbol eventSymbol)
	{
		return IsEventUsableAsFieldCore(position, eventSymbol);
	}

	protected abstract bool IsEventUsableAsFieldCore(int position, IEventSymbol eventSymbol);

	public PreprocessingSymbolInfo GetPreprocessingSymbolInfo(SyntaxNode nameSyntax)
	{
		return GetPreprocessingSymbolInfoCore(nameSyntax);
	}

	protected abstract PreprocessingSymbolInfo GetPreprocessingSymbolInfoCore(SyntaxNode nameSyntax);

	internal abstract void ComputeDeclarationsInSpan(TextSpan span, bool getSymbol, ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken);

	internal abstract void ComputeDeclarationsInNode(SyntaxNode node, bool getSymbol, ArrayBuilder<DeclarationInfo> builder, CancellationToken cancellationToken, int? levelsToCompute = null);

	protected internal virtual SyntaxNode GetTopmostNodeForDiagnosticAnalysis(ISymbol symbol, SyntaxNode declaringSyntax)
	{
		return declaringSyntax;
	}
}
