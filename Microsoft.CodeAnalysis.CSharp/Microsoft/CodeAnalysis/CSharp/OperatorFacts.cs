using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class OperatorFacts
{
	public static bool DefinitelyHasNoUserDefinedOperators(TypeSymbol type)
	{
		TypeKind typeKind = type.TypeKind;
		if (typeKind != TypeKind.Class && typeKind - 10 > TypeKind.Array)
		{
			return true;
		}
		SpecialType specialType = type.SpecialType;
		if ((uint)(specialType - 1) <= 19u || specialType == SpecialType.System_Array)
		{
			return true;
		}
		return false;
	}

	public static string BinaryOperatorNameFromSyntaxKind(SyntaxKind kind)
	{
		return BinaryOperatorNameFromSyntaxKindIfAny(kind) ?? "op_Addition";
	}

	internal static string BinaryOperatorNameFromSyntaxKindIfAny(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.PlusToken => "op_Addition", 
			SyntaxKind.MinusToken => "op_Subtraction", 
			SyntaxKind.AsteriskToken => "op_Multiply", 
			SyntaxKind.SlashToken => "op_Division", 
			SyntaxKind.PercentToken => "op_Modulus", 
			SyntaxKind.CaretToken => "op_ExclusiveOr", 
			SyntaxKind.AmpersandToken => "op_BitwiseAnd", 
			SyntaxKind.BarToken => "op_BitwiseOr", 
			SyntaxKind.EqualsEqualsToken => "op_Equality", 
			SyntaxKind.LessThanToken => "op_LessThan", 
			SyntaxKind.LessThanEqualsToken => "op_LessThanOrEqual", 
			SyntaxKind.LessThanLessThanToken => "op_LeftShift", 
			SyntaxKind.GreaterThanToken => "op_GreaterThan", 
			SyntaxKind.GreaterThanEqualsToken => "op_GreaterThanOrEqual", 
			SyntaxKind.GreaterThanGreaterThanToken => "op_RightShift", 
			SyntaxKind.ExclamationEqualsToken => "op_Inequality", 
			_ => null, 
		};
	}

	public static string UnaryOperatorNameFromSyntaxKind(SyntaxKind kind)
	{
		return UnaryOperatorNameFromSyntaxKindIfAny(kind) ?? "op_UnaryPlus";
	}

	internal static string UnaryOperatorNameFromSyntaxKindIfAny(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.PlusToken => "op_UnaryPlus", 
			SyntaxKind.MinusToken => "op_UnaryNegation", 
			SyntaxKind.TildeToken => "op_OnesComplement", 
			SyntaxKind.ExclamationToken => "op_LogicalNot", 
			SyntaxKind.PlusPlusToken => "op_Increment", 
			SyntaxKind.MinusMinusToken => "op_Decrement", 
			SyntaxKind.TrueKeyword => "op_True", 
			SyntaxKind.FalseKeyword => "op_False", 
			_ => null, 
		};
	}

	public static string OperatorNameFromDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.OperatorDeclarationSyntax declaration)
	{
		return OperatorNameFromDeclaration((Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OperatorDeclarationSyntax)declaration.Green);
	}

	public static string OperatorNameFromDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax.OperatorDeclarationSyntax declaration)
	{
		SyntaxKind kind = declaration.OperatorToken.Kind;
		if (SyntaxFacts.IsBinaryExpressionOperatorToken(kind))
		{
			if (SyntaxFacts.IsPrefixUnaryExpressionOperatorToken(kind) && declaration.ParameterList.Parameters.Count == 1)
			{
				return UnaryOperatorNameFromSyntaxKind(kind);
			}
			return BinaryOperatorNameFromSyntaxKind(kind);
		}
		if (SyntaxFacts.IsUnaryOperatorDeclarationToken(kind))
		{
			return UnaryOperatorNameFromSyntaxKind(kind);
		}
		return "op_UnaryPlus";
	}

	public static string UnaryOperatorNameFromOperatorKind(UnaryOperatorKind kind)
	{
		switch (kind & UnaryOperatorKind.OpMask)
		{
		case UnaryOperatorKind.UnaryPlus:
			return "op_UnaryPlus";
		case UnaryOperatorKind.UnaryMinus:
			return "op_UnaryNegation";
		case UnaryOperatorKind.BitwiseComplement:
			return "op_OnesComplement";
		case UnaryOperatorKind.LogicalNegation:
			return "op_LogicalNot";
		case UnaryOperatorKind.PostfixIncrement:
		case UnaryOperatorKind.PrefixIncrement:
			return "op_Increment";
		case UnaryOperatorKind.PostfixDecrement:
		case UnaryOperatorKind.PrefixDecrement:
			return "op_Decrement";
		case UnaryOperatorKind.True:
			return "op_True";
		case UnaryOperatorKind.False:
			return "op_False";
		default:
			throw ExceptionUtilities.UnexpectedValue(kind & UnaryOperatorKind.OpMask);
		}
	}

	public static string BinaryOperatorNameFromOperatorKind(BinaryOperatorKind kind)
	{
		return (kind & BinaryOperatorKind.OpMask) switch
		{
			BinaryOperatorKind.Addition => "op_Addition", 
			BinaryOperatorKind.And => "op_BitwiseAnd", 
			BinaryOperatorKind.Division => "op_Division", 
			BinaryOperatorKind.Equal => "op_Equality", 
			BinaryOperatorKind.GreaterThan => "op_GreaterThan", 
			BinaryOperatorKind.GreaterThanOrEqual => "op_GreaterThanOrEqual", 
			BinaryOperatorKind.LeftShift => "op_LeftShift", 
			BinaryOperatorKind.LessThan => "op_LessThan", 
			BinaryOperatorKind.LessThanOrEqual => "op_LessThanOrEqual", 
			BinaryOperatorKind.Multiplication => "op_Multiply", 
			BinaryOperatorKind.Or => "op_BitwiseOr", 
			BinaryOperatorKind.NotEqual => "op_Inequality", 
			BinaryOperatorKind.Remainder => "op_Modulus", 
			BinaryOperatorKind.RightShift => "op_RightShift", 
			BinaryOperatorKind.Subtraction => "op_Subtraction", 
			BinaryOperatorKind.Xor => "op_ExclusiveOr", 
			_ => throw ExceptionUtilities.UnexpectedValue(kind & BinaryOperatorKind.OpMask), 
		};
	}
}
