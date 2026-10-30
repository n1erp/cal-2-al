using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

public sealed class UnresolvedAnalyzerReference : AnalyzerReference
{
	private readonly string _unresolvedPath;

	public override string Display => CodeAnalysisResources.Unresolved + FullPath;

	public override string FullPath => _unresolvedPath;

	public override object Id => _unresolvedPath;

	public UnresolvedAnalyzerReference(string unresolvedPath)
	{
		if (unresolvedPath == null)
		{
			throw new ArgumentNullException("unresolvedPath");
		}
		_unresolvedPath = unresolvedPath;
	}

	public override ImmutableArray<DiagnosticAnalyzer> GetAnalyzersForAllLanguages()
	{
		return ImmutableArray<DiagnosticAnalyzer>.Empty;
	}

	public override ImmutableArray<DiagnosticAnalyzer> GetAnalyzers(string language)
	{
		return ImmutableArray<DiagnosticAnalyzer>.Empty;
	}
}
