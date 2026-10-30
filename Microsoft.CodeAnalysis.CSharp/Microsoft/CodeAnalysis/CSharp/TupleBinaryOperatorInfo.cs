using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class TupleBinaryOperatorInfo
{
	internal class Single : TupleBinaryOperatorInfo
	{
		internal readonly BinaryOperatorKind Kind;

		internal readonly Conversion LeftConversion;

		internal readonly Conversion RightConversion;

		internal readonly MethodSymbol MethodSymbolOpt;

		internal readonly Conversion ConversionForBool;

		internal readonly UnaryOperatorSignature BoolOperator;

		internal override TupleBinaryOperatorInfoKind InfoKind => TupleBinaryOperatorInfoKind.Single;

		internal Single(TypeSymbol leftConvertedTypeOpt, TypeSymbol rightConvertedTypeOpt, BinaryOperatorKind kind, Conversion leftConversion, Conversion rightConversion, MethodSymbol methodSymbolOpt, Conversion conversionForBool, UnaryOperatorSignature boolOperator)
			: base(leftConvertedTypeOpt, rightConvertedTypeOpt)
		{
			Kind = kind;
			LeftConversion = leftConversion;
			RightConversion = rightConversion;
			MethodSymbolOpt = methodSymbolOpt;
			ConversionForBool = conversionForBool;
			BoolOperator = boolOperator;
		}

		public override string ToString()
		{
			return $"binaryOperatorKind: {Kind}";
		}
	}

	internal class Multiple : TupleBinaryOperatorInfo
	{
		internal readonly ImmutableArray<TupleBinaryOperatorInfo> Operators;

		internal static readonly Multiple ErrorInstance = new Multiple(ImmutableArray<TupleBinaryOperatorInfo>.Empty, null, null);

		internal override TupleBinaryOperatorInfoKind InfoKind => TupleBinaryOperatorInfoKind.Multiple;

		internal Multiple(ImmutableArray<TupleBinaryOperatorInfo> operators, TypeSymbol leftConvertedTypeOpt, TypeSymbol rightConvertedTypeOpt)
			: base(leftConvertedTypeOpt, rightConvertedTypeOpt)
		{
			Operators = operators;
		}
	}

	internal class NullNull : TupleBinaryOperatorInfo
	{
		internal readonly BinaryOperatorKind Kind;

		internal override TupleBinaryOperatorInfoKind InfoKind => TupleBinaryOperatorInfoKind.NullNull;

		internal NullNull(BinaryOperatorKind kind)
			: base(null, null)
		{
			Kind = kind;
		}
	}

	internal readonly TypeSymbol LeftConvertedTypeOpt;

	internal readonly TypeSymbol RightConvertedTypeOpt;

	internal abstract TupleBinaryOperatorInfoKind InfoKind { get; }

	private TupleBinaryOperatorInfo(TypeSymbol leftConvertedTypeOpt, TypeSymbol rightConvertedTypeOpt)
	{
		LeftConvertedTypeOpt = leftConvertedTypeOpt;
		RightConvertedTypeOpt = rightConvertedTypeOpt;
	}
}
