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
internal sealed class ReferencePageExtensionTypeSymbol : PageExtensionTypeSymbol
{
	private readonly Lazy<ImmutableArray<Symbol>> lazyMembers;

	private readonly Lazy<ImmutableArray<VariableSymbol>> lazyGlobalVariables;

	private readonly Lazy<ApplicationObjectTypeSymbol> lazyTarget;

	private Dictionary<string, ImmutableArray<Symbol>> codeMembersByName;

	private ImmutableArray<Symbol> codeMembers;

	public override int Id { get; }

	public override string Name { get; }

	internal override bool RequiresCompletion => true;

	internal override ImmutableArray<VariableSymbol> GlobalVariables => lazyGlobalVariables.Value;

	public override ImmutableArray<MethodSymbol> Triggers => ImmutableArray<MethodSymbol>.Empty;

	internal override ApplicationObjectTypeSymbol Target => lazyTarget.Value;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => ImmutableArray<PropertySymbol>.Empty;

	internal ReferencePageExtensionTypeSymbol(ModuleSymbol containingSymbol, PageExtensionDefinition pageExtension)
		: base(containingSymbol)
	{
		ReferencePageExtensionTypeSymbol referencePageExtensionTypeSymbol = this;
		DebugAssertHelper.Assert(pageExtension != null);
		Id = pageExtension.Id ?? (-1);
		Name = pageExtension.Name?.UnquoteIdentifier();
		lazyGlobalVariables = ReferenceSymbolHelper.BuildGlobalVariablesLazy(this, pageExtension);
		lazyTarget = ReferenceSymbolHelper.CreateLazy(() => referencePageExtensionTypeSymbol.GetTargetLazy(pageExtension));
		lazyMembers = ReferenceSymbolHelper.CreateLazy(() => referencePageExtensionTypeSymbol.GetMembersLazy(pageExtension));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return lazyMembers.Value;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		if (codeMembers == null)
		{
			ImmutableInterlocked.InterlockedCompareExchange(ref codeMembers, GetMembers().Concat(FlattenedControls.Where((ControlSymbol m) => !m.IsControlArea())).Concat(FlattenedActions.Where((ActionSymbol m) => !m.IsActionArea())).Concat(FlattenedViews)
				.ToImmutableArray(), default);
		}
		return codeMembers;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return GetCodeMembers(ref codeMembersByName, name);
	}

	private ImmutableArray<Symbol> GetMembersLazy(PageExtensionDefinition pageExtension)
	{
		ImmutableArray<VariableSymbol> globalVariables = GlobalVariables;
		int lengthSafe = pageExtension.Methods.GetLengthSafe();
		IEnumerable<Symbol> enumerable = pageExtension.Methods?.Select((MethodDefinition m) => new ReferenceMethodSymbol(this, m));
		IEnumerable<Symbol> items = enumerable ?? SpecializedCollections.EmptyEnumerable<Symbol>();
		int lengthSafe2 = pageExtension.ActionChanges.GetLengthSafe();
		enumerable = pageExtension.ActionChanges?.Select((ExtensionActionDefinition actionChange) => new ReferenceChangeAddActionSymbol(this, actionChange));
		IEnumerable<Symbol> items2 = enumerable ?? SpecializedCollections.EmptyEnumerable<Symbol>();
		int lengthSafe3 = pageExtension.ControlChanges.GetLengthSafe();
		enumerable = pageExtension.ControlChanges?.Select((ExtensionControlDefinition controlChange) => new ReferenceChangeAddControlSymbol(this, controlChange));
		IEnumerable<Symbol> items3 = enumerable ?? SpecializedCollections.EmptyEnumerable<Symbol>();
		int lengthSafe4 = pageExtension.ViewChanges.GetLengthSafe();
		enumerable = pageExtension.ViewChanges?.Select((ExtensionViewDefinition viewChange) => new ReferenceChangeAddViewSymbol(this, viewChange));
		IEnumerable<Symbol> items4 = enumerable ?? SpecializedCollections.EmptyEnumerable<Symbol>();
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance(globalVariables.Length + lengthSafe + lengthSafe2 + lengthSafe3 + lengthSafe4);
		instance.AddRange(globalVariables);
		instance.AddRange(items);
		instance.AddRange(items2);
		instance.AddRange(items3);
		instance.AddRange(items4);
		return instance.ToImmutableAndFree();
	}

	private ApplicationObjectTypeSymbol GetTargetLazy(PageExtensionDefinition pageExtension)
	{
		ImmutableArray<ISymbol> objectSymbolsByNameAcrossModules = ContainingModule.ReferenceResolver.GetObjectSymbolsByNameAcrossModules(ContainingModule, SymbolKind.Page, pageExtension.TargetObject);
		if (objectSymbolsByNameAcrossModules.Length == 1)
		{
			return objectSymbolsByNameAcrossModules[0] as PageTypeSymbol;
		}
		return null;
	}
}
