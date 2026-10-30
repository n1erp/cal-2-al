using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class ReferenceChangeAddActionSymbol : ChangeAddSymbol
{
	private readonly Lazy<Symbol> lazyAnchor;

	private DiagnosticInfo useSiteDiagnostics;

	private readonly Lazy<ImmutableArray<Symbol>> lazyAddedSymbols;

	public override string Name { get; }

	public override ChangeKind ChangeKind { get; }

	public override SymbolKind ChangeTargetKind => SymbolKind.Action;

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	internal override Symbol Anchor => lazyAnchor.Value;

	internal override ImmutableArray<Symbol> AddedSymbols => lazyAddedSymbols.Value;

	internal ReferenceChangeAddActionSymbol(ApplicationObjectExtensionTypeSymbol owner, ExtensionActionDefinition actionChange)
	{
		ReferenceChangeAddActionSymbol referenceChangeAddActionSymbol = this;
		ChangeKind = actionChange.ChangeKind;
		ContainingSymbol = owner;
		Name = string.Empty;
		lazyAnchor = ReferenceSymbolHelper.CreateLazy(() => referenceChangeAddActionSymbol.GetAnchorLazy(actionChange));
		lazyAddedSymbols = ReferenceSymbolHelper.CreateLazy(() => actionChange.Actions.AsImmutableOrEmpty((Func<ActionDefinition, Symbol>)((ActionDefinition a) => new ReferenceActionSymbol(referenceChangeAddActionSymbol, a))));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return AddedSymbols;
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		return useSiteDiagnostics;
	}

	private Symbol GetAnchorLazy(ExtensionActionDefinition actionChange)
	{
		if (string.IsNullOrEmpty(actionChange.Anchor))
		{
			return null;
		}
		PageTypeSymbol pageTypeSymbol = ((ApplicationObjectExtensionTypeSymbol)ContainingSymbol).Target as PageTypeSymbol;
		if (pageTypeSymbol == null)
		{
			return null;
		}
		PooledList<Symbol> instance = PooledList<Symbol>.GetInstance();
		try
		{
			CollectPotentialAnchors(actionChange.Anchor, pageTypeSymbol, instance);
			if (instance.Count > 0)
			{
				return instance[0];
			}
			if (instance.Count == 0)
			{
				useSiteDiagnostics = new NavDiagnosticInfo(ErrorCode.ERR_SymbolNotFoundInTarget, actionChange.Anchor);
			}
			return null;
		}
		finally
		{
			instance.Free();
		}
	}

	private void CollectPotentialAnchors(string anchorName, PageTypeSymbol target, PooledList<Symbol> results)
	{
		foreach (ActionSymbol flattenedAction in target.FlattenedActions)
		{
			if (SemanticFacts.IsSameName(flattenedAction.Name, anchorName))
			{
				results.Add(flattenedAction);
				break;
			}
		}
		foreach (IApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbolsAcrossModule in ContainingModule.GetApplicationObjectExtensionTypeSymbolsAcrossModules(target))
		{
			Symbol anchorFromPageExtension = ReferenceChangeSymbolHelpers.GetAnchorFromPageExtension(anchorName, SymbolKind.Action, applicationObjectExtensionTypeSymbolsAcrossModule);
			if (anchorFromPageExtension != null)
			{
				results.Add(anchorFromPageExtension);
			}
		}
	}
}
