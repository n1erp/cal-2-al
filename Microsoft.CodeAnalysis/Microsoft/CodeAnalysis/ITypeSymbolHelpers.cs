namespace Microsoft.CodeAnalysis;

internal static class ITypeSymbolHelpers
{
	internal static bool IsNullableType(ITypeSymbol typeOpt)
	{
		if (typeOpt == null)
		{
			return false;
		}
		return typeOpt.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
	}

	internal static bool IsNullableOfBoolean(ITypeSymbol type)
	{
		if (IsNullableType(type))
		{
			return IsBooleanType(GetNullableUnderlyingType(type));
		}
		return false;
	}

	internal static ITypeSymbol GetNullableUnderlyingType(ITypeSymbol type)
	{
		return ((INamedTypeSymbol)type).TypeArguments[0];
	}

	internal static bool IsBooleanType(ITypeSymbol type)
	{
		if (type == null)
		{
			return false;
		}
		return type.SpecialType == SpecialType.System_Boolean;
	}

	internal static bool IsObjectType(ITypeSymbol type)
	{
		if (type == null)
		{
			return false;
		}
		return type.SpecialType == SpecialType.System_Object;
	}

	internal static bool IsSignedIntegralType(ITypeSymbol type)
	{
		return type?.SpecialType.IsSignedIntegralType() ?? false;
	}

	internal static bool IsUnsignedIntegralType(ITypeSymbol type)
	{
		return type?.SpecialType.IsUnsignedIntegralType() ?? false;
	}

	internal static bool IsNumericType(ITypeSymbol type)
	{
		return type?.SpecialType.IsNumericType() ?? false;
	}

	internal static ITypeSymbol GetEnumUnderlyingType(ITypeSymbol type)
	{
		return (type as INamedTypeSymbol)?.EnumUnderlyingType;
	}

	internal static ITypeSymbol GetEnumUnderlyingTypeOrSelf(ITypeSymbol type)
	{
		return GetEnumUnderlyingType(type) ?? type;
	}

	internal static bool IsDynamicType(ITypeSymbol type)
	{
		if (type == null)
		{
			return false;
		}
		return type.Kind == SymbolKind.DynamicType;
	}
}
