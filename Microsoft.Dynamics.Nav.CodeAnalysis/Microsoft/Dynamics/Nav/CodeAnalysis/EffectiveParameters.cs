using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal class EffectiveParameters
{
	internal readonly ImmutableArray<TypeSymbol> ParameterTypes;

	internal readonly ImmutableArray<bool> ParameterRefKinds;

	internal readonly ImmutableArray<ParameterSymbol> Parameters;

	internal EffectiveParameters(ImmutableArray<TypeSymbol> types, ImmutableArray<bool> refKinds, ImmutableArray<ParameterSymbol> parameters)
	{
		ParameterTypes = types;
		ParameterRefKinds = refKinds;
		Parameters = parameters;
	}

	internal string GetDebuggerDisplay()
	{
		return string.Join(", ", ParameterTypes.Select((TypeSymbol p) => p.ToDisplayString(SymbolDisplayFormat.TestFormat)));
	}

	public override string ToString()
	{
		return string.Join(",", Parameters.Select((ParameterSymbol p) => p.ToDisplayString(SymbolDisplayFormat.TestFormat)));
	}
}
