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
internal class ReferenceChangeModifyControlSymbol : ChangeModifySymbol
{
	private NavDiagnosticInfo useSiteDiagnostics;

	private readonly Lazy<Symbol> lazyTarget;

	private readonly Lazy<ImmutableArray<PropertySymbol>> lazyProperties;

	public override ChangeKind ChangeKind { get; }

	public override SymbolKind ChangeTargetKind => SymbolKind.Control;

	public override Symbol ContainingSymbol { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	internal override ISymbol Target => lazyTarget.Value;

	public override ImmutableArray<MethodSymbol> Triggers => ImmutableArray<MethodSymbol>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => lazyProperties.Value;

	internal ReferenceChangeModifyControlSymbol(Symbol owner, ExtensionControlDefinition controlChange)
	{
		ReferenceChangeModifyControlSymbol referenceChangeModifyControlSymbol = this;
		ChangeKind = controlChange.ChangeKind;
		ContainingSymbol = owner;
		lazyTarget = ReferenceSymbolHelper.CreateLazy(() => referenceChangeModifyControlSymbol.GetTargetLazy(controlChange));
		lazyProperties = ReferenceSymbolHelper.CreateLazy(() => controlChange.Properties.AsImmutableOrEmpty((Func<PropertyDefinition, PropertySymbol>)((PropertyDefinition c) => new ReferencePropertySymbol(referenceChangeModifyControlSymbol, c))));
	}

	internal override DiagnosticInfo GetUseSiteDiagnostic()
	{
		return useSiteDiagnostics;
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return null;
	}

	internal override Location GetNameLocation()
	{
		return Location.None;
	}

	private Symbol GetTargetLazy(ExtensionControlDefinition controlChange)
	{
		if (string.IsNullOrEmpty(controlChange.Anchor))
		{
			return null;
		}
		PageTypeSymbol targetPage = this.GetTargetPage();
		if (targetPage == null)
		{
			return null;
		}
		PooledList<Symbol> instance = PooledList<Symbol>.GetInstance();
		try
		{
			CollectPotentialAnchors(controlChange.Anchor, targetPage, instance);
			if (instance.Count > 0)
			{
				return instance[0];
			}
			if (instance.Count == 0)
			{
				useSiteDiagnostics = new NavDiagnosticInfo(ErrorCode.ERR_SymbolNotFoundInTarget, controlChange.Anchor);
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
		foreach (ControlSymbol flattenedControl in target.FlattenedControls)
		{
			if (SemanticFacts.IsSameName(flattenedControl.Name, anchorName))
			{
				results.Add(flattenedControl);
				break;
			}
		}
		foreach (IApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbolsAcrossModule in ContainingModule.GetApplicationObjectExtensionTypeSymbolsAcrossModules(target))
		{
			Symbol anchorFromPageExtension = ReferenceChangeSymbolHelpers.GetAnchorFromPageExtension(anchorName, SymbolKind.Control, applicationObjectExtensionTypeSymbolsAcrossModule);
			if (anchorFromPageExtension != null)
			{
				results.Add(anchorFromPageExtension);
			}
		}
	}
}
