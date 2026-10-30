using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class SystemOptionSymbol : OptionSymbol
{
	public override VersionCompatibility Compatibility { get; }

	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override string MetadataName { get; }

	public override Accessibility DeclaredAccessibility { get; } = Accessibility.Public;

	public override int Ordinal { get; }

	public override TypeSymbol Type { get; }

	public override ImmutableArray<Location> Locations { get; } = ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; } = ImmutableArray<SyntaxReference>.Empty;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitSystemOption(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitSystemOption(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitSystemOption(this, a);
	}

	public static SystemOptionSymbol Create(TypeSymbol owner, string name, int ordinal, string metadataName = null, VersionCompatibility compatibility = null)
	{
		return new SystemOptionSymbol(owner, name, ordinal, metadataName, compatibility);
	}

	private SystemOptionSymbol(TypeSymbol containingSymbol, string name, int ordinal, string metadataName, VersionCompatibility compatibility)
	{
		ContainingSymbol = containingSymbol;
		Name = name;
		Ordinal = ordinal;
		MetadataName = metadataName ?? name;
		Type = containingSymbol;
		Compatibility = compatibility;
	}
}
