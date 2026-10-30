using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

public sealed class UnresolvedAnalyzerReference : AnalyzerReference
{
	private readonly string unresolvedPath;

	public override string Display => CodeAnalysisResources.Unresolved + FullPath;

	public override string FullPath => unresolvedPath;

	public override object Id => unresolvedPath;

	public string ExceptionMessage { get; }

	internal UnresolvedAnalyzerReference(string unresolvedPath, string exceptionMessage)
	{
		this.unresolvedPath = unresolvedPath ?? throw new ArgumentNullException("unresolvedPath");
		ExceptionMessage = exceptionMessage ?? throw new ArgumentNullException("exceptionMessage");
	}

	public override ImmutableArray<DiagnosticAnalyzer> GetAnalyzers()
	{
		return ImmutableArray<DiagnosticAnalyzer>.Empty;
	}
}
