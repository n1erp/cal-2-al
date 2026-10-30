using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SynthesizedReturnValueSymbol : ReturnValueSymbol
{
	private readonly MethodSymbol container;

	public override TypeSymbol Type { get; }

	public override string Name { get; }

	public override bool IsImplicitlyDeclared => true;

	public override bool IsOptional { get; }

	public override bool IsNamed => false;

	public override Symbol ContainingSymbol => container;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public SynthesizedReturnValueSymbol(MethodSymbol container, TypeSymbol type, string name = "", bool isOptional = false)
	{
		DebugAssertHelper.Assert((object)type != null);
		DebugAssertHelper.Assert(name != null);
		this.container = container;
		Type = type;
		Name = name;
		IsOptional = isOptional;
	}
}
