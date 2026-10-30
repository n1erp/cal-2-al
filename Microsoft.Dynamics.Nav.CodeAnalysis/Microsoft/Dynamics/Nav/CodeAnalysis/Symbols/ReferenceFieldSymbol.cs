using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class ReferenceFieldSymbol : FieldSymbol
{
	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private readonly Lazy<TypeSymbol> lazyType;

	public override Symbol ContainingSymbol { get; }

	public override int Id { get; }

	public override string Name { get; }

	public override TypeSymbol Type => lazyType.Value;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public override ImmutableArray<MethodSymbol> Triggers => ImmutableArray<MethodSymbol>.Empty;

	internal ReferenceFieldSymbol(ContainerSymbol containingSymbol, FieldDefinition field)
	{
		ReferenceFieldSymbol referenceFieldSymbol = this;
		DebugAssertHelper.Assert(field != null);
		ContainingSymbol = containingSymbol;
		Id = field.Id ?? (-1);
		Name = field.Name();
		lazyType = ReferenceSymbolHelper.CreateLazy(() => SerializableSymbolModelConverter.ResolveType(referenceFieldSymbol.ContainingSymbol.ContainingModule, referenceFieldSymbol, field.GetTypeDefinition()));
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, field);
		lazyMembers = ReferenceSymbolHelper.CreateLazy(() => referenceFieldSymbol.GetMembersLazy());
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}

	private ImmutableArray<Symbol> GetMembersLazy()
	{
		ImmutableArray<MethodSymbol> triggers = Triggers;
		ImmutableArray<PropertySymbol> properties = Properties;
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(properties.Length + properties.Length);
		instance.AddRange(triggers);
		instance.AddRange(properties);
		return instance.ToImmutableAndFree();
	}
}
