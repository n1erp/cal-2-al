using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class ReferenceActionSymbol : ActionSymbol
{
	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private readonly Lazy<ImmutableArray<ActionSymbol>> lazyActions;

	public override ActionKind ActionKind { get; }

	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override int Id { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public override ImmutableArray<ActionSymbol> Actions => lazyActions.Value;

	public override ImmutableArray<MethodSymbol> Triggers { get; } = ImmutableArray<MethodSymbol>.Empty;

	public override TypeSymbol Type => NavCorLib.None;

	internal ReferenceActionSymbol(Symbol containingSymbol, ActionDefinition actionDefinition)
	{
		ReferenceActionSymbol referenceActionSymbol = this;
		ContainingSymbol = containingSymbol;
		Name = actionDefinition.Name;
		Id = actionDefinition.Id ?? (-1);
		ActionKind = actionDefinition.Kind;
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, actionDefinition);
		lazyActions = ReferenceSymbolHelper.CreateLazy(() => actionDefinition.Actions.AsImmutableOrEmpty((Func<ActionDefinition, ActionSymbol>)((ActionDefinition a) => new ReferenceActionSymbol(referenceActionSymbol, a))));
		lazyMembers = ReferenceSymbolHelper.CreateLazy(() => referenceActionSymbol.GetMembersLazy(actionDefinition));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}

	private ImmutableArray<Symbol> GetMembersLazy(ActionDefinition actionDefinition)
	{
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(Actions.Length + Properties.Length + Triggers.Length);
		instance.AddRange(Actions);
		instance.AddRange(Triggers);
		instance.AddRange(Properties);
		return instance.ToImmutableAndFree();
	}
}
