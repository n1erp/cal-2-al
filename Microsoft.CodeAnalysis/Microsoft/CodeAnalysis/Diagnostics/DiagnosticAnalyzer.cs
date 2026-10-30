using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Diagnostics;

public abstract class DiagnosticAnalyzer
{
	public abstract ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

	public abstract void Initialize(AnalysisContext context);

	public sealed override bool Equals(object obj)
	{
		return this == obj;
	}

	public sealed override int GetHashCode()
	{
		return ReferenceEqualityComparer.GetHashCode(this);
	}

	public sealed override string ToString()
	{
		return GetType().ToString();
	}
}
