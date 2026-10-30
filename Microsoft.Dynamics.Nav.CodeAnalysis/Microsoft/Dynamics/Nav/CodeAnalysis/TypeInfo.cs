using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public struct TypeInfo : IEquatable<TypeInfo>
{
	internal static readonly TypeInfo None = new TypeInfo(null, null);

	[SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
	public ITypeSymbol Type { get; }

	public ITypeSymbol ConvertedType { get; }

	internal TypeInfo(ITypeSymbol type, ITypeSymbol convertedType)
	{
		this = default;
		Type = type;
		ConvertedType = convertedType;
	}

	public bool Equals(TypeInfo other)
	{
		if (object.Equals(Type, other.Type))
		{
			return object.Equals(ConvertedType, other.ConvertedType);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is TypeInfo)
		{
			return Equals((TypeInfo)obj);
		}
		return false;
	}

	public static bool operator ==(TypeInfo left, TypeInfo right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(TypeInfo left, TypeInfo right)
	{
		return !(left == right);
	}

	public override int GetHashCode()
	{
		return Hash.Combine(ConvertedType.GetHashCode(), Hash.Combine(Type.GetHashCode(), 0));
	}
}
