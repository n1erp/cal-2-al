using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ControlListReferencePropertyValuesRecommender : PropertyValuesRecommender
{
	protected internal override bool IsExclusive => true;

	internal ControlListReferencePropertyValuesRecommender(MemberSyntaxContext context)
		: base(context)
	{
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (PropertyTypeInfo == null || PropertyValue == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		bool num = PropertyValue.IsKind(SyntaxKind.CommaSeparatedPropertyValue) && string.Equals(PropertyTypeInfo.ValueKind, SymbolKind.Control.ToString(), StringComparison.OrdinalIgnoreCase);
		bool flag = PropertyValue.IsKind(SyntaxKind.PageFieldReferencePropertyValue);
		if (!num && !flag)
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IEnumerable<ISymbol> symbols = SpecializedCollections.EmptyEnumerable<ISymbol>();
		if (PropertyTypeInfo.Kind == PropertyKind.IndentationControls || PropertyTypeInfo.Kind == PropertyKind.FreezeColumn)
		{
			symbols = FindRepeaterControls();
		}
		else if (PropertyTypeInfo.Kind == PropertyKind.Provider)
		{
			symbols = FindParts();
		}
		return PropertyValuesRecommender.GetPropertyValueRecommendationsFromSymbols(symbols);
	}

	private IEnumerable<ISymbol> FindRepeaterControls()
	{
		ISymbol declaredSymbol = Context.SemanticModel.GetDeclaredSymbol(PropertyValue.Parent);
		DebugAssertHelper.Assert(declaredSymbol != null);
		if (declaredSymbol == null)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		ControlSymbol repeater = declaredSymbol.ContainingSymbol.GetExtendedPageControlOrSelf();
		ControlSymbol controlSymbol = repeater;
		if ((object)controlSymbol == null || controlSymbol.ControlKind != ControlKind.Repeater)
		{
			return SpecializedCollections.EmptyEnumerable<ISymbol>();
		}
		if (Context.DeclaringObject.IsKind(SyntaxKind.PageObject))
		{
			return repeater.Controls;
		}
		return from x in Context.SemanticModel.LookupSymbols(Context.Position, LookupOptions.MustBePageElement)
			where x.GetContainingSymbol() == repeater
			select x;
	}

	private IEnumerable<ISymbol> FindParts()
	{
		IPageBaseTypeSymbol containingPage = Context.SemanticModel.GetDeclaredSymbol(PropertyValue.Parent).ContainingType.GetContainingPage();
		IEnumerable<IControlSymbol> source = (IEnumerable<IControlSymbol>)((!Context.DeclaringObject.IsKind(SyntaxKind.PageObject)) ? Context.SemanticModel.LookupSymbols(Context.Position, LookupOptions.MustBePageElement).OfType<IControlSymbol>() : ((object)containingPage.FlattenedControls));
		return source.Where((IControlSymbol x) => x.ControlKind == ControlKind.Part);
	}
}
