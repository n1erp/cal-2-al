using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class ObjectReferencePropertyValuesRecommender : PropertyValuesRecommender
{
	protected internal override bool IsExclusive => true;

	internal ObjectReferencePropertyValuesRecommender(MemberSyntaxContext context)
		: base(context)
	{
		Next = new FieldListReferencePropertyValuesRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (PropertyTypeInfo == null || PropertyValue == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		SymbolKind symbolKind;
		switch (PropertyValue.Kind)
		{
		case SyntaxKind.QualifiedObjectReferencePropertyValue:
			symbolKind = GetSymbolKindFromQualifiedObjectReferencePropertyValue();
			break;
		case SyntaxKind.ObjectReferencePropertyValue:
			symbolKind = GetSymbolKindFromObjectReferencePropertyValue();
			break;
		case SyntaxKind.CommaSeparatedPropertyValue:
			symbolKind = GetSymbolKindFromCommaSaperatedObjectReferencePropertyValue();
			break;
		default:
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (symbolKind == SymbolKind.Undefined)
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return PropertyTypeInfo.Kind switch
		{
			PropertyKind.SumIndexFields => await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false), 
			PropertyKind.RoleCenter => PropertyValuesRecommender.GetPropertyValueRecommendationsFromSymbols(GetApplicationObjectTypeSymbols(symbolKind).WhereAsArray((IApplicationObjectTypeSymbol r) =>
			{
				PageTypeSymbol obj = r as PageTypeSymbol;
				return (object)obj != null && obj.PageType == PageTypeKind.RoleCenter;
			})), 
			PropertyKind.Customizations => PropertyValuesRecommender.GetPropertyValueRecommendationsFromSymbols(GetApplicationObjectTypeSymbols(symbolKind).WhereAsArray((IApplicationObjectTypeSymbol p) => p.Kind == SymbolKind.PageCustomization)), 
			_ => PropertyValuesRecommender.GetPropertyValueRecommendationsFromSymbols(GetApplicationObjectTypeSymbols(symbolKind)), 
		};
	}

	private ImmutableArray<IApplicationObjectTypeSymbol> GetApplicationObjectTypeSymbols(SymbolKind kind)
	{
		return Context.SemanticModel.Compilation.GetApplicationObjectTypeSymbolsAcrossModules(kind);
	}

	private SymbolKind GetSymbolKindFromQualifiedObjectReferencePropertyValue()
	{
		if (PropertyValue.Kind != SyntaxKind.QualifiedObjectReferencePropertyValue)
		{
			return SymbolKind.Undefined;
		}
		return ((QualifiedObjectReferencePropertyValueSyntax)PropertyValue).ObjectType.Kind.ToSymbolKind();
	}

	private SymbolKind GetSymbolKindFromObjectReferencePropertyValue()
	{
		if (PropertyValue.Kind != SyntaxKind.ObjectReferencePropertyValue)
		{
			return SymbolKind.Undefined;
		}
		return PropertyTypeInfo.Kind.GetSymbolKind();
	}

	private SymbolKind GetSymbolKindFromCommaSaperatedObjectReferencePropertyValue()
	{
		if (PropertyValue.Kind != SyntaxKind.CommaSeparatedPropertyValue)
		{
			return SymbolKind.Undefined;
		}
		return PropertyTypeInfo.Kind.GetSymbolKind();
	}
}
