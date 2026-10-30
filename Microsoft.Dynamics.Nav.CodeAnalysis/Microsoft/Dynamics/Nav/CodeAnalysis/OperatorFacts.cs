using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class OperatorFacts
{
	public static string BinaryOperatorNameFromOperatorKind(BinaryOperatorKind kind)
	{
		return (kind & BinaryOperatorKind.OpMask) switch
		{
			BinaryOperatorKind.Addition => "op_Addition", 
			BinaryOperatorKind.And => "op_LogicalAnd", 
			BinaryOperatorKind.Division => "op_Division", 
			BinaryOperatorKind.IDivision => "op_IntegerDivision", 
			BinaryOperatorKind.Equal => "op_Equal", 
			BinaryOperatorKind.GreaterThan => "GreaterThan", 
			BinaryOperatorKind.GreaterThanOrEqual => "GreaterThanOrEqual", 
			BinaryOperatorKind.LessThan => "op_LessThan", 
			BinaryOperatorKind.LessThanOrEqual => "op_LessThanOrEqual", 
			BinaryOperatorKind.Multiplication => "op_Multiplication", 
			BinaryOperatorKind.Or => "op_LogicalOr", 
			BinaryOperatorKind.NotEqual => "op_NotEqual", 
			BinaryOperatorKind.Remainder => "op_Modulus", 
			BinaryOperatorKind.Subtraction => "op_Subtraction", 
			BinaryOperatorKind.Xor => "op_LogicalXor", 
			BinaryOperatorKind.Range => "op_Range", 
			_ => throw ExceptionUtilities.UnexpectedValue(kind & BinaryOperatorKind.OpMask), 
		};
	}
}
