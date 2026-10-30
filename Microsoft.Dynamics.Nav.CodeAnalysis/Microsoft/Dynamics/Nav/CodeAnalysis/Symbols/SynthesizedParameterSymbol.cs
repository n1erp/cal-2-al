using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SynthesizedParameterSymbol : ParameterSymbol
{
	public override TypeSymbol Type { get; }

	public override string Name { get; }

	public override int Ordinal { get; }

	public override bool IsParams { get; }

	public override bool IsVar { get; }

	public override bool IsImplicitlyDeclared { get; } = true;

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<Location> Locations { get; } = ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; } = ImmutableArray<SyntaxReference>.Empty;

	public SynthesizedParameterSymbol(Symbol container, TypeSymbol type, int ordinal, bool isVar, string name = "")
	{
		DebugAssertHelper.Assert((object)type != null);
		DebugAssertHelper.Assert(name != null);
		DebugAssertHelper.Assert(ordinal >= 0);
		ContainingSymbol = container;
		Type = type;
		Ordinal = ordinal;
		IsVar = isVar;
		Name = name;
	}
}
