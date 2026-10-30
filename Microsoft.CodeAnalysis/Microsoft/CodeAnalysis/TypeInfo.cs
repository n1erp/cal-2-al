using System;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

public struct TypeInfo : IEquatable<TypeInfo>
{
	internal static readonly TypeInfo None = new TypeInfo(null, null);

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

	public override int GetHashCode()
	{
		return Hash.Combine(ConvertedType, Hash.Combine(Type, 0));
	}
}
