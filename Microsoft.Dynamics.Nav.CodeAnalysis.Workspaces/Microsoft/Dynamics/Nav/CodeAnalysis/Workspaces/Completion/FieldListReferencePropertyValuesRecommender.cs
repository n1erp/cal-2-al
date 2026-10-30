using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Completion;

internal class FieldListReferencePropertyValuesRecommender : PropertyValuesRecommender
{
	protected internal override bool IsExclusive => true;

	internal FieldListReferencePropertyValuesRecommender(MemberSyntaxContext context)
		: base(context)
	{
		Next = new ControlListReferencePropertyValuesRecommender(context);
	}

	protected internal override async Task<IEnumerable<PropertyValueRecommendation>> RecommendPropertyValuesAsync(CancellationToken cancellationToken)
	{
		if (PropertyTypeInfo == null || PropertyValue == null)
		{
			return SpecializedCollections.EmptyEnumerable<PropertyValueRecommendation>();
		}
		if (!PropertyValue.IsKind(SyntaxKind.CommaSeparatedPropertyValue) || !string.Equals(PropertyTypeInfo.ValueKind, SymbolKind.Field.ToString(), StringComparison.OrdinalIgnoreCase))
		{
			return await base.RecommendPropertyValuesAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		IContainerSymbol tableTypeSymbol = FindRelatedTableSymbol();
		IEnumerable<ISymbol> fields = RecommendationHelper.RecommendFieldSymbolsForFieldGroup(Context, tableTypeSymbol, cancellationToken);
		fields = FilterResult(fields);
		return PropertyValuesRecommender.GetPropertyValueRecommendationsFromSymbols(fields);
	}

	private IContainerSymbol FindRelatedTableSymbol()
	{
		IContainerSymbol containerSymbol = null;
		switch (PropertyTypeInfo.Kind)
		{
		case PropertyKind.RequestFilterFields:
		case PropertyKind.CalcFields:
		{
			ISymbol declaredSymbol = Context.SemanticModel.GetDeclaredSymbol(PropertyValue.Parent);
			if (declaredSymbol == null)
			{
				break;
			}
			if (declaredSymbol.ContainingType.IsKind(SymbolKind.XmlPortNode))
			{
				TypeSymbol type = ((XmlPortNodeSymbol)declaredSymbol.ContainingType).Type;
				if (type.IsKind(SymbolKind.Record))
				{
					containerSymbol = type.BaseType;
				}
			}
			else if (declaredSymbol.ContainingType.IsKind(SymbolKind.ReportDataItem))
			{
				TypeSymbol type2 = ((ReportDataItemSymbol)declaredSymbol.ContainingType).Type;
				if (type2.IsKind(SymbolKind.Record))
				{
					containerSymbol = type2.BaseType;
				}
			}
			break;
		}
		case PropertyKind.DataCaptionFields:
		case PropertyKind.ODataKeyFields:
		{
			ISymbol declaredSymbol = Context.SemanticModel.GetDeclaredSymbol(PropertyValue.Parent);
			if (declaredSymbol != null)
			{
				containerSymbol = declaredSymbol.GetContainingPage()?.RelatedTable;
			}
			break;
		}
		}
		return containerSymbol ?? DeclaringObject;
	}

	private IEnumerable<ISymbol> FilterResult(IEnumerable<ISymbol> fields)
	{
		if (PropertyTypeInfo.Kind == PropertyKind.SumIndexFields)
		{
			fields = fields.Where((ISymbol symbol) => symbol.Kind == SymbolKind.Field && ((FieldSymbol)symbol).Type.NavTypeKind.IsValidSumIndexFieldValue());
		}
		return fields;
	}
}
