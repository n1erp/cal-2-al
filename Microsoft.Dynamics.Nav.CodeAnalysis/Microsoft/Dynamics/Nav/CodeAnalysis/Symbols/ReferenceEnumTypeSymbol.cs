using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class ReferenceEnumTypeSymbol : EnumTypeSymbol
{
	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private readonly Lazy<ImmutableArray<EnumValueSymbol>> lazyEnumValues;

	public override int Id { get; }

	public override string Name { get; }

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<EnumValueSymbol> Values => lazyEnumValues.Value;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public static ReferenceEnumTypeSymbol Create(ModuleSymbol containingModule, EnumTypeDefinition definition)
	{
		return new ReferenceEnumTypeSymbol(containingModule, definition);
	}

	private ReferenceEnumTypeSymbol(ModuleSymbol containingModule, EnumTypeDefinition definition)
		: base(containingModule)
	{
		ReferenceEnumTypeSymbol referenceEnumTypeSymbol = this;
		Id = definition.Id.Value;
		Name = definition.Name.UnquoteIdentifier();
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, definition);
		lazyEnumValues = ReferenceSymbolHelper.CreateLazy(() => referenceEnumTypeSymbol.GetEnumValuesLazy(definition));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return StaticCast<Symbol>.From(Values);
	}

	private ImmutableArray<EnumValueSymbol> GetEnumValuesLazy(EnumTypeDefinition definition)
	{
		return definition.Values.AsImmutableOrEmpty((Func<EnumValueDefinition, EnumValueSymbol>)((EnumValueDefinition v) => ReferenceEnumValueSymbol.Create(this, v.Name, v.Ordinal)), (Func<EnumValueDefinition, bool>)((EnumValueDefinition v) => v != null));
	}
}
