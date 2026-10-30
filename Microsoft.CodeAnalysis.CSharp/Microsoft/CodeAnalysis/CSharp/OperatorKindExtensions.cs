using System.Collections.Immutable;
using System.Linq.Expressions;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

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

	public static UnaryOperatorKind Unlifted(this UnaryOperatorKind kind)
	{
		return kind & ~UnaryOperatorKind.Lifted;
	}

	public static bool IsLifted(this UnaryOperatorKind kind)
	{
		return (kind & UnaryOperatorKind.Lifted) != 0;
	}

	public static bool IsChecked(this UnaryOperatorKind kind)
	{
		return (kind & UnaryOperatorKind.Checked) != 0;
	}

	public static bool IsUserDefined(this UnaryOperatorKind kind)
	{
		return (kind & UnaryOperatorKind.TypeMask) == UnaryOperatorKind.UserDefined;
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
				switch (kind.Operator())
				{
				case UnaryOperatorKind.PostfixIncrement:
				case UnaryOperatorKind.PostfixDecrement:
				case UnaryOperatorKind.PrefixIncrement:
				case UnaryOperatorKind.PrefixDecrement:
				case UnaryOperatorKind.UnaryMinus:
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
		if ((uint)(unaryOperatorKind - 17) <= 8u || unaryOperatorKind == UnaryOperatorKind.Enum || unaryOperatorKind == UnaryOperatorKind.Pointer)
		{
			return true;
		}
		return false;
	}

	public static UnaryOperatorKind WithType(this UnaryOperatorKind kind, UnaryOperatorKind type)
	{
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

	public static BinaryOperatorKind Unlifted(this BinaryOperatorKind kind)
	{
		return kind & ~BinaryOperatorKind.Lifted;
	}

	public static BinaryOperatorKind OperatorWithLogical(this BinaryOperatorKind kind)
	{
		return kind & (BinaryOperatorKind.OpMask | BinaryOperatorKind.Logical);
	}

	public static BinaryOperatorKind WithType(this BinaryOperatorKind kind, SpecialType type)
	{
		return type switch
		{
			SpecialType.System_Int32 => kind | BinaryOperatorKind.Int, 
			SpecialType.System_UInt32 => kind | BinaryOperatorKind.UInt, 
			SpecialType.System_Int64 => kind | BinaryOperatorKind.Long, 
			SpecialType.System_UInt64 => kind | BinaryOperatorKind.ULong, 
			_ => throw ExceptionUtilities.UnexpectedValue(type), 
		};
	}

	public static UnaryOperatorKind WithType(this UnaryOperatorKind kind, SpecialType type)
	{
		return type switch
		{
			SpecialType.System_Int32 => kind | UnaryOperatorKind.Int, 
			SpecialType.System_UInt32 => kind | UnaryOperatorKind.UInt, 
			SpecialType.System_Int64 => kind | UnaryOperatorKind.Long, 
			SpecialType.System_UInt64 => kind | UnaryOperatorKind.ULong, 
			_ => throw ExceptionUtilities.UnexpectedValue(type), 
		};
	}

	public static BinaryOperatorKind WithType(this BinaryOperatorKind kind, BinaryOperatorKind type)
	{
		return kind | type;
	}

	public static bool IsLifted(this BinaryOperatorKind kind)
	{
		return (kind & BinaryOperatorKind.Lifted) != 0;
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

	public static bool IsChecked(this BinaryOperatorKind kind)
	{
		return (kind & BinaryOperatorKind.Checked) != 0;
	}

	public static bool EmitsAsCheckedInstruction(this BinaryOperatorKind kind)
	{
		if (!kind.IsChecked())
		{
			return false;
		}
		BinaryOperatorKind binaryOperatorKind = kind.Operator();
		if (binaryOperatorKind == BinaryOperatorKind.Multiplication || binaryOperatorKind == BinaryOperatorKind.Addition || binaryOperatorKind == BinaryOperatorKind.Subtraction)
		{
			return true;
		}
		return false;
	}

	public static BinaryOperatorKind WithOverflowChecksIfApplicable(this BinaryOperatorKind kind, bool enabled)
	{
		if (enabled)
		{
			if (kind.IsDynamic())
			{
				return kind | BinaryOperatorKind.Checked;
			}
			if (kind.IsIntegral())
			{
				switch (kind.Operator())
				{
				case BinaryOperatorKind.Multiplication:
				case BinaryOperatorKind.Addition:
				case BinaryOperatorKind.Subtraction:
				case BinaryOperatorKind.Division:
					return kind | BinaryOperatorKind.Checked;
				}
			}
			return kind;
		}
		return kind & ~BinaryOperatorKind.Checked;
	}

	public static bool IsEnum(this BinaryOperatorKind kind)
	{
		BinaryOperatorKind binaryOperatorKind = kind.OperandTypes();
		if ((uint)(binaryOperatorKind - 35) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsEnum(this UnaryOperatorKind kind)
	{
		return kind.OperandTypes() == UnaryOperatorKind.Enum;
	}

	public static bool IsIntegral(this BinaryOperatorKind kind)
	{
		switch (kind.OperandTypes())
		{
		case BinaryOperatorKind.Int:
		case BinaryOperatorKind.UInt:
		case BinaryOperatorKind.Long:
		case BinaryOperatorKind.ULong:
		case BinaryOperatorKind.Char:
		case BinaryOperatorKind.Enum:
		case BinaryOperatorKind.EnumAndUnderlying:
		case BinaryOperatorKind.UnderlyingAndEnum:
		case BinaryOperatorKind.Pointer:
		case BinaryOperatorKind.PointerAndInt:
		case BinaryOperatorKind.PointerAndUInt:
		case BinaryOperatorKind.PointerAndLong:
		case BinaryOperatorKind.PointerAndULong:
		case BinaryOperatorKind.IntAndPointer:
		case BinaryOperatorKind.UIntAndPointer:
		case BinaryOperatorKind.LongAndPointer:
		case BinaryOperatorKind.ULongAndPointer:
			return true;
		default:
			return false;
		}
	}

	public static bool IsLogical(this BinaryOperatorKind kind)
	{
		return (kind & BinaryOperatorKind.Logical) != 0;
	}

	public static BinaryOperatorKind OperandTypes(this BinaryOperatorKind kind)
	{
		return kind & BinaryOperatorKind.TypeMask;
	}

	public static bool IsUserDefined(this BinaryOperatorKind kind)
	{
		return (kind & BinaryOperatorKind.TypeMask) == BinaryOperatorKind.UserDefined;
	}

	public static bool IsShift(this BinaryOperatorKind kind)
	{
		BinaryOperatorKind binaryOperatorKind = kind.Operator();
		if (binaryOperatorKind != BinaryOperatorKind.LeftShift)
		{
			return binaryOperatorKind == BinaryOperatorKind.RightShift;
		}
		return true;
	}

	public static ExpressionType ToExpressionType(this BinaryOperatorKind kind, bool isCompoundAssignment)
	{
		if (isCompoundAssignment)
		{
			switch (kind.Operator())
			{
			case BinaryOperatorKind.Multiplication:
				return ExpressionType.MultiplyAssign;
			case BinaryOperatorKind.Addition:
				return ExpressionType.AddAssign;
			case BinaryOperatorKind.Subtraction:
				return ExpressionType.SubtractAssign;
			case BinaryOperatorKind.Division:
				return ExpressionType.DivideAssign;
			case BinaryOperatorKind.Remainder:
				return ExpressionType.ModuloAssign;
			case BinaryOperatorKind.LeftShift:
				return ExpressionType.LeftShiftAssign;
			case BinaryOperatorKind.RightShift:
				return ExpressionType.RightShiftAssign;
			case BinaryOperatorKind.And:
				return ExpressionType.AndAssign;
			case BinaryOperatorKind.Xor:
				return ExpressionType.ExclusiveOrAssign;
			case BinaryOperatorKind.Or:
				return ExpressionType.OrAssign;
			}
		}
		else
		{
			switch (kind.Operator())
			{
			case BinaryOperatorKind.Multiplication:
				return ExpressionType.Multiply;
			case BinaryOperatorKind.Addition:
				return ExpressionType.Add;
			case BinaryOperatorKind.Subtraction:
				return ExpressionType.Subtract;
			case BinaryOperatorKind.Division:
				return ExpressionType.Divide;
			case BinaryOperatorKind.Remainder:
				return ExpressionType.Modulo;
			case BinaryOperatorKind.LeftShift:
				return ExpressionType.LeftShift;
			case BinaryOperatorKind.RightShift:
				return ExpressionType.RightShift;
			case BinaryOperatorKind.Equal:
				return ExpressionType.Equal;
			case BinaryOperatorKind.NotEqual:
				return ExpressionType.NotEqual;
			case BinaryOperatorKind.GreaterThan:
				return ExpressionType.GreaterThan;
			case BinaryOperatorKind.LessThan:
				return ExpressionType.LessThan;
			case BinaryOperatorKind.GreaterThanOrEqual:
				return ExpressionType.GreaterThanOrEqual;
			case BinaryOperatorKind.LessThanOrEqual:
				return ExpressionType.LessThanOrEqual;
			case BinaryOperatorKind.And:
				return ExpressionType.And;
			case BinaryOperatorKind.Xor:
				return ExpressionType.ExclusiveOr;
			case BinaryOperatorKind.Or:
				return ExpressionType.Or;
			}
		}
		throw ExceptionUtilities.UnexpectedValue(kind.Operator());
	}

	public static ExpressionType ToExpressionType(this UnaryOperatorKind kind)
	{
		switch (kind.Operator())
		{
		case UnaryOperatorKind.PostfixIncrement:
		case UnaryOperatorKind.PrefixIncrement:
			return ExpressionType.Increment;
		case UnaryOperatorKind.PostfixDecrement:
		case UnaryOperatorKind.PrefixDecrement:
			return ExpressionType.Decrement;
		case UnaryOperatorKind.UnaryPlus:
			return ExpressionType.UnaryPlus;
		case UnaryOperatorKind.UnaryMinus:
			return ExpressionType.Negate;
		case UnaryOperatorKind.LogicalNegation:
			return ExpressionType.Not;
		case UnaryOperatorKind.BitwiseComplement:
			return ExpressionType.OnesComplement;
		case UnaryOperatorKind.True:
			return ExpressionType.IsTrue;
		case UnaryOperatorKind.False:
			return ExpressionType.IsFalse;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind.Operator());
		}
	}

	public static RefKind RefKinds(this ImmutableArray<RefKind> ArgumentRefKinds, int index)
	{
		if (!ArgumentRefKinds.IsDefault && index < ArgumentRefKinds.Length)
		{
			return ArgumentRefKinds[index];
		}
		return RefKind.None;
	}
}
