using System;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal struct CSharpTypeInfo : IEquatable<CSharpTypeInfo>
{
	internal static readonly CSharpTypeInfo None = new CSharpTypeInfo(null, null, Conversion.Identity);

	public readonly TypeSymbol Type;

	public readonly TypeSymbol ConvertedType;

	public readonly Conversion ImplicitConversion;

	internal CSharpTypeInfo(TypeSymbol type, TypeSymbol convertedType, Conversion implicitConversion)
	{
		Type = type.GetNonErrorGuess() ?? type;
		ConvertedType = convertedType.GetNonErrorGuess() ?? convertedType;
		ImplicitConversion = implicitConversion;
	}

	public static implicit operator TypeInfo(CSharpTypeInfo info)
	{
		return new TypeInfo(info.Type, info.ConvertedType);
	}

	public override bool Equals(object obj)
	{
		if (obj is CSharpTypeInfo)
		{
			return Equals((CSharpTypeInfo)obj);
		}
		return false;
	}

	public bool Equals(CSharpTypeInfo other)
	{
		if (ImplicitConversion.Equals(other.ImplicitConversion) && Type == other.Type)
		{
			return ConvertedType == other.ConvertedType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(ConvertedType, Hash.Combine(Type, ImplicitConversion.GetHashCode()));
	}
}
