using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

public static class TypeSymbolExtensions
{
	[SuppressMessage("Microsoft.Design", "CA1011: ConsiderPassingBaseTypesAsParameters")]
	public static bool IsErrorType(this ITypeSymbol type)
	{
		DebugAssertHelper.Assert(type != null);
		return type.Kind == SymbolKind.ErrorType;
	}

	public static NavTypeKind GetNavTypeKindSafe(this ITypeSymbol type)
	{
		return type?.NavTypeKind ?? NavTypeKind.None;
	}

	public static bool IsTextType(this ITypeSymbol type)
	{
		NavTypeKind navTypeKindSafe = type.GetNavTypeKindSafe();
		if (navTypeKindSafe == NavTypeKind.String || (uint)(navTypeKindSafe - 2228235) <= 1u || (uint)(navTypeKindSafe - 16646153) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool IsArray(this ITypeSymbol type)
	{
		DebugAssertHelper.Assert(type != null);
		if (type == null)
		{
			return false;
		}
		return type.TypeCategory == TypeCategoryKind.Array;
	}

	public static bool IsGenericType(this ITypeSymbol type)
	{
		DebugAssertHelper.Assert(type != null);
		return type.NavTypeKind.IsGenericType();
	}

	public static bool IsNumericType(this ITypeSymbol type)
	{
		return Conversions.IsNumericType(type.GetNavTypeKindSafe());
	}

	public static ITypeSymbol GetTypeSymbol(this ISymbol symbol)
	{
		if (symbol is ISymbolWithType symbolWithType)
		{
			return symbolWithType.Type;
		}
		return symbol as ITypeSymbol;
	}

	public static ITypeSymbol StrippedType(this ITypeSymbol type)
	{
		return type;
	}

	internal static bool IsEnumerableType(this TypeSymbol type)
	{
		NavTypeKind navTypeKindSafe = type.GetNavTypeKindSafe();
		if (navTypeKindSafe == NavTypeKind.DotNet)
		{
			return (type as DotNetTypeSymbol)?.IsEnumerable ?? false;
		}
		return type.IsEnumerableType_Internal();
	}

	internal static TypeSymbol GetEnumeratorElementType(this TypeSymbol type)
	{
		NavTypeKind navTypeKindSafe = type.GetNavTypeKindSafe();
		if (navTypeKindSafe == NavTypeKind.DotNet)
		{
			return ((DotNetTypeSymbol)type).ElementType;
		}
		return type.GetEnumeratorElementType_Internal();
	}

	internal static NavTypeKind GetNavTypeKind(this ISymbol declaringSymbol)
	{
		return declaringSymbol.GetTypeSymbol()?.GetNavTypeKindSafe() ?? NavTypeKind.None;
	}

	internal static bool IsUserType(this ITypeSymbol type)
	{
		switch (type.GetNavTypeKindSafe())
		{
		case NavTypeKind.Table:
		case NavTypeKind.TestRequestPage:
		case NavTypeKind.TestPage:
		case NavTypeKind.Record:
		case NavTypeKind.Codeunit:
		case NavTypeKind.Page:
		case NavTypeKind.Report:
		case NavTypeKind.XmlPort:
		case NavTypeKind.Query:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsIntegralType(this ITypeSymbol type)
	{
		return Conversions.IsIntegralType(type.GetNavTypeKindSafe());
	}

	internal static bool IsBoolean(this ITypeSymbol type)
	{
		return type.GetNavTypeKindSafe() == NavTypeKind.Boolean;
	}

	internal static bool IsNoneType(this ITypeSymbol type)
	{
		return type.GetNavTypeKindSafe() == NavTypeKind.None;
	}

	internal static bool IsSerializable(this ITypeSymbol type)
	{
		if (type.Kind == SymbolKind.OptionType || type.Kind == SymbolKind.Enum)
		{
			return true;
		}
		switch (type.GetNavTypeKind())
		{
		case NavTypeKind.JsonObject:
		case NavTypeKind.JsonArray:
		case NavTypeKind.String:
		case NavTypeKind.BigText:
		case NavTypeKind.Byte:
		case NavTypeKind.Char:
		case NavTypeKind.DateFormula:
		case NavTypeKind.Boolean:
		case NavTypeKind.Integer:
		case NavTypeKind.BigInteger:
		case NavTypeKind.Decimal:
		case NavTypeKind.DateTime:
		case NavTypeKind.Time:
		case NavTypeKind.Date:
		case NavTypeKind.Duration:
		case NavTypeKind.Guid:
		case NavTypeKind.Text:
		case NavTypeKind.Code:
			return true;
		default:
			return false;
		}
	}

	internal static TypeSymbol GetTypeSymbol(this Symbol symbol)
	{
		if (symbol is ISymbolWithType symbolWithType)
		{
			return symbolWithType.Type;
		}
		return symbol as TypeSymbol;
	}

	internal static bool IsRecordType(this TypeSymbol symbol)
	{
		SymbolKind kind = symbol.Kind;
		if (kind == SymbolKind.Table || kind == SymbolKind.Record)
		{
			return true;
		}
		return false;
	}

	internal static bool IsCurrObjectType(this TypeSymbol symbol)
	{
		NavTypeKind navTypeKindSafe = symbol.GetNavTypeKindSafe();
		if ((uint)(navTypeKindSafe - 202) <= 3u)
		{
			return true;
		}
		return false;
	}

	internal static TypeSymbol GetNativeOptionType(this OptionSymbol option)
	{
		return TypeSymbol.GetNavTypeSymbol((option.Type.NavTypeKind == NavTypeKind.Option) ? NavTypeKind.Integer : option.Type.NavTypeKind);
	}

	internal static bool IsNativeOptionTypeSymbol(this TypeSymbol type)
	{
		if (type == null)
		{
			return false;
		}
		if (type.Kind == SymbolKind.OptionType)
		{
			return type.NavTypeKind == NavTypeKind.Option;
		}
		return false;
	}
}
