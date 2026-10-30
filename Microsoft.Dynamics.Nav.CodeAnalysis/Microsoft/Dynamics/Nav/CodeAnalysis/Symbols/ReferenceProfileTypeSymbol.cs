using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class ReferenceProfileTypeSymbol : ProfileTypeSymbol
{
	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	public override string Name { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	internal ReferenceProfileTypeSymbol(ModuleSymbol module, ProfileDefinition profile)
		: base(module)
	{
		DebugAssertHelper.Assert(profile != null);
		Name = profile.Name().UnquoteIdentifier();
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, profile);
	}
}
