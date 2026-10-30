using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

internal class AnalysisScope
{
	public SyntaxTree FilterTreeOpt { get; }

	public TextSpan? FilterSpanOpt { get; }

	public ImmutableArray<DiagnosticAnalyzer> Analyzers { get; }

	public IEnumerable<SyntaxTree> SyntaxTrees { get; }

	public bool ConcurrentAnalysis { get; }

	public bool CategorizeDiagnostics { get; }

	public bool IsSyntaxOnlyTreeAnalysis { get; }

	public bool IsTreeAnalysis => FilterTreeOpt != null;

	public AnalysisScope(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, bool concurrentAnalysis, bool categorizeDiagnostics)
		: this(compilation.SyntaxTrees, analyzers, null, null, isSyntaxOnlyTreeAnalysis: false, concurrentAnalysis, categorizeDiagnostics)
	{
	}

	public AnalysisScope(ImmutableArray<DiagnosticAnalyzer> analyzers, SyntaxTree filterTree, TextSpan? filterSpan, bool syntaxAnalysis, bool concurrentAnalysis, bool categorizeDiagnostics)
		: this(SpecializedCollections.SingletonEnumerable(filterTree), analyzers, filterTree, filterSpan, syntaxAnalysis, concurrentAnalysis, categorizeDiagnostics)
	{
	}

	private AnalysisScope(IEnumerable<SyntaxTree> trees, ImmutableArray<DiagnosticAnalyzer> analyzers, SyntaxTree filterTreeOpt, TextSpan? filterSpanOpt, bool isSyntaxOnlyTreeAnalysis, bool concurrentAnalysis, bool categorizeDiagnostics)
	{
		SyntaxTrees = trees;
		Analyzers = analyzers;
		FilterTreeOpt = filterTreeOpt;
		FilterSpanOpt = filterSpanOpt;
		IsSyntaxOnlyTreeAnalysis = isSyntaxOnlyTreeAnalysis;
		ConcurrentAnalysis = concurrentAnalysis;
		CategorizeDiagnostics = categorizeDiagnostics;
	}

	public AnalysisScope WithAnalyzers(ImmutableArray<DiagnosticAnalyzer> analyzers)
	{
		return new AnalysisScope(SyntaxTrees, analyzers, FilterTreeOpt, FilterSpanOpt, IsSyntaxOnlyTreeAnalysis, ConcurrentAnalysis, CategorizeDiagnostics);
	}

	public static bool ShouldSkipSymbolAnalysis(SymbolDeclaredCompilationEvent symbolEvent)
	{
		if (!symbolEvent.Symbol.IsImplicitlyDeclared)
		{
			return symbolEvent.DeclaringSyntaxReferences.All((SyntaxReference s) => s.SyntaxTree == null);
		}
		return true;
	}

	public static bool ShouldSkipDeclarationAnalysis(ISymbol symbol)
	{
		if (symbol.IsImplicitlyDeclared)
		{
			if (symbol.Kind == SymbolKind.Namespace)
			{
				return !((INamespaceSymbol)symbol).IsGlobalNamespace;
			}
			return true;
		}
		return false;
	}

	public bool ShouldAnalyze(SyntaxTree tree)
	{
		if (FilterTreeOpt != null)
		{
			return FilterTreeOpt == tree;
		}
		return true;
	}

	public bool ShouldAnalyze(ISymbol symbol)
	{
		if (FilterTreeOpt == null)
		{
			return true;
		}
		foreach (Location location in symbol.Locations)
		{
			if (location.SourceTree != null && FilterTreeOpt == location.SourceTree && ShouldInclude(location.SourceSpan))
			{
				return true;
			}
		}
		return false;
	}

	public bool ShouldAnalyze(SyntaxNode node)
	{
		if (FilterTreeOpt == null)
		{
			return true;
		}
		return ShouldInclude(node.FullSpan);
	}

	private bool ShouldInclude(TextSpan filterSpan)
	{
		if (FilterSpanOpt.HasValue)
		{
			return FilterSpanOpt.Value.IntersectsWith(filterSpan);
		}
		return true;
	}

	public bool ContainsSpan(TextSpan filterSpan)
	{
		if (FilterSpanOpt.HasValue)
		{
			return FilterSpanOpt.Value.Contains(filterSpan);
		}
		return true;
	}

	public bool ShouldInclude(Diagnostic diagnostic)
	{
		if (FilterTreeOpt == null)
		{
			return true;
		}
		if (!diagnostic.Location.IsInSource || diagnostic.Location.SourceTree != FilterTreeOpt)
		{
			return false;
		}
		return ShouldInclude(diagnostic.Location.SourceSpan);
	}
}
