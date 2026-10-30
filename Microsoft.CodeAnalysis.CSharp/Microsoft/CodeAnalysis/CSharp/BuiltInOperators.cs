using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal class BuiltInOperators
{
	private readonly CSharpCompilation _compilation;

	private ImmutableArray<UnaryOperatorSignature>[] _builtInUnaryOperators;

	private ImmutableArray<BinaryOperatorSignature>[][] _builtInOperators;

	internal BuiltInOperators(CSharpCompilation compilation)
	{
		_compilation = compilation;
	}

	private ImmutableArray<UnaryOperatorSignature> GetSignaturesFromUnaryOperatorKinds(int[] operatorKinds)
	{
		ArrayBuilder<UnaryOperatorSignature> instance = ArrayBuilder<UnaryOperatorSignature>.GetInstance();
		foreach (int kind in operatorKinds)
		{
			instance.Add(GetSignature((UnaryOperatorKind)kind));
		}
		return instance.ToImmutableAndFree();
	}

	internal void GetSimpleBuiltInOperators(UnaryOperatorKind kind, ArrayBuilder<UnaryOperatorSignature> operators)
	{
		if (_builtInUnaryOperators == null)
		{
			ImmutableArray<UnaryOperatorSignature>[] value = new ImmutableArray<UnaryOperatorSignature>[10]
			{
				GetSignaturesFromUnaryOperatorKinds(new int[24]
				{
					4113, 4114, 4115, 4116, 4117, 4118, 4119, 4120, 4121, 4122,
					4123, 4124, 69649, 69650, 69651, 69652, 69653, 69654, 69655, 69656,
					69657, 69658, 69659, 69660
				}),
				GetSignaturesFromUnaryOperatorKinds(new int[24]
				{
					4369, 4370, 4371, 4372, 4373, 4374, 4375, 4376, 4377, 4378,
					4379, 4380, 69905, 69906, 69907, 69908, 69909, 69910, 69911, 69912,
					69913, 69914, 69915, 69916
				}),
				GetSignaturesFromUnaryOperatorKinds(new int[24]
				{
					4625, 4626, 4627, 4628, 4629, 4630, 4631, 4632, 4633, 4634,
					4635, 4636, 70161, 70162, 70163, 70164, 70165, 70166, 70167, 70168,
					70169, 70170, 70171, 70172
				}),
				GetSignaturesFromUnaryOperatorKinds(new int[24]
				{
					4881, 4882, 4883, 4884, 4885, 4886, 4887, 4888, 4889, 4890,
					4891, 4892, 70417, 70418, 70419, 70420, 70421, 70422, 70423, 70424,
					70425, 70426, 70427, 70428
				}),
				GetSignaturesFromUnaryOperatorKinds(new int[14]
				{
					5141, 5142, 5143, 5144, 5146, 5147, 5148, 70677, 70678, 70679,
					70680, 70682, 70683, 70684
				}),
				GetSignaturesFromUnaryOperatorKinds(new int[10] { 5397, 5399, 5402, 5403, 5404, 70933, 70935, 70938, 70939, 70940 }),
				GetSignaturesFromUnaryOperatorKinds(new int[2] { 5661, 71197 }),
				GetSignaturesFromUnaryOperatorKinds(new int[8] { 5909, 5910, 5911, 5912, 71445, 71446, 71447, 71448 }),
				ImmutableArray<UnaryOperatorSignature>.Empty,
				ImmutableArray<UnaryOperatorSignature>.Empty
			};
			Interlocked.CompareExchange(ref _builtInUnaryOperators, value, null);
		}
		operators.AddRange(_builtInUnaryOperators[kind.OperatorIndex()]);
	}

	internal UnaryOperatorSignature GetSignature(UnaryOperatorKind kind)
	{
		TypeSymbol typeSymbol = null;
		if (kind.IsLifted())
		{
			NamedTypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Nullable_T);
			switch (kind.OperandTypes())
			{
			case UnaryOperatorKind.SByte:
				typeSymbol = specialType.Construct(_compilation.GetSpecialType(SpecialType.System_SByte));
				break;
			case UnaryOperatorKind.Byte:
				typeSymbol = specialType.Construct(_compilation.GetSpecialType(SpecialType.System_Byte));
				break;
			case UnaryOperatorKind.Short:
				typeSymbol = specialType.Construct(_compilation.GetSpecialType(SpecialType.System_Int16));
				break;
			case UnaryOperatorKind.UShort:
				typeSymbol = specialType.Construct(_compilation.GetSpecialType(SpecialType.System_UInt16));
				break;
			case UnaryOperatorKind.Int:
				typeSymbol = specialType.Construct(_compilation.GetSpecialType(SpecialType.System_Int32));
				break;
			case UnaryOperatorKind.UInt:
				typeSymbol = specialType.Construct(_compilation.GetSpecialType(SpecialType.System_UInt32));
				break;
			case UnaryOperatorKind.Long:
				typeSymbol = specialType.Construct(_compilation.GetSpecialType(SpecialType.System_Int64));
				break;
			case UnaryOperatorKind.ULong:
				typeSymbol = specialType.Construct(_compilation.GetSpecialType(SpecialType.System_UInt64));
				break;
			case UnaryOperatorKind.Char:
				typeSymbol = specialType.Construct(_compilation.GetSpecialType(SpecialType.System_Char));
				break;
			case UnaryOperatorKind.Float:
				typeSymbol = specialType.Construct(_compilation.GetSpecialType(SpecialType.System_Single));
				break;
			case UnaryOperatorKind.Double:
				typeSymbol = specialType.Construct(_compilation.GetSpecialType(SpecialType.System_Double));
				break;
			case UnaryOperatorKind.Decimal:
				typeSymbol = specialType.Construct(_compilation.GetSpecialType(SpecialType.System_Decimal));
				break;
			case UnaryOperatorKind.Bool:
				typeSymbol = specialType.Construct(_compilation.GetSpecialType(SpecialType.System_Boolean));
				break;
			}
		}
		else
		{
			switch (kind.OperandTypes())
			{
			case UnaryOperatorKind.SByte:
				typeSymbol = _compilation.GetSpecialType(SpecialType.System_SByte);
				break;
			case UnaryOperatorKind.Byte:
				typeSymbol = _compilation.GetSpecialType(SpecialType.System_Byte);
				break;
			case UnaryOperatorKind.Short:
				typeSymbol = _compilation.GetSpecialType(SpecialType.System_Int16);
				break;
			case UnaryOperatorKind.UShort:
				typeSymbol = _compilation.GetSpecialType(SpecialType.System_UInt16);
				break;
			case UnaryOperatorKind.Int:
				typeSymbol = _compilation.GetSpecialType(SpecialType.System_Int32);
				break;
			case UnaryOperatorKind.UInt:
				typeSymbol = _compilation.GetSpecialType(SpecialType.System_UInt32);
				break;
			case UnaryOperatorKind.Long:
				typeSymbol = _compilation.GetSpecialType(SpecialType.System_Int64);
				break;
			case UnaryOperatorKind.ULong:
				typeSymbol = _compilation.GetSpecialType(SpecialType.System_UInt64);
				break;
			case UnaryOperatorKind.Char:
				typeSymbol = _compilation.GetSpecialType(SpecialType.System_Char);
				break;
			case UnaryOperatorKind.Float:
				typeSymbol = _compilation.GetSpecialType(SpecialType.System_Single);
				break;
			case UnaryOperatorKind.Double:
				typeSymbol = _compilation.GetSpecialType(SpecialType.System_Double);
				break;
			case UnaryOperatorKind.Decimal:
				typeSymbol = _compilation.GetSpecialType(SpecialType.System_Decimal);
				break;
			case UnaryOperatorKind.Bool:
				typeSymbol = _compilation.GetSpecialType(SpecialType.System_Boolean);
				break;
			}
		}
		return new UnaryOperatorSignature(kind, typeSymbol, typeSymbol);
	}

	private ImmutableArray<BinaryOperatorSignature> GetSignaturesFromBinaryOperatorKinds(int[] operatorKinds)
	{
		ArrayBuilder<BinaryOperatorSignature> instance = ArrayBuilder<BinaryOperatorSignature>.GetInstance();
		foreach (int kind in operatorKinds)
		{
			instance.Add(GetSignature((BinaryOperatorKind)kind));
		}
		return instance.ToImmutableAndFree();
	}

	internal void GetSimpleBuiltInOperators(BinaryOperatorKind kind, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		if (_builtInOperators == null)
		{
			ImmutableArray<BinaryOperatorSignature>[] array = new ImmutableArray<BinaryOperatorSignature>[16]
			{
				ImmutableArray<BinaryOperatorSignature>.Empty,
				ImmutableArray<BinaryOperatorSignature>.Empty,
				ImmutableArray<BinaryOperatorSignature>.Empty,
				ImmutableArray<BinaryOperatorSignature>.Empty,
				ImmutableArray<BinaryOperatorSignature>.Empty,
				ImmutableArray<BinaryOperatorSignature>.Empty,
				ImmutableArray<BinaryOperatorSignature>.Empty,
				ImmutableArray<BinaryOperatorSignature>.Empty,
				ImmutableArray<BinaryOperatorSignature>.Empty,
				ImmutableArray<BinaryOperatorSignature>.Empty,
				ImmutableArray<BinaryOperatorSignature>.Empty,
				ImmutableArray<BinaryOperatorSignature>.Empty,
				ImmutableArray<BinaryOperatorSignature>.Empty,
				ImmutableArray.Create(GetSignature(BinaryOperatorKind.LogicalBoolAnd)),
				ImmutableArray<BinaryOperatorSignature>.Empty,
				ImmutableArray.Create(GetSignature(BinaryOperatorKind.LogicalBoolOr))
			};
			ImmutableArray<BinaryOperatorSignature>[] array2 = new ImmutableArray<BinaryOperatorSignature>[16]
			{
				GetSignaturesFromBinaryOperatorKinds(new int[14]
				{
					4117, 4118, 4119, 4120, 4122, 4123, 4124, 69653, 69654, 69655,
					69656, 69658, 69659, 69660
				}),
				GetSignaturesFromBinaryOperatorKinds(new int[17]
				{
					4373, 4374, 4375, 4376, 4378, 4379, 4380, 69909, 69910, 69911,
					69912, 69914, 69915, 69916, 4383, 4385, 4386
				}),
				GetSignaturesFromBinaryOperatorKinds(new int[14]
				{
					4629, 4630, 4631, 4632, 4634, 4635, 4636, 70165, 70166, 70167,
					70168, 70170, 70171, 70172
				}),
				GetSignaturesFromBinaryOperatorKinds(new int[14]
				{
					4885, 4886, 4887, 4888, 4890, 4891, 4892, 70421, 70422, 70423,
					70424, 70426, 70427, 70428
				}),
				GetSignaturesFromBinaryOperatorKinds(new int[14]
				{
					5141, 5142, 5143, 5144, 5146, 5147, 5148, 70677, 70678, 70679,
					70680, 70682, 70683, 70684
				}),
				GetSignaturesFromBinaryOperatorKinds(new int[8] { 5397, 5398, 5399, 5400, 70933, 70934, 70935, 70936 }),
				GetSignaturesFromBinaryOperatorKinds(new int[8] { 5653, 5654, 5655, 5656, 71189, 71190, 71191, 71192 }),
				GetSignaturesFromBinaryOperatorKinds(new int[18]
				{
					5909, 5910, 5911, 5912, 5914, 5915, 5916, 5917, 71445, 71446,
					71447, 71448, 71450, 71451, 71452, 71453, 5918, 5919
				}),
				GetSignaturesFromBinaryOperatorKinds(new int[18]
				{
					6165, 6166, 6167, 6168, 6170, 6171, 6172, 6173, 71701, 71702,
					71703, 71704, 71706, 71707, 71708, 71709, 6174, 6175
				}),
				GetSignaturesFromBinaryOperatorKinds(new int[14]
				{
					6421, 6422, 6423, 6424, 6426, 6427, 6428, 71957, 71958, 71959,
					71960, 71962, 71963, 71964
				}),
				GetSignaturesFromBinaryOperatorKinds(new int[14]
				{
					6677, 6678, 6679, 6680, 6682, 6683, 6684, 72213, 72214, 72215,
					72216, 72218, 72219, 72220
				}),
				GetSignaturesFromBinaryOperatorKinds(new int[14]
				{
					6933, 6934, 6935, 6936, 6938, 6939, 6940, 72469, 72470, 72471,
					72472, 72474, 72475, 72476
				}),
				GetSignaturesFromBinaryOperatorKinds(new int[14]
				{
					7189, 7190, 7191, 7192, 7194, 7195, 7196, 72725, 72726, 72727,
					72728, 72730, 72731, 72732
				}),
				GetSignaturesFromBinaryOperatorKinds(new int[10] { 7445, 7446, 7447, 7448, 7453, 72981, 72982, 72983, 72984, 72989 }),
				GetSignaturesFromBinaryOperatorKinds(new int[10] { 7701, 7702, 7703, 7704, 7709, 73237, 73238, 73239, 73240, 73245 }),
				GetSignaturesFromBinaryOperatorKinds(new int[10] { 7957, 7958, 7959, 7960, 7965, 73493, 73494, 73495, 73496, 73501 })
			};
			ImmutableArray<BinaryOperatorSignature>[][] value = new ImmutableArray<BinaryOperatorSignature>[2][] { array2, array };
			Interlocked.CompareExchange(ref _builtInOperators, value, null);
		}
		operators.AddRange(_builtInOperators[kind.IsLogical() ? 1 : 0][kind.OperatorIndex()]);
	}

	internal BinaryOperatorSignature GetSignature(BinaryOperatorKind kind)
	{
		TypeSymbol typeSymbol = LeftType(kind);
		switch (kind.Operator())
		{
		case BinaryOperatorKind.Multiplication:
		case BinaryOperatorKind.Subtraction:
		case BinaryOperatorKind.Division:
		case BinaryOperatorKind.Remainder:
		case BinaryOperatorKind.And:
		case BinaryOperatorKind.Xor:
		case BinaryOperatorKind.Or:
			return new BinaryOperatorSignature(kind, typeSymbol, typeSymbol, typeSymbol);
		case BinaryOperatorKind.Addition:
			return new BinaryOperatorSignature(kind, LeftType(kind), RightType(kind), ReturnType(kind));
		case BinaryOperatorKind.LeftShift:
		case BinaryOperatorKind.RightShift:
		{
			TypeSymbol typeSymbol2 = _compilation.GetSpecialType(SpecialType.System_Int32);
			if (kind.IsLifted())
			{
				typeSymbol2 = _compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(typeSymbol2);
			}
			return new BinaryOperatorSignature(kind, typeSymbol, typeSymbol2, typeSymbol);
		}
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
		case BinaryOperatorKind.GreaterThan:
		case BinaryOperatorKind.LessThan:
		case BinaryOperatorKind.GreaterThanOrEqual:
		case BinaryOperatorKind.LessThanOrEqual:
			return new BinaryOperatorSignature(kind, typeSymbol, typeSymbol, _compilation.GetSpecialType(SpecialType.System_Boolean));
		default:
			return new BinaryOperatorSignature(kind, LeftType(kind), RightType(kind), ReturnType(kind));
		}
	}

	private TypeSymbol LeftType(BinaryOperatorKind kind)
	{
		if (kind.IsLifted())
		{
			return LiftedType(kind);
		}
		switch (kind.OperandTypes())
		{
		case BinaryOperatorKind.Int:
			return _compilation.GetSpecialType(SpecialType.System_Int32);
		case BinaryOperatorKind.UInt:
			return _compilation.GetSpecialType(SpecialType.System_UInt32);
		case BinaryOperatorKind.Long:
			return _compilation.GetSpecialType(SpecialType.System_Int64);
		case BinaryOperatorKind.ULong:
			return _compilation.GetSpecialType(SpecialType.System_UInt64);
		case BinaryOperatorKind.Float:
			return _compilation.GetSpecialType(SpecialType.System_Single);
		case BinaryOperatorKind.Double:
			return _compilation.GetSpecialType(SpecialType.System_Double);
		case BinaryOperatorKind.Decimal:
			return _compilation.GetSpecialType(SpecialType.System_Decimal);
		case BinaryOperatorKind.Bool:
			return _compilation.GetSpecialType(SpecialType.System_Boolean);
		case BinaryOperatorKind.Object:
		case BinaryOperatorKind.ObjectAndString:
			return _compilation.GetSpecialType(SpecialType.System_Object);
		case BinaryOperatorKind.String:
		case BinaryOperatorKind.StringAndObject:
			return _compilation.GetSpecialType(SpecialType.System_String);
		default:
			return null;
		}
	}

	private TypeSymbol RightType(BinaryOperatorKind kind)
	{
		if (kind.IsLifted())
		{
			return LiftedType(kind);
		}
		switch (kind.OperandTypes())
		{
		case BinaryOperatorKind.Int:
			return _compilation.GetSpecialType(SpecialType.System_Int32);
		case BinaryOperatorKind.UInt:
			return _compilation.GetSpecialType(SpecialType.System_UInt32);
		case BinaryOperatorKind.Long:
			return _compilation.GetSpecialType(SpecialType.System_Int64);
		case BinaryOperatorKind.ULong:
			return _compilation.GetSpecialType(SpecialType.System_UInt64);
		case BinaryOperatorKind.Float:
			return _compilation.GetSpecialType(SpecialType.System_Single);
		case BinaryOperatorKind.Double:
			return _compilation.GetSpecialType(SpecialType.System_Double);
		case BinaryOperatorKind.Decimal:
			return _compilation.GetSpecialType(SpecialType.System_Decimal);
		case BinaryOperatorKind.Bool:
			return _compilation.GetSpecialType(SpecialType.System_Boolean);
		case BinaryOperatorKind.String:
		case BinaryOperatorKind.ObjectAndString:
			return _compilation.GetSpecialType(SpecialType.System_String);
		case BinaryOperatorKind.Object:
		case BinaryOperatorKind.StringAndObject:
			return _compilation.GetSpecialType(SpecialType.System_Object);
		default:
			return null;
		}
	}

	private TypeSymbol ReturnType(BinaryOperatorKind kind)
	{
		if (kind.IsLifted())
		{
			return LiftedType(kind);
		}
		switch (kind.OperandTypes())
		{
		case BinaryOperatorKind.Int:
			return _compilation.GetSpecialType(SpecialType.System_Int32);
		case BinaryOperatorKind.UInt:
			return _compilation.GetSpecialType(SpecialType.System_UInt32);
		case BinaryOperatorKind.Long:
			return _compilation.GetSpecialType(SpecialType.System_Int64);
		case BinaryOperatorKind.ULong:
			return _compilation.GetSpecialType(SpecialType.System_UInt64);
		case BinaryOperatorKind.Float:
			return _compilation.GetSpecialType(SpecialType.System_Single);
		case BinaryOperatorKind.Double:
			return _compilation.GetSpecialType(SpecialType.System_Double);
		case BinaryOperatorKind.Decimal:
			return _compilation.GetSpecialType(SpecialType.System_Decimal);
		case BinaryOperatorKind.Bool:
			return _compilation.GetSpecialType(SpecialType.System_Boolean);
		case BinaryOperatorKind.Object:
			return _compilation.GetSpecialType(SpecialType.System_Object);
		case BinaryOperatorKind.String:
		case BinaryOperatorKind.StringAndObject:
		case BinaryOperatorKind.ObjectAndString:
			return _compilation.GetSpecialType(SpecialType.System_String);
		default:
			return null;
		}
	}

	private TypeSymbol LiftedType(BinaryOperatorKind kind)
	{
		NamedTypeSymbol specialType = _compilation.GetSpecialType(SpecialType.System_Nullable_T);
		return kind.OperandTypes() switch
		{
			BinaryOperatorKind.Int => (TypeSymbol)specialType.Construct(_compilation.GetSpecialType(SpecialType.System_Int32)), 
			BinaryOperatorKind.UInt => specialType.Construct(_compilation.GetSpecialType(SpecialType.System_UInt32)), 
			BinaryOperatorKind.Long => specialType.Construct(_compilation.GetSpecialType(SpecialType.System_Int64)), 
			BinaryOperatorKind.ULong => specialType.Construct(_compilation.GetSpecialType(SpecialType.System_UInt64)), 
			BinaryOperatorKind.Float => specialType.Construct(_compilation.GetSpecialType(SpecialType.System_Single)), 
			BinaryOperatorKind.Double => specialType.Construct(_compilation.GetSpecialType(SpecialType.System_Double)), 
			BinaryOperatorKind.Decimal => specialType.Construct(_compilation.GetSpecialType(SpecialType.System_Decimal)), 
			BinaryOperatorKind.Bool => specialType.Construct(_compilation.GetSpecialType(SpecialType.System_Boolean)), 
			_ => null, 
		};
	}

	internal static bool IsValidObjectEquality(Conversions Conversions, TypeSymbol leftType, bool leftIsNullOrDefault, TypeSymbol rightType, bool rightIsNullOrDefault, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((object)leftType != null && leftType.IsTypeParameter())
		{
			if (leftType.IsValueType || (!leftType.IsReferenceType && !rightIsNullOrDefault))
			{
				return false;
			}
			leftType = ((TypeParameterSymbol)leftType).EffectiveBaseClass(ref useSiteDiagnostics);
		}
		if ((object)rightType != null && rightType.IsTypeParameter())
		{
			if (rightType.IsValueType || (!rightType.IsReferenceType && !leftIsNullOrDefault))
			{
				return false;
			}
			rightType = ((TypeParameterSymbol)rightType).EffectiveBaseClass(ref useSiteDiagnostics);
		}
		if (((object)leftType == null || !leftType.IsReferenceType) && !leftIsNullOrDefault)
		{
			return false;
		}
		if (((object)rightType == null || !rightType.IsReferenceType) && !rightIsNullOrDefault)
		{
			return false;
		}
		if (leftIsNullOrDefault | rightIsNullOrDefault)
		{
			return true;
		}
		Conversion conversion = Conversions.ClassifyConversionFromType(leftType, rightType, ref useSiteDiagnostics);
		if (conversion.IsIdentity || conversion.IsReference)
		{
			return true;
		}
		Conversion conversion2 = Conversions.ClassifyConversionFromType(rightType, leftType, ref useSiteDiagnostics);
		if (conversion2.IsIdentity || conversion2.IsReference)
		{
			return true;
		}
		return false;
	}
}
