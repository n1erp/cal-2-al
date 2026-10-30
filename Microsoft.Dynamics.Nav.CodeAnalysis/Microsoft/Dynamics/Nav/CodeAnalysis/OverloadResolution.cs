using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class OverloadResolution
{
	internal static class BinopEasyOut
	{
		private const BinaryOperatorKind ERR = BinaryOperatorKind.Error;

		private const BinaryOperatorKind STR = BinaryOperatorKind.String;

		private const BinaryOperatorKind CHR = BinaryOperatorKind.Char;

		private const BinaryOperatorKind INT = BinaryOperatorKind.Int;

		private const BinaryOperatorKind LNG = BinaryOperatorKind.Long;

		private const BinaryOperatorKind DEC = BinaryOperatorKind.Decimal;

		private const BinaryOperatorKind BOL = BinaryOperatorKind.Bool;

		private static readonly BinaryOperatorKind[,] Arithmetic = new BinaryOperatorKind[15, 15]
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
				BinaryOperatorKind.Char,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Char,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
			}
		};

		private static readonly BinaryOperatorKind[,] Addition = new BinaryOperatorKind[15, 15]
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
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.String,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
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
				BinaryOperatorKind.Char,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Char,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
			}
		};

		private static readonly BinaryOperatorKind[,] Equality = new BinaryOperatorKind[15, 15]
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
				BinaryOperatorKind.Error
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.String,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
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
				BinaryOperatorKind.Char,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Char,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Int,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal
			},
			{
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Error,
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
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
				BinaryOperatorKind.Decimal
			}
		};

		private static readonly BinaryOperatorKind[,] Logical = new BinaryOperatorKind[15, 15]
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
				BinaryOperatorKind.Long,
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
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
				BinaryOperatorKind.Long,
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
				BinaryOperatorKind.Error
			}
		};

		private static readonly BinaryOperatorKind[][,] Opkind = new BinaryOperatorKind[16][,]
		{
			Arithmetic, Addition, Arithmetic, Arithmetic, Arithmetic, Equality, Equality, Equality, Equality, Equality,
			Equality, Logical, Logical, Logical, Arithmetic, Arithmetic
		};

		private static int? TypeToIndex(TypeSymbol type)
		{
			return type.GetNavTypeKindSafe() switch
			{
				NavTypeKind.String => (int?)1, 
				NavTypeKind.Boolean => 2, 
				NavTypeKind.Char => 3, 
				NavTypeKind.Integer => 6, 
				NavTypeKind.BigInteger => 7, 
				NavTypeKind.Byte => 8, 
				NavTypeKind.Decimal => 14, 
				_ => null, 
			};
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
			if (kind.IsLogical())
			{
				int? num3 = num;
				int num4 = 29;
				if (num3.GetValueOrDefault() != num4 || !num3.HasValue || num2 != 29)
				{
					goto IL_0088;
				}
			}
			binaryOperatorKind = Opkind[kind.OperatorIndex()][num.Value, num2.Value];
			goto IL_0088;
			IL_0088:
			if (binaryOperatorKind != BinaryOperatorKind.Error)
			{
				return binaryOperatorKind | kind;
			}
			return binaryOperatorKind;
		}
	}

	internal static class UnopEasyOut
	{
		private const UnaryOperatorKind ERR = UnaryOperatorKind.Error;

		private const UnaryOperatorKind BOL = UnaryOperatorKind.Bool;

		private const UnaryOperatorKind U08 = UnaryOperatorKind.Byte;

		private const UnaryOperatorKind CHR = UnaryOperatorKind.Char;

		private const UnaryOperatorKind I32 = UnaryOperatorKind.Int;

		private const UnaryOperatorKind I64 = UnaryOperatorKind.Long;

		private const UnaryOperatorKind DEC = UnaryOperatorKind.Decimal;

		private static readonly UnaryOperatorKind[] plus = new UnaryOperatorKind[6]
		{
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Long,
			UnaryOperatorKind.Decimal
		};

		private static readonly UnaryOperatorKind[] minus = new UnaryOperatorKind[6]
		{
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Int,
			UnaryOperatorKind.Long,
			UnaryOperatorKind.Decimal
		};

		private static readonly UnaryOperatorKind[] logicalNegation = new UnaryOperatorKind[6]
		{
			UnaryOperatorKind.Bool,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Error,
			UnaryOperatorKind.Error
		};

		private static readonly UnaryOperatorKind[][] opkind = new UnaryOperatorKind[3][] { plus, minus, logicalNegation };

		private static int? TypeToIndex(TypeSymbol type)
		{
			return type.GetNavTypeKindSafe() switch
			{
				NavTypeKind.Boolean => (int?)0, 
				NavTypeKind.Byte => 1, 
				NavTypeKind.Char => 2, 
				NavTypeKind.Integer => 3, 
				NavTypeKind.BigInteger => 4, 
				NavTypeKind.Decimal => 5, 
				_ => null, 
			};
		}

		public static UnaryOperatorKind OpKind(UnaryOperatorKind kind, TypeSymbol operand)
		{
			int? num = TypeToIndex(operand);
			if (!num.HasValue)
			{
				return UnaryOperatorKind.Error;
			}
			int num2 = kind.OperatorIndex();
			UnaryOperatorKind unaryOperatorKind = ((num2 < opkind.Length) ? opkind[num2][num.Value] : UnaryOperatorKind.Error);
			if (unaryOperatorKind != UnaryOperatorKind.Error)
			{
				return unaryOperatorKind | kind;
			}
			return unaryOperatorKind;
		}
	}

	private readonly Binder binder;

	private Compilation Compilation => binder.Compilation;

	private Conversions Conversions => binder.Conversions;

	private static void BinaryOperatorEasyOut(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, BinaryOperatorOverloadResolutionResult result)
	{
		TypeSymbol typeSymbol = left.Type;
		if ((object)typeSymbol == null)
		{
			return;
		}
		TypeSymbol typeSymbol2 = right.Type;
		if ((object)typeSymbol2 != null && !PossiblyUnusualConstantOperation(left, right))
		{
			if (AreCharLiterals(left, right))
			{
				typeSymbol = NavCorLib.StringType;
				typeSymbol2 = NavCorLib.StringType;
			}
			BinaryOperatorKind binaryOperatorKind = BinopEasyOut.OpKind(kind, typeSymbol, typeSymbol2);
			if (binaryOperatorKind != BinaryOperatorKind.Error)
			{
				BinaryOperatorSignature signature = BuiltInOperators.GetSignature(binaryOperatorKind);
				Conversion leftConversion = Conversions.FastClassifyConversion(left.Type, signature.LeftType);
				Conversion rightConversion = Conversions.FastClassifyConversion(right.Type, signature.RightType);
				DebugAssertHelper.Assert(leftConversion.Exists && leftConversion.IsImplicit);
				DebugAssertHelper.Assert(rightConversion.Exists && rightConversion.IsImplicit);
				result.Results.Add(BinaryOperatorAnalysisResult.Applicable(signature, leftConversion, rightConversion));
			}
		}
	}

	private static bool AreCharLiterals(BoundExpression left, BoundExpression right)
	{
		if (left.Kind == BoundKind.Literal && right.Kind == BoundKind.Literal && left.Type.NavTypeKind == NavTypeKind.Char)
		{
			return right.Type.NavTypeKind == NavTypeKind.Char;
		}
		return false;
	}

	private static bool PossiblyUnusualConstantOperation(BoundExpression left, BoundExpression right)
	{
		DebugAssertHelper.Assert(left != null);
		DebugAssertHelper.Assert((object)left.Type != null);
		DebugAssertHelper.Assert(right != null);
		DebugAssertHelper.Assert((object)right.Type != null);
		if (left.ConstantValue == null && right.ConstantValue == null)
		{
			return false;
		}
		if (left.Type.NavTypeKind != right.Type.NavTypeKind)
		{
			return true;
		}
		if (left.Type.NavTypeKind == NavTypeKind.Integer || left.Type.NavTypeKind == NavTypeKind.Boolean || left.Type.NavTypeKind == NavTypeKind.String || left.Type.NavTypeKind == NavTypeKind.Char)
		{
			return false;
		}
		return true;
	}

	public void BinaryOperatorOverloadResolution(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, BinaryOperatorOverloadResolutionResult result, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert(left != null);
		DebugAssertHelper.Assert(right != null);
		DebugAssertHelper.Assert(result.Results.Count == 0);
		BinaryOperatorEasyOut(kind & ~BinaryOperatorKind.Logical, left, right, result);
		if (result.Results.Count <= 0)
		{
			result.Results.Clear();
			GetAllBuiltInOperators(kind, left, right, result.Results, ref useSiteDiagnostics);
			BinaryOperatorOverloadResolution(left, right, result, ref useSiteDiagnostics);
		}
	}

	private void GetAllBuiltInOperators(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorAnalysisResult> results, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		kind = kind.OperatorWithLogical();
		ArrayBuilder<BinaryOperatorSignature> instance = ArrayBuilder<BinaryOperatorSignature>.GetInstance();
		Compilation.BuiltInOperators.GetSimpleBuiltInOperators(kind, instance);
		GetEnumOperators(kind, left, right, instance);
		CandidateOperators(instance, left, right, results, ref useSiteDiagnostics);
		instance.Free();
	}

	private static void GetEnumOperators(BinaryOperatorKind kind, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorSignature> operators)
	{
		DebugAssertHelper.Assert(left != null);
		DebugAssertHelper.Assert(right != null);
		if (left.Type.NavTypeKind == right.Type.NavTypeKind && ((left.Type.NavTypeKind.IsClrOption() && right.Type.NavTypeKind.IsClrOption()) || (left.Type.NavTypeKind == NavTypeKind.Enum && right.Type.NavTypeKind == NavTypeKind.Enum)) && (kind == BinaryOperatorKind.Equal || kind == BinaryOperatorKind.NotEqual))
		{
			operators.Add(new BinaryOperatorSignature(kind, left.Type, right.Type, Compilation.GetNavTypeSymbol(NavTypeKind.Boolean)));
		}
	}

	private bool CandidateOperators(ArrayBuilder<BinaryOperatorSignature> operators, BoundExpression left, BoundExpression right, ArrayBuilder<BinaryOperatorAnalysisResult> results, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		bool result = false;
		foreach (BinaryOperatorSignature @operator in operators)
		{
			Conversion leftConversion = Conversions.ClassifyConversionFromExpression(left, @operator.LeftType, ref useSiteDiagnostics);
			Conversion rightConversion = Conversions.ClassifyConversionFromExpression(right, @operator.RightType, ref useSiteDiagnostics);
			if (leftConversion.IsPureImplicit && rightConversion.IsPureImplicit)
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

	private static void BinaryOperatorOverloadResolution(BoundExpression left, BoundExpression right, BinaryOperatorOverloadResolutionResult result, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (result.GetValidCount() == 1)
		{
			return;
		}
		ArrayBuilder<BinaryOperatorAnalysisResult> results = result.Results;
		for (int i = 0; i < results.Count; i++)
		{
			if (results[i].Kind != OperatorAnalysisResultKind.Applicable)
			{
				continue;
			}
			for (int j = 0; j < results.Count; j++)
			{
				if (i != j && results[j].Kind != OperatorAnalysisResultKind.Inapplicable)
				{
					switch (BetterOperator(results[i], results[j], left, right, ref useSiteDiagnostics))
					{
					case BetterResult.Left:
						results[j] = results[j].Worse();
						break;
					case BetterResult.Right:
						results[i] = results[i].Worse();
						break;
					}
				}
			}
		}
	}

	private static BetterResult BetterOperator(BinaryOperatorAnalysisResult result1, BinaryOperatorAnalysisResult result2, BoundExpression left, BoundExpression right, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		BinaryOperatorSignature signature = result1.Signature;
		BinaryOperatorSignature signature2 = result2.Signature;
		DebugAssertHelper.Assert(signature.Priority.HasValue == signature2.Priority.HasValue || result1.ConversionRank != result2.ConversionRank);
		if (signature.Priority.HasValue && signature2.Priority.HasValue && signature.Priority.GetValueOrDefault() != signature2.Priority.GetValueOrDefault())
		{
			if (signature.Priority.GetValueOrDefault() >= signature2.Priority.GetValueOrDefault())
			{
				return BetterResult.Right;
			}
			return BetterResult.Left;
		}
		BetterResult betterResult = BetterConversionFromExpression(left, signature.LeftType, signature2.LeftType, ref useSiteDiagnostics);
		BetterResult betterResult2 = BetterConversionFromExpression(right, signature.RightType, signature2.RightType, ref useSiteDiagnostics);
		if ((betterResult == BetterResult.Left && betterResult2 != BetterResult.Right) || (betterResult != BetterResult.Right && betterResult2 == BetterResult.Left))
		{
			return BetterResult.Left;
		}
		if ((betterResult == BetterResult.Right && betterResult2 != BetterResult.Left) || (betterResult != BetterResult.Left && betterResult2 == BetterResult.Right))
		{
			return BetterResult.Right;
		}
		if (Conversions.HasIdentityConversion(signature.LeftType, signature2.LeftType) && Conversions.HasIdentityConversion(signature.RightType, signature2.RightType))
		{
			BetterResult betterResult3 = MoreSpecificOperator(signature, signature2, ref useSiteDiagnostics);
			if (betterResult3 == BetterResult.Left || betterResult3 == BetterResult.Right)
			{
				return betterResult3;
			}
		}
		int num = result1.ConversionRank - result2.ConversionRank;
		if (num < 0)
		{
			return BetterResult.Right;
		}
		if (num > 0)
		{
			return BetterResult.Left;
		}
		return BetterResult.Neither;
	}

	private static BetterResult MoreSpecificOperator(BinaryOperatorSignature op1, BinaryOperatorSignature op2, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeSymbol item;
		TypeSymbol item2;
		if ((object)op1.Method != null)
		{
			ImmutableArray<ParameterSymbol> parameters = op1.Method.OriginalDefinition.GetParameters();
			item = parameters[0].Type;
			item2 = parameters[1].Type;
		}
		else
		{
			item = op1.LeftType;
			item2 = op1.RightType;
		}
		TypeSymbol item3;
		TypeSymbol item4;
		if ((object)op2.Method != null)
		{
			ImmutableArray<ParameterSymbol> parameters2 = op2.Method.OriginalDefinition.GetParameters();
			item3 = parameters2[0].Type;
			item4 = parameters2[1].Type;
		}
		else
		{
			item3 = op2.LeftType;
			item4 = op2.RightType;
		}
		ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
		ArrayBuilder<TypeSymbol> instance2 = ArrayBuilder<TypeSymbol>.GetInstance();
		instance.Add(item);
		instance.Add(item2);
		instance2.Add(item3);
		instance2.Add(item4);
		BetterResult result = MoreSpecificType(instance, instance2, ref useSiteDiagnostics);
		instance.Free();
		instance2.Free();
		return result;
	}

	private static void UnaryOperatorEasyOut(UnaryOperatorKind kind, BoundExpression operand, UnaryOperatorOverloadResolutionResult result)
	{
		TypeSymbol type = operand.Type;
		if ((object)type != null)
		{
			UnaryOperatorKind unaryOperatorKind = UnopEasyOut.OpKind(kind, type);
			if (unaryOperatorKind != UnaryOperatorKind.Error)
			{
				UnaryOperatorSignature signature = BuiltInOperators.GetSignature(unaryOperatorKind);
				Conversion? conversion = Conversions.FastClassifyConversion(type, signature.OperandType);
				DebugAssertHelper.Assert(conversion.HasValue && conversion.Value.IsImplicit);
				result.Results.Add(UnaryOperatorAnalysisResult.Applicable(signature, conversion.Value));
			}
		}
	}

	public void UnaryOperatorOverloadResolution(UnaryOperatorKind kind, BoundExpression operand, UnaryOperatorOverloadResolutionResult result, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert(operand != null);
		DebugAssertHelper.Assert(result.Results.Count == 0);
		UnaryOperatorEasyOut(kind, operand, result);
		if (result.Results.Count <= 0)
		{
			result.Results.Clear();
			GetAllBuiltInOperators(kind, operand, result.Results, ref useSiteDiagnostics);
			UnaryOperatorOverloadResolution(operand, result, ref useSiteDiagnostics);
		}
	}

	private static void UnaryOperatorOverloadResolution(BoundExpression operand, UnaryOperatorOverloadResolutionResult result, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (result.GetValidCount() == 1)
		{
			return;
		}
		ArrayBuilder<UnaryOperatorAnalysisResult> results = result.Results;
		for (int i = 0; i < results.Count; i++)
		{
			if (results[i].Kind != OperatorAnalysisResultKind.Applicable)
			{
				continue;
			}
			for (int j = 0; j < results.Count; j++)
			{
				if (i != j && results[j].Kind != OperatorAnalysisResultKind.Inapplicable)
				{
					switch (BetterOperator(results[i].Signature, results[j].Signature, operand, ref useSiteDiagnostics))
					{
					case BetterResult.Left:
						results[j] = results[j].Worse();
						break;
					case BetterResult.Right:
						results[i] = results[i].Worse();
						break;
					}
				}
			}
		}
	}

	private static BetterResult BetterOperator(UnaryOperatorSignature op1, UnaryOperatorSignature op2, BoundExpression operand, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		BetterResult betterResult = BetterConversionFromExpression(operand, op1.OperandType, op2.OperandType, ref useSiteDiagnostics);
		if (betterResult == BetterResult.Left || betterResult == BetterResult.Right)
		{
			return betterResult;
		}
		return BetterResult.Neither;
	}

	private void GetAllBuiltInOperators(UnaryOperatorKind kind, BoundExpression operand, ArrayBuilder<UnaryOperatorAnalysisResult> results, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		ArrayBuilder<UnaryOperatorSignature> instance = ArrayBuilder<UnaryOperatorSignature>.GetInstance();
		Compilation.BuiltInOperators.GetSimpleBuiltInOperators(kind, instance);
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

	public OverloadResolution(Binder binder)
	{
		this.binder = binder;
	}

	public void MethodInvocationOverloadResolution(ArrayBuilder<MethodSymbol> methods, AnalyzedArguments arguments, BoundExpression receiverOpt, OverloadResolutionResult<MethodSymbol> result, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		MethodOrPropertyOverloadResolution(methods, arguments, receiverOpt, result, ref useSiteDiagnostics);
	}

	internal void MethodOrPropertyOverloadResolution<TMember>(ArrayBuilder<TMember> members, AnalyzedArguments arguments, BoundExpression receiverOpt, OverloadResolutionResult<TMember> result, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		ArrayBuilder<MemberResolutionResult<TMember>> resultsBuilder = result.ResultsBuilder;
		PerformMemberOverloadResolution(resultsBuilder, members, arguments, receiverOpt, completeResults: false, ref useSiteDiagnostics);
		if (!OverloadResolutionResultIsValid(resultsBuilder))
		{
			result.Clear();
			PerformMemberOverloadResolution(resultsBuilder, members, arguments, receiverOpt, completeResults: true, ref useSiteDiagnostics);
		}
	}

	private void PerformMemberOverloadResolution<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, ArrayBuilder<TMember> members, AnalyzedArguments arguments, BoundExpression receiverOpt, bool completeResults, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		for (int i = 0; i < members.Count; i++)
		{
			AddMemberToCandidateSet(members[i], results, arguments, receiverOpt, completeResults, ref useSiteDiagnostics);
		}
		RemoveWorseMembers(results, arguments, ref useSiteDiagnostics);
	}

	private static void RemoveWorseMembers<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results, AnalyzedArguments arguments, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		ArrayBuilder<int> instance = ArrayBuilder<int>.GetInstance(results.Count, 0);
		for (int i = 0; i < results.Count; i++)
		{
			MemberResolutionResult<TMember> m = results[i];
			if (!m.Result.IsValid || instance[i] == 1)
			{
				continue;
			}
			bool flag = true;
			for (int j = 0; j < results.Count; j++)
			{
				MemberResolutionResult<TMember> m2 = results[j];
				if (!m2.Result.IsValid || m.Member == m2.Member)
				{
					continue;
				}
				BetterResult betterResult = BetterFunctionMember(m, m2, arguments.Arguments, ref useSiteDiagnostics);
				if (betterResult == BetterResult.Left)
				{
					instance[j] = 1;
					continue;
				}
				flag = false;
				if (betterResult == BetterResult.Right)
				{
					instance[i] = 1;
					break;
				}
			}
			if (instance[i] == 0)
			{
				instance[i] = (flag ? 3 : 2);
			}
		}
		bool flag2 = false;
		int num = 0;
		int num2 = -1;
		for (int k = 0; k < instance.Count; k++)
		{
			DebugAssertHelper.Assert(!results[k].Result.IsValid || instance[k] != 0);
			if (instance[k] == 3)
			{
				flag2 = true;
				break;
			}
			if (instance[k] == 2)
			{
				num++;
				num2 = k;
			}
		}
		DebugAssertHelper.Assert(num == 0 || !flag2);
		if (flag2 || num == 0)
		{
			for (int l = 0; l < instance.Count; l++)
			{
				DebugAssertHelper.Assert(!results[l].Result.IsValid || instance[l] != 0);
				if (instance[l] == 1 || instance[l] == 2)
				{
					results[l] = new MemberResolutionResult<TMember>(results[l].Member, MemberAnalysisResult.Worse());
				}
			}
		}
		else if (num == 1)
		{
			for (int n = 0; n < instance.Count; n++)
			{
				DebugAssertHelper.Assert(!results[n].Result.IsValid || instance[n] != 0);
				if (instance[n] == 1)
				{
					MemberAnalysisResult result = ((BetterFunctionMember(results[num2], results[n], arguments.Arguments, ref useSiteDiagnostics) == BetterResult.Left) ? MemberAnalysisResult.Worst() : MemberAnalysisResult.Worse());
					results[n] = new MemberResolutionResult<TMember>(results[n].Member, result);
				}
				else
				{
					DebugAssertHelper.Assert(instance[n] != 2 || n == num2);
				}
			}
			DebugAssertHelper.Assert(instance[num2] == 2);
			results[num2] = new MemberResolutionResult<TMember>(results[num2].Member, MemberAnalysisResult.Worse());
		}
		else
		{
			DebugAssertHelper.Assert(num > 1);
			for (int num3 = 0; num3 < instance.Count; num3++)
			{
				DebugAssertHelper.Assert(!results[num3].Result.IsValid || instance[num3] != 0);
				if (instance[num3] == 1)
				{
					results[num3] = new MemberResolutionResult<TMember>(results[num3].Member, MemberAnalysisResult.Worst());
				}
				else if (instance[num3] == 2)
				{
					results[num3] = new MemberResolutionResult<TMember>(results[num3].Member, MemberAnalysisResult.Worse());
				}
			}
		}
		instance.Free();
	}

	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "useSiteDiagnostics")]
	[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "arguments")]
	private static BetterResult BetterFunctionMember<TMember>(MemberResolutionResult<TMember> m1, MemberResolutionResult<TMember> m2, ArrayBuilder<BoundExpression> arguments, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		int num = CalcArgumentConversionRank(m1) - CalcArgumentConversionRank(m2);
		if (num < 0)
		{
			return BetterResult.Right;
		}
		if (num > 0)
		{
			return BetterResult.Left;
		}
		return BetterResult.Neither;
	}

	private static int CalcArgumentConversionRank<TMember>(MemberResolutionResult<TMember> m) where TMember : Symbol
	{
		if (m.Result.ConversionsOpt.IsDefault)
		{
			return m.Result.EffectiveParameters.Parameters.Length * 28;
		}
		int num = 0;
		for (int i = 0; i < m.Result.ConversionsOpt.Length; i++)
		{
			num += (int)m.Result.ConversionsOpt[i].Kind;
		}
		return num;
	}

	private static bool OverloadResolutionResultIsValid<TMember>(ArrayBuilder<MemberResolutionResult<TMember>> results) where TMember : Symbol
	{
		bool flag = false;
		foreach (MemberResolutionResult<TMember> result in results)
		{
			if (result.Result.IsValid)
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

	private MemberResolutionResult<TMember> IsApplicable<TMember>(TMember member, AnalyzedArguments arguments, EffectiveParameters effectiveParameters, BoundExpression receiverOpt, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		ArgumentAnalysisResult argAnalysis = AnalyzeArguments(member, arguments, effectiveParameters);
		if (!argAnalysis.IsValid)
		{
			return new MemberResolutionResult<TMember>(member, MemberAnalysisResult.ArgumentParameterMismatch(argAnalysis));
		}
		return new MemberResolutionResult<TMember>(member, IsApplicable(effectiveParameters, arguments, receiverOpt, ref useSiteDiagnostics));
	}

	private MemberAnalysisResult IsApplicable(EffectiveParameters parameters, AnalyzedArguments arguments, BoundExpression receiverOpt, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		int length = parameters.ParameterTypes.Length;
		DebugAssertHelper.Assert(length == arguments.Arguments.Count);
		TryRebindBadMemberArgumentInReceiverScope(parameters, arguments, receiverOpt);
		ArrayBuilder<Conversion> arrayBuilder = null;
		ArrayBuilder<int> arrayBuilder2 = null;
		ArrayBuilder<int> arrayBuilder3 = null;
		ArrayBuilder<int> arrayBuilder4 = null;
		for (int i = 0; i < length; i++)
		{
			BoundExpression boundExpression = arguments.Argument(i);
			ParameterSymbol parameterSymbol = parameters.Parameters[i];
			if (!CheckArgumentForMemberApplicability(boundExpression, parameterSymbol, receiverOpt))
			{
				arrayBuilder3 = arrayBuilder3 ?? ArrayBuilder<int>.GetInstance();
				arrayBuilder3.Add(i);
			}
			if (parameterSymbol.TypeFromFirstArgument)
			{
				TypeSymbol memberType = arguments.Argument(0).MemberType;
				if (!Conversions.ClassifyImplicitConversionFromExpression(boundExpression, memberType, ref useSiteDiagnostics).Exists)
				{
					arrayBuilder4 = arrayBuilder4 ?? ArrayBuilder<int>.GetInstance();
					arrayBuilder4.Add(i);
				}
			}
			Conversion item = CheckArgumentForConversionApplicability(boundExpression, parameterSymbol, ref useSiteDiagnostics);
			if (!item.Exists)
			{
				arrayBuilder2 = arrayBuilder2 ?? ArrayBuilder<int>.GetInstance();
				arrayBuilder2.Add(i);
			}
			if (arrayBuilder != null)
			{
				arrayBuilder.Add(item);
			}
			else if (!item.IsIdentity)
			{
				arrayBuilder = ArrayBuilder<Conversion>.GetInstance(length);
				arrayBuilder.AddMany(Conversion.Identity, i);
				arrayBuilder.Add(item);
			}
		}
		ImmutableArray<Conversion> conversions = arrayBuilder?.ToImmutableAndFree() ?? default(ImmutableArray<Conversion>);
		if (arrayBuilder3 != null)
		{
			return MemberAnalysisResult.BadMembers(parameters, arrayBuilder3.ToImmutableAndFree());
		}
		if (arrayBuilder4 != null)
		{
			return MemberAnalysisResult.BadTypeFromFirstArgumentConversions(parameters, arrayBuilder4.ToImmutableAndFree(), conversions);
		}
		if (arrayBuilder2 != null)
		{
			return MemberAnalysisResult.BadArgumentConversions(parameters, arrayBuilder2.ToImmutableAndFree(), conversions);
		}
		return MemberAnalysisResult.NormalForm(parameters, conversions);
	}

	private void TryRebindBadMemberArgumentInReceiverScope(EffectiveParameters parameters, AnalyzedArguments arguments, BoundExpression receiverOpt)
	{
		for (int i = 0; i < parameters.Parameters.Length; i++)
		{
			if (parameters.Parameters[i].IsSameVar && receiverOpt != null && arguments.Arguments[i].Syntax is IdentifierNameSyntax node)
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				BoundExpression boundExpression = binder.BindMemberArgument(node, receiverOpt, instance);
				if (!boundExpression.HasAnyErrors)
				{
					arguments.Arguments[i] = boundExpression;
					arguments.ArgumentDiagnostics[i].Free();
					arguments.ArgumentDiagnostics[i] = instance;
				}
				else
				{
					instance.Free();
				}
			}
		}
	}

	private static bool IsSelf(BoundExpression receiver)
	{
		if (receiver == null)
		{
			return true;
		}
		SynthesizedGlobalVariableSymbol synthesizedGlobalVariableSymbol = receiver.ExpressionSymbol as SynthesizedGlobalVariableSymbol;
		if (synthesizedGlobalVariableSymbol != null)
		{
			return synthesizedGlobalVariableSymbol.IsThis;
		}
		return false;
	}

	private static bool CheckArgumentForMemberApplicability(BoundExpression argument, ParameterSymbol parameter, BoundExpression receiverOpt)
	{
		if (!parameter.IsMember && !parameter.IsMemberReference)
		{
			return true;
		}
		if (!parameter.IsSameVar)
		{
			return true;
		}
		BoundExpression receiverOpt2;
		switch (argument.Kind)
		{
		case BoundKind.FieldAccess:
			receiverOpt2 = ((BoundFieldAccess)argument).ReceiverOpt;
			break;
		case BoundKind.QueryElementAccess:
			receiverOpt2 = ((BoundQueryElementAccess)argument).ReceiverOpt;
			break;
		case BoundKind.TestFieldAccess:
			receiverOpt2 = ((BoundTestFieldAccess)argument).ReceiverOpt;
			break;
		case BoundKind.TestFilterFieldAccess:
			receiverOpt2 = ((BoundTestFilterFieldAccess)argument).ReceiverOpt;
			break;
		default:
			return false;
		}
		if (IsSelf(receiverOpt) && IsSelf(receiverOpt2))
		{
			return true;
		}
		if (receiverOpt != null && receiverOpt2 != null && receiverOpt2.ExpressionSymbol == receiverOpt.ExpressionSymbol)
		{
			return true;
		}
		return false;
	}

	private void AddMemberToCandidateSet<TMember>(TMember member, ArrayBuilder<MemberResolutionResult<TMember>> results, AnalyzedArguments arguments, BoundExpression receiverOpt, bool completeResults, ref HashSet<DiagnosticInfo> useSiteDiagnostics) where TMember : Symbol
	{
		MemberResolutionResult<TMember> item = IsApplicable(member, arguments, GetEffectiveParametersInNormalForm(member, arguments.Arguments.Count), receiverOpt, ref useSiteDiagnostics);
		if ((item.Result.IsValid | completeResults) || item.HasUseSiteDiagnosticToReport)
		{
			results.Add(item);
		}
	}

	private Conversion CheckArgumentForConversionApplicability(BoundExpression argument, ParameterSymbol parameter, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		TypeSymbol destination = parameter.Type;
		if (!parameter.IsVar)
		{
			Conversion result = Conversions.ClassifyImplicitConversionFromExpression(argument, destination, ref useSiteDiagnostics);
			DebugAssertHelper.Assert(!result.Exists || result.IsImplicit, "ClassifyImplicitConversion should only return implicit conversions");
			return result;
		}
		TypeSymbol source = argument.Type;
		if ((object)source != null)
		{
			Conversions.ApplyCompatibilityEnumTypeSubstitution(ref source, ref destination);
			return GetVarArgumentCompatibility(parameter, destination, source, ref useSiteDiagnostics);
		}
		return Conversion.NoConversion;
	}

	private Conversion GetVarArgumentCompatibility(ParameterSymbol parameter, TypeSymbol parameterType, TypeSymbol argType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (IsAlTypeToSourceDotNetVarParameter(parameter, parameterType, argType))
		{
			return Conversion.NoConversion;
		}
		return GetVarArgumentCompatibility(argType, parameterType, ref useSiteDiagnostics);
	}

	private static bool IsAlTypeToSourceDotNetVarParameter(ParameterSymbol parameter, TypeSymbol parameterType, TypeSymbol argType)
	{
		if (ConversionDotNet.IsStronglyTypedDotNetSymbol(argType) || !ConversionDotNet.IsStronglyTypedDotNetSymbol(parameterType))
		{
			return false;
		}
		if (parameter.IsSourceSymbol())
		{
			return true;
		}
		return false;
	}

	private Conversion GetVarArgumentCompatibility(TypeSymbol argType, TypeSymbol parameterType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (Conversions.HasIdentityConversion(argType, parameterType))
		{
			return Conversion.Identity;
		}
		if (Conversions.IsToJokerConversion(argType, parameterType))
		{
			return Conversion.ToJoker;
		}
		if (Conversions.HasImplicitRecordConversion(argType, parameterType))
		{
			return Conversion.ImplicitRecord;
		}
		if (Conversions.IsFromVariantConversion(argType, parameterType))
		{
			return Conversion.FromVariant;
		}
		if (Conversions.IsDotNetConversion(argType, parameterType))
		{
			return Conversions.ClassifyDotNetByRefConversion(argType, parameterType);
		}
		if (parameterType.NavTypeKind == argType.NavTypeKind)
		{
			switch (parameterType.NavTypeKind)
			{
			case NavTypeKind.Text:
			case NavTypeKind.Code:
				return Conversion.ImplicitText;
			case NavTypeKind.Option:
				return Conversion.ImplicitOption;
			case NavTypeKind.Array:
				return GetVarArrayConversion((ArrayTypeSymbol)parameterType, (ArrayTypeSymbol)argType, ref useSiteDiagnostics);
			}
		}
		return Conversion.NoConversion;
	}

	private Conversion GetVarArrayConversion(ArrayTypeSymbol paramArrayType, ArrayTypeSymbol argArrayType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((argArrayType.Dimensions.Length == paramArrayType.Dimensions.Length || paramArrayType.Dimensions.Length == 0) && Conversions.ClassifyConversionFromType(argArrayType.ElementType, paramArrayType.ElementType, ref useSiteDiagnostics).Kind.IsValidForArrayConversion())
		{
			return Conversion.ImplicitArray;
		}
		return Conversion.NoConversion;
	}

	private static EffectiveParameters GetEffectiveParametersInNormalForm<TMember>(TMember member, int argumentCount) where TMember : Symbol
	{
		int num = member.GetParameterCount();
		ImmutableArray<ParameterSymbol> parameters = member.GetParameters();
		ImmutableArray<bool> refKinds = member.GetParameterIsVars();
		if (num < argumentCount && num > 0 && parameters[parameters.Length - 1].IsParams)
		{
			ArrayBuilder<TypeSymbol> instance = ArrayBuilder<TypeSymbol>.GetInstance();
			instance.AddRange(member.GetParameterTypes());
			instance.AddMany(parameters[parameters.Length - 1].Type, argumentCount - num);
			ArrayBuilder<ParameterSymbol> instance2 = ArrayBuilder<ParameterSymbol>.GetInstance();
			instance2.AddRange(parameters);
			instance2.AddMany(parameters[parameters.Length - 1], argumentCount - num);
			if (!refKinds.IsDefaultOrEmpty)
			{
				ArrayBuilder<bool> instance3 = ArrayBuilder<bool>.GetInstance();
				instance3.AddRange(member.GetParameterIsVars());
				instance3.AddMany(item: false, argumentCount - num);
				refKinds = instance3.ToImmutableAndFree();
			}
			return new EffectiveParameters(instance.ToImmutableAndFree(), refKinds, instance2.ToImmutableAndFree());
		}
		if (num > argumentCount)
		{
			while (num > argumentCount && parameters[num - 1].IsOptional)
			{
				num--;
			}
			if (member.GetParameterCount() != num)
			{
				ArrayBuilder<TypeSymbol> instance4 = ArrayBuilder<TypeSymbol>.GetInstance();
				instance4.AddRange(member.GetParameterTypes().Take(num));
				ArrayBuilder<ParameterSymbol> instance5 = ArrayBuilder<ParameterSymbol>.GetInstance();
				instance5.AddRange(parameters.Take(num));
				if (!refKinds.IsDefaultOrEmpty)
				{
					ArrayBuilder<bool> instance6 = ArrayBuilder<bool>.GetInstance();
					instance6.AddRange(member.GetParameterIsVars().Take(num));
					refKinds = instance6.ToImmutableAndFree();
				}
				return new EffectiveParameters(instance4.ToImmutableAndFree(), refKinds, instance5.ToImmutableAndFree());
			}
		}
		return new EffectiveParameters(member.GetParameterTypes(), refKinds, parameters);
	}

	private static ArgumentAnalysisResult AnalyzeArguments(Symbol symbol, AnalyzedArguments arguments, EffectiveParameters effectiveParameters)
	{
		ImmutableArray<ParameterSymbol> parameters = symbol.GetParameters();
		DebugAssertHelper.Assert(!parameters.IsDefault);
		DebugAssertHelper.Assert(arguments != null);
		DebugAssertHelper.Assert(arguments.Names.Count == 0);
		int length = parameters.Length;
		int count = arguments.Arguments.Count;
		if (count < length)
		{
			for (int i = count; i < length; i++)
			{
				if (parameters.Length == i || !parameters[i].IsOptional)
				{
					return ArgumentAnalysisResult.RequiredParameterMissing(i, effectiveParameters);
				}
			}
		}
		else if (length < count && !symbol.IsParams())
		{
			return ArgumentAnalysisResult.NoCorrespondingParameter(length, effectiveParameters);
		}
		return ArgumentAnalysisResult.NormalForm(effectiveParameters);
	}

	private static BetterResult MoreSpecificType(ArrayBuilder<TypeSymbol> t1, ArrayBuilder<TypeSymbol> t2, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert(t1.Count == t2.Count);
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
		if (t1.IsArray())
		{
			ArrayTypeSymbol arrayTypeSymbol = (ArrayTypeSymbol)t1;
			ArrayTypeSymbol arrayTypeSymbol2 = (ArrayTypeSymbol)t2;
			DebugAssertHelper.Assert(arrayTypeSymbol.HasSameShapeAs(arrayTypeSymbol2));
			return MoreSpecificType(arrayTypeSymbol.ElementType, arrayTypeSymbol2.ElementType, ref useSiteDiagnostics);
		}
		DebugAssertHelper.Assert((object)t1 == null == ((object)t2 == null));
		if ((object)t1 == null)
		{
			return BetterResult.Neither;
		}
		DebugAssertHelper.Assert(t1.OriginalDefinition == t2.OriginalDefinition);
		return BetterResult.Equal;
	}

	private static BetterResult BetterConversionFromExpression(BoundExpression node, TypeSymbol t1, TypeSymbol t2, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (Conversions.HasIdentityConversion(t1, t2))
		{
			return BetterResult.Neither;
		}
		bool flag = ExpressionMatchExactly(node, t1);
		bool flag2 = ExpressionMatchExactly(node, t2);
		if (flag)
		{
			if (flag2)
			{
				return BetterResult.Neither;
			}
			return BetterResult.Left;
		}
		if (flag2)
		{
			return BetterResult.Right;
		}
		return BetterConversionTarget(t1, t2, ref useSiteDiagnostics);
	}

	private static BetterResult BetterConversionTarget(TypeSymbol type1, TypeSymbol type2, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (Conversions.HasIdentityConversion(type1, type2))
		{
			return BetterResult.Neither;
		}
		bool isImplicit = Conversions.ClassifyImplicitConversion(type1, type2, ref useSiteDiagnostics).IsImplicit;
		bool isImplicit2 = Conversions.ClassifyImplicitConversion(type2, type1, ref useSiteDiagnostics).IsImplicit;
		if (isImplicit)
		{
			if (isImplicit2)
			{
				return BetterResult.Neither;
			}
			return BetterResult.Left;
		}
		if (isImplicit2)
		{
			return BetterResult.Right;
		}
		return BetterResult.Neither;
	}

	private static bool ExpressionMatchExactly(BoundExpression node, TypeSymbol t)
	{
		if ((object)node.Type != null && Conversions.HasIdentityConversion(node.Type, t))
		{
			return true;
		}
		return false;
	}
}
