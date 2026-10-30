using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal struct UnaryOperatorSignature(UnaryOperatorKind kind, TypeSymbol operandType, TypeSymbol returnType, MethodSymbol method = null)
{
	public static UnaryOperatorSignature Error;

	public readonly MethodSymbol Method = method;

	public readonly TypeSymbol OperandType = operandType;

	public readonly TypeSymbol ReturnType = returnType;

	public readonly UnaryOperatorKind Kind = kind;

	public RefKind RefKind
	{
		get
		{
			if ((object)Method != null && !Method.ParameterRefKinds.IsDefaultOrEmpty)
			{
				return Method.ParameterRefKinds.Single();
			}
			return RefKind.None;
		}
	}

	public override string ToString()
	{
		return $"kind: {Kind} operandType: {OperandType} operandRefKind: {RefKind} return: {ReturnType}";
	}
}
