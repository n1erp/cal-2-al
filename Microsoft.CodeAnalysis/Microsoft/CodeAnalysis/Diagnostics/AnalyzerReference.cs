using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics;

public abstract class AnalyzerReference
{
	public abstract string FullPath { get; }

	public virtual string Display => null;

	public abstract object Id { get; }

	public abstract ImmutableArray<DiagnosticAnalyzer> GetAnalyzersForAllLanguages();

	public abstract ImmutableArray<DiagnosticAnalyzer> GetAnalyzers(string language);
}
