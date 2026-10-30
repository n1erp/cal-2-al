using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class Expression
{
	internal static UnaryOperationKind DeriveUnaryOperationKind(UnaryOperatorKind operatorKind)
	{
		switch (operatorKind & UnaryOperatorKind.OpMask)
		{
		case UnaryOperatorKind.UnaryPlus:
			switch (operatorKind & UnaryOperatorKind.TypeMask)
			{
			case UnaryOperatorKind.Int:
			case UnaryOperatorKind.UInt:
			case UnaryOperatorKind.Long:
			case UnaryOperatorKind.ULong:
				return UnaryOperationKind.IntegerPlus;
			case UnaryOperatorKind.Decimal:
				return UnaryOperationKind.DecimalPlus;
			}
			break;
		case UnaryOperatorKind.UnaryMinus:
			switch (operatorKind & UnaryOperatorKind.TypeMask)
			{
			case UnaryOperatorKind.Int:
			case UnaryOperatorKind.UInt:
			case UnaryOperatorKind.Long:
			case UnaryOperatorKind.ULong:
				return UnaryOperationKind.IntegerMinus;
			case UnaryOperatorKind.Decimal:
				return UnaryOperationKind.DecimalMinus;
			}
			break;
		case UnaryOperatorKind.LogicalNegation:
		{
			UnaryOperatorKind unaryOperatorKind = operatorKind & UnaryOperatorKind.TypeMask;
			if (unaryOperatorKind == UnaryOperatorKind.Bool)
			{
				return UnaryOperationKind.BooleanLogicalNot;
			}
			break;
		}
		}
		return UnaryOperationKind.Invalid;
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
	internal static BinaryOperationKind DeriveBinaryOperationKind(BinaryOperatorKind operatorKind)
	{
		switch (operatorKind & BinaryOperatorKind.OpMask)
		{
		case BinaryOperatorKind.Addition:
			switch (operatorKind & BinaryOperatorKind.TypeMask)
			{
			case BinaryOperatorKind.Int:
			case BinaryOperatorKind.Long:
				return BinaryOperationKind.IntegerAdd;
			case BinaryOperatorKind.Decimal:
				return BinaryOperationKind.DecimalAdd;
			case BinaryOperatorKind.EnumAndUnderlying:
			case BinaryOperatorKind.UnderlyingAndEnum:
				return BinaryOperationKind.EnumAdd;
			case BinaryOperatorKind.String:
			case BinaryOperatorKind.StringAndObject:
			case BinaryOperatorKind.ObjectAndString:
				return BinaryOperationKind.StringConcatenate;
			}
			break;
		case BinaryOperatorKind.Subtraction:
			switch (operatorKind & BinaryOperatorKind.TypeMask)
			{
			case BinaryOperatorKind.Int:
			case BinaryOperatorKind.Long:
				return BinaryOperationKind.IntegerSubtract;
			case BinaryOperatorKind.Decimal:
				return BinaryOperationKind.DecimalSubtract;
			case BinaryOperatorKind.EnumAndUnderlying:
			case BinaryOperatorKind.UnderlyingAndEnum:
				return BinaryOperationKind.EnumSubtract;
			}
			break;
		case BinaryOperatorKind.Multiplication:
			switch (operatorKind & BinaryOperatorKind.TypeMask)
			{
			case BinaryOperatorKind.Int:
			case BinaryOperatorKind.Long:
				return BinaryOperationKind.IntegerMultiply;
			case BinaryOperatorKind.Decimal:
				return BinaryOperationKind.DecimalMultiply;
			}
			break;
		case BinaryOperatorKind.Division:
			switch (operatorKind & BinaryOperatorKind.TypeMask)
			{
			case BinaryOperatorKind.Int:
			case BinaryOperatorKind.Long:
				return BinaryOperationKind.IntegerDivide;
			case BinaryOperatorKind.Decimal:
				return BinaryOperationKind.DecimalDivide;
			}
			break;
		case BinaryOperatorKind.Remainder:
		{
			BinaryOperatorKind binaryOperatorKind = operatorKind & BinaryOperatorKind.TypeMask;
			if (binaryOperatorKind == BinaryOperatorKind.Int || binaryOperatorKind == BinaryOperatorKind.Long)
			{
				return BinaryOperationKind.IntegerRemainder;
			}
			break;
		}
		case BinaryOperatorKind.And:
			switch (operatorKind & BinaryOperatorKind.TypeMask)
			{
			case BinaryOperatorKind.Int:
			case BinaryOperatorKind.Long:
				return BinaryOperationKind.IntegerAnd;
			case BinaryOperatorKind.Bool:
				if ((operatorKind & BinaryOperatorKind.Logical) != 0)
				{
					return BinaryOperationKind.BooleanConditionalAnd;
				}
				return BinaryOperationKind.BooleanAnd;
			case BinaryOperatorKind.Enum:
				return BinaryOperationKind.EnumAnd;
			}
			break;
		case BinaryOperatorKind.Or:
			switch (operatorKind & BinaryOperatorKind.TypeMask)
			{
			case BinaryOperatorKind.Int:
			case BinaryOperatorKind.Long:
				return BinaryOperationKind.IntegerOr;
			case BinaryOperatorKind.Bool:
				if ((operatorKind & BinaryOperatorKind.Logical) != 0)
				{
					return BinaryOperationKind.BooleanConditionalOr;
				}
				return BinaryOperationKind.BooleanOr;
			case BinaryOperatorKind.Enum:
				return BinaryOperationKind.EnumOr;
			}
			break;
		case BinaryOperatorKind.Xor:
			switch (operatorKind & BinaryOperatorKind.TypeMask)
			{
			case BinaryOperatorKind.Int:
			case BinaryOperatorKind.Long:
				return BinaryOperationKind.IntegerExclusiveOr;
			case BinaryOperatorKind.Bool:
				return BinaryOperationKind.BooleanExclusiveOr;
			case BinaryOperatorKind.Enum:
				return BinaryOperationKind.EnumExclusiveOr;
			}
			break;
		case BinaryOperatorKind.LessThan:
			switch (operatorKind & BinaryOperatorKind.TypeMask)
			{
			case BinaryOperatorKind.Int:
			case BinaryOperatorKind.Long:
				return BinaryOperationKind.IntegerLessThan;
			case BinaryOperatorKind.Decimal:
				return BinaryOperationKind.DecimalLessThan;
			case BinaryOperatorKind.Enum:
				return BinaryOperationKind.EnumLessThan;
			}
			break;
		case BinaryOperatorKind.LessThanOrEqual:
			switch (operatorKind & BinaryOperatorKind.TypeMask)
			{
			case BinaryOperatorKind.Int:
			case BinaryOperatorKind.Long:
				return BinaryOperationKind.IntegerLessThanOrEqual;
			case BinaryOperatorKind.Decimal:
				return BinaryOperationKind.DecimalLessThanOrEqual;
			case BinaryOperatorKind.Enum:
				return BinaryOperationKind.EnumLessThanOrEqual;
			}
			break;
		case BinaryOperatorKind.Equal:
			switch (operatorKind & BinaryOperatorKind.TypeMask)
			{
			case BinaryOperatorKind.Int:
			case BinaryOperatorKind.UInt:
			case BinaryOperatorKind.Long:
			case BinaryOperatorKind.ULong:
				return BinaryOperationKind.IntegerEquals;
			case BinaryOperatorKind.Decimal:
				return BinaryOperationKind.DecimalEquals;
			case BinaryOperatorKind.Enum:
				return BinaryOperationKind.EnumEquals;
			case BinaryOperatorKind.Bool:
				return BinaryOperationKind.BooleanEquals;
			case BinaryOperatorKind.String:
				return BinaryOperationKind.StringEquals;
			}
			break;
		case BinaryOperatorKind.NotEqual:
			switch (operatorKind & BinaryOperatorKind.TypeMask)
			{
			case BinaryOperatorKind.Int:
			case BinaryOperatorKind.UInt:
			case BinaryOperatorKind.Long:
			case BinaryOperatorKind.ULong:
				return BinaryOperationKind.IntegerNotEquals;
			case BinaryOperatorKind.Decimal:
				return BinaryOperationKind.DecimalNotEquals;
			case BinaryOperatorKind.Enum:
				return BinaryOperationKind.EnumNotEquals;
			case BinaryOperatorKind.Bool:
				return BinaryOperationKind.BooleanNotEquals;
			case BinaryOperatorKind.String:
				return BinaryOperationKind.StringNotEquals;
			}
			break;
		case BinaryOperatorKind.GreaterThanOrEqual:
			switch (operatorKind & BinaryOperatorKind.TypeMask)
			{
			case BinaryOperatorKind.Int:
			case BinaryOperatorKind.Long:
				return BinaryOperationKind.IntegerGreaterThanOrEqual;
			case BinaryOperatorKind.Decimal:
				return BinaryOperationKind.DecimalGreaterThanOrEqual;
			case BinaryOperatorKind.Enum:
				return BinaryOperationKind.EnumGreaterThanOrEqual;
			}
			break;
		case BinaryOperatorKind.GreaterThan:
			switch (operatorKind & BinaryOperatorKind.TypeMask)
			{
			case BinaryOperatorKind.Int:
			case BinaryOperatorKind.Long:
				return BinaryOperationKind.IntegerGreaterThan;
			case BinaryOperatorKind.Decimal:
				return BinaryOperationKind.DecimalGreaterThan;
			case BinaryOperatorKind.Enum:
				return BinaryOperationKind.EnumGreaterThan;
			}
			break;
		}
		return BinaryOperationKind.Invalid;
	}
}
