using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public static class SemanticFacts
{
	private class NameEqualityComparerImpl : EqualityComparer<string>
	{
		public override bool Equals(string x, string y)
		{
			return StringComparer.OrdinalIgnoreCase.Equals(x, y);
		}

		public override int GetHashCode(string obj)
		{
			return StringComparer.OrdinalIgnoreCase.GetHashCode(obj);
		}
	}

	private class ObjectTypeSymbolComparerImpl : EqualityComparer<IObjectTypeSymbol>
	{
		public override bool Equals(IObjectTypeSymbol x, IObjectTypeSymbol y)
		{
			if (x == y)
			{
				return true;
			}
			if (y == null)
			{
				return false;
			}
			if (x.Kind != y.Kind)
			{
				return false;
			}
			if (x.Kind.IsApplicationObject())
			{
				return ((IApplicationObjectTypeSymbol)x).Id == ((IApplicationObjectTypeSymbol)y).Id;
			}
			return x.Name.Equals(y.Name);
		}

		public override int GetHashCode(IObjectTypeSymbol obj)
		{
			if (obj == null)
			{
				return 0;
			}
			if (obj.Kind.IsApplicationObject())
			{
				return Hash.Combine(((IApplicationObjectTypeSymbol)obj).Id.GetHashCode(), obj.Kind.GetHashCode());
			}
			if (string.IsNullOrEmpty(obj.Name))
			{
				return obj.Kind.GetHashCode();
			}
			return Hash.Combine(obj.Name.GetHashCode(), obj.Kind.GetHashCode());
		}
	}

	internal const int ArrayMaxNoOfElements = 1000000;

	internal const decimal DecimalDelta = 0.0000000000000000000000001m;

	internal const int ArrayMaxNoOfDimensions = 10;

	internal const int TextFieldMaxLength = 250;

	internal const int TextFieldMaxLength_3_0 = 2048;

	internal const int CodeVariableMaxLength = int.MaxValue;

	internal const int CodeVariableMaxLength_3_0 = 2048;

	internal const int CodeFieldMaxLength = 250;

	internal const int CodeFieldMaxLength_3_0 = 2048;

	internal const int ApplicationObjectNameMaxLength = 30;

	internal const int TableFieldNameMaxLength = 30;

	internal const int TableKeyNameMaxLength = 28;

	internal const string ViewSupportedTypes = "List";

	public static EqualityComparer<string> NameEqualityComparer { get; } = new NameEqualityComparerImpl();

	public static IComparer<string> NameComparer { get; } = StringComparer.OrdinalIgnoreCase;

	internal static EqualityComparer<IObjectTypeSymbol> ObjectTypeSymbolEqualityComparer { get; } = new ObjectTypeSymbolComparerImpl();

	public static bool IsSameName(string nameA, string nameB)
	{
		return NameEqualityComparer.Equals(nameA, nameB);
	}

	public static bool TryGetNavTypeKindFromErrorTypeSymbol(ITypeSymbol symbol, out NavTypeKind result)
	{
		result = NavTypeKind.None;
		if (symbol.Kind != SymbolKind.ErrorType)
		{
			return false;
		}
		ExtendedErrorTypeSymbol extendedErrorTypeSymbol = symbol as ExtendedErrorTypeSymbol;
		if (extendedErrorTypeSymbol == null)
		{
			return false;
		}
		result = extendedErrorTypeSymbol.IntendedKind;
		return true;
	}

	public static bool TryGetArrayTypeFromTypeSymbol(ITypeSymbol symbol, out NavTypeKind result)
	{
		result = NavTypeKind.None;
		if (symbol.Kind != SymbolKind.ArrayType)
		{
			return false;
		}
		ArrayTypeSymbol arrayTypeSymbol = symbol as ArrayTypeSymbol;
		if (arrayTypeSymbol?.ElementType == null)
		{
			return false;
		}
		result = arrayTypeSymbol.ElementType.NavTypeKind;
		if (arrayTypeSymbol.ElementType.Kind == SymbolKind.ErrorType)
		{
			return TryGetNavTypeKindFromErrorTypeSymbol(arrayTypeSymbol.ElementType, out result);
		}
		return true;
	}

	public static IOperation GetBoundExpressionArgument(IInvocationExpression invocation, int argPos)
	{
		if (!(invocation is BoundCall boundCall))
		{
			return null;
		}
		if (argPos >= boundCall.Arguments.Length)
		{
			return null;
		}
		return boundCall.Arguments[argPos];
	}

	public static bool IsBoundExpression(this IOperation operation)
	{
		return operation is BoundExpression;
	}

	internal static bool IsVariableTypeWritable(this ITypeSymbol type)
	{
		NavTypeKind navTypeKindSafe = type.GetNavTypeKindSafe();
		if ((uint)(navTypeKindSafe - 2228235) <= 1u)
		{
			return false;
		}
		return true;
	}

	internal static bool CanTypeBeUsedInWithStatement(this ITypeSymbol type)
	{
		TypeCategoryKind typeCategory = type.TypeCategory;
		if ((uint)(typeCategory - 2) <= 1u)
		{
			return true;
		}
		return false;
	}

	internal static bool IsValidFieldType(this ITypeSymbol type)
	{
		DebugAssertHelper.Assert(type != null);
		return (type.NavTypeKind & NavTypeKind._Field) != 0;
	}

	internal static bool IsValidVariableType(this ITypeSymbol type)
	{
		DebugAssertHelper.Assert(type != null);
		NavTypeKind navTypeKind = type.NavTypeKind;
		if (type.NavTypeKind == NavTypeKind.Array)
		{
			TypeSymbol elementType = ((ArrayTypeSymbol)type).ElementType;
			if (elementType.IsErrorType())
			{
				return true;
			}
			navTypeKind = elementType.NavTypeKind;
		}
		return (navTypeKind & NavTypeKind._Variable) != 0;
	}

	internal static bool IsValidParameterType(this ITypeSymbol type, bool isVar)
	{
		DebugAssertHelper.Assert(type != null);
		NavTypeKind navTypeKind = type.NavTypeKind;
		if (type.NavTypeKind == NavTypeKind.Array)
		{
			TypeSymbol elementType = ((ArrayTypeSymbol)type).ElementType;
			if (elementType.IsErrorType())
			{
				return true;
			}
			navTypeKind = elementType.NavTypeKind;
		}
		if ((navTypeKind & NavTypeKind._Parameter) == 0)
		{
			return (navTypeKind == NavTypeKind.Dialog) & isVar;
		}
		return true;
	}

	internal static bool IsValidKeyType(this ITypeSymbol type)
	{
		DebugAssertHelper.Assert(type != null);
		return (type.NavTypeKind & NavTypeKind._Key) != 0;
	}

	internal static bool HasValidFieldClassForKey(this FieldSymbol field)
	{
		FieldClassKind? enumPropertyValue = field.GetEnumPropertyValue<FieldClassKind>(PropertyKind.FieldClass);
		if (enumPropertyValue.HasValue)
		{
			return enumPropertyValue.Value == FieldClassKind.Normal;
		}
		return true;
	}

	internal static bool HasValidSqlTimestampPropertyForKey(this FieldSymbol field)
	{
		return field.GetSimplePropertyValue<bool>(PropertyKind.SqlTimestamp) != true;
	}

	internal static bool HasValidPropertiesForKey(this FieldSymbol field)
	{
		if (field.HasValidFieldClassForKey())
		{
			return field.HasValidSqlTimestampPropertyForKey();
		}
		return false;
	}

	internal static bool IsValidReturnType(this ITypeSymbol type)
	{
		DebugAssertHelper.Assert(type != null);
		return (type.NavTypeKind & NavTypeKind._ReturnValue) != 0;
	}

	internal static bool IsValidPageFieldType(this ITypeSymbol type)
	{
		DebugAssertHelper.Assert(type != null);
		return type.NavTypeKind.IsValidControlField();
	}

	internal static bool IsValidColumnType(ITypeSymbol type)
	{
		return type.NavTypeKind.IsValidColumnKind();
	}

	internal static bool IsValidColumnTypeForAggregationMethod(ITypeSymbol type)
	{
		return type.GetNavTypeKindSafe().IsValidColumnTypeForAggregationMethod();
	}

	internal static bool IsValidColumnTypeForDateMethod(ITypeSymbol type)
	{
		DebugAssertHelper.Assert(type != null);
		return type.GetNavTypeKindSafe().IsValidColumnTypeForDateMethod();
	}

	internal static bool IsValidOptionType(this ITypeSymbol type)
	{
		if (type.TypeCategory == TypeCategoryKind.Option)
		{
			return true;
		}
		if (type.IsArray() && ((ArrayTypeSymbol)type).ElementType.TypeCategory == TypeCategoryKind.Option)
		{
			return true;
		}
		if (type.Kind == SymbolKind.Class && type.TypeCategory == TypeCategoryKind.ApplicationObject)
		{
			return true;
		}
		if (IsDotNetEnum(type))
		{
			return true;
		}
		return false;
	}

	internal static bool IsValidOptionMember(this OptionTypeSymbol optionSymbol, int optionIndex)
	{
		if (optionIndex < 0 || optionIndex >= optionSymbol.EffectiveOptionsCount)
		{
			return false;
		}
		return true;
	}

	internal static bool IsValidForLoopVariableType(this ITypeSymbol type)
	{
		DebugAssertHelper.Assert(type != null);
		if (type.IsNumericType())
		{
			return true;
		}
		switch (type.NavTypeKind)
		{
		case NavTypeKind.Byte:
		case NavTypeKind.Char:
		case NavTypeKind.Boolean:
		case NavTypeKind.Option:
		case NavTypeKind.Time:
		case NavTypeKind.Date:
		case NavTypeKind.Enum:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsPartPageType(this PageTypeKind pageType)
	{
		if ((uint)(pageType - 3) <= 1u || pageType == PageTypeKind.HeadlinePart)
		{
			return true;
		}
		return false;
	}

	internal static bool IsViewSupported(this PageTypeKind pageType)
	{
		if (pageType == PageTypeKind.List)
		{
			return true;
		}
		return false;
	}

	internal static bool IsActionAreaDedicatedToRoleCenter(this ActionAreaKind actionAreaKind)
	{
		if (actionAreaKind != ActionAreaKind.Embedding)
		{
			return actionAreaKind == ActionAreaKind.Sections;
		}
		return true;
	}

	internal static bool IsAggregationMethod(this QueryColumnMethodKind kind)
	{
		switch (kind)
		{
		case QueryColumnMethodKind.Day:
		case QueryColumnMethodKind.Month:
		case QueryColumnMethodKind.Year:
			return false;
		case QueryColumnMethodKind.Sum:
		case QueryColumnMethodKind.Count:
		case QueryColumnMethodKind.Average:
		case QueryColumnMethodKind.Min:
		case QueryColumnMethodKind.Max:
			return true;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	internal static bool IsCountMethod(this QueryColumnMethodKind kind)
	{
		return kind == QueryColumnMethodKind.Count;
	}

	internal static bool IsDateMethod(this QueryColumnMethodKind kind)
	{
		switch (kind)
		{
		case QueryColumnMethodKind.Day:
		case QueryColumnMethodKind.Month:
		case QueryColumnMethodKind.Year:
			return true;
		case QueryColumnMethodKind.Sum:
		case QueryColumnMethodKind.Count:
		case QueryColumnMethodKind.Average:
		case QueryColumnMethodKind.Min:
		case QueryColumnMethodKind.Max:
			return false;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	internal static bool IsValidInDataSetType(this NavTypeKind typeKind)
	{
		switch (typeKind)
		{
		case NavTypeKind.Char:
		case NavTypeKind.Boolean:
		case NavTypeKind.Integer:
		case NavTypeKind.BigInteger:
		case NavTypeKind.Decimal:
		case NavTypeKind.Option:
		case NavTypeKind.Enum:
		case NavTypeKind.Text:
		case NavTypeKind.Code:
			return true;
		default:
			return false;
		}
	}

	internal static bool IsDeclaredSymbol(this ISymbol symbol)
	{
		SymbolKind kind = symbol.Kind;
		if (kind == SymbolKind.Change)
		{
			return false;
		}
		return true;
	}

	internal static bool IsValidForExpression(this ISymbol symbol)
	{
		ITypeSymbol typeSymbol = symbol.GetTypeSymbol();
		if (typeSymbol == null)
		{
			return false;
		}
		if (typeSymbol.NavTypeKind == NavTypeKind.None && typeSymbol.Kind != SymbolKind.Class)
		{
			return false;
		}
		return true;
	}

	internal static bool IsStatementExpression(BoundExpression expression)
	{
		switch (expression.Syntax.Kind)
		{
		case SyntaxKind.AssignmentStatement:
		case SyntaxKind.InvocationExpression:
			return true;
		case SyntaxKind.MemberAccessExpression:
		case SyntaxKind.IdentifierName:
			return expression.Kind == BoundKind.Call;
		default:
			return false;
		}
	}

	internal static bool IsPersonalizationObject(this ISymbol symbol)
	{
		return symbol.IsKind(SymbolKind.Profile, SymbolKind.PageCustomization);
	}

	private static bool IsDotNetEnum(ITypeSymbol type)
	{
		if (type.Kind == SymbolKind.DotNet)
		{
			return ((IDotNetTypeSymbol)type).DotNetTypeInfo.IsEnum;
		}
		return false;
	}
}
