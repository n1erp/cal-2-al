using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class ReferenceRequestPageTypeSymbol : RequestPageTypeSymbol
{
	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	private ImmutableArray<Symbol> codeMembers;

	private Dictionary<string, ImmutableArray<Symbol>> codeMembersByName;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => ImmutableArray<VariableSymbol>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	public override ImmutableArray<MethodSymbol> Triggers => ImmutableArray<MethodSymbol>.Empty;

	public ReferenceRequestPageTypeSymbol(ApplicationObjectTypeSymbol containingSymbol, RequestPageDefinition requestPage)
		: base(containingSymbol)
	{
		ReferenceRequestPageTypeSymbol referenceRequestPageTypeSymbol = this;
		lazyProperties = ReferenceSymbolHelper.BuildPropertiesLazy(this, requestPage);
		lazyMembers = ReferenceSymbolHelper.CreateLazy(() => referenceRequestPageTypeSymbol.GetMembersSlow(requestPage));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		if (codeMembers == null)
		{
			ImmutableInterlocked.InterlockedCompareExchange(ref codeMembers, GetMembers().Concat(FlattenedControls.Where((ControlSymbol m) => !m.IsControlArea())).Concat(FlattenedActions.Where((ActionSymbol m) => !m.IsActionArea())).Distinct()
				.ToImmutableArray(), default);
		}
		return codeMembers;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return GetCodeMembers(ref codeMembersByName, name);
	}

	private ImmutableArray<Symbol> GetMembersSlow(RequestPageDefinition requestPage)
	{
		ImmutableArray<PropertySymbol> properties = Properties;
		IEnumerable<ReferenceActionSymbol> items = requestPage?.Actions?.Select((ActionDefinition a) => new ReferenceActionSymbol(this, a)) ?? SpecializedCollections.EmptyEnumerable<ReferenceActionSymbol>();
		int num = requestPage?.Actions.GetLengthSafe() ?? 0;
		IEnumerable<ReferenceControlSymbol> items2 = requestPage?.Controls?.Select((ControlDefinition c) => new ReferenceControlSymbol(this, c)) ?? SpecializedCollections.EmptyEnumerable<ReferenceControlSymbol>();
		int num2 = requestPage?.Controls.GetLengthSafe() ?? 0;
		PageTypeKind enumPropertyValueOrDefault = this.GetEnumPropertyValueOrDefault<PageTypeKind>(PropertyKind.PageType);
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(properties.Length + num + num2);
		try
		{
			instance.AddRange(properties);
			instance.AddRange(items);
			PageObjectMembers.AddStubActionAreas(this, enumPropertyValueOrDefault, instance);
			instance.AddRange(items2);
			PageObjectMembers.AddStubAreas(this, enumPropertyValueOrDefault, instance);
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}
}
