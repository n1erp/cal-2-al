using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

internal class AnalysisScope
{
	public SyntaxTree FilterTreeOpt { get; private set; }

	public TextSpan? FilterSpanOpt { get; private set; }

	public ImmutableArray<DiagnosticAnalyzer> Analyzers { get; private set; }

	public IEnumerable<SyntaxTree> SyntaxTrees { get; private set; }

	public bool ConcurrentAnalysis { get; private set; }

	public bool CategorizeDiagnostics { get; private set; }

	public bool IsSyntaxOnlyTreeAnalysis { get; private set; }

	public bool IsTreeAnalysis => FilterTreeOpt != null;

	public AnalysisScope(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, bool concurrentAnalysis, bool categorizeDiagnostics)
	{
		SyntaxTrees = compilation.SyntaxTrees;
		Analyzers = analyzers;
		ConcurrentAnalysis = concurrentAnalysis;
		CategorizeDiagnostics = categorizeDiagnostics;
		FilterTreeOpt = null;
		FilterSpanOpt = null;
		IsSyntaxOnlyTreeAnalysis = false;
	}

	public AnalysisScope(ImmutableArray<DiagnosticAnalyzer> analyzers, SyntaxTree filterTree, TextSpan? filterSpan, bool syntaxAnalysis, bool concurrentAnalysis, bool categorizeDiagnostics)
	{
		DebugAssertHelper.Assert(filterTree != null);
		SyntaxTrees = SpecializedCollections.SingletonEnumerable(filterTree);
		Analyzers = analyzers;
		FilterTreeOpt = filterTree;
		FilterSpanOpt = filterSpan;
		IsSyntaxOnlyTreeAnalysis = syntaxAnalysis;
		ConcurrentAnalysis = concurrentAnalysis;
		CategorizeDiagnostics = categorizeDiagnostics;
	}

	public static bool ShouldSkipSymbolAnalysis(SymbolDeclaredCompilationEvent symbolEvent)
	{
		if (symbolEvent.DeclaringSyntaxReferences.All((SyntaxReference s) => s.SyntaxTree == null))
		{
			return symbolEvent.Symbol.Kind != SymbolKind.Module;
		}
		return false;
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "symbol")]
	public static bool ShouldSkipDeclarationAnalysis(ISymbol symbol)
	{
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
