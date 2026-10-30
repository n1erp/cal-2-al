using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;

public abstract class DiagnosticAnalyzer
{
	public abstract ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

	public abstract void Initialize(AnalysisContext context);

	public sealed override bool Equals(object obj)
	{
		return ReferenceEqualityComparer.Instance.Equals(this, obj);
	}

	public sealed override int GetHashCode()
	{
		return ReferenceEqualityComparer.Instance.GetHashCode(this);
	}

	public sealed override string ToString()
	{
		return GetType().ToString();
	}
}
