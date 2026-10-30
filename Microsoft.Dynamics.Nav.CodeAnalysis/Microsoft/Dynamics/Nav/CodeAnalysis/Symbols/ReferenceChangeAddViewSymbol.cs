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
internal sealed class ReferenceChangeAddViewSymbol : ChangeAddSymbol
{
	private DiagnosticInfo useSiteDiagnostics;

	private readonly Lazy<ImmutableArray<Symbol>> lazyAddedSymbols;

	private readonly Lazy<Symbol> lazyAnchor;

	public override string Name { get; }

	public override ChangeKind ChangeKind { get; }

	public override SymbolKind ChangeTargetKind { get; } = SymbolKind.View;

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<Location> Locations { get; } = ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences { get; } = ImmutableArray<SyntaxReference>.Empty;

	internal override Symbol Anchor => lazyAnchor.Value;

	internal override ImmutableArray<Symbol> AddedSymbols => lazyAddedSymbols.Value;

	internal ReferenceChangeAddViewSymbol(ApplicationObjectExtensionTypeSymbol owner, ExtensionViewDefinition viewChange)
	{
		ReferenceChangeAddViewSymbol referenceChangeAddViewSymbol = this;
		ChangeKind = viewChange.ChangeKind;
		ContainingSymbol = owner;
		Name = string.Empty;
		lazyAddedSymbols = ReferenceSymbolHelper.CreateLazy(() => viewChange.Views.AsImmutableOrEmpty((Func<ViewDefinition, Symbol>)((ViewDefinition c) => new ReferenceViewSymbol(referenceChangeAddViewSymbol, c))));
		lazyAnchor = ReferenceSymbolHelper.CreateLazy(() => referenceChangeAddViewSymbol.GetAnchorLazy(viewChange));
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return AddedSymbols;
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		return useSiteDiagnostics;
	}

	private Symbol GetAnchorLazy(ExtensionViewDefinition viewChange)
	{
		if (string.IsNullOrEmpty(viewChange.Anchor))
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
			CollectPotentialAnchors(viewChange.Anchor, pageTypeSymbol, instance);
			if (instance.Count > 0)
			{
				return instance[0];
			}
			if (instance.Count == 0)
			{
				useSiteDiagnostics = new NavDiagnosticInfo(ErrorCode.ERR_SymbolNotFoundInTarget, viewChange.Anchor);
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
		foreach (ViewSymbol flattenedView in target.FlattenedViews)
		{
			if (SemanticFacts.IsSameName(flattenedView.Name, anchorName))
			{
				results.Add(flattenedView);
				break;
			}
		}
		foreach (IApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbolsAcrossModule in ContainingModule.GetApplicationObjectExtensionTypeSymbolsAcrossModules(target))
		{
			Symbol anchorFromPageExtension = ReferenceChangeSymbolHelpers.GetAnchorFromPageExtension(anchorName, SymbolKind.View, applicationObjectExtensionTypeSymbolsAcrossModule);
			if (anchorFromPageExtension != null)
			{
				results.Add(anchorFromPageExtension);
			}
		}
	}
}
