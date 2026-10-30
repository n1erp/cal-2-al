using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class ReferenceControlAddInTypeSymbol : ControlAddInTypeSymbol
{
	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	public override string Name { get; }

	public override string PublicKeyToken { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	internal ReferenceControlAddInTypeSymbol(ModuleSymbol containingSymbol, ControlAddInDefinition definition)
		: base(containingSymbol)
	{
		ReferenceControlAddInTypeSymbol referenceControlAddInTypeSymbol = this;
		Name = definition.Name;
		PublicKeyToken = definition.PublicKeyToken;
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, definition);
		lazyMembers = ReferenceSymbolHelper.CreateLazy(() => referenceControlAddInTypeSymbol.GetMembersLazy(definition));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}

	private ImmutableArray<Symbol> GetMembersLazy(ControlAddInDefinition definition)
	{
		IEnumerable<Symbol> enumerable = definition.Methods?.Select((MethodDefinition m) => new ReferenceMethodSymbol(this, m));
		IEnumerable<Symbol> items = enumerable ?? Enumerable.Empty<Symbol>();
		enumerable = definition.Events?.Select((EventDefinition m) => new ReferenceEventSymbol(this, m));
		IEnumerable<Symbol> items2 = enumerable ?? Enumerable.Empty<Symbol>();
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(Properties.Length + definition.Methods.GetLengthSafe() + definition.Events.GetLengthSafe());
		instance.AddRange(Properties);
		instance.AddRange(items);
		instance.AddRange(items2);
		return instance.ToImmutableAndFree();
	}
}
