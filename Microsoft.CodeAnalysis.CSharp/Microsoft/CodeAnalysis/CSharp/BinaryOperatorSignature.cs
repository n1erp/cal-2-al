using System;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal struct BinaryOperatorSignature(BinaryOperatorKind kind, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol returnType, MethodSymbol method = null) : IEquatable<BinaryOperatorSignature>
{
	public static BinaryOperatorSignature Error;

	public readonly TypeSymbol LeftType = leftType;

	public readonly TypeSymbol RightType = rightType;

	public readonly TypeSymbol ReturnType = returnType;

	public readonly MethodSymbol Method = method;

	public readonly BinaryOperatorKind Kind = kind;

	public int? Priority = null;

	public RefKind LeftRefKind
	{
		get
		{
			if ((object)Method != null && !Method.ParameterRefKinds.IsDefaultOrEmpty)
			{
				return Method.ParameterRefKinds[0];
			}
			return RefKind.None;
		}
	}

	public RefKind RightRefKind
	{
		get
		{
			if ((object)Method != null && !Method.ParameterRefKinds.IsDefaultOrEmpty)
			{
				return Method.ParameterRefKinds[1];
			}
			return RefKind.None;
		}
	}

	public override string ToString()
	{
		return $"kind: {Kind} leftType: {LeftType} leftRefKind: {LeftRefKind} rightType: {RightType} rightRefKind: {RightRefKind} return: {ReturnType}";
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
		return Hash.Combine(ReturnType, Hash.Combine(LeftType, Hash.Combine(RightType, Hash.Combine(Method, (int)Kind))));
	}
}
