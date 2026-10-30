using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal struct BinaryOperatorSignature(BinaryOperatorKind kind, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol returnType, MethodSymbol method = null) : IEquatable<BinaryOperatorSignature>
{
	public static BinaryOperatorSignature Error;

	public readonly TypeSymbol LeftType = leftType;

	public readonly TypeSymbol RightType = rightType;

	public readonly TypeSymbol ReturnType = returnType;

	public readonly MethodSymbol Method = method;

	public readonly BinaryOperatorKind Kind = kind;

	public int? Priority = null;

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "kind: {0} left: {1} right: {2} return: {3}", Kind, LeftType, RightType, ReturnType);
	}

	public bool Equals(BinaryOperatorSignature other)
	{
		if (Kind == other.Kind && LeftType == other.LeftType && RightType == other.RightType && ReturnType == other.ReturnType)
		{
			return Method == other.Method;
		}
		return false;
	}

	public static bool operator ==(BinaryOperatorSignature x, BinaryOperatorSignature y)
	{
		return x.Equals(y);
	}

	public static bool operator !=(BinaryOperatorSignature x, BinaryOperatorSignature y)
	{
		return !x.Equals(y);
	}

	public override bool Equals(object obj)
	{
		if (obj is BinaryOperatorSignature)
		{
			return Equals((BinaryOperatorSignature)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(ReturnType.GetHashCode(), Hash.Combine(LeftType.GetHashCode(), Hash.Combine(RightType.GetHashCode(), Hash.Combine(Method.GetHashCode(), (int)Kind))));
	}
}
