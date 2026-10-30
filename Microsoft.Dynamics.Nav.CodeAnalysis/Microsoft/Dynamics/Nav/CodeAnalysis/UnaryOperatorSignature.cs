using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal struct UnaryOperatorSignature(UnaryOperatorKind kind, TypeSymbol operandType, TypeSymbol returnType, MethodSymbol method = null)
{
	public static UnaryOperatorSignature Error;

	public readonly MethodSymbol Method = method;

	public readonly TypeSymbol OperandType = operandType;

	public readonly TypeSymbol ReturnType = returnType;

	public readonly UnaryOperatorKind Kind = kind;

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "kind: {0} operand: {1} return: {2}", Kind, OperandType, ReturnType);
	}
}
