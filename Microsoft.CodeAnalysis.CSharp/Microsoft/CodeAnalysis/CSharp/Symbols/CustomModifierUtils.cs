using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols.Metadata.PE;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class CustomModifierUtils
{
	internal static void CopyMethodCustomModifiers(MethodSymbol sourceMethod, MethodSymbol destinationMethod, out TypeSymbol returnType, out CustomModifiersTuple customModifiers, out ImmutableArray<ParameterSymbol> parameters, bool alsoCopyParamsModifier)
	{
		MethodSymbol methodSymbol = sourceMethod.ConstructIfGeneric(destinationMethod.TypeArguments);
		customModifiers = CustomModifiersTuple.Create(methodSymbol.ReturnTypeCustomModifiers, (destinationMethod.RefKind != RefKind.None) ? methodSymbol.RefCustomModifiers : ImmutableArray<CustomModifier>.Empty);
		parameters = CopyParameterCustomModifiers(methodSymbol.Parameters, destinationMethod.Parameters, alsoCopyParamsModifier);
		returnType = destinationMethod.ReturnType;
		TypeSymbol returnType2 = methodSymbol.ReturnType;
		if (returnType.Equals(returnType2, TypeCompareKind.AllIgnoreOptions))
		{
			returnType = CopyTypeCustomModifiers(returnType2, returnType, destinationMethod.ContainingAssembly);
		}
	}

	internal static TypeSymbol CopyTypeCustomModifiers(TypeSymbol sourceType, TypeSymbol destinationType, AssemblySymbol containingAssembly)
	{
		ImmutableArray<bool> dynamicTransformFlags = CSharpCompilation.DynamicTransformsEncoder.EncodeWithoutCustomModifierFlags(destinationType, RefKind.None);
		TypeSymbol typeSymbol = DynamicTypeDecoder.TransformTypeWithoutCustomModifierFlags(sourceType, containingAssembly, RefKind.None, dynamicTransformFlags);
		if (destinationType.ContainsTuple() && !sourceType.Equals(destinationType, TypeCompareKind.IgnoreCustomModifiersAndArraySizesAndLowerBounds | TypeCompareKind.IgnoreDynamic))
		{
			ImmutableArray<string> elementNames = CSharpCompilation.TupleNamesEncoder.Encode(destinationType);
			return TupleTypeDecoder.DecodeTupleTypesIfApplicable(typeSymbol, elementNames);
		}
		return typeSymbol;
	}

	internal static ImmutableArray<ParameterSymbol> CopyParameterCustomModifiers(ImmutableArray<ParameterSymbol> sourceParameters, ImmutableArray<ParameterSymbol> destinationParameters, bool alsoCopyParamsModifier)
	{
		ArrayBuilder<ParameterSymbol> arrayBuilder = null;
		int length = destinationParameters.Length;
		for (int i = 0; i < length; i++)
		{
			SourceParameterSymbolBase sourceParameterSymbolBase = (SourceParameterSymbolBase)destinationParameters[i];
			ParameterSymbol parameterSymbol = sourceParameters[i];
			if (parameterSymbol.CustomModifiers.Any() || parameterSymbol.RefCustomModifiers.Any() || parameterSymbol.Type.HasCustomModifiers(flagNonDefaultArraySizesOrLowerBounds: true) || sourceParameterSymbolBase.CustomModifiers.Any() || sourceParameterSymbolBase.RefCustomModifiers.Any() || sourceParameterSymbolBase.Type.HasCustomModifiers(flagNonDefaultArraySizesOrLowerBounds: true) || (alsoCopyParamsModifier && parameterSymbol.IsParams != sourceParameterSymbolBase.IsParams))
			{
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<ParameterSymbol>.GetInstance();
					arrayBuilder.AddRange(destinationParameters, i);
				}
				bool newIsParams = (alsoCopyParamsModifier ? parameterSymbol.IsParams : sourceParameterSymbolBase.IsParams);
				arrayBuilder.Add(sourceParameterSymbolBase.WithCustomModifiersAndParams(parameterSymbol.Type, parameterSymbol.CustomModifiers, (sourceParameterSymbolBase.RefKind != RefKind.None) ? parameterSymbol.RefCustomModifiers : ImmutableArray<CustomModifier>.Empty, newIsParams));
			}
			else
			{
				arrayBuilder?.Add(sourceParameterSymbolBase);
			}
		}
		return arrayBuilder?.ToImmutableAndFree() ?? destinationParameters;
	}

	internal static bool HasInAttributeModifier(this ImmutableArray<CustomModifier> modifiers)
	{
		return modifiers.Any((CustomModifier modifier) => !modifier.IsOptional && modifier.Modifier.IsWellKnownTypeInAttribute());
	}
}
