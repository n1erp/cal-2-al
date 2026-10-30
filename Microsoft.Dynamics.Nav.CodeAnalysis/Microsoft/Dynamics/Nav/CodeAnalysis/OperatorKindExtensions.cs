using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class OperatorKindExtensions
{
	public static int OperatorIndex(this UnaryOperatorKind kind)
	{
		return ((int)kind.Operator() >> 8) - 16;
	}

	public static UnaryOperatorKind Operator(this UnaryOperatorKind kind)
	{
		return kind & UnaryOperatorKind.OpMask;
	}

	public static bool IsChecked(this UnaryOperatorKind kind)
	{
		return (kind & UnaryOperatorKind.Checked) != 0;
	}

	public static UnaryOperatorKind OverflowChecks(this UnaryOperatorKind kind)
	{
		return kind & UnaryOperatorKind.Checked;
	}

	public static UnaryOperatorKind WithOverflowChecksIfApplicable(this UnaryOperatorKind kind, bool enabled)
	{
		if (enabled)
		{
			if (kind.IsDynamic())
			{
				return kind | UnaryOperatorKind.Checked;
			}
			if (kind.IsIntegral())
			{
				UnaryOperatorKind unaryOperatorKind = kind.Operator();
				if (unaryOperatorKind == UnaryOperatorKind.UnaryMinus)
				{
					return kind | UnaryOperatorKind.Checked;
				}
			}
			return kind;
		}
		return kind & ~UnaryOperatorKind.Checked;
	}

	public static UnaryOperatorKind OperandTypes(this UnaryOperatorKind kind)
	{
		return kind & UnaryOperatorKind.TypeMask;
	}

	public static bool IsDynamic(this UnaryOperatorKind kind)
	{
		return kind.OperandTypes() == UnaryOperatorKind.Dynamic;
	}

	public static bool IsIntegral(this UnaryOperatorKind kind)
	{
		UnaryOperatorKind unaryOperatorKind = kind.OperandTypes();
		if ((uint)(unaryOperatorKind - 17) <= 8u || unaryOperatorKind == UnaryOperatorKind.Enum)
		{
			return true;
		}
		return false;
	}

	public static UnaryOperatorKind WithType(this UnaryOperatorKind kind, UnaryOperatorKind type)
	{
		DebugAssertHelper.Assert(kind == (kind & ~UnaryOperatorKind.TypeMask));
		DebugAssertHelper.Assert(type == (type & UnaryOperatorKind.TypeMask));
		return kind | type;
	}

	public static int OperatorIndex(this BinaryOperatorKind kind)
	{
		return ((int)kind.Operator() >> 8) - 16;
	}

	public static BinaryOperatorKind Operator(this BinaryOperatorKind kind)
	{
		return kind & BinaryOperatorKind.OpMask;
	}

	public static BinaryOperatorKind OperatorWithLogical(this BinaryOperatorKind kind)
	{
		return kind & (BinaryOperatorKind.OpMask | BinaryOperatorKind.Logical);
	}

	public static BinaryOperatorKind WithType(this BinaryOperatorKind kind, NavTypeKind type)
	{
		DebugAssertHelper.Assert(kind == (kind & ~BinaryOperatorKind.TypeMask));
		return type switch
		{
			NavTypeKind.Integer => kind | BinaryOperatorKind.Int, 
			NavTypeKind.BigInteger => kind | BinaryOperatorKind.Long, 
			_ => throw ExceptionUtilities.UnexpectedValue(type), 
		};
	}

	public static UnaryOperatorKind WithType(this UnaryOperatorKind kind, NavTypeKind type)
	{
		DebugAssertHelper.Assert(kind == (kind & ~UnaryOperatorKind.TypeMask));
		return type switch
		{
			NavTypeKind.Integer => kind | UnaryOperatorKind.Int, 
			NavTypeKind.BigInteger => kind | UnaryOperatorKind.Long, 
			_ => throw ExceptionUtilities.UnexpectedValue(type), 
		};
	}

	public static BinaryOperatorKind WithType(this BinaryOperatorKind kind, BinaryOperatorKind type)
	{
		DebugAssertHelper.Assert(kind == (kind & ~BinaryOperatorKind.TypeMask));
		DebugAssertHelper.Assert(type == (type & BinaryOperatorKind.TypeMask));
		return kind | type;
	}

	public static bool IsDynamic(this BinaryOperatorKind kind)
	{
		return kind.OperandTypes() == BinaryOperatorKind.Dynamic;
	}

	public static bool IsComparison(this BinaryOperatorKind kind)
	{
		switch (kind.Operator())
		{
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
		case BinaryOperatorKind.GreaterThan:
		case BinaryOperatorKind.LessThan:
		case BinaryOperatorKind.GreaterThanOrEqual:
		case BinaryOperatorKind.LessThanOrEqual:
			return true;
		default:
			return false;
		}
	}

	public static bool IsEqualityComparison(this BinaryOperatorKind kind)
	{
		BinaryOperatorKind binaryOperatorKind = kind.Operator();
		if (binaryOperatorKind == BinaryOperatorKind.Equal || binaryOperatorKind == BinaryOperatorKind.NotEqual)
		{
			return true;
		}
		return false;
	}

	public static bool IsIntegral(this BinaryOperatorKind kind)
	{
		BinaryOperatorKind binaryOperatorKind = kind.OperandTypes();
		if ((uint)(binaryOperatorKind - 21) <= 4u || (uint)(binaryOperatorKind - 35) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsLogical(this BinaryOperatorKind kind)
	{
		return (kind & BinaryOperatorKind.Logical) != 0;
	}

	public static BinaryOperatorKind OperandTypes(this BinaryOperatorKind kind)
	{
		return kind & BinaryOperatorKind.TypeMask;
	}

	internal static UnaryOperatorKind MapUnaryFilterExpression(SyntaxNode node)
	{
		return node.Kind switch
		{
			SyntaxKind.UnaryEqualsFilterExpression => UnaryOperatorKind.FilterEquals, 
			SyntaxKind.UnaryNotEqualsFilterExpression => UnaryOperatorKind.FilterNotEquals, 
			SyntaxKind.UnaryLessThanFilterExpression => UnaryOperatorKind.FilterLessThan, 
			SyntaxKind.UnaryLessThanEqualsFilterExpression => UnaryOperatorKind.FilterLessThanEquals, 
			SyntaxKind.UnaryGreaterThanFilterExpression => UnaryOperatorKind.FilterGreaterThan, 
			SyntaxKind.UnaryGreaterThanEqualsFilterExpression => UnaryOperatorKind.FilterGreaterThanEquals, 
			SyntaxKind.RangeToFilterExpression => UnaryOperatorKind.FilterRangeTo, 
			SyntaxKind.RangeFromFilterExpression => UnaryOperatorKind.FilterRangeFrom, 
			_ => UnaryOperatorKind.Error, 
		};
	}

	internal static BinaryOperatorKind MapBinaryFilterExpression(SyntaxNode node)
	{
		return node.Kind switch
		{
			SyntaxKind.AndFilterExpression => BinaryOperatorKind.FilterAnd, 
			SyntaxKind.OrFilterExpression => BinaryOperatorKind.FilterOr, 
			SyntaxKind.RangeBetweenFilterExpression => BinaryOperatorKind.FilterRangeBetween, 
			_ => BinaryOperatorKind.Error, 
		};
	}
}
