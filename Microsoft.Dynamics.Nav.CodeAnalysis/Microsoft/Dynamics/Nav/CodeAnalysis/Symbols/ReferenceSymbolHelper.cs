using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal static class ReferenceSymbolHelper
{
	internal static Lazy<ImmutableArray<PropertySymbol>> BuildPropertiesLazy(Symbol containingSymbol, LanguageElementWithProperties element)
	{
		return CreateLazy(() => BuildProperties(containingSymbol, element));
	}

	internal static Lazy<ImmutableArray<VariableSymbol>> BuildGlobalVariablesLazy(ContainerSymbol containingSymbol, IElementWithVariables element)
	{
		return CreateLazy(() => BuildGlobalVariables(containingSymbol, element));
	}

	internal static Lazy<ImmutableArray<AttributeSymbol>> BuildAttributesLazy(Symbol containingSymbol, IElementWithAttributes element)
	{
		return CreateLazy(() => BuildAttributes(containingSymbol, element));
	}

	internal static Lazy<ImmutableArray<ParameterSymbol>> BuildParametersLazy(Symbol containingSymbol, IElementWithParameters element)
	{
		return CreateLazy(() => BuildParameters(containingSymbol, element));
	}

	internal static Lazy<T> CreateLazy<T>(Func<T> factory)
	{
		return new Lazy<T>(factory, LazyThreadSafetyMode.PublicationOnly);
	}

	private static ImmutableArray<VariableSymbol> BuildGlobalVariables(ContainerSymbol containingSymbol, IElementWithVariables element)
	{
		if (element.Variables == null || element.Variables.Length == 0)
		{
			return ImmutableArray<VariableSymbol>.Empty;
		}
		VariableSymbol[] array = new VariableSymbol[element.Variables.Length];
		for (int i = 0; i < element.Variables.Length; i++)
		{
			array[i] = new ReferenceGlobalVariableSymbol(containingSymbol, element.Variables[i]);
		}
		return ImmutableArray.Create(array);
	}

	private static ImmutableArray<PropertySymbol> BuildProperties(Symbol containingSymbol, LanguageElementWithProperties element)
	{
		if (element.Properties == null || element.Properties.Length == 0)
		{
			return ImmutableArray<PropertySymbol>.Empty;
		}
		PropertySymbol[] array = new PropertySymbol[element.Properties.Length];
		for (int i = 0; i < element.Properties.Length; i++)
		{
			array[i] = new ReferencePropertySymbol(containingSymbol, element.Properties[i]);
		}
		return ImmutableArray.Create(array);
	}

	private static ImmutableArray<ParameterSymbol> BuildParameters(Symbol containingSymbol, IElementWithParameters element)
	{
		if (element.Parameters == null || element.Parameters.Length == 0)
		{
			return ImmutableArray<ParameterSymbol>.Empty;
		}
		ParameterSymbol[] array = new ParameterSymbol[element.Parameters.Length];
		for (int i = 0; i < element.Parameters.Length; i++)
		{
			array[i] = new ReferenceParameterSymbol(containingSymbol, element.Parameters[i], i);
		}
		return ImmutableArray.Create(array);
	}

	internal static ImmutableArray<AttributeSymbol> BuildAttributes(Symbol containingSymbol, IElementWithAttributes element)
	{
		if (element.Attributes == null || element.Attributes.Length == 0)
		{
			return ImmutableArray<AttributeSymbol>.Empty;
		}
		AttributeSymbol[] array = new AttributeSymbol[element.Attributes.Length];
		for (int i = 0; i < element.Attributes.Length; i++)
		{
			array[i] = new ReferenceAttributeSymbol(containingSymbol, element.Attributes[i]);
		}
		return ImmutableArray.Create(array);
	}

	internal static TableTypeSymbol GetRelatedTableSymbol(Symbol containingSymbol, string relatedTableNameOrId)
	{
		if (int.TryParse(relatedTableNameOrId, out var result))
		{
			return (TableTypeSymbol)containingSymbol.ContainingModule.GetObjectSymbolsByIdAcrossModules(SymbolKind.Table, result).FirstOrDefault();
		}
		return (TableTypeSymbol)containingSymbol.ContainingModule.GetObjectSymbolsByNameAcrossModules(SymbolKind.Table, relatedTableNameOrId).FirstOrDefault();
	}
}
