using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

public abstract class AnalyzerReference
{
	public abstract string FullPath { get; }

	public virtual string Display => null;

	public abstract object Id { get; }

	[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
	public abstract ImmutableArray<DiagnosticAnalyzer> GetAnalyzers();
}
