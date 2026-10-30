using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class BuiltInOperators
{
	private ImmutableArray<BinaryOperatorSignature>[][] builtInOperators;

	private ImmutableArray<UnaryOperatorSignature>[] builtInUnaryOperators;

	private static ImmutableArray<BinaryOperatorSignature> GetSignaturesFromBinaryOperatorKinds(int[] operatorKinds)
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
		if (builtInOperators == null)
		{
			ImmutableArray<BinaryOperatorSignature>[] array = new ImmutableArray<BinaryOperatorSignature>[14]
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
				ImmutableArray.Create(GetSignature(BinaryOperatorKind.LogicalBoolAnd)),
				ImmutableArray.Create(GetSignature(BinaryOperatorKind.LogicalBoolXor)),
				ImmutableArray.Create(GetSignature(BinaryOperatorKind.LogicalBoolOr))
			};
			ImmutableArray<BinaryOperatorSignature>[] array2 = new ImmutableArray<BinaryOperatorSignature>[16]
			{
				GetSignaturesFromBinaryOperatorKinds(new int[3] { 4117, 4119, 4124 }),
				GetSignaturesFromBinaryOperatorKinds(new int[25]
				{
					4373, 4375, 4380, 4383, 4415, 4416, 4417, 4418, 4419, 4420,
					4421, 4422, 4423, 4424, 4425, 4426, 4427, 4428, 4408, 4409,
					4410, 4411, 4412, 4413, 4414
				}),
				GetSignaturesFromBinaryOperatorKinds(new int[27]
				{
					4629, 4631, 4636, 4660, 4671, 4672, 4673, 4674, 4675, 4676,
					4677, 4662, 4678, 4679, 4680, 4681, 4682, 4683, 4684, 4661,
					4664, 4665, 4666, 4667, 4668, 4669, 4670
				}),
				GetSignaturesFromBinaryOperatorKinds(new int[4] { 4885, 4887, 4892, 4919 }),
				GetSignaturesFromBinaryOperatorKinds(new int[4] { 5141, 5143, 5175, 5148 }),
				GetSignaturesFromBinaryOperatorKinds(new int[15]
				{
					5401, 5397, 5399, 5404, 5405, 5407, 5427, 5430, 5428, 5429,
					5415, 5416, 5417, 5453, 5454
				}),
				GetSignaturesFromBinaryOperatorKinds(new int[15]
				{
					5657, 5653, 5655, 5660, 5661, 5663, 5683, 5686, 5684, 5685,
					5671, 5672, 5673, 5709, 5710
				}),
				GetSignaturesFromBinaryOperatorKinds(new int[9] { 5913, 5909, 5911, 5916, 5919, 5940, 5942, 5941, 5965 }),
				GetSignaturesFromBinaryOperatorKinds(new int[9] { 6169, 6165, 6167, 6172, 6175, 6196, 6198, 6197, 6221 }),
				GetSignaturesFromBinaryOperatorKinds(new int[9] { 6425, 6421, 6423, 6428, 6431, 6452, 6454, 6453, 6477 }),
				GetSignaturesFromBinaryOperatorKinds(new int[9] { 6681, 6677, 6679, 6684, 6687, 6708, 6710, 6709, 6733 }),
				GetSignaturesFromBinaryOperatorKinds(new int[1] { 6941 }),
				GetSignaturesFromBinaryOperatorKinds(new int[1] { 7197 }),
				GetSignaturesFromBinaryOperatorKinds(new int[1] { 7453 }),
				GetRangeOperatorSignatures(),
				GetSignaturesFromBinaryOperatorKinds(new int[4] { 7957, 7959, 7964, 7991 })
			};
			ImmutableArray<BinaryOperatorSignature>[][] value = new ImmutableArray<BinaryOperatorSignature>[2][] { array2, array };
			Interlocked.CompareExchange(ref builtInOperators, value, null);
		}
		operators.AddRange(builtInOperators[kind.IsLogical() ? 1 : 0][kind.OperatorIndex()]);
	}

	internal static BinaryOperatorSignature GetSignature(BinaryOperatorKind kind)
	{
		TypeSymbol typeSymbol = LeftType(kind);
		switch (kind.Operator())
		{
		case BinaryOperatorKind.Multiplication:
		case BinaryOperatorKind.And:
		case BinaryOperatorKind.Xor:
		case BinaryOperatorKind.Or:
		case BinaryOperatorKind.Range:
			return new BinaryOperatorSignature(kind, typeSymbol, typeSymbol, typeSymbol);
		case BinaryOperatorKind.Addition:
		case BinaryOperatorKind.Subtraction:
		case BinaryOperatorKind.Remainder:
		case BinaryOperatorKind.IDivision:
			return new BinaryOperatorSignature(kind, LeftType(kind), RightType(kind), ReturnType(kind));
		case BinaryOperatorKind.Division:
			return new BinaryOperatorSignature(kind, typeSymbol, typeSymbol, Compilation.GetNavTypeSymbol(NavTypeKind.Decimal));
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
		case BinaryOperatorKind.GreaterThan:
		case BinaryOperatorKind.LessThan:
		case BinaryOperatorKind.GreaterThanOrEqual:
		case BinaryOperatorKind.LessThanOrEqual:
			return new BinaryOperatorSignature(kind, typeSymbol, typeSymbol, Compilation.GetNavTypeSymbol(NavTypeKind.Boolean));
		default:
			return new BinaryOperatorSignature(kind, LeftType(kind), RightType(kind), ReturnType(kind));
		}
	}

	private static TypeSymbol LeftType(BinaryOperatorKind kind)
	{
		switch (kind.OperandTypes())
		{
		case BinaryOperatorKind.Char:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Char);
		case BinaryOperatorKind.Int:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Integer);
		case BinaryOperatorKind.Long:
			return Compilation.GetNavTypeSymbol(NavTypeKind.BigInteger);
		case BinaryOperatorKind.Decimal:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Decimal);
		case BinaryOperatorKind.Duration:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Duration);
		case BinaryOperatorKind.Bool:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Boolean);
		case BinaryOperatorKind.String:
			return Compilation.GetNavTypeSymbol(NavTypeKind.String);
		case BinaryOperatorKind.Guid:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Guid);
		case BinaryOperatorKind.RecordId:
			return Compilation.GetNavTypeSymbol(NavTypeKind.RecordId);
		case BinaryOperatorKind.DateFormula:
			return Compilation.GetNavTypeSymbol(NavTypeKind.DateFormula);
		case BinaryOperatorKind.Joker:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Joker);
		case BinaryOperatorKind.Version:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Version);
		case BinaryOperatorKind.TableFilter:
			return Compilation.GetNavTypeSymbol(NavTypeKind.TableFilter);
		case BinaryOperatorKind.Date:
		case BinaryOperatorKind.DateAndByte:
		case BinaryOperatorKind.DateAndChar:
		case BinaryOperatorKind.DateAndOption:
		case BinaryOperatorKind.DateAndInteger:
		case BinaryOperatorKind.DateAndBigInteger:
		case BinaryOperatorKind.DateAndDecimal:
		case BinaryOperatorKind.DateAndDuration:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Date);
		case BinaryOperatorKind.Time:
		case BinaryOperatorKind.TimeAndByte:
		case BinaryOperatorKind.TimeAndChar:
		case BinaryOperatorKind.TimeAndOption:
		case BinaryOperatorKind.TimeAndInteger:
		case BinaryOperatorKind.TimeAndBigInteger:
		case BinaryOperatorKind.TimeAndDecimal:
		case BinaryOperatorKind.TimeAndDuration:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Time);
		case BinaryOperatorKind.DateTime:
		case BinaryOperatorKind.DateTimeAndByte:
		case BinaryOperatorKind.DateTimeAndChar:
		case BinaryOperatorKind.DateTimeAndOption:
		case BinaryOperatorKind.DateTimeAndInteger:
		case BinaryOperatorKind.DateTimeAndBigInteger:
		case BinaryOperatorKind.DateTimeAndDecimal:
		case BinaryOperatorKind.DateTimeAndDuration:
			return Compilation.GetNavTypeSymbol(NavTypeKind.DateTime);
		default:
			DebugAssertHelper.Assert(condition: false, "Bad operator kind in left type");
			return null;
		}
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1502")]
	private static TypeSymbol RightType(BinaryOperatorKind kind)
	{
		switch (kind.OperandTypes())
		{
		case BinaryOperatorKind.Char:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Char);
		case BinaryOperatorKind.Int:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Integer);
		case BinaryOperatorKind.Long:
			return Compilation.GetNavTypeSymbol(NavTypeKind.BigInteger);
		case BinaryOperatorKind.Decimal:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Decimal);
		case BinaryOperatorKind.Duration:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Duration);
		case BinaryOperatorKind.Bool:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Boolean);
		case BinaryOperatorKind.String:
			return Compilation.GetNavTypeSymbol(NavTypeKind.String);
		case BinaryOperatorKind.Guid:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Guid);
		case BinaryOperatorKind.RecordId:
			return Compilation.GetNavTypeSymbol(NavTypeKind.RecordId);
		case BinaryOperatorKind.DateFormula:
			return Compilation.GetNavTypeSymbol(NavTypeKind.DateFormula);
		case BinaryOperatorKind.Joker:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Joker);
		case BinaryOperatorKind.Version:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Version);
		case BinaryOperatorKind.TableFilter:
			return Compilation.GetNavTypeSymbol(NavTypeKind.TableFilter);
		case BinaryOperatorKind.Date:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Date);
		case BinaryOperatorKind.DateAndByte:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Byte);
		case BinaryOperatorKind.DateAndChar:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Char);
		case BinaryOperatorKind.DateAndOption:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Option);
		case BinaryOperatorKind.DateAndInteger:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Integer);
		case BinaryOperatorKind.DateAndBigInteger:
			return Compilation.GetNavTypeSymbol(NavTypeKind.BigInteger);
		case BinaryOperatorKind.DateAndDecimal:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Decimal);
		case BinaryOperatorKind.DateAndDuration:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Duration);
		case BinaryOperatorKind.Time:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Time);
		case BinaryOperatorKind.TimeAndByte:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Byte);
		case BinaryOperatorKind.TimeAndChar:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Char);
		case BinaryOperatorKind.TimeAndOption:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Option);
		case BinaryOperatorKind.TimeAndInteger:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Integer);
		case BinaryOperatorKind.TimeAndBigInteger:
			return Compilation.GetNavTypeSymbol(NavTypeKind.BigInteger);
		case BinaryOperatorKind.TimeAndDecimal:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Decimal);
		case BinaryOperatorKind.TimeAndDuration:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Duration);
		case BinaryOperatorKind.DateTime:
			return Compilation.GetNavTypeSymbol(NavTypeKind.DateTime);
		case BinaryOperatorKind.DateTimeAndByte:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Byte);
		case BinaryOperatorKind.DateTimeAndChar:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Char);
		case BinaryOperatorKind.DateTimeAndOption:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Option);
		case BinaryOperatorKind.DateTimeAndDecimal:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Decimal);
		case BinaryOperatorKind.DateTimeAndInteger:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Integer);
		case BinaryOperatorKind.DateTimeAndBigInteger:
			return Compilation.GetNavTypeSymbol(NavTypeKind.BigInteger);
		case BinaryOperatorKind.DateTimeAndDuration:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Duration);
		default:
			DebugAssertHelper.Assert(condition: false, "Bad operator kind in right type");
			return null;
		}
	}

	private static TypeSymbol ReturnType(BinaryOperatorKind kind)
	{
		switch (kind.OperandTypes())
		{
		case BinaryOperatorKind.Char:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Char);
		case BinaryOperatorKind.Int:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Integer);
		case BinaryOperatorKind.Long:
			return Compilation.GetNavTypeSymbol(NavTypeKind.BigInteger);
		case BinaryOperatorKind.Decimal:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Decimal);
		case BinaryOperatorKind.Duration:
			return Compilation.GetNavTypeSymbol(NavTypeKind.BigInteger);
		case BinaryOperatorKind.String:
			return Compilation.GetNavTypeSymbol(NavTypeKind.String);
		case BinaryOperatorKind.Date:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Integer);
		case BinaryOperatorKind.DateAndByte:
		case BinaryOperatorKind.DateAndChar:
		case BinaryOperatorKind.DateAndOption:
		case BinaryOperatorKind.DateAndInteger:
		case BinaryOperatorKind.DateAndBigInteger:
		case BinaryOperatorKind.DateAndDecimal:
		case BinaryOperatorKind.DateAndDuration:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Date);
		case BinaryOperatorKind.Time:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Integer);
		case BinaryOperatorKind.TimeAndByte:
		case BinaryOperatorKind.TimeAndChar:
		case BinaryOperatorKind.TimeAndOption:
		case BinaryOperatorKind.TimeAndInteger:
		case BinaryOperatorKind.TimeAndBigInteger:
		case BinaryOperatorKind.TimeAndDecimal:
		case BinaryOperatorKind.TimeAndDuration:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Time);
		case BinaryOperatorKind.DateTime:
			return Compilation.GetNavTypeSymbol(NavTypeKind.Duration);
		case BinaryOperatorKind.DateTimeAndByte:
		case BinaryOperatorKind.DateTimeAndChar:
		case BinaryOperatorKind.DateTimeAndOption:
		case BinaryOperatorKind.DateTimeAndInteger:
		case BinaryOperatorKind.DateTimeAndBigInteger:
		case BinaryOperatorKind.DateTimeAndDecimal:
		case BinaryOperatorKind.DateTimeAndDuration:
			return Compilation.GetNavTypeSymbol(NavTypeKind.DateTime);
		default:
			DebugAssertHelper.Assert(condition: false, "Bad operator kind in return type");
			return null;
		}
	}

	private static ImmutableArray<BinaryOperatorSignature> GetRangeOperatorSignatures()
	{
		ArrayBuilder<BinaryOperatorSignature> instance = ArrayBuilder<BinaryOperatorSignature>.GetInstance();
		int[] array = new int[5] { 7709, 7708, 7733, 7732, 7734 };
		foreach (int kind in array)
		{
			instance.Add(GetSignature((BinaryOperatorKind)kind));
		}
		BinaryOperatorSignature signature = GetSignature(BinaryOperatorKind.CharRange);
		signature.Priority = 0;
		instance.Add(signature);
		BinaryOperatorSignature signature2 = GetSignature(BinaryOperatorKind.StringRange);
		signature2.Priority = 1;
		instance.Add(signature2);
		return instance.ToImmutableAndFree();
	}

	private static ImmutableArray<UnaryOperatorSignature> GetSignaturesFromUnaryOperatorKinds(int[] operatorKinds)
	{
		ArrayBuilder<UnaryOperatorSignature> instance = ArrayBuilder<UnaryOperatorSignature>.GetInstance();
		foreach (int kind in operatorKinds)
		{
			instance.Add(GetSignature((UnaryOperatorKind)kind));
		}
		return instance.ToImmutableAndFree();
	}

	internal static UnaryOperatorSignature GetSignature(UnaryOperatorKind kind)
	{
		TypeSymbol typeSymbol = null;
		switch (kind.OperandTypes())
		{
		case UnaryOperatorKind.Int:
			typeSymbol = Compilation.GetNavTypeSymbol(NavTypeKind.Integer);
			break;
		case UnaryOperatorKind.Long:
			typeSymbol = Compilation.GetNavTypeSymbol(NavTypeKind.BigInteger);
			break;
		case UnaryOperatorKind.Decimal:
			typeSymbol = Compilation.GetNavTypeSymbol(NavTypeKind.Decimal);
			break;
		case UnaryOperatorKind.Bool:
			typeSymbol = Compilation.GetNavTypeSymbol(NavTypeKind.Boolean);
			break;
		}
		DebugAssertHelper.Assert((object)typeSymbol != null);
		return new UnaryOperatorSignature(kind, typeSymbol, typeSymbol);
	}

	internal void GetSimpleBuiltInOperators(UnaryOperatorKind kind, ArrayBuilder<UnaryOperatorSignature> operators)
	{
		if (builtInUnaryOperators == null)
		{
			ImmutableArray<UnaryOperatorSignature>[] value = new ImmutableArray<UnaryOperatorSignature>[10]
			{
				ImmutableArray<UnaryOperatorSignature>.Empty,
				ImmutableArray<UnaryOperatorSignature>.Empty,
				ImmutableArray<UnaryOperatorSignature>.Empty,
				ImmutableArray<UnaryOperatorSignature>.Empty,
				GetSignaturesFromUnaryOperatorKinds(new int[3] { 5141, 5143, 5148 }),
				GetSignaturesFromUnaryOperatorKinds(new int[3] { 5397, 5399, 5404 }),
				GetSignaturesFromUnaryOperatorKinds(new int[1] { 5661 }),
				ImmutableArray<UnaryOperatorSignature>.Empty,
				ImmutableArray<UnaryOperatorSignature>.Empty,
				ImmutableArray<UnaryOperatorSignature>.Empty
			};
			Interlocked.CompareExchange(ref builtInUnaryOperators, value, null);
		}
		operators.AddRange(builtInUnaryOperators[kind.OperatorIndex()]);
	}
}
