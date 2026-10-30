using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class OverloadResolution
{
	internal static class BinopEasyOut
	{
		private const BinaryOperatorKind ERR = BinaryOperatorKind.Error;

		private const BinaryOperatorKind OBJ = BinaryOperatorKind.Object;

		private const BinaryOperatorKind STR = BinaryOperatorKind.String;

		private const BinaryOperatorKind OSC = BinaryOperatorKind.ObjectAndString;

		private const BinaryOperatorKind SOC = BinaryOperatorKind.StringAndObject;

		private const BinaryOperatorKind INT = BinaryOperatorKind.Int;

		private const BinaryOperatorKind UIN = BinaryOperatorKind.UInt;

		private const BinaryOperatorKind LNG = BinaryOperatorKind.Long;

		private const BinaryOperatorKind ULG = BinaryOperatorKind.ULong;

		private const BinaryOperatorKind FLT = BinaryOperatorKind.Float;

		private const BinaryOperatorKind DBL = BinaryOperatorKind.Double;

		private const BinaryOperatorKind DEC = BinaryOperatorKind.Decimal;

		private const BinaryOperatorKind BOL = BinaryOperatorKind.Bool;

		private const BinaryOperatorKind LIN = BinaryOperatorKind.Int | BinaryOperatorKind.Lifted;

		private const BinaryOperatorKind LUN = BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted;

		private const BinaryOperatorKind LLG = BinaryOperatorKind.Long | BinaryOperatorKind.Lifted;

		private const BinaryOperatorKind LUL = BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted;

		private const BinaryOperatorKind LFL = BinaryOperatorKind.Float | BinaryOperatorKind.Lifted;

		private const BinaryOperatorKind LDB = BinaryOperatorKind.Double | BinaryOperatorKind.Lifted;

		private const BinaryOperatorKind LDC = BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted;

		private const BinaryOperatorKind LBL = BinaryOperatorKind.Bool | BinaryOperatorKind.Lifted;

		private static readonly BinaryOperatorKind[,] s_arithmetic = new BinaryOperatorKind[28, 28]
		{
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			}
		};

		private static readonly BinaryOperatorKind[,] s_addition = new BinaryOperatorKind[28, 28]
		{
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.String,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject,
				BinaryOperatorKind.StringAndObject
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ObjectAndString,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			}
		};

		private static readonly BinaryOperatorKind[,] s_shift = new BinaryOperatorKind[28, 28]
		{
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			}
		};

		private static readonly BinaryOperatorKind[,] s_equality = new BinaryOperatorKind[28, 28]
		{
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.String,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Bool,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Bool | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Float,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Double,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Decimal,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Bool | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Bool | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Float | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Double | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object
			},
			{
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Object,
				BinaryOperatorKind.Decimal | BinaryOperatorKind.Lifted
			}
		};

		private static readonly BinaryOperatorKind[,] s_logical = new BinaryOperatorKind[28, 28]
		{
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Bool,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Bool | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.UInt,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.ULong,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Bool | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Bool | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Int | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Long | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.UInt | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.ULong | BinaryOperatorKind.Lifted,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error
			}
		};

		private static readonly BinaryOperatorKind[][,] s_opkind = new BinaryOperatorKind[16][,]
		{
			s_arithmetic, s_addition, s_arithmetic, s_arithmetic, s_arithmetic, s_shift, s_shift, s_equality, s_equality, s_arithmetic,
			s_arithmetic, s_arithmetic, s_arithmetic, s_logical, s_logical, s_logical
		};

		private static int? TypeToIndex(TypeSymbol type)
		{
			switch (type.GetSpecialTypeSafe())
			{
			case SpecialType.System_Object:
				return 0;
			case SpecialType.System_String:
				return 1;
			case SpecialType.System_Boolean:
				return 2;
			case SpecialType.System_Char:
				return 3;
			case SpecialType.System_SByte:
				return 4;
			case SpecialType.System_Int16:
				return 5;
			case SpecialType.System_Int32:
				return 6;
			case SpecialType.System_Int64:
				return 7;
			case SpecialType.System_Byte:
				return 8;
			case SpecialType.System_UInt16:
				return 9;
			case SpecialType.System_UInt32:
				return 10;
			case SpecialType.System_UInt64:
				return 11;
			case SpecialType.System_Single:
				return 12;
			case SpecialType.System_Double:
				return 13;
			case SpecialType.System_Decimal:
				return 14;
			case SpecialType.None:
				if ((object)type != null && type.IsNullableType())
				{
					switch (type.GetNullableUnderlyingType().GetSpecialTypeSafe())
					{
					case SpecialType.System_Boolean:
						return 15;
					case SpecialType.System_Char:
						return 16;
					case SpecialType.System_SByte:
						return 17;
					case SpecialType.System_Int16:
						return 18;
					case SpecialType.System_Int32:
						return 19;
					case SpecialType.System_Int64:
						return 20;
					case SpecialType.System_Byte:
						return 21;
					case SpecialType.System_UInt16:
						return 22;
					case SpecialType.System_UInt32:
						return 23;
					case SpecialType.System_UInt64:
						return 24;
					case SpecialType.System_Single:
						return 25;
					case SpecialType.System_Double:
						return 26;
					case SpecialType.System_Decimal:
						return 27;
					}
				}
				break;
			}
			return null;
		}

		public static BinaryOperatorKind OpKind(BinaryOperatorKind kind, TypeSymbol left, TypeSymbol right)
		{
			int? num = TypeToIndex(left);
			if (!num.HasValue)
			{
				return BinaryOperatorKind.Error;
			}
			int? num2 = TypeToIndex(right);
			if (!num2.HasValue)
			{
				return BinaryOperatorKind.Error;
			}
			BinaryOperatorKind binaryOperatorKind = BinaryOperatorKind.Error;
			if (!kind.IsLogical() || (num == 29 && num2 == 29))
			{
				binaryOperatorKind = s_opkind[kind.OperatorIndex()][num.Value, num2.Value];
			}
			if (binaryOperatorKind != BinaryOperatorKind.Error)
			{
				return binaryOperatorKind | kind;
			}
			return binaryOperatorKind;
		}
	}

	private enum LiftingResult
	{
		NotLifted,
		LiftOperandsAndResult,
		LiftOperandsButNotResult
	}

	internal static class UnopEasyOut
	{
		private const UnaryOperatorKind ERR = UnaryOperatorKind.Error;

		private const UnaryOperatorKind BOL = UnaryOperatorKind.Bool;

		private const UnaryOperatorKind CHR = UnaryOperatorKind.Char;

		private const UnaryOperatorKind I08 = UnaryOperatorKind.SByte;

		private const UnaryOperatorKind U08 = UnaryOperatorKind.Byte;

		private const UnaryOperatorKind I16 = UnaryOperatorKind.Short;

		private const UnaryOperatorKind U16 = UnaryOperatorKind.UShort;

		private const UnaryOperatorKind I32 = UnaryOperatorKind.Int;

		private const UnaryOperatorKind U32 = UnaryOperatorKind.UInt;

		private const UnaryOperatorKind I64 = UnaryOperatorKind.Long;

		private const UnaryOperatorKind U64 = UnaryOperatorKind.ULong;

		private const UnaryOperatorKind R32 = UnaryOperatorKind.Float;

		private const UnaryOperatorKind R64 = UnaryOperatorKind.Double;

		private const UnaryOperatorKind DEC = UnaryOperatorKind.Decimal;

		private const UnaryOperatorKind LBOL = UnaryOperatorKind.Bool | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LCHR = UnaryOperatorKind.Char | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LI08 = UnaryOperatorKind.SByte | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LU08 = UnaryOperatorKind.Byte | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LI16 = UnaryOperatorKind.Short | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LU16 = UnaryOperatorKind.UShort | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LI32 = UnaryOperatorKind.Int | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LU32 = UnaryOperatorKind.UInt | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LI64 = UnaryOperatorKind.Long | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LU64 = UnaryOperatorKind.ULong | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LR32 = UnaryOperatorKind.Float | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LR64 = UnaryOperatorKind.Double | UnaryOperatorKind.Lifted;

		private const UnaryOperatorKind LDEC = UnaryOperatorKind.Decimal | UnaryOperatorKind.Lifted;

		private static readonly UnaryOperatorKind[] s_increment = new UnaryOperatorKind[28]
		{
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Char,
			UnaryOperatorKind.SByte,
			UnaryOperatorKind.Short,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Long,
			UnaryOperatorKind.Byte,
			UnaryOperatorKind.UShort,
			UnaryOperatorKind.UInt,
			UnaryOperatorKind.ULong,
			UnaryOperatorKind.Float,
			UnaryOperatorKind.Double,
			UnaryOperatorKind.Decimal,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Char | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.SByte | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Short | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Long | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Byte | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.UShort | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.UInt | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.ULong | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Float | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Double | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Decimal | UnaryOperatorKind.Lifted
		};

		private static readonly UnaryOperatorKind[] s_plus = new UnaryOperatorKind[28]
		{
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Long,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.UInt,
			UnaryOperatorKind.ULong,
			UnaryOperatorKind.Float,
			UnaryOperatorKind.Double,
			UnaryOperatorKind.Decimal,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Long | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.UInt | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.ULong | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Float | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Double | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Decimal | UnaryOperatorKind.Lifted
		};

		private static readonly UnaryOperatorKind[] s_minus = new UnaryOperatorKind[28]
		{
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Long,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Long,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Float,
			UnaryOperatorKind.Double,
			UnaryOperatorKind.Decimal,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Long | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Long | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Float | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Double | UnaryOperatorKind.Lifted,
			UnaryOperatorKind.Decimal | UnaryOperatorKind.Lifted
		};

		private static readonly UnaryOperatorKind[] s_logicalNegation;

		private static readonly UnaryOperatorKind[] s_bitwiseComplement;

		private static readonly UnaryOperatorKind[][] s_opkind;

		private static int? TypeToIndex(TypeSymbol type)
		{
			switch (type.GetSpecialTypeSafe())
			{
			case SpecialType.System_Object:
				return 0;
			case SpecialType.System_String:
				return 1;
			case SpecialType.System_Boolean:
				return 2;
			case SpecialType.System_Char:
				return 3;
			case SpecialType.System_SByte:
				return 4;
			case SpecialType.System_Int16:
				return 5;
			case SpecialType.System_Int32:
				return 6;
			case SpecialType.System_Int64:
				return 7;
			case SpecialType.System_Byte:
				return 8;
			case SpecialType.System_UInt16:
				return 9;
			case SpecialType.System_UInt32:
				return 10;
			case SpecialType.System_UInt64:
				return 11;
			case SpecialType.System_Single:
				return 12;
			case SpecialType.System_Double:
				return 13;
			case SpecialType.System_Decimal:
				return 14;
			case SpecialType.None:
				if ((object)type != null && type.IsNullableType())
				{
					switch (type.GetNullableUnderlyingType().GetSpecialTypeSafe())
					{
					case SpecialType.System_Boolean:
						return 15;
					case SpecialType.System_Char:
						return 16;
					case SpecialType.System_SByte:
						return 17;
					case SpecialType.System_Int16:
						return 18;
					case SpecialType.System_Int32:
						return 19;
					case SpecialType.System_Int64:
						return 20;
					case SpecialType.System_Byte:
						return 21;
					case SpecialType.System_UInt16:
						return 22;
					case SpecialType.System_UInt32:
						return 23;
					case SpecialType.System_UInt64:
						return 24;
					case SpecialType.System_Single:
						return 25;
					case SpecialType.System_Double:
						return 26;
					case SpecialType.System_Decimal:
						return 27;
					}
				}
				break;
			}
			return null;
		}

		public static UnaryOperatorKind OpKind(UnaryOperatorKind kind, TypeSymbol operand)
		{
			int? num = TypeToIndex(operand);
			if (!num.HasValue)
			{
				return UnaryOperatorKind.Error;
			}
			int num2 = kind.OperatorIndex();
			UnaryOperatorKind unaryOperatorKind = ((num2 < s_opkind.Length) ? s_opkind[num2][num.Value] : UnaryOperatorKind.Error);
			if (unaryOperatorKind != UnaryOperatorKind.Error)
			{
				return unaryOperatorKind | kind;
			}
			return unaryOperatorKind;
		}

		static UnopEasyOut()
		{
			UnaryOperatorKind[] array = new UnaryOperatorKind[28];
			array[2] = UnaryOperatorKind.Bool;
			array[15] = UnaryOperatorKind.Bool | UnaryOperatorKind.Lifted;
			s_logicalNegation = array;
			s_bitwiseComplement = new UnaryOperatorKind[28]
			{
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Int,
				UnaryOperatorKind.Int,
				UnaryOperatorKind.Int,
				UnaryOperatorKind.Int,
				UnaryOperatorKind.Long,
				UnaryOperatorKind.Int,
				UnaryOperatorKind.Int,
				UnaryOperatorKind.UInt,
				UnaryOperatorKind.ULong,
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.Long | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.Int | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.UInt | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.ULong | UnaryOperatorKind.Lifted,
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Error,
				UnaryOperatorKind.Error
			};
			s_opkind = new UnaryOperatorKind[8][] { s_increment, s_increment, s_increment, s_increment, s_plus, s_minus, s_logicalNegation, s_bitwiseComplement };
		}
	}

	private class ReturnStatements : BoundTreeWalker
	{
		private readonly ArrayBuilder<BoundReturnStatement> _returns;

		public ReturnStatements(ArrayBuilder<BoundReturnStatement> returns)
		{
			_returns = returns;
		}

		public override BoundNode Visit(BoundNode node)
		{
			if (!(node is BoundExpression))
			{
				return base.Visit(node);
			}
			return null;
		}

		protected override BoundExpression VisitExpressionWithoutStackGuard(BoundExpression node)
		{
			throw ExceptionUtilities.Unreachable;
		}

		public override BoundNode VisitLocalFunctionStatement(BoundLocalFunctionStatement node)
		{
			return null;
		}

		public override BoundNode VisitReturnStatement(BoundReturnStatement node)
		{
			_returns.Add(node);
			return null;
		}
	}

	private struct EffectiveParameters
	{
		internal readonly ImmutableArray<TypeSymbol> ParameterTypes;

		internal readonly ImmutableArray<RefKind> ParameterRefKinds;

		internal EffectiveParameters(ImmutableArray<TypeSymbol> types, ImmutableArray<RefKind> refKinds)
		{
			ParameterTypes = types;
			ParameterRefKinds = refKinds;
		}
	}

	private struct ParameterMap(int[] parameters, int length)
	{
		private readonly int[] _parameters = parameters;

		private readonly int _length = length;

		public bool IsTrivial => _parameters == null;

		public int Length => _length;

		public int this[int argument]
		{
			get
			{
				if (_parameters != null)
				{
					return _parameters[argument];
				}
				return argument;
			}
		}

		public ImmutableArray<int> ToImmutableArray()
		{
			return _parameters.AsImmutableOrNull();
		}
	}

	private readonly Binder _binder;

	private bool? _strict;

	private const int BetterConversionTargetRecursionLimit = 100;

	private CSharpCompilation Compilation => _binder.Compilation;

	private Conversions Conversions => _binder.Conversions;

	private bool Strict
	{
		get
		{
			if (_strict.HasValue)
			{
				return _strict.Value;
			}
			bool featureStrictEnabled = _binder.Compilation.FeatureStrictEnabled;
			_strict = featureStrictEnabled;
			return featureStrictEnabled;
		}
	}

	private void BinaryOperatorEasyOut(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, BinaryOperatorOverloadResolutionResult result)
	{
		TypeSymbol type = left.Type;
		if ((object)type == null)
		{
			return;
		}
		TypeSymbol type2 = right.Type;
		if ((object)type2 != null && !PossiblyUnusualConstantOperation(left, right))
		{
			BinaryOperatorKind binaryOperatorKind = BinopEasyOut.OpKind(kind, type, type2);
			if (binaryOperatorKind != BinaryOperatorKind.Error)
			{
				BinaryOperatorSignature signature = Compilation.builtInOperators.GetSignature(binaryOperatorKind);
				Conversion leftConversion = ConversionsBase.FastClassifyConversion(left.Type, signature.LeftType);
				Conversion rightConversion = ConversionsBase.FastClassifyConversion(right.Type, signature.RightType);
				result.Results.Add(BinaryOperatorAnalysisResult.Applicable(signature, leftConversion, rightConversion));
			}
		}
	}

	private static bool PossiblyUnusualConstantOperation(BoundExpression left, BoundExpression right)
	{
		if (left.ConstantValue == null && right.ConstantValue == null)
		{
			return false;
		}
		if (left.Type.SpecialType != right.Type.SpecialType)
		{
			return true;
		}
		if (left.Type.SpecialType == SpecialType.System_Int32 || left.Type.SpecialType == SpecialType.System_Boolean || left.Type.SpecialType == SpecialType.System_String)
		{
			return false;
		}
		return true;
	}

	public void BinaryOperatorOverloadResolution(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, BinaryOperatorOverloadResolutionResult result, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		BinaryOperatorKind kind2 = kind & ~BinaryOperatorKind.Logical;
		BinaryOperatorEasyOut(kind2, left, right, result);
		if (result.Results.Count <= 0)
		{
			if (!GetUserDefinedOperators(kind2, left, right, result.Results, ref useSiteDiagnostics))
			{
				result.Results.Clear();
				GetAllBuiltInOperators(kind, left, right, result.Results, ref useSiteDiagnostics);
			}
			BinaryOperatorOverloadResolution(left, right, result, ref useSiteDiagnostics);
		}
	}

	private void AddDelegateOperation(BinaryOperatorKind kind, TypeSymbol delegateType, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		switch (kind)
		{
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
			operators.Add(new BinaryOperatorSignature(kind | BinaryOperatorKind.Delegate, delegateType, delegateType, Compilation.GetSpecialType(SpecialType.System_Boolean)));
			break;
		default:
			operators.Add(new BinaryOperatorSignature(kind | BinaryOperatorKind.Delegate, delegateType, delegateType, delegateType));
			break;
		}
	}

	private void GetDelegateOperations(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorSignature> operators, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		switch (kind)
		{
		case BinaryOperatorKind.Multiplication:
		case BinaryOperatorKind.Division:
		case BinaryOperatorKind.Remainder:
		case BinaryOperatorKind.LeftShift:
		case BinaryOperatorKind.RightShift:
		case BinaryOperatorKind.GreaterThan:
		case BinaryOperatorKind.LessThan:
		case BinaryOperatorKind.GreaterThanOrEqual:
		case BinaryOperatorKind.LessThanOrEqual:
		case BinaryOperatorKind.And:
		case BinaryOperatorKind.Xor:
		case BinaryOperatorKind.Or:
		case BinaryOperatorKind.LogicalAnd:
		case BinaryOperatorKind.LogicalOr:
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		case BinaryOperatorKind.Addition:
		case BinaryOperatorKind.Subtraction:
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
		{
			TypeSymbol type = left.Type;
			bool flag = type?.IsDelegateType() ?? false;
			TypeSymbol type2 = right.Type;
			bool flag2 = type2?.IsDelegateType() ?? false;
			if (!flag && !flag2)
			{
				BinaryOperatorKind binaryOperatorKind = kind.Operator();
				if (binaryOperatorKind == BinaryOperatorKind.Equal || binaryOperatorKind == BinaryOperatorKind.NotEqual)
				{
					TypeSymbol specialType = _binder.GetSpecialType(SpecialType.System_Delegate, _binder.Compilation.DeclarationDiagnostics, left.Syntax);
					if (Conversions.ClassifyImplicitConversionFromExpression(left, specialType, ref useSiteDiagnostics).IsValid && Conversions.ClassifyImplicitConversionFromExpression(right, specialType, ref useSiteDiagnostics).IsValid)
					{
						AddDelegateOperation(kind, specialType, operators);
					}
				}
			}
			else if (flag & flag2)
			{
				AddDelegateOperation(kind, type, operators);
				if (!((kind == BinaryOperatorKind.Equal || kind == BinaryOperatorKind.NotEqual) ? ConversionsBase.HasIdentityConversion(type, type2) : type.Equals(type2)))
				{
					AddDelegateOperation(kind, type2, operators);
				}
			}
			else
			{
				TypeSymbol delegateType = (flag ? type : type2);
				BoundExpression boundExpression = (flag ? right : left);
				if ((kind != BinaryOperatorKind.Equal && kind != BinaryOperatorKind.NotEqual) || boundExpression.Kind != BoundKind.UnboundLambda)
				{
					AddDelegateOperation(kind, delegateType, operators);
				}
			}
			break;
		}
		}
	}

	private void GetEnumOperation(BinaryOperatorKind kind, TypeSymbol enumType, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		if (!enumType.IsValidEnumType())
		{
			return;
		}
		NamedTypeSymbol enumUnderlyingType = enumType.GetEnumUnderlyingType();
		NamedTypeSymbol specialType = Compilation.GetSpecialType(SpecialType.System_Nullable_T);
		NamedTypeSymbol namedTypeSymbol = specialType.Construct(enumType);
		NamedTypeSymbol namedTypeSymbol2 = specialType.Construct(enumUnderlyingType);
		switch (kind)
		{
		case BinaryOperatorKind.Addition:
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.EnumAndUnderlyingAddition, enumType, enumUnderlyingType, enumType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.UnderlyingAndEnumAddition, enumUnderlyingType, enumType, enumType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.LiftedEnumAndUnderlyingAddition, namedTypeSymbol, namedTypeSymbol2, namedTypeSymbol));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.LiftedUnderlyingAndEnumAddition, namedTypeSymbol2, namedTypeSymbol, namedTypeSymbol));
			break;
		case BinaryOperatorKind.Subtraction:
		{
			if (Strict)
			{
				operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.EnumSubtraction, enumType, enumType, enumUnderlyingType));
				operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.EnumAndUnderlyingSubtraction, enumType, enumUnderlyingType, enumType));
				operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.LiftedEnumSubtraction, namedTypeSymbol, namedTypeSymbol, namedTypeSymbol2));
				operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.LiftedEnumAndUnderlyingSubtraction, namedTypeSymbol, namedTypeSymbol2, namedTypeSymbol));
				break;
			}
			bool flag = right.Type?.StrippedType() == enumUnderlyingType;
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.EnumSubtraction, enumType, enumType, enumUnderlyingType)
			{
				Priority = 2
			});
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.EnumAndUnderlyingSubtraction, enumType, enumUnderlyingType, enumType)
			{
				Priority = (flag ? 1 : 3)
			});
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.LiftedEnumSubtraction, namedTypeSymbol, namedTypeSymbol, namedTypeSymbol2)
			{
				Priority = 12
			});
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.LiftedEnumAndUnderlyingSubtraction, namedTypeSymbol, namedTypeSymbol2, namedTypeSymbol)
			{
				Priority = (flag ? 11 : 13)
			});
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.UnderlyingAndEnumSubtraction, enumUnderlyingType, enumType, enumType)
			{
				Priority = 4
			});
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.LiftedUnderlyingAndEnumSubtraction, namedTypeSymbol2, namedTypeSymbol, namedTypeSymbol)
			{
				Priority = 14
			});
			break;
		}
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
		case BinaryOperatorKind.GreaterThan:
		case BinaryOperatorKind.LessThan:
		case BinaryOperatorKind.GreaterThanOrEqual:
		case BinaryOperatorKind.LessThanOrEqual:
		{
			NamedTypeSymbol specialType2 = Compilation.GetSpecialType(SpecialType.System_Boolean);
			operators.Add(new BinaryOperatorSignature(kind | BinaryOperatorKind.Enum, enumType, enumType, specialType2));
			operators.Add(new BinaryOperatorSignature(kind | BinaryOperatorKind.Lifted | BinaryOperatorKind.Enum, namedTypeSymbol, namedTypeSymbol, specialType2));
			break;
		}
		case BinaryOperatorKind.And:
		case BinaryOperatorKind.Xor:
		case BinaryOperatorKind.Or:
			operators.Add(new BinaryOperatorSignature(kind | BinaryOperatorKind.Enum, enumType, enumType, enumType));
			operators.Add(new BinaryOperatorSignature(kind | BinaryOperatorKind.Lifted | BinaryOperatorKind.Enum, namedTypeSymbol, namedTypeSymbol, namedTypeSymbol));
			break;
		}
	}

	private void GetPointerArithmeticOperators(BinaryOperatorKind kind, PointerTypeSymbol pointerType, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		switch (kind)
		{
		case BinaryOperatorKind.Addition:
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerAndIntAddition, pointerType, Compilation.GetSpecialType(SpecialType.System_Int32), pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerAndUIntAddition, pointerType, Compilation.GetSpecialType(SpecialType.System_UInt32), pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerAndLongAddition, pointerType, Compilation.GetSpecialType(SpecialType.System_Int64), pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerAndULongAddition, pointerType, Compilation.GetSpecialType(SpecialType.System_UInt64), pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.IntAndPointerAddition, Compilation.GetSpecialType(SpecialType.System_Int32), pointerType, pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.UIntAndPointerAddition, Compilation.GetSpecialType(SpecialType.System_UInt32), pointerType, pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.LongAndPointerAddition, Compilation.GetSpecialType(SpecialType.System_Int64), pointerType, pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.ULongAndPointerAddition, Compilation.GetSpecialType(SpecialType.System_UInt64), pointerType, pointerType));
			break;
		case BinaryOperatorKind.Subtraction:
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerAndIntSubtraction, pointerType, Compilation.GetSpecialType(SpecialType.System_Int32), pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerAndUIntSubtraction, pointerType, Compilation.GetSpecialType(SpecialType.System_UInt32), pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerAndLongSubtraction, pointerType, Compilation.GetSpecialType(SpecialType.System_Int64), pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerAndULongSubtraction, pointerType, Compilation.GetSpecialType(SpecialType.System_UInt64), pointerType));
			operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.PointerSubtraction, pointerType, pointerType, Compilation.GetSpecialType(SpecialType.System_Int64)));
			break;
		}
	}

	private void GetPointerComparisonOperators(BinaryOperatorKind kind, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		switch (kind)
		{
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
		case BinaryOperatorKind.GreaterThan:
		case BinaryOperatorKind.LessThan:
		case BinaryOperatorKind.GreaterThanOrEqual:
		case BinaryOperatorKind.LessThanOrEqual:
		{
			PointerTypeSymbol pointerTypeSymbol = new PointerTypeSymbol(Compilation.GetSpecialType(SpecialType.System_Void));
			operators.Add(new BinaryOperatorSignature(kind | BinaryOperatorKind.Pointer, pointerTypeSymbol, pointerTypeSymbol, Compilation.GetSpecialType(SpecialType.System_Boolean)));
			break;
		}
		}
	}

	private void GetEnumOperations(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorSignature> results)
	{
		switch (kind)
		{
		case BinaryOperatorKind.Multiplication:
		case BinaryOperatorKind.Division:
		case BinaryOperatorKind.Remainder:
		case BinaryOperatorKind.LeftShift:
		case BinaryOperatorKind.RightShift:
		case BinaryOperatorKind.LogicalAnd:
		case BinaryOperatorKind.LogicalOr:
			return;
		}
		TypeSymbol typeSymbol = left.Type;
		if ((object)typeSymbol != null)
		{
			typeSymbol = typeSymbol.StrippedType();
		}
		TypeSymbol typeSymbol2 = right.Type;
		if ((object)typeSymbol2 != null)
		{
			typeSymbol2 = typeSymbol2.StrippedType();
		}
		bool flag;
		switch (kind)
		{
		case BinaryOperatorKind.And:
		case BinaryOperatorKind.Xor:
		case BinaryOperatorKind.Or:
			flag = false;
			break;
		case BinaryOperatorKind.Addition:
			flag = true;
			break;
		case BinaryOperatorKind.Subtraction:
			flag = true;
			break;
		case BinaryOperatorKind.Equal:
		case BinaryOperatorKind.NotEqual:
		case BinaryOperatorKind.GreaterThan:
		case BinaryOperatorKind.LessThan:
		case BinaryOperatorKind.GreaterThanOrEqual:
		case BinaryOperatorKind.LessThanOrEqual:
			flag = true;
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
		if ((object)typeSymbol != null)
		{
			GetEnumOperation(kind, typeSymbol, left, right, results);
		}
		if ((object)typeSymbol2 != null && ((object)typeSymbol == null || !(flag ? ConversionsBase.HasIdentityConversion(typeSymbol2, typeSymbol) : typeSymbol2.Equals(typeSymbol))))
		{
			GetEnumOperation(kind, typeSymbol2, left, right, results);
		}
	}

	private void GetPointerOperators(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorSignature> results)
	{
		PointerTypeSymbol pointerTypeSymbol = left.Type as PointerTypeSymbol;
		PointerTypeSymbol pointerTypeSymbol2 = right.Type as PointerTypeSymbol;
		if ((object)pointerTypeSymbol != null)
		{
			GetPointerArithmeticOperators(kind, pointerTypeSymbol, results);
		}
		if ((object)pointerTypeSymbol2 != null && ((object)pointerTypeSymbol == null || !ConversionsBase.HasIdentityConversion(pointerTypeSymbol2, pointerTypeSymbol)))
		{
			GetPointerArithmeticOperators(kind, pointerTypeSymbol2, results);
		}
		if ((object)pointerTypeSymbol != null || (object)pointerTypeSymbol2 != null)
		{
			GetPointerComparisonOperators(kind, results);
		}
	}

	private void GetAllBuiltInOperators(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorAnalysisResult> results, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		kind = kind.OperatorWithLogical();
		ArrayBuilder<BinaryOperatorSignature> instance = ArrayBuilder<BinaryOperatorSignature>.GetInstance();
		if ((kind == BinaryOperatorKind.Equal || kind == BinaryOperatorKind.NotEqual) && UseOnlyReferenceEquality(left, right, ref useSiteDiagnostics))
		{
			GetReferenceEquality(kind, instance);
		}
		else
		{
			Compilation.builtInOperators.GetSimpleBuiltInOperators(kind, instance);
			GetDelegateOperations(kind, left, right, instance, ref useSiteDiagnostics);
			GetEnumOperations(kind, left, right, instance);
			GetPointerOperators(kind, left, right, instance);
		}
		CandidateOperators(instance, left, right, results, ref useSiteDiagnostics);
		instance.Free();
	}

	private bool UseOnlyReferenceEquality(BoundExpression left, BoundExpression right, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (BuiltInOperators.IsValidObjectEquality(Conversions, left.Type, left.IsLiteralNull(), right.Type, right.IsLiteralNull(), ref useSiteDiagnostics) && ((object)left.Type == null || (!left.Type.IsDelegateType() && left.Type.SpecialType != SpecialType.System_String && left.Type.SpecialType != SpecialType.System_Delegate)))
		{
			if ((object)right.Type != null)
			{
				if (!right.Type.IsDelegateType() && right.Type.SpecialType != SpecialType.System_String)
				{
					return right.Type.SpecialType != SpecialType.System_Delegate;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private void GetReferenceEquality(BinaryOperatorKind kind, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		NamedTypeSymbol specialType = Compilation.GetSpecialType(SpecialType.System_Object);
		operators.Add(new BinaryOperatorSignature(kind | BinaryOperatorKind.Object, specialType, specialType, Compilation.GetSpecialType(SpecialType.System_Boolean)));
	}

	private bool CandidateOperators(ArrayBuilder<BinaryOperatorSignature> operators, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorAnalysisResult> results, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		bool result = false;
		foreach (BinaryOperatorSignature @operator in operators)
		{
			Conversion leftConversion = Conversions.ClassifyConversionFromExpression(left, @operator.LeftType, ref useSiteDiagnostics);
			Conversion rightConversion = Conversions.ClassifyConversionFromExpression(right, @operator.RightType, ref useSiteDiagnostics);
			if (leftConversion.IsImplicit && rightConversion.IsImplicit)
			{
				results.Add(BinaryOperatorAnalysisResult.Applicable(@operator, leftConversion, rightConversion));
				result = true;
			}
			else
			{
				results.Add(BinaryOperatorAnalysisResult.Inapplicable(@operator, leftConversion, rightConversion));
			}
		}
		return result;
	}

	private bool GetUserDefinedOperators(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorAnalysisResult> results, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayBuilder<BinaryOperatorAnalysisResult> instance = ArrayBuilder<BinaryOperatorAnalysisResult>.GetInstance();
		TypeSymbol typeSymbol = left.Type?.StrippedType();
		bool flag = false;
		if ((object)typeSymbol != null && !OperatorFacts.DefinitelyHasNoUserDefinedOperators(typeSymbol))
		{
			flag = GetUserDefinedOperators(kind, typeSymbol, left, right, instance, ref useSiteDiagnostics);
			if (!flag)
			{
				instance.Clear();
			}
		}
		TypeSymbol typeSymbol2 = right.Type?.StrippedType();
		if ((object)typeSymbol2 != null && !typeSymbol2.Equals(typeSymbol) && !OperatorFacts.DefinitelyHasNoUserDefinedOperators(typeSymbol2))
		{
			ArrayBuilder<BinaryOperatorAnalysisResult> instance2 = ArrayBuilder<BinaryOperatorAnalysisResult>.GetInstance();
			flag |= GetUserDefinedOperators(kind, typeSymbol2, left, right, instance2, ref useSiteDiagnostics);
			AddDistinctOperators(instance, instance2);
			instance2.Free();
		}
		if (flag)
		{
			results.AddRange(instance);
		}
		instance.Free();
		return flag;
	}

	private static void AddDistinctOperators(ArrayBuilder<BinaryOperatorAnalysisResult> result, ArrayBuilder<BinaryOperatorAnalysisResult> additionalOperators)
	{
		int count = result.Count;
		foreach (BinaryOperatorAnalysisResult additionalOperator in additionalOperators)
		{
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				BinaryOperatorSignature signature = result[i].Signature;
				if (additionalOperator.Signature.Kind == signature.Kind && additionalOperator.Signature.ReturnType.Equals(signature.ReturnType) && additionalOperator.Signature.LeftType.Equals(signature.LeftType, TypeCompareKind.IgnoreDynamicAndTupleNames) && additionalOperator.Signature.RightType.Equals(signature.RightType, TypeCompareKind.IgnoreDynamicAndTupleNames) && additionalOperator.Signature.Method.ContainingType.Equals(signature.Method.ContainingType, TypeCompareKind.IgnoreDynamicAndTupleNames))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				result.Add(additionalOperator);
			}
		}
	}

	private bool GetUserDefinedOperators(BinaryOperatorKind kind, TypeSymbol type0, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorAnalysisResult> results, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		string name = OperatorFacts.BinaryOperatorNameFromOperatorKind(kind);
		ArrayBuilder<BinaryOperatorSignature> instance = ArrayBuilder<BinaryOperatorSignature>.GetInstance();
		bool result = false;
		NamedTypeSymbol namedTypeSymbol = type0 as NamedTypeSymbol;
		if ((object)namedTypeSymbol == null)
		{
			namedTypeSymbol = type0.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		if ((object)namedTypeSymbol == null && type0.IsTypeParameter())
		{
			namedTypeSymbol = ((TypeParameterSymbol)type0).EffectiveBaseClass(ref useSiteDiagnostics);
		}
		while ((object)namedTypeSymbol != null)
		{
			instance.Clear();
			GetUserDefinedBinaryOperatorsFromType(namedTypeSymbol, kind, name, instance);
			results.Clear();
			if (CandidateOperators(instance, left, right, results, ref useSiteDiagnostics))
			{
				result = true;
				break;
			}
			namedTypeSymbol = namedTypeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		instance.Free();
		return result;
	}

	private void GetUserDefinedBinaryOperatorsFromType(NamedTypeSymbol type, BinaryOperatorKind kind, string name, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		foreach (MethodSymbol @operator in type.GetOperators(name))
		{
			if (@operator.ParameterCount == 2 && !@operator.ReturnsVoid)
			{
				TypeSymbol typeSymbol = @operator.ParameterTypes[0];
				TypeSymbol typeSymbol2 = @operator.ParameterTypes[1];
				TypeSymbol returnType = @operator.ReturnType;
				operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.UserDefined | kind, typeSymbol, typeSymbol2, returnType, @operator));
				switch (UserDefinedBinaryOperatorCanBeLifted(typeSymbol, typeSymbol2, returnType, kind))
				{
				case LiftingResult.LiftOperandsAndResult:
					operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.UserDefined | BinaryOperatorKind.Lifted | kind, MakeNullable(typeSymbol), MakeNullable(typeSymbol2), MakeNullable(returnType), @operator));
					break;
				case LiftingResult.LiftOperandsButNotResult:
					operators.Add(new BinaryOperatorSignature(BinaryOperatorKind.UserDefined | BinaryOperatorKind.Lifted | kind, MakeNullable(typeSymbol), MakeNullable(typeSymbol2), returnType, @operator));
					break;
				}
			}
		}
	}

	private static LiftingResult UserDefinedBinaryOperatorCanBeLifted(TypeSymbol left, TypeSymbol right, TypeSymbol result, BinaryOperatorKind kind)
	{
		if (!left.IsValueType || left.IsNullableType() || !right.IsValueType || right.IsNullableType())
		{
			return LiftingResult.NotLifted;
		}
		if (kind <= BinaryOperatorKind.GreaterThan)
		{
			if (kind != BinaryOperatorKind.Equal && kind != BinaryOperatorKind.NotEqual)
			{
				if (kind != BinaryOperatorKind.GreaterThan)
				{
					goto IL_0076;
				}
			}
			else if (left != right)
			{
				return LiftingResult.NotLifted;
			}
		}
		else if (kind != BinaryOperatorKind.LessThan && kind != BinaryOperatorKind.GreaterThanOrEqual && kind != BinaryOperatorKind.LessThanOrEqual)
		{
			goto IL_0076;
		}
		if (result.SpecialType != SpecialType.System_Boolean)
		{
			return LiftingResult.NotLifted;
		}
		return LiftingResult.LiftOperandsButNotResult;
		IL_0076:
		if (!result.IsValueType || result.IsNullableType())
		{
			return LiftingResult.NotLifted;
		}
		return LiftingResult.LiftOperandsAndResult;
	}

	private void BinaryOperatorOverloadResolution(BoundExpression left, BoundExpression right, BinaryOperatorOverloadResolutionResult result, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (result.SingleValid())
		{
			return;
		}
		ArrayBuilder<BinaryOperatorAnalysisResult> results = result.Results;
		int theBestCandidateIndex = GetTheBestCandidateIndex(left, right, results, ref useSiteDiagnostics);
		if (theBestCandidateIndex != -1)
		{
			for (int i = 0; i < results.Count; i++)
			{
				if (results[i].Kind != OperatorAnalysisResultKind.Inapplicable && i != theBestCandidateIndex)
				{
					results[i] = results[i].Worse();
				}
			}
			return;
		}
		for (int j = 1; j < results.Count; j++)
		{
			if (results[j].Kind != OperatorAnalysisResultKind.Applicable)
			{
				continue;
			}
			for (int k = 0; k < j; k++)
			{
				if (results[k].Kind != OperatorAnalysisResultKind.Inapplicable)
				{
					switch (BetterOperator(results[j].Signature, results[k].Signature, left, right, ref useSiteDiagnostics))
					{
					case BetterResult.Left:
						results[k] = results[k].Worse();
						break;
					case BetterResult.Right:
						results[j] = results[j].Worse();
						break;
					}
				}
			}
		}
	}

	private int GetTheBestCandidateIndex(BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorAnalysisResult> candidates, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		int num = -1;
		for (int i = 0; i < candidates.Count; i++)
		{
			if (candidates[i].Kind != OperatorAnalysisResultKind.Applicable)
			{
				continue;
			}
			if (num == -1)
			{
				num = i;
				continue;
			}
			switch (BetterOperator(candidates[num].Signature, candidates[i].Signature, left, right, ref useSiteDiagnostics))
			{
			case BetterResult.Right:
				num = i;
				break;
			default:
				num = -1;
				break;
			case BetterResult.Left:
				break;
			}
		}
		for (int j = 0; j < num; j++)
		{
			if (candidates[j].Kind != OperatorAnalysisResultKind.Inapplicable && BetterOperator(candidates[num].Signature, candidates[j].Signature, left, right, ref useSiteDiagnostics) != BetterResult.Left)
			{
				return -1;
			}
		}
		return num;
	}

	private BetterResult BetterOperator(BinaryOperatorSignature op1, BinaryOperatorSignature op2, BoundExpression left, BoundExpression right, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (op1.Priority.HasValue && op1.Priority.GetValueOrDefault() != op2.Priority.GetValueOrDefault())
		{
			if (op1.Priority.GetValueOrDefault() >= op2.Priority.GetValueOrDefault())
			{
				return BetterResult.Right;
			}
			return BetterResult.Left;
		}
		BetterResult betterResult = BetterConversionFromExpression(left, op1.LeftType, op2.LeftType, ref useSiteDiagnostics);
		BetterResult betterResult2 = BetterConversionFromExpression(right, op1.RightType, op2.RightType, ref useSiteDiagnostics);
		if ((betterResult == BetterResult.Left && betterResult2 != BetterResult.Right) || (betterResult != BetterResult.Right && betterResult2 == BetterResult.Left))
		{
			return BetterResult.Left;
		}
		if ((betterResult == BetterResult.Right && betterResult2 != BetterResult.Left) || (betterResult != BetterResult.Left && betterResult2 == BetterResult.Right))
		{
			return BetterResult.Right;
		}
		if (ConversionsBase.HasIdentityConversion(op1.LeftType, op2.LeftType) && ConversionsBase.HasIdentityConversion(op1.RightType, op2.RightType))
		{
			BetterResult betterResult3 = MoreSpecificOperator(op1, op2, ref useSiteDiagnostics);
			if (betterResult3 == BetterResult.Left || betterResult3 == BetterResult.Right)
			{
				return betterResult3;
			}
			bool flag = op1.Kind.IsLifted();
			bool flag2 = op2.Kind.IsLifted();
			if (flag && !flag2)
			{
				return BetterResult.Right;
			}
			if (!flag & flag2)
			{
				return BetterResult.Left;
			}
		}
		BetterResult betterResult4 = ((op1.LeftRefKind != RefKind.None || op2.LeftRefKind != RefKind.In) ? ((op2.LeftRefKind == RefKind.None && op1.LeftRefKind == RefKind.In) ? BetterResult.Right : BetterResult.Neither) : BetterResult.Left);
		if (op1.RightRefKind == RefKind.None && op2.RightRefKind == RefKind.In)
		{
			if (betterResult4 == BetterResult.Right)
			{
				return BetterResult.Neither;
			}
			betterResult4 = BetterResult.Left;
		}
		else if (op2.RightRefKind == RefKind.None && op1.RightRefKind == RefKind.In)
		{
			if (betterResult4 == BetterResult.Left)
			{
				return BetterResult.Neither;
			}
			betterResult4 = BetterResult.Right;
		}
		return betterResult4;
	}

	private BetterResult MoreSpecificOperator(BinaryOperatorSignature op1, BinaryOperatorSignature op2, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeSymbol typeSymbol;
		TypeSymbol typeSymbol2;
		if ((object)op1.Method != null)
		{
			ImmutableArray<ParameterSymbol> parameters = op1.Method.OriginalDefinition.GetParameters();
			typeSymbol = parameters[0].Type;
			typeSymbol2 = parameters[1].Type;
			if (op1.Kind.IsLifted())
			{
				typeSymbol = MakeNullable(typeSymbol);
				typeSymbol2 = MakeNullable(typeSymbol2);
			}
		}
		else
		{
			typeSymbol = op1.LeftType;
			typeSymbol2 = op1.RightType;
		}
		TypeSymbol typeSymbol3;
		TypeSymbol typeSymbol4;
		if ((object)op2.Method != null)
		{
			ImmutableArray<ParameterSymbol> parameters2 = op2.Method.OriginalDefinition.GetParameters();
			typeSymbol3 = parameters2[0].Type;
			typeSymbol4 = parameters2[1].Type;
			if (op2.Kind.IsLifted())
			{
				typeSymbol3 = MakeNullable(typeSymbol3);
				typeSymbol4 = MakeNullable(typeSymbol4);
			}
		}
		else
		{
			typeSymbol3 = op2.LeftType;
			typeSymbol4 = op2.RightType;
		}
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		ArrayBuilder<TypeSymbol> instance2 = ArrayBuilder<TypeSymbol>.GetInstance();
		instance.Add(typeSymbol);
		instance.Add(typeSymbol2);
		instance2.Add(typeSymbol3);
		instance2.Add(typeSymbol4);
		BetterResult result = MoreSpecificType(instance, instance2, ref useSiteDiagnostics);
		instance.Free();
		instance2.Free();
		return result;
	}

	[Conditional("DEBUG")]
	private static void AssertNotChecked(BinaryOperatorKind kind)
	{
	}

	private void UnaryOperatorEasyOut(UnaryOperatorKind kind, BoundExpression operand, UnaryOperatorOverloadResolutionResult result)
	{
		TypeSymbol type = operand.Type;
		if ((object)type != null)
		{
			UnaryOperatorKind unaryOperatorKind = UnopEasyOut.OpKind(kind, type);
			if (unaryOperatorKind != UnaryOperatorKind.Error)
			{
				UnaryOperatorSignature signature = Compilation.builtInOperators.GetSignature(unaryOperatorKind);
				Conversion? conversion = ConversionsBase.FastClassifyConversion(type, signature.OperandType);
				result.Results.Add(UnaryOperatorAnalysisResult.Applicable(signature, conversion.Value));
			}
		}
	}

	private NamedTypeSymbol MakeNullable(TypeSymbol type)
	{
		return Compilation.GetSpecialType(SpecialType.System_Nullable_T).Construct(type);
	}

	public void UnaryOperatorOverloadResolution(UnaryOperatorKind kind, BoundExpression operand, UnaryOperatorOverloadResolutionResult result, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		UnaryOperatorEasyOut(kind, operand, result);
		if (result.Results.Count <= 0)
		{
			if (!GetUserDefinedOperators(kind, operand, result.Results, ref useSiteDiagnostics))
			{
				result.Results.Clear();
				GetAllBuiltInOperators(kind, operand, result.Results, ref useSiteDiagnostics);
			}
			UnaryOperatorOverloadResolution(operand, result, ref useSiteDiagnostics);
		}
	}

	private void UnaryOperatorOverloadResolution(BoundExpression operand, UnaryOperatorOverloadResolutionResult result, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (result.SingleValid())
		{
			return;
		}
		ArrayBuilder<UnaryOperatorAnalysisResult> results = result.Results;
		int theBestCandidateIndex = GetTheBestCandidateIndex(operand, results, ref useSiteDiagnostics);
		if (theBestCandidateIndex != -1)
		{
			for (int i = 0; i < results.Count; i++)
			{
				if (results[i].Kind != OperatorAnalysisResultKind.Inapplicable && i != theBestCandidateIndex)
				{
					results[i] = results[i].Worse();
				}
			}
			return;
		}
		for (int j = 1; j < results.Count; j++)
		{
			if (results[j].Kind != OperatorAnalysisResultKind.Applicable)
			{
				continue;
			}
			for (int k = 0; k < j; k++)
			{
				if (results[k].Kind != OperatorAnalysisResultKind.Inapplicable)
				{
					switch (BetterOperator(results[j].Signature, results[k].Signature, operand, ref useSiteDiagnostics))
					{
					case BetterResult.Left:
						results[k] = results[k].Worse();
						break;
					case BetterResult.Right:
						results[j] = results[j].Worse();
						break;
					}
				}
			}
		}
	}

	private int GetTheBestCandidateIndex(BoundExpression operand, ArrayBuilder<UnaryOperatorAnalysisResult> candidates, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		int num = -1;
		for (int i = 0; i < candidates.Count; i++)
		{
			if (candidates[i].Kind != OperatorAnalysisResultKind.Applicable)
			{
				continue;
			}
			if (num == -1)
			{
				num = i;
				continue;
			}
			switch (BetterOperator(candidates[num].Signature, candidates[i].Signature, operand, ref useSiteDiagnostics))
			{
			case BetterResult.Right:
				num = i;
				break;
			default:
				num = -1;
				break;
			case BetterResult.Left:
				break;
			}
		}
		for (int j = 0; j < num; j++)
		{
			if (candidates[j].Kind != OperatorAnalysisResultKind.Inapplicable && BetterOperator(candidates[num].Signature, candidates[j].Signature, operand, ref useSiteDiagnostics) != BetterResult.Left)
			{
				return -1;
			}
		}
		return num;
	}

	private BetterResult BetterOperator(UnaryOperatorSignature op1, UnaryOperatorSignature op2, BoundExpression operand, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		BetterResult betterResult = BetterConversionFromExpression(operand, op1.OperandType, op2.OperandType, ref useSiteDiagnostics);
		if (betterResult == BetterResult.Left || betterResult == BetterResult.Right)
		{
			return betterResult;
		}
		if (ConversionsBase.HasIdentityConversion(op1.OperandType, op2.OperandType))
		{
			bool flag = op1.Kind.IsLifted();
			bool flag2 = op2.Kind.IsLifted();
			if (flag && !flag2)
			{
				return BetterResult.Right;
			}
			if (!flag & flag2)
			{
				return BetterResult.Left;
			}
		}
		if (op1.RefKind == RefKind.None && op2.RefKind == RefKind.In)
		{
			return BetterResult.Left;
		}
		if (op2.RefKind == RefKind.None && op1.RefKind == RefKind.In)
		{
			return BetterResult.Right;
		}
		return BetterResult.Neither;
	}

	private void GetAllBuiltInOperators(UnaryOperatorKind kind, BoundExpression operand, ArrayBuilder<UnaryOperatorAnalysisResult> results, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayBuilder<UnaryOperatorSignature> instance = ArrayBuilder<UnaryOperatorSignature>.GetInstance();
		Compilation.builtInOperators.GetSimpleBuiltInOperators(kind, instance);
		GetEnumOperations(kind, operand, instance);
		UnaryOperatorSignature? pointerOperation = GetPointerOperation(kind, operand);
		if (pointerOperation.HasValue)
		{
			instance.Add(pointerOperation.Value);
		}
		CandidateOperators(instance, operand, results, ref useSiteDiagnostics);
		instance.Free();
	}

	private bool CandidateOperators(ArrayBuilder<UnaryOperatorSignature> operators, BoundExpression operand, ArrayBuilder<UnaryOperatorAnalysisResult> results, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		bool result = false;
		foreach (UnaryOperatorSignature @operator in operators)
		{
			Conversion conversion = Conversions.ClassifyConversionFromExpression(operand, @operator.OperandType, ref useSiteDiagnostics);
			if (conversion.IsImplicit)
			{
				result = true;
				results.Add(UnaryOperatorAnalysisResult.Applicable(@operator, conversion));
			}
			else
			{
				results.Add(UnaryOperatorAnalysisResult.Inapplicable(@operator, conversion));
			}
		}
		return result;
	}

	private void GetEnumOperations(UnaryOperatorKind kind, BoundExpression operand, ArrayBuilder<UnaryOperatorSignature> operators)
	{
		TypeSymbol type = operand.Type;
		if ((object)type == null)
		{
			return;
		}
		type = type.StrippedType();
		if (type.IsValidEnumType())
		{
			NamedTypeSymbol namedTypeSymbol = MakeNullable(type);
			switch (kind)
			{
			case UnaryOperatorKind.PostfixIncrement:
			case UnaryOperatorKind.PostfixDecrement:
			case UnaryOperatorKind.PrefixIncrement:
			case UnaryOperatorKind.PrefixDecrement:
			case UnaryOperatorKind.BitwiseComplement:
				operators.Add(new UnaryOperatorSignature(kind | UnaryOperatorKind.Enum, type, type));
				operators.Add(new UnaryOperatorSignature(kind | UnaryOperatorKind.Lifted | UnaryOperatorKind.Enum, namedTypeSymbol, namedTypeSymbol));
				break;
			}
		}
	}

	private static UnaryOperatorSignature? GetPointerOperation(UnaryOperatorKind kind, BoundExpression operand)
	{
		if (!(operand.Type is PointerTypeSymbol pointerTypeSymbol))
		{
			return null;
		}
		UnaryOperatorSignature? result = null;
		switch (kind)
		{
		case UnaryOperatorKind.PostfixIncrement:
		case UnaryOperatorKind.PostfixDecrement:
		case UnaryOperatorKind.PrefixIncrement:
		case UnaryOperatorKind.PrefixDecrement:
			result = new UnaryOperatorSignature(kind | UnaryOperatorKind.Pointer, pointerTypeSymbol, pointerTypeSymbol);
			break;
		}
		return result;
	}

	private bool GetUserDefinedOperators(UnaryOperatorKind kind, BoundExpression operand, ArrayBuilder<UnaryOperatorAnalysisResult> results, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((object)operand.Type == null)
		{
			return false;
		}
		TypeSymbol typeSymbol = operand.Type.StrippedType();
		if (OperatorFacts.DefinitelyHasNoUserDefinedOperators(typeSymbol))
		{
			return false;
		}
		string name = OperatorFacts.UnaryOperatorNameFromOperatorKind(kind);
		ArrayBuilder<UnaryOperatorSignature> instance = ArrayBuilder<UnaryOperatorSignature>.GetInstance();
		bool result = false;
		NamedTypeSymbol namedTypeSymbol = typeSymbol as NamedTypeSymbol;
		if ((object)namedTypeSymbol == null)
		{
			namedTypeSymbol = typeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		if ((object)namedTypeSymbol == null && typeSymbol.IsTypeParameter())
		{
			namedTypeSymbol = ((TypeParameterSymbol)typeSymbol).EffectiveBaseClass(ref useSiteDiagnostics);
		}
		while ((object)namedTypeSymbol != null)
		{
			instance.Clear();
			GetUserDefinedUnaryOperatorsFromType(namedTypeSymbol, kind, name, instance);
			results.Clear();
			if (CandidateOperators(instance, operand, results, ref useSiteDiagnostics))
			{
				result = true;
				break;
			}
			namedTypeSymbol = namedTypeSymbol.BaseTypeWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics);
		}
		instance.Free();
		return result;
	}

	private void GetUserDefinedUnaryOperatorsFromType(NamedTypeSymbol type, UnaryOperatorKind kind, string name, ArrayBuilder<UnaryOperatorSignature> operators)
	{
		foreach (MethodSymbol @operator in type.GetOperators(name))
		{
			if (@operator.ParameterCount != 1 || @operator.ReturnsVoid)
			{
				continue;
			}
			TypeSymbol typeSymbol = @operator.ParameterTypes[0];
			TypeSymbol returnType = @operator.ReturnType;
			operators.Add(new UnaryOperatorSignature(UnaryOperatorKind.UserDefined | kind, typeSymbol, returnType, @operator));
			switch (kind)
			{
			case UnaryOperatorKind.PostfixIncrement:
			case UnaryOperatorKind.PostfixDecrement:
			case UnaryOperatorKind.PrefixIncrement:
			case UnaryOperatorKind.PrefixDecrement:
			case UnaryOperatorKind.UnaryPlus:
			case UnaryOperatorKind.UnaryMinus:
			case UnaryOperatorKind.LogicalNegation:
			case UnaryOperatorKind.BitwiseComplement:
				if (typeSymbol.IsValueType && !typeSymbol.IsNullableType() && returnType.IsValueType && !returnType.IsNullableType())
				{
					operators.Add(new UnaryOperatorSignature(UnaryOperatorKind.UserDefined | UnaryOperatorKind.Lifted | kind, MakeNullable(typeSymbol), MakeNullable(returnType), @operator));
				}
				break;
			}
		}
	}

	public OverloadResolution(Binder binder)
	{
		_binder = binder;
	}

	private static bool AnyValidResult<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results) where TMember : Symbol
	{
		foreach (MemberResolutionResult<TMember> result in results)
		{
			if (result.IsValid)
			{
				return true;
			}
		}
		return false;
	}

	private static bool SingleValidResult<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results) where TMember : Symbol
	{
		bool flag = false;
		foreach (MemberResolutionResult<TMember> result in results)
		{
			if (result.IsValid)
			{
				if (flag)
				{
					return false;
				}
				flag = true;
			}
		}
		return flag;
	}

	public void ObjectCreationOverloadResolution(ImmutableArray<MethodSymbol> constructors, AnalyzedArguments arguments, OverloadResolutionResult<MethodSymbol> result, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayBuilder<MemberResolutionResult<MethodSymbol>> resultsBuilder = result.ResultsBuilder;
		PerformObjectCreationOverloadResolution(resultsBuilder, constructors, arguments, completeResults: false, ref useSiteDiagnostics);
		if (!OverloadResolutionResultIsValid(resultsBuilder, arguments.HasDynamicArgument))
		{
			result.Clear();
			PerformObjectCreationOverloadResolution(resultsBuilder, constructors, arguments, completeResults: true, ref useSiteDiagnostics);
		}
	}

	public void MethodInvocationOverloadResolution(ArrayBuilder<MethodSymbol> methods, ArrayBuilder<TypeSymbol> typeArguments, BoundExpression receiver, AnalyzedArguments arguments, OverloadResolutionResult<MethodSymbol> result, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool isMethodGroupConversion = false, bool allowRefOmittedArguments = false, bool inferWithDynamic = false, bool allowUnexpandedForm = true, RefKind returnRefKind = RefKind.None, TypeSymbol returnType = null)
	{
		MethodOrPropertyOverloadResolution(methods, typeArguments, receiver, arguments, result, isMethodGroupConversion, allowRefOmittedArguments, ref useSiteDiagnostics, inferWithDynamic, allowUnexpandedForm, returnRefKind, returnType);
	}

	public void PropertyOverloadResolution(ArrayBuilder<PropertySymbol> indexers, BoundExpression receiverOpt, AnalyzedArguments arguments, OverloadResolutionResult<PropertySymbol> result, bool allowRefOmittedArguments, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		MethodOrPropertyOverloadResolution(indexers, instance, receiverOpt, arguments, result, isMethodGroupConversion: false, allowRefOmittedArguments, ref useSiteDiagnostics);
		instance.Free();
	}

	internal void MethodOrPropertyOverloadResolution<TMember>(ArrayBuilder<TMember> members, ArrayBuilder<TypeSymbol> typeArguments, BoundExpression receiver, AnalyzedArguments arguments, OverloadResolutionResult<TMember> result, bool isMethodGroupConversion, bool allowRefOmittedArguments, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool inferWithDynamic = false, bool allowUnexpandedForm = true, RefKind returnRefKind = RefKind.None, TypeSymbol returnType = null) where TMember : Symbol
	{
		ArrayBuilder<MemberResolutionResult<TMember>> resultsBuilder = result.ResultsBuilder;
		PerformMemberOverloadResolution(resultsBuilder, members, typeArguments, receiver, arguments, completeResults: false, isMethodGroupConversion, returnRefKind, returnType, allowRefOmittedArguments, ref useSiteDiagnostics, inferWithDynamic, allowUnexpandedForm);
		if (!OverloadResolutionResultIsValid(resultsBuilder, arguments.HasDynamicArgument))
		{
			result.Clear();
			PerformMemberOverloadResolution(resultsBuilder, members, typeArguments, receiver, arguments, completeResults: true, isMethodGroupConversion, returnRefKind, returnType, allowRefOmittedArguments, ref useSiteDiagnostics, inferWithDynamic: false, allowUnexpandedForm);
		}
	}

	private static bool OverloadResolutionResultIsValid<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, bool hasDynamicArgument) where TMember : Symbol
	{
		if (hasDynamicArgument)
		{
			foreach (MemberResolutionResult<TMember> result in results)
			{
				if (result.Result.IsApplicable)
				{
					return true;
				}
			}
			return false;
		}
		return SingleValidResult(results);
	}

	private void PerformMemberOverloadResolution<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, ArrayBuilder<TMember> members, ArrayBuilder<TypeSymbol> typeArguments, BoundExpression receiver, AnalyzedArguments arguments, bool completeResults, bool isMethodGroupConversion, RefKind returnRefKind, TypeSymbol returnType, bool allowRefOmittedArguments, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool inferWithDynamic = false, bool allowUnexpandedForm = true) where TMember : Symbol
	{
		Dictionary<NamedTypeSymbol, ArrayBuilder<TMember>> containingTypeMapOpt = null;
		if (members.Count > 50)
		{
			containingTypeMapOpt = PartitionMembersByContainingType(members);
		}
		for (int i = 0; i < members.Count; i++)
		{
			AddMemberToCandidateSet(members[i], results, members, typeArguments, receiver, arguments, completeResults, isMethodGroupConversion, allowRefOmittedArguments, containingTypeMapOpt, inferWithDynamic, ref useSiteDiagnostics, allowUnexpandedForm);
		}
		ClearContainingTypeMap(ref containingTypeMapOpt);
		RemoveInaccessibleTypeArguments(results, ref useSiteDiagnostics);
		RemoveLessDerivedMembers(results, ref useSiteDiagnostics);
		if (Compilation.LanguageVersion.AllowImprovedOverloadCandidates())
		{
			RemoveStaticInstanceMismatches(results, arguments, receiver);
			RemoveConstraintViolations(results);
			if (isMethodGroupConversion)
			{
				RemoveDelegateConversionsWithWrongReturnType(results, ref useSiteDiagnostics, returnRefKind, returnType);
			}
		}
		ReportUseSiteDiagnostics(results, ref useSiteDiagnostics);
		if (AnyValidResult(results))
		{
			RemoveWorseMembers(results, arguments, ref useSiteDiagnostics);
		}
	}

	private void RemoveStaticInstanceMismatches<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, AnalyzedArguments arguments, BoundExpression receiverOpt) where TMember : Symbol
	{
		if (arguments.IsExtensionMethodInvocation || Binder.IsTypeOrValueExpression(receiverOpt))
		{
			return;
		}
		bool flag = Binder.WasImplicitReceiver(receiverOpt);
		bool flag2 = !_binder.HasThis(!flag, out var inStaticContext) | inStaticContext;
		if (flag && !flag2)
		{
			return;
		}
		bool flag3 = (flag & flag2) || Binder.IsMemberAccessedThroughType(receiverOpt);
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult = results[i];
			TMember member = memberResolutionResult.Member;
			if (memberResolutionResult.Result.IsValid && member.IsStatic != flag3)
			{
				results[i] = new MemberResolutionResult<TMember>(member, memberResolutionResult.LeastOverriddenMember, MemberAnalysisResult.StaticInstanceMismatch());
			}
		}
	}

	private void RemoveConstraintViolations<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results) where TMember : Symbol
	{
		if ((object)typeof(TMember) != typeof(MethodSymbol))
		{
			return;
		}
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult = results[i];
			MethodSymbol method = (MethodSymbol)(object)memberResolutionResult.Member;
			if ((memberResolutionResult.Result.IsValid || memberResolutionResult.Result.Kind == MemberResolutionKind.ConstructedParameterFailedConstraintCheck) && FailsConstraintChecks(method, out var constraintFailureDiagnosticsOpt))
			{
				results[i] = new MemberResolutionResult<TMember>(memberResolutionResult.Member, memberResolutionResult.LeastOverriddenMember, MemberAnalysisResult.ConstraintFailure(constraintFailureDiagnosticsOpt.ToImmutableAndFree()));
			}
		}
	}

	private bool FailsConstraintChecks(MethodSymbol method, out ArrayBuilder<TypeParameterDiagnosticInfo> constraintFailureDiagnosticsOpt)
	{
		if (method.Arity == 0 || (object)method.OriginalDefinition == method)
		{
			constraintFailureDiagnosticsOpt = null;
			return false;
		}
		ArrayBuilder<TypeParameterDiagnosticInfo> instance = ArrayBuilder<TypeParameterDiagnosticInfo>.GetInstance();
		ArrayBuilder<TypeParameterDiagnosticInfo> useSiteDiagnosticsBuilder = null;
		if (!ConstraintsHelper.CheckMethodConstraints(method, Conversions, Compilation, instance, ref useSiteDiagnosticsBuilder))
		{
			if (useSiteDiagnosticsBuilder != null)
			{
				instance.AddRange(useSiteDiagnosticsBuilder);
				useSiteDiagnosticsBuilder.Free();
			}
			constraintFailureDiagnosticsOpt = instance;
			return true;
		}
		instance.Free();
		useSiteDiagnosticsBuilder?.Free();
		constraintFailureDiagnosticsOpt = null;
		return false;
	}

	private void RemoveDelegateConversionsWithWrongReturnType<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, ref HashSet<DiagnosticInfo> useSiteDiagnostics, RefKind? returnRefKind, TypeSymbol returnType) where TMember : Symbol
	{
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult = results[i];
			if (memberResolutionResult.Result.IsValid)
			{
				MethodSymbol methodSymbol = (MethodSymbol)(object)memberResolutionResult.Member;
				if ((object)returnType != null && !methodSymbol.ReturnType.Equals(returnType, TypeCompareKind.AllIgnoreOptions) && (returnRefKind != RefKind.None || !Conversions.HasIdentityOrImplicitReferenceConversion(methodSymbol.ReturnType, returnType, ref useSiteDiagnostics)))
				{
					results[i] = new MemberResolutionResult<TMember>(memberResolutionResult.Member, memberResolutionResult.LeastOverriddenMember, MemberAnalysisResult.WrongReturnType());
				}
				else if (methodSymbol.RefKind != returnRefKind)
				{
					results[i] = new MemberResolutionResult<TMember>(memberResolutionResult.Member, memberResolutionResult.LeastOverriddenMember, MemberAnalysisResult.WrongRefKind());
				}
			}
		}
	}

	private static Dictionary<NamedTypeSymbol, ArrayBuilder<TMember>> PartitionMembersByContainingType<TMember>(ArrayBuilder<TMember> members) where TMember : Symbol
	{
		Dictionary<NamedTypeSymbol, ArrayBuilder<TMember>> dictionary = new Dictionary<NamedTypeSymbol, ArrayBuilder<TMember>>();
		for (int i = 0; i < members.Count; i++)
		{
			TMember val = members[i];
			NamedTypeSymbol containingType = val.ContainingType;
			if (!dictionary.TryGetValue(containingType, out var value))
			{
				value = (dictionary[containingType] = ArrayBuilder<TMember>.GetInstance());
			}
			value.Add(val);
		}
		return dictionary;
	}

	private static void ClearContainingTypeMap<TMember>(ref Dictionary<NamedTypeSymbol, ArrayBuilder<TMember>> containingTypeMapOpt) where TMember : Symbol
	{
		if (containingTypeMapOpt == null)
		{
			return;
		}
		foreach (ArrayBuilder<TMember> value in containingTypeMapOpt.Values)
		{
			value.Free();
		}
		containingTypeMapOpt = null;
	}

	private void AddConstructorToCandidateSet(MethodSymbol constructor, ArrayBuilder<MemberResolutionResult<MethodSymbol>> results, AnalyzedArguments arguments, bool completeResults, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (constructor.HasUnsupportedMetadata)
		{
			if (completeResults)
			{
				results.Add(new MemberResolutionResult<MethodSymbol>(constructor, constructor, MemberAnalysisResult.UnsupportedMetadata()));
			}
			return;
		}
		MemberAnalysisResult memberAnalysisResult = IsConstructorApplicableInNormalForm(constructor, arguments, completeResults, ref useSiteDiagnostics);
		MemberAnalysisResult result = memberAnalysisResult;
		if (!memberAnalysisResult.IsValid && IsValidParams(constructor))
		{
			MemberAnalysisResult memberAnalysisResult2 = IsConstructorApplicableInExpandedForm(constructor, arguments, completeResults, ref useSiteDiagnostics);
			if (memberAnalysisResult2.IsValid | completeResults)
			{
				result = memberAnalysisResult2;
			}
		}
		if ((result.IsValid | completeResults) || result.HasUseSiteDiagnosticToReportFor(constructor))
		{
			results.Add(new MemberResolutionResult<MethodSymbol>(constructor, constructor, result));
		}
	}

	private MemberAnalysisResult IsConstructorApplicableInNormalForm(MethodSymbol constructor, AnalyzedArguments arguments, bool completeResults, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArgumentAnalysisResult argAnalysis = AnalyzeArguments(constructor, arguments, isMethodGroupConversion: false, expanded: false);
		if (!argAnalysis.IsValid)
		{
			return MemberAnalysisResult.ArgumentParameterMismatch(argAnalysis);
		}
		if (constructor.HasUseSiteError)
		{
			return MemberAnalysisResult.UseSiteError();
		}
		EffectiveParameters effectiveParametersInNormalForm = GetEffectiveParametersInNormalForm(constructor, arguments.Arguments.Count, argAnalysis.ArgsToParamsOpt, arguments.RefKinds, isMethodGroupConversion: false, allowRefOmittedArguments: false);
		return IsApplicable(constructor, effectiveParametersInNormalForm, arguments, argAnalysis.ArgsToParamsOpt, constructor.IsVararg, hasAnyRefOmittedArgument: false, ignoreOpenTypes: false, completeResults, ref useSiteDiagnostics);
	}

	private MemberAnalysisResult IsConstructorApplicableInExpandedForm(MethodSymbol constructor, AnalyzedArguments arguments, bool completeResults, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArgumentAnalysisResult argAnalysis = AnalyzeArguments(constructor, arguments, isMethodGroupConversion: false, expanded: true);
		if (!argAnalysis.IsValid)
		{
			return MemberAnalysisResult.ArgumentParameterMismatch(argAnalysis);
		}
		if (constructor.HasUseSiteError)
		{
			return MemberAnalysisResult.UseSiteError();
		}
		EffectiveParameters effectiveParametersInExpandedForm = GetEffectiveParametersInExpandedForm(constructor, arguments.Arguments.Count, argAnalysis.ArgsToParamsOpt, arguments.RefKinds, isMethodGroupConversion: false, allowRefOmittedArguments: false);
		MemberAnalysisResult result = IsApplicable(constructor, effectiveParametersInExpandedForm, arguments, argAnalysis.ArgsToParamsOpt, isVararg: false, hasAnyRefOmittedArgument: false, ignoreOpenTypes: false, completeResults, ref useSiteDiagnostics);
		if (!result.IsValid)
		{
			return result;
		}
		return MemberAnalysisResult.ExpandedForm(result.ArgsToParamsOpt, result.ConversionsOpt, hasAnyRefOmittedArgument: false);
	}

	private void AddMemberToCandidateSet<TMember>(TMember member, ArrayBuilder<MemberResolutionResult<TMember>> results, ArrayBuilder<TMember> members, ArrayBuilder<TypeSymbol> typeArguments, BoundExpression receiverOpt, AnalyzedArguments arguments, bool completeResults, bool isMethodGroupConversion, bool allowRefOmittedArguments, Dictionary<NamedTypeSymbol, ArrayBuilder<TMember>> containingTypeMapOpt, bool inferWithDynamic, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool allowUnexpandedForm) where TMember : Symbol
	{
		if (members.Count >= 2)
		{
			if (containingTypeMapOpt == null)
			{
				if (MemberGroupContainsOverride(members, member) || MemberGroupHidesByName(members, member, ref useSiteDiagnostics))
				{
					return;
				}
			}
			else if (containingTypeMapOpt.Count != 1)
			{
				NamedTypeSymbol containingType = member.ContainingType;
				foreach (KeyValuePair<NamedTypeSymbol, ArrayBuilder<TMember>> item2 in containingTypeMapOpt)
				{
					if (item2.Key.IsDerivedFrom(containingType, TypeCompareKind.ConsiderEverything, ref useSiteDiagnostics))
					{
						ArrayBuilder<TMember> value = item2.Value;
						if (MemberGroupContainsOverride(value, member) || MemberGroupHidesByName(value, member, ref useSiteDiagnostics))
						{
							return;
						}
					}
				}
			}
		}
		TMember val = (TMember)member.GetLeastOverriddenMember(_binder.ContainingType);
		if (member.HasUnsupportedMetadata)
		{
			if (completeResults)
			{
				results.Add(new MemberResolutionResult<TMember>(member, val, MemberAnalysisResult.UnsupportedMetadata()));
			}
			return;
		}
		MemberResolutionResult<TMember> memberResolutionResult = ((allowUnexpandedForm || !IsValidParams(val)) ? IsMemberApplicableInNormalForm(member, val, typeArguments, arguments, isMethodGroupConversion, allowRefOmittedArguments, inferWithDynamic, completeResults, ref useSiteDiagnostics) : default(MemberResolutionResult<TMember>));
		MemberResolutionResult<TMember> item = memberResolutionResult;
		if (!memberResolutionResult.Result.IsValid && !isMethodGroupConversion && IsValidParams(val))
		{
			MemberResolutionResult<TMember> memberResolutionResult2 = IsMemberApplicableInExpandedForm(member, val, typeArguments, arguments, allowRefOmittedArguments, completeResults, ref useSiteDiagnostics);
			if (PreferExpandedFormOverNormalForm(memberResolutionResult.Result, memberResolutionResult2.Result))
			{
				item = memberResolutionResult2;
			}
		}
		if ((item.Result.IsValid | completeResults) || item.HasUseSiteDiagnosticToReport)
		{
			results.Add(item);
		}
	}

	private static bool PreferExpandedFormOverNormalForm(MemberAnalysisResult normalResult, MemberAnalysisResult expandedResult)
	{
		if (expandedResult.IsValid)
		{
			return true;
		}
		MemberResolutionKind kind = normalResult.Kind;
		if (kind == MemberResolutionKind.NoCorrespondingParameter || kind == MemberResolutionKind.RequiredParameterMissing)
		{
			switch (expandedResult.Kind)
			{
			case MemberResolutionKind.NoCorrespondingNamedParameter:
			case MemberResolutionKind.DuplicateNamedArgument:
			case MemberResolutionKind.NameUsedForPositional:
			case MemberResolutionKind.BadNonTrailingNamedArgument:
			case MemberResolutionKind.UseSiteError:
			case MemberResolutionKind.BadArgumentConversion:
			case MemberResolutionKind.TypeInferenceFailed:
			case MemberResolutionKind.TypeInferenceExtensionInstanceArgument:
			case MemberResolutionKind.ConstructedParameterFailedConstraintCheck:
				return true;
			}
		}
		return false;
	}

	public static bool IsValidParams(Symbol member)
	{
		if (member.GetIsVararg())
		{
			return false;
		}
		if (member.GetParameterCount() == 0)
		{
			return false;
		}
		ParameterSymbol parameterSymbol = member.GetParameters().Last();
		if (parameterSymbol.IsParams)
		{
			return parameterSymbol.OriginalDefinition.Type.IsSZArray();
		}
		return false;
	}

	private static bool IsOverride(Symbol overridden, Symbol overrider)
	{
		if (overridden.ContainingType == overrider.ContainingType || !MemberSignatureComparer.SloppyOverrideComparer.Equals(overridden, overrider))
		{
			return false;
		}
		Symbol symbol = overrider;
		do
		{
			if (!symbol.IsOverride)
			{
				return false;
			}
			symbol = symbol.GetOverriddenMember();
			if ((object)symbol == null)
			{
				return false;
			}
			if (symbol == overridden)
			{
				return true;
			}
		}
		while (!(symbol.ContainingType == overridden.ContainingType));
		return false;
	}

	private static bool MemberGroupContainsOverride<TMember>(ArrayBuilder<TMember> members, TMember member) where TMember : Symbol
	{
		if (!member.IsVirtual && !member.IsAbstract && !member.IsOverride)
		{
			return false;
		}
		for (int i = 0; i < members.Count; i++)
		{
			if (IsOverride(member, members[i]))
			{
				return true;
			}
		}
		return false;
	}

	private static bool MemberGroupHidesByName<TMember>(ArrayBuilder<TMember> members, TMember member, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		NamedTypeSymbol containingType = member.ContainingType;
		foreach (TMember member2 in members)
		{
			NamedTypeSymbol containingType2 = member2.ContainingType;
			if (HidesByName(member2) && containingType2.IsDerivedFrom(containingType, TypeCompareKind.ConsiderEverything, ref useSiteDiagnostics))
			{
				return true;
			}
		}
		return false;
	}

	private static bool HidesByName(Symbol member)
	{
		return member.Kind switch
		{
			SymbolKind.Method => ((MethodSymbol)member).HidesBaseMethodsByName, 
			SymbolKind.Property => ((PropertySymbol)member).HidesBasePropertiesByName, 
			_ => throw ExceptionUtilities.UnexpectedValue(member.Kind), 
		};
	}

	private void RemoveInaccessibleTypeArguments<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult = results[i];
			if (memberResolutionResult.Result.IsValid && !TypeArgumentsAccessible(memberResolutionResult.Member.GetMemberTypeArgumentsNoUseSiteDiagnostics(), ref useSiteDiagnostics))
			{
				results[i] = new MemberResolutionResult<TMember>(memberResolutionResult.Member, memberResolutionResult.LeastOverriddenMember, MemberAnalysisResult.InaccessibleTypeArgument());
			}
		}
	}

	private bool TypeArgumentsAccessible(ImmutableArray<TypeSymbol> typeArguments, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		foreach (TypeSymbol item in typeArguments)
		{
			if (!_binder.IsAccessible(item, ref useSiteDiagnostics))
			{
				return false;
			}
		}
		return true;
	}

	private static void RemoveLessDerivedMembers<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		RemoveAllInterfaceMembers(results);
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult = results[i];
			if ((memberResolutionResult.Result.IsValid || memberResolutionResult.HasUseSiteDiagnosticToReport) && IsLessDerivedThanAny(memberResolutionResult.LeastOverriddenMember.ContainingType, results, ref useSiteDiagnostics))
			{
				results[i] = new MemberResolutionResult<TMember>(memberResolutionResult.Member, memberResolutionResult.LeastOverriddenMember, MemberAnalysisResult.LessDerived());
			}
		}
	}

	private static bool IsLessDerivedThanAny<TMember>(TypeSymbol type, ArrayBuilder<MemberResolutionResult<TMember>> results, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult = results[i];
			if (memberResolutionResult.Result.IsValid)
			{
				NamedTypeSymbol containingType = memberResolutionResult.LeastOverriddenMember.ContainingType;
				if (type.SpecialType == SpecialType.System_Object && containingType.SpecialType != SpecialType.System_Object)
				{
					return true;
				}
				if (containingType.IsInterfaceType() && type.IsInterfaceType() && containingType.AllInterfacesWithDefinitionUseSiteDiagnostics(ref useSiteDiagnostics).Contains((NamedTypeSymbol)type))
				{
					return true;
				}
				if (containingType.IsClassType() && type.IsClassType() && containingType.IsDerivedFrom(type, TypeCompareKind.ConsiderEverything, ref useSiteDiagnostics))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void RemoveAllInterfaceMembers<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results) where TMember : Symbol
	{
		bool flag = false;
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> memberResolutionResult = results[i];
			if (memberResolutionResult.Result.IsValid)
			{
				NamedTypeSymbol containingType = memberResolutionResult.LeastOverriddenMember.ContainingType;
				if (containingType.IsClassType() && containingType.GetSpecialTypeSafe() != SpecialType.System_Object)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		for (int j = 0; j < results.Count; j++)
		{
			MemberResolutionResult<TMember> memberResolutionResult2 = results[j];
			if (memberResolutionResult2.Result.IsValid)
			{
				TMember member = memberResolutionResult2.Member;
				if (member.ContainingType.IsInterfaceType())
				{
					results[j] = new MemberResolutionResult<TMember>(member, memberResolutionResult2.LeastOverriddenMember, MemberAnalysisResult.LessDerived());
				}
			}
		}
	}

	private void PerformObjectCreationOverloadResolution(ArrayBuilder<MemberResolutionResult<MethodSymbol>> results, ImmutableArray<MethodSymbol> constructors, AnalyzedArguments arguments, bool completeResults, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		foreach (MethodSymbol item in constructors)
		{
			AddConstructorToCandidateSet(item, results, arguments, completeResults, ref useSiteDiagnostics);
		}
		ReportUseSiteDiagnostics(results, ref useSiteDiagnostics);
		RemoveWorseMembers(results, arguments, ref useSiteDiagnostics);
	}

	private static void ReportUseSiteDiagnostics<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		foreach (MemberResolutionResult<TMember> result in results)
		{
			if (result.HasUseSiteDiagnosticToReport)
			{
				useSiteDiagnostics = useSiteDiagnostics ?? new HashSet<DiagnosticInfo>();
				useSiteDiagnostics.Add(result.Member.GetUseSiteDiagnostic());
			}
		}
	}

	private int GetTheBestCandidateIndex<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, AnalyzedArguments arguments, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		int num = -1;
		for (int i = 0; i < results.Count; i++)
		{
			if (!results[i].IsValid)
			{
				continue;
			}
			if (num == -1)
			{
				num = i;
				continue;
			}
			if (results[num].Member == results[i].Member)
			{
				num = -1;
				continue;
			}
			switch (BetterFunctionMember(results[num], results[i], arguments.Arguments, ref useSiteDiagnostics))
			{
			case BetterResult.Right:
				num = i;
				break;
			default:
				num = -1;
				break;
			case BetterResult.Left:
				break;
			}
		}
		for (int j = 0; j < num; j++)
		{
			if (results[j].IsValid)
			{
				if (results[num].Member == results[j].Member)
				{
					return -1;
				}
				if (BetterFunctionMember(results[num], results[j], arguments.Arguments, ref useSiteDiagnostics) != BetterResult.Left)
				{
					return -1;
				}
			}
		}
		return num;
	}

	private void RemoveWorseMembers<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, AnalyzedArguments arguments, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		if (SingleValidResult(results))
		{
			return;
		}
		int theBestCandidateIndex = GetTheBestCandidateIndex(results, arguments, ref useSiteDiagnostics);
		if (theBestCandidateIndex != -1)
		{
			for (int i = 0; i < results.Count; i++)
			{
				if (results[i].IsValid && i != theBestCandidateIndex)
				{
					results[i] = results[i].Worse();
				}
			}
			return;
		}
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance(results.Count, 0);
		int num = 0;
		int index = -1;
		for (int j = 0; j < results.Count; j++)
		{
			MemberResolutionResult<TMember> m = results[j];
			if (!m.IsValid || instance[j] == 1)
			{
				continue;
			}
			for (int k = 0; k < results.Count; k++)
			{
				MemberResolutionResult<TMember> m2 = results[k];
				if (m2.IsValid && j != k && !(m.Member == m2.Member))
				{
					switch (BetterFunctionMember(m, m2, arguments.Arguments, ref useSiteDiagnostics))
					{
					case BetterResult.Left:
						instance[k] = 1;
						continue;
					case BetterResult.Right:
						break;
					default:
						continue;
					}
					instance[j] = 1;
					break;
				}
			}
			if (instance[j] == 0)
			{
				instance[j] = 2;
				num++;
				index = j;
			}
		}
		switch (num)
		{
		case 0:
		{
			for (int n = 0; n < instance.Count; n++)
			{
				if (instance[n] == 1)
				{
					results[n] = results[n].Worse();
				}
			}
			break;
		}
		case 1:
		{
			for (int num2 = 0; num2 < instance.Count; num2++)
			{
				if (instance[num2] == 1)
				{
					results[num2] = ((BetterFunctionMember(results[index], results[num2], arguments.Arguments, ref useSiteDiagnostics) == BetterResult.Left) ? results[num2].Worst() : results[num2].Worse());
				}
			}
			results[index] = results[index].Worse();
			break;
		}
		default:
		{
			for (int l = 0; l < instance.Count; l++)
			{
				if (instance[l] == 1)
				{
					results[l] = results[l].Worst();
				}
				else if (instance[l] == 2)
				{
					results[l] = results[l].Worse();
				}
			}
			break;
		}
		}
		instance.Free();
	}

	private static TypeSymbol GetParameterType(ParameterSymbol parameter, MemberAnalysisResult result)
	{
		if (result.Kind == MemberResolutionKind.ApplicableInExpandedForm && parameter.IsParams && parameter.Type.IsSZArray())
		{
			return ((ArrayTypeSymbol)parameter.Type).ElementType;
		}
		return parameter.Type;
	}

	private static ParameterSymbol GetParameter(int argIndex, MemberAnalysisResult result, ImmutableArray<ParameterSymbol> parameters)
	{
		int index = result.ParameterFromArgument(argIndex);
		return parameters[index];
	}

	private BetterResult BetterFunctionMember<TMember>(MemberResolutionResult<TMember> m1, MemberResolutionResult<TMember> m2, ArrayBuilder<BoundExpression> arguments, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		bool hasAnyRefOmittedArgument = m1.Result.HasAnyRefOmittedArgument;
		bool hasAnyRefOmittedArgument2 = m2.Result.HasAnyRefOmittedArgument;
		if (hasAnyRefOmittedArgument != hasAnyRefOmittedArgument2)
		{
			if (!hasAnyRefOmittedArgument)
			{
				return BetterResult.Left;
			}
			return BetterResult.Right;
		}
		return BetterFunctionMember(m1, m2, arguments, hasAnyRefOmittedArgument, ref useSiteDiagnostics);
	}

	private BetterResult BetterFunctionMember<TMember>(MemberResolutionResult<TMember> m1, MemberResolutionResult<TMember> m2, ArrayBuilder<BoundExpression> arguments, bool considerRefKinds, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		BetterResult betterResult = BetterResult.Neither;
		bool flag = false;
		bool flag2 = false;
		ImmutableArray<ParameterSymbol> parameters = m1.LeastOverriddenMember.GetParameters();
		ImmutableArray<ParameterSymbol> parameters2 = m2.LeastOverriddenMember.GetParameters();
		bool flag3 = true;
		int i;
		for (i = 0; i < arguments.Count; i++)
		{
			if (arguments[i].Kind == BoundKind.ArgListOperator)
			{
				continue;
			}
			ParameterSymbol parameter = GetParameter(i, m1.Result, parameters);
			TypeSymbol parameterType = GetParameterType(parameter, m1.Result);
			ParameterSymbol parameter2 = GetParameter(i, m2.Result, parameters2);
			TypeSymbol parameterType2 = GetParameterType(parameter2, m2.Result);
			BetterResult betterResult2 = BetterConversionFromExpression(arguments[i], parameterType, m1.Result.ConversionForArg(i), parameter.RefKind, parameterType2, m2.Result.ConversionForArg(i), parameter2.RefKind, considerRefKinds, ref useSiteDiagnostics, out var okToDowngradeToNeither);
			TypeSymbol source = parameterType.NormalizeTaskTypes(Compilation);
			TypeSymbol destination = parameterType2.NormalizeTaskTypes(Compilation);
			if (betterResult2 == BetterResult.Neither)
			{
				if (flag3 && Conversions.ClassifyImplicitConversionFromType(source, destination, ref useSiteDiagnostics).Kind != ConversionKind.Identity)
				{
					flag3 = false;
				}
				continue;
			}
			if (Conversions.ClassifyImplicitConversionFromType(source, destination, ref useSiteDiagnostics).Kind != ConversionKind.Identity)
			{
				flag3 = false;
			}
			if (betterResult == BetterResult.Neither)
			{
				if (!(flag2 & okToDowngradeToNeither))
				{
					betterResult = betterResult2;
					flag = okToDowngradeToNeither;
				}
			}
			else if (betterResult != betterResult2)
			{
				if (flag)
				{
					if (okToDowngradeToNeither)
					{
						betterResult = BetterResult.Neither;
						flag = false;
						flag2 = true;
					}
					else
					{
						betterResult = betterResult2;
						flag = false;
					}
				}
				else if (!okToDowngradeToNeither)
				{
					betterResult = BetterResult.Neither;
					break;
				}
			}
			else
			{
				flag &= okToDowngradeToNeither;
			}
		}
		if (betterResult != BetterResult.Neither)
		{
			return betterResult;
		}
		GetParameterCounts(m1, arguments, out var declaredParameterCount, out var parametersUsedIncludingExpansionAndOptional);
		GetParameterCounts(m2, arguments, out var declaredParameterCount2, out var parametersUsedIncludingExpansionAndOptional2);
		if (flag3 && parametersUsedIncludingExpansionAndOptional == parametersUsedIncludingExpansionAndOptional2)
		{
			for (i++; i < arguments.Count; i++)
			{
				if (arguments[i].Kind != BoundKind.ArgListOperator)
				{
					TypeSymbol source2 = GetParameterType(GetParameter(i, m1.Result, parameters), m1.Result).NormalizeTaskTypes(Compilation);
					TypeSymbol destination2 = GetParameterType(GetParameter(i, m2.Result, parameters2), m2.Result).NormalizeTaskTypes(Compilation);
					if (Conversions.ClassifyImplicitConversionFromType(source2, destination2, ref useSiteDiagnostics).Kind != ConversionKind.Identity)
					{
						flag3 = false;
						break;
					}
				}
			}
		}
		if (!flag3 || parametersUsedIncludingExpansionAndOptional != parametersUsedIncludingExpansionAndOptional2)
		{
			if (parametersUsedIncludingExpansionAndOptional != parametersUsedIncludingExpansionAndOptional2)
			{
				if (m1.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm)
				{
					if (m2.Result.Kind != MemberResolutionKind.ApplicableInExpandedForm)
					{
						return BetterResult.Right;
					}
				}
				else if (m2.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm)
				{
					return BetterResult.Left;
				}
				if (parametersUsedIncludingExpansionAndOptional == arguments.Count)
				{
					return BetterResult.Left;
				}
				if (parametersUsedIncludingExpansionAndOptional2 == arguments.Count)
				{
					return BetterResult.Right;
				}
			}
			return PreferValOverInParameters(arguments, m1, parameters, m2, parameters2);
		}
		if (m1.Member.GetMemberArity() == 0)
		{
			if (m2.Member.GetMemberArity() > 0)
			{
				return BetterResult.Left;
			}
		}
		else if (m2.Member.GetMemberArity() == 0)
		{
			return BetterResult.Right;
		}
		if (m1.Result.Kind == MemberResolutionKind.ApplicableInNormalForm && m2.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm)
		{
			return BetterResult.Left;
		}
		if (m1.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm && m2.Result.Kind == MemberResolutionKind.ApplicableInNormalForm)
		{
			return BetterResult.Right;
		}
		if (m1.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm && m2.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm)
		{
			if (declaredParameterCount > declaredParameterCount2)
			{
				return BetterResult.Left;
			}
			if (declaredParameterCount < declaredParameterCount2)
			{
				return BetterResult.Right;
			}
		}
		bool flag4 = m1.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm || declaredParameterCount == arguments.Count;
		bool flag5 = m2.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm || declaredParameterCount2 == arguments.Count;
		if (flag4 && !flag5)
		{
			return BetterResult.Left;
		}
		if (!flag4 & flag5)
		{
			return BetterResult.Right;
		}
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		ArrayBuilder<TypeSymbol> instance2 = ArrayBuilder<TypeSymbol>.GetInstance();
		ImmutableArray<ParameterSymbol> parameters3 = m1.LeastOverriddenMember.OriginalDefinition.GetParameters();
		ImmutableArray<ParameterSymbol> parameters4 = m2.LeastOverriddenMember.OriginalDefinition.GetParameters();
		for (i = 0; i < arguments.Count; i++)
		{
			if (arguments[i].Kind != BoundKind.ArgListOperator)
			{
				ParameterSymbol parameter3 = GetParameter(i, m1.Result, parameters3);
				instance.Add(GetParameterType(parameter3, m1.Result));
				ParameterSymbol parameter4 = GetParameter(i, m2.Result, parameters4);
				instance2.Add(GetParameterType(parameter4, m2.Result));
			}
		}
		betterResult = MoreSpecificType(instance, instance2, ref useSiteDiagnostics);
		instance.Free();
		instance2.Free();
		if (betterResult != BetterResult.Neither)
		{
			return betterResult;
		}
		if (m1.Member.ContainingType.TypeKind == TypeKind.Submission && m2.Member.ContainingType.TypeKind == TypeKind.Submission)
		{
			CSharpCompilation declaringCompilation = m1.Member.DeclaringCompilation;
			CSharpCompilation declaringCompilation2 = m2.Member.DeclaringCompilation;
			int submissionSlotIndex = declaringCompilation.GetSubmissionSlotIndex();
			int submissionSlotIndex2 = declaringCompilation2.GetSubmissionSlotIndex();
			if (submissionSlotIndex > submissionSlotIndex2)
			{
				return BetterResult.Left;
			}
			if (submissionSlotIndex < submissionSlotIndex2)
			{
				return BetterResult.Right;
			}
		}
		int num = m1.LeastOverriddenMember.CustomModifierCount();
		int num2 = m2.LeastOverriddenMember.CustomModifierCount();
		if (num != num2)
		{
			if (num >= num2)
			{
				return BetterResult.Right;
			}
			return BetterResult.Left;
		}
		return PreferValOverInParameters(arguments, m1, parameters, m2, parameters2);
	}

	private static BetterResult PreferValOverInParameters<TMember>(ArrayBuilder<BoundExpression> arguments, MemberResolutionResult<TMember> m1, ImmutableArray<ParameterSymbol> parameters1, MemberResolutionResult<TMember> m2, ImmutableArray<ParameterSymbol> parameters2) where TMember : Symbol
	{
		BetterResult betterResult = BetterResult.Neither;
		for (int i = 0; i < arguments.Count; i++)
		{
			if (arguments[i].Kind == BoundKind.ArgListOperator)
			{
				continue;
			}
			ParameterSymbol parameter = GetParameter(i, m1.Result, parameters1);
			ParameterSymbol parameter2 = GetParameter(i, m2.Result, parameters2);
			if (parameter.RefKind == RefKind.None && parameter2.RefKind == RefKind.In)
			{
				if (betterResult == BetterResult.Right)
				{
					return BetterResult.Neither;
				}
				betterResult = BetterResult.Left;
			}
			else if (parameter2.RefKind == RefKind.None && parameter.RefKind == RefKind.In)
			{
				if (betterResult == BetterResult.Left)
				{
					return BetterResult.Neither;
				}
				betterResult = BetterResult.Right;
			}
		}
		return betterResult;
	}

	private static void GetParameterCounts<TMember>(MemberResolutionResult<TMember> m, ArrayBuilder<BoundExpression> arguments, out int declaredParameterCount, out int parametersUsedIncludingExpansionAndOptional) where TMember : Symbol
	{
		declaredParameterCount = m.Member.GetParameterCount();
		if (m.Result.Kind == MemberResolutionKind.ApplicableInExpandedForm)
		{
			if (arguments.Count < declaredParameterCount)
			{
				ImmutableArray<int> argsToParamsOpt = m.Result.ArgsToParamsOpt;
				if (argsToParamsOpt.IsDefaultOrEmpty || !argsToParamsOpt.Contains(declaredParameterCount - 1))
				{
					parametersUsedIncludingExpansionAndOptional = declaredParameterCount - 1;
				}
				else
				{
					parametersUsedIncludingExpansionAndOptional = declaredParameterCount;
				}
			}
			else
			{
				parametersUsedIncludingExpansionAndOptional = arguments.Count;
			}
		}
		else
		{
			parametersUsedIncludingExpansionAndOptional = declaredParameterCount;
		}
	}

	private static BetterResult MoreSpecificType(ArrayBuilder<TypeSymbol> t1, ArrayBuilder<TypeSymbol> t2, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		BetterResult betterResult = BetterResult.Neither;
		for (int i = 0; i < t1.Count; i++)
		{
			BetterResult betterResult2 = MoreSpecificType(t1[i], t2[i], ref useSiteDiagnostics);
			if (betterResult2 != BetterResult.Neither)
			{
				if (betterResult == BetterResult.Neither)
				{
					betterResult = betterResult2;
				}
				else if (betterResult != betterResult2)
				{
					return BetterResult.Neither;
				}
			}
		}
		return betterResult;
	}

	private static BetterResult MoreSpecificType(TypeSymbol t1, TypeSymbol t2, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		bool flag = t1.IsTypeParameter();
		bool flag2 = t2.IsTypeParameter();
		if (flag && !flag2)
		{
			return BetterResult.Right;
		}
		if (!flag & flag2)
		{
			return BetterResult.Left;
		}
		if (flag & flag2)
		{
			return BetterResult.Neither;
		}
		if (t1.IsArray())
		{
			ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)t1;
			return MoreSpecificType(t2: ((ArrayTypeSymbol)t2).ElementType, t1: arrayTypeSymbol.ElementType, useSiteDiagnostics: ref useSiteDiagnostics);
		}
		if (t1.TypeKind == TypeKind.Pointer)
		{
			PointerTypeSymbol pointerTypeSymbol = (PointerTypeSymbol)t1;
			return MoreSpecificType(t2: ((PointerTypeSymbol)t2).PointedAtType, t1: pointerTypeSymbol.PointedAtType, useSiteDiagnostics: ref useSiteDiagnostics);
		}
		if (t1.IsDynamic() || t2.IsDynamic())
		{
			return BetterResult.Neither;
		}
		NamedTypeSymbol namedTypeSymbol = t1.TupleUnderlyingTypeOrSelf() as NamedTypeSymbol;
		NamedTypeSymbol namedTypeSymbol2 = t2.TupleUnderlyingTypeOrSelf() as NamedTypeSymbol;
		if ((object)namedTypeSymbol == null)
		{
			return BetterResult.Neither;
		}
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		ArrayBuilder<TypeSymbol> instance2 = ArrayBuilder<TypeSymbol>.GetInstance();
		namedTypeSymbol.GetAllTypeArguments(instance, ref useSiteDiagnostics);
		namedTypeSymbol2.GetAllTypeArguments(instance2, ref useSiteDiagnostics);
		BetterResult result = MoreSpecificType(instance, instance2, ref useSiteDiagnostics);
		instance.Free();
		instance2.Free();
		return result;
	}

	private BetterResult BetterConversionFromExpression(BoundExpression node, TypeSymbol t1, TypeSymbol t2, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		bool okToDowngradeToNeither;
		return BetterConversionFromExpression(node, t1, Conversions.ClassifyImplicitConversionFromExpression(node, t1, ref useSiteDiagnostics), t2, Conversions.ClassifyImplicitConversionFromExpression(node, t2, ref useSiteDiagnostics), ref useSiteDiagnostics, out okToDowngradeToNeither);
	}

	private BetterResult BetterConversionFromExpression(BoundExpression node, TypeSymbol t1, Conversion conv1, RefKind refKind1, TypeSymbol t2, Conversion conv2, RefKind refKind2, bool considerRefKinds, ref HashSet<DiagnosticInfo> useSiteDiagnostics, out bool okToDowngradeToNeither)
	{
		okToDowngradeToNeither = false;
		if (considerRefKinds)
		{
			if (refKind1 != refKind2)
			{
				if (refKind1 == RefKind.None)
				{
					if (conv1.Kind != ConversionKind.Identity)
					{
						return BetterResult.Neither;
					}
					return BetterResult.Left;
				}
				if (conv2.Kind != ConversionKind.Identity)
				{
					return BetterResult.Neither;
				}
				return BetterResult.Right;
			}
			if (refKind1 == RefKind.Ref)
			{
				return BetterResult.Neither;
			}
		}
		return BetterConversionFromExpression(node, t1, conv1, t2, conv2, ref useSiteDiagnostics, out okToDowngradeToNeither);
	}

	private BetterResult BetterConversionFromExpression(BoundExpression node, TypeSymbol t1, Conversion conv1, TypeSymbol t2, Conversion conv2, ref HashSet<DiagnosticInfo> useSiteDiagnostics, out bool okToDowngradeToNeither)
	{
		okToDowngradeToNeither = false;
		if (ConversionsBase.HasIdentityConversion(t1, t2))
		{
			return BetterResult.Neither;
		}
		UnboundLambda unboundLambda = node as UnboundLambda;
		BoundKind kind = node.Kind;
		if (kind == BoundKind.OutVariablePendingInference || kind == BoundKind.OutDeconstructVarPendingInference || (kind == BoundKind.DiscardExpression && !node.HasExpressionType()))
		{
			okToDowngradeToNeither = false;
			return BetterResult.Neither;
		}
		bool flag = ExpressionMatchExactly(node, t1, ref useSiteDiagnostics);
		bool flag2 = ExpressionMatchExactly(node, t2, ref useSiteDiagnostics);
		if (flag)
		{
			if (!flag2)
			{
				okToDowngradeToNeither = unboundLambda != null && CanDowngradeConversionFromLambdaToNeither(BetterResult.Left, unboundLambda, t1, t2, ref useSiteDiagnostics, fromTypeAnalysis: false);
				return BetterResult.Left;
			}
		}
		else if (flag2)
		{
			okToDowngradeToNeither = unboundLambda != null && CanDowngradeConversionFromLambdaToNeither(BetterResult.Right, unboundLambda, t1, t2, ref useSiteDiagnostics, fromTypeAnalysis: false);
			return BetterResult.Right;
		}
		return BetterConversionTarget(node, t1, conv1, t2, conv2, ref useSiteDiagnostics, out okToDowngradeToNeither);
	}

	private bool ExpressionMatchExactly(BoundExpression node, TypeSymbol t, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((object)node.Type != null && ConversionsBase.HasIdentityConversion(node.Type, t))
		{
			return true;
		}
		if (node.Kind == BoundKind.TupleLiteral)
		{
			return ExpressionMatchExactly((BoundTupleLiteral)node, t, ref useSiteDiagnostics);
		}
		NamedTypeSymbol delegateType;
		MethodSymbol delegateInvokeMethod;
		TypeSymbol typeSymbol;
		if (node.Kind == BoundKind.UnboundLambda && (object)(delegateType = t.GetDelegateType()) != null && (object)(delegateInvokeMethod = delegateType.DelegateInvokeMethod) != null && (typeSymbol = delegateInvokeMethod.ReturnType).SpecialType != SpecialType.System_Void)
		{
			BoundLambda boundLambda = ((UnboundLambda)node).BindForReturnTypeInference(delegateType);
			TypeSymbol typeSymbol2 = boundLambda.InferredReturnType(ref useSiteDiagnostics);
			if ((object)typeSymbol2 != null && ConversionsBase.HasIdentityConversion(typeSymbol2, typeSymbol))
			{
				return true;
			}
			if (boundLambda.Symbol.IsAsync)
			{
				typeSymbol = ((!typeSymbol.OriginalDefinition.IsGenericTaskType(Compilation)) ? null : ((NamedTypeSymbol)typeSymbol).TypeArgumentsNoUseSiteDiagnostics[0]);
			}
			if ((object)typeSymbol != null)
			{
				int length = boundLambda.Body.Statements.Length;
				if (length != 0)
				{
					if (length == 1 && boundLambda.Body.Statements[0].Kind == BoundKind.ReturnStatement)
					{
						BoundReturnStatement boundReturnStatement = (BoundReturnStatement)boundLambda.Body.Statements[0];
						if (boundReturnStatement.ExpressionOpt != null && ExpressionMatchExactly(boundReturnStatement.ExpressionOpt, typeSymbol, ref useSiteDiagnostics))
						{
							return true;
						}
					}
					else
					{
						ArrayBuilder<BoundReturnStatement> instance = ArrayBuilder<BoundReturnStatement>.GetInstance();
						new ReturnStatements(instance).Visit(boundLambda.Body);
						bool flag = false;
						foreach (BoundReturnStatement item in instance)
						{
							if (item.ExpressionOpt == null || !ExpressionMatchExactly(item.ExpressionOpt, typeSymbol, ref useSiteDiagnostics))
							{
								flag = false;
								break;
							}
							flag = true;
						}
						instance.Free();
						if (flag)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	private bool ExpressionMatchExactly(BoundTupleLiteral tupleSource, TypeSymbol targetType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (targetType.Kind != SymbolKind.NamedType)
		{
			return false;
		}
		NamedTypeSymbol namedTypeSymbol = (NamedTypeSymbol)targetType;
		ImmutableArray<BoundExpression> arguments = tupleSource.Arguments;
		if (!namedTypeSymbol.IsTupleOrCompatibleWithTupleOfCardinality(arguments.Length))
		{
			return false;
		}
		ImmutableArray<TypeSymbol> elementTypesOfTupleOrCompatible = namedTypeSymbol.GetElementTypesOfTupleOrCompatible();
		for (int i = 0; i < arguments.Length; i++)
		{
			if (!ExpressionMatchExactly(arguments[i], elementTypesOfTupleOrCompatible[i], ref useSiteDiagnostics))
			{
				return false;
			}
		}
		return true;
	}

	private BetterResult BetterConversionTarget(TypeSymbol type1, TypeSymbol type2, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		bool okToDowngradeToNeither;
		return BetterConversionTargetCore(null, type1, default, type2, default, ref useSiteDiagnostics, out okToDowngradeToNeither, 100);
	}

	private BetterResult BetterConversionTargetCore(TypeSymbol type1, TypeSymbol type2, ref HashSet<DiagnosticInfo> useSiteDiagnostics, int betterConversionTargetRecursionLimit)
	{
		if (betterConversionTargetRecursionLimit < 0)
		{
			return BetterResult.Neither;
		}
		bool okToDowngradeToNeither;
		return BetterConversionTargetCore(null, type1, default, type2, default, ref useSiteDiagnostics, out okToDowngradeToNeither, betterConversionTargetRecursionLimit - 1);
	}

	private BetterResult BetterConversionTarget(BoundExpression node, TypeSymbol type1, Conversion conv1, TypeSymbol type2, Conversion conv2, ref HashSet<DiagnosticInfo> useSiteDiagnostics, out bool okToDowngradeToNeither)
	{
		return BetterConversionTargetCore(node, type1, conv1, type2, conv2, ref useSiteDiagnostics, out okToDowngradeToNeither, 100);
	}

	private BetterResult BetterConversionTargetCore(BoundExpression node, TypeSymbol type1, Conversion conv1, TypeSymbol type2, Conversion conv2, ref HashSet<DiagnosticInfo> useSiteDiagnostics, out bool okToDowngradeToNeither, int betterConversionTargetRecursionLimit)
	{
		okToDowngradeToNeither = false;
		if (ConversionsBase.HasIdentityConversion(type1, type2))
		{
			return BetterResult.Neither;
		}
		bool isImplicit = Conversions.ClassifyImplicitConversionFromType(type1, type2, ref useSiteDiagnostics).IsImplicit;
		bool isImplicit2 = Conversions.ClassifyImplicitConversionFromType(type2, type1, ref useSiteDiagnostics).IsImplicit;
		UnboundLambda unboundLambda = node as UnboundLambda;
		if (isImplicit)
		{
			if (isImplicit2)
			{
				return BetterResult.Neither;
			}
			okToDowngradeToNeither = unboundLambda != null && CanDowngradeConversionFromLambdaToNeither(BetterResult.Left, unboundLambda, type1, type2, ref useSiteDiagnostics, fromTypeAnalysis: true);
			return BetterResult.Left;
		}
		if (isImplicit2)
		{
			okToDowngradeToNeither = unboundLambda != null && CanDowngradeConversionFromLambdaToNeither(BetterResult.Right, unboundLambda, type1, type2, ref useSiteDiagnostics, fromTypeAnalysis: true);
			return BetterResult.Right;
		}
		bool flag = type1.OriginalDefinition.IsGenericTaskType(Compilation);
		bool flag2 = type2.OriginalDefinition.IsGenericTaskType(Compilation);
		if (flag)
		{
			if (flag2)
			{
				return BetterConversionTargetCore(((NamedTypeSymbol)type1).TypeArgumentsNoUseSiteDiagnostics[0], ((NamedTypeSymbol)type2).TypeArgumentsNoUseSiteDiagnostics[0], ref useSiteDiagnostics, betterConversionTargetRecursionLimit);
			}
			return BetterResult.Neither;
		}
		if (flag2)
		{
			return BetterResult.Neither;
		}
		NamedTypeSymbol delegateType;
		if ((object)(delegateType = type1.GetDelegateType()) != null)
		{
			NamedTypeSymbol delegateType2;
			if ((object)(delegateType2 = type2.GetDelegateType()) != null)
			{
				MethodSymbol delegateInvokeMethod = delegateType.DelegateInvokeMethod;
				MethodSymbol delegateInvokeMethod2 = delegateType2.DelegateInvokeMethod;
				if ((object)delegateInvokeMethod != null && (object)delegateInvokeMethod2 != null)
				{
					TypeSymbol returnType = delegateInvokeMethod.ReturnType;
					TypeSymbol returnType2 = delegateInvokeMethod2.ReturnType;
					BetterResult betterResult = BetterResult.Neither;
					if (returnType.SpecialType != SpecialType.System_Void)
					{
						if (returnType2.SpecialType == SpecialType.System_Void)
						{
							betterResult = BetterResult.Left;
						}
					}
					else if (returnType2.SpecialType != SpecialType.System_Void)
					{
						betterResult = BetterResult.Right;
					}
					if (betterResult == BetterResult.Neither)
					{
						betterResult = BetterConversionTargetCore(returnType, returnType2, ref useSiteDiagnostics, betterConversionTargetRecursionLimit);
					}
					if (node != null && node.Kind == BoundKind.MethodGroup)
					{
						BoundMethodGroup node2 = (BoundMethodGroup)node;
						switch (betterResult)
						{
						case BetterResult.Left:
							if (IsMethodGroupConversionIncompatibleWithDelegate(node2, delegateType, conv1))
							{
								return BetterResult.Neither;
							}
							break;
						case BetterResult.Right:
							if (IsMethodGroupConversionIncompatibleWithDelegate(node2, delegateType2, conv2))
							{
								return BetterResult.Neither;
							}
							break;
						}
					}
					return betterResult;
				}
			}
			return BetterResult.Neither;
		}
		if (type2.GetDelegateType() != null)
		{
			return BetterResult.Neither;
		}
		if (IsSignedIntegralType(type1))
		{
			if (IsUnsignedIntegralType(type2))
			{
				return BetterResult.Left;
			}
		}
		else if (IsUnsignedIntegralType(type1) && IsSignedIntegralType(type2))
		{
			return BetterResult.Right;
		}
		return BetterResult.Neither;
	}

	private bool IsMethodGroupConversionIncompatibleWithDelegate(BoundMethodGroup node, NamedTypeSymbol delegateType, Conversion conv)
	{
		if (conv.IsMethodGroup)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			bool result = !_binder.MethodGroupIsCompatibleWithDelegate(node.ReceiverOpt, conv.IsExtensionMethod, conv.Method, delegateType, Location.None, instance);
			instance.Free();
			return result;
		}
		return false;
	}

	private bool CanDowngradeConversionFromLambdaToNeither(BetterResult currentResult, UnboundLambda lambda, TypeSymbol type1, TypeSymbol type2, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool fromTypeAnalysis)
	{
		NamedTypeSymbol delegateType;
		NamedTypeSymbol delegateType2;
		if ((object)(delegateType = type1.GetDelegateType()) != null && (object)(delegateType2 = type2.GetDelegateType()) != null)
		{
			MethodSymbol delegateInvokeMethod = delegateType.DelegateInvokeMethod;
			MethodSymbol delegateInvokeMethod2 = delegateType2.DelegateInvokeMethod;
			if ((object)delegateInvokeMethod != null && (object)delegateInvokeMethod2 != null)
			{
				if (!IdenticalParameters(delegateInvokeMethod.Parameters, delegateInvokeMethod2.Parameters))
				{
					return true;
				}
				TypeSymbol returnType = delegateInvokeMethod.ReturnType;
				TypeSymbol returnType2 = delegateInvokeMethod2.ReturnType;
				if (returnType.SpecialType == SpecialType.System_Void)
				{
					if (returnType2.SpecialType == SpecialType.System_Void)
					{
						return true;
					}
					return false;
				}
				if (returnType2.SpecialType == SpecialType.System_Void)
				{
					return false;
				}
				if (ConversionsBase.HasIdentityConversion(returnType, returnType2))
				{
					return true;
				}
				if (lambda.InferReturnType(delegateType, ref useSiteDiagnostics) == null)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IdenticalParameters(ImmutableArray<ParameterSymbol> p1, ImmutableArray<ParameterSymbol> p2)
	{
		if (p1.IsDefault || p2.IsDefault)
		{
			return false;
		}
		if (p1.Length != p2.Length)
		{
			return false;
		}
		for (int i = 0; i < p1.Length; i++)
		{
			ParameterSymbol parameterSymbol = p1[i];
			ParameterSymbol parameterSymbol2 = p2[i];
			if (parameterSymbol.RefKind != parameterSymbol2.RefKind)
			{
				return false;
			}
			if (!ConversionsBase.HasIdentityConversion(parameterSymbol.Type, parameterSymbol2.Type))
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsSignedIntegralType(TypeSymbol type)
	{
		if ((object)type != null && type.IsNullableType())
		{
			type = type.GetNullableUnderlyingType();
		}
		switch (type.GetSpecialTypeSafe())
		{
		case SpecialType.System_SByte:
		case SpecialType.System_Int16:
		case SpecialType.System_Int32:
		case SpecialType.System_Int64:
			return true;
		default:
			return false;
		}
	}

	private static bool IsUnsignedIntegralType(TypeSymbol type)
	{
		if ((object)type != null && type.IsNullableType())
		{
			type = type.GetNullableUnderlyingType();
		}
		switch (type.GetSpecialTypeSafe())
		{
		case SpecialType.System_Byte:
		case SpecialType.System_UInt16:
		case SpecialType.System_UInt32:
		case SpecialType.System_UInt64:
			return true;
		default:
			return false;
		}
	}

	private EffectiveParameters GetEffectiveParametersInNormalForm<TMember>(TMember member, int argumentCount, ImmutableArray<int> argToParamMap, ArrayBuilder<RefKind> argumentRefKinds, bool isMethodGroupConversion, bool allowRefOmittedArguments) where TMember : Symbol
	{
		bool hasAnyRefOmittedArgument;
		return GetEffectiveParametersInNormalForm(member, argumentCount, argToParamMap, argumentRefKinds, isMethodGroupConversion, allowRefOmittedArguments, out hasAnyRefOmittedArgument);
	}

	private EffectiveParameters GetEffectiveParametersInNormalForm<TMember>(TMember member, int argumentCount, ImmutableArray<int> argToParamMap, ArrayBuilder<RefKind> argumentRefKinds, bool isMethodGroupConversion, bool allowRefOmittedArguments, out bool hasAnyRefOmittedArgument) where TMember : Symbol
	{
		hasAnyRefOmittedArgument = false;
		ImmutableArray<ParameterSymbol> parameters = member.GetParameters();
		int num = member.GetParameterCount() + (member.GetIsVararg() ? 1 : 0);
		if (argumentCount == num && argToParamMap.IsDefaultOrEmpty)
		{
			ImmutableArray<RefKind> parameterRefKinds = member.GetParameterRefKinds();
			if (parameterRefKinds.IsDefaultOrEmpty)
			{
				return new EffectiveParameters(member.GetParameterTypes(), parameterRefKinds);
			}
		}
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		ArrayBuilder<RefKind> arrayBuilder = null;
		bool flag = argumentRefKinds.Any();
		for (int i = 0; i < argumentCount; i++)
		{
			int num2 = (argToParamMap.IsDefault ? i : argToParamMap[i]);
			if (num2 >= parameters.Length)
			{
				continue;
			}
			ParameterSymbol parameterSymbol = parameters[num2];
			instance.Add(parameterSymbol.Type);
			RefKind argRefKind = (flag ? argumentRefKinds[i] : RefKind.None);
			RefKind effectiveParameterRefKind = GetEffectiveParameterRefKind(parameterSymbol, argRefKind, isMethodGroupConversion, allowRefOmittedArguments, ref hasAnyRefOmittedArgument);
			if (arrayBuilder == null)
			{
				if (effectiveParameterRefKind != RefKind.None)
				{
					arrayBuilder = ArrayBuilder<RefKind>.GetInstance(i, RefKind.None);
					arrayBuilder.Add(effectiveParameterRefKind);
				}
			}
			else
			{
				arrayBuilder.Add(effectiveParameterRefKind);
			}
		}
		ImmutableArray<RefKind> refKinds = arrayBuilder?.ToImmutableAndFree() ?? default(ImmutableArray<RefKind>);
		return new EffectiveParameters(instance.ToImmutableAndFree(), refKinds);
	}

	private RefKind GetEffectiveParameterRefKind(ParameterSymbol parameter, RefKind argRefKind, bool isMethodGroupConversion, bool allowRefOmittedArguments, ref bool hasAnyRefOmittedArgument)
	{
		RefKind refKind = parameter.RefKind;
		if (!isMethodGroupConversion && argRefKind == RefKind.None && refKind == RefKind.In)
		{
			return RefKind.None;
		}
		if (allowRefOmittedArguments && refKind == RefKind.Ref && argRefKind == RefKind.None && !_binder.InAttributeArgument)
		{
			hasAnyRefOmittedArgument = true;
			return RefKind.None;
		}
		return refKind;
	}

	private EffectiveParameters GetEffectiveParametersInExpandedForm<TMember>(TMember member, int argumentCount, ImmutableArray<int> argToParamMap, ArrayBuilder<RefKind> argumentRefKinds, bool isMethodGroupConversion, bool allowRefOmittedArguments) where TMember : Symbol
	{
		bool hasAnyRefOmittedArgument;
		return GetEffectiveParametersInExpandedForm(member, argumentCount, argToParamMap, argumentRefKinds, isMethodGroupConversion, allowRefOmittedArguments, out hasAnyRefOmittedArgument);
	}

	private EffectiveParameters GetEffectiveParametersInExpandedForm<TMember>(TMember member, int argumentCount, ImmutableArray<int> argToParamMap, ArrayBuilder<RefKind> argumentRefKinds, bool isMethodGroupConversion, bool allowRefOmittedArguments, out bool hasAnyRefOmittedArgument) where TMember : Symbol
	{
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		ArrayBuilder<RefKind> instance2 = ArrayBuilder<RefKind>.GetInstance();
		bool flag = false;
		ImmutableArray<ParameterSymbol> parameters = member.GetParameters();
		TypeSymbol elementType = ((ArrayTypeSymbol)parameters[parameters.Length - 1].Type).ElementType;
		bool flag2 = argumentRefKinds.Any();
		hasAnyRefOmittedArgument = false;
		for (int i = 0; i < argumentCount; i++)
		{
			int num = (argToParamMap.IsDefault ? i : argToParamMap[i]);
			ParameterSymbol parameterSymbol = parameters[num];
			instance.Add((num == parameters.Length - 1) ? elementType : parameterSymbol.Type);
			RefKind argRefKind = (flag2 ? argumentRefKinds[i] : RefKind.None);
			RefKind effectiveParameterRefKind = GetEffectiveParameterRefKind(parameterSymbol, argRefKind, isMethodGroupConversion, allowRefOmittedArguments, ref hasAnyRefOmittedArgument);
			instance2.Add(effectiveParameterRefKind);
			if (effectiveParameterRefKind != RefKind.None)
			{
				flag = true;
			}
		}
		ImmutableArray<RefKind> refKinds = (flag ? instance2.ToImmutable() : default(ImmutableArray<RefKind>));
		instance2.Free();
		return new EffectiveParameters(instance.ToImmutableAndFree(), refKinds);
	}

	private MemberResolutionResult<TMember> IsMemberApplicableInNormalForm<TMember>(TMember member, TMember leastOverriddenMember, ArrayBuilder<TypeSymbol> typeArguments, AnalyzedArguments arguments, bool isMethodGroupConversion, bool allowRefOmittedArguments, bool inferWithDynamic, bool completeResults, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		ArgumentAnalysisResult argAnalysis = AnalyzeArguments(member, arguments, isMethodGroupConversion, expanded: false);
		if (!argAnalysis.IsValid)
		{
			ArgumentAnalysisResultKind kind = argAnalysis.Kind;
			if ((kind != ArgumentAnalysisResultKind.NoCorrespondingParameter && kind - 4 > ArgumentAnalysisResultKind.Expanded) || !completeResults)
			{
				return new MemberResolutionResult<TMember>(member, leastOverriddenMember, MemberAnalysisResult.ArgumentParameterMismatch(argAnalysis));
			}
		}
		if (member.HasUseSiteError)
		{
			return new MemberResolutionResult<TMember>(member, leastOverriddenMember, MemberAnalysisResult.UseSiteError());
		}
		EffectiveParameters effectiveParametersInNormalForm = GetEffectiveParametersInNormalForm(GetConstructedFrom(leastOverriddenMember), arguments.Arguments.Count, argAnalysis.ArgsToParamsOpt, arguments.RefKinds, isMethodGroupConversion, allowRefOmittedArguments, out var hasAnyRefOmittedArgument);
		EffectiveParameters effectiveParametersInNormalForm2 = GetEffectiveParametersInNormalForm(leastOverriddenMember, arguments.Arguments.Count, argAnalysis.ArgsToParamsOpt, arguments.RefKinds, isMethodGroupConversion, allowRefOmittedArguments);
		MemberResolutionResult<TMember> result = IsApplicable(member, leastOverriddenMember, typeArguments, arguments, effectiveParametersInNormalForm, effectiveParametersInNormalForm2, argAnalysis.ArgsToParamsOpt, hasAnyRefOmittedArgument, inferWithDynamic, completeResults, ref useSiteDiagnostics);
		if (completeResults && !argAnalysis.IsValid)
		{
			return new MemberResolutionResult<TMember>(member, leastOverriddenMember, MemberAnalysisResult.ArgumentParameterMismatch(argAnalysis));
		}
		return result;
	}

	private MemberResolutionResult<TMember> IsMemberApplicableInExpandedForm<TMember>(TMember member, TMember leastOverriddenMember, ArrayBuilder<TypeSymbol> typeArguments, AnalyzedArguments arguments, bool allowRefOmittedArguments, bool completeResults, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		ArgumentAnalysisResult argAnalysis = AnalyzeArguments(member, arguments, isMethodGroupConversion: false, expanded: true);
		if (!argAnalysis.IsValid)
		{
			return new MemberResolutionResult<TMember>(member, leastOverriddenMember, MemberAnalysisResult.ArgumentParameterMismatch(argAnalysis));
		}
		if (member.HasUseSiteError)
		{
			return new MemberResolutionResult<TMember>(member, leastOverriddenMember, MemberAnalysisResult.UseSiteError());
		}
		EffectiveParameters effectiveParametersInExpandedForm = GetEffectiveParametersInExpandedForm(GetConstructedFrom(leastOverriddenMember), arguments.Arguments.Count, argAnalysis.ArgsToParamsOpt, arguments.RefKinds, isMethodGroupConversion: false, allowRefOmittedArguments, out var hasAnyRefOmittedArgument);
		EffectiveParameters effectiveParametersInExpandedForm2 = GetEffectiveParametersInExpandedForm(leastOverriddenMember, arguments.Arguments.Count, argAnalysis.ArgsToParamsOpt, arguments.RefKinds, isMethodGroupConversion: false, allowRefOmittedArguments);
		MemberResolutionResult<TMember> result = IsApplicable(member, leastOverriddenMember, typeArguments, arguments, effectiveParametersInExpandedForm, effectiveParametersInExpandedForm2, argAnalysis.ArgsToParamsOpt, hasAnyRefOmittedArgument, inferWithDynamic: false, completeResults, ref useSiteDiagnostics);
		if (!result.Result.IsValid)
		{
			return result;
		}
		return new MemberResolutionResult<TMember>(result.Member, result.LeastOverriddenMember, MemberAnalysisResult.ExpandedForm(result.Result.ArgsToParamsOpt, result.Result.ConversionsOpt, hasAnyRefOmittedArgument));
	}

	private MemberResolutionResult<TMember> IsApplicable<TMember>(TMember member, TMember leastOverriddenMember, ArrayBuilder<TypeSymbol> typeArgumentsBuilder, AnalyzedArguments arguments, EffectiveParameters originalEffectiveParameters, EffectiveParameters constructedEffectiveParameters, ImmutableArray<int> argsToParamsMap, bool hasAnyRefOmittedArgument, bool inferWithDynamic, bool completeResults, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		MethodSymbol methodSymbol;
		bool ignoreOpenTypes;
		EffectiveParameters parameters;
		if (member.Kind == SymbolKind.Method && (methodSymbol = (MethodSymbol)(object)member).Arity > 0)
		{
			if (typeArgumentsBuilder.Count == 0 && arguments.HasDynamicArgument && !inferWithDynamic)
			{
				ignoreOpenTypes = true;
				parameters = constructedEffectiveParameters;
			}
			else
			{
				MethodSymbol methodSymbol2 = (MethodSymbol)(object)leastOverriddenMember;
				ImmutableArray<TypeSymbol> immutableArray;
				if (typeArgumentsBuilder.Count > 0)
				{
					immutableArray = typeArgumentsBuilder.ToImmutable();
				}
				else
				{
					immutableArray = InferMethodTypeArguments(methodSymbol, methodSymbol2.ConstructedFrom.TypeParameters, arguments, originalEffectiveParameters, out var error, ref useSiteDiagnostics);
					if (immutableArray.IsDefault)
					{
						return new MemberResolutionResult<TMember>(member, leastOverriddenMember, error);
					}
				}
				member = (TMember)(Symbol)methodSymbol.Construct(immutableArray);
				leastOverriddenMember = (TMember)(Symbol)methodSymbol2.ConstructedFrom.Construct(immutableArray);
				ImmutableArray<TypeSymbol> parameterTypes = leastOverriddenMember.GetParameterTypes();
				for (int i = 0; i < parameterTypes.Length; i++)
				{
					if (!parameterTypes[i].CheckAllConstraints(Conversions))
					{
						return new MemberResolutionResult<TMember>(member, leastOverriddenMember, MemberAnalysisResult.ConstructedParameterFailedConstraintsCheck(i));
					}
				}
				TypeMap typeMap = new TypeMap(methodSymbol.TypeParameters, immutableArray.SelectAsArray(TypeMap.TypeSymbolAsTypeWithModifiers), allowAlpha: true);
				parameters = new EffectiveParameters(typeMap.SubstituteTypesWithoutModifiers(constructedEffectiveParameters.ParameterTypes), constructedEffectiveParameters.ParameterRefKinds);
				ignoreOpenTypes = false;
			}
		}
		else
		{
			parameters = constructedEffectiveParameters;
			ignoreOpenTypes = false;
		}
		MemberAnalysisResult result = IsApplicable(member, parameters, arguments, argsToParamsMap, member.GetIsVararg(), hasAnyRefOmittedArgument, ignoreOpenTypes, completeResults, ref useSiteDiagnostics);
		return new MemberResolutionResult<TMember>(member, leastOverriddenMember, result);
	}

	private ImmutableArray<TypeSymbol> InferMethodTypeArguments(MethodSymbol method, ImmutableArray<TypeParameterSymbol> originalTypeParameters, AnalyzedArguments arguments, EffectiveParameters originalEffectiveParameters, out MemberAnalysisResult error, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ImmutableArray<BoundExpression> arguments2 = arguments.Arguments.ToImmutable();
		MethodTypeInferenceResult methodTypeInferenceResult = MethodTypeInferrer.Infer(_binder, originalTypeParameters, method.ContainingType, originalEffectiveParameters.ParameterTypes, originalEffectiveParameters.ParameterRefKinds, arguments2, ref useSiteDiagnostics);
		if (methodTypeInferenceResult.Success)
		{
			error = default;
			return methodTypeInferenceResult.InferredTypeArguments;
		}
		if (arguments.IsExtensionMethodInvocation && MethodTypeInferrer.InferTypeArgumentsFromFirstArgument(_binder.Conversions, method, arguments2, ref useSiteDiagnostics).IsDefault)
		{
			error = MemberAnalysisResult.TypeInferenceExtensionInstanceArgumentFailed();
			return default;
		}
		error = MemberAnalysisResult.TypeInferenceFailed();
		return default;
	}

	private MemberAnalysisResult IsApplicable(Symbol candidate, EffectiveParameters parameters, AnalyzedArguments arguments, ImmutableArray<int> argsToParameters, bool isVararg, bool hasAnyRefOmittedArgument, bool ignoreOpenTypes, bool completeResults, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		int num = parameters.ParameterTypes.Length + (isVararg ? 1 : 0);
		if (arguments.Arguments.Count < num)
		{
			num = arguments.Arguments.Count;
		}
		ArrayBuilder<Conversion> arrayBuilder = null;
		ArrayBuilder<int> arrayBuilder2 = null;
		for (int i = 0; i < num; i++)
		{
			BoundExpression boundExpression = arguments.Argument(i);
			Conversion conversion;
			if (isVararg && i == num - 1)
			{
				if (boundExpression.Kind == BoundKind.ArgListOperator)
				{
					conversion = Conversion.Identity;
				}
				else
				{
					arrayBuilder2 = arrayBuilder2 ?? ArrayBuilder<int>.GetInstance();
					arrayBuilder2.Add(i);
					conversion = Conversion.NoConversion;
				}
			}
			else
			{
				RefKind argRefKind = arguments.RefKind(i);
				RefKind refKind = ((!parameters.ParameterRefKinds.IsDefault) ? parameters.ParameterRefKinds[i] : RefKind.None);
				bool flag = arguments.IsExtensionMethodThisArgument(i);
				if (flag && refKind == RefKind.Ref)
				{
					argRefKind = refKind;
				}
				conversion = CheckArgumentForApplicability(candidate, boundExpression, argRefKind, parameters.ParameterTypes[i], refKind, ignoreOpenTypes, ref useSiteDiagnostics, flag);
				if (flag && !ConversionsBase.IsValidExtensionMethodThisArgConversion(conversion))
				{
					return MemberAnalysisResult.BadArgumentConversions(argsToParameters, ImmutableArray.Create(i), ImmutableArray.Create(conversion));
				}
				if (!conversion.Exists)
				{
					arrayBuilder2 = arrayBuilder2 ?? ArrayBuilder<int>.GetInstance();
					arrayBuilder2.Add(i);
				}
			}
			if (arrayBuilder != null)
			{
				arrayBuilder.Add(conversion);
			}
			else if (!conversion.IsIdentity)
			{
				arrayBuilder = ArrayBuilder<Conversion>.GetInstance(num);
				arrayBuilder.AddMany(Conversion.Identity, i);
				arrayBuilder.Add(conversion);
			}
			if (arrayBuilder2 != null && !completeResults)
			{
				break;
			}
		}
		ImmutableArray<Conversion> conversions = arrayBuilder?.ToImmutableAndFree() ?? default(ImmutableArray<Conversion>);
		if (arrayBuilder2 != null)
		{
			return MemberAnalysisResult.BadArgumentConversions(argsToParameters, arrayBuilder2.ToImmutableAndFree(), conversions);
		}
		return MemberAnalysisResult.NormalForm(argsToParameters, conversions, hasAnyRefOmittedArgument);
	}

	private Conversion CheckArgumentForApplicability(Symbol candidate, BoundExpression argument, RefKind argRefKind, TypeSymbol parameterType, RefKind parRefKind, bool ignoreOpenTypes, ref HashSet<DiagnosticInfo> useSiteDiagnostics, bool forExtensionMethodThisArg)
	{
		if (argRefKind != parRefKind && (argRefKind != RefKind.None || !argument.HasDynamicType()))
		{
			return Conversion.NoConversion;
		}
		if (ignoreOpenTypes && parameterType.ContainsTypeParameter((MethodSymbol)candidate))
		{
			return Conversion.ImplicitDynamic;
		}
		TypeSymbol type = argument.Type;
		if (argument.Kind == BoundKind.OutVariablePendingInference || argument.Kind == BoundKind.OutDeconstructVarPendingInference || (argument.Kind == BoundKind.DiscardExpression && (object)type == null))
		{
			return Conversion.Identity;
		}
		if (argRefKind == RefKind.None)
		{
			if (!forExtensionMethodThisArg)
			{
				return Conversions.ClassifyImplicitConversionFromExpression(argument, parameterType, ref useSiteDiagnostics);
			}
			return Conversions.ClassifyImplicitExtensionMethodThisArgConversion(argument, argument.Type, parameterType, ref useSiteDiagnostics);
		}
		if ((object)type != null && ConversionsBase.HasIdentityConversion(type, parameterType))
		{
			return Conversion.Identity;
		}
		return Conversion.NoConversion;
	}

	private static TMember GetConstructedFrom<TMember>(TMember member) where TMember : Symbol
	{
		return member.Kind switch
		{
			SymbolKind.Property => member, 
			SymbolKind.Method => (TMember)(Symbol)(member as MethodSymbol).ConstructedFrom, 
			_ => throw ExceptionUtilities.UnexpectedValue(member.Kind), 
		};
	}

	private static ArgumentAnalysisResult AnalyzeArguments(Symbol symbol, AnalyzedArguments arguments, bool isMethodGroupConversion, bool expanded)
	{
		ImmutableArray<ParameterSymbol> parameters = symbol.GetParameters();
		bool isVararg = symbol.GetIsVararg();
		if (!expanded && arguments.Names.Count == 0)
		{
			return AnalyzeArgumentsForNormalFormNoNamedArguments(parameters, arguments, isMethodGroupConversion, isVararg);
		}
		int count = arguments.Arguments.Count;
		int[] array = null;
		int? num = null;
		bool? flag = null;
		bool seenNamedParams = false;
		bool seenOutOfPositionNamedArgument = false;
		bool isValidParams = IsValidParams(symbol);
		for (int i = 0; i < count; i++)
		{
			int num2 = CorrespondsToAnyParameter(parameters, expanded, arguments, i, isValidParams, isVararg, out var isNamedArgument, ref seenNamedParams, ref seenOutOfPositionNamedArgument) ?? (-1);
			if (num2 == -1 && !num.HasValue)
			{
				num = i;
				flag = isNamedArgument;
			}
			if (num2 != i && array == null)
			{
				array = new int[count];
				for (int j = 0; j < i; j++)
				{
					array[j] = j;
				}
			}
			if (array != null)
			{
				array[i] = num2;
			}
		}
		ParameterMap argsToParameters = new ParameterMap(array, count);
		int? num3 = CheckForBadNonTrailingNamedArgument(arguments, argsToParameters, parameters);
		if (num3.HasValue)
		{
			return ArgumentAnalysisResult.BadNonTrailingNamedArgument(num3.Value);
		}
		if (num.HasValue)
		{
			if (flag.Value)
			{
				return ArgumentAnalysisResult.NoCorrespondingNamedParameter(num.Value);
			}
			return ArgumentAnalysisResult.NoCorrespondingParameter(num.Value);
		}
		int? num4 = NameUsedForPositional(arguments, argsToParameters);
		if (num4.HasValue)
		{
			return ArgumentAnalysisResult.NameUsedForPositional(num4.Value);
		}
		int? num5 = CheckForMissingRequiredParameter(argsToParameters, parameters, isMethodGroupConversion, expanded);
		if (num5.HasValue)
		{
			return ArgumentAnalysisResult.RequiredParameterMissing(num5.Value);
		}
		if ((arguments.Names.Any() && arguments.Names.Last() != null) & isVararg)
		{
			return ArgumentAnalysisResult.RequiredParameterMissing(parameters.Length);
		}
		int? num6 = CheckForDuplicateNamedArgument(arguments);
		if (num6.HasValue)
		{
			return ArgumentAnalysisResult.DuplicateNamedArgument(num6.Value);
		}
		if (!expanded)
		{
			return ArgumentAnalysisResult.NormalForm(argsToParameters.ToImmutableArray());
		}
		return ArgumentAnalysisResult.ExpandedForm(argsToParameters.ToImmutableArray());
	}

	private static int? CheckForBadNonTrailingNamedArgument(AnalyzedArguments arguments, ParameterMap argsToParameters, ImmutableArray<ParameterSymbol> parameters)
	{
		if (argsToParameters.IsTrivial)
		{
			return null;
		}
		int num = -1;
		int count = arguments.Arguments.Count;
		for (int i = 0; i < count; i++)
		{
			int num2 = argsToParameters[i];
			if (num2 != -1 && num2 != i && arguments.Name(i) != null)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			for (int j = num + 1; j < count; j++)
			{
				if (arguments.Name(j) == null)
				{
					return num;
				}
			}
		}
		return null;
	}

	private static int? CorrespondsToAnyParameter(ImmutableArray<ParameterSymbol> memberParameters, bool expanded, AnalyzedArguments arguments, int argumentPosition, bool isValidParams, bool isVararg, out bool isNamedArgument, ref bool seenNamedParams, ref bool seenOutOfPositionNamedArgument)
	{
		isNamedArgument = arguments.Names.Count > argumentPosition && arguments.Names[argumentPosition] != null;
		if (!isNamedArgument)
		{
			if (seenNamedParams)
			{
				return null;
			}
			if (seenOutOfPositionNamedArgument)
			{
				return null;
			}
			int num = memberParameters.Length + (isVararg ? 1 : 0);
			if (argumentPosition >= num)
			{
				if (!expanded)
				{
					return null;
				}
				return num - 1;
			}
			return argumentPosition;
		}
		IdentifierNameSyntax identifierNameSyntax = arguments.Names[argumentPosition];
		for (int i = 0; i < memberParameters.Length; i++)
		{
			if (memberParameters[i].Name == identifierNameSyntax.Identifier.ValueText)
			{
				if (isValidParams && i == memberParameters.Length - 1)
				{
					seenNamedParams = true;
				}
				if (i != argumentPosition)
				{
					seenOutOfPositionNamedArgument = true;
				}
				return i;
			}
		}
		return null;
	}

	private static ArgumentAnalysisResult AnalyzeArgumentsForNormalFormNoNamedArguments(ImmutableArray<ParameterSymbol> parameters, AnalyzedArguments arguments, bool isMethodGroupConversion, bool isVararg)
	{
		int num = parameters.Length + (isVararg ? 1 : 0);
		int count = arguments.Arguments.Count;
		if (count < num)
		{
			for (int i = count; i < num; i++)
			{
				if (parameters.Length == i || !CanBeOptional(parameters[i], isMethodGroupConversion))
				{
					return ArgumentAnalysisResult.RequiredParameterMissing(i);
				}
			}
		}
		else if (num < count)
		{
			return ArgumentAnalysisResult.NoCorrespondingParameter(num);
		}
		return ArgumentAnalysisResult.NormalForm(default);
	}

	private static bool CanBeOptional(ParameterSymbol parameter, bool isMethodGroupConversion)
	{
		if (!isMethodGroupConversion)
		{
			return parameter.IsOptional;
		}
		return false;
	}

	private static int? NameUsedForPositional(AnalyzedArguments arguments, ParameterMap argsToParameters)
	{
		if (argsToParameters.IsTrivial)
		{
			return null;
		}
		for (int i = 0; i < argsToParameters.Length; i++)
		{
			if (arguments.Name(i) == null)
			{
				continue;
			}
			for (int j = 0; j < i; j++)
			{
				if (arguments.Name(j) == null && argsToParameters[i] == argsToParameters[j])
				{
					return i;
				}
			}
		}
		return null;
	}

	private static int? CheckForMissingRequiredParameter(ParameterMap argsToParameters, ImmutableArray<ParameterSymbol> parameters, bool isMethodGroupConversion, bool expanded)
	{
		int num = (expanded ? (parameters.Length - 1) : parameters.Length);
		if (argsToParameters.IsTrivial && num <= argsToParameters.Length)
		{
			return null;
		}
		for (int i = 0; i < num; i++)
		{
			if (CanBeOptional(parameters[i], isMethodGroupConversion))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < argsToParameters.Length; j++)
			{
				flag = argsToParameters[j] == i;
				if (flag)
				{
					break;
				}
			}
			if (!flag)
			{
				return i;
			}
		}
		return null;
	}

	private static int? CheckForDuplicateNamedArgument(AnalyzedArguments arguments)
	{
		if (arguments.Names.IsEmpty())
		{
			return null;
		}
		PooledHashSet<string> instance = PooledHashSet<string>.GetInstance();
		for (int i = 0; i < arguments.Names.Count; i++)
		{
			string text = arguments.Name(i);
			if (text != null && !instance.Add(text))
			{
				instance.Free();
				return i;
			}
		}
		instance.Free();
		return null;
	}
}
